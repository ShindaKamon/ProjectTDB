using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Les formes de fusion (Éveil) hors Vol de mouvement : Avalanche, Ascension, Écho soigneur, Appât (zone),
    /// All-in, Partage des gains, Pioche et tempo.
    /// </summary>
    public class FusionFormsTests
    {
        // Grille minimale : seules les unités sont utiles aux formes testées
        private class StubGrid : IGridService
        {
            public readonly List<Unit> Units = new List<Unit>();

            public Unit GetActiveUnit() => null;
            public List<Unit> GetAllPlayerUnits() => Units.FindAll(u => u.GetFaction() == Unit.UnitFaction.Player);
            public List<Unit> GetAllEnemyUnits() => Units.FindAll(u => u.GetFaction() == Unit.UnitFaction.Enemy);
            public List<Unit> GetAllUnits() => Units;
            public Unit GetUnitAtGridPos(Vector2Int gridPos) => Units.Find(u => u.GetCurrentGridPos() == gridPos);
            public SummonUnit SpawnSummon(GameObject prefab, Vector2Int gridPos, Unit owner, int maxHealth) => null;
            public Tile GetTileAtPosition(Vector2Int pos) => null;
            public Vector2Int GetGridPosFromWorldPos(Vector3 worldPos) => Vector2Int.zero;
            public void HighlightTile(Vector2Int pos, Color color) { }
            public Dictionary<Tile, int> GetMovementTiles(Vector2Int startPos, int range, Unit ignoreUnit = null) => new Dictionary<Tile, int>();
            public List<Tile> GetAttackTiles(Vector2Int startPos, int range, Unit ignoreUnit = null) => new List<Tile>();
            public List<Tile> GetPathToTile(Vector2Int startPos, Vector2Int targetPos, int maxRange, Unit ignoreUnit = null) => new List<Tile>();
            public void InvalidateAttackTilesCache() { }
            public TurnStateMachine GetTurnStateMachine() => null;
        }

        private readonly List<Object> _created = new List<Object>();
        private StubGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new StubGrid();
            ServiceLocator.Instance.Register<IGridService>(_grid);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Instance.Unregister<IGridService>();
            foreach (Unit unit in _grid.Units) FusionZones.Clear(unit);
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        private static void SetField(object target, string name, object value)
        {
            System.Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) { field.SetValue(target, value); return; }
                type = type.BaseType;
            }
            Assert.Fail($"Champ {name} introuvable");
        }

        private T NewUnit<T>(Vector2Int pos, int health = 100) where T : Unit
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            var unit = go.AddComponent<T>();
            SetField(unit, "_maxHealth", 100);
            SetField(unit, "_health", health);
            SetField(unit, "_currentGridPos", pos);
            _grid.Units.Add(unit);
            return unit;
        }

        private T NewFusion<T>(EmotionType emotion) where T : FusionData
        {
            var fusion = ScriptableObject.CreateInstance<T>();
            fusion.emotion = emotion;
            _created.Add(fusion);
            return fusion;
        }

        // Champion fusionné avec la forme donnée, avec des PA (10 max, pleins)
        private T NewFused<T>(FusionData fusion, Vector2Int pos, int health = 100) where T : Champion
        {
            T champion = NewUnit<T>(pos, health);
            var data = ScriptableObject.CreateInstance<ChampionData>();
            _created.Add(data);
            data.fusions.Add(fusion);
            champion.championData = data;
            SetField(champion, "_actionPointsComponent", new ActionPointsComponent(10, "test"));
            champion.Gauge.AddPoints(fusion.emotion, EmotionGauge.MaxPoints);
            Assert.IsTrue(champion.TryActivateFusion(fusion.emotion));
            return champion;
        }

        private CardData NewCard(EmotionType emotion, int cost = 1)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.emotionType = emotion;
            card.costPA = cost;
            _created.Add(card);
            return card;
        }

        // ========== Zones (Appât) ==========

        [Test]
        public void Zone_RemovesPMFromEnemiesInside_ButNotAlliesNorOutsiders()
        {
            var owner = NewUnit<EvanUnit>(new Vector2Int(0, 0));
            var inside = NewUnit<Enemy>(new Vector2Int(5, 5));
            var edge = NewUnit<Enemy>(new Vector2Int(5, 6));
            var outside = NewUnit<Enemy>(new Vector2Int(5, 7));
            var ally = NewUnit<CruxUnit>(new Vector2Int(5, 5));
            FusionZones.Add(new Vector2Int(5, 5), 1, 1, owner);

            foreach (Unit unit in new Unit[] { inside, edge, outside, ally }) FusionZones.ApplyOnTurnStart(unit);

            Assert.AreEqual(1, ResourceDebuffManager.GetPending(inside).pm);
            Assert.AreEqual(1, ResourceDebuffManager.GetPending(edge).pm, "le rayon 1 couvre le voisinage");
            Assert.AreEqual(0, ResourceDebuffManager.GetPending(outside).pm);
            Assert.AreEqual(0, ResourceDebuffManager.GetPending(ally).pm, "les alliés du poseur ne sont pas touchés");
        }

        [Test]
        public void Zone_Clear_RemovesOnlyTheOwnersZones()
        {
            var first = NewUnit<EvanUnit>(new Vector2Int(0, 0));
            var second = NewUnit<CruxUnit>(new Vector2Int(1, 0));
            FusionZones.Add(new Vector2Int(5, 5), 1, 1, first);
            FusionZones.Add(new Vector2Int(6, 6), 1, 1, second);

            FusionZones.Clear(first);

            Assert.AreEqual(1, FusionZones.Count);
        }

        [Test]
        public void LureEnd_RemovesTheZone()
        {
            var lure = NewFusion<LureFusion>(EmotionType.Fear);
            var evan = NewFused<EvanUnit>(lure, new Vector2Int(0, 0));
            FusionZones.Add(new Vector2Int(5, 5), 1, 1, evan);

            lure.OnEnded(evan);

            Assert.AreEqual(0, FusionZones.Count);
        }

        // ========== Crux : Avalanche / Ascension ==========

        [Test]
        public void Avalanche_HitsEnemiesAdjacentToArrival_ForDamagePerCase()
        {
            var fusion = NewFusion<AvalancheFusion>(EmotionType.Anger);
            var crux = NewFused<CruxUnit>(fusion, new Vector2Int(2, 2));
            var adjacent = NewUnit<Enemy>(new Vector2Int(2, 3));
            var far = NewUnit<Enemy>(new Vector2Int(2, 5));
            var ally = NewUnit<EvanUnit>(new Vector2Int(3, 2));

            fusion.OnDisplacement(crux, NewCard(EmotionType.Anger), new Vector2Int(2, 2), 4);

            Assert.AreEqual(100 - 4 * fusion.damagePerCase, adjacent.GetHealth());
            Assert.AreEqual(100, far.GetHealth());
            Assert.AreEqual(100, ally.GetHealth());
        }

        [Test]
        public void Avalanche_MakesTheReflexBonusPermanent()
        {
            var crux = NewFused<CruxUnit>(NewFusion<AvalancheFusion>(EmotionType.Anger), Vector2Int.zero);
            Assert.Greater(crux.GetDamageMultiplier(), 1f);

            crux.ConsumeDamageModifier();
            Assert.Greater(crux.GetDamageMultiplier(), 1f, "non consommé pendant la fusion");
        }

        [Test]
        public void Ascension_HealsCrux_CappedPerTurn_AndSharesWithNearbyAllies()
        {
            var fusion = NewFusion<AscensionFusion>(EmotionType.Joy);
            var crux = NewFused<CruxUnit>(fusion, new Vector2Int(2, 2), health: 50);
            var near = NewUnit<EvanUnit>(new Vector2Int(2, 4), health: 50);
            var far = NewUnit<RazeUnit>(new Vector2Int(2, 8), health: 50);

            fusion.OnDisplacement(crux, NewCard(EmotionType.Joy), new Vector2Int(2, 2), 3);
            Assert.AreEqual(56, crux.GetHealth(), "2 PV par case");
            Assert.AreEqual(53, near.GetHealth(), "50 % du soin aux alliés à 2 cases");
            Assert.AreEqual(50, far.GetHealth());

            fusion.OnDisplacement(crux, NewCard(EmotionType.Joy), new Vector2Int(2, 2), 9);
            Assert.AreEqual(60, crux.GetHealth(), "plafond de 10 PV par tour");

            crux.OnOwnTurnStart();
            fusion.OnDisplacement(crux, NewCard(EmotionType.Joy), new Vector2Int(2, 2), 1);
            Assert.AreEqual(62, crux.GetHealth(), "plafond remis à zéro chaque tour");
        }

        // ========== Evan : Écho soigneur ==========

        [Test]
        public void EchoHeal_ReplacesTheEcho_AndHealsEveryAlly()
        {
            var fusion = NewFusion<EchoHealFusion>(EmotionType.Joy);
            var evan = NewFused<EvanUnit>(fusion, Vector2Int.zero, health: 50);
            var lyse = NewUnit<SummonUnit>(new Vector2Int(1, 0), health: 50);
            var crux = NewUnit<CruxUnit>(new Vector2Int(2, 0), health: 50);
            var enemy = NewUnit<Enemy>(new Vector2Int(3, 0));

            bool replaced = fusion.ReplaceSummonEcho(evan, lyse, 20);

            Assert.IsTrue(replaced);
            Assert.AreEqual(58, evan.GetHealth(), "40 % de l'attaque");
            Assert.AreEqual(58, lyse.GetHealth());
            Assert.AreEqual(58, crux.GetHealth());
            Assert.AreEqual(100, enemy.GetHealth());
        }

        [Test]
        public void DefaultFusion_DoesNotReplaceTheEcho()
        {
            var fusion = NewFusion<TempoFusion>(EmotionType.Fear);
            var evan = NewFused<EvanUnit>(fusion, Vector2Int.zero);

            Assert.IsFalse(fusion.ReplaceSummonEcho(evan, null, 20));
        }

        // ========== Raze : All-in / Partage / Tempo ==========

        [Test]
        public void AllIn_EveryCardIsASuite_BonusesAreDoubled_AndRecoilsNeverKills()
        {
            var fusion = NewFusion<AllInFusion>(EmotionType.Anger);
            var raze = NewFused<RazeUnit>(fusion, Vector2Int.zero, health: 3);
            raze.SpendPA(6);
            CardData card = NewCard(EmotionType.Anger, 5);

            raze.OnCardAboutToExecute(card);

            Assert.AreEqual(ComboPattern.Suite, raze.CurrentPattern, "même la première carte");
            Assert.AreEqual(6, raze.GetCurrentPA(), "+2 PA (Suite x2)");

            raze.OnCardResolved(card);
            Assert.AreEqual(1, raze.GetHealth(), "contrecoup de 5 PV plafonné pour ne pas tuer");
        }

        [Test]
        public void AllIn_Bluff_GivesDoubledShield_AndCombosStack()
        {
            var fusion = NewFusion<AllInFusion>(EmotionType.Anger);
            var raze = NewFused<RazeUnit>(fusion, Vector2Int.zero);
            raze.SpendPA(6);
            CardData first = NewCard(EmotionType.Anger, 1);
            raze.OnCardAboutToExecute(first);
            raze.OnCardResolved(first);

            raze.OnCardAboutToExecute(NewCard(EmotionType.Fear, 2));

            Assert.AreEqual(16, raze.GetShield(), "Bluff : 8 x 2");
            Assert.AreEqual(ComboPattern.Bluff, raze.CurrentPattern, "Bluff l'emporte sur la Suite");
        }

        [Test]
        public void NormalRaze_CurrentPattern_FollowsThePreviousCard()
        {
            var raze = NewUnit<RazeUnit>(Vector2Int.zero);
            CardData first = NewCard(EmotionType.Anger, 2);
            raze.OnCardAboutToExecute(first);
            Assert.AreEqual(ComboPattern.None, raze.CurrentPattern);
            raze.OnCardResolved(first);

            raze.OnCardAboutToExecute(NewCard(EmotionType.Anger, 2));

            Assert.AreEqual(ComboPattern.Pair, raze.CurrentPattern);
        }

        [Test]
        public void ShareGains_HealsTheMostWoundedAlly_OrShieldsWhenEveryoneIsFull()
        {
            var fusion = NewFusion<ShareGainsFusion>(EmotionType.Joy);
            var raze = NewFused<RazeUnit>(fusion, Vector2Int.zero, health: 80);
            var hurt = NewUnit<EvanUnit>(new Vector2Int(1, 0), health: 30);

            fusion.OnComboPattern(raze, ComboPattern.Pair, 3);
            Assert.AreEqual(36, hurt.GetHealth(), "2 PV par PA dépensé");
            Assert.AreEqual(80, raze.GetHealth());

            hurt.Heal(100);
            raze.Heal(100);
            fusion.OnComboPattern(raze, ComboPattern.Suite, 2);
            Assert.AreEqual(4, raze.GetShield() + hurt.GetShield(), "tous à pleine vie : bouclier");
        }

        [Test]
        public void Tempo_FirstHitEachTurn_GivesAPA_AndSuiteRemovesEnemyPA()
        {
            var fusion = NewFusion<TempoFusion>(EmotionType.Fear);
            var raze = NewFused<RazeUnit>(fusion, Vector2Int.zero);
            raze.SpendPA(5);
            var enemy = NewUnit<Enemy>(new Vector2Int(1, 0));
            var hit = new List<Unit> { enemy };
            CardData first = NewCard(EmotionType.Fear, 1);
            raze.OnCardAboutToExecute(first);
            raze.OnCardResolved(first);

            raze.OnCardAboutToExecute(NewCard(EmotionType.Fear, 2)); // Suite (1 puis 2)
            raze.OnCardHitEnemies(NewCard(EmotionType.Fear, 2), hit, firstOfCard: true);
            Assert.AreEqual(7, raze.GetCurrentPA(), "+1 PA de la Suite, +1 PA de la fusion à la première carte qui touche");
            Assert.AreEqual(1, ResourceDebuffManager.GetPending(enemy).pa, "la Suite retire 1 PA à l'ennemi");

            raze.OnCardHitEnemies(NewCard(EmotionType.Fear, 2), hit, firstOfCard: true);
            Assert.AreEqual(7, raze.GetCurrentPA(), "une seule fois par tour");

            raze.OnOwnTurnStart();
            raze.SpendPA(5);
            raze.OnCardHitEnemies(NewCard(EmotionType.Fear, 2), hit, firstOfCard: true);
            Assert.AreEqual(3, raze.GetCurrentPA(), "de nouveau disponible au tour suivant");
        }
    }
}
