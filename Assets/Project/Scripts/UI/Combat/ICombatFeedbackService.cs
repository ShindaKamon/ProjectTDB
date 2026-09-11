using UnityEngine;

/// <summary>
/// Interface pour le service de feedbacks visuels de combat (dégâts, soins, shake).
/// Exposé via ServiceLocator pour éviter les appels directs à CombatFeedbackManager.Instance
/// </summary>
public interface ICombatFeedbackService
{
    /// <summary>
    /// Affiche manuellement un nombre de dégâts
    /// </summary>
    void ShowDamage(int damage, Vector3 worldPosition);

    /// <summary>
    /// Affiche manuellement un nombre de soins
    /// </summary>
    void ShowHeal(int healAmount, Vector3 worldPosition);

    /// <summary>
    /// Affiche "IMMUNE" pour un coup bloqué
    /// </summary>
    void ShowImmune(Vector3 worldPosition);

    /// <summary>
    /// Shake manuel d'une unité
    /// </summary>
    void ShakeTransform(Transform target, float duration = -1f, float intensity = -1f);
}
