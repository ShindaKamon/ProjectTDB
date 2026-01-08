using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Données pour les ennemis dans Émotions Tactics.
/// Les ennemis utilisent leur deck comme pattern de combat et piochent séquentiellement (pas de mélange).
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identité")]
    public string enemyName = "Nouvel Ennemi";
    // Référence au prefab de l'ennemi
    public GameObject prefab;
    // Type d'élément
    public CardElementType elementType = CardElementType.None;

    [Header("Classification")]
    [Tooltip("Si true, affiche la barre de vie en haut de l'écran au lieu d'au-dessus de la tête")]
    public bool isBoss = false;

    [Header("Stats de Base")]
    // Point de vie
    public int maxHealth = 50;
    // Point de mouvement
    public int movementRange = 2;
    // PA par tour pour jouer des cartes
    public int maxPA = 2;
    // Defense physique
    public int defenseP = 10;
    // defense magique
    public int defenseM = 10;

    [Header("Deck Pattern")]
    [Tooltip("Le deck définit le pattern de combat de l'ennemi. Les cartes sont jouées dans l'ordre (pas de mélange).")]
    public List<CardData> combatDeck = new List<CardData>();

    [Header("Visual Settings")]
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public Color healthBarColor = Color.red;
}
