using UnityEngine;
using System.Collections.Generic;

// Enum pour le coût de la carte
public enum CardCostType 
{ 
    None, 
    PA, 
    Other 
}

// Enum pour spécifier le type de cible valide
public enum CardTargetType
{
    None,           // Aucune
    Self,           // Soi-même
    Enemy,          // Un ou plusieurs ennemis
    Ally,           // Un ou plusieurs alliés (sauf soi-même)
    AllyOrSelf,     // Cible les alliés ET soi-même
    AllyorEnemy,    // Cible les alliés ET les ennemis
    AnyUnit,        // Cible n'importe quelle unité
    EmptyTile,      // Cible uniquement les tuiles vides
    AnyTile         // Cible n'importe quelle tuile (vide ou occupée)
}

public enum CardAreaEffect
{
    None,           // Aucune zone
    OneTile,        // Une case
    Line,           // Une ligne
    Cross,          // En croix
    Circle,         // En cercle
    Cone            // En cône

}

public enum CardAffectedTarget
{
    None,           // Aucune cible affectée
    Self,           // Soi-même
    Enemies,        // Que un ou plusieurs ennemies
    Ally,           // Que un ou plusieurs alliées
    AllyOrSelf,     // Que les alliés ET soi-même
    AllyorEnemy,    // Que les alliés ET les ennemis
    AnyUnit         // N'importe quelle unité
}

public enum CardFamilyType
{
    None,
    Dechaines,      // Rouge
    Dissidents,     // Vert foncé
    Insurgents,     // Jaune
    Exiles,         // Bleu foncé
    Reprouves,      // Violet
    Gardiens,       // Vert clair
    Eveilles,       // Bleu clair
    Precurseurs     // Orange
}

public enum CardClasseType
{
    None,
    Ancre,          //  Ancre
    Tisseur,        //  Tisseur
    Ombrelame,      //  Ombrelame
    Veilleur,       //  Veilleur
    Harmoniste      //  Harmoniste
}

public enum CardElementType
{
    None,
    Feu,        // Feu
    Ombre,      // Ombre
    Lumiere,    // Lumière
    Eau         // Eau
}

public enum CardEffectType
{
    None,       // Aucun
    Riposte,    // Riposte
    Taunt,      // Taunt
    Knockback,  // Knockback
    Debuff      // Debuff
}

/// <summary>
/// Mode de consommation de Rage pour les cartes
/// </summary>
public enum RageConsumeMode
{
    None,           // Pas de consommation de Rage
    Fixed,          // Consomme un montant fixe de Rage (rageCost)
    All             // Consomme TOUTE la Rage disponible
}

/// <summary>
/// Type de scaling des effets avec la Rage
/// </summary>
public enum RageScalingType
{
    Flat,           // Bonus fixe par Rage (ex: +10 heal par Rage)
    Percent         // Bonus en pourcentage par Rage (ex: +25% dégâts par Rage)
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identité de la carte")]
    // Nom de la carte
    public string cardName = "Nom de la Carte";
    [TextArea(3, 5)]
    // Description de la carte
    public string description = "Description de la carte.";
    // Type de famille
    public CardFamilyType familyType = CardFamilyType.None;
    // Type de classe
    public CardClasseType classeType = CardClasseType.None;
    // Type d'élément
    public CardElementType elementType = CardElementType.None;
    // Illustration de la carte
    public Sprite artwork;
    // Rage carte
    public bool isRageCard = false;

    [Header("Coût et ressources")]
    // Coût en PA (Points d'Action)
    public int costPA = 0; 
    public int costHP = 0; // Coût en Points de Vie
    public int costOther = 0;

    [Header("Type et cible")]
    // Type de cible valide
    public CardTargetType targetType = CardTargetType.None; 
    // Portée de la carte
    public int targetRange = 1; 

