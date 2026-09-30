using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Branche les animations Quaternius sur les prefabs : boucle des clips de repos / marche, un AnimatorController par
/// modèle (Idle, Walk, Hit, Death), puis Animator + UnitAnimator sur l'enfant « Model » de chaque prefab.
/// Menu : Tools > Animations > Configurer les modèles. Relancer régénère les contrôleurs.
/// </summary>
public static class AnimationSetup
{
    private const string ModelFolder = "Assets/ThirdParty/Quaternius/";
    private const string ControllerFolder = "Assets/Project/Animations";
    private const string PrefabFolder = "Assets/Project/Prefabs/";

    private struct Rig
    {
        public string fbx, idle, walk, hit, death, prefab;
    }

    private static readonly Rig[] Rigs =
    {
        Human("ModularMen/Casual_Hoodie", "Champions/Evan_Base"),
        Human("ModularMen/Adventurer", "Champions/Crux_Base"),
        Human("ModularMen/Suit", "Champions/Raze_Base"),
        Human("ModularWomen/Casual", "Summons/Lyse_Summon"),
        // Cthulhu n'a pas de clip de repos ni de marche : il flotte dans les deux cas
        new Rig { fbx = "CuteMonsters/Models/Cthulhu", idle = "Flying", walk = "Flying", hit = "HitRecieve", death = "Death", prefab = "Enemies/UnderBed" },
        new Rig { fbx = "CuteMonsters/Models/Ghost", idle = "Idle", walk = "Walk", hit = "HitRecieve", death = "Death", prefab = "Enemies/MoutonDePoussiere" },
    };

    private static Rig Human(string fbx, string prefab) =>
        new Rig { fbx = fbx, idle = "Idle", walk = "Walk", hit = "HitRecieve", death = "Death", prefab = prefab };

    [MenuItem("Tools/Animations/Configurer les modèles")]
    public static void Configure()
    {
        if (!AssetDatabase.IsValidFolder(ControllerFolder)) AssetDatabase.CreateFolder("Assets/Project", "Animations");

        foreach (Rig rig in Rigs)
        {
            string fbxPath = ModelFolder + rig.fbx + ".fbx";
            LoopClips(fbxPath, rig.idle, rig.walk);
            AnimatorController controller = BuildController(fbxPath, rig);
            AttachToPrefab(PrefabFolder + rig.prefab + ".prefab", fbxPath, controller);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("AnimationSetup : contrôleurs créés et prefabs configurés.");
    }

    private static void LoopClips(string fbxPath, params string[] clipNames)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
        ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clip in clips)
            if (clipNames.Any(n => IsClip(clip.name, n))) clip.loopTime = true;
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static bool IsClip(string assetName, string clipName) => assetName == clipName || assetName.EndsWith("|" + clipName);

    private static AnimationClip FindClip(string fbxPath, string clipName)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>()
            .FirstOrDefault(c => IsClip(c.name, clipName) && !c.name.StartsWith("__preview"));
        if (clip == null) Debug.LogError($"AnimationSetup : clip « {clipName} » introuvable dans {fbxPath}.");
        return clip;
    }

    private static AnimatorController BuildController(string fbxPath, Rig rig)
    {
        string path = $"{ControllerFolder}/{System.IO.Path.GetFileNameWithoutExtension(fbxPath)}.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Walking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = AddState(machine, "Idle", FindClip(fbxPath, rig.idle));
        AnimatorState walk = AddState(machine, "Walk", FindClip(fbxPath, rig.walk));
        AnimatorState hit = AddState(machine, "Hit", FindClip(fbxPath, rig.hit));
        AnimatorState death = AddState(machine, "Death", FindClip(fbxPath, rig.death));
        machine.defaultState = idle;

        // La mort passe avant le coup reçu (ordre des transitions) : un coup fatal joue la mort
        Transition(machine.AddAnyStateTransition(death), 0.1f).AddCondition(AnimatorConditionMode.If, 0, "Death");
        Transition(machine.AddAnyStateTransition(hit), 0.05f).AddCondition(AnimatorConditionMode.If, 0, "Hit");

        AnimatorStateTransition hitEnd = Transition(hit.AddTransition(idle), 0.1f);
        hitEnd.hasExitTime = true;
        hitEnd.exitTime = 1f;

        Transition(idle.AddTransition(walk), 0.15f).AddCondition(AnimatorConditionMode.If, 0, "Walking");
        Transition(walk.AddTransition(idle), 0.15f).AddCondition(AnimatorConditionMode.IfNot, 0, "Walking");
        return controller;
    }

    private static AnimatorState AddState(AnimatorStateMachine machine, string name, AnimationClip clip)
    {
        AnimatorState state = machine.AddState(name);
        state.motion = clip;
        return state;
    }

    private static AnimatorStateTransition Transition(AnimatorStateTransition transition, float duration)
    {
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
        return transition;
    }

    private static void AttachToPrefab(string prefabPath, string fbxPath, AnimatorController controller)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        Transform model = root.transform.Find("Model");
        if (model == null)
        {
            Debug.LogError($"AnimationSetup : {prefabPath} n'a pas d'enfant « Model ».");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        if (!model.TryGetComponent(out Animator animator)) animator = model.gameObject.AddComponent<Animator>();
        animator.avatar = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<Avatar>().FirstOrDefault();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        if (!model.TryGetComponent(out UnitAnimator _)) model.gameObject.AddComponent<UnitAnimator>();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
