using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Retraits de PA/PM (règle du GDD, Combat_System.md) : appliqués au début du prochain tour de la
/// cible, une seule fois. Ils ne se cumulent pas : un retrait plus fort remplace un plus faible,
/// un plus faible n'écrase jamais un plus fort en attente (PA et PM comptés séparément).
/// Ténacité (monstres) : après une perte totale de PM, un monstre ignore les retraits de PM à son
/// tour suivant, pour qu'on ne puisse pas l'immobiliser indéfiniment (même à plusieurs joueurs).
/// </summary>
public static class ResourceDebuffManager
{
    // Retraits en attente par unité : (PA, PM)
    private static Dictionary<Unit, (int pa, int pm)> _pending = new Dictionary<Unit, (int pa, int pm)>();

    // Monstres immunisés aux retraits de PM à leur prochain tour (Ténacité)
    private static HashSet<Unit> _pmImmune = new HashSet<Unit>();

    /// <summary>
    /// Réinitialise le gestionnaire (appelé automatiquement au lancement du jeu)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        _pending = new Dictionary<Unit, (int pa, int pm)>();
        _pmImmune = new HashSet<Unit>();
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
        GameLog.Log($"[Retrait] {source?.name ?? "Effet"} : {target.name} perdra {_pending[target].pa} PA / {(IsPmImmune(target) ? "0 (Ténacité)" : _pending[target].pm.ToString())} PM à son prochain tour");
        EventBus.Publish(new ResourceDebuffChangedEvent(target));
    }

    /// <summary>
    /// Retraits en attente contre une unité (0, 0 si aucun) : ce qu'elle perdra à son prochain tour
    /// (retrait de PM ignoré si elle est protégée par la Ténacité)
    /// </summary>
    public static (int pa, int pm) GetPending(Unit unit)
    {
        if (unit == null || !_pending.TryGetValue(unit, out var debuff)) return (0, 0);
        return (debuff.pa, IsPmImmune(unit) ? 0 : debuff.pm);
    }

    /// <summary>
    /// True si le monstre ignore les retraits de PM jusqu'à la fin de son prochain tour (Ténacité)
    /// </summary>
    public static bool IsPmImmune(Unit unit) => unit != null && _pmImmune.Contains(unit);

    /// <summary>
    /// Appelé au début du tour d'une unité, après la remise à niveau de ses PA/PM
    /// </summary>
    public static void ProcessDebuffsOnTurnStart(Unit unit)
    {
        if (unit == null) return;

        // La Ténacité couvre ce tour-ci : les retraits de PM en attente sont ignorés, puis elle s'arrête
        bool immune = _pmImmune.Remove(unit);

        if (!_pending.TryGetValue(unit, out var debuff)) return;
        _pending.Remove(unit);

        if (debuff.pa > 0 && unit is IActionPointsUser paUser)
        {
            paUser.ReduceCurrentPA(debuff.pa);
            GameLog.Log($"⚡ {unit.name} perd {debuff.pa} PA ce tour");
        }

        if (debuff.pm > 0 && immune)
        {
            GameLog.Log($"🛡 {unit.name} ignore le retrait de {debuff.pm} PM (Ténacité)");
        }
        else if (debuff.pm > 0)
        {
            // Un monstre qui perd tous ses PM devient tenace pour son tour suivant (marqué avant le
            // retrait, pour que l'affichage qui réagit au changement de PM le voie déjà)
            bool losesAll = debuff.pm >= unit.GetCurrentMovementPoints();
            if (losesAll && unit.GetFaction() == Unit.UnitFaction.Enemy)
            {
                _pmImmune.Add(unit);
                GameLog.Log($"🛡 {unit.name} est tenace : retraits de PM ignorés à son prochain tour");
            }

            unit.SpendMovement(debuff.pm);
            GameLog.Log($"⚡ {unit.name} perd {(losesAll ? "tous ses" : debuff.pm.ToString())} PM ce tour");
        }
    }
}
