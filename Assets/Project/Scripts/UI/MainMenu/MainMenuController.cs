using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu principal (MainMenuScene) : Jouer (solo) ou Multijoueur -> Même PC (salon local, coop sur
/// un seul PC), Héberger (partie en réseau local, ce PC est l'hôte) ou Rejoindre (adresse IP de
/// l'hôte). Voir NetworkSession.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panneaux")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _multiplayerPanel;

    [Header("Menu principal")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _multiplayerButton;
    [SerializeField] private Button _quitButton;

    [Header("Multijoueur")]
    [Tooltip("Même PC : salon local, tout le monde joue sur ce PC.")]
    [SerializeField] private Button _createButton;
    [Tooltip("Héberger une partie en réseau local.")]
    [SerializeField] private Button _hostButton;
    [Tooltip("Rejoindre la partie d'un hôte (adresse IP ci-dessous).")]
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_InputField _hostAddressInput;
    [Tooltip("Messages de connexion (connexion en cours, échec…).")]
    [SerializeField] private TextMeshProUGUI _networkStatusText;
    [SerializeField] private Button _backButton;

    [Header("Configuration")]
    [SerializeField] private string _championSelectSceneName = "ChampionSelectScene";

    private const string LastHostAddressKey = "tdb.lastHostAddress";

    void Awake()
    {
        // Retour au menu depuis une partie réseau : on la quitte
        if (NetworkSession.Instance != null) NetworkSession.Instance.Shutdown();

        if (_playButton != null) _playButton.onClick.AddListener(PlaySolo);
        if (_multiplayerButton != null) _multiplayerButton.onClick.AddListener(() => ShowMultiplayer(true));
        if (_quitButton != null) _quitButton.onClick.AddListener(Quit);
        if (_createButton != null) _createButton.onClick.AddListener(CreateLocalLobby);
        if (_hostButton != null) _hostButton.onClick.AddListener(HostNetworkGame);
        if (_joinButton != null) _joinButton.onClick.AddListener(JoinNetworkGame);
        if (_backButton != null) _backButton.onClick.AddListener(() => ShowMultiplayer(false));
        if (_hostAddressInput != null) _hostAddressInput.text = PlayerPrefs.GetString(LastHostAddressKey, "");

        SetStatus("");
        ShowMultiplayer(false);
    }

    private void ShowMultiplayer(bool show)
    {
        if (_mainPanel != null) _mainPanel.SetActive(!show);
        if (_multiplayerPanel != null) _multiplayerPanel.SetActive(show);
    }

    private void PlaySolo() => LoadChampionSelect(false);

    private void CreateLocalLobby() => LoadChampionSelect(true);

    private void LoadChampionSelect(bool multiplayer)
    {
        CombatParty.Clear();
        CombatParty.IsMultiplayer = multiplayer;
        SceneManager.LoadScene(_championSelectSceneName);
    }

    // Réseau : l'hôte ouvre la partie puis le salon, que les autres rejoignent par son adresse IP
    private void HostNetworkGame()
    {
        CombatParty.Clear();
        CombatParty.IsMultiplayer = true;
        if (!NetworkSession.GetOrCreate().StartHost(_championSelectSceneName))
            SetStatus("Impossible d'héberger (le port 7777 est-il déjà utilisé ?)");
    }

    private void JoinNetworkGame()
    {
        string address = _hostAddressInput != null ? _hostAddressInput.text.Trim() : "";
        if (string.IsNullOrEmpty(address))
        {
            SetStatus("Entre l'adresse IP de l'hôte (affichée dans son salon).");
            return;
        }

        PlayerPrefs.SetString(LastHostAddressKey, address);
        CombatParty.Clear();
        CombatParty.IsMultiplayer = true;
        SetStatus(NetworkSession.GetOrCreate().StartClient(address)
            ? $"Connexion à {address}…"
            : $"Impossible de rejoindre {address}.");
    }

    private void SetStatus(string message)
    {
        if (_networkStatusText != null) _networkStatusText.text = message;
    }

    private void Quit()
    {
        GameLog.Log("Quitter le jeu.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
