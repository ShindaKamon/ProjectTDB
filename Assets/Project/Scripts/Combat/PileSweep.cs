using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Au lit ! (phase 2 du Monstre sous le lit, décision du 02/10/2026) : les draps ramènent au Lit tous les tas de débris
/// du plateau. Chaque tas file vers le Lit le long d'un trajet en 4 directions (GridGeometry.StraightPath), de sa case
/// la plus proche du Lit vers la case du Lit la plus proche ; un champion sur ce trajet est touché. C# pur : testable.
/// </summary>
public static class PileSweep
{
    /// <summary>
    /// Cases traversées par un tas jusqu'au Lit, sans les cases du tas ni celles du Lit. Vide si le tas touche le Lit.
    /// </summary>
    public static List<Vector2Int> Path(IList<Vector2Int> pileCells, IList<Vector2Int> litCells)
    {
        var path = new List<Vector2Int>();
        if (pileCells.Count == 0 || litCells.Count == 0) return path;

        // Les deux cases les plus proches (premier couple trouvé à égalité : même résultat sur tous les PC)
        Vector2Int from = pileCells[0], to = litCells[0];
        int best = int.MaxValue;
        foreach (Vector2Int p in pileCells)
            foreach (Vector2Int l in litCells)
                if (GridGeometry.Distance(p, l) < best)
                {
                    best = GridGeometry.Distance(p, l);
                    from = p;
                    to = l;
                }

        foreach (Vector2Int cell in GridGeometry.StraightPath(from, to))
            if (!pileCells.Contains(cell) && !litCells.Contains(cell)) path.Add(cell);
        return path;
    }
}
