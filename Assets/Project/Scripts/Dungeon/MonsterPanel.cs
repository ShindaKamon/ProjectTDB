using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Panneau latéral de l'exploration : une fiche par type de monstre présent dans les combats de la salle
/// (icône, nombre, stats ; PV adaptés au nombre de joueurs). L'icône est le modèle du monstre rendu une fois
/// dans une petite texture, faute d'illustration dans EnemyData.
/// </summary>
public class MonsterPanel : MonoBehaviour
{
    private const int IconResolution = 128;
    private const float CardWidth = 300f, CardHeight = 112f, CardGap = 8f, IconSize = 88f;
    // Zone hors de la salle où les modèles sont photographiés
    private static readonly Vector3 StudioOrigin = new Vector3(1000f, 0f, 1000f);
    private static readonly Color IconBackground = new Color(0.118f, 0.110f, 0.161f);

    private struct Entry
    {
        public EnemyData enemy;
        public int count;
    }

    private readonly List<Entry> _entries = new List<Entry>();
    private readonly Dictionary<EnemyData, RenderTexture> _icons = new Dictionary<EnemyData, RenderTexture>();
    private int _studioSlots;
    private GUIStyle _nameStyle, _statStyle;

    /// <summary>Remplace les fiches par les monstres des groupes donnés (un même monstre est compté une seule fois).</summary>
    public void Show(IEnumerable<EncounterData> groups)
    {
        _entries.Clear();
        foreach (EncounterData group in groups)
        {
            foreach (EncounterData.Spawn spawn in group.enemies)
            {
                int index = _entries.FindIndex(e => e.enemy == spawn.enemy);
                if (index >= 0)
                {
                    Entry entry = _entries[index];
                    entry.count++;
                    _entries[index] = entry;
                }
                else
                {
                    _entries.Add(new Entry { enemy = spawn.enemy, count = 1 });
                    if (!_icons.ContainsKey(spawn.enemy)) _icons[spawn.enemy] = null;
                }
            }
        }
        foreach (Entry entry in _entries)
            if (_icons[entry.enemy] == null) _icons[entry.enemy] = RenderIcon(entry.enemy);
    }

    private void OnDestroy()
    {
        foreach (RenderTexture icon in _icons.Values)
            if (icon != null) icon.Release();
    }

    private RenderTexture RenderIcon(EnemyData enemy)
    {
        Transform model = enemy.prefab != null ? enemy.prefab.transform.Find("Model") : null;
        if (model == null) return null;

        Vector3 origin = StudioOrigin + Vector3.right * 10f * _studioSlots++;
        var rt = new RenderTexture(IconResolution, IconResolution, 16);

        Transform copy = Instantiate(model);
        copy.position = origin;
        copy.localScale = Vector3.one * enemy.prefab.transform.localScale.x;
        copy.rotation = Quaternion.Euler(0f, 180f, 0f); // fait face à la caméra, comme un ennemi en combat

        Bounds bounds = new Bounds(copy.position, Vector3.zero);
        foreach (Renderer renderer in copy.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);

        var camGo = new GameObject("MonsterIconCamera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * 1.15f;
        cam.transform.position = bounds.center - Vector3.forward * 5f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 12f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = IconBackground;
        cam.enabled = false;

        var request = new RenderPipeline.StandardRequest { destination = rt };
        if (RenderPipeline.SupportsRenderRequest(cam, request)) RenderPipeline.SubmitRenderRequest(cam, request);
        else cam.Render();

        Destroy(camGo);
        Destroy(copy.gameObject);
        return rt;
    }

    private void OnGUI()
    {
        if (_entries.Count == 0) return;
        if (_nameStyle == null)
        {
            _nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, wordWrap = true };
            _statStyle = new GUIStyle(GUI.skin.label) { fontSize = 15 };
        }

        int players = CombatParty.Count;
        float x = Screen.width - CardWidth - 16f;
        for (int i = 0; i < _entries.Count; i++)
        {
            EnemyData enemy = _entries[i].enemy;
            var card = new Rect(x, 70f + i * (CardHeight + CardGap), CardWidth, CardHeight);
            GUI.Box(card, GUIContent.none);

            if (_icons.TryGetValue(enemy, out RenderTexture icon) && icon != null)
                GUI.DrawTexture(new Rect(card.x + 12f, card.y + 12f, IconSize, IconSize), icon, ScaleMode.ScaleToFit, false);

            string title = _entries[i].count > 1 ? $"{enemy.enemyName} × {_entries[i].count}" : enemy.enemyName;
            float textX = card.x + IconSize + 24f, textWidth = CardWidth - IconSize - 32f;
            GUI.Label(new Rect(textX, card.y + 8f, textWidth, 44f), title, _nameStyle);

            GUI.Label(new Rect(textX, card.y + 50f, textWidth, 24f),
                $"PV {EnemyScaling.ScaledHealth(enemy.maxHealth, players)}   PM {enemy.movementRange}", _statStyle);
            GUI.Label(new Rect(textX, card.y + 72f, textWidth, 24f),
                $"Armure {enemy.armor}   Rés. magique {enemy.magicResistance}", _statStyle);
        }
    }
}
