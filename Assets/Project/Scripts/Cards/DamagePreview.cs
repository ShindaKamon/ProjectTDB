using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prévision des dégâts d'une carte au survol d'une cible (classe pure : ne modifie rien, ne consomme aucun bonus).
/// Reprend l'ordre de CardData.ExecuteEffect : dégâts de la carte + bonus de prochaine attaque + ATQ du lanceur, multiplicateur
/// du lanceur, puis armure ou résistance magique de la cible. Les cartes dont les dégâts dépendent du
/// déroulé (combo au PA dépensé, main défaussée, charge) ne sont pas prévues : mieux vaut rien afficher qu'un chiffre faux.
/// </summary>
public static class DamagePreview
{
    public readonly struct Entry
    {
        public readonly Unit Target;
        public readonly int Damage;   // après armure / résistance magique, avant bouclier
        public readonly bool Lethal;  // bouclier et PV actuels ne suffisent pas à l'encaisser

        public Entry(Unit target, int damage, bool lethal)
        {
            Target = target;
            Damage = damage;
            Lethal = lethal;
        }
    }

    /// <summary>La carte a-t-elle des dégâts que l'on sait prévoir ?</summary>
    public static bool IsPredictable(CardData card) =>
        card != null
        && card.damageAmount > 0
        && !card.scalesWithPASpentThisTurn
        && !card.isChargeCard
        && card.discardHandAttackBonusPerCard <= 0;

    /// <summary>Dégâts que la carte infligerait à cette unité ; false si rien à prévoir.</summary>
    public static bool TryPredict(CardData card, Unit source, Unit target, out Entry entry)
    {
        entry = default;
        if (source == null || target == null || target.GetHealth() <= 0 || !IsPredictable(card)) return false;

        int damage = card.damageAmount + source.GetNextAttackBonus() + source.GetAttack();
        if (source is IOutgoingDamageModifier modifier)
        {
            float multiplier = modifier.GetDamageMultiplier();
            if (multiplier != 1f) damage = Mathf.RoundToInt(damage * multiplier);
        }

        damage = target.ReduceByDefense(damage, card.damageType);
        entry = new Entry(target, damage, damage >= target.GetHealth() + target.GetShield());
        return true;
    }

    /// <summary>Dégâts par unité touchée si la carte est jouée sur cette case ; vide si rien à prévoir.</summary>
    public static List<Entry> Compute(CardData card, Unit source, Vector2Int epicenter)
    {
        var entries = new List<Entry>();
        if (source == null || !IsPredictable(card) || !Services.IsGridServiceAvailable()) return entries;

        foreach (Unit target in TargetsOf(card, source, epicenter))
        {
            if (TryPredict(card, source, target, out Entry entry)) entries.Add(entry);
        }
        return entries;
    }

    private static IEnumerable<Unit> TargetsOf(CardData card, Unit source, Vector2Int epicenter)
    {
        if (card.isAOE) return card.GetAOEAffectedUnits(source, epicenter);

        Unit target = Services.Grid.GetUnitAtGridPos(epicenter);
        // Carte « allié ou ennemi » : jamais de dégâts sur un allié (voir CardData.ExecuteEffect)
        bool ally = target != null && target.GetFaction() == source.GetFaction();
        if (target == null || (ally && card.targetType == CardTargetType.OtherUnit)) return new Unit[0];
        return new[] { target };
    }
}
