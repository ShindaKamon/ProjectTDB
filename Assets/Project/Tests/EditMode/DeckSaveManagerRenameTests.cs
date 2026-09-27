using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Les decks sauvegardés référencent les cartes par nom : une carte renommée doit être
    /// retrouvée sous son nouveau nom.
    /// </summary>
    public class DeckSaveManagerRenameTests
    {
        [TestCase("Écho de Lyse", "Écho évanescent")]
        [TestCase("Il triche", "Triche")]
        [TestCase("Corde de rappel forcé", "Corde de rappel")]
        public void ResolveCardName_RenamedCard_ReturnsCurrentName(string oldName, string currentName)
        {
            Assert.AreEqual(currentName, DeckSaveManager.ResolveCardName(oldName));
        }

        [Test]
        public void ResolveCardName_UnchangedCard_ReturnsSameName()
        {
            Assert.AreEqual("Coup de colère", DeckSaveManager.ResolveCardName("Coup de colère"));
        }

        [Test]
        public void ResolveCardName_Null_ReturnsNull()
        {
            Assert.IsNull(DeckSaveManager.ResolveCardName(null));
        }

        [TestCase("Soren", "Evan")]
        [TestCase("L'Alpiniste", "Crux")]
        [TestCase("Ace", "Raze")]
        public void MigrateRenamedChampion_OldDecks_AreTakenOverUnderNewName(string oldName, string newName)
        {
            var all = new AllDecksData();
            var old = new ChampionDecksData(oldName);
            old.decks.Add(new DeckData("Mon deck", EmotionType.Anger, EmotionType.None, new System.Collections.Generic.List<string> { "Coup de colère" }));
            all.SetChampionDecks(old);

            var migrated = DeckSaveManager.MigrateRenamedChampion(all, newName);

            Assert.IsNotNull(migrated);
            Assert.AreEqual(newName, migrated.championName);
            Assert.AreEqual("Mon deck", migrated.decks[0].deckName);
            Assert.AreSame(migrated, all.GetChampionDecks(newName));
            Assert.IsNull(all.GetChampionDecks(oldName));
        }

        [Test]
        public void MigrateRenamedChampion_NothingUnderOldName_ReturnsNull()
        {
            Assert.IsNull(DeckSaveManager.MigrateRenamedChampion(new AllDecksData(), "Evan"));
        }
    }
}
