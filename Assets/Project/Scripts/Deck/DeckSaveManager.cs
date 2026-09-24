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
    private static readonly HashSet<string> _syncedChampions = new HashSet<string>();

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
            GameLog.Log($"Decks sauvegardés dans: {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde des decks: {e.Message}");
        }
    }

    /// <summary>
    /// Récupère les decks d'un champion, crée le deck de base si nécessaire
    /// Le deck de base est TOUJOURS synchronisé avec le startingDeck du ChampionData
    /// </summary>
    public static ChampionDecksData GetDecksForChampion(ChampionData champion)
    {
        var allDecks = LoadAllDecks();
        var championDecks = allDecks.GetChampionDecks(champion.championName);

        // Champion renommé : reprend ses decks enregistrés sous l'ancien nom (une seule fois)
        if (championDecks == null)
        {
            championDecks = MigrateRenamedChampion(allDecks, champion.championName);
            if (championDecks != null) SaveAllDecks();
        }

        if (championDecks == null)
        {
            // Créer les données avec le deck de base
            championDecks = CreateDefaultDecksForChampion(champion);
            allDecks.SetChampionDecks(championDecks);
            SaveAllDecks();
            _syncedChampions.Add(champion.championName);
        }
        else if (_syncedChampions.Add(champion.championName))
        {
            // Synchronise le deck de base avec le startingDeck actuel, une seule fois par
            // chargement de cache (évite de rescanner/réécrire à chaque appel de cette méthode,
            // qui est le point d'entrée de quasi toutes les opérations de ce manager)
            SyncBaseDeck(championDecks, champion);
        }

        return championDecks;
    }

    /// <summary>
    /// Synchronise le deck de base avec le startingDeck du ChampionData
    /// </summary>
    private static void SyncBaseDeck(ChampionDecksData championDecks, ChampionData champion)
    {
        // Trouver le deck de base
        DeckData baseDeck = null;
        foreach (var deck in championDecks.decks)
        {
            if (deck.isDefault)
            {
                baseDeck = deck;
                break;
            }
        }

        if (baseDeck == null) return;

        bool needsSave = false;

        // Reconstruire la liste des cartes depuis le startingDeck
        var newCardNames = new List<string>();
        foreach (var card in champion.startingDeck)
        {
            if (card != null)
                newCardNames.Add(card.cardName);
        }

        // Mettre à jour les cartes si différent
        if (!AreCardListsEqual(baseDeck.cardNames, newCardNames))
        {
            baseDeck.cardNames = newCardNames;
            needsSave = true;
            GameLog.Log($"Cartes du deck de base synchronisées pour {champion.championName}");
        }

        // Synchroniser aussi l'émotion avec celle du champion
        if (baseDeck.Emotion1 != champion.emotionType || baseDeck.Emotion2 != champion.emotionType)
        {
            baseDeck.Emotion1 = champion.emotionType;
            baseDeck.Emotion2 = champion.emotionType;
            needsSave = true;
            GameLog.Log($"Emotion du deck de base synchronisée pour {champion.championName}");
        }

        if (needsSave)
            SaveAllDecks();
    }

    /// <summary>
    /// Compare deux listes de noms de cartes
    /// </summary>
    private static bool AreCardListsEqual(List<string> list1, List<string> list2)
    {
        if (list1.Count != list2.Count) return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i]) return false;
        }

        return true;
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

        // Le deck de base utilise l'émotion du champion
        var defaultDeck = new DeckData("Deck de base", champion.emotionType, champion.emotionType, cardNames, true);
        data.decks.Add(defaultDeck);
        data.selectedDeckIndex = 0;

        return data;
    }

    /// <summary>
    /// Crée un nouveau deck custom pour un champion (deck vide)
    /// </summary>
    public static DeckData CreateDeck(ChampionData champion, string deckName, EmotionType emotion1, EmotionType emotion2)
    {
        var championDecks = GetDecksForChampion(champion);

        if (!championDecks.CanAddCustomDeck())
        {
            GameLog.LogWarning("Nombre maximum de decks custom atteint.");
            return null;
        }

        // Creer un deck VIDE (pas de copie du deck de base)
        var emptyCardList = new List<string>();
        var newDeck = new DeckData(deckName, emotion1, emotion2, emptyCardList, false);

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
            GameLog.LogWarning("Impossible de supprimer le deck de base.");
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
            GameLog.LogWarning("Nombre maximum de decks custom atteint.");
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
    /// Change les émotions d'un deck
    /// </summary>
    public static void SetDeckEmotions(ChampionData champion, int deckIndex, EmotionType emotion1, EmotionType emotion2)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return;

        championDecks.decks[deckIndex].Emotion1 = emotion1;
        championDecks.decks[deckIndex].Emotion2 = emotion2;
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
    /// <summary>
    /// Anciens noms de champions renommés -> nom actuel. Les decks sauvegardés sont rangés par nom
    /// de champion : sans cette table, un champion renommé perdrait tous ses decks personnalisés.
    /// À compléter à chaque renommage (ne jamais retirer une entrée).
    /// </summary>
    private static readonly Dictionary<string, string> RenamedChampions = new Dictionary<string, string>
    {
        { "Soren", "Evan" },
        { "L'Alpiniste", "Crux" },
        { "Ace", "Raze" },
    };

    /// <summary>
    /// Si aucun deck n'existe sous le nom actuel du champion, récupère ceux enregistrés sous un
    /// ancien nom (table RenamedChampions) et les renomme. Ne sauvegarde pas : à l'appelant de le faire.
    /// </summary>
    public static ChampionDecksData MigrateRenamedChampion(AllDecksData allDecks, string currentName)
    {
        if (allDecks == null || string.IsNullOrEmpty(currentName)) return null;

        foreach (var rename in RenamedChampions)
        {
            if (rename.Value != currentName) continue;

            var oldDecks = allDecks.GetChampionDecks(rename.Key);
            if (oldDecks == null) continue;

            oldDecks.championName = currentName;
            GameLog.Log($"Decks de « {rename.Key} » repris sous le nouveau nom « {currentName} ».");
            return oldDecks;
        }
        return null;
    }

    /// <summary>
    /// Anciens noms de cartes renommées -> nom actuel. Les decks sauvegardés référencent les cartes
    /// par nom : sans cette table, une carte renommée disparaîtrait des decks existants.
    /// À compléter à chaque renommage de carte (ne jamais retirer une entrée).
    /// </summary>
    private static readonly Dictionary<string, string> RenamedCards = new Dictionary<string, string>
    {
        { "Il triche", "Triche" },
        { "Corde de rappel forcé", "Corde de rappel" },
        { "Écho de Lyse", "Écho évanescent" },
        { "Écho evanescent", "Écho évanescent" }, // orthographe provisoire du 24/09/2026
    };

    /// <summary>Nom actuel d'une carte (suit les renommages successifs).</summary>
    public static string ResolveCardName(string name)
    {
        int guard = 0;
        while (name != null && RenamedCards.TryGetValue(name, out string newName) && guard++ < 10)
        {
            name = newName;
        }
        return name;
    }

    public static List<CardData> GetCardsFromNames(List<string> cardNames, CardCollection collection)
    {
        var cards = new List<CardData>();
        foreach (var name in cardNames)
        {
            var card = collection.GetCardByName(ResolveCardName(name));
            if (card != null)
                cards.Add(card);
            else
                GameLog.LogWarning($"Carte non trouvée dans la collection: '{name}'");
        }
        return cards;
    }

    /// <summary>
    /// Récupère les cartes d'un deck, utilise directement le startingDeck pour le deck de base
    /// </summary>
    public static List<CardData> GetDeckCards(ChampionData champion, int deckIndex, CardCollection collection)
    {
        var championDecks = GetDecksForChampion(champion);

        if (deckIndex < 0 || deckIndex >= championDecks.decks.Count)
            return new List<CardData>();

        var deck = championDecks.decks[deckIndex];

        // Pour le deck de base, utiliser directement le startingDeck du champion
        if (deck.isDefault && champion.startingDeck != null)
        {
            return new List<CardData>(champion.startingDeck);
        }

        // Pour les decks custom, utiliser la recherche par nom
        var cards = GetCardsFromNames(deck.cardNames, collection);

        // Mise en conformité avec les règles de construction (DeckRules), sauvegardée si besoin :
        // cartes hors des couleurs du deck et exemplaires en trop retirés, Signatures ajoutées.
        int removed = DeckRules.EnforceColors(cards, DeckRules.DeckColors(deck, cards));
        removed += DeckRules.EnforceCopyLimits(cards);
        int added = 0;
        if (collection != null)
        {
            var missing = DeckRules.MissingSignatures(cards, champion, collection.AllCards);
            cards.AddRange(missing);
            added = missing.Count;
        }

        if (removed > 0 || added > 0)
        {
            deck.cardNames = cards.ConvertAll(c => c.cardName);
            SaveAllDecks();
            GameLog.Log($"Deck « {deck.deckName} » mis en conformité : {removed} carte(s) hors couleurs ou en trop retirée(s), {added} Signature(s) ajoutée(s).");
        }
        return cards;
    }

    /// <summary>
    /// Supprime toutes les données sauvegardées et force la régénération
    /// </summary>
    public static void DeleteAllSavedDecks()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            GameLog.Log($"Fichier de sauvegarde supprimé: {SaveFilePath}");
        }
        InvalidateCache();
    }

    /// <summary>
    /// Réinitialise les decks d'un champion spécifique
    /// </summary>
    public static void ResetChampionDecks(ChampionData champion)
    {
        var allDecks = LoadAllDecks();

        // Supprimer les données existantes pour ce champion
        for (int i = allDecks.champions.Count - 1; i >= 0; i--)
        {
            if (allDecks.champions[i].championName == champion.championName)
            {
                allDecks.champions.RemoveAt(i);
            }
        }

        // Recréer les données par défaut
        var newData = CreateDefaultDecksForChampion(champion);
        allDecks.SetChampionDecks(newData);
        SaveAllDecks();

        GameLog.Log($"Decks réinitialisés pour {champion.championName}");
    }

    /// <summary>
    /// Force le rechargement des données au prochain accès
    /// </summary>
    public static void InvalidateCache()
    {
        _isLoaded = false;
        _cachedData = null;
        _syncedChampions.Clear();
    }
}
