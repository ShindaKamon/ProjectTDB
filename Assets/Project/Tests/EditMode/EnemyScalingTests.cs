using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Stats des monstres selon le nombre de joueurs : PV × N, dégâts × (1 + 0,5 × (N − 1)).
    /// </summary>
    public class EnemyScalingTests
    {
        [TestCase(1, 500)]
        [TestCase(2, 1000)]
        [TestCase(3, 1500)]
        public void ScaledHealth_MultipliesByPlayerCount(int players, int expected)
        {
            Assert.AreEqual(expected, EnemyScaling.ScaledHealth(500, players));
        }

        [TestCase(1, 1f)]
        [TestCase(2, 1.5f)]
        [TestCase(3, 2f)]
        public void DamageMultiplier_AddsHalfPerExtraPlayer(int players, float expected)
        {
            Assert.AreEqual(expected, EnemyScaling.DamageMultiplier(players), 0.0001f);
        }

        [Test]
        public void ZeroPlayers_CountsAsSolo()
        {
            Assert.AreEqual(500, EnemyScaling.ScaledHealth(500, 0));
            Assert.AreEqual(1f, EnemyScaling.DamageMultiplier(0), 0.0001f);
        }
    }
}
