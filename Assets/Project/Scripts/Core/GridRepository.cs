using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Repository centralisé pour toutes les requêtes liées à la grille et au positionnement.
/// Pattern: Repository Pattern pour découpler la logique de grille.
/// Responsabilités:
/// - Gestion du dictionnaire de tuiles
/// - Conversion de coordonnées (monde ↔ grille)
/// - Requêtes de position (GetTileAtPosition, GetUnitAtGridPos)
/// - Pathfinding (BFS pour mouvement et attaque)
/// - Requêtes d'unités (GetAllPlayerUnits, GetAllUnits)
/// </summary>
public class GridRepository
{
    // ========== DONNÉES ==========
    private readonly Dictionary<Vector2, Tile> _tiles;
    private readonly List<Unit> _units;
    private readonly int _width;
    private readonly int _height;

    // ========== CACHE ==========
    // Cache pour GetAttackTiles (évite recalcul pendant hover)
    private struct AttackTilesCacheKey
    {
        public Vector2 Start;
        public int Range;
        public Unit IgnoreUnit;

        public AttackTilesCacheKey(Vector2 start, int range, Unit ignoreUnit)
        {
            Start = start;
            Range = range;
            IgnoreUnit = ignoreUnit;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttackTilesCacheKey other)
            {
                return Start == other.Start && Range == other.Range && IgnoreUnit == other.IgnoreUnit;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ Range.GetHashCode() ^ (IgnoreUnit?.GetHashCode() ?? 0);
        }
    }

    private Dictionary<AttackTilesCacheKey, List<Tile>> _attackTilesCache = new Dictionary<AttackTilesCacheKey, List<Tile>>();

    // ========== CONSTRUCTEUR ==========

    /// <summary>
    /// Initialise le repository avec les données de la grille
    /// </summary>
    public GridRepository(Dictionary<Vector2, Tile> tiles, List<Unit> units, int width, int height)
    {
        _tiles = tiles ?? new Dictionary<Vector2, Tile>();
        _units = units ?? new List<Unit>();
        _width = width;
        _height = height;

        Debug.Log($"GridRepository initialisé : {_tiles.Count} tuiles, {_units.Count} unités");
    }

    // ========== QUERIES DE TUILES ==========

