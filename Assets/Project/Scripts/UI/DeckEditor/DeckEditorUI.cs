using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Zone bas droite: affiche le contenu du deck
/// Bascule entre mode visualisation et mode édition
/// </summary>
public class DeckEditorUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI _deckNameText;

    [Header("Mode Visualisation")]
    [SerializeField] private GameObject _viewModePanel;
    [SerializeField] private Transform _cardGridParent;      // Grille de cartes visuelles
    [SerializeField] private GameObject _cardGridItemPrefab; // Prefab avec image de carte

    [Header("Mode Édition")]
    [SerializeField] private GameObject _editModePanel;
    [SerializeField] private TMP_InputField _searchInput;
    [SerializeField] private Transform _cardPoolParent;
    [SerializeField] private GameObject _cardPoolItemPrefab;
    [SerializeField] private Transform _editDeckCardsParent;
    [SerializeField] private GameObject _deckCardSlotPrefab;
    [SerializeField] private TextMeshProUGUI _cardCountText;

    [Header("Filtres par Émotion")]
    [SerializeField] private Transform _emotionFiltersParent; // Parent des boutons de filtre
    [SerializeField] private GameObject _emotionFilterButtonPrefab; // Prefab du bouton de filtre
    [SerializeField] private Button _showAllButton; // Bouton "Toutes"

    [Header("Pagination")]
    [SerializeField] private Button _prevPageButton;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private TextMeshProUGUI _pageIndicatorText;
    [SerializeField] private int _cardsPerPage = 8; // Nombre de cartes par page

    [Header("Boutons d'édition")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _resetButton;

    [Header("Configuration")]
    // Total de slots affichés = 2 Signature + 16 Standard (voir DeckData.TOTAL_SLOTS).
    // Les limites par catégorie sont appliquées dans OnPoolCardClicked.
    [SerializeField] private int _deckSize = DeckData.TOTAL_SLOTS;

    private ChampionData _currentChampion;
    private int _currentDeckIndex;
    private DeckData _currentDeckData;
    private CardCollection _cardCollection;

    private List<CardPoolItemUI> _poolItems = new List<CardPoolItemUI>();
    private List<DeckCardSlotUI> _deckSlots = new List<DeckCardSlotUI>();
    private List<CardData> _currentDeckCards = new List<CardData>();
    private List<CardData> _originalDeckCards = new List<CardData>();
    private List<CardData> _filteredCards = new List<CardData>(); // Cartes filtrées pour la pagination

    private List<Button> _emotionFilterButtons = new List<Button>();
    private EmotionType? _activeEmotionFilter = null;
    private int _currentPage = 0;
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

        if (_showAllButton != null)
            _showAllButton.onClick.AddListener(OnShowAllClicked);

        if (_prevPageButton != null)
            _prevPageButton.onClick.AddListener(OnPrevPageClicked);

        if (_nextPageButton != null)
            _nextPageButton.onClick.AddListener(OnNextPageClicked);

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
        SetupEmotionFilter();
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
        ClearEmotionFilterButtons();
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
    }

    private void RefreshCardGrid()
    {
        if (_cardGridParent == null) return;

        // Nettoyer les enfants existants
        foreach (Transform child in _cardGridParent)
        {
            Destroy(child.gameObject);
        }

        if (_cardGridItemPrefab == null)
        {
            Debug.LogWarning("DeckEditorUI: _cardGridItemPrefab n'est pas assigné!");
            return;
        }

        if (_currentDeckCards == null || _currentDeckCards.Count == 0)
        {
            Debug.Log("DeckEditorUI: Aucune carte dans le deck actuel");
            return;
        }

        // Grouper les cartes par nom et compter les quantités
        var cardCounts = new Dictionary<string, (CardData card, int count)>();
        foreach (var card in _currentDeckCards)
        {
            if (card == null) continue;

            if (cardCounts.ContainsKey(card.cardName))
            {
                var existing = cardCounts[card.cardName];
                cardCounts[card.cardName] = (existing.card, existing.count + 1);
            }
            else
            {
                cardCounts[card.cardName] = (card, 1);
            }
        }

        // Trier par coût puis par nom
        var sortedCards = new List<(CardData card, int count)>(cardCounts.Values);
        sortedCards.Sort((a, b) =>
        {
            int costCompare = a.card.costPA.CompareTo(b.card.costPA);
            if (costCompare != 0) return costCompare;
            return a.card.cardName.CompareTo(b.card.cardName);
        });

        // Créer un item pour chaque carte unique avec sa quantité
        foreach (var entry in sortedCards)
        {
            var card = entry.card;
            var count = entry.count;

            var itemGO = Instantiate(_cardGridItemPrefab, _cardGridParent);

            // Priorité: utiliser DeckGridCardUI si présent
            var gridCardUI = itemGO.GetComponent<DeckGridCardUI>();
            if (gridCardUI != null)
            {
                gridCardUI.Setup(card, count);
            }
            else
            {
                // Fallback: configuration manuelle basique
                var image = itemGO.GetComponentInChildren<Image>();
                if (image != null && card.artwork != null)
                    image.sprite = card.artwork;

                var countText = itemGO.GetComponentInChildren<TextMeshProUGUI>();
                if (countText != null)
                    countText.text = $"x{count}";
            }
        }

        Debug.Log($"DeckEditorUI: Affichage de {sortedCards.Count} cartes uniques ({_currentDeckCards.Count} total)");
    }

    #endregion

    #region Mode Édition

    private void SetupEmotionFilter()
    {
        ClearEmotionFilterButtons();

        if (_emotionFiltersParent == null) return;

        // Si le deck n'a pas d'émotions définies, pas de filtres
        if (_currentDeckData == null || !_currentDeckData.HasEmotions)
        {
            if (_showAllButton != null)
                _showAllButton.gameObject.SetActive(false);
            return;
        }

        // Afficher le bouton "Toutes"
        if (_showAllButton != null)
        {
            _showAllButton.gameObject.SetActive(true);
            UpdateShowAllButtonState();
        }

        // Créer un bouton pour chaque émotion du deck
        CreateEmotionFilterButton(_currentDeckData.Emotion1);

        if (_currentDeckData.Emotion2 != EmotionType.None && _currentDeckData.Emotion2 != _currentDeckData.Emotion1)
        {
            CreateEmotionFilterButton(_currentDeckData.Emotion2);
        }

        // Par défaut, afficher toutes les cartes
        _activeEmotionFilter = null;
    }

    private void CreateEmotionFilterButton(EmotionType emotion)
    {
        if (emotion == EmotionType.None) return;
        if (_emotionFilterButtonPrefab == null || _emotionFiltersParent == null) return;

        var buttonGO = Instantiate(_emotionFilterButtonPrefab, _emotionFiltersParent);
        var button = buttonGO.GetComponent<Button>();

        if (button != null)
        {
            // Appliquer la couleur de l'émotion
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = CardVisualHelper.GetEmotionColor(emotion);

            // Stocker l'émotion dans le nom pour la récupérer au clic
            buttonGO.name = emotion.ToString();

            // Ajouter un label si présent
            var label = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = CardVisualHelper.GetEmotionName(emotion);

            // Ajouter le listener
            EmotionType capturedEmotion = emotion; // Capture pour le lambda
            button.onClick.AddListener(() => OnEmotionFilterClicked(capturedEmotion));

            _emotionFilterButtons.Add(button);
        }
    }

    private void ClearEmotionFilterButtons()
    {
        foreach (var button in _emotionFilterButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        _emotionFilterButtons.Clear();
    }

    private void OnEmotionFilterClicked(EmotionType emotion)
    {
        _activeEmotionFilter = emotion;
        _currentPage = 0;
        UpdateEmotionFilterButtonStates();
        ApplyFiltersAndRefreshPool();
    }

    private void OnShowAllClicked()
    {
        _activeEmotionFilter = null;
        _currentPage = 0;
        UpdateEmotionFilterButtonStates();
        ApplyFiltersAndRefreshPool();
    }

    private void UpdateEmotionFilterButtonStates()
    {
        UpdateShowAllButtonState();

        foreach (var button in _emotionFilterButtons)
        {
            if (button == null) continue;

            // Vérifier si ce bouton correspond à l'émotion active
            bool isActive = _activeEmotionFilter.HasValue &&
                           button.gameObject.name == _activeEmotionFilter.Value.ToString();

            // Effet visuel de sélection (scale ou alpha)
            button.transform.localScale = isActive ? Vector3.one * 1.15f : Vector3.one;

            var colors = button.colors;
            colors.normalColor = isActive ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
        }
    }

    private void UpdateShowAllButtonState()
    {
        if (_showAllButton == null) return;

        bool isActive = !_activeEmotionFilter.HasValue;
        _showAllButton.transform.localScale = isActive ? Vector3.one * 1.1f : Vector3.one;
    }

    private void CreatePoolItems()
    {
        ClearPoolItems();

        if (_cardCollection == null || _cardPoolItemPrefab == null || _cardPoolParent == null) return;

        // Initialiser les filtres et afficher la première page
        _currentPage = 0;
        _activeEmotionFilter = null;
        ApplyFiltersAndRefreshPool();
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
        // Permettre de sauvegarder meme si le deck n'est pas complet
        // On peut sauvegarder a tout moment (deck vide inclus pour les customs)
        if (_saveButton != null)
            _saveButton.interactable = true;
    }

    private void OnPoolCardClicked(CardData card)
    {
        if (card == null) return;

        // Limite par catégorie (2 Signature + 16 Standard ; Éveil différé, 0 slot pour l'instant)
        int categoryLimit = card.category switch
        {
            CardCategory.Signature => DeckData.SIGNATURE_SLOTS,
            CardCategory.Standard => DeckData.STANDARD_SLOTS,
            _ => 0,
        };

        int categoryCount = 0;
        foreach (var c in _currentDeckCards)
        {
            if (c != null && c.category == card.category)
                categoryCount++;
        }

        if (categoryCount >= categoryLimit)
        {
            Debug.Log($"Limite atteinte pour la catégorie {card.category} ({categoryLimit}).");
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
        _currentPage = 0;
        ApplyFiltersAndRefreshPool();
    }

    private void ApplyFiltersAndRefreshPool()
    {
        string searchText = _searchInput?.text?.ToLower() ?? "";

        // Filtrer les cartes
        _filteredCards.Clear();

        if (_cardCollection == null) return;

        foreach (var card in _cardCollection.AllCards)
        {
            if (card == null) continue;

            // Cartes Signature : uniquement celles du champion actif (pool partagé sinon)
            if (card.category == CardCategory.Signature && card.signatureOwner != _currentChampion)
                continue;

            // Filtre par émotions du deck
            if (_currentDeckData != null && !_currentDeckData.CardMatchesDeckEmotions(card))
                continue;

            // Filtre par émotion sélectionnée
            if (_activeEmotionFilter.HasValue && card.emotionType != _activeEmotionFilter.Value)
                continue;

            // Filtre par recherche
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matchesSearch = card.cardName.ToLower().Contains(searchText) ||
                                    card.description.ToLower().Contains(searchText);
                if (!matchesSearch) continue;
            }

            _filteredCards.Add(card);
        }

        // Trier par coût puis par nom
        _filteredCards.Sort((a, b) =>
        {
            int costCompare = a.costPA.CompareTo(b.costPA);
            if (costCompare != 0) return costCompare;
            return a.cardName.CompareTo(b.cardName);
        });

        RefreshPoolDisplay();
        UpdatePaginationUI();
    }

    private void RefreshPoolDisplay()
    {
        // Cacher tous les items existants
        foreach (var item in _poolItems)
        {
            if (item != null)
                item.gameObject.SetActive(false);
        }

        // Calculer les indices de la page actuelle
        int startIndex = _currentPage * _cardsPerPage;
        int endIndex = Mathf.Min(startIndex + _cardsPerPage, _filteredCards.Count);

        // Afficher les cartes de la page actuelle
        for (int i = startIndex; i < endIndex; i++)
        {
            var card = _filteredCards[i];
            int poolIndex = i - startIndex;

            // Réutiliser un item existant ou en créer un nouveau
            CardPoolItemUI item;
            if (poolIndex < _poolItems.Count)
            {
                item = _poolItems[poolIndex];
            }
            else
            {
                var itemGO = Instantiate(_cardPoolItemPrefab, _cardPoolParent);
                item = itemGO.GetComponent<CardPoolItemUI>();
                if (item != null)
                {
                    item.OnCardClicked += OnPoolCardClicked;
                    _poolItems.Add(item);
                }
            }

            if (item != null)
            {
                item.Setup(card);
                item.gameObject.SetActive(true);
            }
        }
    }

    #region Pagination

    private void OnPrevPageClicked()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            RefreshPoolDisplay();
            UpdatePaginationUI();
        }
    }

    private void OnNextPageClicked()
    {
        int maxPage = GetMaxPage();
        if (_currentPage < maxPage)
        {
            _currentPage++;
            RefreshPoolDisplay();
            UpdatePaginationUI();
        }
    }

    private int GetMaxPage()
    {
        if (_filteredCards.Count == 0) return 0;
        return (_filteredCards.Count - 1) / _cardsPerPage;
    }

    private void UpdatePaginationUI()
    {
        int maxPage = GetMaxPage();

        if (_prevPageButton != null)
            _prevPageButton.interactable = _currentPage > 0;

        if (_nextPageButton != null)
            _nextPageButton.interactable = _currentPage < maxPage;

        if (_pageIndicatorText != null)
            _pageIndicatorText.text = $"{_currentPage + 1} / {maxPage + 1}";
    }

    #endregion

    private void OnSavePressed()
    {
        // Sauvegarder le deck meme s'il n'est pas complet
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
