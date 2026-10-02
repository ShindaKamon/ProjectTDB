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
    [Tooltip("Portrait affiché dans la frise des tours (généré par Tools > Portraits)")]
    public Sprite portrait;

    [Header("Classification")]
    [Tooltip("Si true, affiche la barre de vie en haut de l'écran au lieu d'au-dessus de la tête")]
    public bool isBoss = false;

    [Header("Stats de Base")]
    public int maxHealth = 50;                 // HP (Points de Vie) maximum
    public int movementRange = 2;              // PM (Points de Mouvement) maximum
    public int attackDamage = 5;               // ATK (Attaque) - dégâts de base
    public int armor = 0;                      // Armure : réduit les dégâts physiques reçus (soustraction fixe)
    [UnityEngine.Serialization.FormerlySerializedAs("barrier")]
    public int magicResistance = 0;            // Résistance magique : réduit les dégâts magiques reçus (soustraction fixe)

    [Header("Deck Pattern")]
    [Tooltip("Le deck définit le pattern de combat de l'ennemi. Les cartes sont jouées dans l'ordre (pas de mélange), une par tour, sans coût : les monstres n'ont pas de PA.")]
    public List<CardData> combatDeck = new List<CardData>();

    [Tooltip("Attaque de base, jouée à la place de la carte prévue quand un contrôle (retrait de PM) l'empêche de la jouer : règle anti-lock")]
    public CardData basicAttack;

    /// <summary>Phase suivante d'un boss : une nouvelle barre de vie pleine et un nouveau pattern.</summary>
    [System.Serializable]
    public class BossPhase
    {
        [Tooltip("PV de la phase (barre pleine au début de la phase), barème d'un joueur")]
        public int maxHealth = 100;
        [Tooltip("Pattern de la phase, joué depuis le début")]
        public List<CardData> combatDeck = new List<CardData>();
    }

    [Header("Phases (boss)")]
    [Tooltip("Phases après la première (maxHealth + combatDeck ci-dessus) : à 0 PV, le boss repart avec la barre pleine et le pattern de la phase suivante. Vide = une seule barre.")]
    public List<BossPhase> nextPhases = new List<BossPhase>();

    [Header("Visual Settings")]
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public Color healthBarColor = Color.red;
}
