using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Géométrie de la grille en 4 directions (Manhattan), valable partout (écho du Miroir fraternel compris).
    /// </summary>
    public class GridGeometryTests
    {
        [Test]
        public void Distance_OrthogonalNeighbour_IsOne_DiagonalIsTwo()
        {
            Assert.AreEqual(1, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(5, 6)));
            Assert.AreEqual(2, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(6, 6)));
        }

        [Test]
        public void Distance_KnightMove_IsThree()
        {
            Assert.AreEqual(3, GridGeometry.Distance(new Vector2Int(5, 5), new Vector2Int(6, 7)));
        }

        [Test]
        public void Distance_SamePosition_IsZero()
        {
            Assert.AreEqual(0, GridGeometry.Distance(new Vector2Int(3, 3), new Vector2Int(3, 3)));
        }

        [Test]
        public void Directions4_ContainsFourDistinctNeighbours()
        {
            Assert.AreEqual(4, GridGeometry.Directions4.Length);
            CollectionAssert.AllItemsAreUnique(GridGeometry.Directions4);
            foreach (var d in GridGeometry.Directions4)
                Assert.IsTrue(GridGeometry.AreAdjacent(Vector2Int.zero, d));
            Assert.IsFalse(GridGeometry.AreAdjacent(Vector2Int.zero, new Vector2Int(1, 1)));
        }

        [Test]
        public void TryGetLine_RowAndColumn_AreLines()
        {
            Assert.IsTrue(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(5, 2), out var step, out int length));
            Assert.AreEqual(new Vector2Int(1, 0), step);
            Assert.AreEqual(3, length);

            Assert.IsTrue(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(2, 0), out step, out length));
            Assert.AreEqual(new Vector2Int(0, -1), step);
            Assert.AreEqual(2, length);
        }

        [Test]
        public void TryGetLine_DiagonalOrSamePosition_IsNotALine()
        {
            Assert.IsFalse(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(4, 4), out _, out _));
            Assert.IsFalse(GridGeometry.TryGetLine(new Vector2Int(2, 2), new Vector2Int(2, 2), out _, out _));
        }

        [Test]
        public void SnapDirection_KeepsDominantAxis()
        {
            Assert.AreEqual(new Vector2Int(1, 0), GridGeometry.SnapDirection(new Vector2Int(0, 0), new Vector2Int(3, 2)));
            Assert.AreEqual(new Vector2Int(0, -1), GridGeometry.SnapDirection(new Vector2Int(0, 0), new Vector2Int(1, -4)));
            Assert.AreEqual(Vector2Int.zero, GridGeometry.SnapDirection(new Vector2Int(1, 1), new Vector2Int(1, 1)));
        }

        [Test]
        public void StraightPath_IsAStaircaseOfNeighbours_EndingOnTarget()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(4, 2);
            var path = GridGeometry.StraightPath(from, to);

            Assert.AreEqual(GridGeometry.Distance(from, to), path.Count, "une case par pas, sans diagonale");
            Assert.IsTrue(GridGeometry.AreAdjacent(from, path[0]));
            for (int i = 1; i < path.Count; i++) Assert.IsTrue(GridGeometry.AreAdjacent(path[i - 1], path[i]));
            Assert.AreEqual(to, path[path.Count - 1]);
            // Au plus près de la droite y = x / 2 : x d'abord, puis l'escalier
            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(3, 1), new Vector2Int(3, 2), new Vector2Int(4, 2) }, path);
        }

        [Test]
        public void StraightPath_AlignedCells_FollowsTheLine_AndSameCellIsEmpty()
        {
            CollectionAssert.AreEqual(new[] { new Vector2Int(2, 4), new Vector2Int(2, 3) },
                GridGeometry.StraightPath(new Vector2Int(2, 5), new Vector2Int(2, 3)));
            Assert.IsEmpty(GridGeometry.StraightPath(new Vector2Int(2, 5), new Vector2Int(2, 5)));
        }

        [Test]
        public void LineOfSightCells_StraightLine_AreTheCellsBetween()
        {
            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 0), new Vector2Int(2, 0) },
                GridGeometry.LineOfSightCells(new Vector2Int(0, 0), new Vector2Int(3, 0)));
            Assert.IsEmpty(GridGeometry.LineOfSightCells(new Vector2Int(0, 0), new Vector2Int(0, 1)), "au contact : rien entre les deux");
        }

        [Test]
        public void LineOfSightCells_ThroughACorner_SkipsBothSideCells()
        {
            // La droite (0,0) → (3,1) passe exactement par le coin entre (1,0), (2,0), (1,1) et (2,1)
            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 0), new Vector2Int(2, 1) },
                GridGeometry.LineOfSightCells(new Vector2Int(0, 0), new Vector2Int(3, 1)));
            // Diagonale parfaite : seules les cases sur la diagonale
            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 1) },
                GridGeometry.LineOfSightCells(new Vector2Int(0, 0), new Vector2Int(2, 2)));
        }

        [Test]
        public void IsLineClear_BlockedByAnythingOnTheLine_NotBySideCells()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(4, 0);
            Assert.IsFalse(GridGeometry.IsLineClear(from, to, c => c == new Vector2Int(2, 0)), "obstacle sur la ligne");
            Assert.IsTrue(GridGeometry.IsLineClear(from, to, c => c == new Vector2Int(2, 1)), "obstacle à côté");
            Assert.IsTrue(GridGeometry.IsLineClear(from, to, c => c == to || c == from), "lanceur et cible ne bloquent pas");
        }
    }
}
