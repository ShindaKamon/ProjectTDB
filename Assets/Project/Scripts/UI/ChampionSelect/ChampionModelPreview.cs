using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aperçu 3D animé (repos) du champion sélectionné : son modèle est photographié en continu dans une texture
/// affichée par une RawImage posée sur l'illustration plein corps, qu'elle remplace tant qu'un modèle existe.
/// </summary>
public class ChampionModelPreview : MonoBehaviour
{
    private const int TextureWidth = 600, TextureHeight = 900;
    // Zone hors de la scène où le modèle est photographié
    private static readonly Vector3 StudioOrigin = new Vector3(2000f, 0f, 2000f);

    private RawImage _image;
    private Camera _camera;
    private RenderTexture _texture;
    private Transform _model;

    /// <summary>Crée l'aperçu, affiché dans le rectangle de l'illustration donnée.</summary>
    public static ChampionModelPreview Create(Graphic artArea)
    {
        var studio = new GameObject("ChampionModelPreview");
        studio.transform.position = StudioOrigin;
        var preview = studio.AddComponent<ChampionModelPreview>();
        preview.Build(artArea);
        return preview;
    }

    private void Build(Graphic artArea)
    {
        _texture = new RenderTexture(TextureWidth, TextureHeight, 24, RenderTextureFormat.ARGB32);

        var camGo = new GameObject("Camera");
        camGo.transform.SetParent(transform, false);
        _camera = camGo.AddComponent<Camera>();
        _camera.orthographic = true;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 30f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        _camera.targetTexture = _texture;

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(transform, false);
        lightGo.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.None;

        // Enfant de l'illustration, à sa taille, en conservant le rapport 2:3 de la texture
        var uiGo = new GameObject("ModelPreview", typeof(RectTransform));
        var rect = (RectTransform)uiGo.transform;
        rect.SetParent(artArea.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var fitter = uiGo.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = (float)TextureWidth / TextureHeight;
        _image = uiGo.AddComponent<RawImage>();
        _image.texture = _texture;
        _image.raycastTarget = false;
        _image.enabled = false;
    }

    /// <summary>Affiche le champion ; renvoie false (et masque l'aperçu) s'il n'a pas de modèle 3D.</summary>
    public bool Show(ChampionData champion)
    {
        if (_model != null) Destroy(_model.gameObject);
        _model = null;

        Transform source = champion != null && champion.prefab != null ? champion.prefab.transform.Find("Model") : null;
        _image.enabled = source != null;
        if (source == null) return false;

        _model = Instantiate(source, transform);
        _model.localPosition = Vector3.zero;
        _model.localRotation = Quaternion.Euler(0f, 180f, 0f); // face à la caméra
        _model.localScale = Vector3.one * champion.prefab.transform.localScale.x;

        var bounds = new Bounds(_model.position, Vector3.zero);
        foreach (Renderer renderer in _model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
        _camera.orthographicSize = bounds.extents.y * 1.08f;
        _camera.transform.position = bounds.center - Vector3.forward * 10f;
        return true;
    }

    private void OnDestroy()
    {
        if (_texture != null) _texture.Release();
    }
}
