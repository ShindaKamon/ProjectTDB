using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente un deck sauvegardé avec son nom, émotions et liste de cartes
/// </summary>
[Serializable]
public class DeckData
{
    // Structure de deck cible : 2 Signature + 16 Standard (les 6 slots Éveil sont
    // différés tant que le système de seuil d'émotion n'existe pas).
    public const int SIGNATURE_SLOTS = 2;
    public const int STANDARD_SLOTS = 16;
    public const int TOTAL_SLOTS = SIGNATURE_SLOTS + STANDARD_SLOTS;

    public string deckName;
    public string emotionType1; // Première émotion (nom du EmotionType)
    public string emotionType2; // Deuxième émotion (nom du EmotionType)
    public List<string> cardNames; // Noms des CardData
    public bool isDefault; // true = deck de base du champion (non supprimable)

    // Propriétés pour accéder aux émotions typées
    public EmotionType Emotion1
    {
        get => ParseEmotion(emotionType1);
        set => emotionType1 = value.ToString();
    }

    public EmotionType Emotion2
    {
        get => ParseEmotion(emotionType2);
        set => emotionType2 = value.ToString();
    }

    /// <summary>
    /// Vérifie si le deck a des émotions définies
    /// </summary>
    public bool HasEmotions => Emotion1 != EmotionType.None || Emotion2 != EmotionType.None;

    // Noms français des émotions avant leur passage en anglais (25/09/2026), encore présents
    // dans les sauvegardes existantes : relus tels quels, réécrits en anglais à la prochaine sauvegarde.
    private static readonly Dictionary<string, EmotionType> LegacyEmotionNames = new Dictionary<string, EmotionType>
    {
        { "Colere", EmotionType.Anger },
        { "Degout", EmotionType.Disgust },
        { "Tristesse", EmotionType.Sadness },
        { "Peur", EmotionType.Fear },
        { "Confiance", EmotionType.Trust },
        { "Joie", EmotionType.Joy },
    };

    private EmotionType ParseEmotion(string emotionName)
    {
        if (string.IsNullOrEmpty(emotionName))
            return EmotionType.None;

        if (LegacyEmotionNames.TryGetValue(emotionName, out EmotionType legacy))
            return legacy;

        if (System.Enum.TryParse(emotionName, out EmotionType result))
            return result;

        return EmotionType.None;
    }

    public DeckData()
    {
        deckName = "Nouveau Deck";
        emotionType1 = EmotionType.None.ToString();
        emotionType2 = EmotionType.None.ToString();
        cardNames = new List<string>();
        isDefault = false;
    }

    public DeckData(string name, EmotionType emotion1, EmotionType emotion2, List<string> cards, bool isDefaultDeck = false)
    {
        deckName = name;
        emotionType1 = emotion1.ToString();
        emotionType2 = emotion2.ToString();
        cardNames = new List<string>(cards);
        isDefault = isDefaultDeck;
    }

    /// <summary>
    /// Crée une copie du deck
    /// </summary>
    public DeckData Clone()
    {
        return new DeckData(deckName + " (copie)", Emotion1, Emotion2, cardNames, false);
    }
}
