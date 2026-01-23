using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Structure contenant les informations d'un chemin de charge
/// </summary>
public struct ChargePathInfo
{
    public bool IsValid;
    public Vector2 StepDirection;
    public int Distance;
    public List<Tile> Path;
    public Unit EnemyHit;

    public static ChargePathInfo Invalid => new ChargePathInfo { IsValid = false };
}

/// <summary>
/// Classe utilitaire pour les calculs de charge
/// </summary>
public static class ChargeHelper
{
    /// <summary>
    /// Calcule la direction de charge entre deux positions (ligne droite uniquement)
    /// </summary>
    /// <returns>La direction normalisée ou Vector2.zero si pas en ligne droite</returns>
    public static bool TryGetChargeDirection(Vector2 sourcePos, Vector2 targetPos, out Vector2 direction, out int distance)
    {
        Vector2 diff = targetPos - sourcePos;

        if (Mathf.Abs(diff.x) > 0 && diff.y == 0)
        {
            // Mouvement horizontal
            direction = new Vector2(Mathf.Sign(diff.x), 0);
            distance = Mathf.RoundToInt(Mathf.Abs(diff.x));
            return true;
        }
        else if (Mathf.Abs(diff.y) > 0 && diff.x == 0)
        {
            // Mouvement vertical
            direction = new Vector2(0, Mathf.Sign(diff.y));
            distance = Mathf.RoundToInt(Mathf.Abs(diff.y));
            return true;
        }

        direction = Vector2.zero;
        distance = 0;
        return false;
    }

    /// <summary>
    /// Calcule le chemin de charge complet, en s'arrêtant si une unité bloque
    /// </summary>
    public static ChargePathInfo CalculateChargePath(Vector2 sourcePos, Vector2 targetPos, Unit source)
    {
        if (!TryGetChargeDirection(sourcePos, targetPos, out Vector2 stepDirection, out int totalDistance))
        {
            return ChargePathInfo.Invalid;
        }

        List<Tile> chargePath = new List<Tile>();
        Vector2 currentPos = sourcePos;
        Unit enemyHit = null;

        for (int i = 0; i < totalDistance; i++)
        {
            Vector2 nextPos = currentPos + stepDirection;
            Tile nextTile = Services.Grid.GetTileAtPosition(nextPos);

            if (nextTile == null) break; // Bord de la grille

            Unit unitOnTile = Services.Grid.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null)
            {
                if (unitOnTile.GetFaction() != source.GetFaction())
                {
                    enemyHit = unitOnTile;
                }
                break; // On s'arrête devant toute unité
            }

            chargePath.Add(nextTile);
            currentPos = nextPos;
        }

        return new ChargePathInfo
        {
            IsValid = true,
            StepDirection = stepDirection,
            Distance = totalDistance,
            Path = chargePath,
            EnemyHit = enemyHit
        };
    }

    /// <summary>
    /// Vérifie si une position cible est valide pour une charge (chemin non bloqué)
    /// </summary>
    public static bool IsValidChargeTarget(Vector2 sourcePos, Vector2 targetPos, Unit source)
    {
        if (!TryGetChargeDirection(sourcePos, targetPos, out Vector2 stepDirection, out int distance))
        {
            return false;
        }

        // Vérifie chaque case sur le chemin jusqu'à la cible
        for (int i = 1; i <= distance; i++)
        {
            Vector2 checkPos = sourcePos + stepDirection * i;
            Unit unitOnPath = Services.Grid.GetUnitAtGridPos(checkPos);

            if (unitOnPath != null)
            {
                // Il y a une unité sur le chemin
                if (checkPos == targetPos)
                {
                    // C'est la case cible : valide seulement si c'est un ennemi
                    return unitOnPath.GetFaction() != source.GetFaction();
                }
                else
                {
                    // C'est une case intermédiaire : le chemin est bloqué
                    return false;
                }
            }
        }

        // Aucune unité sur le chemin = valide
        return true;
    }
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
    AnyTile,        // Cible n'importe quelle tuile (vide ou occupée)
    EnemyOrTile     // Cible un ennemi OU une tuile
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