    // Propriétés dérivées pour compatibilité avec le code existant
    public bool targetsUnit => targetType == CardTargetType.Self || targetType == CardTargetType.Enemy || targetType == CardTargetType.Ally || targetType == CardTargetType.AllyOrSelf || targetType == CardTargetType.AnyUnit;
    public bool targetsTile => targetType == CardTargetType.EmptyTile || targetType == CardTargetType.AnyTile;
    public bool isAOE => areaEffect != CardAreaEffect.None && aoeRadius > 0;

    // Propriétés dérivées pour affectedTarget
    public bool affectsSelf => affectedTarget == CardAffectedTarget.Self || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsAllies => affectedTarget == CardAffectedTarget.Ally || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsEnemies => affectedTarget == CardAffectedTarget.Enemies || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;

    [Header("Zone d'effet ")]
    // Style de zone
    public CardAreaEffect areaEffect = CardAreaEffect.None;
    // Rayon de l'AOE
    public int aoeRadius = 0;
    // Cible affectée 
    public CardAffectedTarget affectedTarget = CardAffectedTarget.None;
 
    [Header("Effets principaux")]
    // Dégâts infligés par la carte
    public int damageAmount = 0;
    // Points de mouvement ajoutés ou déplacés
    public int movementAmount = 0;
    // Points de vie restaurés
    public int healAmount = 0;
    // Defense
    public int defenseAmount = 0;
    // Damage sur soi
    public int damageSelf =0;
    // Vol de vie (Soin sur le lanceur si dégâts infligés)
    public int lifestealFixedAmount = 0;
    // Augmentation de dommage
    public int atkIncreased = 0;
    // Durée du boost de stat
    public int statBoostDuration = 0;

    [Header("Effets secondaire")]
    //Riposte, Taunt, Knockback, etc
    public CardEffectType effectType = CardEffectType.None;
    // Distance de knockback (si effectType = Knockback)
    public int knockbackDistance = 1;
    // Si true, la carte est une charge : le lanceur se déplace vers la cible et repousse les ennemis sur le chemin
    public bool isChargeCard = false;
    // Nombre de carte à piocher
    public int drawAmount = 0; 
    // Carte spécifique à aller chercher dans le deck (Tutor)
    public CardData cardToFetch;
    // Nombre de copies à aller chercher
    public int fetchAmount = 0;

    [Header("Boost par Rage")]
    [Tooltip("Mode de consommation de Rage")]
    public RageConsumeMode rageMode = RageConsumeMode.None;
    [Tooltip("Coût en Rage (uniquement si mode = Fixed)")]
    public int rageCost = 0;
    [Tooltip("Type de scaling : Flat = bonus fixe par Rage, Percent = % par Rage")]
    public RageScalingType rageScaling = RageScalingType.Flat;
    [Tooltip("Bonus par Rage consommée (Flat: +X par Rage, Percent: +X% par Rage)")]
    public int rageBonus = 0;

    [Header("Génération de Cartes")]
    // Carte à ajouter au deck
    public CardData cardToAddToDeck;
    // Nombre de copie à ajouter
    public int cardsToAddCount = 0;

    // Méthode pour vérifier si une unité est une cible valide
    public bool IsValidTarget(Unit source, Unit target)
    {
        if (source == null) return false;

        switch (targetType)
        {
            case CardTargetType.None:
                return true; // Pas besoin de cible

            case CardTargetType.Self:
                return target != null && target == source;

            case CardTargetType.Enemy:
                return target != null && target.GetFaction() != source.GetFaction();

            case CardTargetType.Ally:
                return target != null && target != source && target.GetFaction() == source.GetFaction();

            case CardTargetType.AllyOrSelf:
                return target != null && target.GetFaction() == source.GetFaction();

            case CardTargetType.AnyUnit:
                return target != null;

            default:
                return false;
        }
    }

