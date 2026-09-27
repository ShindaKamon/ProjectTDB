using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Équipe de la coop locale : ajout, champions uniques, 3 joueurs max, ordre conservé.
    /// </summary>
    public class CombatPartyTests
    {
        private readonly List<ChampionData> _champions = new List<ChampionData>();

        private ChampionData NewChampion()
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            _champions.Add(champion);
            return champion;
        }

        [SetUp]
        public void SetUp() => CombatParty.Clear();

        [TearDown]
        public void TearDown()
        {
            CombatParty.Clear();
            foreach (var champion in _champions) Object.DestroyImmediate(champion);
            _champions.Clear();
        }

        [Test]
        public void TryAdd_AddsChampionWithItsDeck()
        {
            var champion = NewChampion();
            var deck = new List<CardData>();

            Assert.IsTrue(CombatParty.TryAdd(champion, deck));
            Assert.AreEqual(1, CombatParty.Count);
            Assert.AreSame(champion, CombatParty.Members[0].Champion);
            Assert.AreSame(deck, CombatParty.Members[0].Deck);
        }

        [Test]
        public void TryAdd_RefusesSameChampionTwice()
        {
            var champion = NewChampion();
            CombatParty.TryAdd(champion, null);

            Assert.IsFalse(CombatParty.TryAdd(champion, null));
            Assert.AreEqual(1, CombatParty.Count);
        }

        [Test]
        public void TryAdd_RefusesNullChampion()
        {
            Assert.IsFalse(CombatParty.TryAdd(null, null));
            Assert.AreEqual(0, CombatParty.Count);
        }

        [Test]
        public void TryAdd_RefusesFourthPlayer()
        {
            for (int i = 0; i < CombatParty.MAX_PLAYERS; i++)
                Assert.IsTrue(CombatParty.TryAdd(NewChampion(), null));

            Assert.IsTrue(CombatParty.IsFull);
            Assert.IsFalse(CombatParty.TryAdd(NewChampion(), null));
            Assert.AreEqual(CombatParty.MAX_PLAYERS, CombatParty.Count);
        }

        [Test]
        public void Members_KeepAddOrder()
        {
            var first = NewChampion();
            var second = NewChampion();
            CombatParty.TryAdd(first, null);
            CombatParty.TryAdd(second, null);

            Assert.AreSame(first, CombatParty.Members[0].Champion);
            Assert.AreSame(second, CombatParty.Members[1].Champion);
        }

        [Test]
        public void Clear_EmptiesPartyAndBackToSolo()
        {
            var champion = NewChampion();
            CombatParty.TryAdd(champion, null);
            CombatParty.IsMultiplayer = true;
            CombatParty.Clear();

            Assert.AreEqual(0, CombatParty.Count);
            Assert.IsFalse(CombatParty.Contains(champion));
            Assert.IsFalse(CombatParty.IsMultiplayer);
        }

        [Test]
        public void TryReplace_ChangesChampionAndDeckInPlace()
        {
            var first = NewChampion();
            var second = NewChampion();
            var replacement = NewChampion();
            var deck = new List<CardData>();
            CombatParty.TryAdd(first, null);
            CombatParty.TryAdd(second, null);

            Assert.IsTrue(CombatParty.TryReplace(0, replacement, deck));
            Assert.AreSame(replacement, CombatParty.Members[0].Champion);
            Assert.AreSame(deck, CombatParty.Members[0].Deck);
            Assert.AreSame(second, CombatParty.Members[1].Champion);
        }

        [Test]
        public void TryReplace_AllowsKeepingOwnChampion()
        {
            var champion = NewChampion();
            CombatParty.TryAdd(champion, null);

            Assert.IsTrue(CombatParty.TryReplace(0, champion, new List<CardData>()));
        }

        [Test]
        public void TryReplace_RefusesChampionOfAnotherPlayer()
        {
            var first = NewChampion();
            var second = NewChampion();
            CombatParty.TryAdd(first, null);
            CombatParty.TryAdd(second, null);

            Assert.IsFalse(CombatParty.TryReplace(0, second, null));
            Assert.AreSame(first, CombatParty.Members[0].Champion);
        }

        [Test]
        public void TryReplace_RefusesMissingPlace()
        {
            Assert.IsFalse(CombatParty.TryReplace(0, NewChampion(), null));
        }

        [Test]
        public void RemoveAt_ShiftsNextPlayers()
        {
            var first = NewChampion();
            var second = NewChampion();
            CombatParty.TryAdd(first, null);
            CombatParty.TryAdd(second, null);

            CombatParty.RemoveAt(0);

            Assert.AreEqual(1, CombatParty.Count);
            Assert.AreSame(second, CombatParty.Members[0].Champion);
            Assert.AreEqual(-1, CombatParty.IndexOf(first));
        }
    }
}
