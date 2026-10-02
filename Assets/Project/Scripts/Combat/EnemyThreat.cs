using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone de menace d'un monstre (affichée au survol, décision du 02/10/2026) : les cases que sa prochaine carte peut
/// toucher ce tour-ci, depuis n'importe quelle case qu'il peut atteindre (portée en 4 directions, ligne de vue).
/// C# pur : testable sans grille.
/// </summary>
public static class EnemyThreat
{
    /// <param name="origins">Cases d'où il peut jouer sa carte (sa case et ses cases de déplacement).</param>
    /// <param name="range">Portée de la carte.</param>
    /// <param name="boardCells">Cases du plateau.</param>
    /// <param name="hasLineOfSight">Ligne de vue d'une case d'origine vers une case.</param>
    public static HashSet<Vector2Int> Cells(IEnumerable<Vector2Int> origins, int range, IEnumerable<Vector2Int> boardCells,
        System.Func<Vector2Int, Vector2Int, bool> hasLineOfSight)
    {
        var threat = new HashSet<Vector2Int>();
        if (range <= 0) return threat;
        var from = new List<Vector2Int>(origins);
        foreach (Vector2Int cell in boardCells)
            foreach (Vector2Int origin in from)
            {
                int distance = GridGeometry.Distance(origin, cell);
                if (distance >= 1 && distance <= range && hasLineOfSight(origin, cell))
                {
                    threat.Add(cell);
                    break;
                }
            }
        return threat;
    }
}
