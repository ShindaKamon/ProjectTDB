using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Evan en Joie — « Écho soigneur » : l'écho de Lyse ne blesse plus personne, il soigne tous les alliés
/// (Evan, Lyse et les autres champions) d'une part des dégâts de l'attaque d'origine.
/// </summary>
[CreateAssetMenu(fileName = "EchoHealFusion", menuName = "Champion/Fusion/Écho soigneur")]
public class EchoHealFusion : FusionData
{
    [Tooltip("Part de l'attaque d'origine rendue en PV à chaque allié (0.4 = 40 %, comme l'écho normal)")]
    public float healRatio = 0.4f;

    public override bool ReplaceSummonEcho(Champion champion, SummonUnit summon, int attackDamage)
    {
        int heal = Mathf.RoundToInt(attackDamage * healRatio);
        if (heal <= 0 || !Services.IsGridServiceAvailable()) return true;

        foreach (Unit unit in new List<Unit>(Services.Grid.GetAllUnits()))
        {
            if (unit == null || unit.GetHealth() <= 0 || unit.GetFaction() != champion.GetFaction()) continue;
            unit.HealFrom(heal, summon);
        }
        GameLog.Log($"[{formName}] l'écho de {summon.name} soigne les alliés de {heal} PV");
        return true;
    }
}
