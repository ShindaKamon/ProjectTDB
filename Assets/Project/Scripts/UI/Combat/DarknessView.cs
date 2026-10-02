using System.Collections;
using UnityEngine;

/// <summary>
/// Rendu du terrain assombri (TerrainDarkness, ex. Marée d'ombre) : la lumière principale et la lumière ambiante
/// baissent en fondu, puis reviennent quand l'ombre se dissipe.
/// </summary>
public class DarknessView : MonoBehaviour
{
    [Tooltip("Part de la lumière qui reste quand le terrain est assombri (0 = noir complet)")]
    [Range(0f, 1f)]
    [SerializeField] private float _darkFactor = 0.35f;

    [Tooltip("Durée du fondu (secondes)")]
    [SerializeField] private float _fadeDuration = 0.6f;

    private float _sunIntensity;
    private Color _ambient;
    private float _ambientIntensity;
    private Coroutine _fade;

    void OnEnable()
    {
        _sunIntensity = RenderSettings.sun != null ? RenderSettings.sun.intensity : 1f;
        _ambient = RenderSettings.ambientLight;
        _ambientIntensity = RenderSettings.ambientIntensity; // lumière ambiante en mode Skybox
        EventBus.Subscribe<TerrainDarknessChangedEvent>(OnDarknessChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<TerrainDarknessChangedEvent>(OnDarknessChanged);
        Apply(1f); // ne laisse pas la scène sombre
    }

    private void OnDarknessChanged(TerrainDarknessChangedEvent e)
    {
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(Fade(e.IsDark ? _darkFactor : 1f));
    }

    private float _current = 1f;

    private IEnumerator Fade(float target)
    {
        float start = _current;
        for (float t = 0f; t < _fadeDuration; t += Time.deltaTime)
        {
            Apply(Mathf.Lerp(start, target, t / _fadeDuration));
            yield return null;
        }
        Apply(target);
        _fade = null;
    }

    private void Apply(float factor)
    {
        _current = factor;
        if (RenderSettings.sun != null) RenderSettings.sun.intensity = _sunIntensity * factor;
        RenderSettings.ambientLight = _ambient * factor;
        RenderSettings.ambientIntensity = _ambientIntensity * factor;
    }
}
