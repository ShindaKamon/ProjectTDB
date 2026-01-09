using UnityEngine;
using System;

/// <summary>
/// États possibles pour une unité
/// </summary>
public enum UnitStateType
{
    Idle,          // En attente (pas son tour)
    Active,        // Tour actif (peut agir)
    Moving,        // En mouvement
    Acting,        // Joue une carte / effectue une action
    Dead,          // Mort
    Stunned        // Étourdi (ne peut pas agir)
}

/// <summary>
/// State Machine pour gérer l'état d'une unité.
/// Pattern: State Machine Pattern
/// Responsabilités:
/// - Gérer les transitions d'états d'une unité
/// - Valider les transitions
/// - Empêcher les actions invalides selon l'état
/// </summary>
public class UnitState
{
    // ========== ÉTAT ==========
    private UnitStateType _currentState;
    private Unit _unit;

    // ========== ÉVÉNEMENTS ==========
    public event Action<UnitStateType, UnitStateType> OnStateChanged; // (oldState, newState)

    // ========== CONSTRUCTEUR ==========

    public UnitState(Unit unit)
    {
        _unit = unit;
        _currentState = UnitStateType.Idle;
    }

    // ========== GETTERS ==========

    public UnitStateType GetCurrentState()
    {
        return _currentState;
    }

    public bool IsIdle() => _currentState == UnitStateType.Idle;
    public bool IsActive() => _currentState == UnitStateType.Active;
    public bool IsMoving() => _currentState == UnitStateType.Moving;
    public bool IsActing() => _currentState == UnitStateType.Acting;
    public bool IsDead() => _currentState == UnitStateType.Dead;
    public bool IsStunned() => _currentState == UnitStateType.Stunned;

    // ========== TRANSITIONS ==========

    /// <summary>
    /// Met l'unité en état Idle (pas son tour)
    /// </summary>
    public void SetIdle()
    {
        if (_currentState == UnitStateType.Dead)
        {
            Debug.LogWarning($"{_unit.name}: Cannot set idle - unit is dead");
            return;
        }

        TransitionTo(UnitStateType.Idle);
    }

    /// <summary>
    /// Active l'unité (devient son tour)
    /// </summary>
    public void SetActive()
    {
        if (_currentState == UnitStateType.Dead)
        {
            Debug.LogWarning($"{_unit.name}: Cannot activate - unit is dead");
            return;
        }

        if (_currentState == UnitStateType.Stunned)
        {
            Debug.Log($"{_unit.name}: Cannot activate - unit is stunned");
            TransitionTo(UnitStateType.Idle); // Skip turn
            return;
        }

        TransitionTo(UnitStateType.Active);
    }

    /// <summary>
    /// Commence un mouvement
    /// </summary>
    public void BeginMoving()
    {
        if (!CanMove())
        {
            Debug.LogWarning($"{_unit.name}: Cannot move from state {_currentState}");
            return;
        }

        TransitionTo(UnitStateType.Moving);
    }

    /// <summary>
    /// Termine un mouvement
    /// </summary>
    public void EndMoving()
    {
        if (_currentState != UnitStateType.Moving)
        {
            Debug.LogWarning($"{_unit.name}: Not currently moving");
            return;
        }

        TransitionTo(UnitStateType.Active);
    }

    /// <summary>
    /// Commence une action (jouer carte, attaquer)
    /// </summary>
    public void BeginActing()
    {
        if (!CanAct())
        {
            Debug.LogWarning($"{_unit.name}: Cannot act from state {_currentState}");
            return;
        }

        TransitionTo(UnitStateType.Acting);
    }

    /// <summary>
    /// Termine une action
    /// </summary>
    public void EndActing()
    {
        if (_currentState != UnitStateType.Acting)
        {
            Debug.LogWarning($"{_unit.name}: Not currently acting");
            return;
        }

        TransitionTo(UnitStateType.Active);
    }

    /// <summary>
    /// Marque l'unité comme morte
    /// </summary>
    public void SetDead()
    {
        TransitionTo(UnitStateType.Dead);
    }

    /// <summary>
    /// Étourdit l'unité
    /// </summary>
    public void SetStunned()
    {
        if (_currentState == UnitStateType.Dead)
        {
            Debug.LogWarning($"{_unit.name}: Cannot stun - unit is dead");
            return;
        }

        TransitionTo(UnitStateType.Stunned);
    }

    /// <summary>
    /// Retire l'étourdissement
    /// </summary>
    public void RemoveStun()
    {
        if (_currentState != UnitStateType.Stunned)
        {
            return;
        }

        TransitionTo(UnitStateType.Idle);
    }

    // ========== VALIDATION ==========

    /// <summary>
    /// Vérifie si l'unité peut se déplacer
    /// </summary>
    public bool CanMove()
    {
        return _currentState == UnitStateType.Active;
    }

    /// <summary>
    /// Vérifie si l'unité peut effectuer une action (jouer carte, attaquer)
    /// </summary>
    public bool CanAct()
    {
        return _currentState == UnitStateType.Active;
    }

    /// <summary>
    /// Vérifie si l'unité peut recevoir des dégâts
    /// </summary>
    public bool CanTakeDamage()
    {
        return _currentState != UnitStateType.Dead;
    }

    /// <summary>
    /// Vérifie si l'unité peut être ciblée
    /// </summary>
    public bool CanBeTargeted()
    {
        return _currentState != UnitStateType.Dead;
    }

    // ========== UTILITAIRES ==========

    /// <summary>
    /// Transition interne vers un nouvel état
    /// </summary>
    private void TransitionTo(UnitStateType newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        UnitStateType oldState = _currentState;
        _currentState = newState;

        Debug.Log($"{_unit.name}: State {oldState} → {newState}");

        // Notifie les abonnés
        OnStateChanged?.Invoke(oldState, newState);

        // Publie événement global via EventBus
        EventBus.Publish(new UnitStateChangedEvent(_unit, oldState, newState));
    }

    /// <summary>
    /// Retourne une description de l'état actuel
    /// </summary>
    public string GetStateDescription()
    {
        switch (_currentState)
        {
            case UnitStateType.Idle:
                return "En attente";
            case UnitStateType.Active:
                return "Actif";
            case UnitStateType.Moving:
                return "En mouvement";
            case UnitStateType.Acting:
                return "En action";
            case UnitStateType.Dead:
                return "Mort";
            case UnitStateType.Stunned:
                return "Étourdi";
            default:
                return "Inconnu";
        }
    }
}
