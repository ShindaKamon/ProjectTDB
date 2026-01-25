using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Élément UI représentant une carte dans le deck en cours d'édition
/// </summary>
public class DeckCardSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Références UI")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private Button _removeButton;

    [Header("État vide")]
    [SerializeField] private GameObject _emptyState;
    [SerializeField] private GameObject _filledState;

    private CardData _cardData;
    private int _slotIndex;

    public System.Action<int> OnRemoveClicked;
    public CardData CardData => _cardData;
    public int SlotIndex => _slotIndex;
    public bool IsEmpty => _cardData == null;

    void Awake()
    {
        if (_removeButton != null)
            _removeButton.onClick.AddListener(OnRemovePressed);
    }

    /// <summary>
    /// Initialise le slot avec son index
    /// </summary>
    public void SetSlotIndex(int index)
    {
        _slotIndex = index;
    }

    /// <summary>
    /// Remplit le slot avec une carte
    /// </summary>
    public void SetCard(CardData card)
    {
        _cardData = card;
        UpdateDisplay();
    }

    /// <summary>
    /// Vide le slot
    /// </summary>
    public void Clear()
    {
        _cardData = null;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        bool hasCard = _cardData != null;

        if (_emptyState != null)
            _emptyState.SetActive(!hasCard);

        if (_filledState != null)
            _filledState.SetActive(hasCard);

        if (hasCard)
        {
            if (_cardNameText != null)
                _cardNameText.text = _cardData.cardName;

            if (_cardCostText != null)
                _cardCostText.text = $"{_cardData.costPA} PA";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Clic sur le slot = retirer la carte si présente
        if (_cardData != null)
        {
            OnRemoveClicked?.Invoke(_slotIndex);
        }
    }

    private void OnRemovePressed()
    {
        OnRemoveClicked?.Invoke(_slotIndex);
    }
}
