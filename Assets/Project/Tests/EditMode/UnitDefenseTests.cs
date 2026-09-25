using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Armure (dégâts physiques) et barrière (dégâts magiques) de Unit : soustraction fixe.
    /// </summary>
    public class UnitDefenseTests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(int armor, int barrier)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxHealth(100);
            unit.ModifyStats(0, armor, barrier, 0); // permanent
            return unit;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _createdGameObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _createdGameObjects.Clear();
        }

        [Test]
        public void Armor_ReducesPhysicalOnly()
        {
            Unit unit = NewUnit(armor: 7, barrier: 0);

            Assert.AreEqual(33, unit.ReduceByDefense(40, DamageType.Physique));
            Assert.AreEqual(40, unit.ReduceByDefense(40, DamageType.Magique));
        }

        [Test]
        public void Barrier_ReducesMagicalOnly()
        {
            Unit unit = NewUnit(armor: 0, barrier: 5);

            Assert.AreEqual(35, unit.ReduceByDefense(40, DamageType.Magique));
            Assert.AreEqual(40, unit.ReduceByDefense(40, DamageType.Physique));
        }

        [Test]
        public void Reduction_NeverBelowOne()
        {
            Unit unit = NewUnit(armor: 20, barrier: 0);

            Assert.AreEqual(1, unit.ReduceByDefense(9, DamageType.Physique));
            Assert.AreEqual(0, unit.ReduceByDefense(0, DamageType.Physique));
        }

        [Test]
        public void NegativeArmor_IncreasesDamage()
        {
            Unit unit = NewUnit(armor: -7, barrier: 0);

            Assert.AreEqual(47, unit.ReduceByDefense(40, DamageType.Physique));
        }

        [Test]
        public void TemporaryArmorBuff_ExpiresAtNextTurnStart()
        {
            Unit unit = NewUnit(armor: 3, barrier: 0);
            unit.ModifyStats(0, -7, 0, 1);
            Assert.AreEqual(-4, unit.GetArmor());

            unit.ProcessBuffsOnTurnStart();

            Assert.AreEqual(3, unit.GetArmor());
        }

        [Test]
        public void UnitChips_ShowAttackAndNonZeroProtections()
        {
            Unit unit = NewUnit(armor: 5, barrier: 0);
            unit.ModifyStats(30, 0, 0, 0);
            unit.AddShield(12, unit);

            var chips = CodexCardVisual.UnitChips(unit);

            Assert.AreEqual(new[] { "dmg", "armor", "shield" }, chips.ConvertAll(c => c.Icon));
            Assert.AreEqual("30", chips[0].Text);
        }

        [Test]
        public void ChampionChips_ShowAttackArmorBarrier_EvenWhenZero()
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            champion.attackDamage = 15;
            champion.armor = 0;
            champion.barrier = 4;
            champion.maxHealth = 100;
            champion.movementRange = 4;
            champion.maxActionPoints = 5;

            var chips = CodexCardVisual.ChampionChips(champion);
            var resources = CodexCardVisual.ChampionResourceChips(champion);
            Object.DestroyImmediate(champion);

            Assert.AreEqual(new[] { "dmg", "armor", "barrier" }, chips.ConvertAll(c => c.Icon));
            Assert.AreEqual(new[] { "15", "0", "4" }, chips.ConvertAll(c => c.Text));
            Assert.AreEqual(new[] { "heal", "pm", "pa" }, resources.ConvertAll(c => c.Icon));
            Assert.AreEqual(new[] { "100", "4", "5" }, resources.ConvertAll(c => c.Text));
        }
    }
}
