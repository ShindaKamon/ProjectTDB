using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coordonne la navigation plein écran de ChampionSelectScene entre ses 3 étapes :
/// sélection du champion -> choix du deck (liste des decks du champion : ouvrir, créer,
/// renommer, supprimer) -> gestionnaire de deck (pool + liste du deck + courbe PA, façon
/// MTG Arena). Les popups (CreateDeckPopup, RenameDeckPopup, ConfirmDeletePopup) restent des
/// overlays indépendants au-dessus de l'écran courant.
/// </summary>
public class ChampionSelectFlowController : MonoBehaviour
{
    public enum Screen
    {
        ChampionSelect,
        DeckSelect,
        DeckManager
    }

    [Header("Écrans plein écran (un seul actif à la fois)")]
    [SerializeField] private GameObject _screenChampionSelectRoot;
    [SerializeField] private GameObject _screenDeckSelectRoot;
    [SerializeField] private GameObject _screenDeckManagerRoot;

    [Header("Boutons Retour")]
    [Tooltip("Écran Choix du deck -> écran Sélection du champion.")]
    [SerializeField] private Button _backToChampionSelectButton;
    [Tooltip("Gestionnaire de deck -> écran Choix du deck.")]
    [SerializeField] private Button _backToDeckSelectButton;

    [Header("Choix du deck")]
    [Tooltip("Liste des decks : ouvrir un deck mène au gestionnaire de deck.")]
    [SerializeField] private LoadoutTabsUI _deckList;

    [Header("Illustration Personnage")]
    [Tooltip("Image plein écran de l'écran Sélection Champion, alimentée par ChampionData.fullBodyArt.")]
    [SerializeField] private Image _characterArtChampionSelect;

    private ChampionData _currentArtChampion;

    public Screen CurrentScreen { get; private set; } = Screen.ChampionSelect;

    void Awake()
    {
        if (_backToChampionSelectButton != null)
            _backToChampionSelectButton.onClick.AddListener(GoToChampionSelect);

        if (_backToDeckSelectButton != null)
            _backToDeckSelectButton.onClick.AddListener(GoToDeckSelect);

        if (_deckList != null)
            _deckList.OnDeckOpened += GoToDeckManager;
    }

    void OnDestroy()
    {
        if (_deckList != null)
            _deckList.OnDeckOpened -= GoToDeckManager;
    }

    void Start()
    {
        ShowScreen(Screen.ChampionSelect);
    }

    /// <summary>
    /// Active l'écran demandé et désactive les autres.
    /// </summary>
    public void ShowScreen(Screen screen)
    {
        CurrentScreen = screen;

        if (_screenChampionSelectRoot != null)
            _screenChampionSelectRoot.SetActive(screen == Screen.ChampionSelect);

        if (_screenDeckSelectRoot != null)
            _screenDeckSelectRoot.SetActive(screen == Screen.DeckSelect);

        if (_screenDeckManagerRoot != null)
            _screenDeckManagerRoot.SetActive(screen == Screen.DeckManager);
    }

    /// <summary>
    /// Met à jour l'illustration plein écran du champion sélectionné (écran Sélection Champion).
    /// </summary>
    public void UpdateCharacterArt(ChampionData champion)
    {
        _currentArtChampion = champion;

        Sprite fullBody = champion != null ? champion.fullBodyArt : null;

        ApplySprite(_characterArtChampionSelect, fullBody);
    }

    private static void ApplySprite(Image image, Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    // Méthodes sans paramètre pour un branchement direct depuis Button.onClick (Inspector).
    public void GoToChampionSelect() => ShowScreen(Screen.ChampionSelect);
    public void GoToDeckSelect() => ShowScreen(Screen.DeckSelect);
    public void GoToDeckManager() => ShowScreen(Screen.DeckManager);
}
