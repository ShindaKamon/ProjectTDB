using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Gestionnaire de sauvegarde/chargement des decks
/// Singleton statique pour accès global
/// </summary>
public static class DeckSaveManager
{
    private const string SAVE_FILE_NAME = "decks.json";
    private static AllDecksData _cachedData;
    private static bool _isLoaded = false;

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    /// <summary>
    /// Charge toutes les données de decks depuis le fichier
    /// </summary>
    public static AllDecksData LoadAllDecks()
    {
        if (_isLoaded && _cachedData != null)
            return _cachedData;

        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                _cachedData = JsonUtility.FromJson<AllDecksData>(json);
                if (_cachedData == null)
                    _cachedData = new AllDecksData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors du chargement des decks: {e.Message}");
                _cachedData = new AllDecksData();
            }
        }
        else
        {
            _cachedData = new AllDecksData();
        }

        _isLoaded = true;
        return _cachedData;
    }

    /// <summary>
    /// Sauvegarde toutes les données de decks dans le fichier
    /// </summary>
    public static void SaveAllDecks()
    {
        if (_cachedData == null)
            _cachedData = new AllDecksData();

        try
        {
            string json = JsonUtility.ToJson(_cachedData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"Decks sauvegardés dans: {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde des decks: {e.Message}");
        }
    }

    /// <summary>
    /// Récupère les decks d'un champion, crée le deck de base si nécessaire
    /// </summary>
    public static ChampionDecksData GetDecksForChampion(ChampionData champion)
    {
        var allDecks = LoadAllDecks();
        var championDecks = allDecks.GetChampionDecks(champion.championName);

        if (championDecks == null)
        {
            // Créer les données avec le deck de base
            championDecks = CreateDefaultDecksForChampion(champion);
            allDecks.SetChampionDecks(championDecks);
            SaveAllDecks();
        }

        return championDecks;
    }

    /// <summary>
    /// Crée les données de decks par défaut pour un champion
    /// </summary>
    private static ChampionDecksData CreateDefaultDecksForChampion(ChampionData champion)
    {
        var data = new ChampionDecksData(champion.championName);

        // Créer le deck de base à partir du startingDeck du champion
        var cardNames = new List<string>();
        foreach (var card in champion.startingDeck)
        {
            if (card != null)
                cardNames.Add(card.cardName);
        }

        var defaultDeck = new DeckData("Deck de base", "#E74C3C", cardNames, true);
        data.decks.Add(defaultDeck);
        data.selectedDeckIndex = 0;

        return data;
    }

    /// <summary>
    /// Crée un nouveau deck custom pour un champion
    /// </summary>
    public static DeckData CreateDeck(ChampionData champion, string deckName, string color)
    {
        var championDecks = GetDecksForChampion(champion);

        if (!championDecks.CanAddCustomDeck())
        {
            Debug.LogWarning("Nombre maximum de decks custom atteint.");
            return null;
        }

        // Copier les cartes du deck de base
        var baseDeck = championDecks.decks[0];
        var newDeck = new DeckData(deckName, color, baseDeck.cardNames, false);

        championDecks.decks.Add(newDeck);
        SaveAllDecks();

        return newDeck;
    }

    /// <summary>
    /// Supprime un deck (impossible pour le deck de base)
    /// </summary>
    public static bool DeleteDeck(ChampionData champion, int deckIndex)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return false;

        if (championDecks.decks[deckIndex].isDefault)
        {
            Debug.LogWarning("Impossible de supprimer le deck de base.");
            return false;
        }

        championDecks.decks.RemoveAt(deckIndex);

        // Ajuster l'index sélectionné si nécessaire
        if (championDecks.selectedDeckIndex >= championDecks.decks.Count)
            championDecks.selectedDeckIndex = 0;

        SaveAllDecks();
        return true;
    }

    /// <summary>
    /// Duplique un deck existant
    /// </summary>
    public static DeckData DuplicateDeck(ChampionData champion, int deckIndex)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return null;

        if (!championDecks.CanAddCustomDeck())
        {
            Debug.LogWarning("Nombre maximum de decks custom atteint.");
            return null;
        }

        var originalDeck = championDecks.decks[deckIndex];
        var newDeck = originalDeck.Clone();

        championDecks.decks.Add(newDeck);
        SaveAllDecks();

        return newDeck;
    }

    /// <summary>
    /// Renomme un deck
    /// </summary>
    public static void RenameDeck(ChampionData champion, int deckIndex, string newName)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return;

        championDecks.decks[deckIndex].deckName = newName;
        SaveAllDecks();
    }

    /// <summary>
    /// Change la couleur d'un deck
    /// </summary>
    public static void SetDeckColor(ChampionData champion, int deckIndex, string hexColor)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return;

        championDecks.decks[deckIndex].deckColor = hexColor;
        SaveAllDecks();
    }

    /// <summary>
    /// Sélectionne un deck pour un champion
    /// </summary>
    public static void SelectDeck(ChampionData champion, int deckIndex)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return;

        championDecks.selectedDeckIndex = deckIndex;
        SaveAllDecks();
    }

    /// <summary>
    /// Met à jour les cartes d'un deck
    /// </summary>
    public static void UpdateDeckCards(ChampionData champion, int deckIndex, List<string> cardNames)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return;

        championDecks.decks[deckIndex].cardNames = new List<string>(cardNames);
        SaveAllDecks();
    }

    /// <summary>
    /// Convertit une liste de noms de cartes en CardData
    /// </summary>
    public static List<CardData> GetCardsFromNames(List<string> cardNames, CardCollection collection)
    {
        var cards = new List<CardData>();
        foreach (var name in cardNames)
        {
            var card = collection.GetCardByName(name);
            if (card != null)
                cards.Add(card);
        }
        return cards;
    }

    /// <summary>
    /// Force le rechargement des données au prochain accès
    /// </summary>
    public static void InvalidateCache()
    {
        _isLoaded = false;
        _cachedData = null;
    }
}
