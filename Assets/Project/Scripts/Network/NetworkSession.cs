using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Partie en réseau local (LAN) : un PC héberge (Héberger), les autres le rejoignent par son IP
/// (Rejoindre). Crée le NetworkManager (Netcode for GameObjects + Unity Transport) à la demande,
/// tient le salon (LobbyState) — l'hôte fait foi et le renvoie à tous — et lance les scènes pour
/// tout le monde. En combat, relaie les actions des joueurs (CombatCommand) : l'hôte les vérifie,
/// les renvoie à tous dans leur ordre d'arrivée, puis chaque PC les exécute.
/// </summary>
public class NetworkSession : MonoBehaviour
{
    public const ushort Port = 7777;
    private const string MsgPick = "tdb.lobby.pick";
    private const string MsgState = "tdb.lobby.state";
    private const string MsgCommand = "tdb.combat.command"; // client -> hôte : action proposée
    private const string MsgExecute = "tdb.combat.execute"; // hôte -> clients : action à exécuter
    private const string MsgTurnState = "tdb.combat.state";  // client -> hôte : empreinte de l'état au début d'un tour
    private const string MsgDesync = "tdb.combat.desync";    // hôte -> clients : états différents à ce tour
    private const string MsgLeft = "tdb.combat.left";        // hôte -> clients : un joueur s'est déconnecté en combat
    private const string MainMenuScene = "MainMenuScene";

    public static NetworkSession Instance { get; private set; }

    /// <summary>Une partie réseau est en cours (hôte ou client connecté).</summary>
    public static bool IsActive => Instance != null && Instance._manager != null && Instance._manager.IsListening;

    public bool IsHost => _manager != null && _manager.IsHost;
    public ulong LocalClientId => _manager != null ? _manager.LocalClientId : 0;
    public LobbyState Lobby { get; private set; } = new LobbyState();

    /// <summary>Le salon a changé (arrivée, départ, choix d'un joueur).</summary>
    public event Action OnLobbyChanged;

    private NetworkManager _manager;
    private UnityTransport _transport;
    private bool _acceptingPlayers;
    private DesyncDetector _desync = new DesyncDetector();

    // Hôte : places (CombatParty) des joueurs déconnectés en plein combat ; le salon garde leur place
    // pour que les autres joueurs conservent la leur
    private readonly HashSet<int> _departed = new HashSet<int>();

    /// <summary>Le joueur à cette place s'est déconnecté en plein combat (connu de l'hôte seulement).</summary>
    public bool IsDeparted(int actor) => _departed.Contains(actor);

    /// <summary>Au moins un joueur a quitté le combat (connu de l'hôte seulement).</summary>
    public bool HasDeparted => _departed.Count > 0;

