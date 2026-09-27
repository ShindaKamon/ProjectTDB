using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu principal (MainMenuScene) : Jouer (solo) ou Multijoueur -> Créer (salon local, coop
/// sur un seul PC). « Rejoindre » est réservé au multijoueur en ligne (V2) : visible mais désactivé.
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
    [SerializeField] private Button _createButton;
    [Tooltip("Multijoueur en ligne (V2) : désactivé pour l'instant.")]
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _backButton;

    [Header("Configuration")]
    [SerializeField] private string _championSelectSceneName = "ChampionSelectScene";

    void Awake()
    {
        if (_playButton != null) _playButton.onClick.AddListener(PlaySolo);
        if (_multiplayerButton != null) _multiplayerButton.onClick.AddListener(() => ShowMultiplayer(true));
        if (_quitButton != null) _quitButton.onClick.AddListener(Quit);
        if (_createButton != null) _createButton.onClick.AddListener(CreateLocalLobby);
        if (_joinButton != null) _joinButton.interactable = false;
        if (_backButton != null) _backButton.onClick.AddListener(() => ShowMultiplayer(false));

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
