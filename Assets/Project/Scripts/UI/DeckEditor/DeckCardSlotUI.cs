using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Element UI representant une carte dans le deck en cours d'edition
/// Style moderne avec artwork et effets visuels
/// </summary>
public class DeckCardSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Structure")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _cardImage;           // Artwork de la carte
    [SerializeField] private Image _emptyStateImage;     // Affiche quand vide

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _costText;

    [Header("Bouton")]
    [SerializeField] private Button _removeButton;

    [Header("Etats (legacy)")]
    [SerializeField] private GameObject _emptyState;
    [SerializeField] private GameObject _filledState;

    [Header("Couleurs")]
    [SerializeField] private Color _emptyColor = new Color(0.15f, 0.15f, 0.2f);
    [SerializeField] private Color _filledColor = new Color(0.2f, 0.2f, 0.25f);
    [SerializeField] private Color _hoverColor = new Color(0.25f, 0.25f, 0.3f);

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _animDuration = 0.1f;

    private CardData _cardData;
    private int _slotIndex;
    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    public System.Action<int> OnRemoveClicked;
    public CardData CardData => _cardData;
    public int SlotIndex => _slotIndex;
    public bool IsEmpty => _cardData == null;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;

        if (_removeButton != null)
            _removeButton.onClick.AddListener(OnRemovePressed);
    }

    /// <summary>
    /// Initialise le slot avec son index
    /// </summary>
    public void SetSlotIndex(int index)
    {
        _slotIndex = index;
        UpdateDisplay();
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

        // Legacy states
        if (_emptyState != null)
            _emptyState.SetActive(!hasCard);

        if (_filledState != null)
            _filledState.SetActive(hasCard);

        // Nouveau systeme
        if (_emptyStateImage != null)
            _emptyStateImage.gameObject.SetActive(!hasCard);

        if (_backgroundImage != null)
            _backgroundImage.color = hasCard ? _filledColor : _emptyColor;

        if (_removeButton != null)
            _removeButton.gameObject.SetActive(hasCard);

        if (hasCard)
        {
            // Artwork
            if (_cardImage != null)
            {
                if (_cardData.artwork != null)
                {
                    _cardImage.sprite = _cardData.artwork;
                    _cardImage.color = Color.white;
                }
                else
                {
                    _cardImage.sprite = null;
                    _cardImage.color = CardVisualHelper.GetEmotionColor(_cardData.emotionType);
                }
                _cardImage.gameObject.SetActive(true);
            }

            // Nom
            if (_cardNameText != null)
                _cardNameText.text = _cardData.cardName;

            // Cout
            if (_costText != null)
                _costText.text = _cardData.costPA.ToString();
        }
        else
        {
            if (_cardImage != null)
                _cardImage.gameObject.SetActive(false);

            if (_cardNameText != null)
                _cardNameText.text = "";

            if (_costText != null)
                _costText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cardData != null)
        {
            OnRemoveClicked?.Invoke(_slotIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardData != null && _backgroundImage != null)
            _backgroundImage.color = _hoverColor;

        if (_cardData != null && _rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale * _hoverScale, _animDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _cardData != null ? _filledColor : _emptyColor;

        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale, _animDuration));
        }
    }

    private void OnRemovePressed()
    {
        OnRemoveClicked?.Invoke(_slotIndex);
    }
}
