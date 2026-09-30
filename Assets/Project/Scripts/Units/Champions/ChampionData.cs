using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChampionData", menuName = "Champion/Champion Data")]
public class ChampionData : ScriptableObject
{
    [Header("Identite")]
    public string championName = "Nouveau Champion";
    public string title = "";                  // Titre/sous-titre du champion
    [TextArea(3, 8)]
    public string description = "";            // Histoire du champion (plus affichée : la fiche montre le passif)
    [Tooltip("Nom du passif (ex: Réflexe du grimpeur)")]
    public string passiveName = "";
    [TextArea(2, 4)]
    [Tooltip("Résumé du passif, affiché sur la fiche de sélection et sous les stats en combat")]
    public string passiveDescription = "";
    public Sprite portrait;                    // Portrait pour l'UI de selection
    [Tooltip("Illustration du champion en pied, plein cadre (1000x1600, pieds ancres a 90% de la hauteur). " +
             "Utilisee en fond plein ecran dans l'ecran de selection de champion. Distincte de 'portrait'.")]
    public Sprite fullBodyArt;                 // Illustration plein ecran pour la selection de champion

    [Space(5)]
    public GameObject prefab;                  // Reference au prefab du champion
    public EmotionType emotionType = EmotionType.None;    // Emotion

    [Header("Stats de base")]
    public int maxHealth = 100;                // HP (Points de Vie) maximum
    public int movementRange = 3;              // PM (Points de Mouvement) maximum
    public int maxActionPoints = 5;            // PA (Points d'Action) maximum
    public int attackDamage = 10;              // ATK (Attaque) - degats de base
    public int armor = 0;                      // Armure : réduit les dégâts physiques reçus (soustraction fixe)
    [UnityEngine.Serialization.FormerlySerializedAs("barrier")]
    public int magicResistance = 0;            // Résistance magique : réduit les dégâts magiques reçus (soustraction fixe)

    [Header("Gain par niveau (arrondi à l'entier inférieur)")]
    [Tooltip("ATQ gagnée par niveau au-dessus du 1 (ex: 0,5 = +1 tous les 2 niveaux)")]
    public float attackPerLevel = 0f;
    [Tooltip("Armure gagnée par niveau au-dessus du 1")]
    public float armorPerLevel = 0f;
    [Tooltip("Résistance magique gagnée par niveau au-dessus du 1")]
    public float magicResistancePerLevel = 0f;

    public int AttackAtLevel(int level) => attackDamage + Gain(attackPerLevel, level);
    public int ArmorAtLevel(int level) => armor + Gain(armorPerLevel, level);
    public int MagicResistanceAtLevel(int level) => magicResistance + Gain(magicResistancePerLevel, level);

    private static int Gain(float perLevel, int level) => Mathf.FloorToInt(perLevel * Mathf.Max(0, level - 1));

    [Header("Fusion (Éveil)")]
    [Tooltip("Formes de fusion du champion : au plus une par émotion (Colère = Rage, Joie = Extase, Peur = Terreur)")]
    public List<FusionData> fusions = new List<FusionData>();

    [Header("Deck de Depart")]
    public List<CardData> startingDeck = new List<CardData>();
}
