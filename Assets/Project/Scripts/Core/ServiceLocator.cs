using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service Locator pour l'injection de dépendances.
/// Pattern: Service Locator Pattern
/// Responsabilités:
/// - Enregistrer des services (Register)
/// - Résoudre des dépendances (Get)
/// - Gérer le cycle de vie des services
/// - Éviter les Singleton statiques (Services.Grid, etc.)
/// </summary>
public class ServiceLocator
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    // ========== SINGLETON ==========

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ServiceLocator();
            }
            return _instance;
        }
    }

    private ServiceLocator()
    {
        // Private constructor pour Singleton
    }

    // ========== REGISTRATION ==========

    /// <summary>
    /// Enregistre un service avec son interface
    /// </summary>
    public void Register<TInterface>(TInterface service) where TInterface : class
    {
        Type serviceType = typeof(TInterface);

        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"ServiceLocator: Service {serviceType.Name} déjà enregistré. Remplacement.");
            _services[serviceType] = service;
        }
        else
        {
            _services.Add(serviceType, service);
            Debug.Log($"ServiceLocator: Service {serviceType.Name} enregistré");
        }
    }

    /// <summary>
    /// Désenregistre un service
    /// </summary>
    public void Unregister<TInterface>() where TInterface : class
    {
        Type serviceType = typeof(TInterface);

        if (_services.ContainsKey(serviceType))
        {
            _services.Remove(serviceType);
            Debug.Log($"ServiceLocator: Service {serviceType.Name} désenregistré");
        }
    }

    // ========== RESOLUTION ==========

    /// <summary>
    /// Résout un service enregistré
    /// </summary>
    public TInterface Get<TInterface>() where TInterface : class
    {
        Type serviceType = typeof(TInterface);

        if (_services.TryGetValue(serviceType, out object service))
        {
            return service as TInterface;
        }

        Debug.LogError($"ServiceLocator: Service {serviceType.Name} non trouvé !");
        return null;
    }

    /// <summary>
    /// Tente de résoudre un service (retourne null si non trouvé)
    /// </summary>
    public bool TryGet<TInterface>(out TInterface service) where TInterface : class
    {
        Type serviceType = typeof(TInterface);

        if (_services.TryGetValue(serviceType, out object foundService))
        {
            service = foundService as TInterface;
            return service != null;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Vérifie si un service est enregistré
    /// </summary>
    public bool IsRegistered<TInterface>() where TInterface : class
    {
        return _services.ContainsKey(typeof(TInterface));
    }

    // ========== LIFECYCLE ==========

    /// <summary>
    /// Efface tous les services (utile pour tests ou changement de scène)
    /// </summary>
    public void ClearAll()
    {
        Debug.Log("ServiceLocator: Tous les services effacés");
        _services.Clear();
    }

    /// <summary>
    /// Retourne le nombre de services enregistrés
    /// </summary>
    public int GetServiceCount()
    {
        return _services.Count;
    }

    // ========== RESET (pour tests) ==========

    /// <summary>
    /// Reset complet du ServiceLocator (utile pour tests unitaires)
    /// </summary>
    public static void ResetInstance()
    {
        _instance = null;
    }
}