    public static NetworkSession GetOrCreate()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("NetworkSession");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<NetworkSession>();
        Instance.Setup();
        return Instance;
    }

    private void Setup()
    {
        _transport = gameObject.AddComponent<UnityTransport>();
        _manager = gameObject.AddComponent<NetworkManager>();
        if (_manager.NetworkConfig == null) _manager.NetworkConfig = new NetworkConfig();
        _manager.NetworkConfig.NetworkTransport = _transport;
        _manager.NetworkConfig.EnableSceneManagement = true;
    }

    // ========== CONNEXION ==========

    /// <summary>Ouvre la partie sur ce PC (écoute toutes les interfaces du réseau local), puis ouvre le salon.</summary>
    public bool StartHost(string lobbySceneName)
    {
        _transport.SetConnectionData("127.0.0.1", Port, "0.0.0.0");
        Subscribe();
        Lobby = new LobbyState();
        _acceptingPlayers = true;

        if (!_manager.StartHost())
        {
            GameLog.LogWarning("Réseau : impossible d'héberger la partie (port déjà utilisé ?).");
            Shutdown();
            return false;
        }

        RegisterMessages();
        GameLog.Log($"Réseau : partie hébergée sur {LocalIPv4()}:{Port}.");
        _manager.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        return true;
    }

    /// <summary>Rejoint la partie d'un hôte ; la scène du salon est chargée à la connexion.</summary>
    public bool StartClient(string hostAddress)
    {
        _transport.SetConnectionData(hostAddress.Trim(), Port);
        Subscribe();
        Lobby = new LobbyState();

        if (!_manager.StartClient())
        {
            GameLog.LogWarning($"Réseau : impossible de rejoindre {hostAddress}.");
            Shutdown();
            return false;
        }

        RegisterMessages();
        GameLog.Log($"Réseau : connexion à {hostAddress}:{Port}…");
        return true;
    }

    /// <summary>Quitte la partie réseau (et la ferme pour tous si c'est l'hôte).</summary>
    public void Shutdown()
    {
        if (_manager != null)
        {
            _manager.OnClientConnectedCallback -= OnClientConnected;
            _manager.OnClientDisconnectCallback -= OnClientDisconnected;
            if (_manager.IsListening) _manager.Shutdown();
        }
        Instance = null;
        Destroy(gameObject);
    }

    /// <summary>Adresse IPv4 de ce PC sur le réseau local (à donner aux joueurs qui rejoignent).</summary>
    public static string LocalIPv4()
    {
        try
        {
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    return address.ToString();
            }
        }
        catch (SocketException) { }
        return "127.0.0.1";
    }

    private void Subscribe()
    {
        _manager.OnClientConnectedCallback += OnClientConnected;
        _manager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void RegisterMessages()
    {
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgPick, OnPickReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgState, OnStateReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgCommand, OnCommandReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgExecute, OnExecuteReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgTurnState, OnTurnStateReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgDesync, OnDesyncReceived);
        _manager.CustomMessagingManager.RegisterNamedMessageHandler(MsgLeft, OnLeftReceived);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!_manager.IsServer) return;

        // Salon fermé (combat lancé) ou complet : le nouveau venu est refusé
        if (!_acceptingPlayers || !Lobby.AddPlayer(clientId))
        {
            if (clientId != NetworkManager.ServerClientId) _manager.DisconnectClient(clientId);
            return;
        }
        GameLog.Log($"Réseau : joueur {clientId} connecté ({Lobby.Count}/{LobbyState.MAX_PLAYERS}).");
        BroadcastLobby();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_manager.IsServer)
        {
            if (!_acceptingPlayers)
            {
                PlayerLeftCombat(clientId);
                return;
            }

            Lobby.RemovePlayer(clientId);
            GameLog.Log($"Réseau : joueur {clientId} parti.");
            BroadcastLobby();
            return;
        }

        // Côté client : l'hôte a fermé la partie ou la connexion a échoué
        GameLog.LogWarning("Réseau : connexion à l'hôte perdue, retour au menu.");
        LeaveToMenu();
    }

    /// <summary>Ferme la session réseau (si active) et retourne au menu principal.</summary>
    public static void LeaveToMenu()
    {
        if (IsActive) Instance.Shutdown();
        CombatParty.Clear();
        SceneManager.LoadScene(MainMenuScene);
    }

    // ========== SALON ==========

    /// <summary>Choix de ce joueur (champion et deck par leur nom) ; l'hôte le valide et le renvoie à tous.</summary>
    public void SubmitPick(string championName, List<string> deckCardNames)
    {
        if (!IsActive) return;

        if (_manager.IsServer)
        {
            if (Lobby.SetPick(LocalClientId, championName, deckCardNames)) BroadcastLobby();
            return;
        }

        // Le choix voyage comme un salon d'un seul joueur
        var pick = new LobbyState();
        pick.AddPlayer(LocalClientId);
        pick.SetPick(LocalClientId, championName, deckCardNames);
        Send(MsgPick, pick.Serialize(), NetworkManager.ServerClientId);
    }

    /// <summary>Hôte : lance le combat pour tous (salon complet et prêt).</summary>
    public bool StartCombat(string combatSceneName)
    {
        if (!IsActive || !_manager.IsServer || !Lobby.CanStart) return false;

        _acceptingPlayers = false;
        Lobby.Seed = new System.Random().Next(1, int.MaxValue); // mélange des decks commun à tous les PC
        _desync = new DesyncDetector();
        _departed.Clear();
        BroadcastLobby();
        _manager.SceneManager.LoadScene(combatSceneName, LoadSceneMode.Single);
        return true;
    }

    // ========== COMBAT ==========

    /// <summary>
    /// Action d'un joueur de ce PC : envoyée à l'hôte, qui la vérifie et la renvoie à tous
    /// (l'hôte la traite directement)
    /// </summary>
    public void SubmitCommand(CombatCommand command)
    {
        if (_manager.IsServer) AcceptCommand(LocalClientId, command);
        else Send(MsgCommand, command.Serialize(), NetworkManager.ServerClientId);
    }

    private void OnCommandReceived(ulong senderClientId, FastBufferReader reader)
    {
        if (!_manager.IsServer) return;
        reader.ReadValueSafe(out string data);
        AcceptCommand(senderClientId, CombatCommand.Deserialize(data));
    }

    // Hôte : un joueur n'agit que pour son propre champion ; l'ordre d'arrivée fait foi pour tous
    private void AcceptCommand(ulong senderClientId, CombatCommand command)
    {
        if (Lobby.IndexOf(senderClientId) != command.Actor)
        {
            GameLog.LogWarning($"Réseau : action refusée, le joueur {senderClientId} ne contrôle pas le joueur {command.Actor + 1} ({command}).");
            return;
        }

        BroadcastCommand(command);
    }

    /// <summary>
    /// Hôte : action décidée par l'hôte lui-même pour un joueur absent (passer son tour), renvoyée à
    /// tous comme n'importe quelle action.
    /// </summary>
    public void SubmitCommandForDeparted(CombatCommand command)
    {
        if (_manager.IsServer && IsDeparted(command.Actor)) BroadcastCommand(command);
    }

    private void BroadcastCommand(CombatCommand command)
    {
        string data = command.Serialize();
        foreach (ulong clientId in _manager.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId) Send(MsgExecute, data, clientId);
        }
        Services.Commands?.Enqueue(command);
    }

    // Hôte : un joueur part en plein combat. Son champion reste sur la grille ; tous sont prévenus
    // et l'hôte passera ses tours (CombatCommandExecutor).
    private void PlayerLeftCombat(ulong clientId)
    {
        int actor = Lobby.IndexOf(clientId);
        if (actor < 0 || !_departed.Add(actor)) return;

        GameLog.Log($"Réseau : joueur {clientId} (place {actor + 1}) déconnecté en plein combat.");
        foreach (ulong other in _manager.ConnectedClientsIds)
        {
            if (other != NetworkManager.ServerClientId) Send(MsgLeft, actor.ToString(), other);
        }
        EventBus.Publish(new NetworkPlayerLeftEvent(actor));
    }

    private void OnLeftReceived(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string data);
        if (int.TryParse(data, out int actor)) EventBus.Publish(new NetworkPlayerLeftEvent(actor));
    }

    private void OnExecuteReceived(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string data);
        Services.Commands?.Enqueue(CombatCommand.Deserialize(data));
    }

    // ========== CONTRÔLE DE SYNCHRONISATION ==========

    private const char StateSep = '\u001f';

    /// <summary>
    /// Empreinte de l'état de ce PC au début d'un tour (CombatStateFingerprint) : l'hôte la garde,
    /// un client l'envoie à l'hôte, qui signale à tous tout écart (NetworkDesyncEvent)
    /// </summary>
    public void ReportTurnState(int turn, string state)
    {
        if (_manager.IsServer) ReportMismatches(_desync.AddHost(turn, state));
        else Send(MsgTurnState, turn + StateSep.ToString() + state, NetworkManager.ServerClientId);
    }

    private void OnTurnStateReceived(ulong senderClientId, FastBufferReader reader)
    {
        if (!_manager.IsServer) return;
        reader.ReadValueSafe(out string data);
        int sep = data.IndexOf(StateSep);
        if (sep < 0 || !int.TryParse(data.Substring(0, sep), out int turn)) return;
        ReportMismatches(_desync.AddClient(senderClientId, turn, data.Substring(sep + 1)));
    }

    private void ReportMismatches(List<DesyncDetector.Mismatch> mismatches)
    {
        foreach (DesyncDetector.Mismatch m in mismatches)
        {
            Debug.LogWarning($"Réseau : désynchronisation au tour {m.Turn} avec le joueur {m.ClientId}.\n" +
                             $"Hôte  : {m.HostState}\nClient : {m.ClientState}");
            foreach (ulong clientId in _manager.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.ServerClientId) Send(MsgDesync, m.Turn.ToString(), clientId);
            }
            EventBus.Publish(new NetworkDesyncEvent(m.Turn));
        }
    }

    private void OnDesyncReceived(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string data);
        if (int.TryParse(data, out int turn)) EventBus.Publish(new NetworkDesyncEvent(turn));
    }

    private void OnPickReceived(ulong senderClientId, FastBufferReader reader)
    {
        if (!_manager.IsServer) return;
        reader.ReadValueSafe(out string data);
        LobbyState pick = LobbyState.Deserialize(data);
        if (pick.Count == 0) return;

        // L'expéditeur ne peut choisir que pour lui-même
        LobbyState.Member member = pick.Members[0];
        if (Lobby.SetPick(senderClientId, member.ChampionName, member.DeckCardNames)) BroadcastLobby();
        else BroadcastLobby(); // refusé (champion pris) : on renvoie l'état pour que le joueur voie la vraie liste
    }

    private void OnStateReceived(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string data);
        Lobby = LobbyState.Deserialize(data);
        OnLobbyChanged?.Invoke();
    }

    private void BroadcastLobby()
    {
        OnLobbyChanged?.Invoke();
        if (_manager.ConnectedClientsIds.Count <= 1) return;

        string data = Lobby.Serialize();
        foreach (ulong clientId in _manager.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId) Send(MsgState, data, clientId);
        }
    }

    private void Send(string messageName, string data, ulong clientId)
    {
        using var writer = new FastBufferWriter(1024, Allocator.Temp, 64 * 1024);
        writer.WriteValueSafe(data);
        _manager.CustomMessagingManager.SendNamedMessage(messageName, clientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
    }
}
