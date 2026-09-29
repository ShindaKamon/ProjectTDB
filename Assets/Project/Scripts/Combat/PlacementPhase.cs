using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Phase de placement avant le combat (façon Dofus) : les champions sont posés sur les cases de
/// départ (en rouge), puis chaque joueur place son champion à tour de rôle, comme en solo : son
/// champion est sélectionné d'office et chaque clic sur une case rouge libre l'y déplace (on ne peut
/// pas déplacer le champion d'un autre joueur). Le bouton passe au joueur suivant, puis lance le
/// combat après le dernier. Les boss gardent la position fixée dans la scène. Les règles sont dans
/// PlacementBoard.
/// </summary>
public class PlacementPhase : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panneau affiché pendant le placement (consigne + bouton).")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _instructionText;
    [SerializeField] private Button _launchButton;
    [SerializeField] private string _nextPlayerLabel = "Joueur suivant";
    [SerializeField] private string _launchLabel = "Lancer le combat";
    [Tooltip("Interface utile seulement pendant le combat (main, pioche, fin de tour, HUD…) : masquée pendant le placement.")]
    [SerializeField] private GameObject[] _combatOnlyUI;

    [Header("Couleurs")]
    [SerializeField] private Color _startCellColor = new Color(0.85f, 0.25f, 0.25f);
    [SerializeField] private Color _selectedCellColor = new Color(0.95f, 0.8f, 0.2f);

    private PlacementBoard<Champion> _board;
    private List<Champion> _champions;
    private int _currentIndex;
    private Action _onDone;

    public bool IsActive => _board != null;

    /// <summary>Place dans CombatParty du joueur qui se place.</summary>
    public int CurrentIndex => IsActive ? CombatParty.IndexOf(Current.championData) : -1;

    // Champion du joueur qui se place
    private Champion Current => _champions[_currentIndex];

    void Awake()
    {
        if (_launchButton != null) _launchButton.onClick.AddListener(OnButtonClicked);
        if (_panel != null) _panel.SetActive(false);
    }

    /// <summary>
    /// Démarre le placement : les champions sont déjà sur les premières cases de départ, dans l'ordre.
    /// onDone est appelé quand le dernier joueur lance le combat.
    /// </summary>
    public void Begin(List<Champion> champions, IReadOnlyList<Vector2Int> startCells, Action onDone)
    {
        _champions = champions;
        _onDone = onDone;
        _board = new PlacementBoard<Champion>(startCells);
        _board.PlaceInOrder(champions);
        _currentIndex = 0;

        if (_panel != null) _panel.SetActive(true);
        SetCombatUIVisible(false);
        Refresh();
        GameLog.Log($"Phase de placement : {champions.Count} champion(s), {startCells.Count} cases de départ.");
    }

    void Update()
    {
        if (!IsActive || Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!TryGetClickedCell(out Vector2Int cell)) return;

        // Action du joueur : passe par les commandes (voir CombatCommandExecutor)
        Services.Commands?.Submit(CombatCommand.PlacementMove(CurrentIndex, cell));
    }

    /// <summary>Place le champion courant sur une case rouge libre (commande PlacementMove).</summary>
    public void TryPlaceCurrent(Vector2Int cell)
    {
        if (!IsActive) return;

        // Seules les cases rouges libres comptent : pas question de bouger le champion d'un autre joueur
        if (!_board.IsStartCell(cell) || _board.UnitAt(cell) != null) return;

        if (_board.TryMove(Current, cell))
        {
            Current.TeleportTo(cell);
            GameLog.Log($"Placement : {Current.name} -> {cell}");
            Refresh();
        }
    }

    private void OnButtonClicked()
    {
        if (IsActive) Services.Commands?.Submit(CombatCommand.PlacementNext(CurrentIndex));
    }

    /// <summary>Joueur suivant, ou lancement du combat après le dernier (commande PlacementNext).</summary>
    public void Advance()
    {
        if (!IsActive) return;

        if (_currentIndex < _champions.Count - 1)
        {
            _currentIndex++;
            Refresh();
        }
        else
        {
            End();
        }
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
            bool isCurrent = _board.UnitAt(cell) == Current;
            Services.Grid.HighlightTile(cell, isCurrent ? _selectedCellColor : _startCellColor);
        }

        if (_instructionText != null)
            _instructionText.text = $"Joueur {_currentIndex + 1} – {Current.championData.championName} : clique une case rouge pour te placer.";

        TextMeshProUGUI label = _launchButton != null ? _launchButton.GetComponentInChildren<TextMeshProUGUI>() : null;
        if (label != null)
            label.text = _currentIndex < _champions.Count - 1 ? _nextPlayerLabel : _launchLabel;
    }

    private void End()
    {
        _board = null;
        if (_panel != null) _panel.SetActive(false);
        EventBus.Publish(new ResetTileColorsEvent());
        // Réafficher l'interface de combat avant le premier tour, pour qu'elle reçoive son TurnChangedEvent
        SetCombatUIVisible(true);
        GameLog.Log("Fin du placement : début du combat.");
        _onDone?.Invoke();
    }

    private void SetCombatUIVisible(bool visible)
    {
        if (_combatOnlyUI == null) return;
        foreach (GameObject ui in _combatOnlyUI)
        {
            if (ui != null) ui.SetActive(visible);
        }
    }
}
