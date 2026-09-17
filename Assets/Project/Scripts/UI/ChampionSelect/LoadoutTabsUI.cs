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

    [Header("Menu contextuel (onglet actif)")]
    [SerializeField] private GameObject _contextMenu;
    [SerializeField] private Button _contextMenuRenameButton;
    [SerializeField] private Button _contextMenuDeleteButton;

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

    void Awake()
    {
        if (_addTabButton != null)
            _addTabButton.onClick.AddListener(OnAddTabClicked);

        if (_contextMenuRenameButton != null)
            _contextMenuRenameButton.onClick.AddListener(OnContextRenameClicked);

        if (_contextMenuDeleteButton != null)
            _contextMenuDeleteButton.onClick.AddListener(OnContextDeleteClicked);

        SetupPopupCallbacks();
        ConfigureLayoutGroup();
        HideContextMenu();
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
        horizontalLayout.spacing = 8f;
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;

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

        layoutElement.minWidth = 44f;
        layoutElement.minHeight = 44f;
        layoutElement.preferredWidth = 44f;
        layoutElement.preferredHeight = 44f;

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
        HideContextMenu();
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

                tab.Setup(deckData, i);
                tab.SetSelected(i == _activeDeckIndex);
                tab.OnSlotClicked += OnTabClicked;
                tab.OnContextMenuClicked += OnTabContextMenuClicked;
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

    private void UpdateAddTabButtonState()
    {
        if (_addTabButton == null || _currentDecksData == null) return;
        _addTabButton.interactable = _currentDecksData.CanAddCustomDeck();
    }

    private void OnTabClicked(DeckSlotUI slot, int index)
    {
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
        HideContextMenu();
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

    #region Menu contextuel (onglet actif)

    private void OnTabContextMenuClicked(DeckSlotUI slot, int index)
    {
        _activeDeckIndex = index;
        ShowContextMenu(slot);
    }

    private void ShowContextMenu(DeckSlotUI slot)
    {
        if (_contextMenu == null) return;

        _contextMenu.SetActive(true);

        var menuRT = _contextMenu.GetComponent<RectTransform>();
        var slotRT = slot != null ? slot.GetComponent<RectTransform>() : null;
        if (menuRT != null && slotRT != null)
        {
            Vector3 pos = slotRT.position;
            pos.y -= slotRT.rect.height * slotRT.lossyScale.y;
            menuRT.position = pos;
        }

        bool canDelete = _currentDecksData != null && _activeDeckIndex >= 0 &&
                         _activeDeckIndex < _currentDecksData.decks.Count &&
                         !_currentDecksData.decks[_activeDeckIndex].isDefault;

        if (_contextMenuDeleteButton != null)
            _contextMenuDeleteButton.interactable = canDelete;
    }

    private void HideContextMenu()
    {
        if (_contextMenu != null)
            _contextMenu.SetActive(false);
    }

    private void OnContextRenameClicked()
    {
        HideContextMenu();

        if (_currentDecksData == null || _activeDeckIndex < 0 || _activeDeckIndex >= _currentDecksData.decks.Count)
            return;

        _renameDeckPopup?.Show(_activeDeckIndex, _currentDecksData.decks[_activeDeckIndex].deckName);
    }

    private void OnContextDeleteClicked()
    {
        HideContextMenu();

        if (_currentDecksData == null || _activeDeckIndex < 0 || _activeDeckIndex >= _currentDecksData.decks.Count)
            return;

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
