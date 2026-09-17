using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coordonne la navigation plein écran de ChampionSelectScene entre ses 2 étapes :
/// sélection du champion, et écran unifié de gestion des decks (loadout + pool + deck +
/// courbe PA, façon MTG Arena). Les popups (CreateDeckPopup, RenameDeckPopup,
/// ConfirmDeletePopup) restent des overlays indépendants au-dessus de l'écran courant.
/// </summary>
public class ChampionSelectFlowController : MonoBehaviour
{
    public enum Screen
    {
        ChampionSelect,
        DeckManager
    }

    [Header("Écrans plein écran (un seul actif à la fois)")]
    [SerializeField] private GameObject _screenChampionSelectRoot;
    [SerializeField] private GameObject _screenDeckManagerRoot;

    [Header("Bouton Retour (écran Gestion des Decks -> écran Sélection Champion)")]
    [SerializeField] private Button _backToChampionSelectButton;

    [Header("Illustration Personnage")]
    [Tooltip("Image plein écran de l'écran Sélection Champion, alimentée par ChampionData.fullBodyArt.")]
    [SerializeField] private Image _characterArtChampionSelect;
    [Tooltip("Image de la bande personnage dédiée (écran Gestion des Decks, colonne exclusive " +
             "jamais recouverte par le pool/deck), même sprite que ci-dessus.")]
    [SerializeField] private Image _characterArtDeckManager;

    private ChampionData _currentArtChampion;

    public Screen CurrentScreen { get; private set; } = Screen.ChampionSelect;

    void Awake()
    {
        if (_backToChampionSelectButton != null)
            _backToChampionSelectButton.onClick.AddListener(GoToChampionSelect);
    }

    void Start()
    {
        ShowScreen(Screen.ChampionSelect);
    }

    /// <summary>
    /// Active l'écran demandé et désactive l'autre.
    /// </summary>
    public void ShowScreen(Screen screen)
    {
        CurrentScreen = screen;

        if (_screenChampionSelectRoot != null)
            _screenChampionSelectRoot.SetActive(screen == Screen.ChampionSelect);

        if (_screenDeckManagerRoot != null)
            _screenDeckManagerRoot.SetActive(screen == Screen.DeckManager);
    }

    /// <summary>
    /// Met à jour l'illustration du champion sélectionné sur les 2 emplacements (plein écran
    /// Sélection Champion, bande dédiée Gestion des Decks). Appelé par ChampionSelectManager à
    /// chaque changement de sélection.
    /// </summary>
    public void UpdateCharacterArt(ChampionData champion)
    {
        _currentArtChampion = champion;

        Sprite fullBody = champion != null ? champion.fullBodyArt : null;

        ApplySprite(_characterArtChampionSelect, fullBody);
        ApplySprite(_characterArtDeckManager, fullBody);
    }

    private static void ApplySprite(Image image, Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    // Méthodes sans paramètre pour un branchement direct depuis Button.onClick (Inspector).
    public void GoToChampionSelect() => ShowScreen(Screen.ChampionSelect);
    public void GoToDeckManager() => ShowScreen(Screen.DeckManager);
}
