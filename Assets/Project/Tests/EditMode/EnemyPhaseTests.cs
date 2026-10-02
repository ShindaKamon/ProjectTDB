using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Phases d'un boss (EnemyData.nextPhases) : à 0 PV il ne meurt pas, il repart avec la barre pleine et le
    /// pattern de la phase suivante.
    /// </summary>
    public class EnemyPhaseTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<BossPhaseChangedEvent> _events = new List<BossPhaseChangedEvent>();

        private void OnPhaseChanged(BossPhaseChangedEvent e) => _events.Add(e);

        [SetUp]
        public void SetUp() => EventBus.Subscribe<BossPhaseChangedEvent>(OnPhaseChanged);

        [TearDown]
        public void TearDown()
        {
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnPhaseChanged);
            foreach (Object o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
            _events.Clear();
        }

        private CardData NewCard(string name)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.cardName = name;
            return card;
        }

        private static void Set(object target, System.Type type, string field, object value) =>
            type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

        // Boss à 3 barres (100 / 120 / 150), sans passer par InitializeEnemy (qui demande la grille)
        private Enemy NewBoss(CardData phase1Card, CardData phase2Card, CardData phase3Card)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(data);
            data.maxHealth = 100;
            data.combatDeck = new List<CardData> { phase1Card };
            data.nextPhases = new List<EnemyData.BossPhase>
            {
                new EnemyData.BossPhase { maxHealth = 120, combatDeck = new List<CardData> { phase2Card } },
                new EnemyData.BossPhase { maxHealth = 150, combatDeck = new List<CardData> { phase3Card } },
            };

            var go = new GameObject("Boss");
            _created.Add(go);
            Enemy boss = go.AddComponent<Enemy>();
            Set(boss, typeof(Enemy), "_enemyData", data);
            Set(boss, typeof(Enemy), "_combatDeck", new List<CardData>(data.combatDeck));
            Set(boss, typeof(Unit), "_maxHealth", 100);
            Set(boss, typeof(Unit), "_health", 100);
            return boss;
        }

        [Test]
        public void AtZeroHealth_StartsNextPhase_WithFullBarAndItsPattern()
        {
            CardData second = NewCard("Agrippe du Lit");
            Enemy boss = NewBoss(NewCard("Agrippe"), second, NewCard("Agrippe au contact"));
            Assert.AreEqual(3, boss.PhaseCount);

            boss.TakeDamage(500);

            Assert.AreEqual(1, boss.Phase);
            Assert.AreEqual(120, boss.GetMaxHealth());
            Assert.AreEqual(120, boss.GetHealth(), "barre pleine : il revient avec toute sa vie");
            Assert.AreSame(second, boss.GetNextCard(), "pattern de la phase 2, depuis le début");
            Assert.AreEqual(1, _events.Count);
            Assert.AreEqual(1, _events[0].Phase);
            Assert.AreEqual(3, _events[0].PhaseCount);
        }

        [Test]
        public void LoseHealth_GoesThroughPhases_LikeDamage()
        {
            CardData third = NewCard("Agrippe au contact");
            Enemy boss = NewBoss(NewCard("Agrippe"), NewCard("Agrippe du Lit"), third);

            boss.LoseHealth(30);
            Assert.AreEqual(70, boss.GetHealth());
            Assert.AreEqual(0, boss.Phase);

            boss.LoseHealth(70);
            boss.LoseHealth(120);

            Assert.AreEqual(2, boss.Phase, "dernière phase");
            Assert.AreEqual(150, boss.GetHealth());
            Assert.AreSame(third, boss.GetNextCard());
        }
    }
}
