using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe Enemy hérite de Unit et représente les ennemis.
/// Les ennemis ont :
/// - Un deck séquentiel (pas de mélange) qui boucle : une carte par tour, sans coût (pas de PA, 02/10/2026)
/// - Un comportement prévisible pour le joueur
/// </summary>
public class Enemy : Unit, IOutgoingDamageModifier
{
    // ========== ENEMY DATA ==========

    [Header("=== Enemy Specific ==")]
    [SerializeField] private EnemyData _enemyData;

    // ========== DECK PATTERN ==========
    // Deck system pour pattern de combat
    private List<CardData> _combatDeck = new List<CardData>();
    private int _currentCardIndex = 0; // Index de la prochaine carte à jouer

    // ========== EVENTS ==========
    public event System.Action<CardData> OnNextCardChanged;   // Prochaine carte visible

    // ========== GETTERS PUBLICS ==========

    public EnemyData GetEnemyData() => _enemyData;
    public bool IsBoss() => _enemyData != null && _enemyData.isBoss;
    public override string DisplayName => _enemyData != null ? _enemyData.enemyName : name;

    // Surcharge pour définir la faction automatiquement
    public override UnitFaction GetFaction() => UnitFaction.Enemy;

    /// <summary>
    /// Retourne la prochaine carte qui sera jouée (visible par le joueur)
    /// </summary>
    public CardData GetNextCard()
    {
        if (_combatDeck == null || _combatDeck.Count == 0) return null;

        // Boucle automatique : si on a atteint la fin du deck, recommence au début
        if (_currentCardIndex >= _combatDeck.Count)
        {
            _currentCardIndex = 0;
            GameLog.Log($"{name} (Enemy): Fin du cycle de cartes, retour au début du pattern");
        }

        return _combatDeck[_currentCardIndex];
    }

    /// <summary>
    /// True si la prochaine carte est annulée (ex: Sidération) : à son prochain tour, le monstre
    /// ne la joue pas (ni attaque de base) et passe à la suivante de son pattern
    /// </summary>
    public bool IsNextCardCancelled { get; private set; }

    public void CancelNextCard()
    {
        ClearPendingThrow(); // Sidération annule aussi un lancer déjà annoncé
        if (GetNextCard() == null) return;
        IsNextCardCancelled = true;
        EventBus.Publish(new UnitEffectAppliedEvent(this, UnitEffect.CardCancelled, 0));
        OnNextCardChanged?.Invoke(GetNextCard()); // l'aperçu affiche la carte comme annulée
    }

    /// <summary>
    /// Au tour du monstre : si sa carte était annulée, la saute et retourne true
    /// </summary>
    public bool ConsumeCancelledCard()
    {
        if (!IsNextCardCancelled) return false;
        IsNextCardCancelled = false;
        GetNextCard(); // boucle du pattern si besoin
        _currentCardIndex++;
        GameLog.Log($"{name} (Enemy) : carte annulée, passe à la suivante");
        OnNextCardChanged?.Invoke(GetNextCard());
        return true;
    }

    /// <summary>
    /// True si la prochaine carte est entravée (ex: Aura de terreur) : à son prochain tour, le monstre
    /// joue son attaque de base à la place, sans avancer son pattern (la carte revient au tour suivant)
    /// </summary>
    public bool IsNextCardHindered { get; private set; }

    public void HinderNextCard()
    {
        if (GetNextCard() == null) return;
        IsNextCardHindered = true;
        EventBus.Publish(new UnitEffectAppliedEvent(this, UnitEffect.CardHindered, 0));
        OnNextCardChanged?.Invoke(GetNextCard()); // l'aperçu affiche la carte comme entravée
    }

    /// <summary>
    /// Au tour du monstre : retourne true si sa carte était entravée, et lève l'entrave
    /// </summary>
    public bool ConsumeHinderedCard()
    {
        if (!IsNextCardHindered) return false;
        IsNextCardHindered = false;
        GameLog.Log($"{name} (Enemy) : carte entravée, attaque de base à la place");
        OnNextCardChanged?.Invoke(GetNextCard());
        return true;
    }

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise l'ennemi avec EnemyData (au lieu de ChampionData)
    /// </summary>
    public void InitializeEnemy(EnemyData data, Vector2Int initialGridPos)
    {
        // Protection contre la double initialisation
        if (_isInitialized)
        {
            GameLog.LogWarning($"{gameObject.name} (Enemy) est déjà initialisé. Initialisation ignorée.");
            return;
        }

        // Validation centralisée des données
        ValidationResult validation = GameActionValidator.ValidateEnemyData(data);
        if (!validation.IsValid)
        {
            Debug.LogError($"❌ Échec initialisation Enemy : {validation.ErrorMessage}");
            enabled = false;
            return;
        }

        _enemyData = data;

        // Initialise les stats de base via la classe Unit (HP, PM, ATK)
        InitUnitStats(data.maxHealth, data.movementRange, data.attackDamage, data.armor, data.magicResistance);

        // Copie le deck de combat (pattern)
        _combatDeck.Clear();
        if (data.combatDeck != null)
        {
            _combatDeck.AddRange(data.combatDeck);
        }
        _currentCardIndex = 0;
        
        // Initialise les aspects communs (position, faction, etc.) via la classe Unit
        // et définit le nom du GameObject.
        gameObject.name = data.enemyName;
        Initialize(initialGridPos);

        // Notifie la prochaine carte
        OnNextCardChanged?.Invoke(GetNextCard());

        GameLog.Log($"{name} (Enemy) initialisé - HP: {_health}/{_maxHealth}, Deck: {_combatDeck.Count} cartes");
    }

