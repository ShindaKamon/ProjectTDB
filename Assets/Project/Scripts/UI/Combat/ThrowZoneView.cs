using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zones rouges au sol des lancers annoncés par les monstres (ThrowZonesChangedEvent) : affichées de l'annonce
/// jusqu'à ce qu'elles tombent, soient annulées ou que le monstre meure. Indépendantes des surbrillances des cases.
/// Les trajets des tas qu'Au lit ! ramènera (IsSweep) s'affichent à part, en orange.
/// </summary>
public class ThrowZoneView : MonoBehaviour
{
    [Tooltip("Matériau transparent des marques (ex. TileMaterial)")]
    [SerializeField] private Material _markerMaterial;
    [SerializeField] private Color _markerColor = new Color(0.85f, 0.1f, 0.1f, 0.6f);
    [SerializeField] private Color _sweepColor = new Color(0.95f, 0.55f, 0.1f, 0.55f);

    private const float MarkerHeight = 0.07f; // juste au-dessus du calque de surbrillance des cases
    private readonly Dictionary<(Enemy, bool), List<GameObject>> _markers = new Dictionary<(Enemy, bool), List<GameObject>>(); // (monstre, trajets d'Au lit !)

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
        var key = (e.Thrower, e.IsSweep);
        Clear(key);
        if (e.Cells.Count == 0) return;

        var markers = new List<GameObject>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", e.IsSweep ? _sweepColor : _markerColor);
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
        _markers[key] = markers;
    }

    private void OnUnitDied(UnitDiedEvent e)
    {
        if (!(e.DeadUnit is Enemy enemy)) return;
        Clear((enemy, false));
        Clear((enemy, true));
    }

    private void Clear((Enemy, bool) key)
    {
        if (!_markers.TryGetValue(key, out List<GameObject> markers)) return;
        foreach (GameObject marker in markers) if (marker != null) Destroy(marker);
        _markers.Remove(key);
    }
}
