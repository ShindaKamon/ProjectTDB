using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère la zone haut droite: affichage des decks et boutons d'action
/// </summary>
public class DeckListUI : MonoBehaviour
{
    [Header("Zone Decks")]
    [SerializeField] private Transform _deckSlotsParent;
    [SerializeField] private GameObject _deckSlotPrefab;
    [SerializeField] private Button _addDeckButton;

    [Header("Boutons d'action")]
    [SerializeField] private Button _modifyButton;
    [SerializeField] private Button _deleteButton;

    [Header("Popups")]
    [SerializeField] private CreateDeckPopup _createDeckPopup;
    [SerializeField] private RenameDeckPopup _renameDeckPopup;
    [SerializeField] private ConfirmDeletePopup _confirmDeletePopup;

    [Header("Zone Bas Droite")]
    [SerializeField] private DeckEditorUI _deckEditor;

    [Header("Configuration")]
    [SerializeField] private CardCollection _cardCollection;

    private ChampionData _currentChampion;
    private ChampionDecksData _currentDecksData;
    private List<DeckSlotUI> _deckSlots = new List<DeckSlotUI>();
    private int _clickedDeckIndex = -1; // Index du deck cliqué (pour les actions)

    public System.Action<List<CardData>> OnDeckSelected;

    void Awake()
    {
        if (_addDeckButton != null)
            _addDeckButton.onClick.AddListener(OnAddDeckClicked);

        if (_modifyButton != null)
            _modifyButton.onClick.AddListener(OnModifyClicked);

        if (_deleteButton != null)
            _deleteButton.onClick.AddListener(OnDeleteClicked);

        SetupPopupCallbacks();
        ConfigureLayoutGroup();
        UpdateActionButtons();
    }

    /// <summary>
    /// Configure le layout group du parent pour un affichage correct des slots
    /// </summary>
    private void ConfigureLayoutGroup()
    {
        if (_deckSlotsParent == null) return;

        // Essayer HorizontalLayoutGroup d'abord
        var horizontalLayout = _deckSlotsParent.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
        {
            // childControlWidth/Height = true pour que le LayoutElement soit respecté
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;
            horizontalLayout.spacing = 15f;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        }
        else
        {
            // Sinon GridLayoutGroup
            var gridLayout = _deckSlotsParent.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.cellSize = new Vector2(120f, 80f);
                gridLayout.spacing = new Vector2(15f, 15f);
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
            else
            {
                // Si aucun layout group, ajouter un HorizontalLayoutGroup
                horizontalLayout = _deckSlotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayout.childControlWidth = true;
                horizontalLayout.childControlHeight = true;
                horizontalLayout.childForceExpandWidth = false;
                horizontalLayout.childForceExpandHeight = false;
                horizontalLayout.spacing = 15f;
                horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            }
        }

        // Configurer le bouton "+" pour qu'il ait la bonne taille dans le layout
        ConfigureAddButton();
    }

    /// <summary>
    /// Configure le bouton "+" pour qu'il s'aligne correctement avec les slots
    /// </summary>
    private void ConfigureAddButton()
    {
        if (_addDeckButton == null) return;

        // Configurer le LayoutElement du bouton "+" (requis pour childControlWidth/Height)
        var layoutElement = _addDeckButton.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = _addDeckButton.gameObject.AddComponent<LayoutElement>();

        // Taille carrée pour le bouton "+"
        layoutElement.minWidth = 80f;
        layoutElement.minHeight = 80f;
        layoutElement.preferredWidth = 80f;
        layoutElement.preferredHeight = 80f;

        // S'assurer que le bouton est toujours à la fin du layout
        _addDeckButton.transform.SetAsLastSibling();
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
            _deckEditor.OnDeckSaved += HandleDeckSaved;
    }

    /// <summary>
    /// Affiche les decks du champion spécifié
    /// </summary>
    public void ShowDecksForChampion(ChampionData champion)
    {
        _currentChampion = champion;
        _currentDecksData = DeckSaveManager.GetDecksForChampion(champion);
        _clickedDeckIndex = _currentDecksData.selectedDeckIndex;

        RefreshDeckSlots();
        UpdateSelectedDeckPreview();
        UpdateAddButtonState();
        UpdateActionButtons();
    }

