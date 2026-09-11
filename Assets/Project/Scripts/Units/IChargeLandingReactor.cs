/// <summary>
/// Interface pour toute unité qui réagit à son propre atterrissage après une carte de charge
/// (ex: Réflexe du grimpeur de L'Alpiniste). Appelée par CardData juste après la fin du
/// déplacement, avant l'application des dégâts/knockback éventuels sur l'ennemi touché.
/// </summary>
public interface IChargeLandingReactor
{
    void OnChargeLanded();
}
