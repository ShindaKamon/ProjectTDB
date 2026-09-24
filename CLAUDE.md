# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@Docs/GDD/claude_md_coarchitect.md

Le fichier importé ci-dessus définit le rôle (co-architecte) et résume le jeu. Pour le reste :
- **Design visé (MVP)** : `Docs/GDD/GDD_Main.md` (décisions, questions ouvertes, tableau « source unique de vérité ») et `Docs/GDD/MVP_Excel_Snapshot.md` (chiffres : budget des cartes, bibliothèque, progression, barème monstres).
- **Ce qui est réellement implémenté** : `Docs/GDD/Technical_Specs.md`, section « État du code » — fait foi en cas de conflit entre le code et le design.
- Quand une décision de design change ou que le code diverge, mettre à jour le document de référence concerné (voir `Docs/GDD/README.md`).

Le reste de ce fichier couvre uniquement le côté technique.

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

Équivalent PowerShell : `& "C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults.xml -logFile -`. Code de sortie ≠ 0 si un test échoue ; le détail est dans `TestResults.xml`.

**Éditeur ouvert** : passer par la Unity CLI (`unity`, package `com.unity.pipeline` installé), qui pilote l'éditeur en cours sans le fermer :

```bash
unity status                                   # éditeur connecté, état "ready"
unity command recompile                        # prend en compte les nouveaux fichiers .cs (crée les .meta)
unity command recompile_status                 # attendre "completed", vérifier "errors"
unity command run_tests --mode EditMode [--filter CardPoolQueryTests] --timeout 300 --json
unity command console --level error            # erreurs de la console
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
- `TurnStateMachine` (classe C# pure, pas MonoBehaviour) porte l'état du tour ; `GridManager` orchestre la grille (**carrée** 10×10, positions `Vector2Int`, distance de Manhattan), le spawn et la rotation des tours (un tour par unité dans l'ordre de `_units`, invocations sautées), avec `GridRepository` pour les données de grille.
- Unités : `Unit` (base MonoBehaviour, PV, buffs, marques via `IMarkable`) → `Champion` (abstrait, PA via `IActionPointsUser`) → une sous-classe par champion (`AceUnit`, `AlpinisteUnit`, `SorenUnit`) qui porte ses passifs. Les mécaniques transverses passent par des interfaces opt-in dans `Scripts/Units/` (`IComboTracker`, `IOutgoingDamageModifier`, `IChargeLandingReactor`, `ISummonOwner`) : le code générique teste `if (unit is IXxx)` plutôt que de connaître les champions.
- Ennemis : `Enemy` + `EnemyAI` (un monstre = un `EnemyData` + un prefab, ex. `UnderBed`), qui joue aussi des `CardData`.
- Invocations : `SummonUnit : Unit` (pas de tour propre, PA/PM = 0, pilotée par les cartes de l'invocateur `ISummonOwner`) ; `LyseUnit` en dérive, avec des PV recalculés en continu depuis ceux de Soren.
- États transverses gérés par des classes statiques plutôt que par les unités : `ResourceDebuffManager` (retraits de PA/PM appliqués au début du tour suivant). Les marques (`MarkType` : Poison, AllMarks) sont portées par l'unité (`IMarkable`) ; les valeurs de `MarkType` sont sérialisées dans les cartes, ne pas les renuméroter.

### Cartes
- `CardData` (ScriptableObject, `Cards/CardData.cs`, gros fichier) est **data-driven** : dégâts, ciblage (`CardTargetType`, `CardAreaEffect`), effets (`CardEffectType`), marques, charge, catégorie de slot (`CardCategory` : Signature / Standard / Eveil). La résolution passe par `CardData.ExecuteEffect(source, target, tile, isAdditionalMultiTargetHit)`, appelée par `HandUIController` (joueur) et `EnemyAI`. Une nouvelle carte = en général un nouvel asset, pas une nouvelle classe ; ajouter un champ/enum seulement si l'effet n'est pas exprimable.
- `DeckManager` (sur l'unité) gère pioche/main/défausse et les coûts effectifs (`GetEffectiveCost`, overrides de coût).
- `GameActionValidator` (statique, retourne `ValidationResult`) centralise la validation « peut-on jouer cette carte / se déplacer / cibler » — y ajouter les nouvelles règles plutôt que de disperser les checks dans l'UI. C'est aussi la partie la plus couverte par les tests.

### Données
ScriptableObjects dans `Assets/ScriptableObjects/` (champions, cartes, ennemis, `CardCollection`). Les decks sauvegardés référencent les cartes **par nom** (`GetCardsFromNames`) : renommer un asset `CardData` casse les sauvegardes existantes.

### Logs
Utiliser `GameLog.Log` / `GameLog.LogWarning` (strippés hors éditeur/dev build via `[Conditional]`) au lieu de `Debug.Log`. `Debug.LogError` reste direct.

### Outils éditeur
`Scripts/Editor/UISetupWizard.cs` (génération de hiérarchies UI), `DeckDebugMenu.cs` (reset/inspection des sauvegardes de decks).

## Documentation
`Docs/GDD/` : GDD découpé par système (index : `Docs/GDD/README.md`) — à consulter avant de concevoir une mécanique. Émotions de lancement : Colère, Peur, Joie ; roster MVP : Ace, l'Alpiniste, Soren ; deck cible 24 cartes (2 Signature + 6 Éveil + 16 Standard, le code en gère 18 pour l'instant).
