using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plus court chemin sur la grille d'exploration (4 directions, largeur d'abord). C# pur : testable.
/// </summary>
public static class ExplorationPathfinder
{
    /// <summary>
    /// Cases à parcourir (départ exclu) pour atteindre la plus proche case vérifiant
    /// <paramref name="isGoal"/>. Liste vide si le départ est déjà un but, null si rien n'est
    /// atteignable. Les cases bloquées ne sont jamais traversées.
    /// </summary>
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int size,
        Func<Vector2Int, bool> isBlocked, Func<Vector2Int, bool> isGoal)
    {
        if (isGoal(from)) return new List<Vector2Int>();

        var cameFrom = new Dictionary<Vector2Int, Vector2Int> { [from] = from };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(from);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int dir in GridGeometry.Directions4)
            {
                Vector2Int next = current + dir;
                if (next.x < 0 || next.y < 0 || next.x >= size.x || next.y >= size.y) continue;
                if (cameFrom.ContainsKey(next) || isBlocked(next)) continue;

                cameFrom[next] = current;
                if (isGoal(next)) return Rebuild(cameFrom, from, next);
                queue.Enqueue(next);
            }
        }
        return null;
    }

    private static List<Vector2Int> Rebuild(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int from, Vector2Int end)
    {
        var path = new List<Vector2Int>();
        for (Vector2Int c = end; c != from; c = cameFrom[c]) path.Add(c);
        path.Reverse();
        return path;
    }
}
