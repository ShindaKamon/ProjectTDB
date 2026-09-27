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

    // Surcouche de coût par carte (ex: "Triche" d'Ace, ±1 PA). CardData est un
    // ScriptableObject partagé : si la main contient 2 exemplaires de la même carte, les deux
    // partagent le même override (limitation connue, acceptable tant qu'aucune UI de ciblage
    // "carte de la main" n'existe pour choisir un exemplaire précis).
    private readonly Dictionary<CardData, int> _costOverrides = new Dictionary<CardData, int>();

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
        GameLog.Log("Deck initialisé et mélangé avec " + _deck.Count + " cartes.");
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
        GameLog.Log("Deck mélangé.");
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
                GameLog.LogWarning("Impossible de piocher : deck et défausse vides.");
                return null;
            }
        }

        if (_hand.Count >= _maxHandSize)
        {
            GameLog.LogWarning("Main pleine. Impossible de piocher une nouvelle carte.");
            return null; // La main est pleine, ne pioche pas
        }

        CardData drawnCard = _deck[0];
        _deck.RemoveAt(0);
        _hand.Add(drawnCard);
        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke(_deck.Count);
        GameLog.Log("Carte piochée : " + drawnCard.cardName + ". Cartes restantes dans le deck : " + _deck.Count);
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
            // Un override de coût (ex: Triche) ne vaut que tant que la carte reste en main ;
            // on le retire à la défausse (voir doc de ClearCostOverride) pour éviter qu'il ne
            // persiste indéfiniment si la carte est rebattue et repiochée plus tard.
            ClearCostOverride(cardToPlay);
            OnHandChanged?.Invoke();
            OnDiscardChanged?.Invoke(_discardPile.Count);
            GameLog.Log("Carte jouée : " + cardToPlay.cardName + ". " + _hand.Count + " cartes restantes en main.");
        }
        else
        {
            GameLog.LogWarning("La carte " + cardToPlay.cardName + " n'est pas dans la main.");
        }
    }

    /// <summary>
    /// Coût effectif d'une carte, après application d'un éventuel override (ex: Triche).
    /// Le plancher de 1 PA ne s'applique que si un override est actif : une carte à coût de
    /// base 0 PA sans override reste gratuite (bug corrigé : le plancher
    /// s'appliquait auparavant même sans override, rendant les cartes à 0 PA injouables dès
    /// qu'un DeckManager était présent).
    /// </summary>
    public int GetEffectiveCost(CardData card)
    {
        if (card == null) return 0;
        int delta = _costOverrides.TryGetValue(card, out int d) ? d : 0;
        return delta != 0 ? Mathf.Max(1, card.costPA + delta) : card.costPA;
    }

    /// <summary>
    /// Modifie le coût d'une carte de ±delta PA (ex: Triche modifie de ±1).
    /// </summary>
    public void ModifyCardCost(CardData card, int delta)
    {
        if (card == null || delta == 0) return;

        int current = _costOverrides.TryGetValue(card, out int d) ? d : 0;
        _costOverrides[card] = current + delta;
        GameLog.Log($"DeckManager: coût de {card.cardName} modifié ({(delta > 0 ? "+" : "")}{delta}) -> coût effectif {GetEffectiveCost(card)}.");
    }

    /// <summary>
    /// Retire tout override de coût sur une carte (ex: à la défausse/fin de partie).
    /// </summary>
    public void ClearCostOverride(CardData card)
    {
        if (card != null) _costOverrides.Remove(card);
    }

    private void ReshuffleDiscardIntoDeck()
    {
        GameLog.Log("Défausse mélangée dans le deck.");
        _deck.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
        OnDeckChanged?.Invoke(_deck.Count);
        OnDiscardChanged?.Invoke(_discardPile.Count);
    }

    public void DiscardHand()
    {
        GameLog.Log("Main défaussée.");
        // Retire tout override de coût sur les cartes défaussées (voir doc de ClearCostOverride).
        foreach (CardData card in _hand)
        {
            ClearCostOverride(card);
        }
        _discardPile.AddRange(_hand);
        _hand.Clear();
        OnHandChanged?.Invoke();
        OnDiscardChanged?.Invoke(_discardPile.Count);
    }

}
