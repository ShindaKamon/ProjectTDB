using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Barre d'onglets de loadout (façon MTG Arena) : jusqu'à 4 decks (1 base + 3 custom) + un
/// onglet "+". Remplace l'ancien DeckSlotArea de l'écran "Mes Decks" ; ne gère plus que la
/// sélection de deck et le menu contextuel (Renommer/Supprimer) de l'onglet actif — l'édition
/// du contenu du deck (pool + liste groupée + courbe PA) vit entièrement dans DeckEditorUI,
/// toujours visible et éditable (autosave permanent, plus de mode Modifier séparé).
/// </summary>
public class LoadoutTabsUI : MonoBehaviour
{
    [Header("Onglets de loadout")]
    [SerializeField] private Transform _tabsParent;
    [SerializeField] private GameObject _tabPrefab;
    [SerializeField] private Button _addTabButton;

    [Header("Actions sur le deck sélectionné (boutons sous la liste)")]
    [SerializeField] private Button _openDeckButton;      // « Modifier » : ouvre le gestionnaire de deck
    [SerializeField] private Button _renameDeckButton;
    [SerializeField] private Button _deleteDeckButton;    // indisponible pour le deck de base

    [Header("Popups")]
    [SerializeField] private CreateDeckPopup _createDeckPopup;
    [SerializeField] private RenameDeckPopup _renameDeckPopup;
    [SerializeField] private ConfirmDeletePopup _confirmDeletePopup;

    [Header("Éditeur de deck (Pool + Deck + Courbe PA)")]
    [SerializeField] private DeckEditorUI _deckEditor;

    [Header("Configuration")]
    [SerializeField] private CardCollection _cardCollection;

    private ChampionData _currentChampion;
    private ChampionDecksData _currentDecksData;
    private List<DeckSlotUI> _tabs = new List<DeckSlotUI>();
    private int _activeDeckIndex = -1;

    /// <summary>Émis à chaque changement de deck actif ou de contenu du deck actif.</summary>
    public System.Action<List<CardData>> OnDeckSelected;

    /// <summary>
    /// Émis quand le joueur ouvre un deck (clic sur un deck, ou deck tout juste créé) : l'écran
    /// de choix du deck passe alors au gestionnaire de deck.
    /// </summary>
    public System.Action OnDeckOpened;

    void Awake()
    {
        if (_addTabButton != null)
            _addTabButton.onClick.AddListener(OnAddTabClicked);

        if (_openDeckButton != null)
            _openDeckButton.onClick.AddListener(OpenSelectedDeck);

        if (_renameDeckButton != null)
            _renameDeckButton.onClick.AddListener(RenameSelectedDeck);

        if (_deleteDeckButton != null)
            _deleteDeckButton.onClick.AddListener(DeleteSelectedDeck);

        SetupPopupCallbacks();
        ConfigureLayoutGroup();
    }

    /// <summary>
    /// Configure le layout group du parent pour un affichage correct des onglets
    /// </summary>
    private void ConfigureLayoutGroup()
    {
        if (_tabsParent == null) return;

        var horizontalLayout = _tabsParent.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout == null)
            horizontalLayout = _tabsParent.gameObject.AddComponent<HorizontalLayoutGroup>();

        horizontalLayout.childControlWidth = true;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.childForceExpandHeight = false;
        horizontalLayout.spacing = 20f;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter; // decks centrés sur la page Choix du deck

