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

public enum CardDamageType
{
    None,           // Aucun
    Physical,       // Dommage physique
    Magical         // Dommage magique

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
    // Type de dommage
    public CardDamageType damageType = CardDamageType.None;
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
    // Augmentation de dommage
    public int atkIncreased = 0;
    // Durée du boost de stat
    public int statBoostDuration = 0;

    [Header("Effets secondaire")]
    //Riposte, Taunt, Knockback, etc
    public CardEffectType effectType = CardEffectType.None;
    // Nombre de carte à piocher
    public int drawAmount = 0; 
    // Carte spécifique à aller chercher dans le deck (Tutor)
    public CardData cardToFetch;
    // Nombre de copies à aller chercher
    public int fetchAmount = 0;

    [Header("Boost par Rage")]
    [Tooltip("Rage requise pour activer le boost (0 = pas de boost, -1 = consomme TOUTE la Rage)")]
    public int rageRequired = 0;
    [Tooltip("Multiplicateur des effets si boost activé (ex: 2 = double les dégâts/soins/défense)")]
    public float rageBoostMultiplier = 1f;
    [Tooltip("Bonus fixe ajouté PAR Rage consommée (utile avec rageRequired = -1)")]
    public int rageBonusPerStack = 0;

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

        // --- SYSTÈME DE BOOST PAR RAGE ---
        int rageConsumed = 0;
        bool boostActivated = false;

        if (rageRequired != 0 && source is IlyaUnit ilyaPlayer)
        {
            int currentRage = ilyaPlayer.GetRageStock();

            // Mode "Consommer TOUTE la Rage" (rageRequired = -1)
            if (rageRequired == -1 && currentRage > 0)
            {
                if (ilyaPlayer.ConsumeRageStock(currentRage))
                {
                    rageConsumed = currentRage;
                    boostActivated = true;
                }
            }
            // Mode "Consommer X Rage" (rageRequired > 0)
            else if (rageRequired > 0 && currentRage >= rageRequired)
            {
                if (ilyaPlayer.ConsumeRageStock(rageRequired))
                {
                    rageConsumed = rageRequired;
                    boostActivated = true;
                }
            }

            // Applique le boost
            if (boostActivated)
            {
                // Multiplicateur sur les valeurs de base
                finalDamage = Mathf.RoundToInt(finalDamage * rageBoostMultiplier);
                finalHeal = Mathf.RoundToInt(finalHeal * rageBoostMultiplier);
                finalDefense = Mathf.RoundToInt(finalDefense * rageBoostMultiplier);
                finalDraw = Mathf.RoundToInt(finalDraw * rageBoostMultiplier);
                finalFetch = Mathf.RoundToInt(finalFetch * rageBoostMultiplier);
                finalAtk = Mathf.RoundToInt(finalAtk * rageBoostMultiplier);

                // Bonus fixe par Rage consommée
                if (rageBonusPerStack > 0)
                {
                    finalDamage += rageBonusPerStack * rageConsumed;
                    finalHeal += rageBonusPerStack * rageConsumed;
                    finalDefense += rageBonusPerStack * rageConsumed;
                    finalDraw += rageBonusPerStack * rageConsumed;
                    finalFetch += rageBonusPerStack * rageConsumed;
                    finalAtk += rageBonusPerStack * rageConsumed;
                }

                Debug.Log($"🔥 BOOST ! {source.name} consomme {rageConsumed} Rage → Dégâts: {finalDamage}, Soins: {finalHeal}, Défense: {finalDefense}, Attaque: {finalAtk}, Pioche: {finalDraw}, Fetch: {finalFetch}");
            }
        }

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
                if (finalDamage > 0)
                {
                    unit.TakeDamage(finalDamage);
                    Debug.Log($"  → {unit.name} prend {finalDamage} dégâts AOE");
                }
                if (finalHeal > 0)
                {
                    unit.Heal(finalHeal);
                    Debug.Log($"  → {unit.name} récupère {finalHeal} PV AOE");
                }
            }
        }
        // Sinon, applique l'effet sur la cible unique
        else
        {
            if (targetsUnit && targetUnit != null)
            {
                if (finalDamage > 0)
                {
                    targetUnit.TakeDamage(finalDamage);
                    Debug.Log($"{source.name} inflige {finalDamage} dégâts à {targetUnit.name} avec {cardName}.");
                }
                if (finalHeal > 0)
                {
                    targetUnit.Heal(finalHeal);
                    Debug.Log($"{source.name} soigne {targetUnit.name} de {finalHeal} PV avec {cardName}.");
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
            // Note: defenseAmount est appliqué aux deux défenses (P et M) pour simplifier
            statTarget.ModifyStats(finalAtk, finalDefense, finalDefense, statBoostDuration);
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