using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Frise des tours : un portrait par unité qui joue (champions et monstres, pas les invocations), en
/// commençant par celle dont c'est le tour (cadre or). Cadres bleus pour les alliés, rouges pour les
/// ennemis, mini-jauge de PV sous chaque portrait. Le nom de l'unité active s'affiche sous la frise ;
/// en réseau, le champion de ce PC est marqué « (toi) ».
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    private const float CellSize = 44f;
    private const float Spacing = 6f;
    private const int Columns = 5;
    private const float HeaderHeight = 46f;
    private const float NameHeight = 32f;
    private const float Padding = 8f;

    [SerializeField] private GameObject _panel;
    [Tooltip("Nom de l'unité dont c'est le tour, sous la frise")]
    [SerializeField] private TextMeshProUGUI _text;
    [Tooltip("Sprite arrondi des cadres de portrait")]
    [SerializeField] private Sprite _roundedSprite;
    [SerializeField] private Color _currentColor = new Color(1f, 0.85f, 0.3f);

    private RectTransform _strip;
    private readonly List<UnitPortraitView> _portraits = new List<UnitPortraitView>();

    void Awake()
    {
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitChanged);
        EventBus.Subscribe<UnitHealedEvent>(OnUnitChanged);
        if (_panel != null) _panel.SetActive(false); // affiché au premier tour (après le placement)
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitChanged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitChanged);
    }

    private void OnTurnChanged(TurnChangedEvent e) => Refresh(e.NewActiveUnit);

    private void OnUnitDied(UnitDiedEvent e) => RefreshFromGrid();

    private void OnUnitChanged(GameEvent e) => RefreshFromGrid();

    private void RefreshFromGrid()
    {
        if (Services.IsGridServiceAvailable()) Refresh(Services.Grid.GetActiveUnit());
    }

    private void Refresh(Unit active)
    {
        if (_panel == null || !Services.IsGridServiceAvailable()) return;

        var order = new List<Unit>();
        foreach (Unit unit in Services.Grid.GetAllUnits())
        {
            if (unit != null && unit.TakesTurns && unit.GetHealth() > 0) order.Add(unit);
        }
        int start = Mathf.Max(0, order.IndexOf(active));

        EnsureStrip();
        while (_portraits.Count < order.Count)
            _portraits.Add(UnitPortraitView.Create(_strip, CellSize, _roundedSprite, true));

        for (int i = 0; i < _portraits.Count; i++)
        {
            bool used = i < order.Count;
            _portraits[i].Root.gameObject.SetActive(used);
            if (!used) continue;

            _portraits[i].Set(order[(start + i) % order.Count], i == 0);
            _portraits[i].Root.localScale = i == 0 ? Vector3.one * 1.12f : Vector3.one;
        }

        ShowActiveName(order.Count > 0 ? order[start] : null);
        Resize(order.Count);
        _panel.SetActive(order.Count > 0);
    }

    private void EnsureStrip()
    {
        if (_strip != null) return;

        var go = new GameObject("Portraits", typeof(RectTransform), typeof(GridLayoutGroup));
        _strip = (RectTransform)go.transform;
        _strip.SetParent(_panel.transform, false);
        _strip.anchorMin = _strip.anchorMax = _strip.pivot = new Vector2(0.5f, 1f);
        _strip.anchoredPosition = new Vector2(0f, -HeaderHeight);

        var grid = go.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(CellSize, UnitPortraitView.TotalHeight(CellSize, true));
        grid.spacing = new Vector2(Spacing, Spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Columns;
        grid.childAlignment = TextAnchor.UpperCenter;
        _strip.sizeDelta = new Vector2(Columns * CellSize + (Columns - 1) * Spacing, 0f);
    }

    private void ShowActiveName(Unit active)
    {
        if (_text == null) return;

        if (active == null)
        {
            _text.text = "";
            return;
        }
        string name = active.DisplayName + (NetworkSession.IsActive && LocalView.IsLocalChampion(active) ? " (toi)" : "");
        _text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_currentColor)}><b>» {name}</b></color>";
    }

    // Hauteur du cadre selon le nombre de lignes de portraits ; le nom de l'unité active se place dessous
    private void Resize(int count)
    {
        int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)Columns));
        float cellHeight = UnitPortraitView.TotalHeight(CellSize, true);
        float stripHeight = rows * cellHeight + (rows - 1) * Spacing;
        _strip.sizeDelta = new Vector2(_strip.sizeDelta.x, stripHeight);

        if (_text != null)
        {
            var textRect = _text.rectTransform;
            textRect.anchoredPosition = new Vector2(0f, -(HeaderHeight + stripHeight + Padding));
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, NameHeight);
            _text.alignment = TextAlignmentOptions.Center;
            _text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        var root = (RectTransform)transform;
        root.sizeDelta = new Vector2(root.sizeDelta.x, HeaderHeight + stripHeight + Padding + NameHeight + Padding);
    }
}
