using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Rôle d'une carte (pictogramme en main), déduit de ses effets.
    /// </summary>
    public class CardRolesTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(System.Action<CardData> setup)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            setup(card);
            _created.Add(card);
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void DamageThatAlsoSlows_IsAttack() =>
            Assert.AreEqual(CardRole.Attack, CardRoles.RoleOf(NewCard(c => { c.damageAmount = 14; c.pmReduction = 1; })));

        [Test]
        public void LeapThatHits_IsMovement() =>
            Assert.AreEqual(CardRole.Movement, CardRoles.RoleOf(NewCard(c => { c.leapToTarget = true; c.damageAmount = 17; })));

        [Test]
        public void HealOrShield_IsHeal()
        {
            Assert.AreEqual(CardRole.Heal, CardRoles.RoleOf(NewCard(c => c.healAmount = 12)));
            Assert.AreEqual(CardRole.Heal, CardRoles.RoleOf(NewCard(c => c.shieldAmount = 27)));
        }

        [Test]
        public void SlowWithoutDamage_IsControl() =>
            Assert.AreEqual(CardRole.Control, CardRoles.RoleOf(NewCard(c => c.removeAllMovement = true)));

        [Test]
        public void BonusDrawSummon_IsSupport()
        {
            Assert.AreEqual(CardRole.Support, CardRoles.RoleOf(NewCard(c => c.nextTurnActionGain = 2)));
            Assert.AreEqual(CardRole.Support, CardRoles.RoleOf(NewCard(c => c.drawAmount = 2)));
            Assert.AreEqual(CardRole.Support, CardRoles.RoleOf(NewCard(c => { c.isSummonCard = true; c.healAmount = 15; })), "Invocation de Lyse");
        }
    }
}
