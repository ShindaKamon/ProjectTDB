using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Zone Pool + Deck + Courbe PA de l'écran unifié de gestion des decks (façon MTG Arena).
/// Toujours en édition : plus de bascule ViewMode/EditMode, plus de boutons
/// Enregistrer/Annuler — chaque modification déclenche un autosave debouncé
/// (DeckSaveManager.UpdateDeckCards) et une notification immédiate via OnDeckChanged.
/// Le deck de base (isDefault) est en lecture seule : pool grisé/non cliquable, retrait de
/// carte désactivé, seul le bouton "Dupliquer en deck personnalisé" reste actif.
/// </summary>
public class DeckEditorUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI _deckNameText;
    [SerializeField] private TextMeshProUGUI _deckCompositionText; // "Signature 2/2 · Standard 14/16"

    [Header("Zone Pool")]
    [SerializeField] private CanvasGroup _poolInteractionGroup; // grisé/non cliquable si deck de base
    [SerializeField] private Transform _cardPoolParent;
    [SerializeField] private GameObject _cardPoolItemPrefab;
    [SerializeField] private PoolFilterBarUI _filterBar;          // recherche, filtres, tri
    [SerializeField] private TextMeshProUGUI _emptyPoolText;      // "Aucune carte ne correspond à ces filtres."

    [Header("Pagination (optionnelle : sans boutons, tout le pool défile)")]
    [SerializeField] private Button _prevPageButton;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private TextMeshProUGUI _pageIndicatorText;
    [SerializeField] private int _cardsPerPage = 8;

    [Header("Zone Deck (liste groupée triée par coût)")]
    [SerializeField] private Transform _deckGridParent;
    [SerializeField] private GameObject _deckGridItemPrefab; // DeckGridCardUI

    [Header("Courbe de coût PA")]
    [SerializeField] private PACurveUI _paCurve;

    [Header("Boutons")]
    [SerializeField] private Button _resetButton;     // caché en lecture seule (deck de base)
    [SerializeField] private Button _duplicateButton; // toujours visible

    [Header("Configuration")]
    // Total de slots affichés = 2 Signature + 16 Standard (voir DeckData.TOTAL_SLOTS).
    // Les limites par catégorie sont appliquées dans OnPoolCardClicked.
    [SerializeField] private int _deckSize = DeckData.TOTAL_SLOTS;
    [SerializeField] private float _autosaveDebounceSeconds = 0.4f;

    private ChampionData _currentChampion;
    private int _currentDeckIndex;
    private DeckData _currentDeckData;
    private CardCollection _cardCollection;

    private List<CardPoolItemUI> _poolItems = new List<CardPoolItemUI>();
    private List<DeckGridCardUI> _deckGridItems = new List<DeckGridCardUI>();
    private List<CardData> _currentDeckCards = new List<CardData>();
    private List<CardData> _filteredCards = new List<CardData>(); // Cartes filtrées pour la pagination

    private readonly CardPoolQuery _poolQuery = new CardPoolQuery(); // filtres + tri du pool
    private int _currentPage = 0;
    private Coroutine _autosaveRoutine;

    /// <summary>Émis après persistance effective (autosave debouncé) du deck.</summary>
    public System.Action<int, List<CardData>> OnDeckSaved;

    /// <summary>Émis à chaque changement du contenu du deck (avant la persistance débouncée).</summary>
    public System.Action<List<CardData>> OnDeckChanged;

    /// <summary>Émis quand un deck est dupliqué en deck personnalisé (index du nouveau deck).</summary>
    public System.Action<int> OnDeckDuplicated;

    void Awake()
    {
        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetPressed);

        if (_duplicateButton != null)
            _duplicateButton.onClick.AddListener(OnDuplicatePressed);

        if (_filterBar != null)
            _filterBar.Changed += OnFiltersChanged;

        if (_prevPageButton != null)
            _prevPageButton.onClick.AddListener(OnPrevPageClicked);

        if (_nextPageButton != null)
            _nextPageButton.onClick.AddListener(OnNextPageClicked);
    }

    void OnDestroy()
    {
        FlushPendingAutosave();
    }

    /// <summary>
    /// Affiche et rend éditable le deck spécifié (base ou custom). Point d'entrée unique,
    /// appelé à chaque changement d'onglet de loadout par LoadoutTabsUI.
    /// </summary>
    public void ShowDeck(ChampionData champion, int deckIndex, DeckData deckData, List<CardData> cards, CardCollection collection)
    {
        FlushPendingAutosave();

        _currentChampion = champion;
        _currentDeckIndex = deckIndex;
        _currentDeckData = deckData;
        _cardCollection = collection;
        _currentDeckCards = new List<CardData>(cards);

        if (_filterBar != null)
            _filterBar.Bind(_poolQuery, deckData);
        else
            _poolQuery.ResetFilters();

        CreatePoolItems();
        UpdateDeckDisplay();
        ApplyReadOnlyState();
    }

    /// <summary>Rafraîchit uniquement le nom affiché (après un renommage externe).</summary>
    public void NotifyDeckMetadataChanged() => UpdateHeader();

    private void UpdateHeader()
    {
        if (_deckNameText != null && _currentDeckData != null)
            _deckNameText.text = _currentDeckData.deckName;

        if (_deckCompositionText != null)
        {
            int signatureCount = 0;
            int standardCount = 0;
            foreach (var card in _currentDeckCards)
            {
                if (card == null) continue;
                if (card.category == CardCategory.Signature) signatureCount++;
                else if (card.category == CardCategory.Standard) standardCount++;
            }

            _deckCompositionText.text =
                $"Signature {signatureCount}/{DeckData.SIGNATURE_SLOTS} · Standard {standardCount}/{DeckData.STANDARD_SLOTS}";
        }
    }

    private void ApplyReadOnlyState()
    {
        bool isReadOnly = _currentDeckData != null && _currentDeckData.isDefault;

        if (_poolInteractionGroup != null)
        {
            _poolInteractionGroup.interactable = !isReadOnly;
            _poolInteractionGroup.blocksRaycasts = !isReadOnly;
            _poolInteractionGroup.alpha = isReadOnly ? 0.5f : 1f;
        }

        // Le deck de base ne peut pas être réinitialisé (rien à modifier) : seule l'action
        // "Dupliquer en deck personnalisé" reste proposée.
        if (_resetButton != null)
            _resetButton.gameObject.SetActive(!isReadOnly);

        foreach (var item in _deckGridItems)
        {
            if (item != null)
                item.SetInteractable(!isReadOnly);
        }
    }

    #region Zone Deck (liste groupée)

    private void RefreshDeckGrid()
    {
        if (_deckGridParent == null) return;

        foreach (Transform child in _deckGridParent)
            Destroy(child.gameObject);
        _deckGridItems.Clear();

        if (_deckGridItemPrefab == null)
        {
            GameLog.LogWarning("DeckEditorUI: _deckGridItemPrefab n'est pas assigné!");
            return;
        }

        // Grouper les cartes par nom et compter les quantités
        var cardCounts = new Dictionary<string, (CardData card, int count)>();
        foreach (var card in _currentDeckCards)
        {
            if (card == null) continue;

            if (cardCounts.TryGetValue(card.cardName, out var existing))
                cardCounts[card.cardName] = (existing.card, existing.count + 1);
            else
                cardCounts[card.cardName] = (card, 1);
        }

        // Trier par coût puis par nom
        var sortedCards = new List<(CardData card, int count)>(cardCounts.Values);
        sortedCards.Sort((a, b) =>
        {
            int costCompare = a.card.costPA.CompareTo(b.card.costPA);
            if (costCompare != 0) return costCompare;
            return a.card.cardName.CompareTo(b.card.cardName);
        });

        bool isReadOnly = _currentDeckData != null && _currentDeckData.isDefault;

        foreach (var entry in sortedCards)
        {
            var itemGO = Instantiate(_deckGridItemPrefab, _deckGridParent);
            var gridCardUI = itemGO.GetComponent<DeckGridCardUI>();

            if (gridCardUI != null)
            {
                gridCardUI.Setup(entry.card, entry.count);
                gridCardUI.SetInteractable(!isReadOnly);
                gridCardUI.OnCardClicked += OnDeckCardGroupClicked;
                _deckGridItems.Add(gridCardUI);
            }
        }

        _paCurve?.SetCards(_currentDeckCards);
    }

    private void OnDeckCardGroupClicked(CardData card)
    {
        if (card == null) return;
        if (_currentDeckData != null && _currentDeckData.isDefault) return; // lecture seule

        int indexToRemove = _currentDeckCards.FindLastIndex(c => c != null && c.cardName == card.cardName);
        if (indexToRemove < 0) return;

        _currentDeckCards.RemoveAt(indexToRemove);
        NotifyDeckContentChanged();
    }

    #endregion

    #region Pool

    private void CreatePoolItems()
    {
        ClearPoolItems();

        if (_cardCollection == null || _cardPoolItemPrefab == null || _cardPoolParent == null) return;

        _currentPage = 0;
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

    private void UpdateDeckDisplay()
    {
        RefreshDeckGrid();
        UpdateHeader();
    }

    /// <summary>
    /// Point d'entrée commun pour toute modification du contenu du deck (ajout/retrait) :
    /// rafraîchit l'affichage immédiatement, notifie les abonnés (OnDeckChanged) puis planifie
    /// l'autosave débouncé. Pas d'appel pour un deck en lecture seule (interdit en amont).
    /// </summary>
    private void NotifyDeckContentChanged()
    {
        UpdateDeckDisplay();
        OnDeckChanged?.Invoke(new List<CardData>(_currentDeckCards));
        ScheduleAutosave();
    }

    private void OnPoolCardClicked(CardData card)
    {
        if (card == null) return;
        if (_currentDeckData != null && _currentDeckData.isDefault) return; // deck de base : lecture seule

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
            GameLog.Log($"Limite atteinte pour la catégorie {card.category} ({categoryLimit}).");
            return;
        }

        _currentDeckCards.Add(card);
        NotifyDeckContentChanged();
    }

    private void OnFiltersChanged()
    {
        _currentPage = 0;
        ApplyFiltersAndRefreshPool();
    }

    private void ApplyFiltersAndRefreshPool()
    {
        _filteredCards = _poolQuery.Apply(_cardCollection?.AllCards, _currentChampion, _currentDeckData);

        RefreshPoolDisplay();
        UpdatePaginationUI();
    }

    private void RefreshPoolDisplay()
    {
        foreach (var item in _poolItems)
        {
            if (item != null)
                item.gameObject.SetActive(false);
        }

        int pageSize = GetPageSize();
        int startIndex = _currentPage * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, _filteredCards.Count);

        if (_emptyPoolText != null)
            _emptyPoolText.gameObject.SetActive(_filteredCards.Count == 0);

        for (int i = startIndex; i < endIndex; i++)
        {
            var card = _filteredCards[i];
            int poolIndex = i - startIndex;

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

    // Sans boutons de page câblés, le pool entier est affiché dans la zone de défilement
    // (sinon seules les _cardsPerPage premières cartes seraient accessibles).
    private int GetPageSize() =>
        _prevPageButton != null && _nextPageButton != null ? Mathf.Max(1, _cardsPerPage) : int.MaxValue;

    private int GetMaxPage()
    {
        if (_filteredCards.Count == 0) return 0;
        return (_filteredCards.Count - 1) / GetPageSize();
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

    #endregion

    #region Autosave & actions

    /// <summary>
    /// Planifie l'écriture différée (debounce) du deck courant : évite d'écrire sur disque à
    /// chaque clic si plusieurs modifications rapprochées surviennent. Aucun effet pour le
    /// deck de base (jamais persisté : SyncBaseDeck reste seul maître de son contenu).
    /// </summary>
    private void ScheduleAutosave()
    {
        if (_currentDeckData != null && _currentDeckData.isDefault) return;

        if (_autosaveRoutine != null)
            StopCoroutine(_autosaveRoutine);

        _autosaveRoutine = StartCoroutine(AutosaveAfterDelay());
    }

    private IEnumerator AutosaveAfterDelay()
    {
        yield return new WaitForSeconds(_autosaveDebounceSeconds);
        PersistDeckCards();
        _autosaveRoutine = null;
    }

    /// <summary>Écrit immédiatement toute autosave en attente (changement d'onglet, fermeture).</summary>
    private void FlushPendingAutosave()
    {
        if (_autosaveRoutine == null) return;

        StopCoroutine(_autosaveRoutine);
        _autosaveRoutine = null;
        PersistDeckCards();
    }

    private void PersistDeckCards()
    {
        if (_currentChampion == null || _currentDeckData == null || _currentDeckData.isDefault) return;

        var cardNames = new List<string>();
        foreach (var card in _currentDeckCards)
        {
            if (card != null)
                cardNames.Add(card.cardName);
        }

        DeckSaveManager.UpdateDeckCards(_currentChampion, _currentDeckIndex, cardNames);
        OnDeckSaved?.Invoke(_currentDeckIndex, new List<CardData>(_currentDeckCards));
    }

    /// <summary>
    /// Revert vers le dernier état sauvegardé (relit depuis DeckSaveManager), pas vers une
    /// copie de session : toute autosave en attente non encore écrite est simplement annulée.
    /// </summary>
    private void OnResetPressed()
    {
        if (_currentChampion == null || _currentDeckData == null) return;

        if (_autosaveRoutine != null)
        {
            StopCoroutine(_autosaveRoutine);
            _autosaveRoutine = null;
        }

        _currentDeckCards = DeckSaveManager.GetDeckCards(_currentChampion, _currentDeckIndex, _cardCollection);
        UpdateDeckDisplay();
        OnDeckChanged?.Invoke(new List<CardData>(_currentDeckCards));
    }

    private void OnDuplicatePressed()
    {
        if (_currentChampion == null) return;

        var newDeck = DeckSaveManager.DuplicateDeck(_currentChampion, _currentDeckIndex);
        if (newDeck == null)
        {
            GameLog.LogWarning("Impossible de dupliquer : nombre maximum de decks personnalisés atteint.");
            return;
        }

        var championDecks = DeckSaveManager.GetDecksForChampion(_currentChampion);
        int newIndex = championDecks.decks.Count - 1;
        OnDeckDuplicated?.Invoke(newIndex);
    }

    #endregion
}
