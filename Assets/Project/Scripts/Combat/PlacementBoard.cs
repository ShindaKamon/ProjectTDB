using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Règles de la phase de placement (façon Dofus), en C# pur : des cases de départ, au plus une
/// unité par case ; une unité ne peut aller que sur une case de départ libre.
/// </summary>
public class PlacementBoard<T> where T : class
{
    private readonly List<Vector2Int> _startCells;
    private readonly Dictionary<T, Vector2Int> _positions = new Dictionary<T, Vector2Int>();

    public PlacementBoard(IEnumerable<Vector2Int> startCells)
    {
        _startCells = new List<Vector2Int>(startCells);
    }

    public IReadOnlyList<Vector2Int> StartCells => _startCells;

    public bool IsStartCell(Vector2Int cell) => _startCells.Contains(cell);

    public T UnitAt(Vector2Int cell)
    {
        foreach (var kvp in _positions)
        {
            if (kvp.Value == cell) return kvp.Key;
        }
        return null;
    }

    public bool TryGetPosition(T unit, out Vector2Int cell) => _positions.TryGetValue(unit, out cell);

    /// <summary>
    /// Pose chaque unité sur une case de départ, dans l'ordre des cases ; les unités en trop
    /// (plus d'unités que de cases) ne sont pas posées. Renvoie le nombre d'unités posées.
    /// </summary>
    public int PlaceInOrder(IReadOnlyList<T> units)
    {
        _positions.Clear();
        int count = Mathf.Min(units.Count, _startCells.Count);
        for (int i = 0; i < count; i++)
            _positions[units[i]] = _startCells[i];
        return count;
    }

    /// <summary>
    /// Déplace une unité posée vers une case de départ libre. Refusé hors des cases de départ,
    /// sur une case occupée par une autre unité, ou pour une unité non posée.
    /// </summary>
    public bool TryMove(T unit, Vector2Int target)
    {
        if (unit == null || !_positions.TryGetValue(unit, out Vector2Int from) || !IsStartCell(target))
            return false;

        if (from == target) return true;
        if (UnitAt(target) != null) return false;

        _positions[unit] = target;
        return true;
    }
}
