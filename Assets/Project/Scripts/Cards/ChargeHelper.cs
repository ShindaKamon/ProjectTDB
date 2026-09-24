using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Structure contenant les informations d'un chemin de charge
/// </summary>
public struct ChargePathInfo
{
    public bool IsValid;
    public Vector2Int StepDirection;
    public int Distance;
    public List<Tile> Path;
    public Unit EnemyHit;

    public static ChargePathInfo Invalid => new ChargePathInfo { IsValid = false };
}

/// <summary>
/// Classe utilitaire pour les calculs de charge
/// </summary>
public static class ChargeHelper
{
    /// <summary>
    /// Calcule la direction de charge entre deux positions (ligne droite uniquement)
    /// </summary>
    /// <returns>La direction normalisée ou Vector2.zero si pas en ligne droite</returns>
    public static bool TryGetChargeDirection(Vector2Int sourcePos, Vector2Int targetPos, out Vector2Int direction, out int distance)
    {
        // Même ligne ou même colonne (grille en 4 directions)
        return GridGeometry.TryGetLine(sourcePos, targetPos, out direction, out distance);
    }

    /// <summary>
    /// Calcule le chemin de charge complet, en s'arrêtant si une unité bloque
    /// </summary>
    public static ChargePathInfo CalculateChargePath(Vector2Int sourcePos, Vector2Int targetPos, Unit source)
    {
        if (!TryGetChargeDirection(sourcePos, targetPos, out Vector2Int stepDirection, out int totalDistance))
        {
            return ChargePathInfo.Invalid;
        }

        List<Tile> chargePath = new List<Tile>();
        Vector2Int currentPos = sourcePos;
        Unit enemyHit = null;

        for (int i = 0; i < totalDistance; i++)
        {
            Vector2Int nextPos = currentPos + stepDirection;
            Tile nextTile = Services.Grid.GetTileAtPosition(nextPos);

            if (nextTile == null) break; // Bord de la grille

            Unit unitOnTile = Services.Grid.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null)
            {
                if (unitOnTile.GetFaction() != source.GetFaction())
                {
                    enemyHit = unitOnTile;
                }
                break; // On s'arrête devant toute unité
            }

            chargePath.Add(nextTile);
            currentPos = nextPos;
        }

        return new ChargePathInfo
        {
            IsValid = true,
            StepDirection = stepDirection,
            Distance = totalDistance,
            Path = chargePath,
            EnemyHit = enemyHit
        };
    }

    /// <summary>
    /// Vérifie si une position cible est valide pour une charge (chemin non bloqué)
    /// </summary>
    public static bool IsValidChargeTarget(Vector2Int sourcePos, Vector2Int targetPos, Unit source)
    {
        if (!TryGetChargeDirection(sourcePos, targetPos, out Vector2Int stepDirection, out int distance))
        {
            return false;
        }

        // Vérifie chaque case sur le chemin jusqu'à la cible
        for (int i = 1; i <= distance; i++)
        {
            Vector2Int checkPos = sourcePos + stepDirection * i;
            Unit unitOnPath = Services.Grid.GetUnitAtGridPos(checkPos);

            if (unitOnPath != null)
            {
                // Il y a une unité sur le chemin
                if (checkPos == targetPos)
                {
                    // C'est la case cible : valide seulement si c'est un ennemi
                    return unitOnPath.GetFaction() != source.GetFaction();
                }
                else
                {
                    // C'est une case intermédiaire : le chemin est bloqué
                    return false;
                }
            }
        }

        // Aucune unité sur le chemin = valide
        return true;
    }
}
