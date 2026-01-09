using UnityEngine;

/// <summary>
/// Helper statique pour accéder facilement aux services enregistrés.
/// Pattern: Facade Pattern + Service Locator
/// Usage: Services.Grid.GetActiveUnit() au lieu de GridManager.Instance.GetActiveUnit()
/// </summary>
public static class Services
{
    // ========== GRID SERVICE ==========

    /// <summary>
    /// Accès rapide au service de grille
    /// </summary>
    public static IGridService Grid
    {
        get
        {
            IGridService service = ServiceLocator.Instance.Get<IGridService>();
            if (service == null)
            {
                Debug.LogError("Services: IGridService non enregistré ! GridManager a-t-il été initialisé?");
            }
            return service;
        }
    }

    /// <summary>
    /// Vérifie si le service de grille est disponible
    /// </summary>
    public static bool IsGridServiceAvailable()
    {
        return ServiceLocator.Instance.IsRegistered<IGridService>();
    }

    // NOTE: Futurs services à ajouter ici
    // public static IAudioService Audio => ServiceLocator.Instance.Get<IAudioService>();
    // public static ISaveService Save => ServiceLocator.Instance.Get<ISaveService>();
    // public static IInputService Input => ServiceLocator.Instance.Get<IInputService>();
}
