using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Composant UI pour un slot de deck visuel moderne
/// Affiche une carte colorée avec effets visuels
/// </summary>
public class DeckSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Structure")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundImage2;    // Deuxième couleur (gradient)
    [SerializeField] private Image _selectionBorder;
    [SerializeField] private Image _defaultIcon;         // Icone pour le deck de base

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _deckNameText;
    [SerializeField] private TextMeshProUGUI _cardCountText;

    [Header("Couleurs")]
    [SerializeField] private Color _selectedBorderColor = new Color(1f, 0.84f, 0f);    // Or
    [SerializeField] private Color _normalBorderColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color _hoverBorderColor = new Color(0.6f, 0.6f, 0.6f);

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.03f;
    [SerializeField] private float _animDuration = 0.1f;

    [Header("Taille")]
    [SerializeField] private float _preferredWidth = 120f;
    [SerializeField] private float _preferredHeight = 80f;

    private DeckData _deckData;
    private int _deckIndex;
    private bool _isSelected;
    private bool _isHovered;
    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    public System.Action<DeckSlotUI, int> OnSlotClicked;

    public DeckData DeckData => _deckData;
    public int DeckIndex => _deckIndex;
    public bool IsSelected => _isSelected;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;

        // Assurer une taille minimale via LayoutElement
        EnsureLayoutElement();
    }

    /// <summary>
    /// Configure le LayoutElement pour forcer une taille minimale
    /// </summary>
    private void EnsureLayoutElement()
    {
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = _preferredWidth;
        layoutElement.minHeight = _preferredHeight;
        layoutElement.preferredWidth = _preferredWidth;
        layoutElement.preferredHeight = _preferredHeight;
    }

    /// <summary>
    /// Initialise le slot avec les données du deck
    /// </summary>
    public void Setup(DeckData deckData, int index)
    {
        _deckData = deckData;
        _deckIndex = index;
        UpdateDisplay();
    }

    /// <summary>
    /// Met à jour l'affichage du slot
    /// </summary>
    public void UpdateDisplay()
    {
        if (_deckData == null) return;

        // Nom du deck
        if (_deckNameText != null)
            _deckNameText.text = _deckData.deckName;

        // Nombre de cartes
        if (_cardCountText != null)
        {
            int count = _deckData.cardNames.Count;
            _cardCountText.text = $"{count} carte{(count > 1 ? "s" : "")}";
        }

        // Icone du deck de base - désactivé (non nécessaire)
        if (_defaultIcon != null)
            _defaultIcon.gameObject.SetActive(false);

        // Couleur de fond principale (basée sur la première émotion)
        if (_backgroundImage != null)
        {
            _backgroundImage.color = _deckData.GetPrimaryColor();
        }

        // Couleur de fond secondaire (basée sur la deuxième émotion)
        if (_backgroundImage2 != null)
        {
            Color secondaryColor = _deckData.GetSecondaryColor();
            _backgroundImage2.color = secondaryColor;
            // Afficher la deuxième couleur si différente de la première
            _backgroundImage2.gameObject.SetActive(_deckData.Emotion1 != _deckData.Emotion2);
        }

        UpdateSelectionVisual();
    }

    /// <summary>
    /// Définit l'état de sélection du slot
    /// </summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (_selectionBorder != null)
        {
            if (_isSelected)
                _selectionBorder.color = _selectedBorderColor;
            else if (_isHovered)
                _selectionBorder.color = _hoverBorderColor;
            else
                _selectionBorder.color = _normalBorderColor;

            _selectionBorder.gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this, _deckIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateSelectionVisual();

        if (_rectTransform != null && !_isSelected)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale * _hoverScale, _animDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateSelectionVisual();

        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale, _animDuration));
        }
    }
}
