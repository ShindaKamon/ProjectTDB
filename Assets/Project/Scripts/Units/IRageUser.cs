/// <summary>
/// Unité qui a une Rage (ex: Ilya, voir RageGauge) : les cartes qui la remplissent, la consomment
/// ou brisent les chaînes (CardData, champs « RAGE ») passent par cette interface.
/// </summary>
public interface IRageUser
{
    int RageStock { get; }

    /// <summary>Forme Déchaînée en cours (chaînes brisées).</summary>
    bool IsUnchained { get; }

    /// <summary>Tours restants de la forme Déchaînée (0 hors de cette forme).</summary>
    int UnchainedTurnsLeft { get; }

    /// <summary>Stock plein et pas déjà Déchaîné.</summary>
    bool CanBreakChains { get; }

    void GainRage(int amount);

    /// <summary>Vide le stock ; retourne la Rage consommée.</summary>
    int ConsumeAllRage();

    void BreakChains();
}
