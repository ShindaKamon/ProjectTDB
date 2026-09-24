using UnityEngine;

/// <summary>
/// Validateur centralisé pour toutes les actions du jeu (cartes, déplacements, ciblage).
/// Fournit des messages d'erreur clairs et cohérents.
/// Pattern: Validator avec Result Type
/// </summary>
public static class GameActionValidator
{
    // ========== VALIDATION DES CARTES ==========

    /// <summary>
    /// Valide qu'une unité peut jouer une carte donnée
    /// </summary>
    public static ValidationResult CanPlayCard(Unit player, CardData card)
    {
        // Validation des paramètres
        if (player == null)
            return ValidationResult.Fail("Joueur null - impossible de jouer une carte");

        if (card == null)
            return ValidationResult.Fail("Carte null - aucune carte sélectionnée");

        // Carte de déplacement d'invocation (ex: Écho évanescent) : il faut une invocation à déplacer
        if (card.isRepositionSummonCard && !HasSummonToMove(player))
            return ValidationResult.Fail("Aucune invocation à déplacer");

        // Coût effectif (tient compte d'un éventuel override, ex: Triche)
        int effectiveCostPA = card.costPA;
        if (player.TryGetComponentSafe(out DeckManager deckManager))
        {
            effectiveCostPA = deckManager.GetEffectiveCost(card);
        }

        // Validation des PA (uniquement pour IActionPointsUser)
        if (effectiveCostPA > 0)
        {
            if (player is IActionPointsUser paUser)
            {
                if (paUser.GetCurrentPA() < effectiveCostPA)
                {
                    return ValidationResult.Fail($"PA insuffisants : {paUser.GetCurrentPA()}/{effectiveCostPA} requis");
                }
            }
            else
            {
                return ValidationResult.Fail($"{player.name} ne peut pas dépenser de PA");
            }
        }

        // Validation des PV (Coût HP)
        if (card.costHP > 0)
        {
            if (player.GetHealth() < card.costHP)
            {
                return ValidationResult.Fail($"PV insuffisants : {player.GetHealth()}/{card.costHP} requis");
            }
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide qu'une carte peut cibler une unité spécifique
    /// </summary>
    public static ValidationResult CanTargetUnit(CardData card, Unit source, Unit target)
    {
        // Validation des paramètres
        if (card == null)
            return ValidationResult.Fail("Carte null - impossible de valider le ciblage");

        if (source == null)
            return ValidationResult.Fail("Source null - impossible de valider le ciblage");

        // Si la carte ne cible pas d'unité, c'est OK
        if (!card.targetsUnit)
            return ValidationResult.Success();

        // Si la carte cible une unité mais qu'aucune cible n'est fournie
        if (target == null)
            return ValidationResult.Fail($"{card.cardName} nécessite une cible");

        // Validation de la portée (4 directions, voir GridGeometry)
        int distance = GridGeometry.Distance(source.GetCurrentGridPos(), target.GetCurrentGridPos());
        if (distance > card.targetRange)
        {
            return ValidationResult.Fail($"{card.cardName} hors de portée : {distance}/{card.targetRange}");
        }

        // Validation du type de cible (allié/ennemi)
        bool isAlly = source.GetFaction() == target.GetFaction();
        bool isEnemy = source.GetFaction() != target.GetFaction();
        bool isSelf = target == source;

        switch (card.targetType)
        {
            case CardTargetType.Enemy:
                if (!isEnemy)
                    return ValidationResult.Fail($"{card.cardName} ne peut cibler que les ennemis");
                break;

            case CardTargetType.EnemyOrTile:
                if (!isEnemy)
                    return ValidationResult.Fail($"{card.cardName} ne peut cibler que les ennemis (ou une tuile)");
                break;

            case CardTargetType.Ally:
                // Allié SAUF soi-même
                if (!isAlly || isSelf)
                    return ValidationResult.Fail($"{card.cardName} ne peut cibler que les alliés (pas soi-même)");
                break;

            case CardTargetType.AllyOrSelf:
                // Allié OU soi-même (même faction)
                if (!isAlly)
                    return ValidationResult.Fail($"{card.cardName} ne peut cibler que les alliés ou soi-même");
                break;

            case CardTargetType.Self:
                if (!isSelf)
                    return ValidationResult.Fail($"{card.cardName} ne peut cibler que soi-même");
                break;

            case CardTargetType.AnyUnit:
            case CardTargetType.AllyorEnemy:
                // N'importe quelle cible est valide
                break;

            case CardTargetType.None:
            case CardTargetType.EmptyTile:
            case CardTargetType.AnyTile:
                // Ces types ne ciblent pas d'unités, ne devraient pas arriver ici
                // mais on les accepte quand même (la validation targetsUnit a déjà filtré)
                break;
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide qu'une carte peut cibler une tuile spécifique
    /// </summary>
    public static ValidationResult CanTargetTile(CardData card, Unit source, Vector2Int targetTilePos)
    {
        // Validation des paramètres
        if (card == null)
            return ValidationResult.Fail("Carte null - impossible de valider le ciblage");

        if (source == null)
            return ValidationResult.Fail("Source null - impossible de valider le ciblage");

        // Si la carte ne cible pas de tuile, c'est OK
        if (!card.targetsTile)
            return ValidationResult.Success();

        Vector2Int sourcePos = source.GetCurrentGridPos();

        // Charge : ligne droite uniquement (même ligne ou même colonne)
        if (card.isChargeCard && !GridGeometry.TryGetLine(sourcePos, targetTilePos, out _, out _))
        {
            return ValidationResult.Fail($"{card.cardName} ne peut cibler qu'en ligne droite");
        }

        // Portée en 4 directions (voir GridGeometry)
        int distance = GridGeometry.Distance(sourcePos, targetTilePos);
        if (distance > card.targetRange)
        {
            return ValidationResult.Fail($"{card.cardName} hors de portée : {distance}/{card.targetRange}");
        }

        return ValidationResult.Success();
    }

    // ========== CARTES DE DÉPLACEMENT D'INVOCATION (ex: Écho évanescent) ==========
    // Ciblage en 2 étapes : 1) choisir une invocation du lanceur, 2) choisir la case d'arrivée,
    // à portée de l'INVOCATION (pas du lanceur).

    /// <summary>Le lanceur a-t-il au moins une invocation vivante à déplacer ?</summary>
    public static bool HasSummonToMove(Unit caster)
    {
        return caster is ISummonOwner owner && owner.ActiveSummon != null && !IsDead(owner.ActiveSummon);
    }

    /// <summary>Étape 1 : la cible cliquée est-elle une invocation vivante du lanceur ?</summary>
    public static ValidationResult CanSelectSummonToMove(CardData card, Unit caster, Unit target)
    {
        if (card == null || !card.isRepositionSummonCard)
            return ValidationResult.Fail("Cette carte ne déplace pas d'invocation");

        if (!(target is SummonUnit summon))
            return ValidationResult.Fail("Choisis d'abord une de tes invocations");

        if (summon.Owner != caster)
            return ValidationResult.Fail($"{summon.name} n'est pas une de tes invocations");

        if (IsDead(summon))
            return ValidationResult.Fail($"{summon.name} est morte");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Étape 2 : la case d'arrivée est-elle valide ? Distance en cases (4 directions, comme toute
    /// la grille) entre 1 et la portée de la carte, mesurée depuis l'invocation. L'existence et
    /// la disponibilité de la case sont fournies par l'appelant (la grille), pour garder la règle testable.
    /// </summary>
    public static ValidationResult CanMoveSummonTo(CardData card, SummonUnit summon, Vector2Int destination, bool destinationIsFree)
    {
        if (card == null || summon == null)
            return ValidationResult.Fail("Carte ou invocation manquante");

        Vector2Int from = summon.GetCurrentGridPos();
        int distance = GridGeometry.Distance(from, destination);

        if (distance < 1)
            return ValidationResult.Fail($"{summon.name} est déjà sur cette case");

        if (distance > card.targetRange)
            return ValidationResult.Fail($"Case trop loin de {summon.name} : {distance}/{card.targetRange}");

        if (!destinationIsFree)
            return ValidationResult.Fail("La case d'arrivée doit être libre");

        return ValidationResult.Success();
    }

    private static bool IsDead(Unit unit)
    {
        UnitState state = unit.GetUnitState();
        return state != null && state.IsDead();
    }

    // ========== VALIDATION DES DÉPLACEMENTS ==========

    /// <summary>
    /// Valide qu'une unité peut se déplacer
    /// </summary>
    public static ValidationResult CanMove(Unit unit)
    {
        // Validation des paramètres
        if (unit == null)
            return ValidationResult.Fail("Unité null - impossible de se déplacer");

        // Validation des PM restants
        if (unit.GetCurrentMovementPoints() <= 0)
        {
            return ValidationResult.Fail($"{unit.name} n'a plus de PM");
        }

        // Validation que l'unité n'est pas déjà en mouvement
        if (unit.IsMoving())
        {
            return ValidationResult.Fail($"{unit.name} est déjà en train de se déplacer");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide qu'une unité peut se déplacer vers une destination spécifique
    /// </summary>
    public static ValidationResult CanMoveToTile(Unit unit, Vector2Int destinationPos, int pathCost)
    {
        // Validation de base
        ValidationResult baseCheck = CanMove(unit);
        if (!baseCheck.IsValid)
            return baseCheck;

        // Validation du coût du chemin
        if (pathCost > unit.GetCurrentMovementPoints())
        {
            return ValidationResult.Fail($"Destination trop loin : {pathCost} PM requis, {unit.GetCurrentMovementPoints()} disponibles");
        }

        // Validation que la destination n'est pas la position actuelle
        if (unit.GetCurrentGridPos() == destinationPos)
        {
            return ValidationResult.Fail("Vous êtes déjà sur cette case");
        }

        return ValidationResult.Success();
    }

    // ========== VALIDATION DES TOURS ==========

    /// <summary>
    /// Valide que c'est le tour de l'unité
    /// </summary>
    public static ValidationResult IsUnitTurn(Unit unit, Unit activeUnit)
    {
        // Validation des paramètres
        if (unit == null)
            return ValidationResult.Fail("Unité null - impossible de vérifier le tour");

        if (activeUnit == null)
            return ValidationResult.Fail("Aucune unité active - tour invalide");

        // Validation que c'est bien le tour de cette unité
        if (unit != activeUnit)
        {
            return ValidationResult.Fail($"Ce n'est pas le tour de {unit.name}");
        }

        return ValidationResult.Success();
    }

    // ========== VALIDATION DES DONNÉES ==========

    /// <summary>
    /// Valide qu'un ChampionData est valide
    /// </summary>
    public static ValidationResult ValidateChampionData(ChampionData data)
    {
        if (data == null)
            return ValidationResult.Fail("ChampionData null");

        if (data.maxHealth <= 0)
            return ValidationResult.Fail($"ChampionData '{data.championName}' : maxHealth doit être > 0");

        if (data.movementRange < 0)
            return ValidationResult.Fail($"ChampionData '{data.championName}' : movementRange doit être >= 0");

        if (data.maxActionPoints < 0)
            return ValidationResult.Fail($"ChampionData '{data.championName}' : maxActionPoints doit être >= 0");

        if (data.prefab == null)
            return ValidationResult.Fail($"ChampionData '{data.championName}' : prefab manquant");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide qu'un EnemyData est valide
    /// </summary>
    public static ValidationResult ValidateEnemyData(EnemyData data)
    {
        if (data == null)
            return ValidationResult.Fail("EnemyData null");

        if (data.maxHealth <= 0)
            return ValidationResult.Fail($"EnemyData '{data.enemyName}' : maxHealth doit être > 0");

        if (data.movementRange < 0)
            return ValidationResult.Fail($"EnemyData '{data.enemyName}' : movementRange doit être >= 0");

        if (data.maxActionPoints < 0)
            return ValidationResult.Fail($"EnemyData '{data.enemyName}' : maxActionPoints doit être >= 0");

        if (data.prefab == null)
            return ValidationResult.Fail($"EnemyData '{data.enemyName}' : prefab manquant");

        if (data.combatDeck == null || data.combatDeck.Count == 0)
            return ValidationResult.Fail($"EnemyData '{data.enemyName}' : combatDeck vide");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide qu'un CardData est valide
    /// </summary>
    public static ValidationResult ValidateCardData(CardData card)
    {
        if (card == null)
            return ValidationResult.Fail("CardData null");

        if (card.costPA < 0)
            return ValidationResult.Fail($"CardData '{card.cardName}' : costPA doit être >= 0");

        if (card.targetRange < 0)
            return ValidationResult.Fail($"CardData '{card.cardName}' : targetRange doit être >= 0");

        if (card.targetCount < 1)
            return ValidationResult.Fail($"CardData '{card.cardName}' : targetCount doit être >= 1");

        return ValidationResult.Success();
    }
}