    // Méthode pour vérifier si une tuile est une cible valide
    public bool IsValidTileTarget(Tile tile)
    {
        switch (targetType)
        {
            case CardTargetType.None:
                return true;

            case CardTargetType.EmptyTile:
                // Vérifie qu'aucune unité n'occupe cette tuile
                return tile != null && !IsUnitOnTile(tile);

            case CardTargetType.AnyTile:
                return tile != null;

            default:
                return false;
        }
    }

    /// <summary>
    /// Vérifie si une tuile est une cible valide pour une carte de charge (en ligne droite uniquement)
    /// Accepte les cases vides OU les cases avec un ennemi
    /// </summary>
    public bool IsValidChargeTarget(Tile tile, Unit source)
    {
        if (tile == null || source == null) return false;
        if (!isChargeCard) return IsValidTileTarget(tile);

        Vector2 sourcePos = source.GetCurrentGridPos();
        Vector2 tilePos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);

        // La charge ne peut cibler qu'en ligne droite (horizontale ou verticale)
        bool isInLine = (sourcePos.x == tilePos.x || sourcePos.y == tilePos.y);
        if (!isInLine) return false;

        // Vérifie si la case est vide OU contient un ennemi
        Unit unitOnTile = Services.Grid.GetUnitAtGridPos(tilePos);
        if (unitOnTile == null)
        {
            return true; // Case vide = valide
        }
        else
        {
            // Case occupée : valide seulement si c'est un ennemi
            return unitOnTile.GetFaction() != source.GetFaction();
        }
    }

    // Méthode helper pour vérifier si une unité occupe une tuile
    private bool IsUnitOnTile(Tile tile)
    {
        // OPTIMISATION: Utilise GridRepository au lieu de FindObjectsByType
        Vector2 tilePos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);
        Unit unitOnTile = Services.Grid.GetUnitAtGridPos(tilePos);
        return unitOnTile != null;
    }

    // Méthode pour obtenir toutes les unités affectées par l'AOE
    public List<Unit> GetAOEAffectedUnits(Unit source, Vector2 epicenter)
    {
        List<Unit> affectedUnits = new List<Unit>();

        if (!isAOE || aoeRadius <= 0)
        {
            return affectedUnits;
        }

        // OPTIMISATION: Utilise GridRepository au lieu de FindObjectsByType
        List<Unit> allUnits = Services.Grid.GetAllUnits();

        foreach (Unit unit in allUnits)
        {
            Vector2 unitPos = unit.GetCurrentGridPos();
            float distance = Vector2.Distance(epicenter, unitPos);

            // Vérifie si l'unité est dans le rayon
            if (distance <= aoeRadius)
            {
                bool shouldAffect = false;

                // Vérifie si c'est le lanceur
                if (unit == source)
                {
                    shouldAffect = affectsSelf;
                }
                // Vérifie si c'est un allié
                else if (unit.GetFaction() == source.GetFaction())
                {
                    shouldAffect = affectsAllies;
                }
                // C'est un ennemi
                else
                {
                    shouldAffect = affectsEnemies;
                }

                if (shouldAffect)
                {
                    affectedUnits.Add(unit);
                }
            }
        }

        return affectedUnits;
    }

    // Méthode pour exécuter l'effet de la carte
    public virtual void ExecuteEffect(Unit source, Unit targetUnit = null, Vector2 targetTile = default)
    {
        Debug.Log($"Exécution de l'effet de la carte {cardName} par {source.name}.");

        // --- NOUVEAU : STOCKAGE DE RAGE ---
        // Si c'est une carte Rage, on l'ajoute au stock du lanceur (si c'est Ilya)
        if (isRageCard)
        {
            if (source is IRageUser rageUser)
            {
                rageUser.AddRageStock(1);
            }
        }

        // Variables locales pour les valeurs finales (modifiables par boost)
        int finalDamage = damageAmount;
        int finalHeal = healAmount;
        int finalDefense = defenseAmount;
        int finalDraw = drawAmount;
        int finalFetch = fetchAmount;
        int finalAtk = atkIncreased;
        int finalLifesteal = lifestealFixedAmount;

        // --- SYSTÈME DE BOOST PAR RAGE (Simplifié) ---
        int rageConsumed = 0;

        if (rageMode != RageConsumeMode.None && source is IlyaUnit ilyaPlayer)
        {
            int currentRage = ilyaPlayer.GetRageStock();

            // Détermine combien de Rage consommer
            if (rageMode == RageConsumeMode.All && currentRage > 0)
            {
                // Consomme TOUTE la Rage
                if (ilyaPlayer.ConsumeRageStock(currentRage))
                {
                    rageConsumed = currentRage;
                }
            }
            else if (rageMode == RageConsumeMode.Fixed && rageCost > 0 && currentRage >= rageCost)
            {
                // Consomme un montant fixe
                if (ilyaPlayer.ConsumeRageStock(rageCost))
                {
                    rageConsumed = rageCost;
                }
            }

            // Applique le boost si de la Rage a été consommée
            if (rageConsumed > 0 && rageBonus > 0)
            {
                if (rageScaling == RageScalingType.Flat)
                {
                    // Bonus FLAT : +rageBonus par Rage consommée
                    int totalBonus = rageBonus * rageConsumed;
                    
                    // CORRECTION : On applique le bonus uniquement aux valeurs de base non nulles
                    // Cela évite qu'une carte de Soin inflige des Dégâts (car damageAmount était 0 + bonus)
                    // ou qu'une carte de Dégâts soigne l'ennemi.
                    if (damageAmount > 0) finalDamage += totalBonus;
                    if (healAmount > 0) finalHeal += totalBonus;
                    if (defenseAmount > 0) finalDefense += totalBonus;
                    if (atkIncreased > 0) finalAtk += totalBonus;
                    if (lifestealFixedAmount > 0) finalLifesteal += totalBonus;
                    // Draw et Fetch restent inchangés (pas de scaling)

                    Debug.Log($"🔥 RAGE BOOST (Flat) ! {source.name} consomme {rageConsumed} Rage → +{totalBonus} aux effets");
                }
                else // RageScalingType.Percent
                {
                    // Bonus PERCENT : +rageBonus% par Rage consommée
                    float percentBonus = 1f + (rageBonus * rageConsumed / 100f);
                    finalDamage = Mathf.RoundToInt(finalDamage * percentBonus);
                    finalHeal = Mathf.RoundToInt(finalHeal * percentBonus);
                    finalDefense = Mathf.RoundToInt(finalDefense * percentBonus);
                    finalAtk = Mathf.RoundToInt(finalAtk * percentBonus);
                    finalLifesteal = Mathf.RoundToInt(finalLifesteal * percentBonus);

                    Debug.Log($"🔥 RAGE BOOST (Percent) ! {source.name} consomme {rageConsumed} Rage → x{percentBonus:F2} aux effets");
                }
            }
        }

        Debug.Log($"[CardData] {cardName} calculé : FinalHeal={finalHeal} (Base={healAmount} + Boost={finalHeal - healAmount}), RageConsumed={rageConsumed}");

        // Détermine l'épicentre de l'effet
        Vector2 effectEpicenter;
        if (targetsUnit && targetUnit != null)
        {
            effectEpicenter = targetUnit.GetCurrentGridPos();
        }
        else if (targetsTile)
        {
            effectEpicenter = targetTile;
        }
        else
        {
            effectEpicenter = source.GetCurrentGridPos();
        }

        // Applique l'effet AOE si activé
        if (isAOE && aoeRadius > 0)
        {
            List<Unit> affectedUnits = GetAOEAffectedUnits(source, effectEpicenter);
            Debug.Log($"🔥 AOE {cardName} : {affectedUnits.Count} unités affectées dans un rayon de {aoeRadius}");

            foreach (Unit unit in affectedUnits)
            {
                bool damageDealt = false;
                if (finalDamage > 0)
                {
                    int hpBefore = unit.GetHealth();
                    unit.TakeDamage(finalDamage);
                    if (unit.GetHealth() < hpBefore) damageDealt = true;
                    Debug.Log($"  → {unit.name} prend {finalDamage} dégâts AOE");
                }
                if (finalHeal > 0)
                {
                    unit.Heal(finalHeal);
                    Debug.Log($"  → {unit.name} récupère {finalHeal} PV AOE");
                }
                if (finalLifesteal > 0 && damageDealt)
                {
                    source.Heal(finalLifesteal);
                    Debug.Log($"  → {source.name} vole {finalLifesteal} PV à {unit.name} (AOE)");
                }
            }
        }
        // Sinon, applique l'effet sur la cible unique
        else
        {
            if (targetsUnit && targetUnit != null)
            {
                bool damageDealt = false;
                if (finalDamage > 0)
                {
                    int hpBefore = targetUnit.GetHealth();
                    targetUnit.TakeDamage(finalDamage);
                    if (targetUnit.GetHealth() < hpBefore) damageDealt = true;
                    Debug.Log($"{source.name} inflige {finalDamage} dégâts à {targetUnit.name} avec {cardName}.");
                }
                if (finalHeal > 0)
                {
                    targetUnit.Heal(finalHeal);
                    Debug.Log($"{source.name} soigne {targetUnit.name} de {finalHeal} PV avec {cardName}.");
                }
                if (finalLifesteal > 0 && damageDealt)
                {
                    source.Heal(finalLifesteal);
                    Debug.Log($"{source.name} vole {finalLifesteal} PV à {targetUnit.name}.");
                }
            }
            else if (targetType == CardTargetType.Self && finalHeal > 0)
            {
                source.Heal(finalHeal);
                Debug.Log($"{source.name} se soigne de {finalHeal} PV avec {cardName}.");
            }
        }

        // Dégâts sur soi-même (ex: cartes puissantes mais risquées)
        if (damageSelf > 0)
        {
            source.TakeDamage(damageSelf);
            Debug.Log($"{source.name} subit {damageSelf} dégâts de contrecoup avec {cardName}.");
        }

        if (movementAmount > 0)
        {
            // La logique de mouvement sera gérée par l'InputManager ou une autre entité
            // pour l'instant, nous pouvons juste loguer l'intention.
            Debug.Log($"{source.name} gagne {movementAmount} points de mouvement supplémentaires avec {cardName}.");
        }

        // --- NOUVELLES CAPACITÉS ---

        // 1. Pioche de cartes
        if (finalDraw > 0)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                deckManager.DrawCards(finalDraw);
            }
        }

        // 2. Aller chercher une carte spécifique (Fetch)
        if (cardToFetch != null && finalFetch > 0)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                deckManager.FetchCards(c => c == cardToFetch, finalFetch);
            }
        }

        // 4. Gain de Stats (Force / Défense)
        if (finalAtk != 0 || finalDefense != 0)
        {
            // Si la cible est définie, on l'utilise, sinon si c'est Self/None, c'est le lanceur
            Unit statTarget = (targetsUnit && targetUnit != null) ? targetUnit : source;

            // Applique les stats (nécessite la méthode ModifyStats sur Unit)
            statTarget.ModifyStats(finalAtk, finalDefense, statBoostDuration);
        }

        // 5. Ajout de cartes au deck (Génération de Rage ou autre)
        if (cardToAddToDeck != null && cardsToAddCount > 0)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                for (int i = 0; i < cardsToAddCount; i++)
                {
                    deckManager.AddCardToDeck(cardToAddToDeck);
                }

                deckManager.ShuffleDeck();

                Debug.Log($"{source.name} ajoute {cardsToAddCount}x {cardToAddToDeck.cardName} à son deck.");
            }
        }

        // 6. Knockback simple (sur la cible)
        if (effectType == CardEffectType.Knockback && !isChargeCard && targetUnit != null && knockbackDistance > 0)
        {
            Vector2 sourcePos = source.GetCurrentGridPos();
            Vector2 targetPos = targetUnit.GetCurrentGridPos();
            Vector2 knockbackDir = (targetPos - sourcePos).normalized;
            targetUnit.ApplyKnockback(knockbackDir, knockbackDistance);
        }
    }

    /// <summary>
    /// Exécute l'effet de charge : le lanceur se déplace vers la cible, s'arrête si un ennemi bloque le chemin et le repousse
    /// </summary>
    /// <param name="source">L'unité qui charge</param>
    /// <param name="targetTilePos">La position cible de la charge</param>
    /// <param name="onComplete">Callback appelé quand la charge est terminée</param>
    public void ExecuteChargeEffect(Unit source, Vector2 targetTilePos, System.Action onComplete = null)
    {
        if (!isChargeCard)
        {
            Debug.LogWarning($"{cardName} n'est pas une carte de charge!");
            onComplete?.Invoke();
            return;
        }

        Vector2 sourcePos = source.GetCurrentGridPos();

        // Calcule la direction de déplacement (ligne droite uniquement)
        Vector2 diff = targetTilePos - sourcePos;
        Vector2 stepDirection;

        if (Mathf.Abs(diff.x) > 0 && diff.y == 0)
        {
            // Mouvement horizontal
            stepDirection = new Vector2(Mathf.Sign(diff.x), 0);
        }
        else if (Mathf.Abs(diff.y) > 0 && diff.x == 0)
        {
            // Mouvement vertical
            stepDirection = new Vector2(0, Mathf.Sign(diff.y));
        }
        else
        {
            Debug.LogWarning($"Charge invalide : la cible n'est pas en ligne droite!");
            onComplete?.Invoke();
            return;
        }

        // Calcule le nombre de cases à parcourir (distance Manhattan)
        int totalDistance = Mathf.RoundToInt(Mathf.Abs(diff.x) + Mathf.Abs(diff.y));

        // Parcourt le chemin case par case
        List<Tile> chargePath = new List<Tile>();
        Vector2 currentPos = sourcePos;
        Unit enemyHit = null;

        Debug.Log($"🏃 CHARGE ! {source.name} de {sourcePos} vers {targetTilePos} (distance: {totalDistance}, direction: {stepDirection})");

        for (int i = 0; i < totalDistance; i++)
        {
            Vector2 nextPos = currentPos + stepDirection;

            Tile nextTile = Services.Grid.GetTileAtPosition(nextPos);
            if (nextTile == null)
            {
                // Bord de la grille
                Debug.Log($"🏃 CHARGE arrêtée : bord de grille à {nextPos}");
                break;
            }

            // Vérifie s'il y a une unité sur la case
            Unit unitOnTile = Services.Grid.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null)
            {
                if (unitOnTile.GetFaction() != source.GetFaction())
                {
                    // C'est un ennemi : on s'arrête AVANT et on le frappe
                    enemyHit = unitOnTile;
                    Debug.Log($"🏃 CHARGE ! {source.name} percute {enemyHit.name} à {nextPos}");
                }
                else
                {
                    // C'est un allié, on s'arrête devant
                    Debug.Log($"🏃 CHARGE arrêtée : allié {unitOnTile.name} à {nextPos}");
                }
                break;
            }

            // Case libre, on l'ajoute au chemin
            chargePath.Add(nextTile);
            currentPos = nextPos;
        }

        // Déplace le lanceur
        if (chargePath.Count > 0)
        {
            source.MoveToTile(chargePath);
            Debug.Log($"🏃 CHARGE ! {source.name} se déplace de {sourcePos} vers {currentPos}");
        }

        // Si un ennemi a été touché, applique le knockback et les dégâts
        if (enemyHit != null)
        {
            Debug.Log($"🏃 CHARGE ! Ennemi touché: {enemyHit.name}, knockback: {knockbackDistance}, dégâts: {damageAmount}");

            // Applique les dégâts de la charge
            if (damageAmount > 0)
            {
                enemyHit.TakeDamage(damageAmount);
                Debug.Log($"🏃 CHARGE ! {source.name} inflige {damageAmount} dégâts à {enemyHit.name}");
            }

            // Applique le knockback APRÈS les dégâts
            if (knockbackDistance > 0)
            {
                Debug.Log($"🏃 KNOCKBACK ! Direction: {stepDirection}, Distance: {knockbackDistance}");
                enemyHit.ApplyKnockback(stepDirection, knockbackDistance);
            }
        }
        else
        {
            Debug.Log($"🏃 CHARGE ! Aucun ennemi touché");
        }

        onComplete?.Invoke();
    }
}

