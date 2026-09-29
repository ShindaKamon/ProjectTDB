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
/// tout le monde. Étape 1 : salon et lancement ; le combat n'est pas encore synchronisé.
/// </summary>
public class NetworkSession : MonoBehaviour
{
    public const ushort Port = 7777;
    private const string MsgPick = "tdb.lobby.pick";
    private const string MsgState = "tdb.lobby.state";
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
            Lobby.RemovePlayer(clientId);
            GameLog.Log($"Réseau : joueur {clientId} parti.");
            BroadcastLobby();
            return;
        }

        // Côté client : l'hôte a fermé la partie ou la connexion a échoué
        GameLog.LogWarning("Réseau : connexion à l'hôte perdue, retour au menu.");
        Shutdown();
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
        BroadcastLobby();
        _manager.SceneManager.LoadScene(combatSceneName, LoadSceneMode.Single);
        return true;
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
