using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Tests de régression pour la surcouche de coût de DeckManager
    /// (GetEffectiveCost / ModifyCardCost / ClearCostOverride).
    /// </summary>
    public class DeckManagerCostOverrideTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private CardData NewCard(int costPA, string name = "TestCard")
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.costPA = costPA;
            _created.Add(card);
            return card;
        }

        private DeckManager NewDeckManager()
        {
            var go = new GameObject("TestDeckManager");
            _createdGameObjects.Add(go);
            return go.AddComponent<DeckManager>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();

            foreach (var go in _createdGameObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _createdGameObjects.Clear();
        }

        [Test]
        public void GetEffectiveCost_ZeroCostCardWithoutOverride_StaysFree()
        {
            // Régression : le plancher "1 PA minimum" s'appliquait auparavant même sans
            // override actif, rendant les cartes à coût 0 injouables gratuitement
            // dès qu'un DeckManager était présent.
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 0);

            Assert.AreEqual(0, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void GetEffectiveCost_NoOverride_ReturnsBaseCost()
        {
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 2);

            Assert.AreEqual(2, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void ModifyCardCost_NegativeDelta_ReducesCost()
        {
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 2);

            deckManager.ModifyCardCost(card, -1);

            Assert.AreEqual(1, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void ModifyCardCost_OverrideBelowFloor_ClampsToOne()
        {
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 1);

            deckManager.ModifyCardCost(card, -5);

            Assert.AreEqual(1, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void ClearCostOverride_RemovesOverride_RevertsToBaseCost()
        {
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 2);
            deckManager.ModifyCardCost(card, -1);

            deckManager.ClearCostOverride(card);

            Assert.AreEqual(2, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void PlayCard_ClearsCostOverrideOnPlayedCard()
        {
            // Régression : ClearCostOverride était documenté comme devant s'appliquer "à la
            // défausse" mais n'était jamais appelé, laissant l'override permanent.
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 2);
            deckManager.InitializeDeck(new List<CardData> { card });
            deckManager.ModifyCardCost(card, -1);
            Assert.AreEqual(1, deckManager.GetEffectiveCost(card));

            deckManager.PlayCard(card);

            Assert.AreEqual(2, deckManager.GetEffectiveCost(card));
        }

        [Test]
        public void DiscardHand_ClearsCostOverridesOnDiscardedCards()
        {
            var deckManager = NewDeckManager();
            var card = NewCard(costPA: 2);
            deckManager.InitializeDeck(new List<CardData> { card });
            deckManager.ModifyCardCost(card, -1);

            deckManager.DiscardHand();

            Assert.AreEqual(2, deckManager.GetEffectiveCost(card));
        }
    }
}
