using UnityEngine;

/// <summary>
/// Classe abstraite Champion - Base pour tous les personnages jouables.
/// Les champions ont :
/// - Un système PA pour jouer leurs cartes personnelles (via ActionPointsComponent)
/// - Un deck personnel avec pioche/défausse/mélange (géré par DeckManager)
/// - Des stats spécifiques (défenses, etc.) définies dans les classes dérivées
/// - Potentiellement un système d'émotions (optionnel)
/// </summary>
public abstract class Champion : Unit, IActionPointsUser
{
    // ========== CHAMPION DATA ==========
    [Header("Champion Data")]
    [SerializeField] public ChampionData championData;

    // ========== SYSTÈME PA (Points d'Action) ==========
    // Les champions utilisent leurs PA pour jouer des cartes de leur deck personnel
    // Utilise la composition avec ActionPointsComponent pour éviter la duplication de code
    [Header("Champion Action Points")]
    [SerializeField] protected int _maxActionPoints = 3;         // PA (Points d'Action) maximum par tour (valeur initiale)

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

    // Surcharge pour définir la faction automatiquement
    public override UnitFaction GetFaction() => UnitFaction.Player;

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise le champion avec les données ChampionData.
    /// </summary>
    public void Initialize(ChampionData data, Vector2 initialGridPos)
    {
        if (_isInitialized) return;

        // Assigne la data utilisée pour l'initialisation
        this.championData = data;

        // Définit le nom
        gameObject.name = data.championName;

        // Initialise les stats de base (HP, Movement) via Unit
        InitUnitStats(data.maxHealth, data.movementRange);

        // Initialise le component PA depuis ChampionData
        _maxActionPoints = data.maxActionPoints;
        _actionPointsComponent = new ActionPointsComponent(_maxActionPoints, $"{name} (Champion)");

        // Initialise les aspects communs (Position, Faction, State) via Unit
        base.Initialize(initialGridPos);

        // Initialise le système d'émotion (méthode de Unit)
        InitEmotionSystem(data);

        Debug.Log($"{name} (Champion): Stats initialisées - HP: {GetHealth()}/{GetMaxHealth()}, PA: {GetCurrentPA()}/{GetMaxPA()}, PM: {GetMaxMovementPoints()}");
    }
}
