using System.Linq;
using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Boss caché sous les lits : PV répartis entre les lits, et il change toujours de lit.
    /// </summary>
    public class BedHideoutTests
    {
        [Test]
        public void SplitHealth_SumsToTotal_SharesDifferByAtMostOne()
        {
            int[] shares = BedHideout.SplitHealth(175, 6);

            Assert.AreEqual(175, shares.Sum());
            Assert.LessOrEqual(shares.Max() - shares.Min(), 1);
            CollectionAssert.AreEqual(new[] { 30, 29, 29, 29, 29, 29 }, shares);
        }

        [Test]
        public void NextBed_NeverStaysOnTheSameBed()
        {
            var rng = new System.Random(3);
            for (int current = 0; current < 5; current++)
                for (int i = 0; i < 50; i++)
                {
                    int next = BedHideout.NextBed(rng, 5, current);
                    Assert.AreNotEqual(current, next);
                    Assert.That(next, Is.InRange(0, 4));
                }
        }

        [Test]
        public void NextBed_LastBed_StaysThere()
        {
            Assert.AreEqual(0, BedHideout.NextBed(new System.Random(1), 1, 0));
        }
    }
}
