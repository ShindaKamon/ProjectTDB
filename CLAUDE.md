# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@Docs/GDD/claude_md_coarchitect.md

Le fichier importé ci-dessus définit le rôle (co-architecte), la vision du jeu et l'état de l'implémentation (la section « Mise a jour implementation » y fait foi en cas de conflit). Le reste de ce fichier couvre uniquement le côté technique.

## Projet Unity

- Unity **6000.4.0f1** (`ProjectSettings/ProjectVersion.txt`), URP 17.3, uGUI + TextMeshPro, Input System, Test Framework 1.6.
- Tout le code du jeu est sous `Assets/Project/` ; `Assets/ThirdParty/` et `Assets/TextMesh Pro/` sont des assets importés à ne pas modifier.
- Code et commentaires en français ; classes dans le namespace global (pas de `namespace` malgré le `rootNamespace` de l'asmdef).
- Assemblies : `ProjectTDB` (runtime, `Assets/Project/Scripts`), `ProjectTDB.Editor` (`Scripts/Editor`), `ProjectTDB.Tests.EditMode` (`Assets/Project/Tests/EditMode`, NUnit).
- Les fichiers `.csproj`/`.sln*` à la racine sont générés par Unity — ne pas les éditer.
- Tout asset créé/déplacé doit garder son `.meta` (les GUID sont référencés par les scènes, prefabs et ScriptableObjects).

## Commandes

Pas de build CLI dédié : la compilation se fait via l'éditeur Unity. Lancer les tests EditMode en batchmode (l'éditeur doit être **fermé** sur ce projet) :

```bash
"/c/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe" -batchmode -projectPath . \
  -runTests -testPlatform EditMode -testResults TestResults.xml -logFile -
# Un seul test / une seule classe :
#   ajouter  -testFilter "GameActionValidatorTests"   (nom de classe, ou Classe.Methode)
```

Sinon : Window > General > Test Runner dans l'éditeur.

## Architecture

### Flux de scènes
`ChampionSelectScene` → `CombatScene` (`Assets/Project/Scenes/`). L'éditeur force le Play Mode à démarrer sur ChampionSelectScene (`Scripts/Editor/PlayModeStartSceneSetup.cs`, désactivable via *Tools > Play Mode Start Scene*), car CombatScene dépend d'un état statique posé par la sélection :
- `ChampionSelectManager.SelectedChampion` (static) — `GridManager` instancie `SelectedChampion.prefab` au lancement du combat.
- `DeckSaveManager` (classe statique) — decks persistés en JSON dans `Application.persistentDataPath`, avec cache ; le deck « de base » est resynchronisé depuis les cartes de départ du champion à chaque session.

### Communication entre systèmes
- **ServiceLocator + façade `Services`** (`Scripts/Core/`) : les managers de scène s'enregistrent sous une interface dans leur `Awake` (`IGridService` ← `GridManager`, `IBattleUIService` ← `BattleUIManager`, `IHealthBarService` ← `HealthBarManager`). Accéder via `Services.Grid`, `Services.BattleUI`, etc. plutôt que via des singletons ou `FindObjectOfType`.
- **EventBus** (statique, typé) : événements = classes dérivant de `GameEvent` (`Core/GameEvent.cs`). Toujours appairer `Subscribe`/`Unsubscribe` (typiquement `OnEnable`/`OnDisable`) ; le bus est vidé au chargement du runtime.
- **ComponentLocator** : helpers type `TryGetComponentSafe` pour récupérer des composants sur les unités.

### Combat
- `TurnStateMachine` (classe C# pure, pas MonoBehaviour) porte l'état du tour ; `GridManager` orchestre la grille hex, le spawn et l'alternance joueur/ennemi, avec `GridRepository` pour les données de grille.
- Unités : `Unit` (base MonoBehaviour, PV, buffs, marques via `IMarkable`) → `Champion` (abstrait, PA via `IActionPointsUser`) → une sous-classe par champion (`AceUnit`, `AlpinisteUnit`, `SorenUnit`, `IlyaUnit`, …) qui porte ses passifs. Les mécaniques transverses passent par des interfaces opt-in dans `Scripts/Units/` (`IRageUser`, `IComboTracker`, `IOutgoingDamageModifier`, `IChargeLandingReactor`, `ISummonOwner`) : le code générique teste `if (unit is IXxx)` plutôt que de connaître les champions.
- Ennemis : `Enemy` + `EnemyAI`, qui joue aussi des `CardData`.

### Cartes
- `CardData` (ScriptableObject, `Cards/CardData.cs`, gros fichier) est **data-driven** : dégâts, ciblage (`CardTargetType`, `CardAreaEffect`), effets (`CardEffectType`), marques, Rage (`RageConsumeMode`, `RageScalingType`), charge, catégorie de slot (`CardCategory` : Signature / Standard / Eveil). La résolution passe par `CardData.ExecuteEffect(source, target, tile, isAdditionalMultiTargetHit)`, appelée par `HandUIController` (joueur) et `EnemyAI`. Une nouvelle carte = en général un nouvel asset, pas une nouvelle classe ; ajouter un champ/enum seulement si l'effet n'est pas exprimable.
- `DeckManager` (sur l'unité) gère pioche/main/défausse et les coûts effectifs (`GetEffectiveCost`, overrides de coût).
- `GameActionValidator` (statique, retourne `ValidationResult`) centralise la validation « peut-on jouer cette carte / se déplacer / cibler » — y ajouter les nouvelles règles plutôt que de disperser les checks dans l'UI. C'est aussi la partie la plus couverte par les tests.

### Données
ScriptableObjects dans `Assets/ScriptableObjects/` (champions, cartes, ennemis, `CardCollection`). Les decks sauvegardés référencent les cartes **par nom** (`GetCardsFromNames`) : renommer un asset `CardData` casse les sauvegardes existantes.

### Logs
Utiliser `GameLog.Log` / `GameLog.LogWarning` (strippés hors éditeur/dev build via `[Conditional]`) au lieu de `Debug.Log`. `Debug.LogError` reste direct.

### Outils éditeur
`Scripts/Editor/UISetupWizard.cs` (génération de hiérarchies UI), `DeckDebugMenu.cs` (reset/inspection des sauvegardes de decks) ; helpers de debug runtime dans `Scripts/Debug/`.

## Documentation
`Docs/GDD/` : GDD découpé par système (combat, cartes, grille, émotions, UI/UX…) — à consulter avant de concevoir une mécanique.
