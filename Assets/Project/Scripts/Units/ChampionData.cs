using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChampionData", menuName = "Champion/Champion Data")]
public class ChampionData : ScriptableObject
{
    [Header("Identité")]
    public string championName = "Nouveau Champion";
    // Référence au GameObject du champion
    public GameObject prefab;
    // Type de famille
    public CardFamillyType famillyType = CardFamillyType.None;
    // Type d'élément
    public CardElementType elementType = CardElementType.None;

    [Header("Stats de base")]
    public int maxHealth = 100;
    public int movementRange = 3;
    public int maxPA = 5;
    public int defenseP = 10;
    public int defenseM = 10;

    [Header("Deck de Départ")]
    public List<CardData> startingDeck = new List<CardData>();

    [Header("Système d'Émotion et Transformations")]
    [Tooltip("Données des émotions de la famille (définit les noms des 3 états émotionnels)")]
    // Emotion positive
    public FamilyEmotionData familyEmotionData;
    [Tooltip("Transformation positive (Gauche) - déclenchée à +100")]
    // Emotion negative
    public TransformationData positiveTransformation;
    [Tooltip("Transformation négative (Droite) - déclenchée à -100")]
    public TransformationData negativeTransformation;
    [Tooltip("Seuil pour déclencher la transformation positive")]
    public float positiveThreshold = 100f;
    [Tooltip("Seuil pour déclencher la transformation négative")]
    public float negativeThreshold = -100f;
}
