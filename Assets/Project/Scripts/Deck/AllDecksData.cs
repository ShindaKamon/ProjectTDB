using System;
using System.Collections.Generic;

/// <summary>
/// Conteneur principal pour toutes les données de decks de tous les champions
/// Utilisé pour la sérialisation JSON
/// </summary>
[Serializable]
public class AllDecksData
{
    public List<ChampionDecksData> champions;

    public AllDecksData()
    {
        champions = new List<ChampionDecksData>();
    }

    /// <summary>
    /// Récupère les données de decks d'un champion par son nom
    /// </summary>
    public ChampionDecksData GetChampionDecks(string championName)
    {
        foreach (var champion in champions)
        {
            if (champion.championName == championName)
                return champion;
        }
        return null;
    }

    /// <summary>
    /// Ajoute ou met à jour les données de decks d'un champion
    /// </summary>
    public void SetChampionDecks(ChampionDecksData data)
    {
        for (int i = 0; i < champions.Count; i++)
        {
            if (champions[i].championName == data.championName)
            {
                champions[i] = data;
                return;
            }
        }
        champions.Add(data);
    }
}
