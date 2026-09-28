using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Bulle d'information au survol d'un monstre (boss compris) ou d'une invocation (ex: Lyse) : nom, PV (+ bouclier) et pastilles
/// de stats, les mêmes que sous la barre du boss (CodexCardVisual.UnitChips : ATQ, armure,
/// résistance magique, bouclier, PA/PM avec les retraits en attente, Ténacité).
/// La bulle est construite à la première utilisation et suit la souris.
/// </summary>
public class UnitTooltipUI : MonoBehaviour
{
    [SerializeField] private Sprite _chipBackground;
    [SerializeField] private Vector2 _mouseOffset = new Vector2(24f, -24f);
    [Tooltip("Rafraîchissement des stats pendant le survol (secondes)")]
    [SerializeField] private float _refreshInterval = 0.25f;

    private RectTransform _panel;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _healthText;
    private Transform _chips;
    private Unit _shownUnit;
    private float _nextRefresh;

    void Update()
    {
        Unit hovered = FindHoveredUnit();
        if (hovered == null)
        {
            Hide();
            return;
        }

        if (_panel == null) Build();
        if (hovered != _shownUnit || Time.time >= _nextRefresh) Refresh(hovered);

        _panel.gameObject.SetActive(true);
        FollowMouse();
    }

    // Monstres (boss compris) et invocations (ex: Lyse) ; les champions ont leur HUD
    private static Unit FindHoveredUnit()
    {
        if (Mouse.current == null) return null;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;
        if (!Services.IsGridServiceAvailable()) return null;
        if (!InputManager.TryGetPointedObject(out GameObject pointed)) return null;
        if (!pointed.TryGetComponent(out Unit unit) || unit.GetHealth() <= 0) return null;

        return unit is Enemy || unit is SummonUnit ? unit : null;
    }

    private static string DisplayName(Unit unit) => unit switch
    {
        Enemy enemy when enemy.GetEnemyData() != null => enemy.GetEnemyData().enemyName,
        SummonUnit summon => summon.DisplayName,
        _ => unit.name
    };

    private void Refresh(Unit unit)
    {
        _shownUnit = unit;
        _nextRefresh = Time.time + _refreshInterval;

        _nameText.text = DisplayName(unit);
        int shield = unit.GetShield();
        _healthText.text = $"PV {unit.GetHealth()}/{unit.GetMaxHealth()}"
            + (shield > 0 ? $"  <color=#{ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(ChipKind.Shield))}>+{shield}</color>" : "");
        CardChipsView.Build(_chips, CodexCardVisual.UnitChips(unit), _chipBackground, _nameText.font, 18f, 20f);
    }

    private void Hide()
    {
        _shownUnit = null;
        if (_panel != null) _panel.gameObject.SetActive(false);
    }

    private void FollowMouse()
    {
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        var canvasRect = (RectTransform)canvas.transform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screen = Mouse.current.position.ReadValue() + _mouseOffset;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out Vector2 local))
            _panel.localPosition = local;
    }

    private void Build()
    {
        var canvas = GetComponentInParent<Canvas>().rootCanvas;

        var root = new GameObject("UnitTooltip", typeof(RectTransform), typeof(Canvas), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _panel = (RectTransform)root.transform;
        _panel.SetParent(canvas.transform, false);
        _panel.pivot = new Vector2(0f, 1f); // coin haut-gauche sous la souris
        var ownCanvas = root.GetComponent<Canvas>();
        ownCanvas.overrideSorting = true;
        ownCanvas.sortingOrder = 400;

        var bg = root.GetComponent<Image>();
        bg.color = CodexCardVisual.CardBackground;
        bg.raycastTarget = false;

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 10, 12);
        layout.spacing = 6f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = root.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _nameText = NewText("Name", 22f, FontStyles.Bold);
        _healthText = NewText("Health", 18f, FontStyles.Normal);

        var chips = new GameObject("Chips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        chips.transform.SetParent(_panel, false);
        var chipsLayout = chips.GetComponent<HorizontalLayoutGroup>();
        chipsLayout.spacing = 6f;
        chipsLayout.childControlWidth = chipsLayout.childControlHeight = true;
        chipsLayout.childForceExpandWidth = chipsLayout.childForceExpandHeight = false;
        _chips = chips.transform;
    }

    private TextMeshProUGUI NewText(string name, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(_panel, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = CodexCardVisual.Ink;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }
}
