using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Armure (dégâts physiques) et résistance magique (dégâts magiques) de Unit : soustraction fixe.
    /// </summary>
    public class UnitDefenseTests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(int armor, int magicResistance)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxHealth(100);
            unit.ModifyStats(0, armor, magicResistance, 0); // permanent
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
            Unit unit = NewUnit(armor: 7, magicResistance: 0);

            Assert.AreEqual(33, unit.ReduceByDefense(40, DamageType.Physical));
            Assert.AreEqual(40, unit.ReduceByDefense(40, DamageType.Magical));
        }

        [Test]
        public void MagicResistance_ReducesMagicalOnly()
        {
            Unit unit = NewUnit(armor: 0, magicResistance: 5);

            Assert.AreEqual(35, unit.ReduceByDefense(40, DamageType.Magical));
            Assert.AreEqual(40, unit.ReduceByDefense(40, DamageType.Physical));
        }

        [Test]
        public void Reduction_NeverBelowOne()
        {
            Unit unit = NewUnit(armor: 20, magicResistance: 0);

            Assert.AreEqual(1, unit.ReduceByDefense(9, DamageType.Physical));
            Assert.AreEqual(0, unit.ReduceByDefense(0, DamageType.Physical));
        }

        [Test]
        public void NegativeArmor_IncreasesDamage()
        {
            Unit unit = NewUnit(armor: -7, magicResistance: 0);

            Assert.AreEqual(47, unit.ReduceByDefense(40, DamageType.Physical));
        }

        [Test]
        public void TemporaryArmorBuff_LastsUntilCastersNextTurn()
        {
            Unit caster = NewUnit(armor: 0, magicResistance: 0);
            Unit target = NewUnit(armor: 3, magicResistance: 0);
            target.ModifyStats(0, -7, 0, 1, caster);
            Assert.AreEqual(-4, target.GetArmor());

            target.TickEffectsOnTurnStartOf(target);
            Assert.AreEqual(-4, target.GetArmor(), "Le tour du porteur ne compte pas : durée en tours du lanceur");

            target.TickEffectsOnTurnStartOf(caster);
            Assert.AreEqual(3, target.GetArmor());
        }

        [Test]
        public void BuffWithoutSource_CountsInHolderTurns()
        {
            Unit unit = NewUnit(armor: 0, magicResistance: 0);
            unit.ModifyStats(0, 5, 0, 1);

            unit.TickEffectsOnTurnStartOf(unit);

            Assert.AreEqual(0, unit.GetArmor());
        }

        [Test]
        public void UnitChips_ShowAttackAndNonZeroProtections()
        {
            Unit unit = NewUnit(armor: 5, magicResistance: 0);
            unit.ModifyStats(30, 0, 0, 0);
            unit.AddShield(12, unit);

            var chips = CodexCardVisual.UnitChips(unit);

            Assert.AreEqual(new[] { "dmg", "armor", "shield", "pm" }, chips.ConvertAll(c => c.Icon), "Ordre ATQ, DEFP, DEFM, bouclier, PA, PM ; PM toujours affichés (0 compris)");
            Assert.AreEqual("30", chips[0].Text);
            Assert.AreEqual(ChipKind.Defense, chips[1].Kind, "Armure en gris");
            Assert.AreEqual(ChipKind.MovementPoints, chips[3].Kind, "PM en vert");
        }

        [Test]
        public void ChampionChips_ShowAttackArmorMagicResistance_EvenWhenZero()
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            champion.attackDamage = 15;
            champion.armor = 0;
            champion.magicResistance = 4;
            champion.maxHealth = 100;
            champion.movementRange = 4;
            champion.maxActionPoints = 5;

            var chips = CodexCardVisual.ChampionChips(champion);
            var resources = CodexCardVisual.ChampionResourceChips(champion);
            Object.DestroyImmediate(champion);

            Assert.AreEqual(new[] { "dmg", "armor", "magicresist" }, chips.ConvertAll(c => c.Icon));
            Assert.AreEqual(new[] { "15", "0", "4" }, chips.ConvertAll(c => c.Text));
            Assert.AreEqual(new[] { "heal", "pm", "pa" }, resources.ConvertAll(c => c.Icon));
            Assert.AreEqual(new[] { "100", "4", "5" }, resources.ConvertAll(c => c.Text));
        }
    }
}
