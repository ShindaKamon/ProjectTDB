using UnityEngine;

/// <summary>
/// Interface pour les unités qui utilisent un système de Points d'Action (PA).
/// Permet aux Champions et Enemies d'avoir un système PA unifié sans duplication de code.
/// </summary>
public interface IActionPointsUser
{
    /// <summary>
    /// Retourne les PA (Points d'Action) courants
    /// </summary>
    int GetCurrentPA();

    /// <summary>
    /// Retourne les PA (Points d'Action) maximum
    /// </summary>
    int GetMaxPA();

    /// <summary>
    /// Dépense des PA pour une action (jouer une carte, etc.)
    /// </summary>
    /// <param name="amount">Nombre de PA à dépenser</param>
    /// <returns>True si les PA ont été dépensés, false si insuffisants</returns>
    bool SpendPA(int amount);

    /// <summary>
    /// Rafraîchit les PA au maximum (début de tour)
    /// </summary>
    void RefreshPA();

    /// <summary>
    /// Modifie les PA maximum (utilisé pour les transformations, buffs, etc.)
    /// </summary>
    /// <param name="value">Nouvelle valeur de PA maximum</param>
    void SetMaxPA(int value);

    /// <summary>
    /// Événement déclenché quand les PA changent
    /// </summary>
    event System.Action<int, int> OnActionPointsChanged; // (current, max)

    /// <summary>
    /// Réduit les PA courants d'un montant donné (utilisé pour les debuffs)
    /// </summary>
    /// <param name="amount">Montant de PA à retirer</param>
    void ReduceCurrentPA(int amount);
}
