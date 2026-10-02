using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Au lit ! : trajet d'un tas de débris vers le Lit (PileSweep), en 4 directions, sans les cases du tas ni du Lit.
    /// </summary>
    public class PileSweepTests
    {
        private static readonly Vector2Int[] Lit = { new Vector2Int(3, 8), new Vector2Int(3, 7) }; // tête, pied

        [Test]
        public void Path_GoesFromClosestPileCell_ToClosestLitCell_WithoutEndpoints()
        {
            var path = PileSweep.Path(new[] { new Vector2Int(3, 3) }, Lit);

            CollectionAssert.AreEqual(new[] { new Vector2Int(3, 4), new Vector2Int(3, 5), new Vector2Int(3, 6) }, path);
        }

        [Test]
        public void Path_FromTwoCellPile_StartsFromItsCellNearestTheLit()
        {
            // Débris d'un lit (2 cases) : le trajet part de la case la plus proche du Lit
            var path = PileSweep.Path(new[] { new Vector2Int(8, 5), new Vector2Int(7, 5) }, Lit);

            Assert.IsFalse(path.Contains(new Vector2Int(7, 5)));
            Assert.AreEqual(GridGeometry.Distance(new Vector2Int(7, 5), new Vector2Int(3, 7)) - 1, path.Count);
            Assert.IsTrue(GridGeometry.AreAdjacent(new Vector2Int(7, 5), path[0]));
        }

        [Test]
        public void Path_PileTouchingTheLit_IsEmpty()
        {
            Assert.IsEmpty(PileSweep.Path(new[] { new Vector2Int(3, 6) }, Lit));
        }
    }
}
