using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ordre des tours : les unités qui jouent (champions et monstres, pas les invocations), en
/// commençant par celle dont c'est le tour. Alliés et ennemis en couleurs différentes ; en
/// réseau, le champion de ce PC est marqué « (toi) ».
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Color _currentColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color _allyColor = new Color(0.55f, 0.85f, 1f);
    [SerializeField] private Color _enemyColor = new Color(1f, 0.55f, 0.5f);

    void Awake()
    {
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
        if (_panel != null) _panel.SetActive(false); // affiché au premier tour (après le placement)
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    private void OnTurnChanged(TurnChangedEvent e) => Refresh(e.NewActiveUnit);

    private void OnUnitDied(UnitDiedEvent e)
    {
        if (Services.IsGridServiceAvailable()) Refresh(Services.Grid.GetActiveUnit());
    }

    private void Refresh(Unit active)
    {
        if (_text == null || !Services.IsGridServiceAvailable()) return;

        var order = new List<Unit>();
        foreach (Unit unit in Services.Grid.GetAllUnits())
        {
            if (unit != null && unit.TakesTurns && unit.GetHealth() > 0) order.Add(unit);
        }
        int start = Mathf.Max(0, order.IndexOf(active));

        var lines = new List<string>();
        for (int i = 0; i < order.Count; i++)
        {
            Unit unit = order[(start + i) % order.Count];
            bool ally = unit.GetFaction() == Unit.UnitFaction.Player;
            string name = unit.DisplayName + (NetworkSession.IsActive && LocalView.IsLocalChampion(unit) ? " (toi)" : "");
            Color color = i == 0 ? _currentColor : ally ? _allyColor : _enemyColor;
            string line = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{(i == 0 ? "» " : "")}{name}</color>";
            lines.Add(i == 0 ? $"<b>{line}</b>" : line);
        }

        _text.text = string.Join("\n", lines);
        if (_panel != null) _panel.SetActive(order.Count > 0);
    }
}
