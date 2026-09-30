using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crux en Joie — « Ascension » : chaque déplacement fait par une carte (charge, bond) de N cases soigne
/// Crux de N × soin par case (plafonné par tour), et les alliés proches de l'arrivée d'une part de ce soin.
/// </summary>
[CreateAssetMenu(fileName = "AscensionFusion", menuName = "Champion/Fusion/Ascension")]
public class AscensionFusion : FusionData
{
    [Tooltip("PV rendus à Crux par case parcourue")]
    public int healPerCase = 2;

    [Tooltip("Plafond de PV rendus à Crux au cours d'un tour")]
    public int healCapPerTurn = 10;

    [Tooltip("Rayon (en cases, depuis l'arrivée) des alliés soignés")]
    public int allyRadius = 2;

    [Tooltip("Part du soin de Crux rendue à chaque allié proche (0.5 = 50 %)")]
    public float allyShare = 0.5f;

    public override void OnDisplacement(Champion champion, CardData card, Vector2Int arrival, int cases)
    {
        int heal = Mathf.Min(cases * healPerCase, healCapPerTurn - champion.FusionTurnCounter);
        if (heal <= 0) return;

        champion.FusionTurnCounter += heal;
        champion.Heal(heal);
        GameLog.Log($"[{formName}] {champion.name} récupère {heal} PV ({champion.FusionTurnCounter}/{healCapPerTurn} ce tour)");

        if (!Services.IsGridServiceAvailable()) return;
        int allyHeal = Mathf.RoundToInt(heal * allyShare);
        if (allyHeal <= 0) return;

        foreach (Unit unit in new List<Unit>(Services.Grid.GetAllUnits()))
        {
            if (unit == null || unit == champion || unit.GetHealth() <= 0 || unit.GetFaction() != champion.GetFaction()) continue;
            if (GridGeometry.Distance(arrival, unit.GetCurrentGridPos()) > allyRadius) continue;

            unit.HealFrom(allyHeal, champion);
        }
    }
}
