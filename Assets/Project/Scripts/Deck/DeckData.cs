using System;
using System.Collections.Generic;

/// <summary>
/// Représente un deck sauvegardé avec son nom, couleur et liste de cartes
/// </summary>
[Serializable]
public class DeckData
{
    public string deckName;
    public string deckColor; // Hex color pour le visuel (ex: "#E74C3C")
    public List<string> cardNames; // Noms des CardData
    public bool isDefault; // true = deck de base du champion (non supprimable)

    public DeckData()
    {
        deckName = "Nouveau Deck";
        deckColor = "#3498DB";
        cardNames = new List<string>();
        isDefault = false;
    }

    public DeckData(string name, string color, List<string> cards, bool isDefaultDeck = false)
    {
        deckName = name;
        deckColor = color;
        cardNames = new List<string>(cards);
        isDefault = isDefaultDeck;
    }

    /// <summary>
    /// Crée une copie du deck
    /// </summary>
    public DeckData Clone()
    {
        return new DeckData(deckName + " (copie)", deckColor, cardNames, false);
    }
}
