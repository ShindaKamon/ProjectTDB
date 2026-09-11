using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChampionData", menuName = "Champion/Champion Data")]
public class ChampionData : ScriptableObject
{
    [Header("Identite")]
    public string championName = "Nouveau Champion";
    public string title = "";                  // Titre/sous-titre du champion
    [TextArea(2, 4)]
    public string description = "";            // Description du champion
    public Sprite portrait;                    // Portrait pour l'UI de selection

    [Space(5)]
    public GameObject prefab;                  // Reference au prefab du champion
    public EmotionType emotionType = EmotionType.None;    // Emotion

    [Header("Stats de base")]
    public int maxHealth = 100;                // HP (Points de Vie) maximum
    public int movementRange = 3;              // PM (Points de Mouvement) maximum
    public int maxActionPoints = 5;            // PA (Points d'Action) maximum
    public int attackDamage = 10;              // ATK (Attaque) - degats de base
    public int defense = 10;                   // Defense (reduit les degats recus)

    [Header("Deck de Depart")]
    public List<CardData> startingDeck = new List<CardData>();
}
