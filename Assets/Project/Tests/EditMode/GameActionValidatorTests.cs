using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    public class GameActionValidatorTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private CardData NewCard(string name = "TestCard")
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            _created.Add(card);
            return card;
        }

        private ChampionData NewChampionData(string name = "TestChampion")
        {
            var data = ScriptableObject.CreateInstance<ChampionData>();
            data.championName = name;
            data.maxHealth = 100;
            data.movementRange = 3;
            data.maxActionPoints = 5;
            data.prefab = new GameObject("DummyChampionPrefab");
            _createdGameObjects.Add(data.prefab);
            _created.Add(data);
            return data;
        }

        private EnemyData NewEnemyData(string name = "TestEnemy")
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = name;
            data.maxHealth = 50;
            data.movementRange = 2;
            data.maxActionPoints = 2;
            data.prefab = new GameObject("DummyEnemyPrefab");
            _createdGameObjects.Add(data.prefab);
            data.combatDeck = new List<CardData> { NewCard("PatternCard") };
            _created.Add(data);
            return data;
        }

        /// <summary>
        /// Crée une Unit "nue" (sans passer par Initialize/Services.Grid) et pousse son état
        /// interne (santé, position, PM, mouvement) directement via réflexion, pour tester
        /// GameActionValidator sans dépendre d'un GridManager ou d'assets ScriptableObject.
        /// </summary>
        private T NewUnit<T>(Vector2Int? gridPos = null, int health = 100, int maxHealth = 100,
            int movementPoints = 3, bool isMoving = false) where T : Unit
        {
            var go = new GameObject("TestUnit_" + typeof(T).Name);
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<T>();

            SetField(unit, "_health", health);
            SetField(unit, "_maxHealth", maxHealth);
            SetField(unit, "_currentGridPos", gridPos ?? Vector2Int.zero);
            SetField(unit, "_currentMovementPoints", movementPoints);
            SetField(unit, "_isMoving", isMoving);

            return unit;
        }

        private Enemy NewEnemyWithPA(int currentPA, int maxPA, Vector2Int? gridPos = null)
        {
            var enemy = NewUnit<Enemy>(gridPos);
            var apComponent = new ActionPointsComponent(maxPA, "TestEnemy");
            // Ramène currentPA au niveau voulu (le constructeur initialise currentPA = maxPA).
            if (currentPA < maxPA)
                apComponent.SpendPA(maxPA - currentPA);
            SetField(enemy, "_actionPointsComponent", apComponent);
            return enemy;
        }

        /// <summary>
        /// Ajoute un DeckManager sur le GameObject de l'unité (TryGetComponentSafe cherche sur
        /// le même GameObject, cf. ComponentLocator.TryGetComponentSafe).
        /// </summary>
        private DeckManager AddDeckManager(Unit unit)
        {
            return unit.gameObject.AddComponent<DeckManager>();
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Assert.Fail($"Champ '{fieldName}' introuvable sur {target.GetType()} ou ses classes de base.");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();

            foreach (var go in _createdGameObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _createdGameObjects.Clear();
        }

        // ==================== CanPlayCard ====================

        [Test]
        public void CanPlayCard_NullPlayer_Fails()
        {
            var result = GameActionValidator.CanPlayCard(null, NewCard());

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Joueur null", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_NullCard_Fails()
        {
            var result = GameActionValidator.CanPlayCard(null, null);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CanPlayCard_ZeroCost_Succeeds()
        {
            var unit = NewUnit<Unit>();
            var card = NewCard();

            var result = GameActionValidator.CanPlayCard(unit, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_PlayerCannotSpendPA_Fails()
        {
            // Unit "nue" n'implémente pas IActionPointsUser.
            var unit = NewUnit<Unit>();
            var card = NewCard();
            card.costPA = 1;

            var result = GameActionValidator.CanPlayCard(unit, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("ne peut pas dépenser de PA", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_InsufficientPA_Fails()
        {
            var enemy = NewEnemyWithPA(currentPA: 1, maxPA: 3);
            var card = NewCard();
            card.costPA = 2;

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("PA insuffisants", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_SufficientPA_Succeeds()
        {
            var enemy = NewEnemyWithPA(currentPA: 3, maxPA: 3);
            var card = NewCard();
            card.costPA = 2;

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_InsufficientHP_Fails()
        {
            var unit = NewUnit<Unit>(health: 5);
            var card = NewCard();
            card.costHP = 10;

            var result = GameActionValidator.CanPlayCard(unit, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("PV insuffisants", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_SufficientHP_Succeeds()
        {
            var unit = NewUnit<Unit>(health: 50);
            var card = NewCard();
            card.costHP = 10;

            var result = GameActionValidator.CanPlayCard(unit, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== CanPlayCard - DeckManager (coût effectif) ====================

        [Test]
        public void CanPlayCard_DeckManagerWithReducedCostOverride_UsesEffectiveCost()
        {
            // Sans override, le coût brut (2 PA) dépasserait le PA disponible (1) et échouerait.
            // Avec l'override actif (-1), le coût effectif (1) doit être utilisé à la place.
            var enemy = NewEnemyWithPA(currentPA: 1, maxPA: 3);
            var deckManager = AddDeckManager(enemy);
            var card = NewCard();
            card.costPA = 2;
            deckManager.ModifyCardCost(card, -1); // coût effectif : 1

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_DeckManagerWithIncreasedCostOverride_UsesEffectiveCost()
        {
            // Sans override, le coût brut (1 PA) serait jouable avec 1 PA disponible.
            // Avec l'override actif (+1), le coût effectif (2) doit bloquer l'action.
            var enemy = NewEnemyWithPA(currentPA: 1, maxPA: 3);
            var deckManager = AddDeckManager(enemy);
            var card = NewCard();
            card.costPA = 1;
            deckManager.ModifyCardCost(card, 1); // coût effectif : 2

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("PA insuffisants", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_DeckManagerWithoutActiveOverride_BehavesLikeBaseCost()
        {
            // DeckManager présent mais aucun override actif : le comportement doit être
            // identique à celui d'avant l'introduction de la surcouche de coût.
            var enemy = NewEnemyWithPA(currentPA: 1, maxPA: 3);
            AddDeckManager(enemy);
            var card = NewCard();
            card.costPA = 2;

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("PA insuffisants", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_DeckManagerZeroCostCardWithoutOverride_StaysFreeEvenWithNoPA()
        {
            // Régression du bug corrigé dans DeckManager.GetEffectiveCost : le plancher de
            // 1 PA s'appliquait auparavant même sans override actif, rendant les cartes à
            // coût 0 (ex: Rage) injouables gratuitement dès qu'un DeckManager était présent.
            var enemy = NewEnemyWithPA(currentPA: 0, maxPA: 3);
            AddDeckManager(enemy);
            var card = NewCard();
            card.costPA = 0;

            var result = GameActionValidator.CanPlayCard(enemy, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== CanPlayCard - Stock de Rage plein ====================

        [Test]
        public void CanPlayCard_RageCard_StockFull_Fails()
        {
            var ilya = NewUnit<IlyaUnit>();
            SetField(ilya, "_rageStock", 5);
            SetField(ilya, "_maxRageStock", 5);
            var card = NewCard();
            card.isRageCard = true;
            card.costPA = 0;

            var result = GameActionValidator.CanPlayCard(ilya, card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Stock de Rage plein", result.ErrorMessage);
        }

        [Test]
        public void CanPlayCard_RageCard_StockNotFull_Succeeds()
        {
            var ilya = NewUnit<IlyaUnit>();
            SetField(ilya, "_rageStock", 2);
            SetField(ilya, "_maxRageStock", 5);
            var card = NewCard();
            card.isRageCard = true;
            card.costPA = 0;

            var result = GameActionValidator.CanPlayCard(ilya, card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== CanTargetUnit ====================

        [Test]
        public void CanTargetUnit_NullCard_Fails()
        {
            var result = GameActionValidator.CanTargetUnit(null, null, null);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Carte null", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_NullSource_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;

            var result = GameActionValidator.CanTargetUnit(card, null, null);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Source null", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_DoesNotTargetUnit_SucceedsWithoutTarget()
        {
            var card = NewCard();
            card.targetType = CardTargetType.None;
            var source = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetUnit(card, source, null);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_RequiresTarget_NullTarget_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;
            var source = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetUnit(card, source, null);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("nécessite une cible", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_OutOfRange_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyUnit;
            card.targetRange = 1;
            var source = NewUnit<Unit>(gridPos: new Vector2Int(0, 0));
            var target = NewUnit<Unit>(gridPos: new Vector2Int(5, 5));

            var result = GameActionValidator.CanTargetUnit(card, source, target);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("hors de portée", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_EnemyType_TargetingAlly_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;
            card.targetRange = 10;
            var source = NewUnit<Unit>(); // faction Player par défaut
            var ally = NewUnit<Unit>();   // faction Player aussi

            var result = GameActionValidator.CanTargetUnit(card, source, ally);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("que les ennemis", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_EnemyType_TargetingEnemy_Succeeds()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;
            card.targetRange = 10;
            var source = NewUnit<Unit>();
            var enemyTarget = NewUnit<Enemy>();

            var result = GameActionValidator.CanTargetUnit(card, source, enemyTarget);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_AllyType_TargetingSelf_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Ally;
            card.targetRange = 10;
            var source = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetUnit(card, source, source);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("pas soi-même", result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_SelfType_TargetingSelf_Succeeds()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Self;
            card.targetRange = 10;
            var source = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetUnit(card, source, source);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanTargetUnit_SelfType_TargetingOther_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Self;
            card.targetRange = 10;
            var source = NewUnit<Unit>();
            var other = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetUnit(card, source, other);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("que soi-même", result.ErrorMessage);
        }

        // ==================== CanTargetTile ====================

        [Test]
        public void CanTargetTile_NullCard_Fails()
        {
            var result = GameActionValidator.CanTargetTile(null, null, Vector2Int.zero);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CanTargetTile_DoesNotTargetTile_Succeeds()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Self;
            var source = NewUnit<Unit>();

            var result = GameActionValidator.CanTargetTile(card, source, new Vector2Int(9, 9));

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanTargetTile_OutOfRange_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyTile;
            card.targetRange = 1;
            var source = NewUnit<Unit>(gridPos: Vector2Int.zero);

            var result = GameActionValidator.CanTargetTile(card, source, new Vector2Int(5, 0));

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("hors de portée", result.ErrorMessage);
        }

        [Test]
        public void CanTargetTile_InRange_Succeeds()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyTile;
            card.targetRange = 5;
            var source = NewUnit<Unit>(gridPos: Vector2Int.zero);

            var result = GameActionValidator.CanTargetTile(card, source, new Vector2Int(2, 0));

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanTargetTile_ChargeCard_NotInStraightLine_Fails()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyTile;
            card.targetRange = 10;
            card.isChargeCard = true;
            var source = NewUnit<Unit>(gridPos: Vector2Int.zero);

            var result = GameActionValidator.CanTargetTile(card, source, new Vector2Int(2, 2));

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("ligne droite", result.ErrorMessage);
        }

        [Test]
        public void CanTargetTile_ChargeCard_InLineWithinRange_Succeeds()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyTile;
            card.targetRange = 5;
            card.isChargeCard = true;
            var source = NewUnit<Unit>(gridPos: Vector2Int.zero);

            var result = GameActionValidator.CanTargetTile(card, source, new Vector2Int(3, 0));

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== CanMove / CanMoveToTile ====================

        [Test]
        public void CanMove_NullUnit_Fails()
        {
            var result = GameActionValidator.CanMove(null);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CanMove_NoMovementPoints_Fails()
        {
            var unit = NewUnit<Unit>(movementPoints: 0);

            var result = GameActionValidator.CanMove(unit);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("plus de PM", result.ErrorMessage);
        }

        [Test]
        public void CanMove_AlreadyMoving_Fails()
        {
            var unit = NewUnit<Unit>(movementPoints: 3, isMoving: true);

            var result = GameActionValidator.CanMove(unit);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("déjà en train de se déplacer", result.ErrorMessage);
        }

        [Test]
        public void CanMove_Valid_Succeeds()
        {
            var unit = NewUnit<Unit>(movementPoints: 3, isMoving: false);

            var result = GameActionValidator.CanMove(unit);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void CanMoveToTile_PathTooExpensive_Fails()
        {
            var unit = NewUnit<Unit>(gridPos: Vector2Int.zero, movementPoints: 2);

            var result = GameActionValidator.CanMoveToTile(unit, new Vector2Int(3, 0), pathCost: 3);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("trop loin", result.ErrorMessage);
        }

        [Test]
        public void CanMoveToTile_SamePosition_Fails()
        {
            var unit = NewUnit<Unit>(gridPos: new Vector2Int(2, 2), movementPoints: 3);

            var result = GameActionValidator.CanMoveToTile(unit, new Vector2Int(2, 2), pathCost: 0);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("déjà sur cette case", result.ErrorMessage);
        }

        [Test]
        public void CanMoveToTile_Valid_Succeeds()
        {
            var unit = NewUnit<Unit>(gridPos: Vector2Int.zero, movementPoints: 3);

            var result = GameActionValidator.CanMoveToTile(unit, new Vector2Int(2, 0), pathCost: 2);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== IsUnitTurn ====================

        [Test]
        public void IsUnitTurn_NullUnit_Fails()
        {
            var activeUnit = NewUnit<Unit>();

            var result = GameActionValidator.IsUnitTurn(null, activeUnit);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void IsUnitTurn_NullActiveUnit_Fails()
        {
            var unit = NewUnit<Unit>();

            var result = GameActionValidator.IsUnitTurn(unit, null);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Aucune unité active", result.ErrorMessage);
        }

        [Test]
        public void IsUnitTurn_NotActiveUnit_Fails()
        {
            var unit = NewUnit<Unit>();
            var activeUnit = NewUnit<Unit>();

            var result = GameActionValidator.IsUnitTurn(unit, activeUnit);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Ce n'est pas le tour de", result.ErrorMessage);
        }

        [Test]
        public void IsUnitTurn_IsActiveUnit_Succeeds()
        {
            var unit = NewUnit<Unit>();

            var result = GameActionValidator.IsUnitTurn(unit, unit);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        // ==================== ValidateChampionData ====================

        [Test]
        public void ValidateChampionData_Null_Fails()
        {
            Assert.IsFalse(GameActionValidator.ValidateChampionData(null).IsValid);
        }

        [Test]
        public void ValidateChampionData_Valid_Succeeds()
        {
            var data = NewChampionData();

            var result = GameActionValidator.ValidateChampionData(data);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void ValidateChampionData_ZeroMaxHealth_Fails()
        {
            var data = NewChampionData();
            data.maxHealth = 0;

            var result = GameActionValidator.ValidateChampionData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("maxHealth", result.ErrorMessage);
        }

        [Test]
        public void ValidateChampionData_NegativeMovementRange_Fails()
        {
            var data = NewChampionData();
            data.movementRange = -1;

            var result = GameActionValidator.ValidateChampionData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("movementRange", result.ErrorMessage);
        }

        [Test]
        public void ValidateChampionData_NegativeMaxActionPoints_Fails()
        {
            var data = NewChampionData();
            data.maxActionPoints = -1;

            var result = GameActionValidator.ValidateChampionData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("maxActionPoints", result.ErrorMessage);
        }

        [Test]
        public void ValidateChampionData_MissingPrefab_Fails()
        {
            var data = NewChampionData();
            data.prefab = null;

            var result = GameActionValidator.ValidateChampionData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("prefab", result.ErrorMessage);
        }

        // ==================== ValidateEnemyData ====================

        [Test]
        public void ValidateEnemyData_Null_Fails()
        {
            Assert.IsFalse(GameActionValidator.ValidateEnemyData(null).IsValid);
        }

        [Test]
        public void ValidateEnemyData_Valid_Succeeds()
        {
            var data = NewEnemyData();

            var result = GameActionValidator.ValidateEnemyData(data);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void ValidateEnemyData_ZeroMaxHealth_Fails()
        {
            var data = NewEnemyData();
            data.maxHealth = 0;

            var result = GameActionValidator.ValidateEnemyData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("maxHealth", result.ErrorMessage);
        }

        [Test]
        public void ValidateEnemyData_MissingPrefab_Fails()
        {
            var data = NewEnemyData();
            data.prefab = null;

            var result = GameActionValidator.ValidateEnemyData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("prefab", result.ErrorMessage);
        }

        [Test]
        public void ValidateEnemyData_EmptyCombatDeck_Fails()
        {
            var data = NewEnemyData();
            data.combatDeck.Clear();

            var result = GameActionValidator.ValidateEnemyData(data);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("combatDeck", result.ErrorMessage);
        }

        // ==================== ValidateCardData ====================

        [Test]
        public void ValidateCardData_Null_Fails()
        {
            Assert.IsFalse(GameActionValidator.ValidateCardData(null).IsValid);
        }

        [Test]
        public void ValidateCardData_NegativeCostPA_Fails()
        {
            var card = NewCard();
            card.costPA = -1;

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("costPA", result.ErrorMessage);
        }

        [Test]
        public void ValidateCardData_NegativeTargetRange_Fails()
        {
            var card = NewCard();
            card.targetRange = -3;

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("targetRange", result.ErrorMessage);
        }

        [Test]
        public void ValidateCardData_Default_IsValid()
        {
            var card = NewCard();

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [Test]
        public void ValidateCardData_ZeroTargetCount_Fails()
        {
            var card = NewCard();
            card.targetCount = 0;

            var result = GameActionValidator.ValidateCardData(card);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("targetCount", result.ErrorMessage);
        }

        [Test]
        public void IsMultiTarget_TargetCountOne_False()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;
            card.targetCount = 1;

            Assert.IsFalse(card.isMultiTarget);
        }

        [Test]
        public void IsMultiTarget_TargetCountTwoAndTargetsUnit_True()
        {
            var card = NewCard();
            card.targetType = CardTargetType.Enemy;
            card.targetCount = 2;

            Assert.IsTrue(card.isMultiTarget);
        }

        [Test]
        public void IsMultiTarget_TargetCountTwoButTargetsTile_False()
        {
            var card = NewCard();
            card.targetType = CardTargetType.AnyTile;
            card.targetCount = 2;

            Assert.IsFalse(card.isMultiTarget);
        }
    }
}
