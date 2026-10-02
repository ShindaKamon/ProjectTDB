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

    [Tooltip("Garde du corps (ex: soldat de bois) : se place entre le mob le plus proche (ni boss, ni autre garde) et le champion qui le menace ; sans mob à protéger, va au contact comme les autres")]
    public bool guardsAllies = false;

    [Tooltip("Passif « Tapi dans le noir » (boss) : au début de son tour, s'il est dans l'ombre (dans un lit ou terrain assombri) et n'a pas été touché depuis son tour précédent, il récupère ce pourcentage de sa barre en cours. 0 = pas de passif.")]
    public int darknessHealPercent = 0;

    /// <summary>Phase suivante d'un boss : une nouvelle barre de vie pleine et un nouveau pattern.</summary>
    [System.Serializable]
    public class BossPhase
    {
        [Tooltip("Titre affiché en grand au début de la phase (ex: « Le Lit »)")]
        public string title = "";
        [Tooltip("Objectif de la phase en une phrase, affiché sous le titre (ex: « Il a fusionné avec le dernier lit : frappe le Lit ! »)")]
        [TextArea] public string objective = "";
        [Tooltip("PV de la phase (barre pleine au début de la phase), barème d'un joueur")]
        public int maxHealth = 100;
        [Tooltip("Pattern de la phase, joué depuis le début")]
        public List<CardData> combatDeck = new List<CardData>();
        [Tooltip("Attaque de base de la phase (vide = celle d'EnemyData)")]
        public CardData basicAttack;
    }

    [Header("Phases (boss)")]
    [Tooltip("Titre de la première phase, affiché en grand au début du combat (vide = pas de bandeau de départ)")]
    public string firstPhaseTitle = "";
    [Tooltip("Objectif de la première phase en une phrase, sous le titre (ex: « Il se cache sous un des lits… »)")]
    [TextArea] public string firstPhaseObjective = "";
    [Tooltip("Phases après la première (maxHealth + combatDeck ci-dessus) : à 0 PV, le boss repart avec la barre pleine et le pattern de la phase suivante. Vide = une seule barre.")]
    public List<BossPhase> nextPhases = new List<BossPhase>();

    [Header("Visual Settings")]
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public Color healthBarColor = Color.red;
}
