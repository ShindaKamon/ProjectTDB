using UnityEngine;
using System;

/// <summary>
/// État du système de tours
/// </summary>
public enum TurnState
{
    Initializing,      // Démarrage du combat
    PlayerTurn,        // Tour du joueur en cours
    EnemyTurn,         // Tour de l'ennemi en cours
    TransitioningTurn, // Transition entre tours (animations, effets)
    BattleEnd          // Combat terminé (victoire/défaite)
}

/// <summary>
/// State Machine pour gérer les états du système de tours.
/// Pattern: State Machine Pattern
/// Responsabilités:
/// - Gérer les transitions d'états de tours
/// - Valider les transitions
/// - Publier des événements de changement d'état
/// - Empêcher les actions invalides selon l'état
/// </summary>
public class TurnStateMachine
{
    // ========== ÉTAT ==========
    private TurnState _currentState;
    private Unit _activeUnit;

    // ========== ÉVÉNEMENTS ==========
    public event Action<TurnState, TurnState> OnStateChanged; // (oldState, newState)

    // ========== CONSTRUCTEUR ==========

    public TurnStateMachine()
    {
        _currentState = TurnState.Initializing;
    }

    // ========== GETTERS ==========

    public TurnState GetCurrentState()
    {
        return _currentState;
    }

    public Unit GetActiveUnit()
    {
        return _activeUnit;
    }

    // ========== TRANSITIONS ==========

    /// <summary>
    /// Démarre le combat avec une unité active
    /// </summary>
    public void StartBattle(Unit firstUnit)
    {
        if (_currentState != TurnState.Initializing)
        {
            Debug.LogWarning($"TurnStateMachine: Cannot start battle from state {_currentState}");
            return;
        }

        _activeUnit = firstUnit;

        if (_activeUnit.GetFaction() == Unit.UnitFaction.Player)
        {
            TransitionTo(TurnState.PlayerTurn);
        }
        else
        {
            TransitionTo(TurnState.EnemyTurn);
        }
    }

    /// <summary>
    /// Commence le tour du joueur
    /// </summary>
    public void BeginPlayerTurn(Unit playerUnit)
    {
        if (!CanTransitionToPlayerTurn())
        {
            Debug.LogWarning($"TurnStateMachine: Cannot begin player turn from state {_currentState}");
            return;
        }

        _activeUnit = playerUnit;
        TransitionTo(TurnState.PlayerTurn);
    }

    /// <summary>
    /// Commence le tour de l'ennemi
    /// </summary>
    public void BeginEnemyTurn(Unit enemyUnit)
    {
        if (!CanTransitionToEnemyTurn())
        {
            Debug.LogWarning($"TurnStateMachine: Cannot begin enemy turn from state {_currentState}");
            return;
        }

        _activeUnit = enemyUnit;
        TransitionTo(TurnState.EnemyTurn);
    }

    /// <summary>
    /// Commence la transition entre tours
    /// </summary>
    public void BeginTurnTransition()
    {
        if (_currentState != TurnState.PlayerTurn && _currentState != TurnState.EnemyTurn)
        {
            Debug.LogWarning($"TurnStateMachine: Cannot begin transition from state {_currentState}");
            return;
        }

        TransitionTo(TurnState.TransitioningTurn);
    }

    /// <summary>
    /// Termine le combat
    /// </summary>
    public void EndBattle()
    {
        TransitionTo(TurnState.BattleEnd);
    }

    // ========== VALIDATION ==========

    /// <summary>
    /// Vérifie si on peut commencer un tour joueur
    /// </summary>
    public bool CanTransitionToPlayerTurn()
    {
        return _currentState == TurnState.Initializing ||
               _currentState == TurnState.TransitioningTurn ||
               _currentState == TurnState.EnemyTurn;
    }

    /// <summary>
    /// Vérifie si on peut commencer un tour ennemi
    /// </summary>
    public bool CanTransitionToEnemyTurn()
    {
        return _currentState == TurnState.Initializing ||
               _currentState == TurnState.TransitioningTurn ||
               _currentState == TurnState.PlayerTurn;
    }

    /// <summary>
    /// Vérifie si le joueur peut agir (jouer carte, bouger)
    /// </summary>
    public bool CanPlayerAct()
    {
        return _currentState == TurnState.PlayerTurn;
    }

    /// <summary>
    /// Vérifie si l'ennemi peut agir
    /// </summary>
    public bool CanEnemyAct()
    {
        return _currentState == TurnState.EnemyTurn;
    }

    /// <summary>
    /// Vérifie si le combat est terminé
    /// </summary>
    public bool IsBattleOver()
    {
        return _currentState == TurnState.BattleEnd;
    }

    // ========== UTILITAIRES ==========

    /// <summary>
    /// Transition interne vers un nouvel état
    /// </summary>
    private void TransitionTo(TurnState newState)
    {
        if (_currentState == newState)
        {
            Debug.LogWarning($"TurnStateMachine: Already in state {newState}");
            return;
        }

        TurnState oldState = _currentState;
        _currentState = newState;

        Debug.Log($"TurnStateMachine: {oldState} → {newState}");

        // Notifie les abonnés
        OnStateChanged?.Invoke(oldState, newState);

        // Publie événement global via EventBus
        EventBus.Publish(new TurnStateChangedEvent(oldState, newState, _activeUnit));
    }

    /// <summary>
    /// Retourne une description de l'état actuel
    /// </summary>
    public string GetStateDescription()
    {
        switch (_currentState)
        {
            case TurnState.Initializing:
                return "Initialisation du combat...";
            case TurnState.PlayerTurn:
                return $"Tour du joueur ({_activeUnit?.name ?? "Unknown"})";
            case TurnState.EnemyTurn:
                return $"Tour de l'ennemi ({_activeUnit?.name ?? "Unknown"})";
            case TurnState.TransitioningTurn:
                return "Transition entre tours...";
            case TurnState.BattleEnd:
                return "Combat terminé";
            default:
                return "État inconnu";
        }
    }
}
