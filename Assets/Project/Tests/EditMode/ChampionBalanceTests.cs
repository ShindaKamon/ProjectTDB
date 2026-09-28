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
    }
}
