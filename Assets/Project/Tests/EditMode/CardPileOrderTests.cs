using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Ordre d'affichage de la pioche (par coût) et de la défausse (dernière défaussée en premier).
    /// </summary>
    public class CardPileOrderTests
    {
        private readonly List<CardData> _created = new List<CardData>();

        private CardData NewCard(string name, int cost)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.costPA = cost;
            _created.Add(card);
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var card in _created) Object.DestroyImmediate(card);
            _created.Clear();
        }

        [Test]
        public void DeckView_SortedByCostThenName_NotDrawOrder()
        {
            var rage = NewCard("Rage totale", 6);
            var coup = NewCard("Coup de colère", 2);
            var armure = NewCard("Armure de rage", 2);

            var view = CardPileOrder.ForDeckView(new[] { rage, coup, armure });

            CollectionAssert.AreEqual(new[] { armure, coup, rage }, view);
        }

        [Test]
        public void DiscardView_MostRecentFirst()
        {
            var first = NewCard("Première", 1);
            var last = NewCard("Dernière", 3);

            var view = CardPileOrder.ForDiscardView(new[] { first, last });

            CollectionAssert.AreEqual(new[] { last, first }, view);
        }
    }
}
