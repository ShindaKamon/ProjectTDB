using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Contenu visuel des cartes façon codex émotionnel (schéma de portée, légende, pastilles).
    /// </summary>
    public class CodexCardVisualTests
    {
        private readonly List<Object> _created = new List<Object>();

        private CardData NewCard(CardTargetType target, int range, CardAreaEffect area = CardAreaEffect.None, int radius = 0)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Test";
            card.targetType = target;
            card.targetRange = range;
            card.areaEffect = area;
            card.aoeRadius = radius;
            _created.Add(card);
            return card;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        private static int Count(DiagramCell[,] cells, DiagramCell kind)
        {
            int n = 0;
            foreach (var c in cells) if (c == kind) n++;
            return n;
        }

        [Test]
        public void Melee_SingleTarget_CasterAtLeft_OneEnemyCellAdjacent()
        {
            var cells = CodexCardVisual.BuildDiagram(NewCard(CardTargetType.Enemy, 1));

            Assert.AreEqual(DiagramCell.Caster, cells[1, 4]);
            Assert.AreEqual(DiagramCell.Area, cells[2, 4]);
            Assert.AreEqual(1, Count(cells, DiagramCell.Area));
        }

        [Test]
        public void Circle_Radius1_DrawsDiamondOf5Cells()
        {
            var card = NewCard(CardTargetType.Enemy, 1, CardAreaEffect.Circle, 1);
            var cells = CodexCardVisual.BuildDiagram(card);

            // Au contact, le losange de 5 cases recouvre la case du lanceur, dessiné par-dessus
            Assert.AreEqual(4, Count(cells, DiagramCell.Area));
            Assert.AreEqual(DiagramCell.Caster, cells[1, 4]);
            Assert.AreEqual("au contact · zone rayon 1", CodexCardVisual.Caption(card));
        }

        [Test]
        public void SelfCard_CasterInCenter_NoRange()
        {
            var card = NewCard(CardTargetType.Self, 0);
            var cells = CodexCardVisual.BuildDiagram(card);

            Assert.AreEqual(DiagramCell.Caster, cells[4, 4]);
            Assert.AreEqual(0, Count(cells, DiagramCell.Range));
            Assert.AreEqual("sur soi", CodexCardVisual.Caption(card));
        }

        [Test]
        public void Range3_ShowsRangeCells_AndCaption()
        {
            var card = NewCard(CardTargetType.Enemy, 3);

            Assert.Greater(Count(CodexCardVisual.BuildDiagram(card), DiagramCell.Range), 0);
            Assert.AreEqual("portée 1-3", CodexCardVisual.Caption(card));
        }

        [Test]
        public void AllyCard_ZoneIsAllyColored()
        {
            var cells = CodexCardVisual.BuildDiagram(NewCard(CardTargetType.Ally, 3));

            Assert.AreEqual(1, Count(cells, DiagramCell.AreaAlly));
            Assert.AreEqual(0, Count(cells, DiagramCell.Area));
        }

        [Test]
        public void Chips_DamageShieldPushAndSelfDamage()
        {
            var card = NewCard(CardTargetType.Enemy, 1);
            card.damageAmount = 33;
            card.defenseAmount = 10;
            card.knockbackDistance = 2;
            card.damageSelf = 5;

            var chips = CodexCardVisual.Chips(card);

            Assert.AreEqual("dmg", chips[0].Icon);
            Assert.AreEqual("33", chips[0].Text);
            Assert.IsTrue(chips.Exists(c => c.Icon == "shield" && c.Text == "10"));
            Assert.IsTrue(chips.Exists(c => c.Icon == "push" && c.Text == "2"));
            Assert.IsTrue(chips.Exists(c => c.Icon == "self" && c.Kind == ChipKind.Warn));
        }

        [Test]
        public void Chips_PullWhenPullsTowardCaster()
        {
            var card = NewCard(CardTargetType.AllyorEnemy, 3);
            card.knockbackDistance = 2;
            card.pullsTowardCaster = true;

            Assert.IsTrue(CodexCardVisual.Chips(card).Exists(c => c.Icon == "pull"));
        }
    }
}
