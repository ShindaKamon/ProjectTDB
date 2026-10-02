using System.Collections.Generic;

/// <summary>
/// Bilan d'une expédition dans un donjon, affiché à l'écran de fin de donjon : combats gagnés, et dégâts et soins de
/// chaque allié (champions et invocations) cumulés sur tous les combats, par nom. C# pur : testable en EditMode.
/// </summary>
public class DungeonRunStats
{
    private readonly List<CombatStats.Entry> _allies = new List<CombatStats.Entry>();

    public int CombatsWon { get; private set; }

    /// <summary>Alliés de l'expédition, dans l'ordre de leur première apparition.</summary>
    public List<CombatStats.Entry> Allies => new List<CombatStats.Entry>(_allies);

    /// <summary>Combat gagné : ajoute le récapitulatif de ses alliés au bilan.</summary>
    public void AddCombat(IEnumerable<CombatStats.Entry> allies)
    {
        CombatsWon++;
        foreach (CombatStats.Entry entry in allies)
        {
            CombatStats.Entry total = _allies.Find(e => e.Name == entry.Name);
            if (total == null)
            {
                total = new CombatStats.Entry { Name = entry.Name, IsAlly = true };
                _allies.Add(total);
            }
            total.Damage += entry.Damage;
            total.Healing += entry.Healing;
        }
    }
}
