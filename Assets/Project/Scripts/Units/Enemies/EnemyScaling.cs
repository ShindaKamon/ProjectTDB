using UnityEngine;

/// <summary>
/// Stats des monstres selon le nombre de joueurs (Enemies.md, acté le 24/09/2026, à valider
/// en playtest) : PV × N, dégâts × (1 + 0,5 × (N − 1)) ; PA, PM, portée et pattern inchangés.
/// Le barème de base (EnemyData) est celui d'un joueur.
/// </summary>
public static class EnemyScaling
{
    public static int ScaledHealth(int baseHealth, int playerCount)
    {
        return baseHealth * Mathf.Max(1, playerCount);
    }

    public static float DamageMultiplier(int playerCount)
    {
        return 1f + 0.5f * (Mathf.Max(1, playerCount) - 1);
    }
}
