using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Géométrie de la grille en 8 directions : une diagonale vaut 1 case.
    /// </summary>
    public class GridGeometryTests
    {
        [Test]
        public void Distance_OrthogonalAndDiagonalNeighbours_AreOne()
        {
            Assert.AreEqual(1, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(5, 6)));
            Assert.AreEqual(1, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(6, 6)));
        }

        [Test]
        public void Distance_KnightMove_IsTwo()
        {
            Assert.AreEqual(2, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(6, 7)));
        }

        [Test]
        public void Distance_SamePosition_IsZero()
        {
            Assert.AreEqual(0, GridGeometry.Distance(new Vector2Int(3, 3), new Vector2Int(3, 3)));
        }

        [Test]
        public void Directions8_ContainsEightDistinctNeighbours()
        {
            Assert.AreEqual(8, GridGeometry.Directions8.Length);
            CollectionAssert.AllItemsAreUnique(GridGeometry.Directions8);
            foreach (var d in GridGeometry.Directions8)
                Assert.IsTrue(GridGeometry.AreAdjacent(Vector2Int.zero, d));
        }

        [Test]
        public void TryGetLine_RowColumnAndDiagonal_AreLines()
        {
            Assert.IsTrue(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(5, 2), out var step, out int length));
            Assert.AreEqual(new Vector2Int(1, 0), step);
            Assert.AreEqual(3, length);

            Assert.IsTrue(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(0, 4), out step, out length));
            Assert.AreEqual(new Vector2Int(-1, 1), step);
            Assert.AreEqual(2, length);
        }

        [Test]
        public void TryGetLine_KnightMoveOrSamePosition_IsNotALine()
        {
            Assert.IsFalse(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(3, 4), out _, out _));
            Assert.IsFalse(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(2, 2), out _, out _));
        }

        [Test]
        public void SnapDirection_RoundsToNearest45Degrees()
        {
            Assert.AreEqual(new Vector2Int(1, 0), GridGeometry.SnapDirection(new Vector2Int(0, 0), new Vector2Int(3, 1)));
            Assert.AreEqual(new Vector2Int(1, 1), GridGeometry.SnapDirection(new Vector2Int(0, 0), new Vector2Int(3, 2)));
            Assert.AreEqual(new Vector2Int(0, -1), GridGeometry.SnapDirection(new Vector2Int(0, 0), new Vector2Int(1, -4)));
            Assert.AreEqual(Vector2Int.zero, GridGeometry.SnapDirection(new Vector2Int(1, 1), new Vector2Int(1, 1)));
        }
    }
}
