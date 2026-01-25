using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject contenant toutes les cartes disponibles dans le jeu
/// </summary>
[CreateAssetMenu(fileName = "CardCollection", menuName = "Cards/Card Collection")]
public class CardCollection : ScriptableObject
{
    [Header("Toutes les cartes du jeu")]
    [SerializeField] private List<CardData> _allCards = new List<CardData>();

    /// <summary>
    /// Retourne la liste de toutes les cartes
    /// </summary>
    public List<CardData> AllCards => _allCards;

    /// <summary>
    /// Récupère une carte par son nom
    /// </summary>
    public CardData GetCardByName(string cardName)
    {
        foreach (var card in _allCards)
        {
            if (card != null && card.cardName == cardName)
                return card;
        }
        return null;
    }

    /// <summary>
    /// Récupère toutes les cartes d'une famille spécifique
    /// </summary>
    public List<CardData> GetCardsByFamily(CardFamilyType family)
    {
        var result = new List<CardData>();
        foreach (var card in _allCards)
        {
            if (card != null && card.familyType == family)
                result.Add(card);
        }
        return result;
    }

    /// <summary>
    /// Récupère toutes les cartes d'une classe spécifique
    /// </summary>
    public List<CardData> GetCardsByClasse(CardClasseType classe)
    {
        var result = new List<CardData>();
        foreach (var card in _allCards)
        {
            if (card != null && card.classeType == classe)
                result.Add(card);
        }
        return result;
    }

    /// <summary>
    /// Récupère toutes les cartes d'un élément spécifique
    /// </summary>
    public List<CardData> GetCardsByElement(CardElementType element)
    {
        var result = new List<CardData>();
        foreach (var card in _allCards)
        {
            if (card != null && card.elementType == element)
                result.Add(card);
        }
        return result;
    }

    /// <summary>
    /// Vérifie si une carte existe dans la collection
    /// </summary>
    public bool HasCard(string cardName)
    {
        return GetCardByName(cardName) != null;
    }

    /// <summary>
    /// Retourne le nombre total de cartes
    /// </summary>
    public int Count => _allCards.Count;

#if UNITY_EDITOR
    /// <summary>
    /// Collecte automatiquement toutes les cartes du projet (éditeur uniquement)
    /// </summary>
    [ContextMenu("Collecter toutes les cartes")]
    private void CollectAllCards()
    {
        _allCards.Clear();

        // Trouver tous les CardData dans le projet
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            CardData card = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (card != null)
            {
                // Exclure les cartes "Enemy" du pool joueur
                if (!path.Contains("/Enemy/"))
                {
                    _allCards.Add(card);
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"CardCollection: {_allCards.Count} cartes collectées.");
    }
#endif
}
