using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Crée (ou recrée) le donjon de l'Orphelinat : les rencontres, le DungeonData chargé depuis
/// Resources par DungeonRun, et ExplorationScene (ajoutée aux Build Settings). Menu :
/// Tools > Donjon > Créer l'Orphelinat. Relancer écrase les assets du donjon, pas la scène si elle existe déjà.
/// </summary>
public static class DungeonSetup
{
    private const string EnemyFolder = "Assets/ScriptableObjects/Characters/Enemy/";
    private const string DungeonFolder = "Assets/ScriptableObjects/Dungeons";
    private const string ResourcesFolder = "Assets/Resources/Dungeons";
    private const string ScenePath = "Assets/Project/Scenes/ExplorationScene.unity";

    [MenuItem("Tools/Donjon/Créer l'Orphelinat")]
    public static void CreateOrphanage()
    {
        EnsureFolder("Assets/ScriptableObjects", "Dungeons");
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "Dungeons");

        var sheep = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyFolder + "MoutonDePoussiere.asset");
        var boss = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyFolder + "UnderBed.asset");
        if (sheep == null || boss == null)
        {
            Debug.LogError("DungeonSetup : MoutonDePoussiere.asset ou UnderBed.asset introuvable.");
            return;
        }

        EncounterData twoSheep = SaveEncounter("Orphelinat_DeuxMoutons", "Deux moutons de poussière",
            (sheep, new Vector2Int(3, 7)), (sheep, new Vector2Int(6, 7)));
        EncounterData threeSheep = SaveEncounter("Orphelinat_TroisMoutons", "Trois moutons de poussière",
            (sheep, new Vector2Int(2, 7)), (sheep, new Vector2Int(5, 7)), (sheep, new Vector2Int(7, 7)));
        EncounterData bossFight = SaveEncounter("Orphelinat_UnderBed", "Le monstre sous le lit",
            (boss, new Vector2Int(5, 8)), (sheep, new Vector2Int(3, 7)), (sheep, new Vector2Int(7, 7)));

        var dungeon = ScriptableObject.CreateInstance<DungeonData>();
        dungeon.dungeonName = "L'Orphelinat";
        dungeon.rooms = new List<DungeonData.Room>
        {
            Room("Le dortoir", new Vector2Int(1, 3), (new Vector2Int(4, 3), twoSheep),
                Door(8, 3, 1, 1, 3)),
            Room("Le couloir", new Vector2Int(1, 3), (new Vector2Int(4, 3), threeSheep),
                Door(0, 3, 0, 7, 3), Door(8, 3, 2, 1, 3)),
            Room("Sous le lit", new Vector2Int(1, 3), (new Vector2Int(4, 3), bossFight),
                Door(0, 3, 1, 7, 3)),
        };
        Save(dungeon, ResourcesFolder + "/Orphelinat.asset");

        if (!System.IO.File.Exists(ScenePath)) CreateExplorationScene();
        AssetDatabase.SaveAssets();
        Debug.Log("DungeonSetup : Orphelinat créé (3 salles, 3 rencontres, ExplorationScene).");
    }

    private static DungeonData.Room Room(string name, Vector2Int start, (Vector2Int cell, EncounterData encounter) monster,
        params DungeonData.Door[] doors)
    {
        var room = new DungeonData.Room { roomName = name, size = new Vector2Int(9, 7), start = start };
        room.monsters.Add(new DungeonData.MonsterSpot { cell = monster.cell, encounter = monster.encounter });
        room.doors.AddRange(doors);
        return room;
    }

    private static DungeonData.Door Door(int x, int y, int targetRoom, int arrivalX, int arrivalY) =>
        new DungeonData.Door { cell = new Vector2Int(x, y), targetRoom = targetRoom, arrivalCell = new Vector2Int(arrivalX, arrivalY) };

    private static EncounterData SaveEncounter(string fileName, string displayName, params (EnemyData enemy, Vector2Int cell)[] spawns)
    {
        var encounter = ScriptableObject.CreateInstance<EncounterData>();
        encounter.encounterName = displayName;
        foreach (var s in spawns)
            encounter.enemies.Add(new EncounterData.Spawn { enemy = s.enemy, cell = s.cell });
        return Save(encounter, $"{DungeonFolder}/{fileName}.asset");
    }

    // Écrase l'asset existant en gardant son GUID (les références restent valides)
    private static T Save<T>(T data, string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(data, path);
            return data;
        }
        EditorUtility.CopySerialized(data, existing);
        EditorUtility.SetDirty(existing);
        Object.DestroyImmediate(data);
        return existing;
    }

    private static void CreateExplorationScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
        var controller = new GameObject("Exploration").AddComponent<ExplorationController>();
        controller.transform.SetSiblingIndex(0);

        var so = new SerializedObject(controller);
        so.FindProperty("_tilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/Prefabs/Grid/Tile.prefab");
        so.ApplyModifiedProperties();

        // Même fond et même lumière que CombatScene (la caméra isométrique est posée par ExplorationController)
        Camera camera = Camera.main;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.065f, 0.09f);
        camera.orthographic = true;
        camera.orthographicSize = 6f;

        Light light = Object.FindFirstObjectByType<Light>();
        light.intensity = 2f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = new Quaternion(0.40821788f, -0.23456968f, 0.10938163f, 0.8754261f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.CloseScene(scene, true);

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }
}
