/// <summary>
/// Rôle d'une carte, déduit de ses effets, pour la distinguer d'un coup d'œil en main
/// (pictogramme et bandeau, voir CodexCardVisual.RoleName / RoleIcon / RoleChipKind).
/// </summary>
public enum CardRole
{
    Attack,    // inflige des dégâts
    Heal,      // soigne ou protège (bouclier)
    Movement,  // déplace le lanceur ou son invocation (charge, bond, repositionnement)
    Control,   // gêne l'ennemi sans le blesser (retrait de PA/PM, poussée, carte annulée)
    Support    // tout le reste : bonus, PA, pioche, invocation, coût d'une carte (Triche)
}

public static class CardRoles
{
    /// <summary>
    /// Priorité : déplacement du lanceur, puis dégâts, puis soin/bouclier, puis contrôle, sinon soutien
    /// (Bond percutant, qui bondit et frappe, est une carte de mouvement ; Frappe hésitante, qui frappe
    /// et ralentit, une carte d'attaque).
    /// </summary>
    public static CardRole RoleOf(CardData card)
    {
        if (card.isChargeCard || card.leapToTarget || card.isRepositionSummonCard) return CardRole.Movement;
        if (card.damageAmount > 0 || card.damageAroundTarget > 0 || card.comboDamagePerPASpent > 0) return CardRole.Attack;
        if (card.healAmount > 0 && !card.isSummonCard || card.shieldAmount > 0) return CardRole.Heal;
        if (card.pmReduction > 0 || card.paReduction > 0 || card.removeAllMovement || card.cancelsEnemyNextCard
            || card.knockbackDistance > 0 || card.armorAmount < 0 || card.magicResistanceAmount < 0) return CardRole.Control;
        return CardRole.Support;
    }
}
