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

    // Surcharge pour définir la faction automatiquement
    public override UnitFaction GetFaction() => UnitFaction.Player;

    // ========== GETTERS PUBLICS ==========

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise le champion avec les données ChampionData.
    /// </summary>
    public void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        if (_isInitialized) return;

        // Assigne la data utilisée pour l'initialisation
        this.championData = data;

        // Définit le nom
        gameObject.name = data.championName;

        // Initialise les stats de base (HP, Movement, ATK) via Unit
        InitUnitStats(data.maxHealth, data.movementRange, data.attackDamage, data.armor, data.magicResistance);

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
