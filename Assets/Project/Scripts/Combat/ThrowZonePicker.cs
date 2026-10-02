using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Choix des zones d'un lancer annoncé (boss, voir CardData.telegraphedZoneCount) : d'abord une zone sur chaque
/// champion, dans un ordre tiré au hasard, pour l'obliger à bouger, puis le reste au hasard sur le plateau.
/// Deux zones ne se chevauchent jamais. Le tirage ne dépend que du générateur reçu : avec la graine partagée
/// du combat, il est le même sur tous les PC.
/// </summary>
public static class ThrowZonePicker
{
    /// <param name="cells">Cases du plateau, dans le même ordre sur tous les PC.</param>
    /// <param name="championCells">Cases des champions.</param>
    /// <param name="count">Nombre de zones voulues (moins s'il n'y a pas la place).</param>
    /// <param name="radius">Rayon de chaque zone (0 = une case) : deux épicentres sont à plus de 2 × rayon l'un de l'autre.</param>
    /// <returns>Les épicentres des zones.</returns>
    public static List<Vector2Int> Pick(System.Random rng, IList<Vector2Int> cells, IList<Vector2Int> championCells, int count, int radius)
    {
        var picked = new List<Vector2Int>();
        bool IsFree(Vector2Int cell) => picked.TrueForAll(p => GridGeometry.Distance(p, cell) > 2 * radius);

        foreach (Vector2Int cell in Shuffled(rng, championCells))
            if (picked.Count < count && IsFree(cell)) picked.Add(cell);
        foreach (Vector2Int cell in Shuffled(rng, cells))
            if (picked.Count < count && IsFree(cell)) picked.Add(cell);
        return picked;
    }

    /// <summary>
    /// Zone du jouet qui s'anime (CardData.animatedToy) : une des zones tirées au hasard, jamais une zone posée
    /// sur un champion. -1 s'il n'y en a pas.
    /// </summary>
    public static int PickToyIndex(System.Random rng, IList<Vector2Int> epicenters, IList<Vector2Int> championCells)
    {
        var candidates = new List<int>();
        for (int i = 0; i < epicenters.Count; i++)
            if (!championCells.Contains(epicenters[i])) candidates.Add(i);
        return candidates.Count == 0 ? -1 : candidates[rng.Next(candidates.Count)];
    }

    // Mélange de Fisher-Yates, sur une copie
    private static List<Vector2Int> Shuffled(System.Random rng, IList<Vector2Int> source)
    {
        var list = new List<Vector2Int>(source);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
