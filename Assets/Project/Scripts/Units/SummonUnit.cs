using UnityEngine;

/// <summary>
/// Classe de base pour toute unité invoquée par un champion (ex: Lyse pour Soren).
/// Réutilisable pour d'autres personnages avec d'autres types d'invocations à l'avenir.
/// Ne joue pas de tour propre (PA/PM = 0 par défaut) : sert de pion positionnel contrôlé
/// indirectement par son invocateur via des cartes dédiées.
/// </summary>
public class SummonUnit : Unit
{
    [Tooltip("Couleur de la barre de vie flottante de l'invocation")]
    [SerializeField] protected Color _healthBarColor = Color.cyan;

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

    /// <summary>
    /// Téléporte l'invocation directement sur une nouvelle case (pas de pathfinding/animation —
    /// utilisé par les cartes de repositionnement comme Écho de Lyse).
    /// </summary>
    public virtual void TeleportTo(Vector2Int newPos)
    {
        Tile tile = Services.Grid.GetTileAtPosition(newPos);
        if (tile == null) return;

        _currentGridPos = newPos;
        transform.position = tile.transform.position + new Vector3(0, 0.5f, 0);
    }
}
