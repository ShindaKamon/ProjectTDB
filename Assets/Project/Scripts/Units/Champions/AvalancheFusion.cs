using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crux en Colère — « Avalanche » : Réflexe du grimpeur permanent (bonus de dégâts toujours actif, non
/// consommé), et chaque déplacement fait par une carte (charge, bond) de N cases inflige N × dégâts par
/// case à tous les ennemis au contact de la case d'arrivée.
/// </summary>
[CreateAssetMenu(fileName = "AvalancheFusion", menuName = "Champion/Fusion/Avalanche")]
public class AvalancheFusion : FusionData
{
    [Tooltip("Dégâts physiques par case parcourue, aux ennemis au contact de l'arrivée")]
    public int damagePerCase = 3;

    public override void OnDisplacement(Champion champion, CardData card, Vector2Int arrival, int cases)
    {
        if (!Services.IsGridServiceAvailable()) return;

        int damage = cases * damagePerCase;
        foreach (Unit unit in new List<Unit>(Services.Grid.GetAllUnits()))
        {
            if (unit == null || unit.GetHealth() <= 0 || unit.GetFaction() == champion.GetFaction()) continue;
            if (!GridGeometry.AreAdjacent(arrival, unit.GetCurrentGridPos())) continue;

            unit.TakeDamageFrom(unit.ReduceByDefense(damage, DamageType.Physical), champion);
            GameLog.Log($"[{formName}] {champion.name} percute {unit.name} : {damage} dégâts ({cases} cases)");
        }
    }
}
