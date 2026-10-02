using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Lancer annoncé d'un monstre (Enemy.AnnounceThrow) : zones annoncées et publiées, reprises une fois
    /// pour être résolues, annulées par Sidération (CancelNextCard).
    /// </summary>
    public class EnemyThrowTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<ThrowZonesChangedEvent> _events = new List<ThrowZonesChangedEvent>();

        private void OnThrowZonesChanged(ThrowZonesChangedEvent e) => _events.Add(e);

        [SetUp]
        public void SetUp() => EventBus.Subscribe<ThrowZonesChangedEvent>(OnThrowZonesChanged);

        [TearDown]
        public void TearDown()
        {
            EventBus.Unsubscribe<ThrowZonesChangedEvent>(OnThrowZonesChanged);
            foreach (Object o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
            _events.Clear();
        }

        private Enemy NewEnemy()
        {
            var go = new GameObject("Monstre");
            _created.Add(go);
            Enemy enemy = go.AddComponent<Enemy>();
            enemy.SetRandomSeed(7);
            return enemy;
        }

        private CardData NewThrow(int zones, CardAreaEffect area = CardAreaEffect.None, int radius = 0)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.targetType = CardTargetType.EnemyOrTile;
            card.affectedTarget = CardAffectedTarget.Enemies;
            card.areaEffect = area;
            card.aoeRadius = radius;
            card.telegraphedZoneCount = zones;
            return card;
        }

        private static List<Vector2Int> Board()
        {
            var cells = new List<Vector2Int>();
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    cells.Add(new Vector2Int(x, y));
            return cells;
        }

        [Test]
        public void Announce_PublishesCoveredCells_ThenTakeClearsThem()
        {
            Enemy enemy = NewEnemy();
            CardData card = NewThrow(3);
            var champion = new Vector2Int(4, 4);

            enemy.AnnounceThrow(card, Board(), new List<Vector2Int> { champion });

            Assert.IsTrue(enemy.HasPendingThrow);
            Assert.AreEqual(3, _events[0].Cells.Count);
            CollectionAssert.Contains(_events[0].Cells, champion);

            var epicenters = new List<Vector2Int>();
            Assert.AreSame(card, enemy.TakePendingThrow(epicenters));
            Assert.AreEqual(3, epicenters.Count);
            Assert.IsFalse(enemy.HasPendingThrow);
            Assert.AreEqual(0, _events[1].Cells.Count, "les zones disparaissent du sol");
            Assert.IsNull(enemy.TakePendingThrow(epicenters), "un lancer ne tombe qu'une fois");
        }

        [Test]
        public void Announce_CircleZone_CoversTheWholeCircle()
        {
            Enemy enemy = NewEnemy();

            enemy.AnnounceThrow(NewThrow(1, CardAreaEffect.Circle, 1), Board(), new List<Vector2Int> { new Vector2Int(4, 4) });

            CollectionAssert.AreEquivalent(new[]
            {
                new Vector2Int(4, 4), new Vector2Int(3, 4), new Vector2Int(5, 4), new Vector2Int(4, 3), new Vector2Int(4, 5)
            }, _events[0].Cells);
        }

        [Test]
        public void CancelNextCard_AlsoCancelsTheAnnouncedThrow()
        {
            Enemy enemy = NewEnemy();
            enemy.AnnounceThrow(NewThrow(2), Board(), new List<Vector2Int>());

            enemy.CancelNextCard();

            Assert.IsFalse(enemy.HasPendingThrow);
            Assert.AreEqual(0, _events[_events.Count - 1].Cells.Count);
        }

        [Test]
        public void Announce_ZoneCountOverride_ForDebris()
        {
            // Bric-à-brac : 1 zone de base + 2 tas de débris ramassés = 3 zones
            Enemy enemy = NewEnemy();
            enemy.AnnounceThrow(NewThrow(1), Board(), new List<Vector2Int>(), 3);

            var epicenters = new List<Vector2Int>();
            enemy.TakePendingThrow(epicenters);
            Assert.AreEqual(3, epicenters.Count);
        }

        [Test]
        public void AnimatedToy_EverySecondThrow_FromTheSecond_NeverOnAChampion()
        {
            var soldier = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(soldier);
            CardData rain = NewThrow(4);
            rain.animatedToy = soldier;
            rain.animatedToyEveryNthThrow = 2;
            var champions = new List<Vector2Int> { new Vector2Int(2, 2) };
            Enemy enemy = NewEnemy();

            enemy.AnnounceThrow(rain, Board(), champions);
            Assert.IsNull(enemy.PendingToy, "1re pluie : pas de soldat");

            enemy.TakePendingThrow(new List<Vector2Int>());
            enemy.AnnounceThrow(rain, Board(), champions);
            Assert.AreSame(soldier, enemy.PendingToy, "2e pluie : un soldat");
            Assert.AreNotEqual(new Vector2Int(2, 2), enemy.PendingToyCell);

            enemy.CancelNextCard();
            Assert.IsNull(enemy.PendingToy, "Sidération annule aussi le soldat");
        }
    }
}