/// <summary>
/// Les 8 Familles représentent les types d'émotions que les champions incarnent
/// Basé sur la Roue de Plutchik
/// </summary>
public enum CardFamilyType
{
    None,
    Dechaines,      // Colère - Rouge #CC0000
    Dissidents,     // Dégoût - Violet #800080
    Insurgents,     // Tristesse - Bleu foncé #000080
    Exiles,         // Surprise - Bleu clair #80CCFF
    Reprouves,      // Peur - Vert foncé #006600
    Gardiens,       // Confiance - Vert clair #80FF80
    Eveilles,       // Joie - Jaune #FFEB00
    Precurseurs     // Anticipation - Orange #FF8000
}

/// <summary>
/// Les 5 Classes représentent les différentes façons psychologiques de gérer une émotion
/// </summary>
public enum CardClasseType
{
    None,
    Reprime,        // Stocke - Répression, accumulation lente, explosions retardées
    Impulsif,       // Consomme - Catharsis, fluctuations rapides, burst
    Alchimiste,     // Transforme - Émotion → Ressource magique
    Emissaire,      // Déplace - Projection, transfert aux invocations
    Evade           // Fuit/Substitue - Émotion → Substances, risque/récompense
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
    Debuff,     // Debuff
    DamageShare // Partage de dégâts (le lanceur prend une partie des dégâts de la cible)
}

/// <summary>
/// Cibles pour la consommation de marques (indépendant du targetType de la carte)
/// </summary>
public enum MarkConsumeTarget
{
    CardTarget,     // Utilise le ciblage de la carte (targetType, AOE, etc.)
    AllEnemies,     // Tous les ennemis sur le terrain
    AllAllies,      // Tous les alliés sur le terrain
    AllUnits        // Toutes les unités sur le terrain
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
    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              1. IDENTITÉ                                   ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ IDENTITÉ ═══")]
    [Tooltip("Nom affiché de la carte")]
    public string cardName = "Nom de la Carte";

    [TextArea(3, 5)]
    [Tooltip("Description de l'effet de la carte")]
    public string description = "Description de la carte.";

    [Tooltip("Illustration de la carte")]
    public Sprite artwork;

    [Space(5)]
    [Tooltip("Famille émotionnelle (Roue de Plutchik)")]
    public CardFamilyType familyType = CardFamilyType.None;

    [Tooltip("Classe de gestion émotionnelle")]
    public CardClasseType classeType = CardClasseType.None;

    [Tooltip("Élément de la carte")]
    public CardElementType elementType = CardElementType.None;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              2. COÛTS                                      ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ COÛTS ═══")]
    [Tooltip("Coût en Points d'Action (payé avant l'effet)")]
    public int costPA = 0;

    [Tooltip("Coût en Points de Vie (payé avant l'effet)")]
    public int costHP = 0;

    [Tooltip("Coût en ressource spéciale (champion-spécifique)")]
    public int costOther = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              3. CIBLAGE                                    ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ CIBLAGE ═══")]
    [Tooltip("Type de cible valide pour la carte")]
    public CardTargetType targetType = CardTargetType.None;

    [Tooltip("Portée maximale de la carte")]
    public int targetRange = 0;

    [Space(5)]
    [Tooltip("Forme de la zone d'effet")]
    public CardAreaEffect areaEffect = CardAreaEffect.None;

    [Tooltip("Rayon de la zone d'effet")]
    public int aoeRadius = 0;

    [Tooltip("Types d'unités affectées dans la zone")]
    public CardAffectedTarget affectedTarget = CardAffectedTarget.None;

    // Propriétés dérivées pour compatibilité
    public bool targetsUnit => targetType == CardTargetType.Self || targetType == CardTargetType.Enemy || targetType == CardTargetType.Ally || targetType == CardTargetType.AllyOrSelf || targetType == CardTargetType.AnyUnit;
    public bool targetsTile => targetType == CardTargetType.EmptyTile || targetType == CardTargetType.AnyTile || targetType == CardTargetType.EnemyOrTile;
    public bool isAOE => areaEffect != CardAreaEffect.None && aoeRadius > 0;
    public bool affectsSelf => affectedTarget == CardAffectedTarget.Self || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsAllies => affectedTarget == CardAffectedTarget.Ally || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsEnemies => affectedTarget == CardAffectedTarget.Enemies || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         4. DÉGÂTS & SOIN                                   ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ DÉGÂTS & SOIN ═══")]
    [Tooltip("Dégâts infligés à la cible")]
    public int damageAmount = 0;

    [Tooltip("Dégâts infligés au lanceur après l'effet (contrecoup)")]
    public int damageSelf = 0;

