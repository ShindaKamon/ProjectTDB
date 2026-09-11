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
    /// Retourne la couleur principale du deck (basée sur Emotion1)
    /// </summary>
    public Color GetPrimaryColor()
    {
        if (Emotion1 == EmotionType.None)
            return new Color(0.3f, 0.3f, 0.4f); // Gris-bleu par défaut
        return CardVisualHelper.GetEmotionColor(Emotion1);
    }

    /// <summary>
    /// Retourne la couleur secondaire du deck (basée sur Emotion2)
    /// </summary>
    public Color GetSecondaryColor()
    {
        if (Emotion2 == EmotionType.None)
            return new Color(0.25f, 0.25f, 0.35f); // Gris-bleu foncé par défaut
        return CardVisualHelper.GetEmotionColor(Emotion2);
    }

    /// <summary>
    /// Vérifie si le deck a des émotions définies
    /// </summary>
    public bool HasEmotions => Emotion1 != EmotionType.None || Emotion2 != EmotionType.None;

    private EmotionType ParseEmotion(string emotionName)
    {
        if (string.IsNullOrEmpty(emotionName))
            return EmotionType.None;

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
    /// Vérifie si une carte appartient à l'une des émotions du deck.
    /// Les cartes Signature (liées à un champion, pas à une émotion) matchent toujours.
    /// </summary>
    public bool CardMatchesDeckEmotions(CardData card)
    {
        if (card == null) return false;

        if (card.category == CardCategory.Signature)
            return true;

        // Si le deck n'a pas d'émotion définie, toutes les cartes sont acceptées
        if (Emotion1 == EmotionType.None && Emotion2 == EmotionType.None)
            return true;

        // Vérifie si la carte appartient à l'une des deux émotions
        return card.emotionType == Emotion1 || card.emotionType == Emotion2;
    }

    /// <summary>
    /// Compte les cartes du deck appartenant à une catégorie donnée (nécessite la collection
    /// pour résoudre les noms en CardData).
    /// </summary>
    public int CountByCategory(CardCategory category, CardCollection collection)
    {
        if (cardNames == null || collection == null) return 0;

        int count = 0;
        foreach (var name in cardNames)
        {
            var card = collection.GetCardByName(name);
            if (card != null && card.category == category)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Crée une copie du deck
    /// </summary>
    public DeckData Clone()
    {
        return new DeckData(deckName + " (copie)", Emotion1, Emotion2, cardNames, false);
    }
}
