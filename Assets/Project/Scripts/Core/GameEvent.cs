using System.Collections.Generic;
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
/// Réseau : l'état du combat diffère entre l'hôte et un client au début de ce tour
/// </summary>
public class NetworkDesyncEvent : GameEvent
{
    public int Turn { get; private set; }

    public NetworkDesyncEvent(int turn)
    {
        Turn = turn;
    }
}

/// <summary>
/// Réseau : un joueur s'est déconnecté en plein combat ; son champion reste sur la grille et
/// l'hôte passe ses tours. Actor = sa place dans CombatParty.
/// </summary>
public class NetworkPlayerLeftEvent : GameEvent
{
    public int Actor { get; private set; }

    public NetworkPlayerLeftEvent(int actor)
    {
        Actor = actor;
    }
}

/// <summary>
/// Fin de tour refusée : le champion a plus de cartes en main que le maximum et doit
/// d'abord en défausser Count (au choix du joueur)
/// </summary>
public class HandDiscardRequiredEvent : GameEvent
{
    public Unit Unit { get; private set; }
    public int Count { get; private set; }

    public HandDiscardRequiredEvent(Unit unit, int count)
    {
        Unit = unit;
        Count = count;
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
/// Publié quand une unité prend des dégâts
/// </summary>
public class UnitDamagedEvent : GameEvent
{
    public Unit Target { get; private set; }
    public Unit Source { get; private set; }
    public int Damage { get; private set; }          // coup reçu (chiffre affiché)
    public int EffectiveDamage { get; private set; } // réellement retiré (bouclier + PV, sans l'excédent au-delà des PV)

    public UnitDamagedEvent(Unit target, Unit source, int damage, int effectiveDamage = -1)
    {
        Target = target;
        Source = source;
        Damage = damage;
        EffectiveDamage = effectiveDamage >= 0 ? effectiveDamage : damage;
    }
}

/// <summary>
/// Publié quand une unité est soignée
/// </summary>
public class UnitHealedEvent : GameEvent
{
    public Unit Target { get; private set; }
    public int HealAmount { get; private set; }
    public Unit Source { get; private set; } // qui soigne (la cible elle-même par défaut)

    public UnitHealedEvent(Unit target, int healAmount, Unit source = null)
    {
        Target = target;
        HealAmount = healAmount;
        Source = source ?? target;
    }
}

/// <summary>
/// Publié quand le combat se termine (voir BattleOutcome) : victoire ou défaite
/// </summary>
public class BattleEndedEvent : GameEvent
{
    public BattleResult Result { get; private set; }

    public BattleEndedEvent(BattleResult result)
    {
        Result = result;
    }
}

/// <summary>
/// Bonus ou malus appliqué à une unité (hors dégâts et soins), pour le retour visuel
/// </summary>
public enum UnitEffect
{
    Shield,
    ReactiveShield,
    NextAttackBonus,
    Attack,
    Armor,
    MagicResistance,
    ActionPoints,           // montant négatif = retrait au prochain tour
    MovementPoints,         // idem ; int.MaxValue en négatif = tous les PM
    DamageTakenPercent,     // ex. -15 : dégâts subis réduits de 15 %
    NextAttackPercent,      // ex. +15 : prochaine carte de dégâts +15 %
    PmImmune,               // Ténacité (montant ignoré)
    CardCancelled           // prochaine carte du monstre annulée (montant ignoré)
}

/// <summary>
/// Publié quand un bonus ou un malus est appliqué à une unité (montant signé)
/// </summary>
public class UnitEffectAppliedEvent : GameEvent
{
    public Unit Target { get; private set; }
    public UnitEffect Effect { get; private set; }
    public int Amount { get; private set; }

    public UnitEffectAppliedEvent(Unit target, UnitEffect effect, int amount)
    {
        Target = target;
        Effect = effect;
        Amount = amount;
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
    public Vector2Int Epicenter { get; private set; }
    public int Radius { get; private set; }
    public CardData Card { get; private set; }
    public Unit Source { get; private set; }

    public ShowAOEZoneEvent(Vector2Int epicenter, int radius, CardData card, Unit source)
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
/// Publié au survol d'une cible avec une carte sélectionnée : dégâts prévus par unité touchée
/// (liste vide = plus rien à afficher)
/// </summary>
public class DamagePreviewEvent : GameEvent
{
    public System.Collections.Generic.IReadOnlyList<DamagePreview.Entry> Entries { get; private set; }

    public DamagePreviewEvent(System.Collections.Generic.IReadOnlyList<DamagePreview.Entry> entries)
    {
        Entries = entries;
    }
}

/// <summary>
/// Publié quand la jauge d'émotion ou la fusion d'un champion change (carte jouée, activation, début de tour)
/// </summary>
public class FusionChangedEvent : GameEvent
{
    public Champion Champion { get; private set; }

    public FusionChangedEvent(Champion champion)
    {
        Champion = champion;
    }
}

/// <summary>
/// Publié quand un retrait de PA/PM est programmé contre une unité (appliqué à son prochain tour)
/// </summary>
public class ResourceDebuffChangedEvent : GameEvent
{
    public Unit Target { get; private set; }

    public ResourceDebuffChangedEvent(Unit target)
    {
        Target = target;
    }
}

/// <summary>
/// Publié quand les zones annoncées d'un lancer de monstre changent : annoncées (cases couvertes), puis vidées
/// quand elles tombent, sont annulées (Sidération) ou que le monstre meurt (liste vide)
/// </summary>
public class ThrowZonesChangedEvent : GameEvent
{
    public Enemy Thrower { get; private set; }
    public IReadOnlyList<Vector2Int> Cells { get; private set; }

    public ThrowZonesChangedEvent(Enemy thrower, IReadOnlyList<Vector2Int> cells)
    {
        Thrower = thrower;
        Cells = cells;
    }
}
