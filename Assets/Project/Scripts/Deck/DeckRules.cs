using System.Collections.Generic;

/// <summary>
/// Règles de construction d'un deck (décisions du 24/09/2026), centralisées ici plutôt que dans l'UI :
/// - un champion peut jouer toutes les émotions, mais chaque deck a ses couleurs (1 ou 2, choisies à
///   sa création) et ne contient que des cartes de ces couleurs, plus les Signatures du champion ;
/// - les Signatures d'un autre champion sont interdites ; celles du champion sont obligatoires ;
/// - 4 exemplaires maximum d'une même carte, 1 seul pour une Signature ;
/// - emplacements par catégorie : DeckData.SIGNATURE_SLOTS et DeckData.STANDARD_SLOTS.
/// C# pur : testable en EditMode.
/// </summary>
public static class DeckRules
{
    public const int MAX_COPIES = 4;
    public const int MAX_SIGNATURE_COPIES = 1;

    /// <summary>
    /// Émotions qu'on peut choisir comme couleurs d'un deck : celles du lancement (MVP). Les 5 autres
    /// de la roue de Plutchik viendront plus tard : il suffira de les ajouter ici.
    /// </summary>
    public static readonly IReadOnlyList<EmotionType> AvailableEmotions = new[]
    {
        EmotionType.Colere, EmotionType.Peur, EmotionType.Joie
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
                ? $"Les cartes {card.category} ne sont pas encore disponibles"
                : $"Plus de place pour une carte {card.category} ({slots} maximum)");

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

    /// <summary>Signatures du champion (dans l'ensemble de cartes fourni) absentes du deck.</summary>
    public static List<CardData> MissingSignatures(IList<CardData> deck, ChampionData champion, IEnumerable<CardData> allCards)
    {
        var missing = new List<CardData>();
        if (allCards == null) return missing;

        foreach (var card in allCards)
        {
            if (!IsOwnSignature(card, champion)) continue;

            bool present = false;
            if (deck != null)
                foreach (var c in deck)
                    if (c != null && c.cardName == card.cardName) { present = true; break; }

            if (!present) missing.Add(card);
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

    /// <summary>
    /// Retire d'un deck existant les cartes qui ne sont pas de ses couleurs. Retourne le nombre de
    /// cartes retirées (aucune si le deck n'a pas de couleur).
    /// </summary>
    public static int EnforceColors(List<CardData> deck, ICollection<EmotionType> deckColors)
    {
        if (deck == null || deckColors == null || deckColors.Count == 0) return 0;
        return deck.RemoveAll(c => c != null && !MatchesColors(c, deckColors));
    }

    static int SlotsFor(CardCategory category) => category switch
    {
        CardCategory.Signature => DeckData.SIGNATURE_SLOTS,
        CardCategory.Standard => DeckData.STANDARD_SLOTS,
        _ => 0, // Éveil : emplacements pas encore ajoutés
    };
}
