/// <summary>
/// Règles du boss caché sous les lits (phase 1 du Monstre sous le lit, voir BedHiding) : PV des lits et
/// choix du lit suivant. Classe pure, testable.
/// </summary>
public static class BedHideout
{
    /// <summary>
    /// PV de chaque lit : casser tous les lits sauf un vide la barre de la phase 1 (le dernier devient le Lit
    /// de la phase 2). Arrondi au-dessus, pour que ces lits suffisent toujours.
    /// </summary>
    public static int BedHealth(int phaseHealth, int bedCount)
    {
        if (bedCount <= 1) return phaseHealth;
        return (phaseHealth + bedCount - 2) / (bedCount - 1);
    }

    /// <summary>
    /// Index du lit où le boss va se cacher : un autre que le lit actuel, tiré au hasard (le même s'il est seul).
    /// </summary>
    public static int NextBed(System.Random rng, int bedCount, int currentIndex)
    {
        if (bedCount <= 1) return 0;
        int next = rng.Next(bedCount - 1);
        return next >= currentIndex ? next + 1 : next;
    }
}
