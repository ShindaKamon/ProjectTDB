using UnityEngine;

/// <summary>
/// Classe de base pour tous les événements du jeu.
/// Pattern: Event Sourcing / Observer
/// </summary>
public abstract class GameEvent
{
    /// <summary>
    /// Timestamp de création de l'événement (pour debugging et replay)
    /// </summary>
    public float Timestamp { get; private set; }

    protected GameEvent()
    {
        Timestamp = Time.time;
    }
}

// ========== ÉVÉNEMENTS DE TOURS ==========

/// <summary>
/// Publié quand le tour change et qu'une nouvelle unité devient active
/// </summary>
public class TurnChangedEvent : GameEvent
{
    public Unit NewActiveUnit { get; private set; }
    public Unit PreviousActiveUnit { get; private set; }

    public TurnChangedEvent(Unit newActiveUnit, Unit previousActiveUnit)
    {
        NewActiveUnit = newActiveUnit;
        PreviousActiveUnit = previousActiveUnit;
    }
}

/// <summary>
/// Publié quand le joueur clique sur "Fin de tour"
/// </summary>
public class TurnEndRequestedEvent : GameEvent
{
    public Unit RequestingUnit { get; private set; }

    public TurnEndRequestedEvent(Unit requestingUnit)
    {
        RequestingUnit = requestingUnit;
    }
}

/// <summary>
/// Publié quand l'état du système de tours change (Phase 3.4)
/// </summary>
public class TurnStateChangedEvent : GameEvent
{
    public TurnState OldState { get; private set; }
    public TurnState NewState { get; private set; }
    public Unit ActiveUnit { get; private set; }

    public TurnStateChangedEvent(TurnState oldState, TurnState newState, Unit activeUnit)
    {
        OldState = oldState;
        NewState = newState;
        ActiveUnit = activeUnit;
    }
}

// ========== ÉVÉNEMENTS D'UNITÉS ==========

/// <summary>
/// Publié quand une unité meurt
/// </summary>
public class UnitDiedEvent : GameEvent
{
    public Unit DeadUnit { get; private set; }

    public UnitDiedEvent(Unit deadUnit)
    {
        DeadUnit = deadUnit;
    }
}

/// <summary>
/// Publié quand l'état d'une unité change (Phase 3.4)
/// </summary>
public class UnitStateChangedEvent : GameEvent
{
    public Unit Unit { get; private set; }
    public UnitStateType OldState { get; private set; }
    public UnitStateType NewState { get; private set; }

    public UnitStateChangedEvent(Unit unit, UnitStateType oldState, UnitStateType newState)
    {
        Unit = unit;
        OldState = oldState;
        NewState = newState;
    }
}

/// <summary>
/// Publié quand une unité se déplace d'une case
/// </summary>
public class UnitMovedEvent : GameEvent
{
    public Unit Unit { get; private set; }
    public Vector2 FromPosition { get; private set; }
    public Vector2 ToPosition { get; private set; }

    public UnitMovedEvent(Unit unit, Vector2 fromPosition, Vector2 toPosition)
    {
        Unit = unit;
        FromPosition = fromPosition;
        ToPosition = toPosition;
    }
}

/// <summary>
/// Publié quand une unité prend des dégâts
/// </summary>
public class UnitDamagedEvent : GameEvent
{
    public Unit Target { get; private set; }
    public Unit Source { get; private set; }
    public int Damage { get; private set; }

    public UnitDamagedEvent(Unit target, Unit source, int damage)
    {
        Target = target;
        Source = source;
        Damage = damage;
    }
}

/// <summary>
/// Publié quand une unité est soignée
/// </summary>
public class UnitHealedEvent : GameEvent
{
    public Unit Target { get; private set; }
    public int HealAmount { get; private set; }

    public UnitHealedEvent(Unit target, int healAmount)
    {
        Target = target;
        HealAmount = healAmount;
    }
}

// ========== ÉVÉNEMENTS DE CARTES ==========

/// <summary>
/// Publié quand une carte est jouée
/// </summary>
public class CardPlayedEvent : GameEvent
{
    public CardData Card { get; private set; }
    public Unit Source { get; private set; }
    public Unit TargetUnit { get; private set; }
    public Vector2 TargetTile { get; private set; }

    public CardPlayedEvent(CardData card, Unit source, Unit targetUnit, Vector2 targetTile)
    {
        Card = card;
        Source = source;
        TargetUnit = targetUnit;
        TargetTile = targetTile;
    }
}

/// <summary>
/// Publié quand une carte est piochée
/// </summary>
public class CardDrawnEvent : GameEvent
{
    public CardData Card { get; private set; }
    public Unit Unit { get; private set; }

    public CardDrawnEvent(CardData card, Unit unit)
    {
        Card = card;
        Unit = unit;
    }
}

// ========== ÉVÉNEMENTS D'AFFICHAGE ==========

/// <summary>
/// Publié pour demander l'affichage de la portée de mouvement
/// </summary>
public class ShowMovementRangeEvent : GameEvent
{
    public Unit Unit { get; private set; }

    public ShowMovementRangeEvent(Unit unit)
    {
        Unit = unit;
    }
}

/// <summary>
/// Publié pour demander l'affichage des cibles de carte
/// </summary>
public class ShowCardTargetsEvent : GameEvent
{
    public CardData Card { get; private set; }
    public Unit Source { get; private set; }

    public ShowCardTargetsEvent(CardData card, Unit source)
    {
        Card = card;
        Source = source;
    }
}

/// <summary>
/// Publié pour demander l'affichage de la zone AOE
/// </summary>
public class ShowAOEZoneEvent : GameEvent
{
    public Vector2 Epicenter { get; private set; }
    public int Radius { get; private set; }
    public CardData Card { get; private set; }
    public Unit Source { get; private set; }

    public ShowAOEZoneEvent(Vector2 epicenter, int radius, CardData card, Unit source)
    {
        Epicenter = epicenter;
        Radius = radius;
        Card = card;
        Source = source;
    }
}

/// <summary>
/// Publié pour demander la réinitialisation des couleurs de tuiles
/// </summary>
public class ResetTileColorsEvent : GameEvent
{
    // Événement simple sans données
}

/// <summary>
/// Publié pour demander la mise à jour de l'UI des unités
/// </summary>
public class UpdateUnitUIEvent : GameEvent
{
    public Unit Unit { get; private set; }

    public UpdateUnitUIEvent(Unit unit = null)
    {
        Unit = unit; // null = mettre à jour l'unité active
    }
}

// ========== ÉVÉNEMENTS D'ÉMOTION ==========

/// <summary>
/// Publié quand l'émotion d'une unité change
/// </summary>
public class EmotionChangedEvent : GameEvent
{
    public Unit Unit { get; private set; }
    public float NewEmotion { get; private set; }
    public float MaxEmotion { get; private set; }

    public EmotionChangedEvent(Unit unit, float newEmotion, float maxEmotion)
    {
        Unit = unit;
        NewEmotion = newEmotion;
        MaxEmotion = maxEmotion;
    }
}
