using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Carte du pool de l'éditeur de deck, au design du codex émotionnel : en-tête (rond de coût à
/// la couleur de l'émotion, nom, « Émotion · Catégorie »), schéma de portée 9×9 avec sa légende,
/// pastilles d'effets avec icônes, description. Contenu calculé par CodexCardVisual.
/// Un clic ajoute la carte au deck (OnCardClicked).
/// </summary>
public class CardPoolItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cadre")]
    [SerializeField] private Image _background;
    [SerializeField] private Outline _border;

    [Header("En-tête")]
    [SerializeField] private Image _costCircle;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Schéma et effets")]
    [SerializeField] private Transform _diagramGrid;          // 81 Images (9×9), ligne par ligne depuis le haut
    [SerializeField] private TextMeshProUGUI _captionText;

    [Header("Texte")]
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.02f;
    [SerializeField] private float _animDuration = 0.1f;

    private CardData _cardData;
    private Vector3 _originalScale = Vector3.one;
    private RectTransform _rectTransform;

    public System.Action<CardData> OnCardClicked;
    public CardData CardData => _cardData;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;
    }

    public void Setup(CardData card)
    {
        _cardData = card;

        // L'item peut être recyclé pendant un survol : on remet l'échelle et le cadre à zéro
        StopAllCoroutines();
        if (_rectTransform != null) _rectTransform.localScale = _originalScale;
        SetBorder(false);

        if (_background != null) _background.color = CodexCardVisual.CardBackground;
        Color costColor = CodexCardVisual.CostColor(card); // couleur de l'émotion, blanc pour une Signature
        if (_costCircle != null) _costCircle.color = costColor;
        if (_costText != null)
        {
            _costText.text = card.costPA.ToString();
            _costText.color = CodexCardVisual.ReadableTextOn(costColor);
        }
        if (_nameText != null) _nameText.text = card.cardName;
        if (_subtitleText != null) _subtitleText.text = CodexCardVisual.Subtitle(card);
        if (_captionText != null) _captionText.text = CodexCardVisual.Caption(card);
        CardTextView.Apply(_descriptionText, card); // texte généré depuis les champs

        PaintDiagram(card);
    }

    private void PaintDiagram(CardData card)
    {
        if (_diagramGrid == null) return;

        DiagramCell[,] cells = CodexCardVisual.BuildDiagram(card);
        int n = CodexCardVisual.DiagramSize;
        for (int i = 0; i < _diagramGrid.childCount && i < n * n; i++)
        {
            int x = i % n, y = i / n;
            var image = _diagramGrid.GetChild(i).GetComponent<Image>();
            if (image == null) continue;

            image.color = cells[x, y] switch
            {
                DiagramCell.Range => CodexCardVisual.GridRange,
                DiagramCell.Area => CodexCardVisual.Foe,
                DiagramCell.AreaAlly => CodexCardVisual.Ally,
                DiagramCell.Caster => CodexCardVisual.Ink,
                _ => CodexCardVisual.GridCell,
            };
        }
    }

    private void SetBorder(bool highlighted)
    {
        if (_border != null)
            _border.effectColor = highlighted ? CodexCardVisual.Accent : CodexCardVisual.CardBorder;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(_cardData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetBorder(true);
        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale * _hoverScale, _animDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetBorder(false);
        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale, _animDuration));
        }
    }
}
