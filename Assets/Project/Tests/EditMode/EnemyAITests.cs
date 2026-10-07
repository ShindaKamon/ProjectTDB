using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Ciblage de base des monstres (EnemyAI.ChooseTarget) : le plus proche, puis le moins de PV.
    /// </summary>
    public class EnemyAITests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(Vector2Int gridPos, int health)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxHealth(100);
            typeof(Unit).GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, health);
            typeof(Unit).GetField("_currentGridPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, gridPos);
            return unit;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _createdGameObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _createdGameObjects.Clear();
        }

        [Test]
        public void ChooseTarget_ClosestFirst()
        {
            Unit near = NewUnit(new Vector2Int(2, 0), 100);
            Unit farButWeak = NewUnit(new Vector2Int(5, 0), 10);

            Assert.AreSame(near, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { farButWeak, near }));
        }

        [Test]
        public void ChooseTarget_SameDistance_LowestHealth()
        {
            Unit healthy = NewUnit(new Vector2Int(2, 0), 80);
            Unit weak = NewUnit(new Vector2Int(0, 2), 30);

            Assert.AreSame(weak, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { healthy, weak }));
        }

        [Test]
        public void ChooseTarget_AllInRange_LowestHealthEvenIfFarther()
        {
            // Attaque de portée 2 : Evan à 1 case, Lyse à 2 cases avec moins de PV → Lyse
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);
            Unit lyse = NewUnit(new Vector2Int(0, 2), 50);

            Assert.AreSame(lyse, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { evan, lyse }, attackRange: 2));
        }

        [Test]
        public void ChooseTarget_OutOfRange_ClosestStillWins()
        {
            // Portée 1 : seul Evan est à portée, Lyse (moins de PV) est trop loin
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);
            Unit lyse = NewUnit(new Vector2Int(0, 2), 50);

            Assert.AreSame(evan, EnemyAI.ChooseTarget(Vector2Int.zero, new[] { evan, lyse }, attackRange: 1));
        }

        [Test]
        public void ChooseTarget_NoCandidate_ReturnsNull()
        {
            Assert.IsNull(EnemyAI.ChooseTarget(Vector2Int.zero, new List<Unit>()));
        }

        // Grille 10×10, cases occupées données
        private static System.Func<Vector2Int, bool> FreeExcept(params Vector2Int[] occupied) =>
            p => p.x >= 0 && p.y >= 0 && p.x < 10 && p.y < 10 && System.Array.IndexOf(occupied, p) < 0;

        [Test]
        public void PathTowards_MonsterBehindAnother_GoesAround()
        {
            // Cible en (5,0), un mouton en (4,0) devant l'autre en (3,0) : contact au bout du chemin
            var path = EnemyAI.PathTowards(new Vector2Int(3, 0), new Vector2Int(5, 0), 1, 4,
                FreeExcept(new Vector2Int(4, 0), new Vector2Int(5, 0)));

            Assert.AreEqual(3, path.Count, "contourne par (3,1) (4,1) (5,1)");
            Assert.AreEqual(1, GridGeometry.Distance(path[path.Count - 1], new Vector2Int(5, 0)));
        }

        [Test]
        public void PathTowards_AlreadyInRange_DoesNotMove()
        {
            var path = EnemyAI.PathTowards(new Vector2Int(0, 0), new Vector2Int(2, 0), 2, 3, FreeExcept(new Vector2Int(2, 0)));

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void PathTowards_LimitedByMovement()
        {
            var path = EnemyAI.PathTowards(new Vector2Int(0, 0), new Vector2Int(9, 0), 1, 3, FreeExcept(new Vector2Int(9, 0)));

            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) }, path);
        }

        [Test]
        public void ChooseTarget_FromAnyBed_UsesTheClosestBed()
        {
            // Boss caché : il frappe depuis n'importe quel lit. Champion à 2 cases du lit (9,4), loin du lit (1,9)
            var beds = new[] { new Vector2Int(1, 9), new Vector2Int(9, 4) };
            Unit champion = NewUnit(new Vector2Int(7, 4), 100);

            Assert.AreEqual(2, EnemyAI.DistanceFrom(beds, champion.GetCurrentGridPos()));
            Assert.AreSame(champion, EnemyAI.ChooseTarget(beds, new[] { champion }, 3));
        }

        [Test]
        public void AmbushCell_NextToTarget_FarthestFromOtherChampions()
        {
            // Cible en (5,5), autre champion en (5,8) au nord : Frayeur surgit au sud de la cible, en (5,4)
            Vector2Int? cell = EnemyAI.AmbushCell(new Vector2Int(5, 5), new[] { new Vector2Int(5, 8) },
                FreeExcept(new Vector2Int(5, 5), new Vector2Int(5, 8)));

            Assert.AreEqual(new Vector2Int(5, 4), cell);
        }

        [Test]
        public void AmbushCell_TargetSurrounded_NearestFreeCell()
        {
            var target = new Vector2Int(5, 5);
            Vector2Int? cell = EnemyAI.AmbushCell(target, new Vector2Int[0],
                FreeExcept(target, new Vector2Int(4, 5), new Vector2Int(6, 5), new Vector2Int(5, 4), new Vector2Int(5, 6)));

            Assert.AreEqual(2, GridGeometry.Distance(target, cell.Value));
        }

        [Test]
        public void GuardCell_BetweenProtegeeAndThreat()
        {
            // Mouton en (5,5), champion en (5,1) au sud : le soldat se place au sud du mouton, en (5,4)
            Vector2Int? cell = EnemyAI.GuardCell(new Vector2Int(5, 5), new Vector2Int(5, 1), new Vector2Int(8, 8),
                FreeExcept(new Vector2Int(5, 5), new Vector2Int(5, 1)));

            Assert.AreEqual(new Vector2Int(5, 4), cell);
        }

        [Test]
        public void GuardCell_AlreadyInPlace_Stays()
        {
            Vector2Int? cell = EnemyAI.GuardCell(new Vector2Int(5, 5), new Vector2Int(5, 1), new Vector2Int(5, 4),
                FreeExcept(new Vector2Int(5, 5), new Vector2Int(5, 1), new Vector2Int(5, 4)));

            Assert.AreEqual(new Vector2Int(5, 4), cell, "sa propre case compte comme libre");
        }

        [Test]
        public void PathTowards_ReachTarget_EndsOnTheCell()
        {
            var path = EnemyAI.PathTowards(new Vector2Int(0, 0), new Vector2Int(2, 0), 0, 5, FreeExcept(), reachTarget: true);

            Assert.AreEqual(new Vector2Int(2, 0), path[path.Count - 1]);
        }

        [Test]
        public void NearestFreeCell_NextToOrigin_SkipsOccupiedCells()
        {
            // Lit en (4,9) contre le mur du fond, voisins (3,9) et (5,9) occupés : le mouton sort devant, en (4,8)
            Vector2Int? cell = EnemyAI.NearestFreeCell(new Vector2Int(4, 9),
                FreeExcept(new Vector2Int(4, 9), new Vector2Int(3, 9), new Vector2Int(5, 9)));

            Assert.AreEqual(new Vector2Int(4, 8), cell);
        }

        [Test]
        public void NearestFreeCell_GoesPastOccupiedNeighbours()
        {
            Vector2Int? cell = EnemyAI.NearestFreeCell(new Vector2Int(0, 0),
                FreeExcept(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1)));

            Assert.AreEqual(2, GridGeometry.Distance(new Vector2Int(0, 0), cell.Value), "au-delà des voisins occupés");
        }

        [Test]
        public void ApplyTaunt_TaunterOnBoard_OnlyTarget()
        {
            Unit ilya = NewUnit(new Vector2Int(5, 0), 100);
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);

            CollectionAssert.AreEqual(new[] { ilya }, EnemyAI.ApplyTaunt(ilya, new List<Unit> { evan, ilya }));
        }

        [Test]
        public void ApplyTaunt_NoTaunterOrGone_AllPlayers()
        {
            Unit evan = NewUnit(new Vector2Int(1, 0), 100);
            Unit gone = NewUnit(new Vector2Int(5, 0), 100);
            var players = new List<Unit> { evan };

            Assert.AreSame(players, EnemyAI.ApplyTaunt(null, players));
            Assert.AreSame(players, EnemyAI.ApplyTaunt(gone, players), "provocateur absent du terrain");
        }
    }
}
