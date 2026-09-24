using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Tests du filtre/tri du pool de cartes de l'éditeur de deck (CardPoolQuery).
    /// </summary>
    public class CardPoolQueryTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(string name, int costPA, EmotionType emotion,
            CardCategory category = CardCategory.Standard, ChampionData owner = null, string description = "")
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.costPA = costPA;
            card.emotionType = emotion;
            card.category = category;
            card.signatureOwner = owner;
            card.description = description;
            _created.Add(card);
            return card;
        }

        private ChampionData NewChampion()
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            _created.Add(champion);
            return champion;
        }

        private static List<string> Names(List<CardData> cards) => cards.ConvertAll(c => c.cardName);

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
        public void Apply_NoCriteria_SortsByCostThenName()
        {
            var pool = new[]
            {
                NewCard("Brasier", 2, EmotionType.Colere),
                NewCard("Accroche", 2, EmotionType.Peur),
                NewCard("Éclat", 1, EmotionType.Joie),
            };

            var result = new CardPoolQuery().Apply(pool, null);

            Assert.AreEqual(new[] { "Éclat", "Accroche", "Brasier" }, Names(result));
        }

        [Test]
        public void Apply_SignatureOfOtherChampion_IsExcluded()
        {
            var mine = NewChampion();
            var other = NewChampion();
            var pool = new[]
            {
                NewCard("A moi", 1, EmotionType.None, CardCategory.Signature, mine),
                NewCard("A lui", 1, EmotionType.None, CardCategory.Signature, other),
            };

            var result = new CardPoolQuery().Apply(pool, mine);

            Assert.AreEqual(new[] { "A moi" }, Names(result));
        }

        [Test]
        public void Apply_AllEmotionsAvailable_PlusOwnSignatures()
        {
            // Toutes les émotions sont autorisées dans un deck : le pool les propose toutes
            var champion = NewChampion();
            var pool = new[]
            {
                NewCard("Rouge", 1, EmotionType.Colere),
                NewCard("Vert", 1, EmotionType.Peur),
                NewCard("Jaune", 1, EmotionType.Joie),
                NewCard("Sig", 1, EmotionType.None, CardCategory.Signature, champion),
            };

            var result = new CardPoolQuery().Apply(pool, champion);

            CollectionAssert.AreEquivalent(new[] { "Rouge", "Vert", "Jaune", "Sig" }, Names(result));
        }

        [Test]
        public void Apply_DeckColors_KeepOnlyThoseColorsPlusSignatures()
        {
            var champion = NewChampion();
            var pool = new[]
            {
                NewCard("Rouge", 1, EmotionType.Colere),
                NewCard("Vert", 1, EmotionType.Peur),
                NewCard("Jaune", 1, EmotionType.Joie),
                NewCard("Sig", 1, EmotionType.None, CardCategory.Signature, champion),
            };

            var result = new CardPoolQuery().Apply(pool, champion, new List<EmotionType> { EmotionType.Colere, EmotionType.Joie });

            CollectionAssert.AreEquivalent(new[] { "Rouge", "Jaune", "Sig" }, Names(result));
        }

        [Test]
        public void Apply_MultipleEmotions_KeepsAnyOfThem()
        {
            var pool = new[]
            {
                NewCard("Rouge", 1, EmotionType.Colere),
                NewCard("Vert", 1, EmotionType.Peur),
                NewCard("Jaune", 1, EmotionType.Joie),
            };
            var query = new CardPoolQuery();
            query.Emotions.Add(EmotionType.Colere);
            query.Emotions.Add(EmotionType.Joie);

            var result = query.Apply(pool, null);

            CollectionAssert.AreEquivalent(new[] { "Rouge", "Jaune" }, Names(result));
        }

        [Test]
        public void Apply_Category_FiltersOthers()
        {
            var champion = NewChampion();
            var pool = new[]
            {
                NewCard("Std", 1, EmotionType.Colere),
                NewCard("Sig", 1, EmotionType.None, CardCategory.Signature, champion),
            };
            var query = new CardPoolQuery();
            query.Categories.Add(CardCategory.Signature);

            Assert.AreEqual(new[] { "Sig" }, Names(query.Apply(pool, champion)));
        }

        [Test]
        public void Apply_CostRange_IsInclusive()
        {
            var pool = new[]
            {
                NewCard("Zero", 0, EmotionType.Joie),
                NewCard("Un", 1, EmotionType.Joie),
                NewCard("Deux", 2, EmotionType.Joie),
                NewCard("Trois", 3, EmotionType.Joie),
            };
            var query = new CardPoolQuery { MinCost = 1, MaxCost = 2 };

            Assert.AreEqual(new[] { "Un", "Deux" }, Names(query.Apply(pool, null)));
        }

        [Test]
        public void Apply_Search_IgnoresCaseAndAccentsAndLooksInDescription()
        {
            var pool = new[]
            {
                NewCard("Élan de Colère", 1, EmotionType.Colere),
                NewCard("Frisson", 1, EmotionType.Peur, description: "Inflige des dégâts."),
                NewCard("Rire", 1, EmotionType.Joie, description: "Soigne un allié."),
            };

            var byName = new CardPoolQuery { Search = "COLERE" }.Apply(pool, null);
            var byDescription = new CardPoolQuery { Search = "degats" }.Apply(pool, null);

            Assert.AreEqual(new[] { "Élan de Colère" }, Names(byName));
            Assert.AreEqual(new[] { "Frisson" }, Names(byDescription));
        }

        [Test]
        public void Apply_SortDescending_ReversesPrimaryKeyOnly()
        {
            var pool = new[]
            {
                NewCard("Bravade", 1, EmotionType.Colere),
                NewCard("Assaut", 1, EmotionType.Colere),
                NewCard("Cri", 3, EmotionType.Colere),
            };
            var query = new CardPoolQuery { SortKey = CardSortKey.Cost, Descending = true };

            // Coût décroissant, mais départage par nom toujours croissant
            Assert.AreEqual(new[] { "Cri", "Assaut", "Bravade" }, Names(query.Apply(pool, null)));
        }

        [Test]
        public void Apply_SortByEmotion_TieBreaksByCostThenName()
        {
            var pool = new[]
            {
                NewCard("Peur2", 2, EmotionType.Peur),
                NewCard("Colere3", 3, EmotionType.Colere),
                NewCard("Colere1", 1, EmotionType.Colere),
            };
            var query = new CardPoolQuery { SortKey = CardSortKey.Emotion };

            Assert.AreEqual(new[] { "Colere1", "Colere3", "Peur2" }, Names(query.Apply(pool, null)));
        }

        [Test]
        public void ResetFilters_ClearsCriteriaButKeepsSort()
        {
            var query = new CardPoolQuery
            {
                MinCost = 2, MaxCost = 3, Search = "x",
                SortKey = CardSortKey.Name, Descending = true,
            };
            query.Emotions.Add(EmotionType.Joie);
            query.Categories.Add(CardCategory.Standard);

            Assert.IsTrue(query.HasActiveFilters);

            query.ResetFilters();

            Assert.IsFalse(query.HasActiveFilters);
            Assert.AreEqual(0, query.Emotions.Count);
            Assert.AreEqual(0, query.Categories.Count);
            Assert.AreEqual(0, query.MinCost);
            Assert.AreEqual(int.MaxValue, query.MaxCost);
            Assert.AreEqual("", query.Search);
            Assert.AreEqual(CardSortKey.Name, query.SortKey);
            Assert.IsTrue(query.Descending);
        }

        [Test]
        public void HasActiveFilters_SortAloneOrBlankSearch_IsFalse()
        {
            var query = new CardPoolQuery { SortKey = CardSortKey.Emotion, Descending = true, Search = "  " };

            Assert.IsFalse(query.HasActiveFilters);

            query.MaxCost = 3;
            Assert.IsTrue(query.HasActiveFilters);
        }

        [Test]
        public void Apply_NullPoolOrNullCards_AreIgnored()
        {
            var query = new CardPoolQuery();

            Assert.IsEmpty(query.Apply(null, null));
            Assert.AreEqual(new[] { "A" },
                Names(query.Apply(new[] { null, NewCard("A", 1, EmotionType.Joie) }, null)));
        }
    }
}
