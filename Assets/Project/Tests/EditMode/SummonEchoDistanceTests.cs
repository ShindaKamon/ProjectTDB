using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Portée de l'écho du Miroir fraternel : 8 directions, une diagonale vaut 1 case.
    /// </summary>
    public class SummonEchoDistanceTests
    {
        [Test]
        public void Orthogonal_Adjacent_IsOne()
        {
            Assert.AreEqual(1, CardData.SummonEchoDistance(new Vector2Int(5, 5), new Vector2Int(5, 6)));
        }

        [Test]
        public void Diagonal_Adjacent_IsOne()
        {
            Assert.AreEqual(1, CardData.SummonEchoDistance(new Vector2Int(5, 5), new Vector2Int(6, 6)));
        }

        [Test]
        public void KnightMove_IsTwo()
        {
            Assert.AreEqual(2, CardData.SummonEchoDistance(new Vector2Int(5, 5), new Vector2Int(6, 7)));
        }

        [Test]
        public void SamePosition_IsZero()
        {
            Assert.AreEqual(0, CardData.SummonEchoDistance(new Vector2Int(3, 3), new Vector2Int(3, 3)));
        }
    }
}
