using NUnit.Framework;
using UnityEditor;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Règles de l'Excel sur les fiches de champions (MVP_Excel_Snapshot.md, « Progression
    /// champions ») : 100 PV au niveau 1, budget PA + PM = 9, au moins 3 PA et 2 PM. Évite qu'une
    /// valeur de test (ex. 10 000 PV) parte dans un commit.
    /// </summary>
    public class ChampionBalanceTests
    {
        [TestCase("Evan")]
        [TestCase("Crux")]
        [TestCase("Raze")]
        public void ChampionSheet_FollowsExcelProfile(string name)
        {
            var champion = AssetDatabase.LoadAssetAtPath<ChampionData>($"Assets/ScriptableObjects/Characters/Champion/{name}.asset");
            Assert.IsNotNull(champion, $"Fiche {name} introuvable");

            Assert.AreEqual(100, champion.maxHealth, "100 PV au niveau 1");
            Assert.AreEqual(9, champion.maxActionPoints + champion.movementRange, "budget PA + PM = 9");
            Assert.GreaterOrEqual(champion.maxActionPoints, 3, "au moins 3 PA");
            Assert.GreaterOrEqual(champion.movementRange, 2, "au moins 2 PM");
            Assert.IsFalse(string.IsNullOrEmpty(champion.title), "titre affiché à la sélection");
        }

        [Test]
        public void StatsAtLevel_GrowByFlooredAmountPerLevel()
        {
            var data = UnityEngine.ScriptableObject.CreateInstance<ChampionData>();
            data.attackDamage = 2;
            data.attackPerLevel = 0.5f;
            data.armor = 1;
            data.armorPerLevel = 0.25f;

            Assert.AreEqual(2, data.AttackAtLevel(1), "niveau 1 = valeur de la fiche");
            Assert.AreEqual(2, data.AttackAtLevel(2), "+0,5 arrondi à 0");
            Assert.AreEqual(3, data.AttackAtLevel(3));
            Assert.AreEqual(11, data.AttackAtLevel(20));
            Assert.AreEqual(1, data.ArmorAtLevel(4), "+0,75 arrondi à 0");
            Assert.AreEqual(2, data.ArmorAtLevel(5));
            Assert.AreEqual(0, data.MagicResistanceAtLevel(20), "gain nul par défaut");

            UnityEngine.Object.DestroyImmediate(data);
        }

        [TestCase("Evan")]
        [TestCase("Crux")]
        [TestCase("Raze")]
        public void ChampionSheet_HasNonNegativeAttackAndDefense(string name)
        {
            var champion = AssetDatabase.LoadAssetAtPath<ChampionData>($"Assets/ScriptableObjects/Characters/Champion/{name}.asset");

            Assert.GreaterOrEqual(champion.attackDamage, 0);
            Assert.GreaterOrEqual(champion.armor, 0);
            Assert.GreaterOrEqual(champion.magicResistance, 0);
            Assert.Greater(champion.attackPerLevel, 0f, "l'ATQ progresse avec le niveau");
        }
    }
}
