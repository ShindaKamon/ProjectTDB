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

    // ========== BATTLE UI SERVICE ==========

    /// <summary>
    /// Accès rapide au service de connexion des UI de combat
    /// </summary>
    public static IBattleUIService BattleUI
    {
        get
        {
            IBattleUIService service = ServiceLocator.Instance.Get<IBattleUIService>();
            if (service == null)
            {
                Debug.LogError("Services: IBattleUIService non enregistré ! BattleUIManager a-t-il été initialisé?");
            }
            return service;
        }
    }

    /// <summary>
    /// Vérifie si le service de UI de combat est disponible
    /// </summary>
    public static bool IsBattleUIServiceAvailable()
    {
        return ServiceLocator.Instance.IsRegistered<IBattleUIService>();
    }

    // ========== HEALTH BAR SERVICE ==========

    /// <summary>
    /// Accès rapide au service de barres de vie flottantes
    /// </summary>
    public static IHealthBarService HealthBar
    {
        get
        {
            IHealthBarService service = ServiceLocator.Instance.Get<IHealthBarService>();
            if (service == null)
            {
                Debug.LogError("Services: IHealthBarService non enregistré ! HealthBarManager a-t-il été initialisé?");
            }
            return service;
        }
    }

    /// <summary>
    /// Vérifie si le service de barres de vie est disponible
    /// </summary>
    public static bool IsHealthBarServiceAvailable()
    {
        return ServiceLocator.Instance.IsRegistered<IHealthBarService>();
    }

    // NOTE: Futurs services à ajouter ici
    // public static IAudioService Audio => ServiceLocator.Instance.Get<IAudioService>();
    // public static ISaveService Save => ServiceLocator.Instance.Get<ISaveService>();
    // public static IInputService Input => ServiceLocator.Instance.Get<IInputService>();
}
