using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Structure représentant un debuff de ressources (PA/PM) sur une unité
/// </summary>
public struct ResourceDebuff
{
    public int paReduction;
    public int pmReduction;
    public int remainingTurns;
    public Unit appliedBy;

    public ResourceDebuff(int pa, int pm, int duration, Unit source)
    {
        paReduction = pa;
        pmReduction = pm;
        remainingTurns = duration;
        appliedBy = source;
    }
}

/// <summary>
/// Gestionnaire des debuffs de ressources (PA/PM).
/// Gère les réductions temporaires qui s'appliquent au début de chaque tour.
/// </summary>
public static class ResourceDebuffManager
{
    // Liste des debuffs actifs par unité
    private static Dictionary<Unit, List<ResourceDebuff>> _activeDebuffs = new Dictionary<Unit, List<ResourceDebuff>>();

    /// <summary>
    /// Réinitialise le gestionnaire (appelé automatiquement au lancement du jeu)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        _activeDebuffs = new Dictionary<Unit, List<ResourceDebuff>>();
    }

    /// <summary>
    /// Applique un debuff de ressources sur une cible
    /// </summary>
    /// <param name="target">L'unité ciblée</param>
    /// <param name="paReduction">PA à retirer par tour</param>
    /// <param name="pmReduction">PM à retirer par tour</param>
    /// <param name="duration">Durée en tours (0 = effet immédiat uniquement)</param>
    /// <param name="source">L'unité qui applique le debuff</param>
    public static void ApplyDebuff(Unit target, int paReduction, int pmReduction, int duration, Unit source)
    {
        if (target == null) return;
        if (paReduction <= 0 && pmReduction <= 0) return;

        // Applique l'effet immédiat
        ApplyImmediateEffect(target, paReduction, pmReduction, source);

        // Si duration > 0, enregistre le debuff pour les tours suivants
        if (duration > 0)
        {
            ResourceDebuff debuff = new ResourceDebuff(paReduction, pmReduction, duration, source);

            if (!_activeDebuffs.ContainsKey(target))
            {
                _activeDebuffs[target] = new List<ResourceDebuff>();
            }

            _activeDebuffs[target].Add(debuff);
            Debug.Log($"[Debuff] {target.name} reçoit un debuff de ressources pour {duration} tour(s) (PA: -{paReduction}, PM: -{pmReduction})");
        }
    }

    /// <summary>
    /// Applique l'effet immédiat de réduction de PA/PM
    /// </summary>
    private static void ApplyImmediateEffect(Unit target, int paReduction, int pmReduction, Unit source)
    {
        // Réduction de PA
        if (paReduction > 0 && target is IActionPointsUser paUser)
        {
            paUser.ReduceCurrentPA(paReduction);
            Debug.Log($"⚡ {source?.name ?? "Effet"} retire {paReduction} PA à {target.name}");
        }

        // Réduction de PM
        if (pmReduction > 0)
        {
            target.SpendMovement(pmReduction);
            Debug.Log($"⚡ {source?.name ?? "Effet"} retire {pmReduction} PM à {target.name}");
        }
    }

    /// <summary>
    /// Appelé au début du tour d'une unité pour appliquer les debuffs actifs
    /// </summary>
    /// <param name="unit">L'unité dont c'est le tour</param>
    public static void ProcessDebuffsOnTurnStart(Unit unit)
    {
        if (unit == null) return;

        if (!_activeDebuffs.TryGetValue(unit, out List<ResourceDebuff> debuffs))
        {
            return;
        }

        // Parcourt les debuffs en sens inverse pour pouvoir supprimer pendant l'itération
        for (int i = debuffs.Count - 1; i >= 0; i--)
        {
            ResourceDebuff debuff = debuffs[i];

            // Applique l'effet
            ApplyImmediateEffect(unit, debuff.paReduction, debuff.pmReduction, debuff.appliedBy);

            // Décrémente la durée
            debuff.remainingTurns--;

            if (debuff.remainingTurns <= 0)
            {
                // Le debuff expire
                debuffs.RemoveAt(i);
                Debug.Log($"[Debuff] Debuff de ressources expiré sur {unit.name}");
            }
            else
            {
                // Met à jour le debuff
                debuffs[i] = debuff;
            }
        }

        // Nettoie si plus de debuffs
        if (debuffs.Count == 0)
        {
            _activeDebuffs.Remove(unit);
        }
    }

    /// <summary>
    /// Vérifie si une unité a des debuffs de ressources actifs
    /// </summary>
    public static bool HasActiveDebuffs(Unit unit)
    {
        return unit != null && _activeDebuffs.ContainsKey(unit) && _activeDebuffs[unit].Count > 0;
    }

    /// <summary>
    /// Retourne le nombre de debuffs actifs sur une unité
    /// </summary>
    public static int GetDebuffCount(Unit unit)
    {
        if (unit == null || !_activeDebuffs.ContainsKey(unit))
        {
            return 0;
        }
        return _activeDebuffs[unit].Count;
    }

    /// <summary>
    /// Retire tous les debuffs d'une unité (appelé quand l'unité meurt)
    /// </summary>
    public static void ClearDebuffs(Unit unit)
    {
        if (unit != null)
        {
            _activeDebuffs.Remove(unit);
        }
    }

    /// <summary>
    /// Retire tous les debuffs appliqués par une source spécifique (appelé quand la source meurt)
    /// </summary>
    public static void ClearDebuffsFromSource(Unit source)
    {
        if (source == null) return;

        foreach (var kvp in _activeDebuffs)
        {
            kvp.Value.RemoveAll(d => d.appliedBy == source);
        }

        // Nettoie les entrées vides
        List<Unit> emptyKeys = new List<Unit>();
        foreach (var kvp in _activeDebuffs)
        {
            if (kvp.Value.Count == 0)
            {
                emptyKeys.Add(kvp.Key);
            }
        }
        foreach (Unit key in emptyKeys)
        {
            _activeDebuffs.Remove(key);
        }
    }
}
