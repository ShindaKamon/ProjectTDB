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
    Unit GetUnitAtGridPos(Vector2 gridPos);

    // ========== TUILES ==========

    /// <summary>
    /// Retourne la tuile à une position donnée
    /// </summary>
    Tile GetTileAtPosition(Vector2 pos);

    /// <summary>
    /// Convertit une position monde en position grille
    /// </summary>
    Vector2 GetGridPosFromWorldPos(Vector3 worldPos);

    /// <summary>
    /// Surligne une tuile avec une couleur
    /// </summary>
    void HighlightTile(Vector2 pos, Color color);

    // ========== PATHFINDING ==========

    /// <summary>
    /// Retourne toutes les tuiles accessibles pour le mouvement avec leur coût
    /// </summary>
    Dictionary<Tile, int> GetMovementTiles(Vector2 startPos, int range, Unit ignoreUnit = null);

    /// <summary>
    /// Retourne toutes les tuiles dans la portée d'attaque
    /// </summary>
    List<Tile> GetAttackTiles(Vector2 startPos, int range, Unit ignoreUnit = null);

    /// <summary>
    /// Calcule le chemin le plus court entre deux positions
    /// </summary>
    List<Tile> GetPathToTile(Vector2 startPos, Vector2 targetPos, int maxRange, Unit ignoreUnit = null);

    // ========== CACHE ==========

    /// <summary>
    /// Invalide le cache de GetAttackTiles
    /// </summary>
    void InvalidateAttackTilesCache();

    // ========== UI ==========

    /// <summary>
    /// Met à jour l'UI de l'unité active
    /// </summary>
    void UpdateUnitUI();

    // ========== STATE MACHINE (Phase 3.4) ==========

    /// <summary>
    /// Retourne la TurnStateMachine
    /// </summary>
    TurnStateMachine GetTurnStateMachine();

    // ========== REPOSITORY ==========

    /// <summary>
    /// Retourne le GridRepository pour accès bas niveau
    /// </summary>
    GridRepository GetGridRepository();
}
