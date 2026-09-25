using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Retraits de PM : appliqués au prochain tour de la cible, une fois, sans cumul (le plus fort gagne).
    /// </summary>
    public class ResourceDebuffManagerTests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(int movement)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxMovementPoints(movement);
            unit.RefreshMovement();
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
        public void PmReduction_AppliesAtNextTurnStart_Once()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, 2, null);
            Assert.AreEqual(4, unit.GetCurrentMovementPoints(), "Rien ne change avant le tour de la cible");

            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);
            Assert.AreEqual(2, unit.GetCurrentMovementPoints());

            unit.RefreshMovement();
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);
            Assert.AreEqual(4, unit.GetCurrentMovementPoints(), "Le retrait ne dure qu'un tour");
        }

        [Test]
        public void RemoveAllMovement_EmptiesPm()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, int.MaxValue, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);

            Assert.AreEqual(0, unit.GetCurrentMovementPoints());
        }

        [Test]
        public void PmReductions_DoNotStack_StrongestWins()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, 1, null);
            ResourceDebuffManager.ApplyDebuff(unit, 0, 3, null);
            ResourceDebuffManager.ApplyDebuff(unit, 0, 2, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);

            Assert.AreEqual(1, unit.GetCurrentMovementPoints());
        }
    }
}
