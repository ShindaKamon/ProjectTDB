using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Réflexe du grimpeur : un contact créé par une carte (ex: tirage de Corde de rappel) donne
    /// un bouclier avec un allié, un bonus de prochaine attaque avec un ennemi.
    /// </summary>
    public class CruxUnitTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        private T New<T>() where T : Unit
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            return go.AddComponent<T>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created) if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        [Test]
        public void ContactWithEnemy_GivesNextAttackBonus()
        {
            var crux = New<CruxUnit>();

            crux.OnContactCreated(New<Enemy>());

            Assert.AreEqual(1.15f, crux.GetDamageMultiplier(), 0.001f);
            Assert.AreEqual(0, crux.GetShield());
        }

        [Test]
        public void ContactWithAlly_GivesShield()
        {
            var crux = New<CruxUnit>();

            crux.OnContactCreated(New<EvanUnit>());

            Assert.AreEqual(15, crux.GetShield());
            Assert.AreEqual(1f, crux.GetDamageMultiplier(), 0.001f);
        }
    }
}
