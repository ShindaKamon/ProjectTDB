using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// GridRepository : accès aux tuiles et aux unités, portées de déplacement et d'attaque (BFS
    /// en 4 directions), chemin le plus court. Grille de test 4×4 ; la tuile (x, y) est en (x - 2, 0, y - 2).
    /// </summary>
    public class GridRepositoryTests
    {
        private class EnemyUnit : Unit
        {
            public override UnitFaction GetFaction() => UnitFaction.Enemy;
        }

        private const int Size = 4;

        private readonly List<GameObject> _created = new List<GameObject>();
        private GridRepository _repo;
        private Dictionary<Vector2Int, Tile> _tiles;
        private List<Unit> _units;

        [SetUp]
        public void SetUp()
        {
            _tiles = new Dictionary<Vector2Int, Tile>();
            _units = new List<Unit>();
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.position = new Vector3(x - Size / 2, 0f, y - Size / 2);
                    _created.Add(go);
                    _tiles[new Vector2Int(x, y)] = go.AddComponent<Tile>();
                }
            }
            _repo = new GridRepository(_tiles, _units, Size, Size);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _created.Clear();
        }

        private T NewUnit<T>(Vector2Int gridPos) where T : Unit
        {
            var go = new GameObject("TestUnit");
            _created.Add(go);
            var unit = go.AddComponent<T>();
            typeof(Unit).GetField("_currentGridPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(unit, gridPos);
            _repo.AddUnit(unit);
            return unit;
        }

        [Test]
        public void GetTileAtPosition_ReturnsTile_OrNullOutsideGrid()
        {
            Assert.AreSame(_tiles[new Vector2Int(1, 2)], _repo.GetTileAtPosition(new Vector2Int(1, 2)));
            Assert.IsNull(_repo.GetTileAtPosition(new Vector2Int(4, 0)));
            Assert.IsNull(_repo.GetTileAtPosition(new Vector2Int(-1, 0)));
        }

        [Test]
        public void GetGridPosFromWorldPos_ConvertsInsideGrid_AndSnapsToClosestTileOutside()
        {
            Assert.AreEqual(new Vector2Int(2, 2), _repo.GetGridPosFromWorldPos(new Vector3(0f, 0f, 0f)));
            Assert.AreEqual(new Vector2Int(0, 3), _repo.GetGridPosFromWorldPos(new Vector3(-2f, 0f, 1f)));
            Assert.AreEqual(new Vector2Int(3, 3), _repo.GetGridPosFromWorldPos(new Vector3(1.2f, 0f, 1.2f)), "hors grille : tuile la plus proche");
        }

        [Test]
        public void GetUnitAtGridPos_FindsUnit_OrNull()
        {
            Unit unit = NewUnit<Unit>(new Vector2Int(1, 1));

            Assert.AreSame(unit, _repo.GetUnitAtGridPos(new Vector2Int(1, 1)));
            Assert.IsNull(_repo.GetUnitAtGridPos(new Vector2Int(2, 2)));
        }

        [Test]
        public void GetAllPlayerUnits_AndEnemyUnits_SplitByFaction()
        {
            Unit player = NewUnit<Unit>(new Vector2Int(0, 0));
            Unit enemy = NewUnit<EnemyUnit>(new Vector2Int(3, 3));

            CollectionAssert.AreEqual(new[] { player }, _repo.GetAllPlayerUnits());
            CollectionAssert.AreEqual(new[] { enemy }, _repo.GetAllEnemyUnits());
            Assert.AreEqual(2, _repo.GetAllUnits().Count);
        }

        [Test]
        public void AddUnit_IgnoresDuplicates()
        {
            Unit unit = NewUnit<Unit>(new Vector2Int(0, 0));

            _repo.AddUnit(unit);

            Assert.AreEqual(1, _repo.GetAllUnits().Count);
        }

        [Test]
        public void GetMovementTiles_UsesManhattanCost_AndNoDiagonals()
        {
            Dictionary<Tile, int> reach = _repo.GetMovementTiles(new Vector2Int(0, 0), 2);

            Assert.AreEqual(6, reach.Count, "(0,0), 2 voisins, 3 cases à 2 pas");
            Assert.AreEqual(0, reach[_tiles[new Vector2Int(0, 0)]]);
            Assert.AreEqual(2, reach[_tiles[new Vector2Int(1, 1)]]);
            Assert.IsFalse(reach.ContainsKey(_tiles[new Vector2Int(2, 2)]), "4 pas de Manhattan");
        }

        [Test]
        public void GetMovementTiles_UnitsBlock_UnlessIgnored()
        {
            Unit blocker = NewUnit<Unit>(new Vector2Int(1, 0));
            Unit mover = NewUnit<Unit>(new Vector2Int(0, 0));

            Dictionary<Tile, int> blocked = _repo.GetMovementTiles(new Vector2Int(0, 0), 1, mover);
            Dictionary<Tile, int> ignoring = _repo.GetMovementTiles(new Vector2Int(0, 0), 1, blocker);

            Assert.IsFalse(blocked.ContainsKey(_tiles[new Vector2Int(1, 0)]), "case occupée");
            Assert.IsTrue(ignoring.ContainsKey(_tiles[new Vector2Int(1, 0)]));
        }

        [Test]
        public void GetAttackTiles_IgnoresUnits_ExcludesStartTile_AndIsCached()
        {
            NewUnit<Unit>(new Vector2Int(1, 0));

            List<Tile> tiles = _repo.GetAttackTiles(new Vector2Int(0, 0), 1);

            Assert.AreEqual(2, tiles.Count, "(1,0) et (0,1) ; une unité ne bloque pas la portée");
            Assert.IsFalse(tiles.Contains(_tiles[new Vector2Int(0, 0)]));
            Assert.AreSame(tiles, _repo.GetAttackTiles(new Vector2Int(0, 0), 1), "résultat mis en cache");

            _repo.InvalidateAttackTilesCache();
            Assert.AreNotSame(tiles, _repo.GetAttackTiles(new Vector2Int(0, 0), 1));
        }

        [Test]
        public void GetAttackTiles_RangeZero_KeepsStartTile()
        {
            List<Tile> tiles = _repo.GetAttackTiles(new Vector2Int(1, 1), 0);

            CollectionAssert.AreEqual(new[] { _tiles[new Vector2Int(1, 1)] }, tiles);
        }

        [Test]
        public void GetPathToTile_ReturnsShortestPath_WithoutStartTile()
        {
            List<Tile> path = _repo.GetPathToTile(new Vector2Int(0, 0), new Vector2Int(2, 1), 5);

            Assert.AreEqual(3, path.Count);
            Assert.AreSame(_tiles[new Vector2Int(2, 1)], path[path.Count - 1]);
        }

        [Test]
        public void GetPathToTile_EmptyWhenOnTarget_TooFar_OrBlocked()
        {
            Assert.IsEmpty(_repo.GetPathToTile(new Vector2Int(1, 1), new Vector2Int(1, 1), 5), "déjà sur la cible");
            Assert.IsEmpty(_repo.GetPathToTile(new Vector2Int(0, 0), new Vector2Int(3, 3), 3), "hors de portée");

            NewUnit<Unit>(new Vector2Int(1, 0));
            NewUnit<Unit>(new Vector2Int(0, 1));
            Assert.IsEmpty(_repo.GetPathToTile(new Vector2Int(0, 0), new Vector2Int(3, 3), 10), "coin enfermé par deux unités");
        }
    }
}
