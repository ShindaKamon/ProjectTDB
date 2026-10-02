using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Boss caché sous les lits : casser tous les lits sauf un vide la barre de la phase 1, et il change
    /// toujours de lit.
    /// </summary>
    public class BedHideoutTests
    {
        [Test]
        public void BedHealth_AllBedsButOne_EmptyThePhaseBar()
        {
            Assert.AreEqual(20, BedHideout.BedHealth(100, 6), "5 lits de 20 = 100");
            Assert.AreEqual(200, 5 * BedHideout.BedHealth(200, 6), "coop à 2 : barre doublée, lits doublés");
            Assert.GreaterOrEqual(5 * BedHideout.BedHealth(101, 6), 101, "arrondi au-dessus");
            Assert.AreEqual(100, BedHideout.BedHealth(100, 1), "un seul lit : toute la barre");
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
