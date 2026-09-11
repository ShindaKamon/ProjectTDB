using UnityEngine;

/// <summary>
/// AlpinisteUnit hérite de Champion et représente le champion L'Alpiniste.
/// Passif : Réflexe du grimpeur — après un déplacement de charge (Piolet d'ascension),
/// s'il atterrit adjacent à un allié il gagne un bouclier (réduit les prochains dégâts subis
/// jusqu'à son prochain tour) ; s'il atterrit adjacent à un ennemi, il gagne un bonus de
/// dégâts sur la prochaine carte de dégâts jouée. Le joueur choisit tank ou assassin à
/// chaque déplacement, selon la cible visée.
/// </summary>
public class AlpinisteUnit : Champion, IActionPointsUser, IChargeLandingReactor, IOutgoingDamageModifier
{
    [Header("=== Réflexe du grimpeur ===")]
    [Tooltip("Réduction des dégâts subis quand le bouclier est actif (0.15 = -15%)")]
    [SerializeField] private float _shieldDamageReduction = 0.15f;

    [Tooltip("Bonus de dégâts sur la prochaine carte de dégâts jouée (0.15 = +15%)")]
    [SerializeField] private float _nextCardDamageBonus = 0.15f;

    // Bouclier actif jusqu'au prochain tour de L'Alpiniste (atterrissage près d'un allié)
    private bool _hasClimberShield = false;

    // Bonus de dégâts à usage unique sur la prochaine carte de dégâts (atterrissage près d'un ennemi)
    private bool _hasNextCardBonus = false;

    public new void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        GameLog.Log($"{name} (L'Alpiniste) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
    }

    protected override void Start()
    {
        base.Start();

        if (Services.IsBattleUIServiceAvailable())
        {
            Services.BattleUI.RegisterPlayer(this);
        }
    }

    // ========== IChargeLandingReactor ==========

    /// <summary>
    /// Appelé par CardData juste après la fin d'un déplacement de charge (Piolet d'ascension).
    /// Vérifie les 4 cases adjacentes (cardinales) pour déterminer tank ou assassin.
    /// </summary>
    public void OnChargeLanded()
    {
        if (!Services.IsGridServiceAvailable()) return;

        Vector2Int pos = GetCurrentGridPos();
        Vector2Int[] neighbors =
        {
            pos + Vector2Int.up, pos + Vector2Int.down, pos + Vector2Int.left, pos + Vector2Int.right
        };

        bool adjacentAlly = false;
        bool adjacentEnemy = false;

        foreach (var n in neighbors)
        {
            Unit unit = Services.Grid.GetUnitAtGridPos(n);
            if (unit == null || unit == this) continue;

            if (unit.GetFaction() == GetFaction())
                adjacentAlly = true;
            else
                adjacentEnemy = true;
        }

        if (adjacentAlly)
        {
            _hasClimberShield = true;
            GameLog.Log($"[Réflexe du grimpeur] {name} atterrit près d'un allié -> bouclier -{_shieldDamageReduction:P0} jusqu'à son prochain tour");
        }
        else if (adjacentEnemy)
        {
            _hasNextCardBonus = true;
            GameLog.Log($"[Réflexe du grimpeur] {name} atterrit près d'un ennemi -> +{_nextCardDamageBonus:P0} dégâts sur la prochaine carte");
        }
    }

    // ========== BOUCLIER (dégâts subis) ==========

    public override void TakeDamage(int rawDamage)
    {
        if (_hasClimberShield && rawDamage > 0)
        {
            int reduced = Mathf.Max(1, Mathf.RoundToInt(rawDamage * (1f - _shieldDamageReduction)));
            GameLog.Log($"[Réflexe du grimpeur] {name} réduit {rawDamage} -> {reduced} dégâts (bouclier actif)");
            base.TakeDamage(reduced);
            return;
        }

        base.TakeDamage(rawDamage);
    }

    /// <summary>
    /// Le bouclier expire au début du prochain tour de L'Alpiniste (protège tout le tour adverse).
    /// </summary>
    public override void ProcessBuffsOnTurnStart()
    {
        if (_hasClimberShield)
        {
            _hasClimberShield = false;
            GameLog.Log($"[Réflexe du grimpeur] Bouclier de {name} expiré (nouveau tour)");
        }

        base.ProcessBuffsOnTurnStart();
    }

    // ========== IOutgoingDamageModifier (bonus prochaine carte) ==========

    public float GetDamageMultiplier() => _hasNextCardBonus ? 1f + _nextCardDamageBonus : 1f;

    public void ConsumeDamageModifier() => _hasNextCardBonus = false;
}
