/// <summary>
/// Point d'entrée des actions des joueurs en combat : l'interface soumet des CombatCommand, le
/// service les exécute dans l'ordre (en réseau : après passage par l'hôte).
/// </summary>
public interface ICombatCommandService
{
    /// <summary>Action d'un joueur de ce PC (en réseau : envoyée à l'hôte avant d'être exécutée).</summary>
    void Submit(CombatCommand command);

    /// <summary>Action confirmée (par l'hôte en réseau) : exécutée ici, dans l'ordre d'arrivée.</summary>
    void Enqueue(CombatCommand command);

    /// <summary>Place dans CombatParty du champion dont c'est le tour (-1 hors tour de joueur).</summary>
    int ActiveActor { get; }

    /// <summary>C'est le tour d'un joueur de ce PC (il peut agir).</summary>
    bool IsLocalTurn { get; }
}
