using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Entrave (Aura de terreur, Enemy.HinderNextCard) : au prochain tour du monstre, attaque de base à la
    /// place de sa carte, sans avancer son pattern ; la carte revient au tour suivant.
    /// </summary>
    public class EnemyHinderTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private CardData NewCard(string name)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.cardName = name;
            return card;
        }

        // Monstre avec un pattern, sans passer par InitializeEnemy (qui demande la grille)
        private Enemy NewEnemy(params CardData[] pattern)
        {
            var go = new GameObject("Monstre");
            _created.Add(go);
            Enemy enemy = go.AddComponent<Enemy>();
            typeof(Enemy).GetField("_combatDeck", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(enemy, new List<CardData>(pattern));
            return enemy;
        }

        [Test]
        public void Hinder_IsConsumedOnce_AndKeepsTheSameNextCard()
        {
            CardData first = NewCard("Agrippe");
            Enemy enemy = NewEnemy(first, NewCard("Pluie de jouets"));

            enemy.HinderNextCard();
            Assert.IsTrue(enemy.IsNextCardHindered);

            Assert.IsTrue(enemy.ConsumeHinderedCard(), "le tour entravé");
            Assert.AreSame(first, enemy.GetNextCard(), "le pattern n'avance pas : la carte revient au tour suivant");
            Assert.IsFalse(enemy.ConsumeHinderedCard(), "une seule fois");
        }

        [Test]
        public void Hinder_WithoutPattern_DoesNothing()
        {
            Enemy enemy = NewEnemy();

            enemy.HinderNextCard();

            Assert.IsFalse(enemy.IsNextCardHindered);
        }
    }
}
