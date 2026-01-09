using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event Bus centralisé pour la communication découplée entre composants.
/// Pattern: Event Bus / Mediator
/// Permet le découplage total entre publishers et subscribers.
///
/// Usage:
///   - Publier: EventBus.Publish(new TurnChangedEvent(newUnit, oldUnit));
///   - S'abonner: EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
///   - Se désabonner: EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
/// </summary>
public static class EventBus
{
    // Dictionary qui mappe chaque type d'événement à sa liste de subscribers
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

    // Statistiques pour debugging
    private static int _totalEventsPublished = 0;
    private static bool _enableLogging = false;

    /// <summary>
    /// Active/désactive le logging des événements (pour debugging)
    /// </summary>
    public static void SetLogging(bool enabled)
    {
        _enableLogging = enabled;
    }

    /// <summary>
    /// S'abonne à un type d'événement spécifique
    /// </summary>
    public static void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        if (!_subscribers[eventType].Contains(handler))
        {
            _subscribers[eventType].Add(handler);

            if (_enableLogging)
            {
                Debug.Log($"[EventBus] Nouvel abonné à {eventType.Name}. Total: {_subscribers[eventType].Count}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventBus] Tentative d'abonnement multiple à {eventType.Name} ignorée.");
        }
    }

    /// <summary>
    /// Se désabonne d'un type d'événement
    /// </summary>
    public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (_subscribers.ContainsKey(eventType))
        {
            if (_subscribers[eventType].Remove(handler))
            {
                if (_enableLogging)
                {
                    Debug.Log($"[EventBus] Désabonnement de {eventType.Name}. Restants: {_subscribers[eventType].Count}");
                }

                // Nettoie la liste si elle est vide
                if (_subscribers[eventType].Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
            else
            {
                Debug.LogWarning($"[EventBus] Handler non trouvé pour {eventType.Name}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventBus] Aucun abonné à {eventType.Name}");
        }
    }

    /// <summary>
    /// Publie un événement à tous les subscribers
    /// </summary>
    public static void Publish<T>(T gameEvent) where T : GameEvent
    {
        if (gameEvent == null)
        {
            Debug.LogError("[EventBus] Tentative de publier un événement null!");
            return;
        }

        Type eventType = typeof(T);
        _totalEventsPublished++;

        if (_enableLogging)
        {
            Debug.Log($"[EventBus] 📣 Publication: {eventType.Name} (#{_totalEventsPublished})");
        }

        if (_subscribers.ContainsKey(eventType))
        {
            // Copie la liste pour éviter les modifications pendant l'itération
            List<Delegate> handlers = new List<Delegate>(_subscribers[eventType]);

            foreach (Delegate handler in handlers)
            {
                try
                {
                    // Invoke le handler avec l'événement
                    (handler as Action<T>)?.Invoke(gameEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] ❌ Erreur dans handler de {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            if (_enableLogging)
            {
                Debug.Log($"[EventBus] ✅ {handlers.Count} handler(s) notifié(s) pour {eventType.Name}");
            }
        }
        else
        {
            if (_enableLogging)
            {
                Debug.Log($"[EventBus] ⚠️ Aucun abonné à {eventType.Name}");
            }
        }
    }

    /// <summary>
    /// Nettoie tous les subscribers (utilisé lors du changement de scène ou reset)
    /// </summary>
    public static void Clear()
    {
        int totalSubscribers = 0;
        foreach (var kvp in _subscribers)
        {
            totalSubscribers += kvp.Value.Count;
        }

        _subscribers.Clear();
        Debug.Log($"[EventBus] Nettoyage complet: {totalSubscribers} abonnement(s) supprimé(s)");
    }

    /// <summary>
    /// Retourne le nombre d'abonnés pour un type d'événement donné
    /// </summary>
    public static int GetSubscriberCount<T>() where T : GameEvent
    {
        Type eventType = typeof(T);
        return _subscribers.ContainsKey(eventType) ? _subscribers[eventType].Count : 0;
    }

    /// <summary>
    /// Retourne le nombre total d'événements publiés depuis le démarrage
    /// </summary>
    public static int GetTotalEventsPublished()
    {
        return _totalEventsPublished;
    }

    /// <summary>
    /// Retourne des statistiques pour debugging
    /// </summary>
    public static string GetStatistics()
    {
        int totalSubscribers = 0;
        int totalEventTypes = _subscribers.Count;

        foreach (var kvp in _subscribers)
        {
            totalSubscribers += kvp.Value.Count;
        }

        return $"EventBus Stats:\n" +
               $"  Types d'événements: {totalEventTypes}\n" +
               $"  Total abonnés: {totalSubscribers}\n" +
               $"  Événements publiés: {_totalEventsPublished}";
    }

    /// <summary>
    /// Affiche tous les subscribers actuels (pour debugging)
    /// </summary>
    public static void DebugPrintSubscribers()
    {
        Debug.Log("=== EventBus Subscribers ===");
        foreach (var kvp in _subscribers)
        {
            Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} subscriber(s)");
            foreach (Delegate handler in kvp.Value)
            {
                Debug.Log($"    - {handler.Target?.GetType().Name ?? "Static"}.{handler.Method.Name}");
            }
        }
        Debug.Log($"Total: {_totalEventsPublished} événements publiés");
    }
}
