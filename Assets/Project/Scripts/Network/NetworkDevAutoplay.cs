#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Outil de test du réseau (éditeur et builds de développement seulement) : un exe lancé avec
///   -tdb-join 127.0.0.1   rejoint l'hôte depuis le menu principal,
///   -tdb-pick Crux        choisit ce champion avec son deck de base dans le salon,
///   -tdb-autoplay         en combat, joue ses tours tout seul (placement validé, défausse, fin de tour).
/// Sert à tester l'hôte et un client sur un seul PC ; sans ces arguments, il ne fait rien.
/// </summary>
public class NetworkDevAutoplay : MonoBehaviour
{
    private string _join;
    private string _pick;
    private bool _autoplay;
    private float _nextAction;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string join = ArgValue(args, "-tdb-join");
        string pick = ArgValue(args, "-tdb-pick");
        bool autoplay = System.Array.IndexOf(args, "-tdb-autoplay") >= 0;
        if (join == null && pick == null && !autoplay) return;

        var go = new GameObject("NetworkDevAutoplay");
        DontDestroyOnLoad(go);
        var tool = go.AddComponent<NetworkDevAutoplay>();
        tool._join = join;
        tool._pick = pick;
        tool._autoplay = autoplay;
        GameLog.Log($"[Test réseau] join={join} pick={pick} autoplay={autoplay}");
    }

    private static string ArgValue(string[] args, string name)
    {
        int i = System.Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    void Update()
    {
        if (Time.time < _nextAction) return;
        _nextAction = Time.time + 1.5f;

        if (_join != null && !NetworkSession.IsActive && SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            CombatParty.IsMultiplayer = true;
            NetworkSession.GetOrCreate().StartClient(_join);
            _join = null;
            return;
        }

        if (_pick != null && NetworkSession.IsActive) TryPick();
        if (_autoplay) PlayLocalTurn();
    }

    private void TryPick()
    {
        NetworkSession session = NetworkSession.Instance;
        int index = session.Lobby.IndexOf(session.LocalClientId);
        var select = FindAnyObjectByType<ChampionSelectManager>();
        ChampionData champion = select != null ? select.FindChampion(_pick) : null;
        if (index < 0 || champion == null) return;

        session.SubmitPick(champion.championName, champion.startingDeck.ConvertAll(c => c.cardName));
        GameLog.Log($"[Test réseau] choix envoyé : {_pick}");
        _pick = null;
    }

    private void PlayLocalTurn()
    {
        ICombatCommandService commands = Services.Commands;
        if (commands == null || !commands.IsLocalTurn) return;

        int actor = commands.ActiveActor;
        Unit active = Services.Grid.GetActiveUnit();
        if (active == null)
        {
            commands.Submit(CombatCommand.PlacementNext(actor)); // placement : garde la case de départ
            return;
        }

        DeckManager deck = active.GetComponent<DeckManager>();
        if (deck != null && deck.ExcessCards > 0)
            commands.Submit(CombatCommand.Discard(actor, deck.GetHand()[0].cardName));
        else
            commands.Submit(CombatCommand.EndTurn(actor));
    }
}
#endif
