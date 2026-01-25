using System;
using System.Collections.Generic;

/// <summary>
/// Contient tous les decks d'un champion spécifique
/// </summary>
[Serializable]
public class ChampionDecksData
{
    public string championName;
    public List<DeckData> decks; // Max 4 (1 base + 3 custom)
    public int selectedDeckIndex;

    public const int MAX_CUSTOM_DECKS = 3;
    public const int MAX_TOTAL_DECKS = 4; // 1 base + 3 custom

    public ChampionDecksData()
    {
        championName = "";
        decks = new List<DeckData>();
        selectedDeckIndex = 0;
    }

    public ChampionDecksData(string name)
    {
        championName = name;
        decks = new List<DeckData>();
        selectedDeckIndex = 0;
    }

    /// <summary>
    /// Retourne le deck actuellement sélectionné
    /// </summary>
    public DeckData GetSelectedDeck()
    {
        if (decks.Count == 0) return null;
        if (selectedDeckIndex < 0 || selectedDeckIndex >= decks.Count)
            selectedDeckIndex = 0;
        return decks[selectedDeckIndex];
    }

    /// <summary>
    /// Vérifie si on peut ajouter un nouveau deck custom
    /// </summary>
    public bool CanAddCustomDeck()
    {
        int customCount = 0;
        foreach (var deck in decks)
        {
            if (!deck.isDefault) customCount++;
        }
        return customCount < MAX_CUSTOM_DECKS;
    }

    /// <summary>
    /// Compte le nombre de decks custom
    /// </summary>
    public int GetCustomDeckCount()
    {
        int count = 0;
        foreach (var deck in decks)
        {
            if (!deck.isDefault) count++;
        }
        return count;
    }
}
