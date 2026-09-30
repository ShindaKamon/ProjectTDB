using UnityEngine;

/// <summary>
/// Aura visuelle d'un champion, aux couleurs de l'émotion : lueur douce autour du corps qui pulse quand une
/// jauge est pleine (fusion possible), plus vive et plus haute pendant la fusion (selon les paliers), éclat
/// qui se dilate à l'activation. Lit l'état du champion à chaque frame ; posée à la volée par FusionPanelUI.
/// </summary>
public class FusionAura : MonoBehaviour
{
    private const float BurstDuration = 0.8f;
    private const int SpriteSize = 128;

    private static Sprite s_glow;

    private Champion _champion;
    private SpriteRenderer _ground;
    private SpriteRenderer _halo;
    private SpriteRenderer _burst;
    private bool _wasFused;
    private float _burstStart = -1f;

    void Awake()
    {
        _champion = GetComponent<Champion>();
        _ground = CreateSprite("FusionGroundGlow", GlowSprite(), Quaternion.Euler(90f, 0f, 0f), new Vector3(0f, 0.04f, 0f));
        _burst = CreateSprite("FusionBurst", GlowSprite(), Quaternion.identity, new Vector3(0f, 0.9f, 0f));
        _halo = CreateSprite("FusionHalo", GlowSprite(), Quaternion.identity, new Vector3(0f, 0.9f, 0f));
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam != null) _halo.transform.rotation = _burst.transform.rotation = cam.transform.rotation;

        EmotionGauge gauge = _champion.Gauge;
        bool fused = gauge.IsFused;
        if (fused && !_wasFused) _burstStart = Time.time;
        _wasFused = fused;

        EmotionType emotion = fused ? gauge.ActiveFusion : ReadyEmotion();
        Color color = emotion != EmotionType.None ? CodexCardVisual.EmotionColor(emotion) : Color.white;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * (fused ? 5f : 3f));
        int tiers = fused ? gauge.GetTiers(emotion) : 0;

        bool visible = emotion != EmotionType.None;
        float haloAlpha = fused ? 0.55f + 0.12f * tiers + 0.1f * pulse : 0.3f + 0.25f * pulse;
        Vector3 haloScale = fused ? new Vector3(2.6f + 0.4f * tiers, 3.6f + 0.5f * tiers, 1f) : new Vector3(2.2f + 0.15f * pulse, 3.2f + 0.2f * pulse, 1f);
        SetSprite(_halo, visible, color, haloAlpha, haloScale);

        float groundAlpha = fused ? 0.7f + 0.2f * pulse : 0.4f + 0.25f * pulse;
        SetSprite(_ground, visible, color, groundAlpha, Vector3.one * (fused ? 2f + 0.25f * tiers : 1.7f));

        float burstAge = _burstStart < 0f ? 1f : (Time.time - _burstStart) / BurstDuration;
        bool bursting = burstAge < 1f;
        SetSprite(_burst, bursting, color, bursting ? (1f - burstAge) * 0.9f : 0f, Vector3.one * (1.5f + 4f * burstAge));
    }

    // Première émotion dont la jauge est pleine et qui a une forme pour ce champion
    private EmotionType ReadyEmotion()
    {
        for (EmotionType emotion = EmotionType.Anger; emotion <= EmotionType.Anticipation; emotion++)
        {
            if (_champion.Gauge.IsFull(emotion) && _champion.GetFusion(emotion) != null) return emotion;
        }
        return EmotionType.None;
    }

    private static void SetSprite(SpriteRenderer sprite, bool visible, Color color, float alpha, Vector3 scale)
    {
        sprite.enabled = visible;
        if (!visible) return;
        sprite.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        sprite.transform.localScale = scale;
    }

    private SpriteRenderer CreateSprite(string objectName, Sprite sprite, Quaternion rotation, Vector3 localPosition)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPosition;
        go.transform.rotation = rotation;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.enabled = false;
        return renderer;
    }

    private static Sprite GlowSprite() => s_glow != null ? s_glow : s_glow = Generate(r =>
        r >= 1f ? 0f : Mathf.Pow(1f - r, 1.5f));

    // Texture carrée dont l'opacité dépend de la distance normalisée au centre (0 au centre, 1 au bord)
    private static Sprite Generate(System.Func<float, float> opacityByRadius)
    {
        var texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        for (int y = 0; y < SpriteSize; y++)
        {
            for (int x = 0; x < SpriteSize; x++)
            {
                float dx = (x + 0.5f) / SpriteSize * 2f - 1f;
                float dy = (y + 0.5f) / SpriteSize * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(opacityByRadius(r))));
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, SpriteSize, SpriteSize), new Vector2(0.5f, 0.5f), SpriteSize);
    }
}
