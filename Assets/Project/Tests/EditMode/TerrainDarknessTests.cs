using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Terrain assombri (TerrainDarkness) jusqu'au prochain tour de celui qui l'a assombri, et passif « Tapi dans
    /// le noir » (Enemy.OnOwnTurnStart) : dans l'ombre et pas touché depuis son tour précédent, il se soigne.
    /// </summary>
    public class TerrainDarknessTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            TerrainDarkness.Clear();
            foreach (Object o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private static void Set(object target, System.Type type, string field, object value) =>
            type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

        // Monstre à 200 PV max avec le passif à 10 %, sans passer par InitializeEnemy (qui demande la grille)
        private Enemy NewBoss(int health, int healPercent = 10)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(data);
            data.darknessHealPercent = healPercent;
            var go = new GameObject("Boss");
            _created.Add(go);
            Enemy boss = go.AddComponent<Enemy>();
            Set(boss, typeof(Enemy), "_enemyData", data);
            Set(boss, typeof(Unit), "_maxHealth", 200);
            Set(boss, typeof(Unit), "_health", health);
            return boss;
        }

        [Test]
        public void Darkness_LastsUntilTheDarkenersNextTurn()
        {
            Enemy boss = NewBoss(200);
            Enemy other = NewBoss(200);

            TerrainDarkness.Darken(boss);
            TerrainDarkness.OnTurnStart(other);
            Assert.IsTrue(TerrainDarkness.IsDark, "le tour d'une autre unité ne dissipe pas l'ombre");

            TerrainDarkness.OnTurnStart(boss);
            Assert.IsFalse(TerrainDarkness.IsDark);
        }

        [Test]
        public void Lurk_InShadowAndUntouched_Heals10Percent()
        {
            Enemy boss = NewBoss(150);
            boss.OnOwnTurnStart(true); // premier tour : rien à comparer

            Assert.AreEqual(20, boss.OnOwnTurnStart(true), "10 % de 200");
            Assert.AreEqual(170, boss.GetHealth());
        }

        [Test]
        public void Lurk_TouchedOrOutOfShadow_NoHeal()
        {
            Enemy boss = NewBoss(150);
            boss.OnOwnTurnStart(true);

            boss.LoseHealth(5);
            Assert.AreEqual(0, boss.OnOwnTurnStart(true), "touché depuis son tour précédent");
            Assert.AreEqual(0, boss.OnOwnTurnStart(false), "hors de l'ombre");
        }

        [Test]
        public void Embrume_LosesPmInShadow_PaOtherwise()
        {
            Assert.AreEqual((1, 0), CardData.ResourceLoss(1, 0, true, false), "hors de l'ombre : -1 PA");
            Assert.AreEqual((0, 1), CardData.ResourceLoss(1, 0, true, true), "dans l'ombre : -1 PM");
            Assert.AreEqual((1, 0), CardData.ResourceLoss(1, 0, false, true), "carte ordinaire : inchangée");
        }

        [Test]
        public void Lurk_WithoutPassive_NoHeal()
        {
            Enemy sheep = NewBoss(150, healPercent: 0);
            sheep.OnOwnTurnStart(true);

            Assert.AreEqual(0, sheep.OnOwnTurnStart(true));
        }
    }
}