    protected override void Start()
    {
        // Si l'ennemi n'a pas été initialisé (placé manuellement dans la scène)
        if (!_isInitialized)
        {
            if (_enemyData != null)
            {
                Vector2Int currentWorldGridPos = Services.Grid.GetGridPosFromWorldPos(transform.position);
                InitializeEnemy(_enemyData, currentWorldGridPos);
            }
            else
            {
                Debug.LogError($"L'ennemi {gameObject.name} n'a pas de EnemyData assigné et ne peut pas être initialisé.");
                enabled = false;
                return;
            }
        }

        // Crée la barre de vie (sauf si c'est un boss - sera géré différemment)
        if (!IsBoss())
        {
            CreateHealthBar(_enemyData.healthBarOffset, _enemyData.healthBarColor);
        }
    }

    // ========== NOMBRE DE JOUEURS (coop) ==========

    private float _damageMultiplier = 1f;
    private int _playerCount = 1; // pour les PV des phases suivantes du boss

    /// <summary>
    /// Adapte le monstre au nombre de joueurs (EnemyScaling) : PV max et dégâts de ses cartes.
    /// Appelé par GridManager au lancement du combat ; le barème d'EnemyData est celui d'un joueur.
    /// </summary>
    public void ScaleForPlayers(int playerCount)
    {
        if (_enemyData == null) return;
        _playerCount = playerCount;
        _damageMultiplier = EnemyScaling.DamageMultiplier(playerCount);
        SetMaxHealth(EnemyScaling.ScaledHealth(_enemyData.maxHealth, playerCount));
        GameLog.Log($"{name} adapté à {playerCount} joueur(s) : PV {_maxHealth}, dégâts x{_damageMultiplier:F2}");
    }

    // ========== PHASES DU BOSS (EnemyData.nextPhases) ==========

    private int _phase; // 0 = première phase (maxHealth et combatDeck d'EnemyData)

    /// <summary>Phase en cours (0 = la première).</summary>
    public int Phase => _phase;

    /// <summary>Nombre de phases : 1 + EnemyData.nextPhases.</summary>
    public int PhaseCount => 1 + (_enemyData != null ? _enemyData.nextPhases.Count : 0);

    /// <summary>
    /// À 0 PV, un boss qui a encore des phases ne meurt pas : il repart avec la barre pleine et le pattern de la
    /// phase suivante (surprise voulue, décision du 02/10/2026). Sinon, mort normale.
    /// </summary>
    protected override void Die()
    {
        if (_phase + 1 < PhaseCount) StartPhase(_phase + 1);
        else base.Die();
    }

    private void StartPhase(int phase)
    {
        EnemyData.BossPhase data = _enemyData.nextPhases[phase - 1];
        _phase = phase;
        SetMaxHealth(EnemyScaling.ScaledHealth(data.maxHealth, _playerCount));
        SetCurrentHealth(GetMaxHealth());

        _combatDeck.Clear();
        _combatDeck.AddRange(data.combatDeck);
        _currentCardIndex = 0;
        OnNextCardChanged?.Invoke(GetNextCard());

        GameLog.Log($"{name} passe en phase {phase + 1}/{PhaseCount} : {GetMaxHealth()} PV, {_combatDeck.Count} cartes");
        EventBus.Publish(new BossPhaseChangedEvent(this, phase, PhaseCount));
    }

    /// <summary>
    /// Perte de PV sans retour visuel propre (ex. coups portés au lit sous lequel le boss se cache, déjà affichés
    /// sur le lit). À 0 PV : même règle que la mort (phase suivante ou mort).
    /// </summary>
    public void LoseHealth(int amount)
    {
        if (amount <= 0) return;
        if (GetHealth() - amount <= 0) Die();
        else SetCurrentHealth(GetHealth() - amount);
    }

    // ========== IMPLÉMENTATION INTERFACE IOutgoingDamageModifier ==========
    // Multiplicateur permanent (nombre de joueurs) : rien à consommer.

    public float GetDamageMultiplier() => _damageMultiplier;

    public void ConsumeDamageModifier() { }

    // ========== LANCER ANNONCÉ (voir CardData.telegraphedZoneCount) ==========

