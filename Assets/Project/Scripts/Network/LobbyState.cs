using System.Collections.Generic;
using System.Text;

/// <summary>
/// Salon réseau (LAN) : un joueur par PC connecté, dans l'ordre d'arrivée (= ordre des tours).
/// Chacun choisit son champion et son deck sur son propre PC ; l'hôte tient la liste qui fait foi
/// et la renvoie à tous (Serialize / Deserialize). Champions et cartes voyagent par leur nom.
/// C# pur : testable en EditMode.
/// </summary>
public class LobbyState
{
    public const int MAX_PLAYERS = CombatParty.MAX_PLAYERS;

    public class Member
    {
        public ulong ClientId;
        public string ChampionName = "";              // vide = en train de choisir
        public List<string> DeckCardNames = new List<string>();

        public bool HasPicked => !string.IsNullOrEmpty(ChampionName);
    }

    private readonly List<Member> _members = new List<Member>();

    public IReadOnlyList<Member> Members => _members;
    public int Count => _members.Count;
    public bool IsFull => _members.Count >= MAX_PLAYERS;

    /// <summary>Le combat peut commencer : au moins 2 joueurs, et tous ont choisi.</summary>
    public bool CanStart => _members.Count >= 2 && _members.TrueForAll(m => m.HasPicked);

    public int IndexOf(ulong clientId) => _members.FindIndex(m => m.ClientId == clientId);

    public bool AddPlayer(ulong clientId)
    {
        if (IsFull || IndexOf(clientId) >= 0) return false;
        _members.Add(new Member { ClientId = clientId });
        return true;
    }

    public void RemovePlayer(ulong clientId) => _members.RemoveAll(m => m.ClientId == clientId);

    /// <summary>Champion déjà pris par un autre joueur ?</summary>
    public bool IsTakenByOther(string championName, ulong clientId) =>
        _members.Exists(m => m.ClientId != clientId && m.ChampionName == championName);

    /// <summary>Choix d'un joueur ; refusé si le champion est pris par un autre ou si le joueur est inconnu.</summary>
    public bool SetPick(ulong clientId, string championName, List<string> deckCardNames)
    {
        int index = IndexOf(clientId);
        if (index < 0 || string.IsNullOrEmpty(championName) || IsTakenByOther(championName, clientId)) return false;
        _members[index].ChampionName = championName;
        _members[index].DeckCardNames = deckCardNames ?? new List<string>();
        return true;
    }

    // Séparateurs de contrôle : absents des noms de champions et de cartes
    private const char MemberSep = '\u001e', FieldSep = '\u001f', CardSep = '\u001d';

    public string Serialize()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _members.Count; i++)
        {
            if (i > 0) sb.Append(MemberSep);
            Member m = _members[i];
            sb.Append(m.ClientId).Append(FieldSep).Append(m.ChampionName).Append(FieldSep)
              .Append(string.Join(CardSep.ToString(), m.DeckCardNames));
        }
        return sb.ToString();
    }

    public static LobbyState Deserialize(string data)
    {
        var state = new LobbyState();
        if (string.IsNullOrEmpty(data)) return state;

        foreach (string entry in data.Split(MemberSep))
        {
            string[] fields = entry.Split(FieldSep);
            if (fields.Length < 3 || !ulong.TryParse(fields[0], out ulong id)) continue;
            state._members.Add(new Member
            {
                ClientId = id,
                ChampionName = fields[1],
                DeckCardNames = fields[2].Length == 0 ? new List<string>() : new List<string>(fields[2].Split(CardSep))
            });
        }
        return state;
    }
}