    [Space(5)]
    [Tooltip("Points de vie restaurés à la cible")]
    public int healAmount = 0;

    [Tooltip("Points de vie volés si dégâts infligés")]
    public int lifestealFixedAmount = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         5. BUFFS & DÉBUFFS                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ BUFFS & DÉBUFFS ═══")]
    [Tooltip("Bonus d'attaque accordé")]
    public int atkIncreased = 0;

    [Tooltip("Points de défense accordés")]
    public int defenseAmount = 0;

    [Tooltip("Points de mouvement bonus accordés")]
    public int movementAmount = 0;

    [Tooltip("Durée des buffs/débuffs en tours")]
    public int effectDuration = 0;

    [Space(5)]
    [Tooltip("PA retirés à la cible")]
    public int paReduction = 0;

    [Tooltip("PM retirés à la cible")]
    public int pmReduction = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         6. EFFETS SPÉCIAUX                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ EFFETS SPÉCIAUX ═══")]
    [Tooltip("Type d'effet spécial")]
    public CardEffectType effectType = CardEffectType.None;

    [Space(5)]
    [Tooltip("Si true, le lanceur charge vers la cible")]
    public bool isChargeCard = false;

    [Tooltip("Distance de knockback/recul")]
    public int knockbackDistance = 0;

    [Space(5)]
    [Tooltip("Pourcentage de dégâts redirigés (si DamageShare)")]
    [Range(0, 100)]
    public int damageSharePercent = 50;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                            7. MARQUES                                      ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ MARQUES ═══")]
    [Tooltip("Type de marque à appliquer (None = pas de marque)")]
    public MarkType markToApply = MarkType.None;

    [Tooltip("Nombre de stacks de marque à appliquer")]
    public int markStacks = 1;

    [Tooltip("Durée de la marque (0 = permanent)")]
    public int markDuration = 0;

    [Tooltip("Valeur bonus stockée dans la marque (heal, dégâts...)")]
    public int markBonusValue = 0;

    [Space(5)]
    [Tooltip("Si true, consomme les marques au lieu d'en appliquer")]
    public bool consumeMarks = false;

    [Tooltip("Type de marque à consommer")]
    public MarkType markToConsume = MarkType.None;

    [Tooltip("Cibles pour la consommation (indépendant du targetType)")]
    public MarkConsumeTarget consumeMarkTarget = MarkConsumeTarget.CardTarget;

    [Tooltip("Dégâts par stack de marque consommée")]
    public int damagePerMarkStack = 0;

    [Tooltip("Soin sur le lanceur par marque présente sur la cible")]
    public int healSelfPerMarkOnTarget = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         8. GESTION DU DECK                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ GESTION DU DECK ═══")]
    [Tooltip("Nombre de cartes à piocher")]
    public int drawAmount = 0;

    [Space(5)]
    [Tooltip("Carte spécifique à chercher (Tutor)")]
    public CardData cardToFetch;

    [Tooltip("Nombre de copies à chercher")]
    public int fetchAmount = 0;

    [Space(5)]
    [Tooltip("Carte à ajouter au deck")]
    public CardData cardToAddToDeck;

    [Tooltip("Nombre de copies à ajouter")]
    public int cardsToAddCount = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         9. SYSTÈME RAGE (Ilya)                             ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ SYSTÈME RAGE ═══")]
    [Tooltip("Si true, cette carte génère 1 Rage quand jouée")]
    public bool isRageCard = false;

    [Space(5)]
    [Tooltip("Mode de consommation de Rage")]
    public RageConsumeMode rageMode = RageConsumeMode.None;

    [Tooltip("Coût en Rage (si mode = Fixed)")]
    public int rageCost = 0;

    [Space(5)]
    [Tooltip("Type de scaling (Flat = +X par Rage, Percent = +X% par Rage)")]
    public RageScalingType rageScaling = RageScalingType.Flat;

    [Tooltip("Bonus par Rage consommée")]
    public int rageBonus = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                       10. DÉGÂTS CONDITIONNELS                             ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ DÉGÂTS CONDITIONNELS ═══")]
    [Tooltip("Dégâts supplémentaires par debuff sur la cible")]
    public int damagePerDebuff = 0;

    // Alias pour compatibilité (anciennes propriétés → effectDuration)
    [System.Obsolete("Utiliser effectDuration à la place")]
    public int statBoostDuration { get => effectDuration; set => effectDuration = value; }
    [System.Obsolete("Utiliser effectDuration à la place")]
    public int resourceDebuffDuration { get => effectDuration; set => effectDuration = value; }

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

