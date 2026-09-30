using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Force le lancement du Play Mode a toujours demarrer sur la scene principale du jeu
/// (MainMenuScene), quelle que soit la scene actuellement ouverte dans l'editeur
/// (ex: CombatScene). Activable/desactivable via Tools/Play Mode Start Scene.
/// </summary>
[InitializeOnLoad]
public static class PlayModeStartSceneSetup
{
    private const string MainScenePath = "Assets/Project/Scenes/MainMenuScene.unity";
    private const string EditorPrefKey = "ProjectTDB.ForcePlayModeStartScene";
    private const string MenuPath = "Tools/Play Mode Start Scene/Toujours demarrer sur MainMenuScene";

    static PlayModeStartSceneSetup()
    {
        ApplyPreference();
    }

    private static bool IsEnabled => EditorPrefs.GetBool(EditorPrefKey, true);

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        bool newValue = !IsEnabled;
        EditorPrefs.SetBool(EditorPrefKey, newValue);
        ApplyPreference();
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked(MenuPath, IsEnabled);
        return true;
    }

    private static void ApplyPreference()
    {
        if (IsEnabled)
        {
            SceneAsset mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
            if (mainScene == null)
            {
                Debug.LogWarning($"PlayModeStartSceneSetup: scene introuvable a '{MainScenePath}'. " +
                    "EditorSceneManager.playModeStartScene n'a pas ete assigne.");
                return;
            }

            EditorSceneManager.playModeStartScene = mainScene;
        }
        else
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
