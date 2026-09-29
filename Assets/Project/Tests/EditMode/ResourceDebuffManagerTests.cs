using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Retraits de PM : appliqués au prochain tour de la cible, une fois, sans cumul (le plus fort gagne).
    /// </summary>
    public class ResourceDebuffManagerTests
    {
        private readonly List<GameObject> _createdGameObjects = new List<GameObject>();

        private Unit NewUnit(int movement)
        {
            var go = new GameObject("TestUnit");
            _createdGameObjects.Add(go);
            var unit = go.AddComponent<Unit>();
            unit.SetMaxMovementPoints(movement);
            unit.RefreshMovement();
            return unit;
        }

        [Test]
        public void ApplyDebuff_AnnouncesPaAndPmLossAtImpact()
        {
            Unit unit = NewUnit(4);
            var received = new List<UnitEffectAppliedEvent>();
            System.Action<UnitEffectAppliedEvent> handler = e => received.Add(e);
            EventBus.Subscribe(handler);
            try
            {
                ResourceDebuffManager.ApplyDebuff(unit, 1, 2, null);
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(UnitEffect.ActionPoints, received[0].Effect);
            Assert.AreEqual(-1, received[0].Amount);
            Assert.AreEqual(UnitEffect.MovementPoints, received[1].Effect);
            Assert.AreEqual(-2, received[1].Amount);
            Assert.AreSame(unit, received[1].Target);

            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit); // vide le retrait en attente
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
        public void ActionBonus_StrongestWins_AndIsConsumedAtNextTurnStart()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyActionBonus(unit, 2, null);
            ResourceDebuffManager.ApplyActionBonus(unit, 1, null);
            Assert.AreEqual(2, ResourceDebuffManager.GetPendingActionBonus(unit), "Pas de cumul, le plus fort l'emporte");

            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);
            Assert.AreEqual(0, ResourceDebuffManager.GetPendingActionBonus(unit));
        }

        [Test]
        public void PmReduction_AppliesAtNextTurnStart_Once()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, 2, null);
            Assert.AreEqual(4, unit.GetCurrentMovementPoints(), "Rien ne change avant le tour de la cible");

            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);
            Assert.AreEqual(2, unit.GetCurrentMovementPoints());

            unit.RefreshMovement();
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);
            Assert.AreEqual(4, unit.GetCurrentMovementPoints(), "Le retrait ne dure qu'un tour");
        }

        [Test]
        public void CasterMovementLoss_AppliesAtCastersNextTurn()
        {
            // Bouclier de la terreur : bouclier tout de suite, 1 PM de moins au prochain tour du lanceur
            Unit caster = NewUnit(4);
            caster.SetMaxHealth(100);
            var card = ScriptableObject.CreateInstance<CardData>();
            card.targetType = CardTargetType.Self;
            card.shieldAmount = 27;
            card.casterMovementLoss = 1;

            card.ExecuteEffect(caster, caster);
            Object.DestroyImmediate(card);
            Assert.AreEqual(27, caster.GetShield());
            Assert.AreEqual(4, caster.GetCurrentMovementPoints(), "Rien ce tour-ci");

            caster.RefreshMovement();
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(caster);
            Assert.AreEqual(3, caster.GetCurrentMovementPoints());
        }

        [Test]
        public void RemoveAllMovement_EmptiesPm()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, int.MaxValue, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);

            Assert.AreEqual(0, unit.GetCurrentMovementPoints());
        }

        [Test]
        public void Tenacity_MonsterIgnoresPmRemovalTheTurnAfterLosingAll()
        {
            var go = new GameObject("TestEnemy");
            _createdGameObjects.Add(go);
            Enemy monster = go.AddComponent<Enemy>();
            monster.SetMaxMovementPoints(3);
            monster.RefreshMovement();

            // Tour 1 : perte totale -> devient tenace
            ResourceDebuffManager.ApplyDebuff(monster, 0, int.MaxValue, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(monster);
            Assert.AreEqual(0, monster.GetCurrentMovementPoints());
            Assert.IsTrue(ResourceDebuffManager.IsPmImmune(monster));

            // Tour 2 : le nouveau retrait est ignoré, la Ténacité s'arrête
            monster.RefreshMovement();
            ResourceDebuffManager.ApplyDebuff(monster, 0, int.MaxValue, null);
            Assert.AreEqual(0, ResourceDebuffManager.GetPending(monster).pm, "Affichage : le retrait ne compte pas");
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(monster);
            Assert.AreEqual(3, monster.GetCurrentMovementPoints());
            Assert.IsFalse(ResourceDebuffManager.IsPmImmune(monster));

            // Tour 3 : de nouveau vulnérable
            monster.RefreshMovement();
            ResourceDebuffManager.ApplyDebuff(monster, 0, 1, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(monster);
            Assert.AreEqual(2, monster.GetCurrentMovementPoints());
        }

        [Test]
        public void Tenacity_DoesNotApplyToChampions()
        {
            Unit champion = NewUnit(4); // faction Joueur

            ResourceDebuffManager.ApplyDebuff(champion, 0, int.MaxValue, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(champion);

            Assert.IsFalse(ResourceDebuffManager.IsPmImmune(champion));
        }

        [Test]
        public void PmReductions_DoNotStack_StrongestWins()
        {
            Unit unit = NewUnit(4);

            ResourceDebuffManager.ApplyDebuff(unit, 0, 1, null);
            ResourceDebuffManager.ApplyDebuff(unit, 0, 3, null);
            ResourceDebuffManager.ApplyDebuff(unit, 0, 2, null);
            ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);

            Assert.AreEqual(1, unit.GetCurrentMovementPoints());
        }
    }
}
