using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Affiche au-dessus de chaque unité visée le « -N » que la carte survolée infligerait
/// (avec « KO » si le coup est fatal). Purement visuel : les chiffres viennent de DamagePreview.
/// </summary>
public class DamagePreviewUI : MonoBehaviour
{
    private static readonly Color DamageColor = new Color(1f, 0.45f, 0.4f);
    private static readonly Color LethalColor = new Color(1f, 0.85f, 0.3f);
    private const float LiftPixels = 110f;

    private RectTransform _canvasRect;
    private Camera _camera;
    private readonly List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
    private readonly List<DamagePreview.Entry> _entries = new List<DamagePreview.Entry>();

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        _canvasRect = canvas != null ? (RectTransform)canvas.rootCanvas.transform : null;
    }

    void OnEnable()
    {
        EventBus.Subscribe<DamagePreviewEvent>(OnPreview);
        EventBus.Subscribe<ResetTileColorsEvent>(OnReset);
        EventBus.Subscribe<UnitDamagedEvent>(OnDamaged);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<DamagePreviewEvent>(OnPreview);
        EventBus.Unsubscribe<ResetTileColorsEvent>(OnReset);
        EventBus.Unsubscribe<UnitDamagedEvent>(OnDamaged);
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
        Clear();
    }

    private void OnPreview(DamagePreviewEvent e)
    {
        _entries.Clear();
        _entries.AddRange(e.Entries);
        Show();
    }

    private void OnReset(ResetTileColorsEvent e) => Clear();
    private void OnDamaged(UnitDamagedEvent e) => Clear();
    private void OnTurnChanged(TurnChangedEvent e) => Clear();

    private void Clear()
    {
        _entries.Clear();
        Show();
    }

    private void Show()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            while (_labels.Count <= i) _labels.Add(NewLabel());

            DamagePreview.Entry entry = _entries[i];
            TextMeshProUGUI label = _labels[i];
            label.text = entry.Lethal ? $"-{entry.Damage}  KO" : $"-{entry.Damage}";
            label.color = entry.Lethal ? LethalColor : DamageColor;
            label.gameObject.SetActive(true);
        }
        for (int i = _entries.Count; i < _labels.Count; i++) _labels[i].gameObject.SetActive(false);
        Reposition();
    }

    // Les unités ne bougent pas pendant un survol, mais la caméra peut : on suit à chaque frame
    void LateUpdate()
    {
        if (_entries.Count > 0) Reposition();
    }

    private void Reposition()
    {
        if (_canvasRect == null) return;
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return;

        for (int i = 0; i < _entries.Count; i++)
        {
            Unit target = _entries[i].Target;
            if (target == null)
            {
                _labels[i].gameObject.SetActive(false);
                continue;
            }

            Vector3 screen = _camera.WorldToScreenPoint(target.transform.position) + Vector3.up * LiftPixels;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, null, out Vector2 local);
            _labels[i].rectTransform.anchoredPosition = local;
        }
    }

    private TextMeshProUGUI NewLabel()
    {
        var go = new GameObject("DamageLabel", typeof(RectTransform));
        go.transform.SetParent(_canvasRect != null ? _canvasRect : transform, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        label.fontSize = 34f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        label.outlineWidth = 0.25f;
        label.outlineColor = new Color32(0, 0, 0, 255);

        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200f, 50f);
        return label;
    }
}
