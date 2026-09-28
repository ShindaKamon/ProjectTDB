using System.Collections.Generic;

/// <summary>
/// Règles de construction d'un deck (décisions du 24/09/2026), centralisées ici plutôt que dans l'UI :
/// - un champion peut jouer toutes les émotions, mais chaque deck a ses couleurs (1 ou 2, choisies à
///   sa création) et ne contient que des cartes de ces couleurs, plus les Signatures du champion ;
/// - les Signatures d'un autre champion sont interdites ; celles du champion sont obligatoires ;
/// - 4 exemplaires maximum d'une même carte, 2 pour une Signature (décision du 28/09/2026) ;
/// - emplacements par catégorie : DeckData.SIGNATURE_SLOTS et DeckData.STANDARD_SLOTS.
/// C# pur : testable en EditMode.
/// </summary>
public static class DeckRules
{
    public const int MAX_COPIES = 4;
    public const int MAX_SIGNATURE_COPIES = 2;

    /// <summary>
    /// Émotions qu'on peut choisir comme couleurs d'un deck : celles du lancement (MVP). Les 5 autres
    /// de la roue de Plutchik viendront plus tard : il suffira de les ajouter ici.
    /// </summary>
    public static readonly IReadOnlyList<EmotionType> AvailableEmotions = new[]
    {
        EmotionType.Anger, EmotionType.Fear, EmotionType.Joy
    };

    /// <summary>
    /// Couleurs d'un deck : celles choisies à sa création ; à défaut (deck de base, qui n'en a pas),
    /// les émotions présentes dans ses cartes. Liste vide = aucune restriction de couleur.
    /// </summary>
    public static List<EmotionType> DeckColors(DeckData deck, IEnumerable<CardData> cards)
    {
        var colors = new List<EmotionType>();
        void Add(EmotionType e) { if (e != EmotionType.None && !colors.Contains(e)) colors.Add(e); }

        if (deck != null && deck.HasEmotions)
        {
            Add(deck.Emotion1);
            Add(deck.Emotion2);
        }
        else if (cards != null)
        {
            foreach (var card in cards)
                if (card != null && card.category != CardCategory.Signature) Add(card.emotionType);
        }

        colors.Sort();
        return colors;
    }

    /// <summary>La carte respecte-t-elle les couleurs du deck ? (les Signatures n'ont pas de couleur)</summary>
    public static bool MatchesColors(CardData card, ICollection<EmotionType> deckColors) =>
        card != null && (card.category == CardCategory.Signature || deckColors == null || deckColors.Count == 0
                         || deckColors.Contains(card.emotionType));

    /// <summary>Un deck n'est jouable que complet : au moins DeckData.TOTAL_SLOTS cartes.</summary>
    public static bool IsComplete(int cardCount) => cardCount >= DeckData.TOTAL_SLOTS;

    /// <summary>
    /// Cartes qui ne sont pas (ou plus, après un changement de couleurs) des couleurs du deck. Elles
    /// restent dans le deck, signalées, jusqu'à ce que le joueur les retire (décision du 28/09/2026).
    /// </summary>
    public static int CountOffColor(IEnumerable<CardData> cards, ICollection<EmotionType> deckColors)
    {
        int count = 0;
        if (cards == null) return count;
        foreach (var card in cards)
            if (card != null && !MatchesColors(card, deckColors)) count++;
        return count;
    }

    /// <summary>Jouable = complet et sans carte hors des couleurs du deck.</summary>
    public static bool IsPlayable(IList<CardData> cards, ICollection<EmotionType> deckColors) =>
        cards != null && IsComplete(cards.Count) && CountOffColor(cards, deckColors) == 0;

    public static int MaxCopies(CardData card) =>
        card != null && card.category == CardCategory.Signature ? MAX_SIGNATURE_COPIES : MAX_COPIES;

    /// <summary>Signature propre à ce champion ?</summary>
    public static bool IsOwnSignature(CardData card, ChampionData champion) =>
        card != null && card.category == CardCategory.Signature && card.signatureOwner == champion;

    /// <param name="deckColors">Couleurs du deck (DeckColors) ; null ou vide = pas de restriction.</param>
    public static ValidationResult CanAddCard(IList<CardData> deck, CardData card, ChampionData champion,
        ICollection<EmotionType> deckColors = null)
    {
        if (card == null)
            return ValidationResult.Fail("Carte manquante");

        if (card.category == CardCategory.Signature && card.signatureOwner != champion)
            return ValidationResult.Fail($"{card.cardName} est une Signature d'un autre champion");

        if (!MatchesColors(card, deckColors))
            return ValidationResult.Fail($"{card.cardName} n'est pas une couleur du deck");

        int copies = 0, sameCategory = 0;
        if (deck != null)
        {
            foreach (var c in deck)
            {
                if (c == null) continue;
                if (c.cardName == card.cardName) copies++;
                if (c.category == card.category) sameCategory++;
            }
        }

        int maxCopies = MaxCopies(card);
        if (copies >= maxCopies)
            return ValidationResult.Fail($"{card.cardName} : {maxCopies} exemplaire{(maxCopies > 1 ? "s" : "")} maximum");

        int slots = SlotsFor(card.category);
        if (sameCategory >= slots)
            return ValidationResult.Fail(slots == 0
                ? $"Les cartes {CodexCardVisual.CategoryName(card.category)} ne sont pas encore disponibles"
                : $"Plus de place pour une carte {CodexCardVisual.CategoryName(card.category)} ({slots} maximum)");

        return ValidationResult.Success();
    }

    public static ValidationResult CanRemoveCard(CardData card, ChampionData champion)
    {
        if (card == null)
            return ValidationResult.Fail("Carte manquante");

        if (IsOwnSignature(card, champion))
            return ValidationResult.Fail("Les cartes Signature sont obligatoires dans le deck");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Exemplaires de Signatures du champion (dans l'ensemble de cartes fourni) qui manquent au deck :
    /// une entrée par exemplaire manquant, jusqu'à MAX_SIGNATURE_COPIES chacune.
    /// </summary>
    public static List<CardData> MissingSignatures(IList<CardData> deck, ChampionData champion, IEnumerable<CardData> allCards)
    {
        var missing = new List<CardData>();
        if (allCards == null) return missing;

        foreach (var card in allCards)
        {
            if (!IsOwnSignature(card, champion)) continue;

            int copies = 0;
            if (deck != null)
                foreach (var c in deck)
                    if (c != null && c.cardName == card.cardName) copies++;

            for (int i = copies; i < MAX_SIGNATURE_COPIES; i++) missing.Add(card);
        }
        return missing;
    }

    /// <summary>
    /// Retire les exemplaires en trop (au-delà de MaxCopies) d'un deck existant, en gardant les
    /// premiers. Retourne le nombre de cartes retirées.
    /// </summary>
    public static int EnforceCopyLimits(List<CardData> deck)
    {
        if (deck == null) return 0;

        var seen = new Dictionary<string, int>();
        int removed = 0;
        for (int i = 0; i < deck.Count; i++)
        {
            CardData card = deck[i];
            if (card == null) continue;

            seen.TryGetValue(card.cardName, out int n);
            if (n >= MaxCopies(card))
            {
                deck.RemoveAt(i--);
                removed++;
            }
            else
            {
                seen[card.cardName] = n + 1;
            }
        }
        return removed;
    }

    static int SlotsFor(CardCategory category) => category switch
    {
        CardCategory.Signature => DeckData.SIGNATURE_SLOTS,
        CardCategory.Standard => DeckData.STANDARD_SLOTS,
        _ => 0, // Éveil : emplacements pas encore ajoutés
    };
}
