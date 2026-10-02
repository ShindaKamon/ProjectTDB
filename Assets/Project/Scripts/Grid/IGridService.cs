using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface pour le service de grille.
/// Pattern: Service Locator + Dependency Injection
/// Exposé via ServiceLocator pour éviter les appels directs à GridManager.Instance
/// </summary>
public interface IGridService
{
    // ========== UNITÉS ==========

    /// <summary>
    /// Retourne l'unité actuellement active
    /// </summary>
    Unit GetActiveUnit();

    /// <summary>
    /// Retourne toutes les unités joueur
    /// </summary>
    List<Unit> GetAllPlayerUnits();

    /// <summary>
    /// Retourne toutes les unités ennemies
    /// </summary>
    List<Unit> GetAllEnemyUnits();

    /// <summary>
    /// Retourne toutes les unités
    /// </summary>
    List<Unit> GetAllUnits();

    /// <summary>
    /// Retourne l'unité à une position grille donnée
    /// </summary>
    Unit GetUnitAtGridPos(Vector2Int gridPos);

    /// <summary>
    /// Instancie et enregistre une invocation (SummonUnit) sur la grille, à la position
    /// donnée. Retourne null si la case est invalide/occupée ou si le prefab n'a pas de
    /// composant SummonUnit.
    /// </summary>
    SummonUnit SpawnSummon(GameObject prefab, Vector2Int gridPos, Unit owner, int maxHealth);

    /// <summary>
    /// Fait apparaître un monstre en cours de combat (ex. Invocation de mouton) : il joue en dernier dans
    /// l'ordre des tours. Retourne null si la case est invalide ou occupée.
    /// </summary>
    Enemy SpawnEnemy(EnemyData data, Vector2Int gridPos);

    /// <summary>
    /// Pose des débris (obstacle non ciblable, ex. un lit cassé) sur les cases données, qui doivent être libres.
    /// </summary>
    DebrisUnit SpawnDebris(GameObject prefab, IList<Vector2Int> cells, Quaternion rotation);

    // ========== TUILES ==========

    /// <summary>
    /// Retourne la tuile à une position donnée
    /// </summary>
    Tile GetTileAtPosition(Vector2Int pos);

    /// <summary>
    /// Toutes les cases du plateau, triées par x puis y (même ordre sur tous les PC)
    /// </summary>
    List<Vector2Int> GetAllCells();

    /// <summary>
    /// Convertit une position monde en position grille
    /// </summary>
    Vector2Int GetGridPosFromWorldPos(Vector3 worldPos);

    /// <summary>
    /// Surligne une tuile avec une couleur
    /// </summary>
    void HighlightTile(Vector2Int pos, Color color);

    // ========== PATHFINDING ==========

    /// <summary>
    /// Retourne toutes les tuiles accessibles pour le mouvement avec leur coût
    /// </summary>
    Dictionary<Tile, int> GetMovementTiles(Vector2Int startPos, int range, Unit ignoreUnit = null);

    /// <summary>
    /// Retourne toutes les tuiles dans la portée d'attaque
    /// </summary>
    List<Tile> GetAttackTiles(Vector2Int startPos, int range, Unit ignoreUnit = null);

    /// <summary>
    /// Calcule le chemin le plus court entre deux positions
    /// </summary>
    List<Tile> GetPathToTile(Vector2Int startPos, Vector2Int targetPos, int maxRange, Unit ignoreUnit = null);

    // ========== CACHE ==========

    /// <summary>
    /// Invalide le cache de GetAttackTiles
    /// </summary>
    void InvalidateAttackTilesCache();

    // ========== STATE MACHINE (Phase 3.4) ==========

    /// <summary>
    /// Retourne la TurnStateMachine
    /// </summary>
    TurnStateMachine GetTurnStateMachine();
}