            case CardTargetType.EnemyOrTile:
                return target != null && target.GetFaction() != source.GetFaction();

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

            case CardTargetType.EnemyOrTile:
                return tile != null;

            default:
                return false;
        }
    }

    /// <summary>
    /// Vérifie si une tuile est une cible valide pour une carte de charge (en ligne droite uniquement)
    /// Accepte les cases vides OU les cases avec un ennemi, mais seulement si aucune unité ne bloque le chemin
    /// </summary>
    public bool IsValidChargeTarget(Tile tile, Unit source)
    {
        if (tile == null || source == null) return false;
        if (!isChargeCard) return IsValidTileTarget(tile);

        Vector2 sourcePos = source.GetCurrentGridPos();
        Vector2 tilePos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);

        return ChargeHelper.IsValidChargeTarget(sourcePos, tilePos, source);
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

        // Pour EnemyOrTile, si aucune unité n'est ciblée explicitement, on regarde sur la case cible
        if (targetType == CardTargetType.EnemyOrTile && targetUnit == null)
        {
            targetUnit = Services.Grid.GetUnitAtGridPos(targetTile);
            // On s'assure que c'est bien un ennemi
            if (targetUnit != null && targetUnit.GetFaction() == source.GetFaction())
                targetUnit = null;
        }

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
        if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
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
                int totalUnitDamage = finalDamage;
                if (damagePerDebuff > 0)
                {
                    totalUnitDamage += damagePerDebuff * unit.GetDebuffCount();
                }

                bool damageDealt = false;
                if (totalUnitDamage > 0)
                {
                    int hpBefore = unit.GetHealth();
                    unit.TakeDamage(totalUnitDamage);
                    if (unit.GetHealth() < hpBefore) damageDealt = true;
                    Debug.Log($"  → {unit.name} prend {totalUnitDamage} dégâts AOE");
                }
                if (finalHeal > 0)
                {
                    unit.Heal(finalHeal);
                    Debug.Log($"  → {unit.name} récupère {finalHeal} PV AOE");
                }
                if (healSelfPerMarkOnTarget > 0)
                {
                    int markCount = unit.GetTotalMarkCount();
                    int healSelf = markCount * healSelfPerMarkOnTarget;
                    if (healSelf > 0)
                    {
                        source.Heal(healSelf);
                        Debug.Log($"  → {source.name} récupère {healSelf} PV (AOE sur {unit.name})");
                    }
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
            if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                int totalTargetDamage = finalDamage;
                if (damagePerDebuff > 0)
                {
                    totalTargetDamage += damagePerDebuff * targetUnit.GetDebuffCount();
                }

                bool damageDealt = false;
                if (totalTargetDamage > 0)
                {
                    int hpBefore = targetUnit.GetHealth();
                    targetUnit.TakeDamage(totalTargetDamage);
                    if (targetUnit.GetHealth() < hpBefore) damageDealt = true;
                    Debug.Log($"{source.name} inflige {totalTargetDamage} dégâts à {targetUnit.name} avec {cardName}.");
                }
                if (finalHeal > 0)
                {
                    targetUnit.Heal(finalHeal);
                    Debug.Log($"{source.name} soigne {targetUnit.name} de {finalHeal} PV avec {cardName}.");
                }
                if (healSelfPerMarkOnTarget > 0)
                {
                    int markCount = targetUnit.GetTotalMarkCount();
                    int healSelf = markCount * healSelfPerMarkOnTarget;
                    if (healSelf > 0)
                    {
                        source.Heal(healSelf);
                        Debug.Log($"{source.name} récupère {healSelf} PV grâce aux marques sur {targetUnit.name} ({markCount} marques).");
                    }
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
            Unit statTarget = ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null) ? targetUnit : source;

            // Applique les stats (nécessite la méthode ModifyStats sur Unit)
            statTarget.ModifyStats(finalAtk, finalDefense, effectDuration);
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

        // 7. Réduction de PA/PM sur la cible
        if (paReduction > 0 || pmReduction > 0)
        {
            // Détermine les cibles pour la réduction
            List<Unit> debuffTargets = new List<Unit>();

            if (isAOE && aoeRadius > 0)
            {
                debuffTargets = GetAOEAffectedUnits(source, effectEpicenter);
            }
            else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                debuffTargets.Add(targetUnit);
            }

            foreach (Unit debuffTarget in debuffTargets)
            {
                // Utilise le ResourceDebuffManager pour gérer la durée
                ResourceDebuffManager.ApplyDebuff(debuffTarget, paReduction, pmReduction, effectDuration, source);
            }
        }

        // 8. Système de Marques
        // 8a. Consommation de marques (doit être fait AVANT l'application pour éviter de consommer ce qu'on vient d'appliquer)
        if (consumeMarks && markToConsume != MarkType.None)
        {
            // Cas spécial : Stigmate utilise son propre système avec heal et perte de PA
            if (markToConsume == MarkType.Stigmate)
            {
                // Consomme tous les Stigmates appliqués par ce champion
                // healAmount de la carte = heal par ennemi marqué
                int consumedCount = StigmateManager.ConsumeAllStigmates(source, healAmount);
                Debug.Log($"🎯 STIGMATE ! {consumedCount} marque(s) consommée(s), heal: {healAmount} par marque");
            }
            else
            {
                // Système générique pour les autres types de marques (incluant AllMarks)
                List<Unit> markTargets = new List<Unit>();

                // Détermine les cibles en fonction de consumeMarkTarget
                switch (consumeMarkTarget)
                {
                    case MarkConsumeTarget.AllEnemies:
                        markTargets = Services.Grid?.GetAllEnemyUnits() ?? new List<Unit>();
                        break;

                    case MarkConsumeTarget.AllAllies:
                        markTargets = Services.Grid?.GetAllPlayerUnits() ?? new List<Unit>();
                        break;

                    case MarkConsumeTarget.AllUnits:
                        markTargets = Services.Grid?.GetAllUnits() ?? new List<Unit>();
                        break;

                    case MarkConsumeTarget.CardTarget:
                    default:
                        // Utilise le ciblage standard de la carte
                        if (isAOE && aoeRadius > 0)
                        {
                            markTargets = GetAOEAffectedUnits(source, effectEpicenter);
                        }
                        else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
                        {
                            markTargets.Add(targetUnit);
                        }
                        break;
                }

                int totalHeal = 0;

                foreach (Unit markTarget in markTargets)
                {
                    // AllMarks : consomme TOUTES les marques de tous types sur cette cible
                    if (markToConsume == MarkType.AllMarks)
                    {
                        List<UnitMark> consumedMarks = markTarget.ConsumeAllMarksFromSource(source);

                        foreach (UnitMark consumedMark in consumedMarks)
                        {
                            // Si c'est un Stigmate, applique l'effet spécial (perte de PA)
                            if (consumedMark.markType == MarkType.Stigmate)
                            {
                                StigmateManager.RegisterPALossPublic(markTarget, StigmateManager.PA_LOSS_ON_CONSUME);
                            }

                            // Dégâts par stack
                            int bonusDamage = consumedMark.stacks * damagePerMarkStack;
                            if (bonusDamage > 0)
                            {
                                markTarget.TakeDamage(bonusDamage);
                                Debug.Log($"🎯 MARQUE CONSOMMÉE ! {source.name} inflige {bonusDamage} dégâts bonus à {markTarget.name} ({consumedMark.stacks} stacks de {consumedMark.markType})");
                            }

                            // Bonus de la marque
                            if (consumedMark.bonusValue > 0)
                            {
                                int markBonus = consumedMark.bonusValue * consumedMark.stacks;
                                markTarget.TakeDamage(markBonus);
                                Debug.Log($"🎯 BONUS DE MARQUE ! {markBonus} dégâts supplémentaires");
                            }

                            // Heal par marque consommée
                            if (healAmount > 0)
                            {
                                totalHeal += healAmount;
                            }
                        }
                    }
                    // Type de marque spécifique
                    else if (markTarget.HasMark(markToConsume))
                    {
                        // Consomme uniquement les marques appliquées par ce champion
                        UnitMark consumedMark = markTarget.ConsumeMarkFromSource(markToConsume, source);

                        if (consumedMark.markType != MarkType.None)
                        {
                            // Calcule les dégâts bonus basés sur les stacks
                            int bonusDamage = consumedMark.stacks * damagePerMarkStack;

                            if (bonusDamage > 0)
                            {
                                markTarget.TakeDamage(bonusDamage);
                                Debug.Log($"🎯 MARQUE CONSOMMÉE ! {source.name} inflige {bonusDamage} dégâts bonus à {markTarget.name} ({consumedMark.stacks} stacks de {markToConsume})");
                            }

                            // Bonus supplémentaire de la marque
                            if (consumedMark.bonusValue > 0)
                            {
                                markTarget.TakeDamage(consumedMark.bonusValue * consumedMark.stacks);
                                Debug.Log($"🎯 BONUS DE MARQUE ! {consumedMark.bonusValue * consumedMark.stacks} dégâts supplémentaires");
                            }

                            // Heal par marque consommée
                            if (healAmount > 0)
                            {
                                totalHeal += healAmount;
                            }
                        }
                    }
                }

                // Applique le heal total au lanceur
                if (totalHeal > 0)
                {
                    source.Heal(totalHeal);
                    Debug.Log($"🎯 {source.name} récupère {totalHeal} PV (marques consommées)");
                }
            }
        }

        // 8b. Application de nouvelles marques
        if (markToApply != MarkType.None && markStacks > 0)
        {
            // Détermine les cibles pour l'application
            List<Unit> markTargets = new List<Unit>();

            if (isAOE && aoeRadius > 0)
            {
                markTargets = GetAOEAffectedUnits(source, effectEpicenter);
            }
            else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                markTargets.Add(targetUnit);
            }

            foreach (Unit markTarget in markTargets)
            {
                // Cas spécial : Stigmate utilise son propre système avec limite de 3
                if (markToApply == MarkType.Stigmate)
                {
                    StigmateManager.ApplyStigmate(source, markTarget, markBonusValue);
                }
                else
                {
                    // Système générique pour les autres types de marques
                    markTarget.ApplyMark(markToApply, source, markStacks, markDuration, markBonusValue);
                    Debug.Log($"🎯 MARQUE APPLIQUÉE ! {source.name} marque {markTarget.name} avec {markToApply} ({markStacks} stack(s), durée: {(markDuration == 0 ? "permanent" : markDuration + " tours")})");
                }
            }
        }

        // 9. Partage de Dégâts (Damage Share)
        if (effectType == CardEffectType.DamageShare && targetUnit != null)
        {
            // Applique le lien de partage de dégâts sur la cible vers le lanceur
            // Ratio basé sur damageSharePercent, durée basée sur statBoostDuration (défaut 1 tour si 0)
            float ratio = Mathf.Clamp01(damageSharePercent / 100f);
            targetUnit.SetDamageShare(source, ratio, effectDuration > 0 ? effectDuration : 1);
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

        // Lance la coroutine via le MonoBehaviour source
        source.StartCoroutine(ExecuteChargeEffectCoroutine(source, targetTilePos, onComplete));
    }

    /// <summary>
    /// Coroutine qui exécute l'effet de charge avec attente du mouvement
    /// </summary>
    private System.Collections.IEnumerator ExecuteChargeEffectCoroutine(Unit source, Vector2 targetTilePos, System.Action onComplete)
    {
        Vector2 sourcePos = source.GetCurrentGridPos();

        // Utilise le helper pour calculer le chemin de charge
        ChargePathInfo pathInfo = ChargeHelper.CalculateChargePath(sourcePos, targetTilePos, source);

        if (!pathInfo.IsValid)
        {
            Debug.LogWarning($"Charge invalide : la cible n'est pas en ligne droite!");
            onComplete?.Invoke();
            yield break;
        }

        Debug.Log($"🏃 CHARGE ! {source.name} de {sourcePos} vers {targetTilePos} (distance: {pathInfo.Distance}, direction: {pathInfo.StepDirection})");

        // Déplace le lanceur
        if (pathInfo.Path.Count > 0)
        {
            source.MoveToTile(pathInfo.Path);
            Debug.Log($"🏃 CHARGE ! {source.name} se déplace ({pathInfo.Path.Count} cases)");

            // Attend que le mouvement soit terminé
            while (source.IsMoving())
            {
                yield return null;
            }
        }

        // Si un ennemi a été touché, applique le knockback et les dégâts
        if (pathInfo.EnemyHit != null)
        {
            Debug.Log($"🏃 CHARGE ! Ennemi touché: {pathInfo.EnemyHit.name}, knockback: {knockbackDistance}, dégâts: {damageAmount}");

            // Applique les dégâts de la charge
            if (damageAmount > 0)
            {
                pathInfo.EnemyHit.TakeDamage(damageAmount);
                Debug.Log($"🏃 CHARGE ! {source.name} inflige {damageAmount} dégâts à {pathInfo.EnemyHit.name}");
            }

            // Applique le knockback APRÈS les dégâts et APRÈS le mouvement
            if (knockbackDistance > 0)
            {
                Debug.Log($"🏃 KNOCKBACK ! Direction: {pathInfo.StepDirection}, Distance: {knockbackDistance}");
                pathInfo.EnemyHit.ApplyKnockback(pathInfo.StepDirection, knockbackDistance);

                // Attend que le knockback soit terminé
                while (pathInfo.EnemyHit.IsMoving())
                {
                    yield return null;
                }
            }
        }
        else
        {
            Debug.Log($"🏃 CHARGE ! Aucun ennemi touché");
        }

        // Rafraîchit l'affichage de la portée de mouvement après la charge et le knockback
        EventBus.Publish(new ShowMovementRangeEvent(source));

        onComplete?.Invoke();
    }
}

