using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Fenêtre de consultation d'une pile en combat (pioche ou défausse) : les cartes en grille, non
/// jouables, dans l'ordre fourni (voir CardPileOrder). Construite à la première ouverture ; se
/// ferme par le fond, le bouton Fermer ou Échap.
/// </summary>
public class CardPileViewerUI : MonoBehaviour
{
    [Tooltip("Visuel de carte de la main (avec CardUIElement), réutilisé sans interaction")]
    [SerializeField] private GameObject _cardPrefab;
    [Tooltip("Échelle des cartes dans la fenêtre")]
    [SerializeField] private float _cardScale = 0.6f;
    [SerializeField] private Vector2 _panelSize = new Vector2(1200f, 760f);
    [SerializeField] private int _sortingOrder = 500; // au-dessus de la main et de la carte en cours de ciblage

    private GameObject _root;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _emptyText;
    private RectTransform _grid;
    private readonly List<GameObject> _shownCards = new List<GameObject>();

    public bool IsOpen => _root != null && _root.activeSelf;

    void Update()
    {
        if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Hide();
    }

    public void Show(string title, IReadOnlyList<CardData> cards)
    {
        if (_root == null) Build();

        foreach (GameObject shown in _shownCards) Destroy(shown);
        _shownCards.Clear();

        _title.text = title;
        _emptyText.gameObject.SetActive(cards.Count == 0);

        foreach (CardData card in cards)
            _shownCards.Add(CreateCard(card));

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (_root != null) _root.SetActive(false);
    }

    // Carte affichée dans une case de la grille : la case a la taille de la carte réduite,
    // la carte y est centrée à l'échelle voulue et ne reçoit aucun clic
    private GameObject CreateCard(CardData card)
    {
        var holder = new GameObject(card.cardName, typeof(RectTransform), typeof(CanvasGroup));
        holder.transform.SetParent(_grid, false);
        var group = holder.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        GameObject visual = Instantiate(_cardPrefab, holder.transform);
        if (visual.TryGetComponent(out CardUIElement element))
        {
            element.SetCardData(card);
            element.enabled = false; // pas de survol, de sélection ni d'animation
        }

        var rt = (RectTransform)visual.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * _cardScale;
        return holder;
    }

    private void Build()
    {
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;

        // Racine plein écran, dans son propre canvas trié au-dessus du reste
        _root = new GameObject("CardPileViewer", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        _root.transform.SetParent(canvas.transform, false);
        Stretch((RectTransform)_root.transform);
        var rootCanvas = _root.GetComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = _sortingOrder;

        // Fond assombri : un clic ferme la fenêtre
        var backdrop = NewChild("Backdrop", _root.transform, typeof(Image), typeof(Button));
        Stretch(backdrop);
        backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        backdrop.GetComponent<Button>().onClick.AddListener(Hide);

        // Panneau (frère du fond, pour qu'un clic dessus ne remonte pas jusqu'au bouton du fond)
        var panel = NewChild("Panel", _root.transform, typeof(Image));
        panel.sizeDelta = _panelSize;
        panel.GetComponent<Image>().color = CodexCardVisual.CardBackground;

        _title = NewText("Title", panel, 34f, FontStyles.Bold);
        _title.alignment = TextAlignmentOptions.Left;
        var titleRt = _title.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.offsetMin = new Vector2(30f, -70f);
        titleRt.offsetMax = new Vector2(-180f, -16f);

        // Bouton Fermer en haut à droite
        var close = NewChild("CloseButton", panel, typeof(Image), typeof(Button));
        close.anchorMin = close.anchorMax = close.pivot = new Vector2(1f, 1f);
        close.anchoredPosition = new Vector2(-20f, -18f);
        close.sizeDelta = new Vector2(140f, 50f);
        close.GetComponent<Image>().color = CodexCardVisual.CardBorder;
        close.GetComponent<Button>().onClick.AddListener(Hide);
        var closeText = NewText("Label", close, 26f, FontStyles.Normal);
        closeText.text = "Fermer";
        Stretch(closeText.rectTransform);

        // Zone défilante
        var scroll = NewChild("Scroll", panel, typeof(ScrollRect));
        scroll.anchorMin = Vector2.zero;
        scroll.anchorMax = Vector2.one;
        scroll.offsetMin = new Vector2(20f, 20f);
        scroll.offsetMax = new Vector2(-20f, -84f);

        var viewport = NewChild("Viewport", scroll, typeof(Image), typeof(RectMask2D));
        Stretch(viewport);
        viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f); // reçoit la molette

        _grid = NewChild("Grid", viewport, typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        _grid.anchorMin = new Vector2(0f, 1f);
        _grid.anchorMax = new Vector2(1f, 1f);
        _grid.pivot = new Vector2(0.5f, 1f);
        _grid.offsetMin = _grid.offsetMax = Vector2.zero;

        Vector2 cardSize = ((RectTransform)_cardPrefab.transform).sizeDelta * _cardScale;
        var layout = _grid.GetComponent<GridLayoutGroup>();
        layout.cellSize = cardSize;
        layout.spacing = new Vector2(16f, 16f);
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;
        _grid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollRect = scroll.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = _grid;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 40f;

        _emptyText = NewText("Empty", viewport, 28f, FontStyles.Italic);
        _emptyText.text = "Aucune carte";
        Stretch(_emptyText.rectTransform);
    }

    private static RectTransform NewChild(string name, Transform parent, params System.Type[] components)
    {
        var go = new GameObject(name, components);
        if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, float size, FontStyles style)
    {
        var text = NewChild(name, parent, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = CodexCardVisual.Ink;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
