/// <summary>
/// Interface pour toute unité capable d'invoquer et de contrôler indirectement une
/// SummonUnit (ex: Soren et Lyse). Permet à CardData de déclencher l'invocation, le
/// repositionnement et de consulter l'invocation active sans connaître le champion précis.
/// </summary>
public interface ISummonOwner
{
    SummonUnit ActiveSummon { get; }
    void RegisterSummon(SummonUnit summon);
    void RepositionSummon(UnityEngine.Vector2Int newPos);
}
