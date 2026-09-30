using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Jauges d'émotion et fusion (Éveil) : paliers, activation, perte d'un palier par tour, fin à 0.
    /// </summary>
    public class EmotionGaugeTests
    {
        private static EmotionGauge Full(EmotionType emotion)
        {
            var gauge = new EmotionGauge();
            gauge.AddPoints(emotion, EmotionGauge.MaxPoints);
            return gauge;
        }

        [Test]
        public void AddPoints_CountsTiersAndCapsAtThree()
        {
            var gauge = new EmotionGauge();

            gauge.AddPoints(EmotionType.Fear, 3);
            Assert.AreEqual(1, gauge.GetTiers(EmotionType.Fear));

            gauge.AddPoints(EmotionType.Fear, 10);
            Assert.AreEqual(EmotionGauge.MaxPoints, gauge.GetPoints(EmotionType.Fear));
            Assert.AreEqual(3, gauge.GetTiers(EmotionType.Fear));
            Assert.IsFalse(gauge.AddPoints(EmotionType.Fear, 1), "jauge déjà pleine : rien ne change");
        }

        [Test]
        public void Gauges_AreIndependentPerEmotion_AndNoneIsIgnored()
        {
            var gauge = new EmotionGauge();

            gauge.AddPoints(EmotionType.Anger, 2);
            Assert.IsFalse(gauge.AddPoints(EmotionType.None, 2));

            Assert.AreEqual(2, gauge.GetPoints(EmotionType.Anger));
            Assert.AreEqual(0, gauge.GetPoints(EmotionType.Joy));
        }

        [Test]
        public void Activate_RequiresFullGauge()
        {
            var gauge = new EmotionGauge();
            gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints - 1);
            Assert.IsFalse(gauge.TryActivate(EmotionType.Fear));

            gauge.AddPoints(EmotionType.Fear, 1);
            Assert.IsTrue(gauge.TryActivate(EmotionType.Fear));
            Assert.AreEqual(EmotionType.Fear, gauge.ActiveFusion);
        }

        [Test]
        public void OnlyOneFusionAtATime()
        {
            EmotionGauge gauge = Full(EmotionType.Fear);
            gauge.AddPoints(EmotionType.Joy, EmotionGauge.MaxPoints);
            gauge.TryActivate(EmotionType.Fear);

            Assert.IsFalse(gauge.TryActivate(EmotionType.Joy));
            Assert.AreEqual(EmotionType.Fear, gauge.ActiveFusion);
        }

        [Test]
        public void Fusion_LosesOneTierPerTurn_AndEndsAtZero()
        {
            EmotionGauge gauge = Full(EmotionType.Fear);
            gauge.TryActivate(EmotionType.Fear);

            Assert.AreEqual(EmotionType.None, gauge.OnTurnStart());
            Assert.AreEqual(2, gauge.GetTiers(EmotionType.Fear));
            Assert.AreEqual(EmotionType.None, gauge.OnTurnStart());
            Assert.AreEqual(1, gauge.GetTiers(EmotionType.Fear));

            Assert.AreEqual(EmotionType.Fear, gauge.OnTurnStart(), "la fusion se termine et renvoie son émotion");
            Assert.IsFalse(gauge.IsFused);
            Assert.AreEqual(0, gauge.GetPoints(EmotionType.Fear));
        }

        [Test]
        public void Fusion_CardsOfTheEmotionRechargeIt()
        {
            EmotionGauge gauge = Full(EmotionType.Fear);
            gauge.TryActivate(EmotionType.Fear);
            gauge.OnTurnStart();
            gauge.OnTurnStart(); // 1 palier restant

            gauge.AddPoints(EmotionType.Fear, 2);

            Assert.AreEqual(EmotionType.None, gauge.OnTurnStart());
            Assert.IsTrue(gauge.IsFused);
        }

        [Test]
        public void DrainActiveFusion_LowersTheActiveGauge_AndEndsAtZero()
        {
            var gauge = new EmotionGauge();
            gauge.AddPoints(EmotionType.Anger, EmotionGauge.MaxPoints);
            gauge.TryActivate(EmotionType.Anger);

            for (int i = 0; i < EmotionGauge.MaxPoints - 1; i++) Assert.AreEqual(EmotionType.None, gauge.DrainActiveFusion(1));
            Assert.IsTrue(gauge.IsFused);

            Assert.AreEqual(EmotionType.Anger, gauge.DrainActiveFusion(1));
            Assert.IsFalse(gauge.IsFused);
            Assert.AreEqual(EmotionType.None, gauge.DrainActiveFusion(1), "sans fusion, rien à retirer");
        }

        [Test]
        public void OnTurnStart_WithoutFusion_ChangesNothing()
        {
            var gauge = new EmotionGauge();
            gauge.AddPoints(EmotionType.Anger, 4);

            Assert.AreEqual(EmotionType.None, gauge.OnTurnStart());
            Assert.AreEqual(4, gauge.GetPoints(EmotionType.Anger));
        }

        [Test]
        public void Describe_ChangesWithState()
        {
            var gauge = new EmotionGauge();
            string empty = gauge.Describe();

            gauge.AddPoints(EmotionType.Joy, 2);

            Assert.AreNotEqual(empty, gauge.Describe());
        }
    }
}
