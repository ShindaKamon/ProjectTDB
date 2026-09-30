using System.Collections.Generic;

/// <summary>
/// Empreinte de l'état du combat (réseau) : une ligne décrivant chaque unité, dans l'ordre des
/// tours. Calculée au début de chaque tour sur tous les PC ; deux PC synchronisés produisent le
/// même texte (voir DesyncDetector).
/// </summary>
public static class CombatStateFingerprint
{
    public static string Describe(IReadOnlyList<Unit> units)
    {
        var parts = new List<string>();
        foreach (Unit unit in units)
        {
            if (unit == null) continue;
            int pa = unit is IActionPointsUser paUser ? paUser.GetCurrentPA() : 0;
            string text = $"{unit.name}@{unit.GetCurrentGridPos()} PV{unit.GetHealth()} B{unit.GetShield()} PA{pa} PM{unit.GetCurrentMovementPoints()}"
                        + $" ARM{unit.GetArmor()} RM{unit.GetMagicResistance()} ATQ{unit.GetAttack()}+{unit.GetNextAttackBonus()}";
            if (unit is Champion champion)
                text += $" {champion.Gauge.Describe()} c{champion.FusionTurnCounter}";
            if (unit.TryGetComponentSafe(out DeckManager deck))
                text += $" main{deck.GetHand().Count} pioche{deck.GetDeckCount()} défausse{deck.GetDiscardCount()}";
            parts.Add(text);
        }
        if (FusionZones.Count > 0) parts.Add($"zones {FusionZones.Describe()}");
        return string.Join(" | ", parts);
    }
}
