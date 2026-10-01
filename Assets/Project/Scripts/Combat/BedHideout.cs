/// <summary>
/// Règles du boss caché sous les lits (phase 1 du Monstre sous le lit, voir BedHiding) : répartition de
/// ses PV entre les lits et choix du lit suivant. Classe pure, testable.
/// </summary>
public static class BedHideout
{
    /// <summary>
    /// Répartit les PV du boss entre les lits : parts égales, le reste va aux premiers lits (la somme vaut total).
    /// </summary>
    public static int[] SplitHealth(int total, int bedCount)
    {
        var shares = new int[bedCount];
        for (int i = 0; i < bedCount; i++)
            shares[i] = total / bedCount + (i < total % bedCount ? 1 : 0);
        return shares;
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
