using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Au survol d'un monstre (décision du 02/10/2026), en plus de sa bulle de stats (UnitTooltipUI) : en rouge toute la zone
/// que sa prochaine carte peut toucher ce tour-ci, et un petit carré bleu sur ses cases de déplacement (EnemyThreat : depuis ses
/// cases atteignables, à portée et en ligne de vue). Les lancers annoncés et les embuscades ont leur propre affichage.
/// Survoler l'aperçu de carte d'un monstre (EnemyPreviewHoveredEvent) fait de même, avec sa case en jaune ; le monstre mis
/// en avant est publié (EnemyFocusChangedEvent) pour que son aperçu grossisse. Marques posées au-dessus des cases, sans
/// toucher à leurs surbrillances.
/// </summary>
public class EnemyThreatView : MonoBehaviour
{
    [Tooltip("Matériau transparent des marques (ex. TileMaterial)")]
    [SerializeField] private Material _markerMaterial;
    [SerializeField] private Color _moveColor = new Color(0.25f, 0.5f, 1f, 0.8f);
    [SerializeField] private Color _threatColor = new Color(1f, 0.3f, 0.25f, 0.35f);

    private const float MarkerHeight = 0.065f; // juste sous les zones de lancer (ThrowZoneView)
    private readonly List<GameObject> _markers = new List<GameObject>();
    [SerializeField] private Color _focusColor = new Color(1f, 0.85f, 0.2f, 0.6f);

    private Enemy _shown;
    private Enemy _previewHovered; // monstre dont on survole l'aperçu de carte (EnemyCardPreviewUI)

    private void OnEnable() => EventBus.Subscribe<EnemyPreviewHoveredEvent>(OnPreviewHovered);

    private void OnDisable()
    {
        EventBus.Unsubscribe<EnemyPreviewHoveredEvent>(OnPreviewHovered);
        Clear();
    }

    private void OnPreviewHovered(EnemyPreviewHoveredEvent e) => _previewHovered = e.Enemy;

    // Monstre survolé sur le plateau, sinon celui de l'aperçu survolé ; l'aperçu du monstre mis en avant grossit
    private void Update()
    {
        Enemy hovered = FindHoveredEnemy();
        if (hovered == null && _previewHovered != null && _previewHovered.GetHealth() > 0 && !_previewHovered.IsHidden)
            hovered = _previewHovered;
        if (hovered == _shown) return;
        Clear();
        _shown = hovered;
        if (hovered != null) Show(hovered);
        EventBus.Publish(new EnemyFocusChangedEvent(hovered));
    }

    private static Enemy FindHoveredEnemy()
    {
        if (Mouse.current == null || !Services.IsGridServiceAvailable()) return null;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;
        if (!InputManager.TryGetPointedObject(out GameObject pointed)) return null;
        return pointed.TryGetComponent(out Enemy enemy) && enemy.GetHealth() > 0 && !enemy.IsHidden ? enemy : null;
    }

    private void Show(Enemy enemy)
    {
        IGridService grid = Services.Grid;
        Vector2Int pos = enemy.GetCurrentGridPos();
        var moves = new HashSet<Vector2Int>();
        foreach (Tile tile in grid.GetMovementTiles(pos, enemy.GetMaxMovementPoints(), enemy).Keys)
            moves.Add(grid.GetGridPosFromWorldPos(tile.transform.position));
        moves.Remove(pos);

        var threat = new HashSet<Vector2Int>();
        CardData card = enemy.GetNextCard();
        if (card != null && (card.targetsUnit || card.targetsTile) && card.telegraphedZoneCount == 0 && !card.isAmbush)
        {
            var origins = new List<Vector2Int>(moves) { pos };
            threat = EnemyThreat.Cells(origins, card.targetRange, grid.GetAllCells(),
                (from, to) => GameActionValidator.HasLineOfSight(card, from, to));
        }

        // Toute la zone de dégâts en rouge (cases de déplacement comprises), le déplacement en petit carré bleu par-dessus
        foreach (Vector2Int cell in threat)
            if (cell != pos) AddMarker(grid, cell, _threatColor, 0.9f, 0f);
        foreach (Vector2Int cell in moves) AddMarker(grid, cell, _moveColor, 0.4f, 0.005f);
        AddMarker(grid, pos, _focusColor, 0.9f, 0f); // sa case : on le repère aussi depuis son aperçu
    }

    private void AddMarker(IGridService grid, Vector2Int cell, Color color, float size, float lift)
    {
        Tile tile = grid.GetTileAtPosition(cell);
        if (tile == null) return;
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(marker.GetComponent<Collider>());
        marker.name = "EnemyThreat";
        marker.transform.SetParent(transform, false);
        marker.transform.position = tile.transform.position + Vector3.up * (MarkerHeight + lift);
        marker.transform.localScale = new Vector3(size, 0.02f, size);
        var renderer = marker.GetComponent<Renderer>();
        renderer.sharedMaterial = _markerMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(block);
        _markers.Add(marker);
    }

    private void Clear()
    {
        foreach (GameObject marker in _markers) if (marker != null) Destroy(marker);
        _markers.Clear();
        _shown = null;
    }
}
