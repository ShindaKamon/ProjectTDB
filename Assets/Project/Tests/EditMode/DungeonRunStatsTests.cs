using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Bilan d'une expédition (DungeonRunStats) : combats gagnés, et dégâts et soins des alliés cumulés par nom.
    /// </summary>
    public class DungeonRunStatsTests
    {
        private static CombatStats.Entry Entry(string name, int damage, int healing) =>
            new CombatStats.Entry { Name = name, IsAlly = true, Damage = damage, Healing = healing };

        [Test]
        public void AddCombat_SumsPerAlly_AndCountsCombats()
        {
            var stats = new DungeonRunStats();

            stats.AddCombat(new List<CombatStats.Entry> { Entry("Raze", 120, 20) });
            stats.AddCombat(new List<CombatStats.Entry> { Entry("Raze", 80, 5), Entry("Evan", 60, 0) });

            Assert.AreEqual(2, stats.CombatsWon);
            Assert.AreEqual(2, stats.Allies.Count);
            Assert.AreEqual("Raze", stats.Allies[0].Name);
            Assert.AreEqual(200, stats.Allies[0].Damage);
            Assert.AreEqual(25, stats.Allies[0].Healing);
            Assert.AreEqual(60, stats.Allies[1].Damage, "Evan, arrivé au 2e combat");
        }
    }
}
