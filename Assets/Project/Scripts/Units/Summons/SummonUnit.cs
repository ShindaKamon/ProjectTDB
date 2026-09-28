using UnityEngine;

/// <summary>
/// Classe de base pour toute unité invoquée par un champion (ex: Lyse pour Evan).
/// Réutilisable pour d'autres personnages avec d'autres types d'invocations à l'avenir.
/// Ne joue pas de tour propre (PA/PM = 0 par défaut) : sert de pion positionnel contrôlé
/// indirectement par son invocateur via des cartes dédiées.
/// </summary>
public class SummonUnit : Unit
{
    [Tooltip("Couleur de la barre de vie flottante de l'invocation")]
    [SerializeField] protected Color _healthBarColor = Color.cyan;

    [Tooltip("Nom affiché en jeu (bulle de survol), ex. « Lyse »")]
    [SerializeField] private string _displayName;

    public override string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

    [Tooltip("Délai entre le coup de l'invocateur et l'écho de l'invocation (secondes) : les deux dégâts se lisent séparément")]
    [SerializeField] private float _echoDelay = 0.5f;

    protected Unit _owner;
    public Unit Owner => _owner;

    public void SetOwner(Unit owner) => _owner = owner;

    /// <summary>
    /// Initialise l'invocation : position sur la grille, PV de départ, propriétaire.
    /// </summary>
    public virtual void InitializeSummon(Unit owner, Vector2Int gridPos, int maxHealth)
    {
        _owner = owner;
        InitUnitStats(Mathf.Max(1, maxHealth), 0, 0);
        Initialize(gridPos);
    }

    protected override void Start()
    {
        base.Start();

        if (_isInitialized)
        {
            CreateHealthBar(new Vector3(0, 1.5f, 0), _healthBarColor);
        }
    }

    // Une invocation suit la faction de son propriétaire (par défaut Player, comme Unit de base).
    public override UnitFaction GetFaction() => _owner != null ? _owner.GetFaction() : base.GetFaction();

    // Pilotée par les cartes de son invocateur : pas de tour propre
    public override bool TakesTurns => false;

    /// <summary>
    /// Écho de l'invocation (ex: Miroir fraternel) : infligé après un court délai, pour qu'il se
    /// distingue du coup de l'invocateur. Annulé si la cible ou l'invocation meurt entre-temps.
    /// </summary>
    public void DealEcho(Unit target, int damage, DamageType damageType)
    {
        if (_echoDelay > 0f && isActiveAndEnabled)
            StartCoroutine(EchoAfterDelay(target, damage, damageType));
        else
            ApplyEcho(target, damage, damageType);
    }

    private System.Collections.IEnumerator EchoAfterDelay(Unit target, int damage, DamageType damageType)
    {
        yield return new WaitForSeconds(_echoDelay);
        ApplyEcho(target, damage, damageType);
    }

    // L'écho (40 % de l'attaque d'origine) subit la défense de SA cible : armure ou résistance
    // magique selon le type de dégâts, sans le minimum de 1 des autres coups (un écho entièrement
    // absorbé fait 0). Le bouclier l'absorbe ensuite normalement.
    private void ApplyEcho(Unit target, int rawDamage, DamageType damageType)
    {
        if (target == null || IsDeadUnit(target) || IsDeadUnit(this)) return;

        int defense = damageType == DamageType.Magical ? target.GetMagicResistance() : target.GetArmor();
        int damage = Mathf.Max(0, rawDamage - defense);

        // Écho nul : rien à infliger (ni bouclier réactif à déclencher), seulement l'affichage « -0 »
        if (damage <= 0)
            EventBus.Publish(new UnitDamagedEvent(target, this, 0));
        else
            target.TakeDamageFrom(damage, this);
    }

    private static bool IsDeadUnit(Unit unit)
    {
        UnitState state = unit.GetUnitState();
        return state != null && state.IsDead();
    }
}