        ConfigureAddTabButton();
    }

    /// <summary>
    /// Configure le bouton "+" pour qu'il s'aligne correctement avec les onglets
    /// </summary>
    private void ConfigureAddTabButton()
    {
        if (_addTabButton == null) return;

        var layoutElement = _addTabButton.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = _addTabButton.gameObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = 120f;
        layoutElement.minHeight = 120f;
        layoutElement.preferredWidth = 120f;
        layoutElement.preferredHeight = 120f;

        _addTabButton.transform.SetAsLastSibling();
    }

    private void SetupPopupCallbacks()
    {
        if (_createDeckPopup != null)
            _createDeckPopup.OnDeckCreated += HandleDeckCreated;

        if (_renameDeckPopup != null)
            _renameDeckPopup.OnDeckRenamed += HandleDeckRenamed;

        if (_confirmDeletePopup != null)
            _confirmDeletePopup.OnDeleteConfirmed += HandleDeleteConfirmed;

        if (_deckEditor != null)
        {
            _deckEditor.OnDeckSaved += HandleDeckSaved;
            _deckEditor.OnDeckChanged += HandleDeckChanged;
            _deckEditor.OnDeckDuplicated += HandleDeckDuplicated;
        }
    }

    /// <summary>
    /// Affiche les decks du champion spécifié
    /// </summary>
    public void ShowDecksForChampion(ChampionData champion)
    {
        _currentChampion = champion;
        _currentDecksData = DeckSaveManager.GetDecksForChampion(champion);
        _activeDeckIndex = _currentDecksData.selectedDeckIndex;

        RefreshTabs();
        LoadActiveDeckIntoEditor();
        UpdateAddTabButtonState();
        UpdateActionButtons();
    }

    private void RefreshTabs()
    {
        foreach (var tab in _tabs)
        {
            if (tab != null)
                Destroy(tab.gameObject);
        }
        _tabs.Clear();

        if (_currentDecksData == null || _tabPrefab == null) return;

        for (int i = 0; i < _currentDecksData.decks.Count; i++)
        {
            var deckData = _currentDecksData.decks[i];
            var tabGO = Instantiate(_tabPrefab, _tabsParent);
            var tab = tabGO.GetComponent<DeckSlotUI>();

            if (tab != null)
            {
                tabGO.transform.SetSiblingIndex(i);

                tab.Setup(deckData, i, DeckColorsOf(deckData));
                tab.SetSelected(i == _activeDeckIndex);
                tab.OnSlotClicked += OnTabClicked;
                _tabs.Add(tab);
            }
        }

        if (_addTabButton != null && _addTabButton.transform.parent == _tabsParent)
            _addTabButton.transform.SetAsLastSibling();
    }

    private void LoadActiveDeckIntoEditor()
    {
        if (_currentDecksData == null || _cardCollection == null || _currentDecksData.decks.Count == 0) return;

        if (_activeDeckIndex < 0 || _activeDeckIndex >= _currentDecksData.decks.Count)
            _activeDeckIndex = 0;

        var deck = _currentDecksData.decks[_activeDeckIndex];

        if (_deckEditor != null && deck != null)
        {
            var cards = DeckSaveManager.GetDeckCards(_currentChampion, _activeDeckIndex, _cardCollection);
            _deckEditor.ShowDeck(_currentChampion, _activeDeckIndex, deck, cards, _cardCollection);
        }
    }

    /// <summary>
    /// Couleurs d'un deck pour sa tuile : celles choisies à sa création ; pour le deck de base (sans
    /// couleur choisie), celles de ses cartes de départ.
    /// </summary>
    private List<EmotionType> DeckColorsOf(DeckData deck)
    {
        if (deck.HasEmotions) return DeckRules.DeckColors(deck, null);

        IEnumerable<CardData> cards = deck.isDefault && _currentChampion != null && _currentChampion.startingDeck != null
            ? _currentChampion.startingDeck
            : DeckSaveManager.GetCardsFromNames(deck.cardNames, _cardCollection);
        return DeckRules.DeckColors(deck, cards);
    }

    private void UpdateAddTabButtonState()
    {
        if (_addTabButton == null || _currentDecksData == null) return;
        _addTabButton.interactable = _currentDecksData.CanAddCustomDeck();
    }

    // 1er clic : sélectionne le deck ; clic sur le deck déjà sélectionné : l'ouvre (comme « Modifier »)
    private void OnTabClicked(DeckSlotUI slot, int index)
    {
        if (index == _activeDeckIndex)
        {
            OpenSelectedDeck();
            return;
        }

        SelectTab(index);
    }

    private void SelectTab(int index)
    {
        _activeDeckIndex = index;

        for (int i = 0; i < _tabs.Count; i++)
            _tabs[i].SetSelected(i == index);

        DeckSaveManager.SelectDeck(_currentChampion, index);
        LoadActiveDeckIntoEditor();
        NotifyDeckSelected();
        UpdateAddTabButtonState();
        UpdateActionButtons();
    }

    private void OnAddTabClicked()
    {
        if (_createDeckPopup != null)
            _createDeckPopup.Show();
        else
            Debug.LogError("CreateDeckPopup est NULL! Vérifie la référence dans l'Inspector.");
    }

    private void HandleDeckCreated(string deckName, EmotionType emotion1, EmotionType emotion2)
    {
        var newDeck = DeckSaveManager.CreateDeck(_currentChampion, deckName, emotion1, emotion2);
        if (newDeck == null) return;

        _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);
        int newIndex = _currentDecksData.decks.Count - 1;

        RefreshTabs();
        SelectTab(newIndex);
        OnDeckOpened?.Invoke(); // un deck tout juste créé s'ouvre directement pour être rempli
    }

    private void HandleDeckRenamed(int index, string newName)
    {
        DeckSaveManager.RenameDeck(_currentChampion, index, newName);
        _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);

        if (index < _tabs.Count)
            _tabs[index].UpdateDisplay();

        if (index == _activeDeckIndex)
            _deckEditor?.NotifyDeckMetadataChanged();
    }

    private void HandleDeleteConfirmed(int index)
    {
        if (DeckSaveManager.DeleteDeck(_currentChampion, index))
        {
            _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);
            RefreshTabs();
            SelectTab(0); // Revenir au deck de base
        }
    }

    private void HandleDeckDuplicated(int newIndex)
    {
        _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);
        RefreshTabs();
        SelectTab(newIndex);
    }

    /// <summary>Post-autosave : rafraîchit uniquement le compteur de cartes de l'onglet concerné.</summary>
    private void HandleDeckSaved(int index, List<CardData> cards)
    {
        if (index < _tabs.Count)
            _tabs[index].UpdateDisplay();
    }

    /// <summary>
    /// Répercute tout changement du deck en cours d'édition (avant même la persistance
    /// débouncée) vers les abonnés externes (ex: ChampionSelectManager pour le badge
    /// d'avertissement "deck incomplet" sur Lancer le combat).
    /// </summary>
    private void HandleDeckChanged(List<CardData> cards)
    {
        OnDeckSelected?.Invoke(new List<CardData>(cards));
    }

    private void NotifyDeckSelected()
    {
        if (_cardCollection == null || _currentDecksData == null || _currentChampion == null) return;

        var cards = DeckSaveManager.GetDeckCards(_currentChampion, _activeDeckIndex, _cardCollection);
        OnDeckSelected?.Invoke(cards);
    }

    public List<CardData> GetSelectedDeckCards()
    {
        if (_cardCollection == null || _currentDecksData == null || _currentChampion == null)
            return new List<CardData>();

        return DeckSaveManager.GetDeckCards(_currentChampion, _currentDecksData.selectedDeckIndex, _cardCollection);
    }

    #region Actions sur le deck sélectionné (boutons sous la liste)

    private bool HasSelectedDeck =>
        _currentDecksData != null && _activeDeckIndex >= 0 && _activeDeckIndex < _currentDecksData.decks.Count;

    /// <summary>Renommer et supprimer ne concernent pas le deck de base.</summary>
    private void UpdateActionButtons()
    {
        bool isCustom = HasSelectedDeck && !_currentDecksData.decks[_activeDeckIndex].isDefault;

        if (_openDeckButton != null) _openDeckButton.interactable = HasSelectedDeck;
        if (_renameDeckButton != null) _renameDeckButton.interactable = isCustom;
        if (_deleteDeckButton != null) _deleteDeckButton.interactable = isCustom;
    }

    private void OpenSelectedDeck()
    {
        if (HasSelectedDeck)
            OnDeckOpened?.Invoke();
    }

    private void RenameSelectedDeck()
    {
        if (!HasSelectedDeck || _currentDecksData.decks[_activeDeckIndex].isDefault) return;
        _renameDeckPopup?.Show(_activeDeckIndex, _currentDecksData.decks[_activeDeckIndex].deckName);
    }

    private void DeleteSelectedDeck()
    {
        if (!HasSelectedDeck) return;

        var deck = _currentDecksData.decks[_activeDeckIndex];
        if (deck.isDefault)
        {
            GameLog.LogWarning("Impossible de supprimer le deck de base.");
            return;
        }

        _confirmDeletePopup?.Show(_activeDeckIndex, deck.deckName);
    }

    #endregion
}
