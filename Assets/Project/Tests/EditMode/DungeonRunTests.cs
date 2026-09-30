using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    public class DungeonRunTests
    {
        private DungeonData _dungeon;
        private EncounterData _encounter;

        [SetUp]
        public void SetUp()
        {
            _encounter = ScriptableObject.CreateInstance<EncounterData>();
            _dungeon = ScriptableObject.CreateInstance<DungeonData>();
            for (int i = 0; i < 2; i++)
            {
                var room = new DungeonData.Room { start = new Vector2Int(1, 1) };
                room.monsters.Add(new DungeonData.MonsterSpot { cell = new Vector2Int(4, 3), encounter = _encounter });
                _dungeon.rooms.Add(room);
            }
            DungeonRun.Begin(_dungeon);
        }

        [TearDown]
        public void TearDown()
        {
            DungeonRun.Clear();
            Object.DestroyImmediate(_dungeon);
            Object.DestroyImmediate(_encounter);
        }

        [Test]
        public void IsRoomCleared_OnlyOnceItsGroupIsDefeated()
        {
            Assert.IsFalse(DungeonRun.IsRoomCleared(0));
            DungeonRun.StartEncounter(0, new Vector2Int(3, 3));
            DungeonRun.CompleteEncounter();
            Assert.IsTrue(DungeonRun.IsRoomCleared(0));
            Assert.IsFalse(DungeonRun.IsRoomCleared(1));
        }

        [Test]
        public void Begin_StartsInRoomZeroOnStartCell()
        {
            Assert.IsTrue(DungeonRun.IsActive);
            Assert.AreEqual(0, DungeonRun.CurrentRoom);
            Assert.AreEqual(new Vector2Int(1, 1), DungeonRun.PartyCell);
            Assert.IsNull(DungeonRun.CurrentEncounter);
        }

        [Test]
        public void CompleteEncounter_MarksOnlyThatMonsterAsDefeated()
        {
            DungeonRun.StartEncounter(0, new Vector2Int(3, 3));
            Assert.AreSame(_encounter, DungeonRun.CurrentEncounter);
            Assert.IsFalse(DungeonRun.IsDefeated(0, 0));

            DungeonRun.CompleteEncounter();

            Assert.IsTrue(DungeonRun.IsDefeated(0, 0));
            Assert.IsFalse(DungeonRun.IsDefeated(1, 0));
            Assert.IsNull(DungeonRun.CurrentEncounter);
            Assert.AreEqual(new Vector2Int(3, 3), DungeonRun.PartyCell);
        }

        [Test]
        public void CompleteEncounter_WithoutFight_DoesNothing()
        {
            DungeonRun.CompleteEncounter();
            Assert.IsFalse(DungeonRun.IsDefeated(0, 0));
        }

        [Test]
        public void IsCompleted_OnlyWhenEveryMonsterIsDefeated()
        {
            DungeonRun.StartEncounter(0, Vector2Int.zero);
            DungeonRun.CompleteEncounter();
            Assert.IsFalse(DungeonRun.IsCompleted);

            DungeonRun.MoveToRoom(1, Vector2Int.zero);
            DungeonRun.StartEncounter(0, Vector2Int.zero);
            DungeonRun.CompleteEncounter();
            Assert.IsTrue(DungeonRun.IsCompleted);
        }

        [Test]
        public void ConsumeDoorsJustOpened_TrueOnceAfterRoomIsCleared()
        {
            Assert.IsFalse(DungeonRun.ConsumeDoorsJustOpened());

            DungeonRun.StartEncounter(0, Vector2Int.zero);
            DungeonRun.CompleteEncounter();

            Assert.IsTrue(DungeonRun.ConsumeDoorsJustOpened());
            Assert.IsFalse(DungeonRun.ConsumeDoorsJustOpened());
        }

        [Test]
        public void Clear_LeavesNoActiveRun()
        {
            DungeonRun.StartEncounter(0, Vector2Int.zero);
            DungeonRun.CompleteEncounter();
            DungeonRun.Clear();

            Assert.IsFalse(DungeonRun.IsActive);
            Assert.IsFalse(DungeonRun.IsDefeated(0, 0));
        }
    }
}
