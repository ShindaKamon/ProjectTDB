using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Rage d'Ilya : une carte RAGE par palier de PV perdus, stock plafonné à 5 (cartes RAGE en main
    /// comprises), consommation totale.
    /// </summary>
    public class RageGaugeTests
    {
        [Test]
        public void HealthLost_OneCardPerTier_RemainderKept()
        {
            var rage = new RageGauge(10);

            Assert.AreEqual(2, rage.OnHealthLost(25, cardsInHand: 0));
            Assert.AreEqual(1, rage.OnHealthLost(5, cardsInHand: 0), "les 5 PV restants comptent pour le palier suivant");
            Assert.AreEqual(0, rage.OnHealthLost(9, cardsInHand: 0));
        }

        [Test]
        public void HealthLost_NeverMoreCardsThanTheStockCanTake()
        {
            var rage = new RageGauge(10);
            rage.Gain(3);

            Assert.AreEqual(1, rage.OnHealthLost(50, cardsInHand: 1), "3 en stock + 1 en main : une seule de plus");
            Assert.AreEqual(0, rage.OnHealthLost(50, cardsInHand: 2));
        }

        [Test]
        public void Gain_CappedAtMaxStock()
        {
            var rage = new RageGauge(10);

            Assert.AreEqual(4, rage.Gain(4));
            Assert.AreEqual(1, rage.Gain(3));
            Assert.AreEqual(RageGauge.MaxStock, rage.Stock);
            Assert.IsTrue(rage.IsFull);
        }

        [Test]
        public void ConsumeAll_ReturnsStockAndEmptiesIt()
        {
            var rage = new RageGauge(10);
            rage.Gain(3);

            Assert.AreEqual(3, rage.ConsumeAll());
            Assert.AreEqual(0, rage.Stock);
            Assert.AreEqual(0, rage.ConsumeAll());
        }
    }
}