/// <summary>
/// Classe helper pour obtenir les couleurs associées aux familles et éléments
/// </summary>
public static class CardVisualHelper
{
    /// <summary>
    /// Retourne la couleur associée à une famille (codes hex du GDD v3.0)
    /// </summary>
    public static Color GetFamilyColor(CardFamilyType family)
    {
        switch (family)
        {
            case CardFamilyType.Dechaines:
                return new Color(204f/255f, 0f, 0f);           // Colère - Rouge #CC0000
            case CardFamilyType.Dissidents:
                return new Color(128f/255f, 0f, 128f/255f);    // Dégoût - Violet #800080
            case CardFamilyType.Insurgents:
                return new Color(0f, 0f, 128f/255f);           // Tristesse - Bleu foncé #000080
            case CardFamilyType.Exiles:
                return new Color(128f/255f, 204f/255f, 1f);    // Surprise - Bleu clair #80CCFF
            case CardFamilyType.Reprouves:
                return new Color(0f, 102f/255f, 0f);           // Peur - Vert foncé #006600
            case CardFamilyType.Gardiens:
                return new Color(128f/255f, 1f, 128f/255f);    // Confiance - Vert clair #80FF80
            case CardFamilyType.Eveilles:
                return new Color(1f, 235f/255f, 0f);           // Joie - Jaune #FFEB00
            case CardFamilyType.Precurseurs:
                return new Color(1f, 128f/255f, 0f);           // Anticipation - Orange #FF8000
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
    /// Retourne l'émotion associée à une famille (Roue de Plutchik)
    /// </summary>
    public static string GetFamilyEmotion(CardFamilyType family)
    {
        switch (family)
        {
            case CardFamilyType.Dechaines: return "Colère";
            case CardFamilyType.Dissidents: return "Dégoût";
            case CardFamilyType.Insurgents: return "Tristesse";
            case CardFamilyType.Exiles: return "Surprise";
            case CardFamilyType.Reprouves: return "Peur";
            case CardFamilyType.Gardiens: return "Confiance";
            case CardFamilyType.Eveilles: return "Joie";
            case CardFamilyType.Precurseurs: return "Anticipation";
            default: return "Neutre";
        }
    }

    /// <summary>
    /// Retourne le nom français de la classe
    /// </summary>
    public static string GetClassName(CardClasseType classe)
    {
        switch (classe)
        {
            case CardClasseType.Reprime: return "Réprimé";
            case CardClasseType.Impulsif: return "Impulsif";
            case CardClasseType.Alchimiste: return "Alchimiste";
            case CardClasseType.Emissaire: return "Émissaire";
            case CardClasseType.Evade: return "Évadé";
            default: return "Sans Classe";
        }
    }

    /// <summary>
    /// Retourne la description de la classe (comment elle gère l'émotion)
    /// </summary>
    public static string GetClassDescription(CardClasseType classe)
    {
        switch (classe)
        {
            case CardClasseType.Reprime: return "Stocke l'émotion";
            case CardClasseType.Impulsif: return "Consomme l'émotion";
            case CardClasseType.Alchimiste: return "Transforme l'émotion";
            case CardClasseType.Emissaire: return "Déplace l'émotion";
            case CardClasseType.Evade: return "Fuit l'émotion";
            default: return "";
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