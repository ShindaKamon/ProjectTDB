using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Fin de combat : victoire (plus d'ennemi), défaite (plus de champion), invocations ignorées.
    /// </summary>
    public class BattleOutcomeTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        private T NewUnit<T>(int health = 100) where T : Unit
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            var unit = go.AddComponent<T>();
            System.Type type = typeof(Unit);
            type.GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, 100);
            type.GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, health);
            return unit;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _created.Clear();
        }

        [Test]
        public void ChampionsAndEnemiesAlive_Ongoing()
        {
            var units = new List<Unit> { NewUnit<EvanUnit>(), NewUnit<Enemy>() };
            Assert.AreEqual(BattleResult.Ongoing, BattleOutcome.Evaluate(units));
        }

        [Test]
        public void NoEnemyLeft_Victory()
        {
            var units = new List<Unit> { NewUnit<EvanUnit>(), NewUnit<Enemy>(health: 0) };
            Assert.AreEqual(BattleResult.Victory, BattleOutcome.Evaluate(units));
        }

        [Test]
        public void OnlySummonLeft_Defeat()
        {
            // Lyse ne compte pas : sans champion vivant, c'est la défaite
            var units = new List<Unit> { NewUnit<SummonUnit>(), NewUnit<Enemy>() };
            Assert.AreEqual(BattleResult.Defeat, BattleOutcome.Evaluate(units));
        }

        [Test]
        public void EverybodyDown_VictoryWins()
        {
            var units = new List<Unit> { NewUnit<EvanUnit>(health: 0), NewUnit<Enemy>(health: 0) };
            Assert.AreEqual(BattleResult.Victory, BattleOutcome.Evaluate(units));
        }
    }
}
