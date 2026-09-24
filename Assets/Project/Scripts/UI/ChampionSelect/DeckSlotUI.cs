using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Tuile d'un deck sur la page Choix du deck (palette du codex émotionnel) : bande aux couleurs
/// du deck, nom, couleurs écrites dans leur teinte (« Colère · Joie »), nombre de cartes.
/// Cadre gris, plus clair au survol, doré quand le deck est sélectionné.
/// </summary>
public class DeckSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Structure")]
    [SerializeField] private Image _background;
    [SerializeField] private Outline _border;
    [SerializeField] private Transform _colorStrip;      // un segment par couleur du deck (créés au Setup)

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _deckNameText;
    [SerializeField] private TextMeshProUGUI _colorsText;
    [SerializeField] private TextMeshProUGUI _cardCountText;

    [Header("Cadre")]
    [SerializeField] private Color _selectedBorderColor = new Color(1f, 0.84f, 0f);    // Or
    [SerializeField] private Color _normalBorderColor = CodexCardVisual.CardBorder;
    [SerializeField] private Color _hoverBorderColor = CodexCardVisual.InkDim;

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.03f;
    [SerializeField] private float _animDuration = 0.1f;

    [Header("Taille")]
    [SerializeField] private float _preferredWidth = 260f;
    [SerializeField] private float _preferredHeight = 120f;

    private DeckData _deckData;
    private int _deckIndex;
    private readonly List<EmotionType> _colors = new List<EmotionType>();
    private bool _isSelected;
    private bool _isHovered;
    private Vector3 _originalScale = Vector3.one;
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

        EnsureLayoutElement();
    }

    /// <summary>Taille de la tuile dans la rangée de decks.</summary>
    private void EnsureLayoutElement()
    {
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = layoutElement.preferredWidth = _preferredWidth;
        layoutElement.minHeight = layoutElement.preferredHeight = _preferredHeight;
    }

    /// <summary>Initialise la tuile avec le deck et ses couleurs (voir DeckRules.DeckColors).</summary>
    public void Setup(DeckData deckData, int index, IEnumerable<EmotionType> colors)
    {
        _deckData = deckData;
        _deckIndex = index;
        _colors.Clear();
        if (colors != null) _colors.AddRange(colors);
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (_deckData == null) return;

        if (_background != null)
            _background.color = CodexCardVisual.CardBackground;

        if (_deckNameText != null)
            _deckNameText.text = _deckData.deckName;

        if (_cardCountText != null)
        {
            int count = _deckData.cardNames.Count;
            _cardCountText.text = $"{count} carte{(count > 1 ? "s" : "")}";
        }

        if (_colorsText != null)
        {
            var names = new List<string>();
            foreach (var emotion in _colors)
            {
                string hex = ColorUtility.ToHtmlStringRGB(CodexCardVisual.EmotionColor(emotion));
                names.Add($"<color=#{hex}>{CardVisualHelper.GetEmotionName(emotion)}</color>");
            }
            _colorsText.text = names.Count > 0 ? string.Join("  ·  ", names) : "Toutes les couleurs";
        }

        BuildColorStrip();
        UpdateSelectionVisual();
    }

    /// <summary>Bande du haut : un segment par couleur du deck, de largeur égale.</summary>
    private void BuildColorStrip()
    {
        if (_colorStrip == null) return;

        for (int i = _colorStrip.childCount - 1; i >= 0; i--)
            Destroy(_colorStrip.GetChild(i).gameObject);

        var colors = _colors.Count > 0 ? _colors : new List<EmotionType> { EmotionType.None };
        foreach (var emotion in colors)
        {
            var segment = new GameObject("Couleur_" + emotion, typeof(RectTransform));
            segment.transform.SetParent(_colorStrip, false);
            var image = segment.AddComponent<Image>();
            image.color = CodexCardVisual.EmotionColor(emotion);
            image.raycastTarget = false;
            segment.AddComponent<LayoutElement>().flexibleWidth = 1;
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (_border == null) return;

        _border.effectColor = _isSelected ? _selectedBorderColor : _isHovered ? _hoverBorderColor : _normalBorderColor;
        _border.effectDistance = _isSelected ? new Vector2(3f, -3f) : new Vector2(1.5f, -1.5f);
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
