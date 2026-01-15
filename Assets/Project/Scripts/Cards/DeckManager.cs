using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    [Header("Configuration du Deck")]
    [SerializeField] private int _maxHandSize = 5;

    private List<CardData> _deck = new List<CardData>();
    private List<CardData> _hand = new List<CardData>();
    private List<CardData> _discardPile = new List<CardData>();

    public System.Action OnHandChanged; // Événement pour notifier les changements dans la main
    public System.Action<int> OnDeckChanged; // Notifie changement taille deck
    public System.Action<int> OnDiscardChanged; // Notifie changement taille défausse

    public void InitializeDeck(List<CardData> initialCards)
    {
        _deck.Clear();
        _hand.Clear();
        _discardPile.Clear();

        foreach (CardData card in initialCards)
        {
            _deck.Add(card);
        }

        ShuffleDeck();
        OnDeckChanged?.Invoke(_deck.Count);
        OnDiscardChanged?.Invoke(_discardPile.Count);
        Debug.Log("Deck initialisé et mélangé avec " + _deck.Count + " cartes.");
        DrawCards(_maxHandSize); // Piocher la main de départ après l'initialisation
    }

    void Start()
    {
        // Si le deck n'a pas été initialisé via InitializeDeck, on ne fait rien ici pour le moment.
        // L'initialisation se fera via GridManager.
    }

    public void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        _deck = _deck.OrderBy(a => rng.Next()).ToList();
        Debug.Log("Deck mélangé.");
    }

    public List<CardData> GetHand()
    {
        return _hand;
    }

    public int GetDeckCount()
    {
        return _deck.Count;
    }

    public int GetDiscardCount()
    {
        return _discardPile.Count;
    }

    public CardData DrawCard()
    {
        if (_deck.Count == 0)
        {
            if (_discardPile.Count > 0)
            {
                ReshuffleDiscardIntoDeck();
            }
            else
            {
                Debug.LogWarning("Impossible de piocher : deck et défausse vides.");
                return null;
            }
        }

        if (_hand.Count >= _maxHandSize)
        {
            Debug.LogWarning("Main pleine. Impossible de piocher une nouvelle carte.");
            return null; // La main est pleine, ne pioche pas
        }

        CardData drawnCard = _deck[0];
        _deck.RemoveAt(0);
        _hand.Add(drawnCard);
        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke(_deck.Count);
        Debug.Log("Carte piochée : " + drawnCard.cardName + ". Cartes restantes dans le deck : " + _deck.Count);
        return drawnCard;
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DrawCard();
        }
    }

    public void PlayCard(CardData cardToPlay)
    {
        if (_hand.Contains(cardToPlay))
        {
            _hand.Remove(cardToPlay);
            _discardPile.Add(cardToPlay);
            OnHandChanged?.Invoke();
            OnDiscardChanged?.Invoke(_discardPile.Count);
            Debug.Log("Carte jouée : " + cardToPlay.cardName + ". " + _hand.Count + " cartes restantes en main.");
        }
        else
        {
            Debug.LogWarning("La carte " + cardToPlay.cardName + " n'est pas dans la main.");
        }
    }

    private void ReshuffleDiscardIntoDeck()
    {
        Debug.Log("Défausse mélangée dans le deck.");
        _deck.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
        OnDeckChanged?.Invoke(_deck.Count);
        OnDiscardChanged?.Invoke(_discardPile.Count);
    }

    public void DiscardHand()
    {
        Debug.Log("Main défaussée.");
        _discardPile.AddRange(_hand);
        _hand.Clear();
        OnHandChanged?.Invoke();
        OnDiscardChanged?.Invoke(_discardPile.Count);
    }

    /// <summary>
    /// Ajoute une carte au deck (utilisé pour les transformations)
    /// </summary>
    public void AddCardToDeck(CardData card)
    {
        if (card != null)
        {
            _deck.Add(card);
            OnDeckChanged?.Invoke(_deck.Count);
            Debug.Log($"Carte {card.cardName} ajoutée au deck.");
        }
    }

    /// <summary>
    /// Retire une carte du deck (utilisé pour les transformations)
    /// </summary>
    public void RemoveCardFromDeck(CardData card)
    {
        if (card != null)
        {
            // Cherche dans le deck
            if (_deck.Contains(card))
            {
                _deck.Remove(card);
                OnDeckChanged?.Invoke(_deck.Count);
                Debug.Log($"Carte {card.cardName} retirée du deck.");
            }
            // Cherche dans la main
            else if (_hand.Contains(card))
            {
                _hand.Remove(card);
                OnHandChanged?.Invoke();
                Debug.Log($"Carte {card.cardName} retirée de la main.");
            }
            // Cherche dans la défausse
            else if (_discardPile.Contains(card))
            {
                _discardPile.Remove(card);
                OnDiscardChanged?.Invoke(_discardPile.Count);
                Debug.Log($"Carte {card.cardName} retirée de la défausse.");
            }
        }
    }

    // Ajoute une carte spécifique directement à la main (ex: carte Rage)
    public void AddCardToHand(CardData cardToAdd)
    {
        if (cardToAdd == null) return;

        // On autorise le dépassement de la taille de main pour les cartes ajoutées directement (Rage, Fetch, etc.)
        _hand.Add(cardToAdd);
        OnHandChanged?.Invoke();
        Debug.Log($"DeckManager: Carte spéciale {cardToAdd.cardName} ajoutée à la main.");
    }

    /// <summary>
    /// Cherche des cartes spécifiques dans le deck et les ajoute à la main
    /// </summary>
    public void FetchCards(System.Predicate<CardData> match, int count)
    {
        int foundCount = 0;
        // Parcours inversé pour pouvoir supprimer sans casser l'index
        for (int i = _deck.Count - 1; i >= 0; i--)
        {
            if (foundCount >= count) break;
            
            if (match(_deck[i]))
            {
                CardData card = _deck[i];
                _deck.RemoveAt(i);
                AddCardToHand(card);
                foundCount++;
            }
        }
        
        if (foundCount > 0)
        {
            Debug.Log($"DeckManager: {foundCount} cartes récupérées du deck.");
            OnDeckChanged?.Invoke(_deck.Count);
            ShuffleDeck();
        }
    }

    /// <summary>
    /// Défausse des cartes correspondant à un critère (ex: Cartes Rage)
    /// Retourne true si le nombre requis a été défaussé
    /// </summary>
    public bool DiscardCards(System.Predicate<CardData> match, int count)
    {
        var cardsToDiscard = _hand.Where(c => match(c)).Take(count).ToList();
        
        if (cardsToDiscard.Count < count) return false;

        foreach (var card in cardsToDiscard)
        {
            _hand.Remove(card);
            _discardPile.Add(card);
        }
        OnHandChanged?.Invoke();
        OnDiscardChanged?.Invoke(_discardPile.Count);
        return true;
    }
}
