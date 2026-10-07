/// <summary>
/// Rage d'Ilya — classe pure, sans Unity. Les PV perdus (coups reçus, PV payés, contrecoups)
/// s'accumulent : chaque palier de PointsPerCard donne une carte RAGE en main. Jouée, une carte
/// RAGE remplit le stock (plafonné à MaxStock) ; le stock se dépense (Exutoire brutal, Second
/// souffle) ou, plein, brise les chaînes (forme Déchaînée). Règles : Docs/GDD/archive/ilya_deck_simple.md.
/// </summary>
public class RageGauge
{
    public const int MaxStock = 5;

    private readonly int _pointsPerCard;
    private int _accumulated;

    public RageGauge(int pointsPerCard) => _pointsPerCard = System.Math.Max(1, pointsPerCard);

    public int Stock { get; private set; }

    public bool IsFull => Stock >= MaxStock;

    /// <summary>
    /// PV perdus : retourne le nombre de cartes RAGE à donner. Jamais plus que ce que le stock peut
    /// encore recevoir, cartes RAGE déjà en main comprises (cardsInHand) ; l'excédent est perdu.
    /// </summary>
    public int OnHealthLost(int amount, int cardsInHand)
    {
        if (amount <= 0) return 0;

        _accumulated += amount;
        int cards = _accumulated / _pointsPerCard;
        _accumulated %= _pointsPerCard;
        return System.Math.Max(0, System.Math.Min(cards, MaxStock - Stock - cardsInHand));
    }

    /// <summary>Ajoute au stock (plafonné). Retourne la Rage réellement gagnée.</summary>
    public int Gain(int amount)
    {
        int gained = System.Math.Max(0, System.Math.Min(amount, MaxStock - Stock));
        Stock += gained;
        return gained;
    }

    /// <summary>Vide le stock. Retourne la Rage consommée.</summary>
    public int ConsumeAll()
    {
        int consumed = Stock;
        Stock = 0;
        return consumed;
    }
}
