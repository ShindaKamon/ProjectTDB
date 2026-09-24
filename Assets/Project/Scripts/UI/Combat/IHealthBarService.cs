using UnityEngine;

/// <summary>
/// Interface pour le service de création des barres de vie flottantes.
/// Exposé via ServiceLocator pour éviter les appels directs à HealthBarManager.Instance
/// </summary>
public interface IHealthBarService
{
    /// <summary>
    /// Crée une barre de vie flottante suivant une cible
    /// </summary>
    HealthBar CreateHealthBar(Transform target, Vector3 offset, Color color, int maxHP);
}
