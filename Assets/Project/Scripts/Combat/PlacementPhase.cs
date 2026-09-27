using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Phase de placement avant le combat (façon Dofus) : les champions sont posés sur les cases de
/// départ (en rouge) ; clic sur un champion pour le choisir, puis sur une case rouge pour l'y
/// déplacer (ou échanger avec l'allié qui l'occupe). « Lancer le combat » termine la phase.
/// Les boss gardent la position fixée dans la scène. Les règles sont dans PlacementBoard.
/// </summary>
public class PlacementPhase : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panneau affiché pendant le placement (consigne + bouton).")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _instructionText;
    [SerializeField] private Button _launchButton;

    [Header("Couleurs")]
    [SerializeField] private Color _startCellColor = new Color(0.85f, 0.25f, 0.25f);
    [SerializeField] private Color _selectedCellColor = new Color(0.95f, 0.8f, 0.2f);

    private PlacementBoard<Champion> _board;
    private List<Champion> _champions;
    private Champion _selected;
    private Action _onDone;

    public bool IsActive => _board != null;

    void Awake()
    {
        if (_launchButton != null) _launchButton.onClick.AddListener(End);
        if (_panel != null) _panel.SetActive(false);
    }

    /// <summary>
    /// Démarre le placement : les champions sont déjà sur les premières cases de départ, dans l'ordre.
    /// onDone est appelé quand le joueur lance le combat.
    /// </summary>
    public void Begin(List<Champion> champions, IReadOnlyList<Vector2Int> startCells, Action onDone)
    {
        _champions = champions;
        _onDone = onDone;
        _board = new PlacementBoard<Champion>(startCells);
        _board.PlaceInOrder(champions);
        _selected = champions.Count > 0 ? champions[0] : null;

        if (_panel != null) _panel.SetActive(true);
        Refresh();
        GameLog.Log($"Phase de placement : {champions.Count} champion(s), {startCells.Count} cases de départ.");
    }

    void Update()
    {
        if (!IsActive || Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _selected = null;
            Refresh();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!TryGetClickedCell(out Vector2Int cell)) return;

        Champion atCell = _board.UnitAt(cell);
        if (_selected != null && _board.IsStartCell(cell) && atCell != _selected)
        {
            MoveSelectedTo(cell);
        }
        else if (atCell != null)
        {
            _selected = atCell;
        }
        Refresh();
    }

    private void MoveSelectedTo(Vector2Int cell)
    {
        _board.TryGetPosition(_selected, out Vector2Int from);
        if (!_board.TryMove(_selected, cell, out Champion swapped)) return;

        _selected.TeleportTo(cell);
        if (swapped != null) swapped.TeleportTo(from);
        GameLog.Log($"Placement : {_selected.name} -> {cell}{(swapped != null ? $", échange avec {swapped.name}" : "")}");
        _selected = null;
    }

    private bool TryGetClickedCell(out Vector2Int cell)
    {
        cell = default;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f)) return false;

        if (hit.collider.gameObject.TryGetComponentSafe(out Unit unit))
        {
            cell = unit.GetCurrentGridPos();
            return true;
        }
        if (hit.collider.gameObject.TryGetComponentSafe(out Tile tile))
        {
            cell = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);
            return true;
        }
        return false;
    }

    private void Refresh()
    {
        EventBus.Publish(new ResetTileColorsEvent());
        foreach (Vector2Int cell in _board.StartCells)
        {
            bool isSelected = _selected != null && _board.UnitAt(cell) == _selected;
            Services.Grid.HighlightTile(cell, isSelected ? _selectedCellColor : _startCellColor);
        }

        if (_instructionText == null) return;
        if (_selected == null)
        {
            _instructionText.text = "Placement : clique un champion, puis une case rouge.";
        }
        else
        {
            int player = _champions.IndexOf(_selected) + 1;
            _instructionText.text = $"Joueur {player} – {_selected.championData.championName} : choisis une case rouge.";
        }
    }

    private void End()
    {
        if (!IsActive) return;

        _board = null;
        _selected = null;
        if (_panel != null) _panel.SetActive(false);
        EventBus.Publish(new ResetTileColorsEvent());
        GameLog.Log("Fin du placement : début du combat.");
        _onDone?.Invoke();
    }
}
