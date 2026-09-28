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
        public void Shield_HasNoDuration_LastsUntilDepleted()
        {
            // Armure de rage : bouclier de 23, 20 dégâts → reste 3, quels que soient les tours écoulés
            Unit caster = NewUnit();
            Unit ally = NewUnit();
            ally.AddShield(23, caster);

            ally.TickEffectsOnTurnStartOf(ally);
            ally.TickEffectsOnTurnStartOf(caster);
            Object.DestroyImmediate(caster.gameObject);
            ally.TickEffectsOnTurnStartOf(ally);
            Assert.AreEqual(23, ally.GetShield(), "Ni les tours ni la mort du lanceur ne retirent le bouclier");

            ally.TakeDamage(20);
            Assert.AreEqual(3, ally.GetShield());
            Assert.AreEqual(100, ally.GetHealth());

            ally.TakeDamage(10);
            Assert.AreEqual(0, ally.GetShield());
            Assert.AreEqual(93, ally.GetHealth());
        }

        [Test]
        public void Lyse_ShieldAbsorbsBeforeHealth_AndSurvivesEvanHealthChanges()
        {
            // Lyse (PV = moitié de ceux d'Evan) : les dégâts entament d'abord son bouclier
            Unit evan = NewUnit(maxHealth: 100);
            var go = new GameObject("TestLyse");
            _createdGameObjects.Add(go);
            var lyse = go.AddComponent<LyseUnit>();
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true; // pas de grille en EditMode (placement ignoré)
            lyse.InitializeSummon(evan, Vector2Int.zero, 0);
            Assert.AreEqual(50, lyse.GetHealth());

            lyse.AddShield(10, evan);
            lyse.TakeDamage(6);
            Assert.AreEqual(4, lyse.GetShield(), "le bouclier baisse");
            Assert.AreEqual(50, lyse.GetHealth(), "la vie ne bouge pas");

            evan.TakeDamage(20); // Evan à 80 → Lyse recalculée à 40 PV max
            Assert.AreEqual(4, lyse.GetShield(), "le recalcul des PV de Lyse ne touche pas au bouclier");

            lyse.TakeDamage(10);
            Assert.AreEqual(0, lyse.GetShield());
            Assert.AreEqual(40 - 6, lyse.GetHealth(), "le reste (6) passe sur la vie");
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
            Assert.AreEqual(93, unit.GetHealth(), "Pas de second déclenchement : seul le reste de 3 absorbe");
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
