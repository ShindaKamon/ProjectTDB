using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    public class GameActionValidatorTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(string name = "TestCard")
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

        // ---- CanPlayCard null guards (no MonoBehaviour needed) ----

        [Test]
        public void CanPlayCard_NullPlayer_Fails()
        {
            var result = GameActionValidator.CanPlayCard(null, NewCard());

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Joueur null", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_NullCard_Fails()
        {
            var result = GameActionValidator.CanPlayCard(null, null);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CanTargetUnit_NullCard_Fails()
        {
            var result = GameActionValidator.CanTargetUnit(null, null, null);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Carte null", result.ErrorMessage);
        }

        // ---- ValidateCardData (pure ScriptableObject logic) ----

        [Test]
        public void ValidateCardData_Null_Fails()
        {
            Assert.IsFalse(GameActionValidator.ValidateCardData(null).IsValid);
        }

        [Test]
        public void ValidateCardData_NegativeCostPA_Fails()
        {
            var card = NewCard();
            card.costPA = -1;

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("costPA", result.ErrorMessage);
        }

        [Test]
        public void ValidateCardData_NegativeTargetRange_Fails()
        {
            var card = NewCard();
            card.targetRange = -3;

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("targetRange", result.ErrorMessage);
        }

        [Test]
        public void ValidateCardData_Default_IsValid()
        {
            var card = NewCard();

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }
    }
}
