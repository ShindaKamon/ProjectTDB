using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Phase de placement : pose initiale, déplacement sur une case libre, échange, cases interdites.
    /// </summary>
    public class PlacementBoardTests
    {
        private static readonly Vector2Int A = new Vector2Int(0, 0);
        private static readonly Vector2Int B = new Vector2Int(2, 0);
        private static readonly Vector2Int C = new Vector2Int(4, 0);

        private PlacementBoard<string> NewBoard(params string[] units)
        {
            var board = new PlacementBoard<string>(new[] { A, B, C });
            board.PlaceInOrder(units);
            return board;
        }

        [Test]
        public void PlaceInOrder_UsesStartCellsInOrder()
        {
            var board = NewBoard("Crux", "Raze");

            Assert.AreEqual("Crux", board.UnitAt(A));
            Assert.AreEqual("Raze", board.UnitAt(B));
            Assert.IsNull(board.UnitAt(C));
        }

        [Test]
        public void PlaceInOrder_IgnoresUnitsBeyondCells()
        {
            var board = new PlacementBoard<string>(new[] { A });

            Assert.AreEqual(1, board.PlaceInOrder(new List<string> { "Crux", "Raze" }));
            Assert.IsFalse(board.TryGetPosition("Raze", out _));
        }

        [Test]
        public void TryMove_ToFreeStartCell_Moves()
        {
            var board = NewBoard("Crux", "Raze");

            Assert.IsTrue(board.TryMove("Crux", C, out string swapped));
            Assert.IsNull(swapped);
            Assert.AreEqual("Crux", board.UnitAt(C));
            Assert.IsNull(board.UnitAt(A));
        }

        [Test]
        public void TryMove_ToOccupiedStartCell_Swaps()
        {
            var board = NewBoard("Crux", "Raze");

            Assert.IsTrue(board.TryMove("Crux", B, out string swapped));
            Assert.AreEqual("Raze", swapped);
            Assert.AreEqual("Crux", board.UnitAt(B));
            Assert.AreEqual("Raze", board.UnitAt(A));
        }

        [Test]
        public void TryMove_OutsideStartCells_IsRefused()
        {
            var board = NewBoard("Crux");

            Assert.IsFalse(board.TryMove("Crux", new Vector2Int(5, 5), out _));
            Assert.AreEqual("Crux", board.UnitAt(A));
        }

        [Test]
        public void TryMove_UnplacedUnit_IsRefused()
        {
            var board = NewBoard("Crux");

            Assert.IsFalse(board.TryMove("Evan", C, out _));
        }

        [Test]
        public void TryMove_ToOwnCell_IsAccepted()
        {
            var board = NewBoard("Crux");

            Assert.IsTrue(board.TryMove("Crux", A, out string swapped));
            Assert.IsNull(swapped);
        }
    }
}
