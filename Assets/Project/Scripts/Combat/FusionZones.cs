using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zones posées par une fusion (ex. Appât d'Evan) : tant qu'elles durent, les ennemis du poseur qui
/// commencent leur tour dedans perdent des PM. Classe statique pure ; la fusion retire ses zones à sa fin.
/// </summary>
public static class FusionZones
{
    private struct Zone
    {
        public Vector2Int Center;
        public int Radius;
        public int PmLoss;
        public Unit Owner;
    }

    private static readonly List<Zone> _zones = new List<Zone>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => _zones.Clear();

    public static int Count => _zones.Count;

    public static void Add(Vector2Int center, int radius, int pmLoss, Unit owner)
    {
        _zones.Add(new Zone { Center = center, Radius = radius, PmLoss = pmLoss, Owner = owner });
    }

    public static void Clear(Unit owner) => _zones.RemoveAll(z => z.Owner == owner);

    /// <summary>Début du tour d'une unité : si elle est dans une zone ennemie, elle perd des PM (une seule zone compte).</summary>
    public static void ApplyOnTurnStart(Unit unit)
    {
        if (unit == null || _zones.Count == 0) return;

        int loss = 0;
        Unit source = null;
        foreach (Zone zone in _zones)
        {
            if (zone.Owner == null || zone.Owner.GetFaction() == unit.GetFaction()) continue;
            if (GridGeometry.Distance(zone.Center, unit.GetCurrentGridPos()) > zone.Radius) continue;
            if (zone.PmLoss > loss)
            {
                loss = zone.PmLoss;
                source = zone.Owner;
            }
        }

        if (loss > 0) ResourceDebuffManager.ApplyDebuff(unit, 0, loss, source);
    }

    public static string Describe()
    {
        var parts = new List<string>();
        foreach (Zone zone in _zones) parts.Add($"{zone.Center}r{zone.Radius}-{zone.PmLoss}");
        return string.Join(",", parts);
    }
}
