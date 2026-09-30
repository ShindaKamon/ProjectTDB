using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Fusion (Éveil) d'un champion : génération de jauge, activation validée, hooks de la forme,
    /// et Vol de mouvement de Crux en Terreur.
    /// </summary>
    public class FusionTests
    {
        private readonly List<Object> _created = new List<Object>();

        private T NewUnit<T>() where T : Unit
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            return go.AddComponent<T>();
        }

        private MovementStealFusion NewFear()
        {
            var fusion = ScriptableObject.CreateInstance<MovementStealFusion>();
            fusion.emotion = EmotionType.Fear;
            _created.Add(fusion);
            return fusion;
        }

        private CruxUnit NewCrux(FusionData fusion)
        {
            var data = ScriptableObject.CreateInstance<ChampionData>();
            _created.Add(data);
            data.fusions.Add(fusion);
            CruxUnit crux = NewUnit<CruxUnit>();
            crux.championData = data;
            crux.SetMaxMovementPoints(4);
            crux.RefreshMovement();
            return crux;
        }

        private CardData NewCard(EmotionType emotion)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.emotionType = emotion;
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void PlayingCards_FillsTheGaugeOfTheirEmotion()
        {
            CruxUnit crux = NewCrux(NewFear());

            crux.OnCardPlayed(NewCard(EmotionType.Fear));
            crux.OnCardPlayed(NewCard(EmotionType.Fear));
            crux.OnCardPlayed(NewCard(EmotionType.Joy));

            Assert.AreEqual(2 * Champion.GaugePointsPerCard, crux.Gauge.GetPoints(EmotionType.Fear));
            Assert.AreEqual(Champion.GaugePointsPerCard, crux.Gauge.GetPoints(EmotionType.Joy));
        }

        [Test]
        public void OtherEmotionCard_DuringFusion_DrainsTheFusion_AndFillsItsOwnGauge()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);

            crux.OnCardPlayed(NewCard(EmotionType.Anger));

            Assert.AreEqual(EmotionGauge.MaxPoints - Champion.GaugePointsPerCard, crux.Gauge.GetPoints(EmotionType.Fear));
            Assert.AreEqual(Champion.GaugePointsPerCard, crux.Gauge.GetPoints(EmotionType.Anger));
        }

        [Test]
        public void SameOrNeutralCard_DuringFusion_DoesNotDrain()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, 4);
            crux.Gauge.AddPoints(EmotionType.Fear, 2);
            crux.TryActivateFusion(EmotionType.Fear);

            crux.OnCardPlayed(NewCard(EmotionType.None));
            crux.OnCardPlayed(NewCard(EmotionType.Fear));

            Assert.AreEqual(EmotionGauge.MaxPoints, crux.Gauge.GetPoints(EmotionType.Fear));
        }

        [Test]
        public void OtherEmotionCards_CanEndTheFusion()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);

            for (int i = 0; i < EmotionGauge.MaxPoints; i++) crux.OnCardPlayed(NewCard(EmotionType.Joy));

            Assert.IsNull(crux.ActiveFusion);
            Assert.IsFalse(crux.Gauge.IsFused);
        }

        [Test]
        public void CanActivateFusion_NeedsFullGauge_AndAFormForTheEmotion()
        {
            CruxUnit crux = NewCrux(NewFear());
            Assert.IsFalse(GameActionValidator.CanActivateFusion(crux, EmotionType.Fear).IsValid, "jauge vide");

            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.Gauge.AddPoints(EmotionType.Joy, EmotionGauge.MaxPoints);
            Assert.IsTrue(GameActionValidator.CanActivateFusion(crux, EmotionType.Fear).IsValid);
            Assert.IsFalse(GameActionValidator.CanActivateFusion(crux, EmotionType.Joy).IsValid, "pas de forme Extase pour Crux");
            Assert.IsFalse(GameActionValidator.CanActivateFusion(NewUnit<Enemy>(), EmotionType.Fear).IsValid, "seul un champion fusionne");
        }

        [Test]
        public void TryActivateFusion_StartsTheFusion_AndCannotRepeat()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);

            Assert.IsTrue(crux.TryActivateFusion(EmotionType.Fear));

            Assert.IsNotNull(crux.ActiveFusion);
            Assert.IsFalse(GameActionValidator.CanActivateFusion(crux, EmotionType.Fear).IsValid, "déjà fusionné");
        }

        [Test]
        public void OwnTurnStart_BurnsATier_AndEndsTheFusionAtZero()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);

            crux.OnOwnTurnStart();
            crux.OnOwnTurnStart();
            Assert.IsNotNull(crux.ActiveFusion);

            crux.OnOwnTurnStart();
            Assert.IsNull(crux.ActiveFusion);
            Assert.IsFalse(crux.Gauge.IsFused);
        }

        [Test]
        public void MovementSteal_RemovesPMFromEnemy_AndGivesThemToCrux()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);
            var enemy = NewUnit<Enemy>();

            crux.OnCardHitEnemies(NewCard(EmotionType.Fear), new List<Unit> { enemy }, firstOfCard: true);

            Assert.AreEqual(1, ResourceDebuffManager.GetPending(enemy).pm);
            Assert.AreEqual(5, crux.GetCurrentMovementPoints());
        }

        [Test]
        public void MovementSteal_GainIsCappedPerTurn_AndResetsNextTurn()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);
            var enemy = NewUnit<Enemy>();
            var hit = new List<Unit> { enemy };
            CardData card = NewCard(EmotionType.Fear);

            for (int i = 0; i < 4; i++) crux.OnCardHitEnemies(card, hit, firstOfCard: true);
            Assert.AreEqual(6, crux.GetCurrentMovementPoints(), "plafond de 2 PM par tour");

            crux.OnOwnTurnStart();
            crux.OnCardHitEnemies(card, hit, firstOfCard: true);
            Assert.AreEqual(1, crux.FusionTurnCounter, "le plafond est remis à zéro chaque tour");
        }

        [Test]
        public void MovementSteal_MultiTargetCard_GivesPMOnlyOnce()
        {
            CruxUnit crux = NewCrux(NewFear());
            crux.Gauge.AddPoints(EmotionType.Fear, EmotionGauge.MaxPoints);
            crux.TryActivateFusion(EmotionType.Fear);
            var first = NewUnit<Enemy>();
            var second = NewUnit<Enemy>();
            CardData card = NewCard(EmotionType.Fear);

            crux.OnCardHitEnemies(card, new List<Unit> { first }, firstOfCard: true);
            crux.OnCardHitEnemies(card, new List<Unit> { second }, firstOfCard: false);

            Assert.AreEqual(5, crux.GetCurrentMovementPoints());
            Assert.AreEqual(1, ResourceDebuffManager.GetPending(second).pm, "chaque ennemi touché perd 1 PM");
        }

        [Test]
        public void Hits_WithoutFusion_DoNothing()
        {
            CruxUnit crux = NewCrux(NewFear());
            var enemy = NewUnit<Enemy>();

            crux.OnCardHitEnemies(NewCard(EmotionType.Fear), new List<Unit> { enemy }, firstOfCard: true);

            Assert.AreEqual(0, ResourceDebuffManager.GetPending(enemy).pm);
            Assert.AreEqual(4, crux.GetCurrentMovementPoints());
        }

        [Test]
        public void ActivateFusionCommand_RoundTrip()
        {
            CombatCommand command = CombatCommand.ActivateFusion(1, EmotionType.Fear);
            command.Turn = 5;

            CombatCommand copy = CombatCommand.Deserialize(command.Serialize());

            Assert.AreEqual(CombatCommandType.ActivateFusion, copy.Type);
            Assert.AreEqual(1, copy.Actor);
            Assert.AreEqual("Fear", copy.CardName);
            Assert.AreEqual(5, copy.Turn);
        }

        [Test]
        public void Fingerprint_ReflectsTheGauge()
        {
            CruxUnit crux = NewCrux(NewFear());
            var units = new List<Unit> { crux };
            string before = CombatStateFingerprint.Describe(units);

            crux.OnCardPlayed(NewCard(EmotionType.Fear));

            Assert.AreNotEqual(before, CombatStateFingerprint.Describe(units));
        }
    }
}
