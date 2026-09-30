using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panneau d'équipe (coop) : une ligne par champion allié avec portrait, nom, jauge de PV et PV chiffrés ;
/// le champion dont c'est le tour est encadré en or. Masqué en solo (un seul champion).
/// </summary>
public class TeamPortraitsUI : MonoBehaviour
{
    private const float RowHeight = 46f;
    private const float PortraitSize = 40f;
    private const float RowSpacing = 6f;
    private const float Padding = 8f;

    [Tooltip("Sprite arrondi du fond du panneau et des cadres de portrait")]
    [SerializeField] private Sprite _roundedSprite;
    [SerializeField] private Color _panelColor = new Color(0.075f, 0.07f, 0.11f, 0.85f);

    private class Row
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public UnitPortraitView Portrait;
        public TextMeshProUGUI Name;
        public RectTransform HpFill;
        public Image HpFillImage;
        public TextMeshProUGUI HpText;
    }

    private readonly List<Row> _rows = new List<Row>();
    private readonly List<Champion> _team = new List<Champion>();
    private RectTransform _rect;
    private Image _background;

    void Awake()
    {
        _rect = (RectTransform)transform;
        _background = gameObject.GetComponent<Image>();
        if (_background == null) _background = gameObject.AddComponent<Image>();
        _background.sprite = _roundedSprite;
        _background.type = _roundedSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        _background.color = _panelColor;
        _background.raycastTarget = false;
        _background.enabled = false; // affiché dès qu'il y a au moins 2 alliés
    }

    void OnEnable()
    {
        EventBus.Subscribe<TurnChangedEvent>(OnEvent);
        EventBus.Subscribe<UnitDamagedEvent>(OnEvent);
        EventBus.Subscribe<UnitHealedEvent>(OnEvent);
        EventBus.Subscribe<UnitDiedEvent>(OnEvent);
        EventBus.Subscribe<UnitEffectAppliedEvent>(OnEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<TurnChangedEvent>(OnEvent);
        EventBus.Unsubscribe<UnitDamagedEvent>(OnEvent);
        EventBus.Unsubscribe<UnitHealedEvent>(OnEvent);
        EventBus.Unsubscribe<UnitDiedEvent>(OnEvent);
        EventBus.Unsubscribe<UnitEffectAppliedEvent>(OnEvent);
    }

    private void OnEvent(GameEvent e) => Refresh();

    private void Refresh()
    {
        if (!Services.IsGridServiceAvailable()) return;

        _team.Clear();
        foreach (Unit unit in Services.Grid.GetAllUnits())
        {
            if (unit is Champion champion && champion.GetFaction() == Unit.UnitFaction.Player) _team.Add(champion);
        }

        bool show = _team.Count >= 2;
        _background.enabled = show;

        Unit active = Services.Grid.GetActiveUnit();
        while (show && _rows.Count < _team.Count) _rows.Add(CreateRow());

        for (int i = 0; i < _rows.Count; i++)
        {
            bool used = show && i < _team.Count;
            _rows[i].Root.gameObject.SetActive(used);
            if (used) Fill(_rows[i], _team[i], _team[i] == active);
        }

        if (!show) return;
        float height = Padding * 2 + _team.Count * RowHeight + (_team.Count - 1) * RowSpacing;
        _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, height);
    }

    private void Fill(Row row, Champion champion, bool active)
    {
        row.Portrait.Set(champion, active);
        row.Name.text = champion.DisplayName + (NetworkSession.IsActive && LocalView.IsLocalChampion(champion) ? " (toi)" : "");
        row.Name.color = active ? UnitPortraitView.ActiveFrame : Color.white;

        int health = champion.GetHealth();
        int max = champion.GetMaxHealth();
        int shield = champion.GetShield();
        row.HpFill.anchorMax = new Vector2(max > 0 ? Mathf.Clamp01((float)health / max) : 0f, 1f);
        row.HpFillImage.color = shield > 0 ? CodexCardVisual.ChipColor(ChipKind.Shield) : new Color(0.86f, 0.30f, 0.30f);
        row.HpText.text = shield > 0 ? $"{health}/{max} (+{shield})" : $"{health}/{max}";
        row.Group.alpha = health > 0 ? 1f : 0.4f;
    }

    private Row CreateRow()
    {
        var row = new Row();
        var go = new GameObject("Row", typeof(RectTransform), typeof(CanvasGroup));
        row.Root = (RectTransform)go.transform;
        row.Root.SetParent(transform, false);
        row.Group = go.GetComponent<CanvasGroup>();
        row.Group.blocksRaycasts = false;
        row.Root.anchorMin = new Vector2(0f, 1f);
        row.Root.anchorMax = new Vector2(1f, 1f);
        row.Root.pivot = new Vector2(0.5f, 1f);
        int index = _rows.Count;
        row.Root.offsetMin = new Vector2(Padding, -(Padding + (index + 1) * RowHeight + index * RowSpacing));
        row.Root.offsetMax = new Vector2(-Padding, -(Padding + index * (RowHeight + RowSpacing)));

        row.Portrait = UnitPortraitView.Create(row.Root, PortraitSize, _roundedSprite, false);
        row.Portrait.Root.anchorMin = row.Portrait.Root.anchorMax = new Vector2(0f, 0.5f);
        row.Portrait.Root.pivot = new Vector2(0f, 0.5f);
        row.Portrait.Root.anchoredPosition = Vector2.zero;

        float left = PortraitSize + 10f;
        row.Name = NewText("Name", row.Root, 20f, TextAlignmentOptions.Left, FontStyles.Bold);
        SetRect(row.Name.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(left, -22f), new Vector2(0f, 0f));

        var hpBack = new GameObject("HpBack", typeof(RectTransform), typeof(Image));
        hpBack.transform.SetParent(row.Root, false);
        hpBack.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f);
        hpBack.GetComponent<Image>().raycastTarget = false;
        SetRect((RectTransform)hpBack.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(left, 4f), new Vector2(0f, 20f));

        var fill = new GameObject("HpFill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(hpBack.transform, false);
        row.HpFillImage = fill.GetComponent<Image>();
        row.HpFillImage.raycastTarget = false;
        row.HpFill = (RectTransform)fill.transform;
        SetRect(row.HpFill, Vector2.zero, Vector2.one, new Vector2(1f, 1f), new Vector2(-1f, -1f));

        row.HpText = NewText("HpText", hpBack.transform, 15f, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(row.HpText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return row;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, float size, TextAlignmentOptions alignment, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = alignment;
        text.fontStyle = style;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform r, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        r.anchorMin = min;
        r.anchorMax = max;
        r.offsetMin = offsetMin;
        r.offsetMax = offsetMax;
    }
}
