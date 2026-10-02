using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Zones d'un lancer annoncé du boss : une sur chaque champion, puis au hasard, sans chevauchement,
    /// et le même tirage pour la même graine (réseau).
    /// </summary>
    public class ThrowZonePickerTests
    {
        private static List<Vector2Int> Board(int width, int height)
        {
            var cells = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cells.Add(new Vector2Int(x, y));
            return cells;
        }

        [Test]
        public void Pick_PutsAZoneOnEveryChampion()
        {
            var champions = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(7, 3) };

            List<Vector2Int> zones = ThrowZonePicker.Pick(new System.Random(1), Board(10, 10), champions, 5, 0);

            Assert.AreEqual(5, zones.Count);
            CollectionAssert.Contains(zones, new Vector2Int(1, 1));
            CollectionAssert.Contains(zones, new Vector2Int(7, 3));
        }

        [Test]
        public void Pick_OneTileZones_AreDistinct()
        {
            List<Vector2Int> zones = ThrowZonePicker.Pick(new System.Random(2), Board(10, 10), new List<Vector2Int>(), 20, 0);

            Assert.AreEqual(20, zones.Count);
            CollectionAssert.AllItemsAreUnique(zones);
        }

        [Test]
        public void Pick_CircleZones_NeverOverlap()
        {
            List<Vector2Int> zones = ThrowZonePicker.Pick(new System.Random(3), Board(10, 10), new List<Vector2Int>(), 6, 1);

            for (int i = 0; i < zones.Count; i++)
                for (int j = i + 1; j < zones.Count; j++)
                    Assert.Greater(GridGeometry.Distance(zones[i], zones[j]), 2, $"{zones[i]} et {zones[j]} se chevauchent");
        }

        [Test]
        public void Pick_ChampionsTooClose_OnlyOneCircleCoversThem()
        {
            // Deux champions côte à côte : la 2e zone en cercle de 1 chevaucherait la 1re, elle va ailleurs
            var champions = new List<Vector2Int> { new Vector2Int(4, 4), new Vector2Int(4, 5) };

            List<Vector2Int> zones = ThrowZonePicker.Pick(new System.Random(4), Board(10, 10), champions, 2, 1);

            Assert.AreEqual(2, zones.Count);
            Assert.IsTrue(zones.Contains(new Vector2Int(4, 4)) ^ zones.Contains(new Vector2Int(4, 5)));
        }

        [Test]
        public void Pick_NotEnoughRoom_ReturnsFewerZones()
        {
            List<Vector2Int> zones = ThrowZonePicker.Pick(new System.Random(5), Board(3, 3), new List<Vector2Int>(), 10, 0);

            Assert.AreEqual(9, zones.Count);
        }

        [Test]
        public void Pick_SameSeed_SameZones()
        {
            var champions = new List<Vector2Int> { new Vector2Int(2, 2) };

            List<Vector2Int> a = ThrowZonePicker.Pick(new System.Random(42), Board(10, 10), champions, 5, 0);
            List<Vector2Int> b = ThrowZonePicker.Pick(new System.Random(42), Board(10, 10), champions, 5, 0);

            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void PickToyIndex_NeverOnAChampion()
        {
            var champions = new List<Vector2Int> { new Vector2Int(2, 2), new Vector2Int(5, 5) };
            var zones = new List<Vector2Int> { new Vector2Int(2, 2), new Vector2Int(5, 5), new Vector2Int(8, 1) };

            for (int seed = 0; seed < 20; seed++)
                Assert.AreEqual(2, ThrowZonePicker.PickToyIndex(new System.Random(seed), zones, champions), "seule zone aléatoire");

            Assert.AreEqual(-1, ThrowZonePicker.PickToyIndex(new System.Random(1), zones.GetRange(0, 2), champions), "que des zones de champions");
        }
    }
}