    private void RefreshDeckSlots()
    {
        // Nettoyer les anciens slots
        foreach (var slot in _deckSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _deckSlots.Clear();

        if (_currentDecksData == null || _deckSlotPrefab == null) return;

        // Créer les nouveaux slots
        for (int i = 0; i < _currentDecksData.decks.Count; i++)
        {
            var deckData = _currentDecksData.decks[i];
            var slotGO = Instantiate(_deckSlotPrefab, _deckSlotsParent);
            var slot = slotGO.GetComponent<DeckSlotUI>();

            if (slot != null)
            {
                // Placer le slot à l'index i
                slotGO.transform.SetSiblingIndex(i);

                slot.Setup(deckData, i);
                slot.SetSelected(i == _clickedDeckIndex);
                slot.OnSlotClicked += OnDeckSlotClicked;
                _deckSlots.Add(slot);
            }
        }

        // S'assurer que le bouton "+" est toujours à la fin
        if (_addDeckButton != null && _addDeckButton.transform.parent == _deckSlotsParent)
        {
            _addDeckButton.transform.SetAsLastSibling();
        }
    }

    private void UpdateSelectedDeckPreview()
    {
        if (_currentDecksData == null || _cardCollection == null) return;

        // Utiliser le deck cliqué pour l'affichage
        if (_clickedDeckIndex < 0 || _clickedDeckIndex >= _currentDecksData.decks.Count)
            _clickedDeckIndex = 0;

        var deckToShow = _currentDecksData.decks[_clickedDeckIndex];

        if (_deckEditor != null && deckToShow != null)
        {
            // Utilise directement le startingDeck pour le deck de base
            var cards = DeckSaveManager.GetDeckCards(_currentChampion, _clickedDeckIndex, _cardCollection);
            _deckEditor.ShowDeck(_currentChampion, _clickedDeckIndex, deckToShow, cards, _cardCollection);
        }
    }

    private void UpdateAddButtonState()
    {
        if (_addDeckButton == null || _currentDecksData == null) return;
        _addDeckButton.interactable = _currentDecksData.CanAddCustomDeck();
    }

    private void UpdateActionButtons()
    {
        bool hasDeckSelected = _clickedDeckIndex >= 0 && _currentDecksData != null &&
                               _clickedDeckIndex < _currentDecksData.decks.Count;

        if (_modifyButton != null)
            _modifyButton.interactable = hasDeckSelected;

        // Le bouton supprimer est désactivé pour le deck de base (index 0 ou isDefault)
        if (_deleteButton != null)
        {
            bool canDelete = hasDeckSelected && !_currentDecksData.decks[_clickedDeckIndex].isDefault;
            _deleteButton.interactable = canDelete;
        }
    }

    private void OnDeckSlotClicked(DeckSlotUI slot, int index)
    {
        _clickedDeckIndex = index;

        // Mettre à jour la sélection visuelle
        for (int i = 0; i < _deckSlots.Count; i++)
        {
            _deckSlots[i].SetSelected(i == index);
        }

        // Persiste le deck choisi (utilisé en combat), pas seulement l'aperçu
        DeckSaveManager.SelectDeck(_currentChampion, index);
        NotifyDeckSelected();

        UpdateSelectedDeckPreview();
        UpdateActionButtons();
    }

    private void OnAddDeckClicked()
    {
        GameLog.Log("OnAddDeckClicked appelé");

        if (_createDeckPopup != null)
        {
            GameLog.Log("CreateDeckPopup trouvé, appel de Show()");
            _createDeckPopup.Show();
        }
        else
        {
            Debug.LogError("CreateDeckPopup est NULL! Vérifie la référence dans l'Inspector.");
        }
    }

    private void OnModifyClicked()
    {
        if (_clickedDeckIndex < 0 || _currentDecksData == null) return;

        if (_deckEditor != null && _cardCollection != null)
        {
            var deck = _currentDecksData.decks[_clickedDeckIndex];
            var cards = DeckSaveManager.GetDeckCards(_currentChampion, _clickedDeckIndex, _cardCollection);
            _deckEditor.Open(_currentChampion, _clickedDeckIndex, deck, cards, _cardCollection);
        }
    }

    private void OnDeleteClicked()
    {
        if (_clickedDeckIndex < 0 || _currentDecksData == null) return;

        var deck = _currentDecksData.decks[_clickedDeckIndex];
        if (deck.isDefault)
        {
            GameLog.LogWarning("Impossible de supprimer le deck de base.");
            return;
        }

        if (_confirmDeletePopup != null)
            _confirmDeletePopup.Show(_clickedDeckIndex, deck.deckName);
    }

    private void HandleDeckCreated(string deckName, EmotionType emotion1, EmotionType emotion2)
    {
        var newDeck = DeckSaveManager.CreateDeck(_currentChampion, deckName, emotion1, emotion2);
        if (newDeck != null)
        {
            _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);
            _clickedDeckIndex = _currentDecksData.decks.Count - 1;
            RefreshDeckSlots();
            UpdateAddButtonState();
            UpdateActionButtons();

            // Ouvrir l'éditeur pour le nouveau deck
            OnModifyClicked();
        }
    }

    private void HandleDeckRenamed(int index, string newName)
    {
        DeckSaveManager.RenameDeck(_currentChampion, index, newName);
        _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);

        if (index < _deckSlots.Count)
            _deckSlots[index].UpdateDisplay();

        UpdateSelectedDeckPreview();
    }

    private void HandleDeleteConfirmed(int index)
    {
        if (DeckSaveManager.DeleteDeck(_currentChampion, index))
        {
            _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);
            _clickedDeckIndex = 0; // Revenir au deck de base
            RefreshDeckSlots();
            UpdateSelectedDeckPreview();
            UpdateAddButtonState();
            UpdateActionButtons();
            NotifyDeckSelected();
        }
    }

    private void HandleDeckSaved(int index, List<CardData> cards)
    {
        var cardNames = new List<string>();
        foreach (var card in cards)
        {
            if (card != null)
                cardNames.Add(card.cardName);
        }

        DeckSaveManager.UpdateDeckCards(_currentChampion, index, cardNames);
        _currentDecksData = DeckSaveManager.GetDecksForChampion(_currentChampion);

        if (index < _deckSlots.Count)
            _deckSlots[index].UpdateDisplay();

        UpdateSelectedDeckPreview();

        if (index == _currentDecksData.selectedDeckIndex)
            NotifyDeckSelected();
    }

    private void NotifyDeckSelected()
    {
        if (_cardCollection == null || _currentDecksData == null || _currentChampion == null) return;

        var cards = DeckSaveManager.GetDeckCards(_currentChampion, _currentDecksData.selectedDeckIndex, _cardCollection);
        OnDeckSelected?.Invoke(cards);
    }

    public List<CardData> GetSelectedDeckCards()
    {
        if (_cardCollection == null || _currentDecksData == null || _currentChampion == null)
            return new List<CardData>();

        return DeckSaveManager.GetDeckCards(_currentChampion, _currentDecksData.selectedDeckIndex, _cardCollection);
    }
}
