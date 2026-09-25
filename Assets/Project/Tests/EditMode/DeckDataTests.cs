using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Couleurs d'un deck sauvegardé : noms anglais actuels et anciens noms français.
    /// </summary>
    public class DeckDataTests
    {
        [Test]
        public void Emotions_ReadCurrentAndLegacyNames()
        {
            var deck = new DeckData { emotionType1 = "Colere", emotionType2 = "Fear" };

            Assert.AreEqual(EmotionType.Anger, deck.Emotion1, "Ancien nom français d'une sauvegarde existante");
            Assert.AreEqual(EmotionType.Fear, deck.Emotion2);
        }

        [Test]
        public void Emotions_AreSavedWithEnglishNames()
        {
            var deck = new DeckData { Emotion1 = EmotionType.Joy };

            Assert.AreEqual("Joy", deck.emotionType1);
        }
    }
}
