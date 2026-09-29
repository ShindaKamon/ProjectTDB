using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Commandes de combat : texte réseau aller-retour, sans perte.
    /// </summary>
    public class CombatCommandTests
    {
        private static void AssertSame(CombatCommand expected, CombatCommand actual)
        {
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.Actor, actual.Actor);
            Assert.AreEqual(expected.CardName, actual.CardName);
            CollectionAssert.AreEqual(expected.Tiles, actual.Tiles);
            Assert.AreEqual(expected.TargetCardName, actual.TargetCardName);
            Assert.AreEqual(expected.Delta, actual.Delta);
            Assert.AreEqual(expected.Turn, actual.Turn);
        }

        [Test]
        public void MultiTargetCard_RoundTrip()
        {
            var command = CombatCommand.PlayCard(1, "Frappe rapide", new Vector2Int(3, 4), new Vector2Int(0, 9));
            AssertSame(command, CombatCommand.Deserialize(command.Serialize()));
        }

        [Test]
        public void CardWithoutTarget_RoundTrip()
        {
            var command = CombatCommand.PlayCard(0, "Sang pour sang");
            AssertSame(command, CombatCommand.Deserialize(command.Serialize()));
        }

        [Test]
        public void Triche_RoundTrip_KeepsNegativeDelta()
        {
            var command = CombatCommand.ChangeHandCardCost(2, "Triche", "Élan de joie, l'été", -1);
            command.Turn = 17;
            AssertSame(command, CombatCommand.Deserialize(command.Serialize()));
        }

        [Test]
        public void MoveEndTurnAndPlacement_RoundTrip()
        {
            AssertSame(CombatCommand.Move(0, new Vector2Int(9, 0)), CombatCommand.Deserialize(CombatCommand.Move(0, new Vector2Int(9, 0)).Serialize()));
            AssertSame(CombatCommand.EndTurn(1), CombatCommand.Deserialize(CombatCommand.EndTurn(1).Serialize()));
            AssertSame(CombatCommand.PlacementNext(2), CombatCommand.Deserialize(CombatCommand.PlacementNext(2).Serialize()));
        }
    }
}