    /// <summary>
    /// Retourne la tuile à une position donnée
    /// </summary>
    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    /// <summary>
    /// Convertit une position monde en position grille
    /// </summary>
    public Vector2 GetGridPosFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x + _width / 2.0f);
        int y = Mathf.RoundToInt(worldPos.z + _height / 2.0f);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Retourne toutes les tuiles de la grille
    /// </summary>
    public Dictionary<Vector2, Tile> GetAllTiles()
    {
        return _tiles;
    }

    /// <summary>
    /// Vérifie si une position existe dans la grille
    /// </summary>
    public bool IsValidGridPosition(Vector2 pos)
    {
        return _tiles.ContainsKey(pos);
    }

    // ========== QUERIES D'UNITÉS ==========

    /// <summary>
    /// Retourne l'unité à une position grille donnée (null si aucune)
    /// </summary>
    public Unit GetUnitAtGridPos(Vector2 gridPos)
    {
        foreach (Unit unit in _units)
        {
            if (unit.GetCurrentGridPos() == gridPos)
            {
                return unit;
            }
        }
        return null;
    }

    /// <summary>
    /// Retourne toutes les unités joueur
    /// </summary>
    public List<Unit> GetAllPlayerUnits()
    {
        List<Unit> playerUnits = new List<Unit>();
        foreach (Unit unit in _units)
        {
            if (unit.GetFaction() == Unit.UnitFaction.Player)
            {
                playerUnits.Add(unit);
            }
        }
        return playerUnits;
    }

    /// <summary>
    /// Retourne toutes les unités ennemies
    /// </summary>
    public List<Unit> GetAllEnemyUnits()
    {
        List<Unit> enemyUnits = new List<Unit>();
        foreach (Unit unit in _units)
        {
            if (unit.GetFaction() == Unit.UnitFaction.Enemy)
            {
                enemyUnits.Add(unit);
            }
        }
        return enemyUnits;
    }

    /// <summary>
    /// Retourne toutes les unités de la grille
    /// </summary>
    public List<Unit> GetAllUnits()
    {
        return _units;
    }

    /// <summary>
    /// Ajoute une unité à la liste (appelé lors de l'apparition)
    /// </summary>
    public void AddUnit(Unit unit)
    {
        if (!_units.Contains(unit))
        {
            _units.Add(unit);
            Debug.Log($"GridRepository: Unité ajoutée - {unit.name}");
        }
    }

    /// <summary>
    /// Retire une unité de la liste (appelé lors de la mort)
    /// </summary>
    public void RemoveUnit(Unit unit)
    {
        if (_units.Remove(unit))
        {
            Debug.Log($"GridRepository: Unité retirée - {unit.name}");
        }
    }

    // ========== PATHFINDING & PORTÉES ==========

    /// <summary>
    /// Retourne toutes les tuiles accessibles pour le mouvement avec leur coût
    /// Utilise BFS (Breadth-First Search) pour calculer les tuiles atteignables
    /// </summary>
    public Dictionary<Tile, int> GetMovementTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
    {
        Dictionary<Tile, int> reachableTilesWithCost = new Dictionary<Tile, int>();
        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, int> visited = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        visited[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2 currentPos = queue.Dequeue();
            int currentCost = visited[currentPos];
            Tile currentTile = GetTileAtPosition(currentPos);

            if (currentTile != null && !reachableTilesWithCost.ContainsKey(currentTile))
            {
                reachableTilesWithCost.Add(currentTile, currentCost);
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                Unit unitAtNeighbor = GetUnitAtGridPos(neighborPos);
                bool isOccupied = (unitAtNeighbor != null && unitAtNeighbor != ignoreUnit);

                if (_tiles.ContainsKey(neighborPos) &&
                    !visited.ContainsKey(neighborPos) &&
                    currentCost + 1 <= range &&
                    !isOccupied)
                {
                    visited[neighborPos] = currentCost + 1;
                    queue.Enqueue(neighborPos);
                }
            }
        }

        return reachableTilesWithCost;
    }

    /// <summary>
    /// Retourne toutes les tuiles dans la portée d'attaque (ignore les obstacles d'unités)
    /// OPTIMISATION: Utilise un cache pour éviter de recalculer pendant le hover
    /// </summary>
    public List<Tile> GetAttackTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
    {
        // Vérifie le cache
        AttackTilesCacheKey cacheKey = new AttackTilesCacheKey(startPos, range, ignoreUnit);
        if (_attackTilesCache.TryGetValue(cacheKey, out List<Tile> cachedResult))
        {
            return cachedResult;
        }

        List<Tile> reachableTiles = new List<Tile>();
        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, int> visited = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        visited[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2 currentPos = queue.Dequeue();
            int currentCost = visited[currentPos];
            Tile currentTile = GetTileAtPosition(currentPos);

            if (currentTile != null)
            {
                reachableTiles.Add(currentTile);
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                if (_tiles.ContainsKey(neighborPos) &&
                    !visited.ContainsKey(neighborPos) &&
                    currentCost + 1 <= range)
                {
                    visited[neighborPos] = currentCost + 1;
                    queue.Enqueue(neighborPos);
                }
            }
        }

        // Retire la tuile de départ si la portée > 0
        if (range > 0 && reachableTiles.Contains(GetTileAtPosition(startPos)))
        {
            reachableTiles.Remove(GetTileAtPosition(startPos));
        }

        // Met en cache
        _attackTilesCache[cacheKey] = reachableTiles;

        return reachableTiles;
    }

    /// <summary>
    /// Calcule le chemin le plus court entre deux positions
    /// Retourne une liste de tuiles à traverser (vide si impossible ou déjà sur la cible)
    /// </summary>
    public List<Tile> GetPathToTile(Vector2 startPos, Vector2 targetPos, int maxRange, Unit ignoreUnit = null)
    {
        if (startPos == targetPos)
        {
            return new List<Tile>();
        }

        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, Vector2> parentMap = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, int> costMap = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        costMap[startPos] = 0;

        Vector2 currentPos;
        while (queue.Count > 0)
        {
            currentPos = queue.Dequeue();
            int currentCost = costMap[currentPos];

            if (currentPos == targetPos)
            {
                break;
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                Unit unitAtNeighbor = GetUnitAtGridPos(neighborPos);
                bool isOccupied = (unitAtNeighbor != null && unitAtNeighbor != ignoreUnit);

                if (_tiles.ContainsKey(neighborPos) && !isOccupied)
                {
                    int newCost = currentCost + 1;

                    if (newCost <= maxRange &&
                        (!costMap.ContainsKey(neighborPos) || newCost < costMap[neighborPos]))
                    {
                        costMap[neighborPos] = newCost;
                        parentMap[neighborPos] = currentPos;
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }

        // Reconstruit le chemin en remontant depuis la cible
        List<Tile> path = new List<Tile>();
        if (parentMap.ContainsKey(targetPos))
        {
            currentPos = targetPos;
            while (currentPos != startPos)
            {
                path.Add(GetTileAtPosition(currentPos));
                currentPos = parentMap[currentPos];
            }
            path.Reverse();
        }

        return path;
    }

    /// <summary>
    /// Calcule le coût en PM pour atteindre une destination
    /// Retourne -1 si la destination est inaccessible
    /// </summary>
    public int GetPathCost(Vector2 startPos, Vector2 targetPos, int maxRange, Unit ignoreUnit = null)
    {
        List<Tile> path = GetPathToTile(startPos, targetPos, maxRange, ignoreUnit);
        return path.Count > 0 ? path.Count : (startPos == targetPos ? 0 : -1);
    }

    // ========== UTILITAIRES ==========

    /// <summary>
    /// Retourne le nombre total de tuiles dans la grille
    /// </summary>
    public int GetTileCount()
    {
        return _tiles.Count;
    }

    /// <summary>
    /// Retourne le nombre total d'unités
    /// </summary>
    public int GetUnitCount()
    {
        return _units.Count;
    }

    /// <summary>
    /// Retourne les dimensions de la grille
    /// </summary>
    public (int width, int height) GetGridDimensions()
    {
        return (_width, _height);
    }

    // ========== CACHE MANAGEMENT ==========

    /// <summary>
    /// Invalide le cache de GetAttackTiles (appelé quand la carte change ou le tour change)
    /// </summary>
    public void InvalidateAttackTilesCache()
    {
        _attackTilesCache.Clear();
    }

    /// <summary>
    /// Invalide tout le cache (appelé à chaque changement de tour)
    /// </summary>
    public void InvalidateAllCaches()
    {
        InvalidateAttackTilesCache();
    }
}
