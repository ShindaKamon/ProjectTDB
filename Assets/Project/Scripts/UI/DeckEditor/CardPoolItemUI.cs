using System.Collections.Generic;
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
    [SerializeField] private Transform _chipsContainer;
    [SerializeField] private Sprite _chipBackground;
    [SerializeField] private Sprite[] _chipIcons;             // nommés « icon_<nom> » (icônes du codex)

    [Header("Texte")]
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.02f;
    [SerializeField] private float _animDuration = 0.1f;

    private CardData _cardData;
    private Vector3 _originalScale = Vector3.one;
    private RectTransform _rectTransform;
    private readonly Dictionary<string, Sprite> _iconsByName = new Dictionary<string, Sprite>();

    public System.Action<CardData> OnCardClicked;
    public CardData CardData => _cardData;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;
    }

    // Setup peut précéder Awake (carte créée alors que l'écran de deck est encore inactif) :
    // le dictionnaire d'icônes est donc construit à la demande.
    private bool TryGetIcon(string name, out Sprite sprite)
    {
        if (_iconsByName.Count == 0 && _chipIcons != null)
        {
            foreach (var icon in _chipIcons)
                if (icon != null) _iconsByName[icon.name.Replace("icon_", "")] = icon;
        }
        return _iconsByName.TryGetValue(name, out sprite);
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
        if (_descriptionText != null) _descriptionText.text = card.description;

        PaintDiagram(card);
        BuildChips(card);
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

    private void BuildChips(CardData card)
    {
        if (_chipsContainer == null) return;

        for (int i = _chipsContainer.childCount - 1; i >= 0; i--)
            Destroy(_chipsContainer.GetChild(i).gameObject);

        foreach (CardChip chip in CodexCardVisual.Chips(card))
            CreateChip(chip);
    }

    /// <summary>Pastille : fond arrondi, icône colorée selon l'effet, valeur (ex. « ↗ 33 »).</summary>
    private void CreateChip(CardChip chip)
    {
        bool warn = chip.Kind == ChipKind.Warn;
        Color accent = CodexCardVisual.ChipColor(chip.Kind);

        var go = new GameObject("Chip_" + chip.Icon, typeof(RectTransform));
        go.transform.SetParent(_chipsContainer, false);

        var bg = go.AddComponent<Image>();
        bg.sprite = _chipBackground;
        bg.type = Image.Type.Sliced;
        bg.color = warn ? CodexCardVisual.WarnBackground : CodexCardVisual.CardBorder;
        bg.raycastTarget = false;

        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(5, 7, 2, 2);
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (TryGetIcon(chip.Icon, out Sprite sprite))
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(go.transform, false);
            var icon = iconGO.AddComponent<Image>();
            icon.sprite = sprite;
            icon.color = accent;
            icon.raycastTarget = false;
            var iconSize = iconGO.AddComponent<LayoutElement>();
            iconSize.preferredWidth = iconSize.minWidth = 14;
            iconSize.preferredHeight = iconSize.minHeight = 14;
        }

        if (!string.IsNullOrEmpty(chip.Text))
        {
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<TextMeshProUGUI>();
            if (_nameText != null) text.font = _nameText.font;
            text.text = chip.Text;
            text.fontSize = 12;
            text.color = warn ? accent : CodexCardVisual.Ink;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
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
