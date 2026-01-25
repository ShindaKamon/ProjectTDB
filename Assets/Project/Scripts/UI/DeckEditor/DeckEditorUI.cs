using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Zone bas droite: affiche le contenu du deck (grille + liste)
/// Bascule entre mode visualisation et mode édition
/// </summary>
public class DeckEditorUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI _deckNameText;

    [Header("Mode Visualisation")]
    [SerializeField] private GameObject _viewModePanel;
    [SerializeField] private Transform _cardGridParent;      // Grille de cartes visuelles (gauche)
    [SerializeField] private GameObject _cardGridItemPrefab; // Prefab avec image de carte
    [SerializeField] private Transform _cardListParent;      // Liste textuelle (droite)
    [SerializeField] private GameObject _cardListItemPrefab; // Prefab texte simple

    [Header("Mode Édition")]
    [SerializeField] private GameObject _editModePanel;
    [SerializeField] private TMP_InputField _searchInput;
    [SerializeField] private TMP_Dropdown _familyFilter;
    [SerializeField] private Transform _cardPoolParent;
    [SerializeField] private GameObject _cardPoolItemPrefab;
    [SerializeField] private Transform _editDeckCardsParent;
    [SerializeField] private GameObject _deckCardSlotPrefab;
    [SerializeField] private TextMeshProUGUI _cardCountText;

    [Header("Boutons d'édition")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _resetButton;

    [Header("Configuration")]
    [SerializeField] private int _deckSize = 10;

    private ChampionData _currentChampion;
    private int _currentDeckIndex;
    private DeckData _currentDeckData;
    private CardCollection _cardCollection;

    private List<CardPoolItemUI> _poolItems = new List<CardPoolItemUI>();
    private List<DeckCardSlotUI> _deckSlots = new List<DeckCardSlotUI>();
    private List<CardData> _currentDeckCards = new List<CardData>();
    private List<CardData> _originalDeckCards = new List<CardData>();

    private bool _isEditMode = false;

    public System.Action<int, List<CardData>> OnDeckSaved;

    void Awake()
    {
        if (_saveButton != null)
            _saveButton.onClick.AddListener(OnSavePressed);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(OnCancelPressed);

        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetPressed);

        if (_searchInput != null)
            _searchInput.onValueChanged.AddListener(OnSearchChanged);

        if (_familyFilter != null)
            _familyFilter.onValueChanged.AddListener(OnFilterChanged);

        SetEditMode(false);
    }

    /// <summary>
    /// Affiche le deck en mode visualisation
    /// </summary>
    public void ShowDeck(ChampionData champion, int deckIndex, DeckData deckData, List<CardData> cards, CardCollection collection)
    {
        _currentChampion = champion;
        _currentDeckIndex = deckIndex;
        _currentDeckData = deckData;
        _cardCollection = collection;
        _currentDeckCards = new List<CardData>(cards);

        UpdateHeader();
        RefreshViewMode();
        SetEditMode(false);
    }

    /// <summary>
    /// Ouvre en mode édition
    /// </summary>
    public void Open(ChampionData champion, int deckIndex, DeckData deckData, List<CardData> currentCards, CardCollection collection)
    {
        _currentChampion = champion;
        _currentDeckIndex = deckIndex;
        _currentDeckData = deckData;
        _cardCollection = collection;

        _currentDeckCards = new List<CardData>(currentCards);
        _originalDeckCards = new List<CardData>(currentCards);

        UpdateHeader();
        SetupFamilyFilter();
        CreatePoolItems();
        CreateDeckSlots();
        UpdateDeckDisplay();
        UpdateSaveButtonState();
        SetEditMode(true);
    }

    public void SetEditMode(bool editMode)
    {
        _isEditMode = editMode;

        if (_viewModePanel != null)
            _viewModePanel.SetActive(!editMode);

        if (_editModePanel != null)
            _editModePanel.SetActive(editMode);
    }

    public void Close()
    {
        SetEditMode(false);
        ClearPoolItems();
        ClearDeckSlots();
        RefreshViewMode();
    }

    private void UpdateHeader()
    {
        if (_deckNameText != null && _currentDeckData != null)
            _deckNameText.text = _currentDeckData.deckName;

        if (_cardCountText != null)
            _cardCountText.text = $"{_currentDeckCards.Count} / {_deckSize}";
    }

    #region Mode Visualisation

    private void RefreshViewMode()
    {
        RefreshCardGrid();
        RefreshCardList();
    }

    private void RefreshCardGrid()
    {
        if (_cardGridParent == null) return;

        foreach (Transform child in _cardGridParent)
        {
            Destroy(child.gameObject);
        }

        if (_cardGridItemPrefab == null) return;

        foreach (var card in _currentDeckCards)
        {
            if (card == null) continue;

            var itemGO = Instantiate(_cardGridItemPrefab, _cardGridParent);

            // Essayer de définir l'image de la carte
            var image = itemGO.GetComponent<Image>();
            if (image != null && card.artwork != null)
                image.sprite = card.artwork;

            // Ou si c'est un composant enfant
            var childImage = itemGO.GetComponentInChildren<Image>();
            if (childImage != null && card.artwork != null)
                childImage.sprite = card.artwork;
        }
    }

    private void RefreshCardList()
    {
        if (_cardListParent == null) return;

        foreach (Transform child in _cardListParent)
        {
            Destroy(child.gameObject);
        }

        if (_cardListItemPrefab == null) return;

        foreach (var card in _currentDeckCards)
        {
            if (card == null) continue;

            var itemGO = Instantiate(_cardListItemPrefab, _cardListParent);
            var text = itemGO.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = card.cardName;
        }
    }

    #endregion

    #region Mode Édition

    private void SetupFamilyFilter()
    {
        if (_familyFilter == null) return;

        _familyFilter.ClearOptions();
        var options = new List<string> { "Toutes" };

        foreach (CardFamilyType family in System.Enum.GetValues(typeof(CardFamilyType)))
        {
            if (family != CardFamilyType.None)
                options.Add(family.ToString());
        }

        _familyFilter.AddOptions(options);
        _familyFilter.value = 0;
    }

    private void CreatePoolItems()
    {
        ClearPoolItems();

        if (_cardCollection == null || _cardPoolItemPrefab == null || _cardPoolParent == null) return;

        foreach (var card in _cardCollection.AllCards)
        {
            if (card == null) continue;

            var itemGO = Instantiate(_cardPoolItemPrefab, _cardPoolParent);
            var item = itemGO.GetComponent<CardPoolItemUI>();

            if (item != null)
            {
                item.Setup(card);
                item.OnCardClicked += OnPoolCardClicked;
                _poolItems.Add(item);
            }
        }
    }

    private void ClearPoolItems()
    {
        foreach (var item in _poolItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _poolItems.Clear();
    }

    private void CreateDeckSlots()
    {
        ClearDeckSlots();

        if (_deckCardSlotPrefab == null || _editDeckCardsParent == null) return;

        for (int i = 0; i < _deckSize; i++)
        {
            var slotGO = Instantiate(_deckCardSlotPrefab, _editDeckCardsParent);
            var slot = slotGO.GetComponent<DeckCardSlotUI>();

            if (slot != null)
            {
                slot.SetSlotIndex(i);
                slot.OnRemoveClicked += OnDeckSlotRemoveClicked;
                _deckSlots.Add(slot);
            }
        }
    }

    private void ClearDeckSlots()
    {
        foreach (var slot in _deckSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _deckSlots.Clear();
    }

    private void UpdateDeckDisplay()
    {
        for (int i = 0; i < _deckSlots.Count; i++)
        {
            if (i < _currentDeckCards.Count)
                _deckSlots[i].SetCard(_currentDeckCards[i]);
            else
                _deckSlots[i].Clear();
        }

        UpdateHeader();
        UpdateSaveButtonState();
    }

    private void UpdateSaveButtonState()
    {
        if (_saveButton != null)
            _saveButton.interactable = _currentDeckCards.Count == _deckSize;
    }

    private void OnPoolCardClicked(CardData card)
    {
        if (_currentDeckCards.Count >= _deckSize)
        {
            Debug.Log("Le deck est plein!");
            return;
        }

        _currentDeckCards.Add(card);
        UpdateDeckDisplay();
    }

    private void OnDeckSlotRemoveClicked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _currentDeckCards.Count)
        {
            _currentDeckCards.RemoveAt(slotIndex);
            UpdateDeckDisplay();
        }
    }

    private void OnSearchChanged(string searchText)
    {
        FilterPoolItems();
    }

    private void OnFilterChanged(int filterIndex)
    {
        FilterPoolItems();
    }

    private void FilterPoolItems()
    {
        string searchText = _searchInput?.text?.ToLower() ?? "";
        int familyIndex = _familyFilter?.value ?? 0;

        CardFamilyType? familyFilter = null;
        if (familyIndex > 0)
        {
            var families = System.Enum.GetValues(typeof(CardFamilyType));
            int actualIndex = 0;
            foreach (CardFamilyType f in families)
            {
                if (f == CardFamilyType.None) continue;
                actualIndex++;
                if (actualIndex == familyIndex)
                {
                    familyFilter = f;
                    break;
                }
            }
        }

        foreach (var item in _poolItems)
        {
            if (item == null || item.CardData == null) continue;

            bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                 item.CardData.cardName.ToLower().Contains(searchText) ||
                                 item.CardData.description.ToLower().Contains(searchText);

            bool matchesFamily = !familyFilter.HasValue ||
                                 item.CardData.familyType == familyFilter.Value;

            item.gameObject.SetActive(matchesSearch && matchesFamily);
        }
    }

    private void OnSavePressed()
    {
        if (_currentDeckCards.Count != _deckSize)
            return;

        OnDeckSaved?.Invoke(_currentDeckIndex, new List<CardData>(_currentDeckCards));
        Close();
    }

    private void OnCancelPressed()
    {
        _currentDeckCards = new List<CardData>(_originalDeckCards);
        Close();
    }

    private void OnResetPressed()
    {
        _currentDeckCards = new List<CardData>(_originalDeckCards);
        UpdateDeckDisplay();
    }

    #endregion
}
