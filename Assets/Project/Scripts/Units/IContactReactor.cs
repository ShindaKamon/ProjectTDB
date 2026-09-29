/// <summary>
/// Interface pour toute unité qui réagit quand une de ses cartes la met au contact d'une autre
/// unité (ex: Réflexe du grimpeur de Crux) : en se déplaçant jusqu'à elle (charge, bond) ou en
/// la tirant contre elle (Corde de rappel). Appelée par CardData après le déplacement.
/// </summary>
public interface IContactReactor
{
    /// <param name="contact">Unité mise au contact, ou null si le lanceur a atterri (il regarde alors ses voisins)</param>
    void OnContactCreated(Unit contact);
}
