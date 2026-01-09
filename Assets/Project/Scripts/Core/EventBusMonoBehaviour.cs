using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe de base MonoBehaviour qui gère automatiquement le désabonnement de l'EventBus.
/// Pattern: Template Method
///
/// Usage:
///   public class MyComponent : EventBusMonoBehaviour
///   {
///       protected override void SubscribeToEvents()
///       {
///           Subscribe<TurnChangedEvent>(OnTurnChanged);
///           Subscribe<UnitDiedEvent>(OnUnitDied);
///       }
///
///       private void OnTurnChanged(TurnChangedEvent e) { ... }
///       private void OnUnitDied(UnitDiedEvent e) { ... }
///   }
///
/// Le désabonnement automatique se fait dans OnDestroy.
/// </summary>
public abstract class EventBusMonoBehaviour : MonoBehaviour
{
    // Liste des abonnements pour le désabonnement automatique
    private readonly List<Action> _unsubscribeActions = new List<Action>();
    private bool _isSubscribed = false;

    /// <summary>
    /// Appelé automatiquement dans OnEnable pour s'abonner aux événements
    /// </summary>
    protected virtual void OnEnable()
    {
        if (!_isSubscribed)
        {
            SubscribeToEvents();
            _isSubscribed = true;
        }
    }

    /// <summary>
    /// Appelé automatiquement dans OnDisable pour se désabonner
    /// </summary>
    protected virtual void OnDisable()
    {
        UnsubscribeFromAllEvents();
    }

    /// <summary>
    /// Override cette méthode pour s'abonner aux événements
    /// Utilise Subscribe<T>(handler) au lieu d'EventBus.Subscribe<T>(handler)
    /// </summary>
    protected abstract void SubscribeToEvents();

    /// <summary>
    /// S'abonne à un événement et enregistre le désabonnement automatique
    /// </summary>
    protected void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        EventBus.Subscribe(handler);

        // Enregistre l'action de désabonnement
        _unsubscribeActions.Add(() => EventBus.Unsubscribe(handler));
    }

    /// <summary>
    /// Se désabonne de tous les événements
    /// </summary>
    private void UnsubscribeFromAllEvents()
    {
        if (_isSubscribed)
        {
            foreach (Action unsubscribe in _unsubscribeActions)
            {
                unsubscribe?.Invoke();
            }
            _unsubscribeActions.Clear();
            _isSubscribed = false;
        }
    }

    /// <summary>
    /// Publier un événement (raccourci vers EventBus.Publish)
    /// </summary>
    protected void Publish<T>(T gameEvent) where T : GameEvent
    {
        EventBus.Publish(gameEvent);
    }
}
