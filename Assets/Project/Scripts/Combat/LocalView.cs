/// <summary>
/// Champion dont ce PC affiche l'interface personnelle (stats, orbe de vie, main, pioche et
/// défausse) : celui dont c'est le tour s'il joue sur ce PC ; sinon (réseau, tour d'un autre
/// PC ou des monstres) le champion du joueur de ce PC. En solo et en coop sur un seul PC,
/// c'est toujours le champion actif.
/// </summary>
public static class LocalView
{
    /// <summary>Champion joué sur ce PC.</summary>
    public static bool IsLocalChampion(Unit unit) =>
        unit is Champion champion && CombatParty.IsLocal(CombatParty.IndexOf(champion.championData));

    /// <summary>Champion à afficher, ou null si aucun (combat pas encore lancé).</summary>
    public static Champion ChampionToShow(Unit activeUnit)
    {
        if (IsLocalChampion(activeUnit)) return (Champion)activeUnit;
        if (!Services.IsGridServiceAvailable()) return null;

        foreach (Unit unit in Services.Grid.GetAllUnits())
        {
            if (IsLocalChampion(unit)) return (Champion)unit;
        }
        return null;
    }
}
