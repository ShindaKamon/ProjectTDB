using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 1 du Monstre sous le lit (voir Enemies.md) : le boss est caché sous un des lits (invisible et ignoré par
/// la grille, seule son ombre dépasse), change de lit au début de chacun de ses tours et frappe depuis là.
/// Ses PV sont ceux des lits (somme affichée par sa barre de boss) ; le lit où il se trouve résiste.
/// Quand il ne reste qu'un lit, le lit cède et le boss en sort avec les PV de ce lit (en attendant les phases 2 et 3).
/// Ajouté au boss par GridManager quand la rencontre a des lits.
/// </summary>
public class BedHiding : MonoBehaviour
{
    private Enemy _boss;
    private readonly List<BedUnit> _beds = new List<BedUnit>();
    private BedUnit _current;

    public bool IsHiding => _current != null;

    /// <summary>Lit sous lequel le boss est caché (null une fois sorti).</summary>
    public BedUnit CurrentBed => _current;

    public void Begin(Enemy boss, List<BedUnit> beds)
    {
        _boss = boss;
        _beds.AddRange(beds);
        int[] shares = BedHideout.SplitHealth(boss.GetMaxHealth(), beds.Count);
        for (int i = 0; i < beds.Count; i++) beds[i].SetMaxHealth(shares[i]);

        SetVisible(false);
        MoveTo(_beds[boss.Rng.Next(_beds.Count)]);
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Subscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
        GameLog.Log($"{boss.name} se cache sous l'un des {beds.Count} lits ({boss.GetMaxHealth()} PV répartis)");
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    /// <summary>Début du tour du boss : il passe sous un autre lit.</summary>
    public void MoveToNextBed()
    {
        if (!IsHiding) return;
        MoveTo(_beds[BedHideout.NextBed(_boss.Rng, _beds.Count, _beds.IndexOf(_current))]);
    }

    private void MoveTo(BedUnit bed)
    {
        if (_current != null) _current.Occupied = false;
        _current = bed;
        bed.Occupied = true;
        _boss.TeleportTo(bed.GetCurrentGridPos());
        GameLog.Log($"{_boss.name} se cache sous le lit {bed.GetCurrentGridPos()}");
    }

    private void OnUnitDamaged(UnitDamagedEvent e) { if (e.Target is BedUnit) SyncBossHealth(); }

    private void OnUnitHealed(UnitHealedEvent e) { if (e.Target is BedUnit) SyncBossHealth(); }

    private void OnUnitDied(UnitDiedEvent e)
    {
        if (!(e.DeadUnit is BedUnit bed) || !_beds.Remove(bed) || !IsHiding) return;
        if (_beds.Count == 1) Emerge();
        else SyncBossHealth();
    }

    // PV du boss = somme des PV des lits restants
    private void SyncBossHealth()
    {
        int total = 0;
        foreach (BedUnit bed in _beds) if (bed != null) total += bed.GetHealth();
        _boss.SetCurrentHealth(total);
    }

    // Il ne reste que son lit : le lit cède, le boss sort sur sa case avec les PV du lit
    private void Emerge()
    {
        BedUnit last = _beds[0];
        int health = last.GetHealth();
        Vector2Int cell = last.GetCurrentGridPos();
        _current = null;
        _beds.Clear();
        last.Collapse();

        SetVisible(true);
        _boss.TeleportTo(cell);
        _boss.SetCurrentHealth(health);
        GameLog.Log($"{_boss.name} sort de sous le dernier lit ({health} PV)");
    }

    private void SetVisible(bool visible)
    {
        _boss.IsHidden = !visible;
        foreach (Renderer renderer in _boss.GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
        foreach (Collider collider in _boss.GetComponentsInChildren<Collider>(true)) collider.enabled = visible; // les clics vont au lit
    }
}
