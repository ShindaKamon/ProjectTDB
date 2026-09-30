using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Salon réseau : joueurs, choix de champions uniques, départ, et transport par le réseau.
    /// </summary>
    public class LobbyStateTests
    {
        [Test]
        public void Pick_ChampionTakenByAnotherPlayer_Refused()
        {
            var lobby = new LobbyState();
            lobby.AddPlayer(0);
            lobby.AddPlayer(1);

            Assert.IsTrue(lobby.SetPick(0, "Evan", new List<string> { "Coup de colère" }));
            Assert.IsFalse(lobby.SetPick(1, "Evan", null), "Evan est déjà pris");
            Assert.IsTrue(lobby.SetPick(0, "Evan", null), "on peut reconfirmer son propre champion");
        }

        [Test]
        public void CanStart_TwoPlayersWhoPicked()
        {
            var lobby = new LobbyState();
            lobby.AddPlayer(0);
            lobby.SetPick(0, "Evan", null);
            Assert.IsFalse(lobby.CanStart, "seul");

            lobby.AddPlayer(1);
            Assert.IsFalse(lobby.CanStart, "le 2e joueur n'a pas choisi");

            lobby.SetPick(1, "Crux", null);
            Assert.IsTrue(lobby.CanStart);
        }

        [Test]
        public void AddPlayer_LimitedToThree()
        {
            var lobby = new LobbyState();
            for (ulong id = 0; id < 3; id++) Assert.IsTrue(lobby.AddPlayer(id));
            Assert.IsFalse(lobby.AddPlayer(3));
        }

        [Test]
        public void Serialize_RoundTrip_KeepsOrderPicksAndDecks()
        {
            var lobby = new LobbyState();
            lobby.AddPlayer(0);
            lobby.AddPlayer(7);
            lobby.SetPick(7, "Raze", new List<string> { "Tapis", "Triche", "Coup de colère" });
            lobby.Seed = 123456789;

            LobbyState copy = LobbyState.Deserialize(lobby.Serialize());

            Assert.AreEqual(123456789, copy.Seed, "graine du mélange des decks");
            Assert.AreEqual(2, copy.Count);
            Assert.AreEqual(0ul, copy.Members[0].ClientId);
            Assert.IsFalse(copy.Members[0].HasPicked);
            Assert.AreEqual(7ul, copy.Members[1].ClientId);
            Assert.AreEqual("Raze", copy.Members[1].ChampionName);
            CollectionAssert.AreEqual(new[] { "Tapis", "Triche", "Coup de colère" }, copy.Members[1].DeckCardNames);
        }
    }
}
