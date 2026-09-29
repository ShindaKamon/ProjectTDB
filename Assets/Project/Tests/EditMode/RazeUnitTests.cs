using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Main gagnante : le Bluff (deux émotions différentes d'affilée) donne un bouclier.
    /// </summary>
    public class RazeUnitTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(EmotionType emotion, int cost)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.emotionType = emotion;
            card.costPA = cost;
            _created.Add(card);
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void TricheCost_CountsForSuite_AndPASpent()
        {
            // Carte à 2 PA passée à 3 par Triche, après une carte à 2 : Suite (+1 PA), 5 PA comptés
            var go = new GameObject("Raze");
            _created.Add(go);
            var deck = go.AddComponent<DeckManager>();
            var raze = go.AddComponent<RazeUnit>();
            CardData first = NewCard(EmotionType.Anger, 2);
            CardData cheated = NewCard(EmotionType.Anger, 2);
            deck.ModifyCardCost(cheated, 1);

            raze.OnCardAboutToExecute(first);
            raze.OnCardResolved(first);
            // La Suite donne +1 PA : Raze de test sans PA initialisés, d'où ce log attendu
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Raze (Champion): ActionPointsComponent n'est pas initialisé !");
            raze.OnCardAboutToExecute(cheated);
            raze.OnCardResolved(cheated);

            Assert.AreEqual(5, raze.PASpentThisTurn);
            Assert.IsFalse(raze.ShouldIgnoreDamageReduction, "3 après 2 : Suite, pas Paire");
        }

        [Test]
        public void Bluff_GivesShield()
        {
            var go = new GameObject("Raze");
            _created.Add(go);
            var raze = go.AddComponent<RazeUnit>();

            CardData anger = NewCard(EmotionType.Anger, 1);
            raze.OnCardAboutToExecute(anger);
            raze.OnCardResolved(anger);
            raze.OnCardAboutToExecute(NewCard(EmotionType.Fear, 3)); // émotion différente, ni suite ni paire

            Assert.AreEqual(8, raze.GetShield());
        }
    }
}
