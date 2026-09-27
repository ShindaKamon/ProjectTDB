using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Bouclier en PV de Unit : absorption des dégâts, cumul, dégâts bruts, expiration.
    /// </summary>
    public class UnitShieldTests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(int maxHealth = 100)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxHealth(maxHealth);
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
        public void TakesTurns_FalseOnlyForSummons()
        {
            Assert.IsTrue(NewUnit().TakesTurns);

            var go = new GameObject("TestSummon");
            _createdGameObjects.Add(go);
            Assert.IsFalse(go.AddComponent<SummonUnit>().TakesTurns);
        }

        [Test]
        public void TakeDamage_ShieldAbsorbsBeforeHealth()
        {
            Unit unit = NewUnit();
            unit.AddShield(20, unit);

            unit.TakeDamage(15);

            Assert.AreEqual(5, unit.GetShield());
            Assert.AreEqual(100, unit.GetHealth());
        }

        [Test]
        public void TakeDamage_ExcessGoesThroughToHealth()
        {
            Unit unit = NewUnit();
            unit.AddShield(20, unit);

            unit.TakeDamage(30);

            Assert.AreEqual(0, unit.GetShield());
            Assert.AreEqual(90, unit.GetHealth());
        }

        [Test]
        public void AddShield_Stacks()
        {
            Unit unit = NewUnit();
            unit.AddShield(10, unit);
            unit.AddShield(15, unit);

            Assert.AreEqual(25, unit.GetShield());
        }

        [Test]
        public void TakeRawDamage_IgnoresShield()
        {
            Unit unit = NewUnit();
            unit.AddShield(20, unit);

            unit.TakeRawDamage(15);

            Assert.AreEqual(20, unit.GetShield());
            Assert.AreEqual(85, unit.GetHealth());
        }

        [Test]
        public void Shield_ExpiresOnlyAtSourceTurnStart()
        {
            Unit caster = NewUnit();
            Unit ally = NewUnit();
            ally.AddShield(20, caster);

            ally.TickEffectsOnTurnStartOf(ally);
            Assert.AreEqual(20, ally.GetShield(), "Le tour du porteur ne doit pas retirer un bouclier donné par un autre");

            ally.TickEffectsOnTurnStartOf(caster);
            Assert.AreEqual(0, ally.GetShield());
        }

        [Test]
        public void Shield_ExpiresWhenSourceIsGone()
        {
            Unit caster = NewUnit();
            Unit ally = NewUnit();
            ally.AddShield(20, caster);

            Object.DestroyImmediate(caster.gameObject);
            ally.TickEffectsOnTurnStartOf(ally);

            Assert.AreEqual(0, ally.GetShield());
        }

        [Test]
        public void ReactiveShield_AbsorbsFirstHitOnly()
        {
            Unit unit = NewUnit();
            unit.ArmReactiveShield(23, unit);
            Assert.AreEqual(0, unit.GetShield(), "Rien tant qu'on n'est pas touché");

            unit.TakeDamage(20);
            Assert.AreEqual(100, unit.GetHealth(), "Le bouclier se déclenche juste avant le coup et l'absorbe");
            Assert.AreEqual(3, unit.GetShield());

            unit.TickEffectsOnTurnStartOf(unit);
            unit.TakeDamage(10);
            Assert.AreEqual(90, unit.GetHealth(), "Pas de second déclenchement");
        }

        [Test]
        public void ReactiveShield_ExpiresUnusedAtCastersTurn()
        {
            Unit unit = NewUnit();
            unit.ArmReactiveShield(23, unit);

            unit.TickEffectsOnTurnStartOf(unit);
            unit.TakeDamage(10);

            Assert.AreEqual(90, unit.GetHealth());
        }

        [Test]
        public void GainMovement_CanExceedMaximumThisTurn()
        {
            Unit unit = NewUnit();
            unit.SetMaxMovementPoints(4);
            unit.RefreshMovement();

            unit.GainMovement(2);

            Assert.AreEqual(6, unit.GetCurrentMovementPoints());
        }
    }
}
