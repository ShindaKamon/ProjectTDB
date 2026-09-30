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
        public void ArmorReduction_BenefitsEveryAttacker_UntilCastersNextTurn()
        {
            // Rugissement destructeur d'Evan : -7 d'armure au boss pendant 1 tour d'Evan (coop)
            Unit boss = NewUnit(armor: 0, magicResistance: 0);
            Unit evan = NewUnit(0, 0);
            Unit crux = NewUnit(0, 0);
            boss.ModifyStats(0, -7, 0, 1, evan);

            boss.TickEffectsOnTurnStartOf(crux);
            Assert.AreEqual(27, boss.ReduceByDefense(20, DamageType.Physical), "Crux profite de la réduction");
            boss.TickEffectsOnTurnStartOf(boss);
            Assert.AreEqual(27, boss.ReduceByDefense(20, DamageType.Physical), "toujours là après le tour du boss");

            boss.TickEffectsOnTurnStartOf(evan);
            Assert.AreEqual(20, boss.ReduceByDefense(20, DamageType.Physical), "expire au prochain tour d'Evan");
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
        public void NextAttackBonus_AddsToNextDamagingCardOnly()
        {
            Unit caster = NewUnit(armor: 0, magicResistance: 0);
            Unit target = NewUnit(armor: 0, magicResistance: 0);

            var adrenaline = ScriptableObject.CreateInstance<CardData>();
            adrenaline.targetType = CardTargetType.Self;
            adrenaline.nextAttackBonus = 23;
            var strike = ScriptableObject.CreateInstance<CardData>();
            strike.targetType = CardTargetType.Enemy;
            strike.damageAmount = 11;

            adrenaline.ExecuteEffect(caster, caster);
            Assert.AreEqual(23, caster.GetNextAttackBonus());

            strike.ExecuteEffect(caster, target);
            Assert.AreEqual(100 - 34, target.GetHealth(), "11 + 23 de bonus");
            Assert.AreEqual(0, caster.GetNextAttackBonus(), "Bonus consommé");

            strike.ExecuteEffect(caster, target);
            Assert.AreEqual(100 - 34 - 11, target.GetHealth(), "Plus de bonus sur la carte suivante");

            Object.DestroyImmediate(adrenaline);
            Object.DestroyImmediate(strike);
        }

        [Test]
        public void NextAttackBonus_StacksAndIsNotUsedByNonDamagingCards()
        {
            Unit caster = NewUnit(armor: 0, magicResistance: 0);
            caster.AddNextAttackBonus(10);
            caster.AddNextAttackBonus(5);

            var heal = ScriptableObject.CreateInstance<CardData>();
            heal.targetType = CardTargetType.Self;
            heal.healAmount = 5;
            heal.ExecuteEffect(caster, caster);
            Object.DestroyImmediate(heal);

            Assert.AreEqual(15, caster.GetNextAttackBonus());
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
        public void UnitChips_ShowAttack_AndProtectionsEvenAtZero()
        {
            Unit unit = NewUnit(armor: 5, magicResistance: 0);
            unit.ModifyStats(30, 0, 0, 0);
            unit.AddShield(12, unit);

            var chips = CodexCardVisual.UnitChips(unit);

            Assert.AreEqual(new[] { "dmg", "armor", "magicresist", "shield", "pm" }, chips.ConvertAll(c => c.Icon),
                "Ordre ATQ, DEFP, DEFM, bouclier, PA, PM ; armure, résistance magique et PM toujours affichés (0 compris)");
            Assert.AreEqual("30", chips[0].Text);
            Assert.AreEqual(ChipKind.Defense, chips[1].Kind, "Armure en gris");
            Assert.AreEqual("0", chips[2].Text, "Résistance magique affichée même à 0");
            Assert.AreEqual(ChipKind.MovementPoints, chips[4].Kind, "PM en vert");
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
