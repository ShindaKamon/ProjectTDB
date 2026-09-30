using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe abstraite Champion - Base pour tous les personnages jouables.
/// Les champions ont :
/// - Un système PA pour jouer leurs cartes personnelles (via ActionPointsComponent)
/// - Un deck personnel avec pioche/défausse/mélange (géré par DeckManager)
/// - Des stats spécifiques (défenses, etc.) définies dans les classes dérivées
/// </summary>
public abstract class Champion : Unit, IActionPointsUser
{
    // ========== CHAMPION DATA ==========
    [Header("Champion Data")]
    [SerializeField] public ChampionData championData;

    public override string DisplayName => championData != null ? championData.championName : name;

    [Header("Barre de vie")]
    [Tooltip("Position de la barre de vie au-dessus du champion")]
    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0f, 2.1f, 0f);
    [SerializeField] private Color _healthBarColor = new Color(0.3f, 0.8f, 0.4f);

    // ========== SYSTÈME PA (Points d'Action) ==========
    // Les champions utilisent leurs PA pour jouer des cartes de leur deck personnel
    // Utilise la composition avec ActionPointsComponent pour éviter la duplication de code
    // Note: maxActionPoints est initialisé depuis ChampionData, pas besoin de SerializeField

    // Component qui gère la logique PA
    private ActionPointsComponent _actionPointsComponent;

    // ========== ÉVÉNEMENTS ==========

    /// <summary>
    /// Événement pour notifier les changements de PA
    /// Redirige l'événement du component vers l'extérieur
    /// </summary>
    public event System.Action<int, int> OnActionPointsChanged
    {
        add { if (_actionPointsComponent != null) _actionPointsComponent.OnActionPointsChanged += value; }
        remove { if (_actionPointsComponent != null) _actionPointsComponent.OnActionPointsChanged -= value; }
    }

    // ========== IMPLÉMENTATION INTERFACE IActionPointsUser ==========

    public int GetCurrentPA() => _actionPointsComponent?.GetCurrentPA() ?? 0;
    public int GetMaxPA() => _actionPointsComponent?.GetMaxPA() ?? 0;

    public bool SpendPA(int amount)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Champion): ActionPointsComponent n'est pas initialisé !");
            return false;
        }
        return _actionPointsComponent.SpendPA(amount);
    }

    public void RefreshPA()
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Champion): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.RefreshPA();
    }

    public void SetMaxPA(int value)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Champion): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.SetMaxPA(value);
    }

    public void ReduceCurrentPA(int amount)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Champion): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.ReduceCurrentPA(amount);
    }

    public void AddPA(int amount, bool canExceedMax = false)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Champion): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.AddPA(amount, canExceedMax);
        if (amount > 0) EventBus.Publish(new UnitEffectAppliedEvent(this, UnitEffect.ActionPoints, amount));
    }

    // ========== FUSION (ÉVEIL) ==========

    /// <summary>Points de jauge gagnés en jouant une carte d'une émotion.</summary>
    public const int GaugePointsPerCard = 1;

    /// <summary>Jauges d'émotion et fusion en cours (voir EmotionGauge).</summary>
    public EmotionGauge Gauge { get; } = new EmotionGauge();

    /// <summary>Compteur par tour à l'usage de la forme active, remis à zéro au début de chaque tour du champion.</summary>
    public int FusionTurnCounter { get; set; }

    /// <summary>Forme de fusion du champion pour cette émotion (null si elle n'en a pas).</summary>
    public FusionData GetFusion(EmotionType emotion) =>
        championData != null ? championData.fusions.Find(f => f != null && f.emotion == emotion) : null;

    /// <summary>Forme de la fusion en cours (null si le champion n'est pas fusionné).</summary>
    public FusionData ActiveFusion => Gauge.IsFused ? GetFusion(Gauge.ActiveFusion) : null;

    /// <summary>
    /// Une carte vient d'être jouée : sa jauge d'émotion monte. Pendant une fusion, une carte d'une autre
    /// émotion (les cartes neutres n'y touchent pas) fait baisser la jauge de la fusion : à 0, elle prend fin.
    /// </summary>
    public void OnCardPlayed(CardData card)
    {
        if (card == null) return;

        bool changed = false;
        if (Gauge.IsFused && card.emotionType != EmotionType.None && card.emotionType != Gauge.ActiveFusion)
        {
            FusionData fusion = ActiveFusion;
            changed = true;
            if (Gauge.DrainActiveFusion(GaugePointsPerCard) != EmotionType.None) fusion?.OnEnded(this);
        }

        changed |= Gauge.AddPoints(card.emotionType, GaugePointsPerCard);
        if (changed) EventBus.Publish(new FusionChangedEvent(this));
    }

    /// <summary>Fusionne avec l'émotion si c'est permis (jauge pleine, pas déjà fusionné, forme existante).</summary>
    public bool TryActivateFusion(EmotionType emotion)
    {
        FusionData fusion = GetFusion(emotion);
        if (fusion == null || !Gauge.TryActivate(emotion)) return false;

        FusionTurnCounter = 0;
        fusion.OnActivated(this);
        EventBus.Publish(new FusionChangedEvent(this));
        return true;
    }

    /// <summary>Une carte du champion vient d'infliger des dégâts à ces ennemis (hook de la fusion en cours).</summary>
    public void OnCardHitEnemies(CardData card, IReadOnlyList<Unit> enemies, bool firstOfCard)
    {
        ActiveFusion?.OnEnemiesHit(this, card, enemies, firstOfCard);
    }

    public override void OnOwnTurnStart()
    {
        base.OnOwnTurnStart();

        FusionTurnCounter = 0;
        if (!Gauge.IsFused) return;

        FusionData fusion = ActiveFusion;
        EmotionType ended = Gauge.OnTurnStart();
        if (ended != EmotionType.None) fusion?.OnEnded(this);
        else fusion?.OnTurnStart(this);
        EventBus.Publish(new FusionChangedEvent(this));
    }

    // Surcharge pour définir la faction automatiquement
    public override UnitFaction GetFaction() => UnitFaction.Player;

    // ========== GETTERS PUBLICS ==========

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise le champion avec les données ChampionData.
    /// </summary>
    public void Initialize(ChampionData data, Vector2Int initialGridPos, int level = 1)
    {
        if (_isInitialized) return;

        // Assigne la data utilisée pour l'initialisation
        this.championData = data;

        // Définit le nom
        gameObject.name = data.championName;

        // Initialise les stats de base (HP, Movement, ATK) via Unit
        InitUnitStats(data.maxHealth, data.movementRange, data.AttackAtLevel(level), data.ArmorAtLevel(level), data.MagicResistanceAtLevel(level));

        // Initialise le component PA depuis ChampionData
        _actionPointsComponent = new ActionPointsComponent(data.maxActionPoints, $"{name} (Champion)");

        // Initialise les aspects communs (Position, Faction, State) via Unit
        base.Initialize(initialGridPos);

        // Barre de vie au-dessus de la tête : les PV des alliés restent visibles (l'orbe ne montre
        // que le champion de ce PC)
        CreateHealthBar(_healthBarOffset, _healthBarColor);

        GameLog.Log($"{name} (Champion): Stats initialisées - HP: {GetHealth()}/{GetMaxHealth()}, PA: {GetCurrentPA()}/{GetMaxPA()}, PM: {GetMaxMovementPoints()}");
    }
}
