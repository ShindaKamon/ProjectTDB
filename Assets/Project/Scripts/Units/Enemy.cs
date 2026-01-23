using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe Enemy hérite de Unit et représente les ennemis.
/// Les ennemis ont :
/// - Un système PA pour jouer leurs cartes de pattern (via ActionPointsComponent)
/// - Un deck séquentiel (pas de mélange) qui boucle
/// - Un comportement prévisible pour le joueur
/// </summary>
public class Enemy : Unit, IActionPointsUser
{
    // ========== ENEMY DATA ==========

    [Header("=== Enemy Specific ==")]
    [SerializeField] private EnemyData _enemyData;

    // ========== SYSTÈME PA (Points d'Action) ==========
    // Les ennemis ont leur propre système PA pour jouer leurs cartes de pattern
    // Utilise la composition avec ActionPointsComponent pour éviter la duplication de code
    // Note: maxActionPoints est initialisé depuis EnemyData, pas besoin de SerializeField

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

    // ========== DECK PATTERN ==========
    // Deck system pour pattern de combat
    private List<CardData> _combatDeck = new List<CardData>();
    private int _currentCardIndex = 0; // Index de la prochaine carte à jouer

    // ========== EVENTS ==========
    public event System.Action<CardData> OnNextCardChanged;   // Prochaine carte visible

    // ========== GETTERS PUBLICS ==========

    public int GetCurrentPA() => _actionPointsComponent?.GetCurrentPA() ?? 0;
    public int GetMaxPA() => _actionPointsComponent?.GetMaxPA() ?? 0;
    public EnemyData GetEnemyData() => _enemyData;
    public bool IsBoss() => _enemyData != null && _enemyData.isBoss;

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
            Debug.Log($"{name} (Enemy): Fin du cycle de cartes, retour au début du pattern");
        }

        return _combatDeck[_currentCardIndex];
    }

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise l'ennemi avec EnemyData (au lieu de ChampionData)
    /// </summary>
    public void InitializeEnemy(EnemyData data, Vector2 initialGridPos)
    {
        // Protection contre la double initialisation
        if (_isInitialized)
        {
            Debug.LogWarning($"{gameObject.name} (Enemy) est déjà initialisé. Initialisation ignorée.");
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
        InitUnitStats(data.maxHealth, data.movementRange, data.attackDamage);

        // Initialise le component PA depuis EnemyData
        _actionPointsComponent = new ActionPointsComponent(data.maxActionPoints, $"{gameObject.name} (Enemy)");

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

        Debug.Log($"{name} (Enemy) initialisé - HP: {_health}/{_maxHealth}, PA: {GetCurrentPA()}/{GetMaxPA()}, Deck: {_combatDeck.Count} cartes");
    }

    protected override void Start()
    {
        // Si l'ennemi n'a pas été initialisé (placé manuellement dans la scène)
        if (!_isInitialized)
        {
            if (_enemyData != null)
            {
                Vector2 currentWorldGridPos = Services.Grid.GetGridPosFromWorldPos(transform.position);
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

    // ========== IMPLÉMENTATION INTERFACE IActionPointsUser ==========

    public bool SpendPA(int amount)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Enemy): ActionPointsComponent n'est pas initialisé !");
            return false;
        }
        return _actionPointsComponent.SpendPA(amount);
    }

    public void RefreshPA()
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Enemy): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.RefreshPA();
    }

    public void SetMaxPA(int value)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Enemy): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.SetMaxPA(value);
    }

    public void ReduceCurrentPA(int amount)
    {
        if (_actionPointsComponent == null)
        {
            Debug.LogError($"{name} (Enemy): ActionPointsComponent n'est pas initialisé !");
            return;
        }
        _actionPointsComponent.ReduceCurrentPA(amount);
    }

    // ========== SYSTÈME DE DECK SÉQUENTIEL ==========

    /// <summary>
    /// Pioche et joue la prochaine carte du deck (séquentiel, pas de mélange)
    /// Retourne null si aucune carte disponible ou pas assez de PA
    /// </summary>
    public CardData DrawAndPlayNextCard()
    {
        // Vérifie s'il reste des cartes
        if (_combatDeck == null || _combatDeck.Count == 0)
        {
            Debug.LogWarning($"{name} (Enemy): Deck vide !");
            return null;
        }

        if (_currentCardIndex >= _combatDeck.Count)
        {
            // Fin du deck, recommence au début (boucle)
            Debug.Log($"{name} (Enemy): Fin du deck, retour au début");
            _currentCardIndex = 0;
        }

        CardData nextCard = _combatDeck[_currentCardIndex];

        // Vérifie si on a assez de PA
        if (nextCard.costPA > GetCurrentPA())
        {
            Debug.LogWarning($"{name} (Enemy): Pas assez de PA pour jouer {nextCard.cardName} (coût: {nextCard.costPA}, dispo: {GetCurrentPA()})");
            return null;
        }

        // Joue la carte
        Debug.Log($"{name} (Enemy) joue la carte: {nextCard.cardName}");
        _currentCardIndex++;

        // Notifie la prochaine carte (pour la preview)
        OnNextCardChanged?.Invoke(GetNextCard());

        return nextCard;
    }

    /// <summary>
    /// Réinitialise le deck au début (utilisé si on veut forcer un reset)
    /// </summary>
    public void ResetDeckIndex()
    {
        _currentCardIndex = 0;
        OnNextCardChanged?.Invoke(GetNextCard());
        Debug.Log($"{name} (Enemy): Index du deck réinitialisé");
    }

    /// <summary>
    /// Retourne le nombre de cartes restantes avant la boucle
    /// </summary>
    public int GetRemainingCardsInCycle()
    {
        if (_combatDeck == null || _combatDeck.Count == 0) return 0;
        return _combatDeck.Count - _currentCardIndex;
    }

    /// <summary>
    /// Appelé quand l'ennemi est détruit (mort ou autre raison)
    /// </summary>
    void OnDestroy()
    {
        // Notifie le BattleUIManager pour nettoyer les UI
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.OnEnemyDied(this);
        }
    }
}
