/// <summary>
/// Interface pour toute unité capable de modifier les dégâts qu'elle inflige via une carte
/// (ex: bonus "prochaine carte" de Réflexe du grimpeur pour L'Alpiniste). Consultée par
/// CardData.ExecuteEffect juste avant l'application des dégâts finaux.
/// </summary>
public interface IOutgoingDamageModifier
{
    /// <summary>Multiplicateur à appliquer aux dégâts sortants (1.0 = aucun bonus).</summary>
    float GetDamageMultiplier();

    /// <summary>Consomme le bonus à usage unique après application.</summary>
    void ConsumeDamageModifier();
}
