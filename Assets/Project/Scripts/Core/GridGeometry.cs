using UnityEngine;

/// <summary>
/// Géométrie de la grille, commune à tout le jeu (décision du 24/09/2026) : grille carrée en
/// 8 directions. Une diagonale vaut 1 case (distance de Tchebychev), pour le déplacement, la
/// portée des cartes, les zones et l'adjacence. Utiliser ces helpers plutôt que de recalculer
/// une distance à la main.
/// </summary>
public static class GridGeometry
{
    /// <summary>Les 8 voisins d'une case : orthogonaux d'abord, puis diagonales.</summary>
    public static readonly Vector2Int[] Directions8 =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    /// <summary>Distance en cases (Tchebychev) : max(|dx|, |dy|).</summary>
    public static int Distance(Vector2Int a, Vector2Int b) =>
        Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

    /// <summary>Les deux cases sont-elles voisines (8 directions) ?</summary>
    public static bool AreAdjacent(Vector2Int a, Vector2Int b) => Distance(a, b) == 1;

    /// <summary>
    /// Les deux cases sont-elles sur une même ligne droite (ligne, colonne ou diagonale) ?
    /// Si oui, donne le pas unitaire de a vers b et le nombre de cases.
    /// </summary>
    public static bool TryGetLine(Vector2Int from, Vector2Int to, out Vector2Int step, out int length)
    {
        Vector2Int diff = to - from;
        bool straight = diff != Vector2Int.zero
                        && (diff.x == 0 || diff.y == 0 || Mathf.Abs(diff.x) == Mathf.Abs(diff.y));
        if (!straight)
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
    /// Direction de grille (parmi les 8) la plus proche d'un vecteur, par pas de 45°.
    /// Vector2Int.zero si le vecteur est nul.
    /// </summary>
    public static Vector2Int SnapDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return Vector2Int.zero;

        float angle = Mathf.Atan2(direction.y, direction.x);
        int octant = Mathf.RoundToInt(angle / (Mathf.PI / 4f));
        float snapped = octant * (Mathf.PI / 4f);
        return new Vector2Int(Mathf.RoundToInt(Mathf.Cos(snapped)), Mathf.RoundToInt(Mathf.Sin(snapped)));
    }

    /// <summary>Direction de grille la plus proche pour aller de from vers to.</summary>
    public static Vector2Int SnapDirection(Vector2Int from, Vector2Int to) => SnapDirection((Vector2)(to - from));
}
