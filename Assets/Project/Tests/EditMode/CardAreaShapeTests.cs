using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Forme de zone des cartes (CardData.IsInAOEShape), utilisée à la fois pour appliquer
    /// l'effet et pour l'aperçu de zone sur la grille.
    /// </summary>
    public class CardAreaShapeTests
    {
        private GameObject _casterGo;
        private CardData _card;

        private Unit NewCaster(Vector2Int gridPos)
        {
            _casterGo = new GameObject("TestCaster");
            var unit = _casterGo.AddComponent<Unit>();
            typeof(Unit).GetField("_currentGridPos", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(unit, gridPos);
            return unit;
        }

        private CardData NewCard(CardAreaEffect area, int radius)
        {
            _card = ScriptableObject.CreateInstance<CardData>();
            _card.areaEffect = area;
            _card.aoeRadius = radius;
            return _card;
        }

        [TearDown]
        public void TearDown()
        {
            if (_casterGo != null) Object.DestroyImmediate(_casterGo);
            if (_card != null) Object.DestroyImmediate(_card);
        }

        [Test]
        public void Line_StartsOnTargetAndGoesAwayFromCaster()
        {
            // Ex. Éclat de rage : E . X X X (la case visée puis les 2 suivantes)
            Unit caster = NewCaster(new Vector2Int(2, 2));
            CardData card = NewCard(CardAreaEffect.Line, 3);
            var epicenter = new Vector2Int(4, 2);

            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(4, 2)), "case visée");
            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(5, 2)));
            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(6, 2)));
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(7, 2)), "au-delà de aoeRadius");
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(3, 2)), "entre le lanceur et la cible");
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(2, 2)), "le lanceur");
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(4, 3)), "à côté de la ligne (un cercle l'inclurait)");
        }

        [Test]
        public void Circle_CoversDiamondAroundEpicenter()
        {
            Unit caster = NewCaster(new Vector2Int(0, 0));
            CardData card = NewCard(CardAreaEffect.Circle, 1);
            var epicenter = new Vector2Int(5, 5);

            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(5, 6)));
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(6, 6)));
        }
    }
}
