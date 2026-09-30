using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Texte flottant des bonus/malus (CombatFeedbackManager.DescribeEffect).
    /// </summary>
    public class CombatFeedbackManagerTests
    {
        [TestCase(UnitEffect.Shield, 5, "+5 bouclier", ChipKind.Shield)]
        [TestCase(UnitEffect.Armor, -2, "-2 armure", ChipKind.Defense)]
        [TestCase(UnitEffect.ActionPoints, -1, "-1 PA", ChipKind.ActionPoints)]
        [TestCase(UnitEffect.MovementPoints, 2, "+2 PM", ChipKind.MovementPoints)]
        [TestCase(UnitEffect.MovementPoints, -int.MaxValue, "-tous les PM", ChipKind.MovementPoints)]
        [TestCase(UnitEffect.DamageTakenPercent, -15, "-15% dégâts subis", ChipKind.Shield)]
        [TestCase(UnitEffect.PmImmune, 0, "Tenace", ChipKind.Mute)]
        public void DescribeEffect_SignedAmountAndChipColor(UnitEffect effect, int amount, string expectedText, ChipKind expectedKind)
        {
            var (text, kind) = CombatFeedbackManager.DescribeEffect(effect, amount);

            Assert.AreEqual(expectedText, text);
            Assert.AreEqual(expectedKind, kind);
        }
    }
}
