using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Débris d'un lit cassé (combat du Monstre sous le lit) : obstacle qui bloque ses cases, sans tour, sans barre de
/// vie, qu'on ne peut ni cibler ni toucher. Les jouets de Pluie de jouets l'évitent ; Bric-à-brac le ramasse pour le
/// lancer (il disparaît). Créé par GridManager.SpawnDebris quand un lit tombe.
/// </summary>
public class DebrisUnit : Unit
{
    private readonly List<Vector2Int> _cells = new List<Vector2Int>();

    public override IEnumerable<Vector2Int> OccupiedCells => _cells;
    public override UnitFaction GetFaction() => UnitFaction.Enemy;
    public override bool TakesTurns => false;
    public override bool ShownInCombatSummary => false;
    public override bool IsTargetable => false;
    public override string DisplayName => "Débris";
    protected override bool ResistsDamage => true;

    public void InitializeDebris(IList<Vector2Int> cells, Quaternion rotation)
    {
        _cells.Clear();
        _cells.AddRange(cells);
        InitUnitStats(1, 0);
        Initialize(cells[0]);
        transform.rotation = rotation;
    }

    // Initialisé par InitializeDebris (GridManager), pas par Unit.Start
    protected override void Start() { }
}
