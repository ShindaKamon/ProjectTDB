using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Génère les portraits carrés (PNG transparent) des champions et des ennemis en photographiant leur modèle 3D
/// en pose de repos, puis les affecte au champ « portrait » de leur fiche. Champions : buste ; ennemis : corps entier.
/// Menu : Tools > Portraits > Générer. Relancer écrase les PNG.
/// </summary>
public static class PortraitGenerator
{
    private const string Folder = "Assets/Project/Textures/Portraits";
    private const int Size = 256;
    private const float BustFraction = 0.36f;
    private static readonly Vector3 StudioOrigin = new Vector3(3000f, 0f, 3000f);

    [MenuItem("Tools/Portraits/Générer")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Project/Textures", "Portraits");

        foreach (ChampionData champion in FindAssets<ChampionData>())
        {
            Sprite sprite = Render(champion.prefab, champion.name, true);
            if (sprite == null) continue;
            champion.portrait = sprite;
            EditorUtility.SetDirty(champion);
        }
        foreach (EnemyData enemy in FindAssets<EnemyData>())
        {
            Sprite sprite = Render(enemy.prefab, enemy.name, false);
            if (sprite == null) continue;
            enemy.portrait = sprite;
            EditorUtility.SetDirty(enemy);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("PortraitGenerator : portraits générés et affectés.");
    }

    private static T[] FindAssets<T>() where T : Object =>
        AssetDatabase.FindAssets("t:" + typeof(T).Name)
            .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();

    private static Sprite Render(GameObject prefab, string assetName, bool bust)
    {
        Transform source = prefab != null ? prefab.transform.Find("Model") : null;
        if (source == null)
        {
            Debug.LogWarning($"PortraitGenerator : {assetName} n'a pas de modèle, portrait ignoré.");
            return null;
        }

        var studio = new GameObject("PortraitStudio") { hideFlags = HideFlags.HideAndDontSave };
        studio.transform.position = StudioOrigin;
        var rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
        try
        {
            Transform model = Object.Instantiate(source, studio.transform);
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.localScale = Vector3.one * prefab.transform.localScale.x;
            PoseIdle(model);

            var bounds = new Bounds(model.position, Vector3.zero);
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(studio.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.targetTexture = rt;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 30f;

            float visibleHeight = bust ? bounds.size.y * BustFraction : Mathf.Max(bounds.size.x, bounds.size.y) * 1.1f;
            cam.orthographicSize = visibleHeight * 0.5f;
            float centerY = bust ? bounds.max.y - visibleHeight * 0.5f - bounds.size.y * 0.02f : bounds.center.y;
            cam.transform.position = new Vector3(bounds.center.x, centerY, bounds.center.z - 10f);

            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(studio.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;

            cam.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            string path = $"{Folder}/{assetName}_Portrait.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally
        {
            Object.DestroyImmediate(studio);
            rt.Release();
            Object.DestroyImmediate(rt);
        }
    }

    // Échantillonne le clip de repos : sans cela le modèle reste en T-pose
    private static void PoseIdle(Transform model)
    {
        if (!model.TryGetComponent(out Animator animator) || animator.runtimeAnimatorController == null) return;
        AnimationClip clip = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name.EndsWith("Idle") || c.name.EndsWith("Flying"));
        if (clip != null) clip.SampleAnimation(model.gameObject, 0.4f);
    }
}
