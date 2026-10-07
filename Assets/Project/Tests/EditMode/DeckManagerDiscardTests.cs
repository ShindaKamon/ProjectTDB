using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Défausse de la main par une carte en cours de résolution (ex: Rage aveugle).
    /// </summary>
    public class DeckManagerDiscardTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(string name)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            _created.Add(card);
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }

        [Test]
        public void SameShuffleSeed_SameDeckOrder()
        {
            // Réseau : chaque PC mélange le deck d'un joueur avec la même graine
            var cards = new List<CardData>();
            for (int i = 0; i < 12; i++) cards.Add(NewCard("C" + i));
            List<CardData> Deal(int seed)
            {
                var go = new GameObject("TestDeckManager");
                _created.Add(go);
                var deck = go.AddComponent<DeckManager>();
                deck.SetShuffleSeed(seed);
                deck.InitializeDeck(new List<CardData>(cards));
                return new List<CardData>(deck.GetHand());
            }

            CollectionAssert.AreEqual(Deal(42), Deal(42));
        }

        [Test]
        public void Draw_HasNoHandLimit_AndExcessIsDiscardedByChoice()
        {
            var cards = new List<CardData>();
            for (int i = 0; i < 8; i++) cards.Add(NewCard("C" + i));
            var go = new GameObject("TestDeckManager");
            _created.Add(go);
            var deck = go.AddComponent<DeckManager>();
            deck.InitializeDeck(cards); // main de départ : 5

            deck.DrawCards(2);
            Assert.AreEqual(7, deck.GetHand().Count, "On pioche au-delà du maximum");
            Assert.AreEqual(2, deck.ExcessCards);

            CardData chosen = deck.GetHand()[3];
            deck.DiscardFromHand(chosen);
            Assert.AreEqual(1, deck.ExcessCards);
            CollectionAssert.DoesNotContain(deck.GetHand(), chosen);
            Assert.AreEqual(1, deck.GetDiscardCount());
        }

        [Test]
        public void DiscardHandExcept_KeepsOneCopyOfThePlayedCard_AndCountsTheOthers()
        {
            CardData played = NewCard("Rage aveugle");
            CardData other = NewCard("Autre");
            var go = new GameObject("TestDeckManager");
            _created.Add(go);
            var deck = go.AddComponent<DeckManager>();
            deck.InitializeDeck(new List<CardData> { played, played, other });

            int discarded = deck.DiscardHandExcept(played);

            Assert.AreEqual(2, discarded);
            CollectionAssert.AreEqual(new[] { played }, deck.GetHand());
            Assert.AreEqual(2, deck.GetDiscardCount());
        }
    }
}