/// <summary>
/// Classe helper pour obtenir les couleurs associées aux familles et éléments
/// </summary>
public static class CardVisualHelper
{
    /// <summary>
    /// Retourne la couleur associée à une famille
    /// </summary>
    public static Color GetFamilyColor(CardFamilyType family)
    {
        switch (family)
        {
            case CardFamilyType.Dechaines:
                return new Color(0.8f, 0f, 0f); // Rouge
            case CardFamilyType.Dissidents:
                return new Color(0f, 0.4f, 0f); // Vert foncé
            case CardFamilyType.Insurgents:
                return new Color(1f, 0.92f, 0f); // Jaune
            case CardFamilyType.Exiles:
                return new Color(0f, 0f, 0.5f); // Bleu foncé
            case CardFamilyType.Reprouves:
                return new Color(0.5f, 0f, 0.5f); // Violet
            case CardFamilyType.Gardiens:
                return new Color(0.5f, 1f, 0.5f); // Vert clair
            case CardFamilyType.Eveilles:
                return new Color(0.5f, 0.8f, 1f); // Bleu clair
            case CardFamilyType.Precurseurs:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Retourne la couleur associée à un élément (pour usage futur)
    /// </summary>
    public static Color GetElementColor(CardElementType element)
    {
        switch (element)
        {
            case CardElementType.Feu:
                return new Color(1f, 0.3f, 0f); // Rouge-orange (feu)
            case CardElementType.Ombre:
                return new Color(0.2f, 0f, 0.3f); // Violet foncé (ombre)
            case CardElementType.Lumiere:
                return new Color(1f, 1f, 0.7f); // Jaune clair (lumière)
            case CardElementType.Eau:
                return new Color(0f, 0.5f, 1f); // Bleu (eau)
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Retourne le nom français de la famille
    /// </summary>
    public static string GetFamilyName(CardFamilyType family)
    {
        switch (family)
        {
            case CardFamilyType.Dechaines: return "Déchaînés";
            case CardFamilyType.Dissidents: return "Dissidents";
            case CardFamilyType.Insurgents: return "Insurgents";
            case CardFamilyType.Exiles: return "Exilés";
            case CardFamilyType.Reprouves: return "Réprouvés";
            case CardFamilyType.Gardiens: return "Gardiens";
            case CardFamilyType.Eveilles: return "Éveillés";
            case CardFamilyType.Precurseurs: return "Précurseurs";
            default: return "Sans Famille";
        }
    }

    /// <summary>
    /// Retourne le nom français de l'élément
    /// </summary>
    public static string GetElementName(CardElementType element)
    {
        switch (element)
        {
            case CardElementType.Feu: return "Feu";
            case CardElementType.Ombre: return "Ombre";
            case CardElementType.Lumiere: return "Lumière";
            case CardElementType.Eau: return "Eau";
            default: return "Sans Élément";
        }
    }
}