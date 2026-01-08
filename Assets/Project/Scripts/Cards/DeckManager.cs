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
    }

    public void DiscardHand()
    {
        Debug.Log("Main défaussée.");
        _discardPile.AddRange(_hand);
        _hand.Clear();
        OnHandChanged?.Invoke();
    }

    /// <summary>
    /// Ajoute une carte au deck (utilisé pour les transformations)
    /// </summary>
    public void AddCardToDeck(CardData card)
    {
        if (card != null)
        {
            _deck.Add(card);
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
                Debug.Log($"Carte {card.cardName} retirée de la défausse.");
            }
        }
    }
}
