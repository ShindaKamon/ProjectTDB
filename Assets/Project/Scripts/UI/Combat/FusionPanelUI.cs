using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Jauges d'émotion et boutons de fusion (Éveil) du champion affiché : un bouton par forme du champion,
/// jauge en points, activable quand elle est pleine. Seules les émotions présentes dans le deck sont affichées ;
/// un deck bi-émotion empile ses deux boutons l'un au-dessus de l'autre. Ne fait que soumettre la commande ActivateFusion.
/// À poser sur le panneau de stats du champion ; la rangée se construit au démarrage, au-dessus du panneau.
/// </summary>
public class FusionPanelUI : MonoBehaviour
{
    [Header("Disposition")]
    [Tooltip("Hauteur de la rangée")]
    [SerializeField] private float _rowHeight = 34f;
    [Tooltip("Décalage vertical au-dessus du haut du panneau (au-dessus de la rangée de statuts)")]
    [SerializeField] private float _offsetAbovePanel = 44f;
    [SerializeField] private float _buttonWidth = 130f;

    private RectTransform _row;
    private readonly System.Collections.Generic.List<RectTransform> _readyButtons = new System.Collections.Generic.List<RectTransform>();

    void Awake()
    {
        var go = new GameObject("FusionRow", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _row = (RectTransform)go.transform;
        _row.SetParent(transform, false);
        _row.anchorMin = new Vector2(0f, 1f);
        _row.anchorMax = new Vector2(1f, 1f);
        _row.pivot = new Vector2(0f, 0f);
        _row.anchoredPosition = new Vector2(0f, _offsetAbovePanel);
        _row.sizeDelta = new Vector2(0f, _rowHeight);

        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.LowerLeft;
        go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
    }

    void OnEnable()
    {
        EventBus.Subscribe<FusionChangedEvent>(OnFusionChanged);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<FusionChangedEvent>(OnFusionChanged);
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void Update()
    {
        float scale = 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 6f);
        foreach (RectTransform button in _readyButtons)
        {
            if (button != null) button.localScale = Vector3.one * scale;
        }
    }

    private void OnFusionChanged(FusionChangedEvent e)
    {
        EnsureAura(e.Champion);
        Refresh();
    }

    private void OnTurnChanged(TurnChangedEvent e) => Refresh();

    private static void EnsureAura(Champion champion)
    {
        if (champion != null && champion.GetComponent<FusionAura>() == null) champion.gameObject.AddComponent<FusionAura>();
    }

    private void Refresh()
    {
        _readyButtons.Clear();
        for (int i = _row.childCount - 1; i >= 0; i--) Destroy(_row.GetChild(i).gameObject);
        if (!Services.IsGridServiceAvailable()) return;

        Unit active = Services.Grid.GetActiveUnit();
        Champion champion = active != null ? LocalView.ChampionToShow(active) : null;
        if (champion == null || champion.championData == null) return;
        EnsureAura(champion);

        var deck = champion.GetComponent<DeckManager>();
        foreach (FusionData form in champion.championData.fusions)
        {
            if (form != null && deck != null && deck.DeckEmotions.Contains(form.emotion)) AddButton(champion, form, active);
        }
    }

    private void AddButton(Champion champion, FusionData form, Unit active)
    {
        EmotionType emotion = form.emotion;
        bool fused = champion.ActiveFusion == form;
        int points = champion.Gauge.GetPoints(emotion);
        int tiers = champion.Gauge.GetTiers(emotion);

        bool myTurn = active == champion && Services.Commands != null && Services.Commands.IsLocalTurn;
        bool canActivate = myTurn && GameActionValidator.CanActivateFusion(champion, emotion).IsValid;

        Color color = CodexCardVisual.EmotionColor(emotion);
        var go = new GameObject($"Fusion_{emotion}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(_row, false);
        var element = go.GetComponent<LayoutElement>();
        element.preferredWidth = _buttonWidth;
        element.preferredHeight = _rowHeight;

        var image = go.GetComponent<Image>();
        image.color = fused || canActivate ? color : new Color(color.r, color.g, color.b, 0.45f);

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = canActivate;
        button.onClick.AddListener(() => Activate(emotion));
        if (canActivate && !fused) _readyButtons.Add((RectTransform)go.transform);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var rect = (RectTransform)textGo.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 14f;
        text.raycastTarget = false;
        text.color = Color.white;
        string state = fused ? $"palier {tiers}/{EmotionGauge.MaxTiers}" : $"{points}/{EmotionGauge.MaxPoints}";
        text.text = $"{form.formName}\n{state}";
    }

    private static void Activate(EmotionType emotion)
    {
        ICombatCommandService commands = Services.Commands;
        if (commands != null && commands.ActiveActor >= 0)
            commands.Submit(CombatCommand.ActivateFusion(commands.ActiveActor, emotion));
    }
}
