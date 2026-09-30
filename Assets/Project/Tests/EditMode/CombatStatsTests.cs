using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Récapitulatif du combat : dégâts et soins par unité, alliés d'un côté, ennemis de l'autre.
    /// </summary>
    public class CombatStatsTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        private T NewUnit<T>(string name) where T : Unit
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go.AddComponent<T>();
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
        public void DamageAndHealing_SplitBetweenAlliesAndEnemies()
        {
            var ally = NewUnit<Unit>("Evan");      // Unit de base : camp joueur
            var enemy = NewUnit<Enemy>("UnderBed");
            var stats = new CombatStats();

            stats.RecordDamage(ally, enemy, 20);
            stats.RecordDamage(ally, enemy, 12);
            stats.RecordHealing(ally, 15);
            stats.RecordDamage(enemy, ally, 25);

            Assert.AreEqual(1, stats.Allies.Count);
            Assert.AreEqual("Evan", stats.Allies[0].Name);
            Assert.AreEqual(32, stats.Allies[0].Damage);
            Assert.AreEqual(15, stats.Allies[0].Healing);
            Assert.AreEqual(1, stats.Enemies.Count);
            Assert.AreEqual(25, stats.Enemies[0].Damage);
        }

        [Test]
        public void SelfDamage_AndUnknownSource_NotCounted()
        {
            var ally = NewUnit<Unit>("Evan");
            var stats = new CombatStats();

            stats.RecordDamage(ally, ally, 23);   // contrecoup
            stats.RecordDamage(null, ally, 10);   // origine inconnue

            Assert.AreEqual(0, stats.Allies.Count);
        }

        [Test]
        public void Register_ListsUnitWithZero()
        {
            var ally = NewUnit<Unit>("Crux");
            var stats = new CombatStats();

            stats.Register(ally);

            Assert.AreEqual(1, stats.Allies.Count);
            Assert.AreEqual(0, stats.Allies[0].Damage);
        }
    }
}
