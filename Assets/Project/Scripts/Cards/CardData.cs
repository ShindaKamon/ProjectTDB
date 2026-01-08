using UnityEngine;
using System.Collections.Generic;

// Enum pour le coût de la carte
public enum CardCostType { None, PA }

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

public enum CardFamillyType
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
    public CardFamillyType famillyType = CardFamillyType.None;
    // Type de classe
    public CardClasseType classeType = CardClasseType.None;
    // Type d'élément
    public CardElementType elementType = CardElementType.None;
    // Illustration de la carte
    public Sprite artwork; 

    [Header("Coût et ressources")]
    // Coût en PA (Points d'Action)
    public int costPA = 0; 

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

    [Header("Modificateur d'Émotion")]
    [Tooltip("Modifie la jauge émotionnelle du lanceur (positif = vers Contrariété/Tank, négatif = vers Rage/DPS)")]
    [Range(-50f, 50f)]
    public float emotionModifier = 0f; 

   

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
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in allUnits)
        {
            if (unit.GetCurrentGridPos() == GridManager.Instance.GetGridPosFromWorldPos(tile.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    // Méthode pour obtenir toutes les unités affectées par l'AOE
    public List<Unit> GetAOEAffectedUnits(Unit source, Vector2 epicenter)
    {
        List<Unit> affectedUnits = new List<Unit>();

        if (!isAOE || aoeRadius <= 0)
        {
            return affectedUnits;
        }

        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);

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
                if (damageAmount > 0)
                {
                    unit.TakeDamage(damageAmount);
                    Debug.Log($"  → {unit.name} prend {damageAmount} dégâts AOE");
                }
                if (healAmount > 0)
                {
                    unit.Heal(healAmount);
                    Debug.Log($"  → {unit.name} récupère {healAmount} PV AOE");
                }
            }
        }
        // Sinon, applique l'effet sur la cible unique
        else
        {
            if (targetsUnit && targetUnit != null)
            {
                if (damageAmount > 0)
                {
                    targetUnit.TakeDamage(damageAmount);
                    Debug.Log($"{source.name} inflige {damageAmount} dégâts à {targetUnit.name} avec {cardName}.");
                }
                if (healAmount > 0)
                {
                    targetUnit.Heal(healAmount);
                    Debug.Log($"{source.name} soigne {targetUnit.name} de {healAmount} PV avec {cardName}.");
                }
            }
            else if (targetType == CardTargetType.Self && healAmount > 0)
            {
                source.Heal(healAmount);
                Debug.Log($"{source.name} se soigne de {healAmount} PV avec {cardName}.");
            }
        }

        if (movementAmount > 0)
        {
            // La logique de mouvement sera gérée par l'InputManager ou une autre entité
            // pour l'instant, nous pouvons juste loguer l'intention.
            Debug.Log($"{source.name} gagne {movementAmount} points de mouvement supplémentaires avec {cardName}.");
        }

        // Appliquer le modificateur d'émotion si l'unité a un système d'émotion
        if (emotionModifier != 0f)
        {
            // Utiliser SendMessage pour éviter la dépendance directe
            source.SendMessage("ModifyEmotion", emotionModifier, SendMessageOptions.DontRequireReceiver);
            string direction = emotionModifier > 0 ? "Contrariété/Tank" : "Rage/DPS";
            Debug.Log($"{source.name} modifie son émotion vers {direction} de {emotionModifier:+F1;-F1;0} avec {cardName}.");
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
    public static Color GetFamilyColor(CardFamillyType family)
    {
        switch (family)
        {
            case CardFamillyType.Dechaines:
                return new Color(0.8f, 0f, 0f); // Rouge
            case CardFamillyType.Dissidents:
                return new Color(0f, 0.4f, 0f); // Vert foncé
            case CardFamillyType.Insurgents:
                return new Color(1f, 0.92f, 0f); // Jaune
            case CardFamillyType.Exiles:
                return new Color(0f, 0f, 0.5f); // Bleu foncé
            case CardFamillyType.Reprouves:
                return new Color(0.5f, 0f, 0.5f); // Violet
            case CardFamillyType.Gardiens:
                return new Color(0.5f, 1f, 0.5f); // Vert clair
            case CardFamillyType.Eveilles:
                return new Color(0.5f, 0.8f, 1f); // Bleu clair
            case CardFamillyType.Precurseurs:
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
    public static string GetFamilyName(CardFamillyType family)
    {
        switch (family)
        {
            case CardFamillyType.Dechaines: return "Déchaînés";
            case CardFamillyType.Dissidents: return "Dissidents";
            case CardFamillyType.Insurgents: return "Insurgents";
            case CardFamillyType.Exiles: return "Exilés";
            case CardFamillyType.Reprouves: return "Réprouvés";
            case CardFamillyType.Gardiens: return "Gardiens";
            case CardFamillyType.Eveilles: return "Éveillés";
            case CardFamillyType.Precurseurs: return "Précurseurs";
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