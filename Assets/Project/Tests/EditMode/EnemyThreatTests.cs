using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Zone de menace d'un monstre (EnemyThreat) : cases que sa prochaine carte peut toucher depuis ses cases atteignables.
    /// </summary>
    public class EnemyThreatTests
    {
        private static List<Vector2Int> Board(int size)
        {
            var cells = new List<Vector2Int>();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++) cells.Add(new Vector2Int(x, y));
            return cells;
        }

        [Test]
        public void Cells_ContactCard_CoversNeighboursOfEveryReachableCell()
        {
            var origins = new[] { new Vector2Int(2, 2), new Vector2Int(3, 2) };
            var threat = EnemyThreat.Cells(origins, 1, Board(6), (a, b) => true);

            Assert.IsTrue(threat.Contains(new Vector2Int(4, 2)), "voisine de la case atteignable");
            Assert.IsTrue(threat.Contains(new Vector2Int(2, 3)));
            Assert.IsFalse(threat.Contains(new Vector2Int(5, 2)), "à 2 cases de toute origine");
            Assert.AreEqual(8, threat.Count, "voisins de (2,2) et (3,2), chacune comprise (voisine de l'autre)");
        }

        [Test]
        public void Cells_RespectLineOfSight_AndNoRangeMeansNoThreat()
        {
            var origin = new[] { new Vector2Int(0, 0) };
            var blocked = new Vector2Int(3, 0);
            var threat = EnemyThreat.Cells(origin, 3, Board(5), (a, b) => b != blocked);

            Assert.IsFalse(threat.Contains(blocked));
            Assert.IsTrue(threat.Contains(new Vector2Int(2, 0)));
            Assert.IsEmpty(EnemyThreat.Cells(origin, 0, Board(5), (a, b) => true));
        }
    }
}
