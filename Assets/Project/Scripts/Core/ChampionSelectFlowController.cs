using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coordonne la navigation plein écran de ChampionSelectScene entre ses 3 étapes :
/// sélection du champion, liste des decks du champion, construction/édition d'un deck.
/// Un seul écran (root) est actif à la fois ; les popups (CreateDeckPopup, RenameDeckPopup,
/// ConfirmDeletePopup) restent des overlays indépendants au-dessus de l'écran courant.
/// </summary>
public class ChampionSelectFlowController : MonoBehaviour
{
    public enum Screen
    {
        ChampionSelect,
        DeckList,
        DeckBuilder
    }

    [Header("Écrans plein écran (un seul actif à la fois)")]
    [SerializeField] private GameObject _screenChampionSelectRoot;
    [SerializeField] private GameObject _screenDeckListRoot;
    [SerializeField] private GameObject _screenDeckBuilderRoot;

    [Header("Références")]
    [Tooltip("Utilisé pour basculer automatiquement vers l'écran Construction du Deck " +
             "quand l'édition s'ouvre, et revenir à l'écran Liste des Decks à la fermeture " +
             "(Sauvegarder/Annuler), sans court-circuiter la logique existante de revert.")]
    [SerializeField] private DeckEditorUI _deckEditor;

    [Header("Bouton Retour (écran Liste des Decks -> écran Sélection Champion)")]
    [SerializeField] private Button _backToChampionSelectButton;

    public Screen CurrentScreen { get; private set; } = Screen.ChampionSelect;

    void Awake()
    {
        if (_deckEditor != null)
        {
            _deckEditor.OnOpened += HandleDeckEditorOpened;
            _deckEditor.OnClosed += HandleDeckEditorClosed;
        }

        if (_backToChampionSelectButton != null)
            _backToChampionSelectButton.onClick.AddListener(GoToChampionSelect);
    }

    void OnDestroy()
    {
        if (_deckEditor != null)
        {
            _deckEditor.OnOpened -= HandleDeckEditorOpened;
            _deckEditor.OnClosed -= HandleDeckEditorClosed;
        }
    }

    void Start()
    {
        ShowScreen(Screen.ChampionSelect);
    }

    /// <summary>
    /// Active l'écran demandé et désactive les deux autres.
    /// </summary>
    public void ShowScreen(Screen screen)
    {
        CurrentScreen = screen;

        if (_screenChampionSelectRoot != null)
            _screenChampionSelectRoot.SetActive(screen == Screen.ChampionSelect);

        if (_screenDeckListRoot != null)
            _screenDeckListRoot.SetActive(screen == Screen.DeckList);

        if (_screenDeckBuilderRoot != null)
            _screenDeckBuilderRoot.SetActive(screen == Screen.DeckBuilder);
    }

    // Méthodes sans paramètre pour un branchement direct depuis Button.onClick (Inspector).
    public void GoToChampionSelect() => ShowScreen(Screen.ChampionSelect);
    public void GoToDeckList() => ShowScreen(Screen.DeckList);
    public void GoToDeckBuilder() => ShowScreen(Screen.DeckBuilder);

    private void HandleDeckEditorOpened() => ShowScreen(Screen.DeckBuilder);

    private void HandleDeckEditorClosed()
    {
        // Ne revenir à l'écran Liste des Decks que si on s'y trouvait déjà avant l'édition
        // (évite un retour surprenant si l'état courant a changé entre-temps).
        if (CurrentScreen == Screen.DeckBuilder)
            ShowScreen(Screen.DeckList);
    }
}
