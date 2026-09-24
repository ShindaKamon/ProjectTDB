using UnityEngine;

/// <summary>
/// Géométrie de la grille, commune à tout le jeu (décision du 24/09/2026) : grille carrée en
/// 4 directions (haut, bas, gauche, droite), pas de diagonales. Distance = distance de Manhattan
/// (|dx| + |dy|), pour le déplacement, la portée des cartes, les zones et l'adjacence. Utiliser
/// ces helpers plutôt que de recalculer une distance à la main.
/// </summary>
public static class GridGeometry
{
    /// <summary>Les 4 voisins d'une case.</summary>
    public static readonly Vector2Int[] Directions4 =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0)
    };

    /// <summary>Distance en cases (Manhattan) : |dx| + |dy|.</summary>
    public static int Distance(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    /// <summary>Les deux cases sont-elles voisines (4 directions) ?</summary>
    public static bool AreAdjacent(Vector2Int a, Vector2Int b) => Distance(a, b) == 1;

    /// <summary>
    /// Les deux cases sont-elles sur une même ligne droite (même ligne ou même colonne) ?
    /// Si oui, donne le pas unitaire de a vers b et le nombre de cases.
    /// </summary>
    public static bool TryGetLine(Vector2Int from, Vector2Int to, out Vector2Int step, out int length)
    {
        Vector2Int diff = to - from;
        if (diff == Vector2Int.zero || (diff.x != 0 && diff.y != 0))
        {
            step = Vector2Int.zero;
            length = 0;
            return false;
        }

        step = new Vector2Int(System.Math.Sign(diff.x), System.Math.Sign(diff.y));
        length = Distance(from, to);
        return true;
    }

    /// <summary>
    /// Direction de grille (parmi les 4) la plus proche d'un vecteur : l'axe dominant.
    /// Vector2Int.zero si le vecteur est nul.
    /// </summary>
    public static Vector2Int SnapDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return Vector2Int.zero;

        return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
            ? new Vector2Int(System.Math.Sign(direction.x), 0)
            : new Vector2Int(0, System.Math.Sign(direction.y));
    }

    /// <summary>Direction de grille la plus proche pour aller de from vers to.</summary>
    public static Vector2Int SnapDirection(Vector2Int from, Vector2Int to) => SnapDirection((Vector2)(to - from));
}
