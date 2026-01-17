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
    public CardFamilyType familyType = CardFamilyType.None;
    // Type d'élément
    public CardElementType elementType = CardElementType.None;

    [Header("Stats de base")]
    public int maxHealth = 100;                // HP (Points de Vie) maximum
    public int movementRange = 3;              // PM (Points de Mouvement) maximum
    public int maxActionPoints = 5;            // PA (Points d'Action) maximum
    public int attackDamage = 10;              // ATK (Attaque) - dégâts de base
    public int defense = 10;                   // Défense (réduit les dégâts reçus)

    [Header("Deck de Départ")]
    public List<CardData> startingDeck = new List<CardData>();
}
