using System.Collections.Generic;

/// <summary>
/// Statuts en cours sur une unité, sous forme de pastilles (bonus de stats temporaires, bouclier réactif,
/// PA/PM programmés pour son prochain tour, Ténacité). Classe pure : la liste est vide s'il n'y a rien à montrer.
/// </summary>
public static class UnitStatusChips
{
    public static List<CardChip> Build(Unit unit)
    {
        var chips = new List<CardChip>();
        if (unit == null) return chips;

        foreach (Unit.StatBuff buff in unit.ActiveBuffs)
        {
            var parts = new List<string>();
            if (buff.atkModifier != 0) parts.Add($"{Signed(buff.atkModifier)} ATQ");
            if (buff.armorModifier != 0) parts.Add($"{Signed(buff.armorModifier)} ARM");
            if (buff.magicResistanceModifier != 0) parts.Add($"{Signed(buff.magicResistanceModifier)} RM");
            if (parts.Count == 0) continue;

            bool malus = buff.atkModifier <= 0 && buff.armorModifier <= 0 && buff.magicResistanceModifier <= 0;
            ChipKind kind = malus ? ChipKind.Warn : buff.atkModifier > 0 ? ChipKind.Damage : ChipKind.Defense;
            chips.Add(new CardChip("buff", $"{string.Join(" ", parts)} · {buff.remainingTurns} t", kind));
        }

        int reactiveShield = unit.GetReactiveShield();
        if (reactiveShield > 0) chips.Add(new CardChip("shield", $"{reactiveShield} au coup", ChipKind.Shield));

        (int pa, int pm) = ResourceDebuffManager.GetPending(unit);
        if (pa > 0) chips.Add(new CardChip("pa", $"-{pa} au prochain tour", ChipKind.Warn));
        if (pm > 0) chips.Add(new CardChip("pm", $"-{pm} au prochain tour", ChipKind.Warn));

        int paBonus = ResourceDebuffManager.GetPendingActionBonus(unit);
        if (paBonus > 0) chips.Add(new CardChip("pa", $"+{paBonus} au prochain tour", ChipKind.ActionPoints));

        if (ResourceDebuffManager.IsPmImmune(unit)) chips.Add(new CardChip("lock", "Ténacité", ChipKind.Mute));

        if (unit is IRageUser rage)
        {
            if (rage.IsUnchained) chips.Add(new CardChip("buff", $"Déchaîné · {rage.UnchainedTurnsLeft} t", ChipKind.Damage));
            else chips.Add(new CardChip("buff", $"Rage {rage.RageStock}/{RageGauge.MaxStock}", ChipKind.Damage));
        }
        return chips;
    }

    private static string Signed(int value) => value > 0 ? $"+{value}" : value.ToString();
}
