using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestionnaire des marques Stigmate.
/// Gère la limite de 3 ennemis marqués par champion et les effets de consommation.
/// </summary>
public static class StigmateManager
{
    /// <summary>
    /// Nombre maximum d'ennemis pouvant être marqués simultanément par un champion
    /// </summary>
    public const int MAX_STIGMATE_TARGETS = 3;

    /// <summary>
    /// Quantité de PA retirée à l'ennemi lors de la consommation
    /// </summary>
    public const int PA_LOSS_ON_CONSUME = 1;

    // Liste des unités qui doivent perdre des PA à leur prochain tour
    private static Dictionary<Unit, int> _pendingPALoss = new Dictionary<Unit, int>();

    /// <summary>
    /// Réinitialise le gestionnaire (appelé automatiquement au lancement du jeu)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        _pendingPALoss = new Dictionary<Unit, int>();
    }

    /// <summary>
    /// Applique un Stigmate sur une cible, en respectant la limite de 3 marques.
    /// Si le champion a déjà 3 cibles marquées, la plus ancienne perd sa marque.
    /// </summary>
    /// <param name="source">Le champion qui applique le Stigmate</param>
    /// <param name="target">L'ennemi à marquer</param>
    /// <param name="healPerMark">Valeur de heal stockée dans la marque (pour la consommation)</param>
    public static void ApplyStigmate(Unit source, Unit target, int healPerMark = 0)
    {
        if (source == null || target == null) return;

        // Vérifie si la cible a déjà un Stigmate de cette source
        if (target.HasMarkFromSource(MarkType.Stigmate, source))
        {
            Debug.Log($"[Stigmate] {target.name} a déjà un Stigmate de {source.name}");
            return;
        }

        // Récupère toutes les unités avec un Stigmate de cette source
        List<Unit> markedUnits = GetUnitsMarkedBySource(source);

        // Si on a atteint la limite, retire la marque la plus ancienne
        if (markedUnits.Count >= MAX_STIGMATE_TARGETS)
        {
            Unit oldestMarked = markedUnits[0]; // La première est la plus ancienne
            oldestMarked.RemoveMark(MarkType.Stigmate);
            Debug.Log($"[Stigmate] Limite atteinte ! {oldestMarked.name} perd son Stigmate (remplacé par {target.name})");
        }

        // Applique le nouveau Stigmate (permanent = duration 0, pas de stacks = 1)
        target.ApplyMark(MarkType.Stigmate, source, 1, 0, healPerMark);
        Debug.Log($"[Stigmate] {source.name} marque {target.name} avec Stigmate (heal bonus: {healPerMark})");
    }

    /// <summary>
    /// Consomme tous les Stigmates appliqués par un champion.
    /// Effet : Heal le champion par ennemi marqué, chaque ennemi perd 1 PA au tour suivant.
    /// </summary>
    /// <param name="source">Le champion qui consomme ses Stigmates</param>
    /// <param name="baseHealPerMark">Heal de base par ennemi marqué</param>
    /// <returns>Le nombre d'ennemis dont le Stigmate a été consommé</returns>
    public static int ConsumeAllStigmates(Unit source, int baseHealPerMark = 0)
    {
        if (source == null) return 0;

        List<Unit> markedUnits = GetUnitsMarkedBySource(source);
        int consumedCount = 0;
        int totalHeal = 0;

        foreach (Unit markedUnit in markedUnits)
        {
            // Consomme la marque
            UnitMark consumedMark = markedUnit.ConsumeMarkFromSource(MarkType.Stigmate, source);

            if (consumedMark.markType != MarkType.None)
            {
                consumedCount++;

                // Calcule le heal (base + bonus stocké dans la marque)
                int healAmount = baseHealPerMark + consumedMark.bonusValue;
                totalHeal += healAmount;

                // Enregistre la perte de PA pour le prochain tour de l'ennemi
                RegisterPALoss(markedUnit, PA_LOSS_ON_CONSUME);

                Debug.Log($"[Stigmate] Consommé sur {markedUnit.name} - Heal: {healAmount}, PA -{PA_LOSS_ON_CONSUME} au prochain tour");
            }
        }

        // Soigne le champion
        if (totalHeal > 0)
        {
            source.Heal(totalHeal);
            Debug.Log($"[Stigmate] {source.name} récupère {totalHeal} PV total ({consumedCount} Stigmate(s) consommé(s))");
        }

        return consumedCount;
    }

    /// <summary>
    /// Enregistre une perte de PA en attente pour une unité (version publique pour AllMarks)
    /// </summary>
    public static void RegisterPALossPublic(Unit target, int amount)
    {
        RegisterPALoss(target, amount);
    }

    /// <summary>
    /// Enregistre une perte de PA en attente pour une unité
    /// </summary>
    private static void RegisterPALoss(Unit target, int amount)
    {
        if (target == null) return;

        if (_pendingPALoss.ContainsKey(target))
        {
            _pendingPALoss[target] += amount;
        }
        else
        {
            _pendingPALoss[target] = amount;
        }

        // Publie l'événement pour les systèmes qui veulent réagir
        EventBus.Publish(new StigmateConsumedEvent(target, amount));
    }

    /// <summary>
    /// Appelé au début du tour d'une unité pour appliquer les pertes de PA en attente
    /// </summary>
    /// <param name="unit">L'unité dont c'est le tour</param>
    public static void ProcessPendingPALoss(Unit unit)
    {
        if (unit == null) return;

        if (_pendingPALoss.TryGetValue(unit, out int paLoss))
        {
            if (unit is IActionPointsUser paUser)
            {
                paUser.ReduceCurrentPA(paLoss);
                Debug.Log($"[Stigmate] {unit.name} perd {paLoss} PA (effet Stigmate)");
            }

            _pendingPALoss.Remove(unit);
        }
    }

    /// <summary>
    /// Vérifie si une unité a une perte de PA en attente
    /// </summary>
    public static bool HasPendingPALoss(Unit unit)
    {
        return unit != null && _pendingPALoss.ContainsKey(unit);
    }

    /// <summary>
    /// Nettoie une unité de la liste des pertes de PA (appelé quand l'unité meurt)
    /// </summary>
    public static void ClearPendingPALoss(Unit unit)
    {
        if (unit != null)
        {
            _pendingPALoss.Remove(unit);
        }
    }

    /// <summary>
    /// Récupère toutes les unités marquées par une source spécifique
    /// </summary>
    public static List<Unit> GetUnitsMarkedBySource(Unit source)
    {
        List<Unit> markedUnits = new List<Unit>();

        if (source == null || Services.Grid == null) return markedUnits;

        // Parcourt toutes les unités sur la grille
        List<Unit> allUnits = Services.Grid.GetAllUnits();

        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.HasMarkFromSource(MarkType.Stigmate, source))
            {
                markedUnits.Add(unit);
            }
        }

        return markedUnits;
    }

    /// <summary>
    /// Compte le nombre d'ennemis marqués par un champion
    /// </summary>
    public static int GetMarkedCount(Unit source)
    {
        return GetUnitsMarkedBySource(source).Count;
    }

    /// <summary>
    /// Retire tous les Stigmates appliqués par un champion (appelé quand le champion meurt)
    /// </summary>
    public static void RemoveAllStigmatesFromSource(Unit source)
    {
        if (source == null) return;

        List<Unit> markedUnits = GetUnitsMarkedBySource(source);

        foreach (Unit markedUnit in markedUnits)
        {
            markedUnit.RemoveMark(MarkType.Stigmate);
        }

        if (markedUnits.Count > 0)
        {
            Debug.Log($"[Stigmate] {source.name} est mort - {markedUnits.Count} Stigmate(s) retiré(s)");
        }
    }
}

/// <summary>
/// Événement publié quand un Stigmate est consommé sur une unité
/// Utilisé pour appliquer la perte de PA au prochain tour
/// </summary>
public class StigmateConsumedEvent : GameEvent
{
    public Unit AffectedUnit { get; private set; }
    public int PALoss { get; private set; }

    public StigmateConsumedEvent(Unit affectedUnit, int paLoss)
    {
        AffectedUnit = affectedUnit;
        PALoss = paLoss;
    }
}
