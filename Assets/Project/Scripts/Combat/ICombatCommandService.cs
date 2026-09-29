/// <summary>
/// Point d'entrée des actions des joueurs en combat : l'interface soumet des CombatCommand, le
/// service les exécute dans l'ordre (en réseau : après passage par l'hôte).
/// </summary>
public interface ICombatCommandService
{
    void Submit(CombatCommand command);

    /// <summary>Place dans CombatParty du champion dont c'est le tour (-1 hors tour de joueur).</summary>
    int ActiveActor { get; }
}
