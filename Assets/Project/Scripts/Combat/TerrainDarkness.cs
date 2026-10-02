using UnityEngine;

/// <summary>
/// Terrain assombri par le Monstre sous le lit (Marée d'ombre, Frayeur ; voir Enemies.md) : tout le terrain est
/// dans l'ombre jusqu'au début du prochain tour du monstre qui l'a assombri. Sert au passif « Tapi dans le noir »
/// et au rendu (DarknessView). État du combat en cours, remis à zéro au lancement de chaque combat (GridManager).
/// </summary>
public static class TerrainDarkness
{
    /// <summary>Monstre qui a assombri le terrain (null = terrain normal).</summary>
    public static Unit DarkenedBy { get; private set; }

    public static bool IsDark => DarkenedBy != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData() => DarkenedBy = null;

    public static void Darken(Unit source)
    {
        if (source == null) return;
        bool changed = !IsDark;
        DarkenedBy = source;
        GameLog.Log($"{source.name} assombrit le terrain jusqu'à son prochain tour");
        if (changed) EventBus.Publish(new TerrainDarknessChangedEvent(true));
    }

    /// <summary>Début du tour d'une unité : l'ombre qu'elle a posée se dissipe.</summary>
    public static void OnTurnStart(Unit unit)
    {
        if (unit == null || unit != DarkenedBy) return;
        Clear();
    }

    public static void Clear()
    {
        if (!IsDark) return;
        DarkenedBy = null;
        EventBus.Publish(new TerrainDarknessChangedEvent(false));
    }
}
