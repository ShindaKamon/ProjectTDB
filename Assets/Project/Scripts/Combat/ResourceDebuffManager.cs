using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Retraits de PA/PM (règle du GDD, Combat_System.md) : appliqués au début du prochain tour de la
/// cible, une seule fois. Ils ne se cumulent pas : un retrait plus fort remplace un plus faible,
/// un plus faible n'écrase jamais un plus fort en attente (PA et PM comptés séparément).
/// </summary>
public static class ResourceDebuffManager
{
    // Retraits en attente par unité : (PA, PM)
    private static Dictionary<Unit, (int pa, int pm)> _pending = new Dictionary<Unit, (int pa, int pm)>();

    /// <summary>
    /// Réinitialise le gestionnaire (appelé automatiquement au lancement du jeu)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        _pending = new Dictionary<Unit, (int pa, int pm)>();
    }

    /// <summary>
    /// Programme un retrait de PA/PM pour le prochain tour de la cible
    /// </summary>
    public static void ApplyDebuff(Unit target, int paReduction, int pmReduction, Unit source)
    {
        if (target == null) return;
        if (paReduction <= 0 && pmReduction <= 0) return;

        _pending.TryGetValue(target, out var current);
        _pending[target] = (Mathf.Max(current.pa, paReduction), Mathf.Max(current.pm, pmReduction));
        GameLog.Log($"[Retrait] {source?.name ?? "Effet"} : {target.name} perdra {_pending[target].pa} PA / {_pending[target].pm} PM à son prochain tour");
        EventBus.Publish(new ResourceDebuffChangedEvent(target));
    }

    /// <summary>
    /// Retraits en attente contre une unité (0, 0 si aucun) : ce qu'elle perdra à son prochain tour
    /// </summary>
    public static (int pa, int pm) GetPending(Unit unit) =>
        unit != null && _pending.TryGetValue(unit, out var debuff) ? debuff : (0, 0);

    /// <summary>
    /// Appelé au début du tour d'une unité, après la remise à niveau de ses PA/PM
    /// </summary>
    public static void ProcessDebuffsOnTurnStart(Unit unit)
    {
        if (unit == null || !_pending.TryGetValue(unit, out var debuff)) return;
        _pending.Remove(unit);

        if (debuff.pa > 0 && unit is IActionPointsUser paUser)
        {
            paUser.ReduceCurrentPA(debuff.pa);
            GameLog.Log($"⚡ {unit.name} perd {debuff.pa} PA ce tour");
        }

        if (debuff.pm > 0)
        {
            unit.SpendMovement(debuff.pm);
            GameLog.Log($"⚡ {unit.name} perd {debuff.pm} PM ce tour");
        }
    }
}
