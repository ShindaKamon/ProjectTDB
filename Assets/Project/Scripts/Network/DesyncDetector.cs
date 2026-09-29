using System.Collections.Generic;

/// <summary>
/// Hôte : compare l'empreinte de l'état (CombatStateFingerprint) de chaque client à la sienne,
/// tour par tour. Les deux peuvent arriver dans n'importe quel ordre (un client en avance ou en
/// retard d'un tour). C# pur : testable en EditMode.
/// </summary>
public class DesyncDetector
{
    public readonly struct Mismatch
    {
        public readonly ulong ClientId;
        public readonly int Turn;
        public readonly string HostState;
        public readonly string ClientState;

        public Mismatch(ulong clientId, int turn, string hostState, string clientState)
        {
            ClientId = clientId;
            Turn = turn;
            HostState = hostState;
            ClientState = clientState;
        }
    }

    private readonly Dictionary<int, string> _host = new Dictionary<int, string>();
    private readonly List<(ulong client, int turn, string state)> _waiting = new List<(ulong, int, string)>();

    /// <summary>État de l'hôte au début d'un tour ; renvoie les écarts avec les clients déjà reçus.</summary>
    public List<Mismatch> AddHost(int turn, string state)
    {
        _host[turn] = state;
        var mismatches = new List<Mismatch>();
        for (int i = _waiting.Count - 1; i >= 0; i--)
        {
            if (_waiting[i].turn != turn) continue;
            if (_waiting[i].state != state) mismatches.Add(new Mismatch(_waiting[i].client, turn, state, _waiting[i].state));
            _waiting.RemoveAt(i);
        }
        return mismatches;
    }

    /// <summary>État d'un client ; renvoie un écart s'il diffère de celui de l'hôte au même tour.</summary>
    public List<Mismatch> AddClient(ulong clientId, int turn, string state)
    {
        var mismatches = new List<Mismatch>();
        if (_host.TryGetValue(turn, out string hostState))
        {
            if (hostState != state) mismatches.Add(new Mismatch(clientId, turn, hostState, state));
        }
        else
        {
            _waiting.Add((clientId, turn, state));
        }
        return mismatches;
    }
}
