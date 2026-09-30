using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raze en Joie — « Partage des gains » : chaque combinaison (Paire, Suite, Bluff) soigne l'allié le plus
/// blessé (Raze compris) de X PV par PA dépensé dans la combinaison ; à pleine vie, il gagne un bouclier.
/// </summary>
[CreateAssetMenu(fileName = "ShareGainsFusion", menuName = "Champion/Fusion/Partage des gains")]
public class ShareGainsFusion : FusionData
{
    [Tooltip("PV rendus (ou bouclier) par PA dépensé dans la combinaison")]
    public int healPerPA = 2;

    public override void OnComboPattern(Champion champion, ComboPattern pattern, int paSpent)
    {
        if (pattern == ComboPattern.None || paSpent <= 0 || !Services.IsGridServiceAvailable()) return;

        Unit neediest = null;
        float lowestRatio = float.MaxValue;
        foreach (Unit unit in new List<Unit>(Services.Grid.GetAllUnits()))
        {
            if (unit == null || unit.GetHealth() <= 0 || unit.GetFaction() != champion.GetFaction()) continue;
            float ratio = (float)unit.GetHealth() / unit.GetMaxHealth();
            if (ratio < lowestRatio)
            {
                lowestRatio = ratio;
                neediest = unit;
            }
        }
        if (neediest == null) return;

        int amount = paSpent * healPerPA;
        if (neediest.GetHealth() < neediest.GetMaxHealth()) neediest.HealFrom(amount, champion);
        else neediest.AddShield(amount, champion);
        GameLog.Log($"[{formName}] {pattern} : {neediest.name} reçoit {amount} ({paSpent} PA)");
    }
}
