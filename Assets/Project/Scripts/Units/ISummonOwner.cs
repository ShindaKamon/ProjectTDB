/// <summary>
/// Interface pour toute unité capable d'invoquer et de contrôler indirectement une
/// SummonUnit (ex: Soren et Lyse). Permet à CardData de déclencher l'invocation, le
/// repositionnement et de consulter l'invocation active sans connaître le champion précis.
/// </summary>
public interface ISummonOwner
{
    SummonUnit ActiveSummon { get; }
    void RegisterSummon(SummonUnit summon);
    /// <summary>Téléporte une invocation de ce lanceur (choisie par le joueur) sur une case libre.</summary>
    void RepositionSummon(SummonUnit summon, UnityEngine.Vector2Int newPos);
}
