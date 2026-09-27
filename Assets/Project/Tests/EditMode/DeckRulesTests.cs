using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Règles de construction d'un deck : exemplaires max, Signatures obligatoires et réservées.
    /// </summary>
    public class DeckRulesTests
    {
        private readonly List<Object> _created = new List<Object>();

        private ChampionData NewChampion()
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            _created.Add(champion);
            return champion;
        }

        private CardData NewCard(string name, CardCategory category = CardCategory.Standard, ChampionData owner = null,
            EmotionType emotion = EmotionType.Anger)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.category = category;
            card.signatureOwner = owner;
            card.emotionType = emotion;
            _created.Add(card);
            return card;
        }

        private static List<CardData> Copies(CardData card, int n)
        {
            var list = new List<CardData>();
            for (int i = 0; i < n; i++) list.Add(card);
            return list;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void CanAdd_UpTo4CopiesOfAStandardCard()
        {
            var champion = NewChampion();
            var card = NewCard("Coup de colère");

            Assert.IsTrue(DeckRules.CanAddCard(Copies(card, 3), card, champion).IsValid);

            var result = DeckRules.CanAddCard(Copies(card, 4), card, champion);
            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("4 exemplaires maximum", result.ErrorMessage);
        }

        [Test]
        public void CanAdd_OnlyOneCopyOfASignature()
        {
            var champion = NewChampion();
            var signature = NewCard("Tapis", CardCategory.Signature, champion);

            Assert.IsTrue(DeckRules.CanAddCard(new List<CardData>(), signature, champion).IsValid);
            Assert.IsFalse(DeckRules.CanAddCard(Copies(signature, 1), signature, champion).IsValid);
        }

        [Test]
        public void CanAdd_AnotherChampionsSignature_Fails()
        {
            var me = NewChampion();
            var other = NewChampion();
            var theirs = NewCard("Triche", CardCategory.Signature, other);

            Assert.IsFalse(DeckRules.CanAddCard(new List<CardData>(), theirs, me).IsValid);
        }

        [Test]
        public void CanAdd_AnyEmotion_IsAllowed()
        {
            var champion = NewChampion();
            var deck = new List<CardData> { NewCard("Rouge", emotion: EmotionType.Anger) };

            Assert.IsTrue(DeckRules.CanAddCard(deck, NewCard("Vert", emotion: EmotionType.Fear), champion).IsValid);
            Assert.IsTrue(DeckRules.CanAddCard(deck, NewCard("Jaune", emotion: EmotionType.Joy), champion).IsValid);
        }

        [Test]
        public void CanAdd_StandardSlotsFull_Fails()
        {
            var champion = NewChampion();
            var deck = new List<CardData>();
            for (int i = 0; i < DeckData.STANDARD_SLOTS; i++) deck.Add(NewCard("Carte" + i));

            Assert.IsFalse(DeckRules.CanAddCard(deck, NewCard("Une de trop"), champion).IsValid);
        }

        [Test]
        public void CanRemove_OwnSignature_Fails_StandardSucceeds()
        {
            var champion = NewChampion();

            Assert.IsFalse(DeckRules.CanRemoveCard(NewCard("Tapis", CardCategory.Signature, champion), champion).IsValid);
            Assert.IsTrue(DeckRules.CanRemoveCard(NewCard("Coup de colère"), champion).IsValid);
        }

        [Test]
        public void DeckColors_ChosenColors_OrFromCardsWhenNoneChosen()
        {
            var chosen = new DeckData("Rouge-jaune", EmotionType.Joy, EmotionType.Anger, new List<string>());
            var baseDeck = new DeckData("Base", EmotionType.None, EmotionType.None, new List<string>());
            var cards = new[] { NewCard("A", emotion: EmotionType.Fear), NewCard("B", emotion: EmotionType.Anger),
                                NewCard("Sig", CardCategory.Signature, NewChampion(), EmotionType.None) };

            CollectionAssert.AreEqual(new[] { EmotionType.Anger, EmotionType.Joy }, DeckRules.DeckColors(chosen, null));
            CollectionAssert.AreEquivalent(new[] { EmotionType.Anger, EmotionType.Fear }, DeckRules.DeckColors(baseDeck, cards));
        }

        [Test]
        public void CanAdd_OffColorCard_Fails_SignatureAlwaysAllowed()
        {
            var champion = NewChampion();
            var colors = new List<EmotionType> { EmotionType.Anger };

            Assert.IsTrue(DeckRules.CanAddCard(new List<CardData>(), NewCard("Rouge", emotion: EmotionType.Anger), champion, colors).IsValid);
            Assert.IsFalse(DeckRules.CanAddCard(new List<CardData>(), NewCard("Vert", emotion: EmotionType.Fear), champion, colors).IsValid);
            Assert.IsTrue(DeckRules.CanAddCard(new List<CardData>(), NewCard("Sig", CardCategory.Signature, champion, EmotionType.None), champion, colors).IsValid);
        }

        [Test]
        public void EnforceColors_RemovesOffColorCardsOnly()
        {
            var champion = NewChampion();
            var deck = new List<CardData>
            {
                NewCard("Rouge", emotion: EmotionType.Anger),
                NewCard("Vert", emotion: EmotionType.Fear),
                NewCard("Sig", CardCategory.Signature, champion, EmotionType.None),
            };

            int removed = DeckRules.EnforceColors(deck, new List<EmotionType> { EmotionType.Anger });

            Assert.AreEqual(1, removed);
            CollectionAssert.AreEquivalent(new[] { "Rouge", "Sig" }, deck.ConvertAll(c => c.cardName));
        }

        [Test]
        public void EnforceCopyLimits_TrimsExtraCopies()
        {
            var champion = NewChampion();
            var roar = NewCard("Rugissement destructeur");
            var signature = NewCard("Tapis", CardCategory.Signature, champion);
            var deck = Copies(roar, 9);
            deck.AddRange(Copies(signature, 2));

            int removed = DeckRules.EnforceCopyLimits(deck);

            Assert.AreEqual(6, removed); // 5 Rugissement + 1 Signature en trop
            Assert.AreEqual(4, deck.FindAll(c => c == roar).Count);
            Assert.AreEqual(1, deck.FindAll(c => c == signature).Count);
        }

        [Test]
        public void MissingSignatures_ListsOnlyOwnSignaturesAbsentFromDeck()
        {
            var me = NewChampion();
            var other = NewChampion();
            var sig1 = NewCard("Triche", CardCategory.Signature, me);
            var sig2 = NewCard("Tapis", CardCategory.Signature, me);
            var theirs = NewCard("Piolet", CardCategory.Signature, other);
            var standard = NewCard("Coup de colère");

            var missing = DeckRules.MissingSignatures(new List<CardData> { sig1, standard }, me,
                new[] { sig1, sig2, theirs, standard });

            Assert.AreEqual(1, missing.Count);
            Assert.AreSame(sig2, missing[0]);
        }

        [Test]
        public void AvailableEmotions_AreTheThreeLaunchEmotions()
        {
            CollectionAssert.AreEqual(new[] { EmotionType.Anger, EmotionType.Fear, EmotionType.Joy },
                DeckRules.AvailableEmotions);
        }
    }
}
