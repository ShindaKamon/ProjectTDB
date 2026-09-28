using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Ciblage de base des monstres (EnemyAI.ChooseTarget) : le plus proche, puis le moins de PV.
    /// </summary>
    public class EnemyAITests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(Vector2Int gridPos, int health)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxHealth(100);
            typeof(Unit).GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, health);
            typeof(Unit).GetField("_currentGridPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, gridPos);
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
        public void ChooseTarget_ClosestFirst()
        {
            Unit near = NewUnit(new Vector2Int(2, 0), 100);
            Unit farButWeak = NewUnit(new Vector2Int(5, 0), 10);

            Assert.AreSame(near, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { farButWeak, near }));
        }

        [Test]
        public void ChooseTarget_SameDistance_LowestHealth()
        {
            Unit healthy = NewUnit(new Vector2Int(2, 0), 80);
            Unit weak = NewUnit(new Vector2Int(0, 2), 30);

            Assert.AreSame(weak, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { healthy, weak }));
        }

        [Test]
        public void ChooseTarget_AllInRange_LowestHealthEvenIfFarther()
        {
            // Attaque de portée 2 : Evan à 1 case, Lyse à 2 cases avec moins de PV → Lyse
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);
            Unit lyse = NewUnit(new Vector2Int(0, 2), 50);

            Assert.AreSame(lyse, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { evan, lyse }, attackRange: 2));
        }

        [Test]
        public void ChooseTarget_OutOfRange_ClosestStillWins()
        {
            // Portée 1 : seul Evan est à portée, Lyse (moins de PV) est trop loin
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);
            Unit lyse = NewUnit(new Vector2Int(0, 2), 50);

            Assert.AreSame(evan, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { evan, lyse }, attackRange: 1));
        }

        [Test]
        public void ChooseTarget_NoCandidate_ReturnsNull()
        {
            Assert.IsNull(EnemyAI.ChooseTarget(Vector2Int.zero, new List<Unit>()));
        }
    }
}
