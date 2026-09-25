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
        public void RulesText_DamageTargetZone()
        {
            var card = NewCard(CardTargetType.Enemy, 1);
            card.damageAmount = 33;
            card.areaEffect = CardAreaEffect.Circle;
            card.aoeRadius = 1;
            card.affectedTarget = CardAffectedTarget.Enemies;

            string text = CardRulesText.Build(card);

            StringAssert.Contains("<sprite name=\"dmg\"", text);
            StringAssert.Contains("Inflige 33", text);
            StringAssert.Contains("<b>Cible :</b> 1 ennemi · au contact", text);
            StringAssert.Contains("<b>Zone :</b> cercle de 1 (ennemis)", text);
        }

        [Test]
        public void RulesText_ShieldPushSelfDamageAndSpecial()
        {
            var card = NewCard(CardTargetType.AllyorEnemy, 3);
            card.defenseAmount = 10;
            card.knockbackDistance = 2;
            card.pullsTowardCaster = true;
            card.damageSelf = 5;
            card.specialText = "Règle maison.";

            string text = CardRulesText.Build(card);

            StringAssert.Contains("Bouclier 10", text);
            StringAssert.Contains("<sprite name=\"pull\"", text);
            StringAssert.Contains("Tire de 2 cases", text);
            StringAssert.Contains("portée 1-3", text);
            StringAssert.Contains("Contrecoup : tu subis 5", text);
            StringAssert.Contains("<i>Spécial :</i> Règle maison.", text);
        }

        [Test]
        public void RulesText_SelfCardWithZone_SaysAroundYou()
        {
            var card = NewCard(CardTargetType.Self, 0);
            card.armorAmount = -7;
            card.effectDuration = 1;
            card.areaEffect = CardAreaEffect.Circle;
            card.aoeRadius = 1;
            card.affectedTarget = CardAffectedTarget.Enemies;

            string text = CardRulesText.Build(card);

            StringAssert.Contains("Armure −7 (1 tour)", text);
            StringAssert.DoesNotContain("Cible", text);
            StringAssert.Contains("<b>Zone :</b> autour de toi, cercle de 1 (ennemis)", text);
        }

        [Test]
        public void RulesText_MultiTargetAndMagicDamage()
        {
            var card = NewCard(CardTargetType.Enemy, 3);
            card.damageAmount = 16;
            card.damageType = DamageType.Magique;
            card.targetCount = 2;

            string text = CardRulesText.Build(card);

            StringAssert.Contains("<sprite name=\"magic\"", text);
            StringAssert.Contains("Inflige 16 (magique)", text);
            StringAssert.Contains("2 ennemis distincts · portée 1-3", text);
        }
    }
}
