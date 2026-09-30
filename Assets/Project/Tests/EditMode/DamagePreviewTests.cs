using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Prévision des dégâts au survol (DamagePreview) et pastilles de statuts (UnitStatusChips).
    /// </summary>
    public class DamagePreviewTests
    {
        private class ModifierUnit : Unit, IOutgoingDamageModifier
        {
            public float Multiplier = 1f;
            public float GetDamageMultiplier() => Multiplier;
            public void ConsumeDamageModifier() { }
        }

        private readonly List<Object> _created = new List<Object>();

        private T NewUnit<T>(int maxHealth = 100) where T : Unit
        {
            var go = new GameObject("TestUnit");
            _created.Add(go);
            var unit = go.AddComponent<T>();
            unit.SetMaxHealth(maxHealth);
            return unit;
        }

        private CardData NewCard(int damage, DamageType type = DamageType.Physical)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.damageAmount = damage;
            card.damageType = type;
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }

        [Test]
        public void TryPredict_AddsCasterAttack()
        {
            Unit source = NewUnit<Unit>();
            source.ModifyStats(3, 0, 0, 0);
            Unit target = NewUnit<Unit>();

            Assert.IsTrue(DamagePreview.TryPredict(NewCard(20), source, target, out DamagePreview.Entry entry));
            Assert.AreEqual(23, entry.Damage);
        }

        [Test]
        public void TryPredict_BaseDamage()
        {
            Unit source = NewUnit<Unit>();
            Unit target = NewUnit<Unit>();

            Assert.IsTrue(DamagePreview.TryPredict(NewCard(20), source, target, out DamagePreview.Entry entry));
            Assert.AreEqual(20, entry.Damage);
            Assert.IsFalse(entry.Lethal);
        }

        [Test]
        public void TryPredict_AddsNextAttackBonus_WithoutConsumingIt()
        {
            Unit source = NewUnit<Unit>();
            Unit target = NewUnit<Unit>();
            source.AddNextAttackBonus(5);

            DamagePreview.TryPredict(NewCard(20), source, target, out DamagePreview.Entry entry);

            Assert.AreEqual(25, entry.Damage);
            Assert.AreEqual(5, source.GetNextAttackBonus());
        }

        [Test]
        public void TryPredict_AppliesOutgoingMultiplier()
        {
            var source = NewUnit<ModifierUnit>();
            source.Multiplier = 1.5f;
            Unit target = NewUnit<Unit>();

            DamagePreview.TryPredict(NewCard(20), source, target, out DamagePreview.Entry entry);

            Assert.AreEqual(30, entry.Damage);
        }

        [Test]
        public void TryPredict_ArmorReducesPhysical_MagicResistanceReducesMagical()
        {
            Unit source = NewUnit<Unit>();
            Unit target = NewUnit<Unit>();
            target.ModifyStats(0, 6, 2, 0);

            DamagePreview.TryPredict(NewCard(20, DamageType.Physical), source, target, out DamagePreview.Entry physical);
            DamagePreview.TryPredict(NewCard(20, DamageType.Magical), source, target, out DamagePreview.Entry magical);

            Assert.AreEqual(14, physical.Damage);
            Assert.AreEqual(18, magical.Damage);
        }

        [Test]
        public void TryPredict_Lethal_CountsShieldAndHealth()
        {
            Unit source = NewUnit<Unit>();
            Unit target = NewUnit<Unit>(20);
            target.AddShield(10, target);

            DamagePreview.TryPredict(NewCard(29), source, target, out DamagePreview.Entry survives);
            DamagePreview.TryPredict(NewCard(30), source, target, out DamagePreview.Entry dies);

            Assert.IsFalse(survives.Lethal);
            Assert.IsTrue(dies.Lethal);
        }

        [Test]
        public void IsPredictable_FalseForCardsWithoutDamageOrWithVariableDamage()
        {
            Assert.IsFalse(DamagePreview.IsPredictable(null));
            Assert.IsFalse(DamagePreview.IsPredictable(NewCard(0)));

            CardData combo = NewCard(10);
            combo.scalesWithPASpentThisTurn = true;
            Assert.IsFalse(DamagePreview.IsPredictable(combo));

            CardData charge = NewCard(10);
            charge.isChargeCard = true;
            Assert.IsFalse(DamagePreview.IsPredictable(charge));

            CardData discard = NewCard(10);
            discard.discardHandAttackBonusPerCard = 3;
            Assert.IsFalse(DamagePreview.IsPredictable(discard));
        }

        [Test]
        public void TryPredict_False_WhenCardNotPredictable()
        {
            CardData charge = NewCard(10);
            charge.isChargeCard = true;

            Assert.IsFalse(DamagePreview.TryPredict(charge, NewUnit<Unit>(), NewUnit<Unit>(), out _));
        }

        [Test]
        public void StatusChips_EmptyWithoutStatus()
        {
            Assert.IsEmpty(UnitStatusChips.Build(NewUnit<Unit>()));
            Assert.IsEmpty(UnitStatusChips.Build(null));
        }

        [Test]
        public void StatusChips_ListsTemporaryBuffAndReactiveShield_NotPermanentOnes()
        {
            Unit unit = NewUnit<Unit>();
            unit.ModifyStats(2, 3, 0, 2);   // temporaire : une pastille
            unit.ModifyStats(1, 1, 1, 0);   // permanent : aucune
            unit.ArmReactiveShield(8, unit);

            List<CardChip> chips = UnitStatusChips.Build(unit);

            Assert.AreEqual(2, chips.Count);
            StringAssert.Contains("+2 ATQ", chips[0].Text);
            StringAssert.Contains("+3 ARM", chips[0].Text);
            Assert.AreEqual("shield", chips[1].Icon);
        }

        [Test]
        public void StatusChips_ShowsPendingResourceDebuff()
        {
            Unit unit = NewUnit<Unit>();
            ResourceDebuffManager.ApplyDebuff(unit, 1, 2, null);

            List<CardChip> chips = UnitStatusChips.Build(unit);

            Assert.AreEqual(2, chips.Count);
            Assert.AreEqual("pa", chips[0].Icon);
            Assert.AreEqual("pm", chips[1].Icon);
        }
    }
}
