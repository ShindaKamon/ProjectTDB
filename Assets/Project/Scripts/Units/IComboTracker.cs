/// <summary>
/// Interface pour toute unité qui détecte des motifs de combo sur les cartes jouées ce tour
/// (ex: Main gagnante d'Ace). CardData.ExecuteEffect appelle OnCardAboutToExecute AVANT de
/// résoudre les effets de la carte (pour que les bonus détectés s'appliquent à CETTE carte),
/// puis OnCardResolved juste après (pour mettre à jour l'historique pour la carte suivante).
/// </summary>
public interface IComboTracker
{
    /// <summary>PA dépensés ce tour AVANT la carte en cours d'exécution.</summary>
    int PASpentThisTurn { get; }

    /// <summary>Si true, les dégâts de la carte en cours doivent ignorer toute réduction de
    /// dégâts en pourcentage active sur la cible (ex: bouclier de L'Alpiniste).</summary>
    bool ShouldIgnoreDamageReduction { get; }

    void OnCardAboutToExecute(CardData card);
    void OnCardResolved(CardData card);
}
