using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zones rouges au sol des lancers annoncés par les monstres (ThrowZonesChangedEvent) : affichées de l'annonce
/// jusqu'à ce qu'elles tombent, soient annulées ou que le monstre meure. Indépendantes des surbrillances des cases.
/// </summary>
public class ThrowZoneView : MonoBehaviour
{
    [Tooltip("Matériau transparent des marques (ex. TileMaterial)")]
    [SerializeField] private Material _markerMaterial;
    [SerializeField] private Color _markerColor = new Color(0.85f, 0.1f, 0.1f, 0.6f);

    private const float MarkerHeight = 0.07f; // juste au-dessus du calque de surbrillance des cases
    private readonly Dictionary<Enemy, List<GameObject>> _markers = new Dictionary<Enemy, List<GameObject>>();

    private void OnEnable()
    {
        EventBus.Subscribe<ThrowZonesChangedEvent>(OnThrowZonesChanged);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ThrowZonesChangedEvent>(OnThrowZonesChanged);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    private void OnThrowZonesChanged(ThrowZonesChangedEvent e)
    {
        Clear(e.Thrower);
        if (e.Cells.Count == 0) return;

        var markers = new List<GameObject>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", _markerColor);
        foreach (Vector2Int cell in e.Cells)
        {
            Tile tile = Services.Grid.GetTileAtPosition(cell);
            if (tile == null) continue;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(marker.GetComponent<Collider>());
            marker.name = "ThrowZone";
            marker.transform.SetParent(transform, false);
            marker.transform.position = tile.transform.position + Vector3.up * MarkerHeight;
            marker.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            var renderer = marker.GetComponent<Renderer>();
            renderer.sharedMaterial = _markerMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.SetPropertyBlock(block);
            markers.Add(marker);
        }
        _markers[e.Thrower] = markers;
    }

    private void OnUnitDied(UnitDiedEvent e)
    {
        if (e.DeadUnit is Enemy enemy) Clear(enemy);
    }

    private void Clear(Enemy thrower)
    {
        if (!_markers.TryGetValue(thrower, out List<GameObject> markers)) return;
        foreach (GameObject marker in markers) if (marker != null) Destroy(marker);
        _markers.Remove(thrower);
    }
}
