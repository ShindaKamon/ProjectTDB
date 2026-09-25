using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event Bus centralisé pour la communication découplée entre composants.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        _subscribers.Clear();
        GameLog.Log("[EventBus] Initialisé au chargement du runtime.");
    }

    public static void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        if (!_subscribers.ContainsKey(eventType))
            _subscribers[eventType] = new List<Delegate>();

        if (_subscribers[eventType].Contains(handler))
        {
            GameLog.LogWarning($"[EventBus] Double souscription ignorée pour {eventType.Name}.");
            return;
        }

        _subscribers[eventType].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        if (!_subscribers.ContainsKey(eventType))
            return;

        _subscribers[eventType].Remove(handler);
        if (_subscribers[eventType].Count == 0)
            _subscribers.Remove(eventType);
    }

    public static void Publish<T>(T gameEvent) where T : GameEvent
    {
        if (gameEvent == null)
        {
            Debug.LogError("[EventBus] Tentative de publication d'un événement null.");
            return;
        }

        Type eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
            return;

        foreach (var handler in new List<Delegate>(_subscribers[eventType]))
        {
            try
            {
                (handler as Action<T>)?.Invoke(gameEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBus] Erreur pendant la publication de {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
