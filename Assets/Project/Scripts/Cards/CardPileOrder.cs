using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ordre d'affichage des piles consultables en combat. C# pur : testable en EditMode.
/// - Pioche : triée par coût puis par nom, pour ne jamais révéler l'ordre de tirage.
/// - Défausse : de la dernière carte défaussée à la première, pour voir ce qu'on vient de jouer.
/// </summary>
public static class CardPileOrder
{
    public static List<CardData> ForDeckView(IEnumerable<CardData> deck) =>
        deck.Where(c => c != null)
            .OrderBy(c => c.costPA)
            .ThenBy(c => c.cardName)
            .ToList();

    public static List<CardData> ForDiscardView(IEnumerable<CardData> discard) =>
        discard.Where(c => c != null).Reverse().ToList();
}