    // Tirages du monstre ; graine commune à tous les PC en réseau (SetRandomSeed, posée par GridManager)
    private System.Random _rng = new System.Random();
    private CardData _pendingThrowCard;
    private readonly List<Vector2Int> _pendingThrowEpicenters = new List<Vector2Int>();

    public void SetRandomSeed(int seed) => _rng = new System.Random(seed);

    /// <summary>Générateur des tirages du monstre (zones, lit où se cacher), commun à tous les PC en réseau.</summary>
    public System.Random Rng => _rng;

    public bool HasPendingThrow => _pendingThrowCard != null;

    private int _throwCount; // lancers annoncés depuis le début du combat (jouet qui s'anime un lancer sur N)

    /// <summary>Jouet du lancer annoncé qui s'animera en monstre (null si aucun) et sa case.</summary>
    public EnemyData PendingToy { get; private set; }
    public Vector2Int PendingToyCell { get; private set; }

    /// <summary>
    /// Annonce les zones d'un lancer (ThrowZonePicker), qui tomberont au début du prochain tour du monstre.
    /// </summary>
    /// <param name="cells">Cases du plateau (même ordre sur tous les PC).</param>
    public void AnnounceThrow(CardData card, IList<Vector2Int> cells, IList<Vector2Int> championCells)
    {
        int radius = card.isAOE ? card.aoeRadius : 0;
        _pendingThrowCard = card;
        _pendingThrowEpicenters.Clear();
        _pendingThrowEpicenters.AddRange(ThrowZonePicker.Pick(_rng, cells, championCells, card.telegraphedZoneCount, radius));

        // Un lancer sur N (à partir du N-ième), un des jouets s'animera : zone non marquée, jamais sur un champion
        _throwCount++;
        PendingToy = null;
        if (card.animatedToy != null && card.animatedToyEveryNthThrow > 0 && _throwCount % card.animatedToyEveryNthThrow == 0)
        {
            int toy = ThrowZonePicker.PickToyIndex(_rng, _pendingThrowEpicenters, championCells);
            if (toy >= 0)
            {
                PendingToy = card.animatedToy;
                PendingToyCell = _pendingThrowEpicenters[toy];
            }
        }

        // Cases couvertes, pour l'affichage des zones au sol
        var covered = new List<Vector2Int>();
        foreach (Vector2Int cell in cells)
            foreach (Vector2Int epicenter in _pendingThrowEpicenters)
                if (card.isAOE ? card.IsInAOEShape(this, epicenter, cell) : cell == epicenter)
                {
                    covered.Add(cell);
                    break;
                }
        GameLog.Log($"{name} annonce {card.cardName} : {_pendingThrowEpicenters.Count} zone(s)");
        EventBus.Publish(new ThrowZonesChangedEvent(this, covered));
    }

    /// <summary>
    /// Retire le lancer annoncé pour le résoudre : la carte et les épicentres de ses zones (null s'il n'y en a pas).
    /// </summary>
    public CardData TakePendingThrow(List<Vector2Int> epicenters)
    {
        CardData card = _pendingThrowCard;
        epicenters.Clear();
        epicenters.AddRange(_pendingThrowEpicenters);
        ClearPendingThrow();
        return card;
    }

    private void ClearPendingThrow()
    {
        if (_pendingThrowCard == null) return;
        _pendingThrowCard = null;
        _pendingThrowEpicenters.Clear();
        PendingToy = null;
        EventBus.Publish(new ThrowZonesChangedEvent(this, new List<Vector2Int>()));
    }

    // ========== SYSTÈME DE DECK SÉQUENTIEL ==========

    /// <summary>
    /// Pioche et joue la prochaine carte du deck (séquentiel, pas de mélange)
    /// Retourne null si aucune carte disponible
    /// </summary>
    public CardData DrawAndPlayNextCard()
    {
        // Vérifie s'il reste des cartes
        if (_combatDeck == null || _combatDeck.Count == 0)
        {
            GameLog.LogWarning($"{name} (Enemy): Deck vide !");
            return null;
        }

        if (_currentCardIndex >= _combatDeck.Count)
        {
            // Fin du deck, recommence au début (boucle)
            GameLog.Log($"{name} (Enemy): Fin du deck, retour au début");
            _currentCardIndex = 0;
        }

        CardData nextCard = _combatDeck[_currentCardIndex];

        // Joue la carte
        GameLog.Log($"{name} (Enemy) joue la carte: {nextCard.cardName}");
        _currentCardIndex++;

        // Notifie la prochaine carte (pour la preview)
        OnNextCardChanged?.Invoke(GetNextCard());

        return nextCard;
    }

    /// <summary>
    /// Appelé quand l'ennemi est détruit (mort ou autre raison)
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Notifie le BattleUIManager pour nettoyer les UI
        if (Services.IsBattleUIServiceAvailable())
        {
            Services.BattleUI.OnEnemyDied(this);
        }
    }
}
