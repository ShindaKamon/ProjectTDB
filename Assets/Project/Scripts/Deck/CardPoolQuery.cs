using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>Critère de tri principal du pool de cartes (départage toujours par nom).</summary>
public enum CardSortKey
{
    Cost,
    Name,
    Emotion
}

/// <summary>
/// Filtre et tri du pool de cartes de l'éditeur de deck (façon SpamDex), en C# pur :
/// l'UI ne fait que modifier les critères puis appeler Apply. Testable en EditMode.
/// Règles de contexte toujours appliquées : cartes Signature limitées au champion actif, cartes
/// limitées aux couleurs du deck (DeckRules.DeckColors).
/// </summary>
public class CardPoolQuery
{
    /// <summary>Émotions retenues ; vide = toutes.</summary>
    public HashSet<EmotionType> Emotions = new HashSet<EmotionType>();

    /// <summary>Catégories retenues ; vide = toutes.</summary>
    public HashSet<CardCategory> Categories = new HashSet<CardCategory>();

    /// <summary>Bornes de coût PA, incluses.</summary>
    public int MinCost = 0;
    public int MaxCost = int.MaxValue;

    /// <summary>Recherche dans le nom et la description, insensible à la casse et aux accents.</summary>
    public string Search = "";

    public CardSortKey SortKey = CardSortKey.Cost;
    public bool Descending;

    /// <summary>Vrai si au moins un filtre restreint le pool (le tri ne compte pas).</summary>
    public bool HasActiveFilters =>
        Emotions.Count > 0 || Categories.Count > 0 || MinCost > 0 || MaxCost != int.MaxValue
        || !string.IsNullOrWhiteSpace(Search);

    /// <summary>Remet tous les critères de filtre à zéro (le tri est conservé).</summary>
    public void ResetFilters()
    {
        Emotions.Clear();
        Categories.Clear();
        MinCost = 0;
        MaxCost = int.MaxValue;
        Search = "";
    }

    /// <param name="deckColors">Couleurs du deck ; null ou vide = toutes les couleurs.</param>
    public List<CardData> Apply(IEnumerable<CardData> pool, ChampionData champion, ICollection<EmotionType> deckColors = null)
    {
        var result = new List<CardData>();
        if (pool == null) return result;

        string search = Normalize(Search);

        foreach (var card in pool)
        {
            if (card == null) continue;

            // Cartes Signature : uniquement celles du champion actif (pool partagé sinon)
            if (card.category == CardCategory.Signature && card.signatureOwner != champion)
                continue;

            if (!DeckRules.MatchesColors(card, deckColors))
                continue;

            if (Emotions.Count > 0 && !Emotions.Contains(card.emotionType))
                continue;

            if (Categories.Count > 0 && !Categories.Contains(card.category))
                continue;

            if (card.costPA < MinCost || card.costPA > MaxCost)
                continue;

            if (search.Length > 0
                && !Normalize(card.cardName).Contains(search)
                && !Normalize(card.description).Contains(search))
                continue;

            result.Add(card);
        }

        result.Sort(Compare);
        return result;
    }

    private int Compare(CardData a, CardData b)
    {
        int primary = SortKey switch
        {
            CardSortKey.Name => CompareNames(a, b),
            CardSortKey.Emotion => a.emotionType.CompareTo(b.emotionType),
            _ => a.costPA.CompareTo(b.costPA),
        };

        if (Descending) primary = -primary;
        if (primary != 0) return primary;

        // Départage, toujours croissant : coût puis nom
        int cost = a.costPA.CompareTo(b.costPA);
        return cost != 0 ? cost : CompareNames(a, b);
    }

    private static int CompareNames(CardData a, CardData b) =>
        string.Compare(a.cardName, b.cardName, System.StringComparison.CurrentCultureIgnoreCase);

    /// <summary>Minuscules sans accents : "Colère" et "colere" se valent.</summary>
    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
