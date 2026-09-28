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
        public void Cone_StartsOnTarget_Widens1_3_5()
        {
            // Lanceur en (2,2), case visée (2,4), cône de 3 rangées vers le haut : 1 + 3 + 5 = 9 cases
            Unit caster = NewCaster(new Vector2Int(2, 2));
            CardData card = NewCard(CardAreaEffect.Cone, 3);
            var epicenter = new Vector2Int(2, 4);

            int covered = 0;
            for (int x = -5; x <= 10; x++)
                for (int y = -5; y <= 10; y++)
                    if (card.IsInAOEShape(caster, epicenter, new Vector2Int(x, y))) covered++;
            Assert.AreEqual(9, covered);

            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(2, 4)), "rangée 1 : la case visée");
            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(1, 5)), "rangée 2 : 3 cases");
            Assert.IsTrue(card.IsInAOEShape(caster, epicenter, new Vector2Int(0, 6)), "rangée 3 : 5 cases");
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(1, 4)), "pas à côté de la case visée");
            Assert.IsFalse(card.IsInAOEShape(caster, epicenter, new Vector2Int(2, 3)), "pas entre le lanceur et la cible");
        }

        [Test]
        public void WholeTeam_IsAreaEvenWithoutRadius_AndSelfCardIsNotMultiTarget()
        {
            // Communion joyeuse : sur soi, toute l'équipe, rayon 0, « 4 cibles » dans ses données
            CardData card = NewCard(CardAreaEffect.WholeTeam, 0);
            card.targetType = CardTargetType.Self;
            card.targetCount = 4;

            Assert.IsTrue(card.isAOE, "toute l'équipe est une zone, sans rayon");
            Assert.IsFalse(card.isMultiTarget, "une carte sur soi ne passe pas par le ciblage à cibles multiples");
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
