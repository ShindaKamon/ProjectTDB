using System.Collections.Generic;

/// <summary>
/// Récapitulatif d'un combat : dégâts infligés et soins donnés par chaque unité, séparés en alliés
/// (champions et invocations) et ennemis. Nom et camp sont retenus au premier enregistrement, car
/// une unité morte peut être détruite avant la fin du combat. C# pur : testable en EditMode.
/// </summary>
public class CombatStats
{
    public class Entry
    {
        public string Name;
        public bool IsAlly;
        public int Damage;
        public int Healing;
    }

    private readonly Dictionary<Unit, Entry> _entries = new Dictionary<Unit, Entry>();
    private readonly List<Entry> _order = new List<Entry>();

    /// <summary>Dégâts infligés par source à une cible (ignorés sans source ou sur soi-même, ex. contrecoup).</summary>
    public void RecordDamage(Unit source, Unit target, int amount)
    {
        if (source == null || source == target || amount <= 0) return;
        EntryOf(source).Damage += amount;
    }

    /// <summary>Soins donnés par source (soin sur soi compris, ex. vol de vie).</summary>
    public void RecordHealing(Unit source, int amount)
    {
        if (source == null || amount <= 0) return;
        EntryOf(source).Healing += amount;
    }

    /// <summary>
    /// Unité déjà présente (ex. un champion qui n'a encore rien fait), pour qu'elle figure au tableau ; sauf un
    /// élément du décor (Unit.ShownInCombatSummary, ex. les lits du boss).
    /// </summary>
    public void Register(Unit unit)
    {
        if (unit != null && unit.ShownInCombatSummary) EntryOf(unit);
    }

    public List<Entry> Allies => _order.FindAll(e => e.IsAlly);
    public List<Entry> Enemies => _order.FindAll(e => !e.IsAlly);

    private Entry EntryOf(Unit unit)
    {
        if (!_entries.TryGetValue(unit, out Entry entry))
        {
            entry = new Entry { Name = unit.DisplayName, IsAlly = unit.GetFaction() == Unit.UnitFaction.Player };
            _entries[unit] = entry;
            _order.Add(entry);
        }
        return entry;
    }
}
