using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    public class ExplorationPathfinderTests
    {
        private static readonly Vector2Int Size = new Vector2Int(5, 5);

        [Test]
        public void Path_ExcludesStartAndEndsOnGoal()
        {
            var goal = new Vector2Int(3, 0);
            var path = ExplorationPathfinder.FindPath(Vector2Int.zero, Size, c => false, c => c == goal);

            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(goal, path[path.Count - 1]);
            Assert.AreEqual(new Vector2Int(1, 0), path[0]);
        }

        [Test]
        public void Path_StartIsGoal_IsEmpty()
        {
            var path = ExplorationPathfinder.FindPath(Vector2Int.one, Size, c => false, c => c == Vector2Int.one);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void Path_GoesAroundBlockedCells()
        {
            // Mur vertical en x = 1 sauf la case (1, 4)
            var path = ExplorationPathfinder.FindPath(Vector2Int.zero, Size,
                c => c.x == 1 && c.y < 4, c => c == new Vector2Int(2, 0));

            Assert.AreEqual(new Vector2Int(2, 0), path[path.Count - 1]);
            Assert.IsFalse(path.Exists(c => c.x == 1 && c.y < 4));
            Assert.AreEqual(10, path.Count); // 4 en haut, 2 de côté, 4 en bas
        }

        [Test]
        public void Path_Unreachable_ReturnsNull()
        {
            var path = ExplorationPathfinder.FindPath(Vector2Int.zero, Size, c => c.x == 1, c => c == new Vector2Int(3, 3));
            Assert.IsNull(path);
        }

        [Test]
        public void Path_ApproachesMonsterFromAnAdjacentCell()
        {
            var monster = new Vector2Int(2, 2);
            var path = ExplorationPathfinder.FindPath(Vector2Int.zero, Size,
                c => c == monster, c => GridGeometry.AreAdjacent(c, monster));

            Assert.AreEqual(3, path.Count);
            Assert.IsTrue(GridGeometry.AreAdjacent(path[path.Count - 1], monster));
        }
    }
}
