# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@Docs/GDD/claude_md_coarchitect.md

Le fichier importé ci-dessus définit le rôle (co-architecte) et la façon de travailler ensemble ; il ne décrit pas le jeu. Pour l'état du jeu :
- **Design visé (MVP)** : `Docs/GDD/GDD_Main.md` (décisions, questions ouvertes, tableau « source unique de vérité ») et `Docs/GDD/MVP_Excel_Snapshot.md` (chiffres : budget des cartes, bibliothèque, progression, barème monstres).
- **Ce qui est réellement implémenté** : `Docs/GDD/Technical_Specs.md`, section « État du code » — fait foi en cas de conflit entre le code et le design.
- Quand une décision de design change ou que le code diverge, mettre à jour le document de référence concerné (voir `Docs/GDD/README.md`).

Le reste de ce fichier couvre uniquement le côté technique.

## Projet Unity

- Unity **6000.4.0f1** (`ProjectSettings/ProjectVersion.txt`), URP 17.3, uGUI + TextMeshPro, Input System, Test Framework 1.6.
- Tout le code du jeu est sous `Assets/Project/` ; `Assets/ThirdParty/` et `Assets/TextMesh Pro/` sont des assets importés à ne pas modifier.
- **Scripts rangés par domaine** : `Core/` (infrastructure seulement), `Grid/`, `Combat/` (tours, retraits de PA/PM), `Cards/`, `Deck/` (construction et sauvegarde), `Units/` (+ `Champions/`, `Enemies/`, `Summons/`), `Validation/` (`GameActionValidator`), `Input/` (`InputManager` : clics/survol → sélection de carte, tuile, cible), `UI/` (`Combat/`, `ChampionSelect/`, `Cards/`, `DeckEditor/`, `Common/`). Un nouveau script va dans le dossier de son domaine, pas dans `Core/`.
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

## Principes de conception (SOLID)

À appliquer quand on écrit, modifie ou relit un `.cs`, **dans la limite de la simplicité** (consignes Karpathy, signaux d'alerte du contrat de co-architecte) : on corrige une violation quand elle gêne vraiment (bug, duplication, fichier qu'on ne sait plus modifier sans casser autre chose), pas pour la beauté du modèle. Une relecture SOLID **signale** d'abord ; on ne refactorise que sur demande.

| Principe | Règle dans ce projet | Signal d'alerte |
|---|---|---|
| **S** — Responsabilité unique | Une classe = une raison de changer. Les règles vont dans des classes C# pures testables (`GameActionValidator`, `DeckRules`, `CardPoolQuery`, `GridGeometry`), l'affichage dans `UI/`, la saisie dans `Input/`. | Classe > ~400 lignes ou méthode > ~80 lignes ; un script d'UI qui applique des règles de jeu (paie des coûts, résout des effets). |
| **O** — Ouvert/fermé | Ajouter une carte = un asset ; ajouter une mécanique de champion = une interface opt-in (`IComboTracker`, `IOutgoingDamageModifier`…) testée par `is`, pas un `if` sur le type concret. | `switch`/`if` sur un type concret (`is AceUnit`) ou sur un nom ; ajout d'un cas qui oblige à modifier plusieurs fichiers génériques. |
| **L** — Substitution de Liskov | Une sous-classe (`Champion`, `Enemy`, `SummonUnit`, `LyseUnit`) respecte le contrat de `Unit` : une surcharge peut ajouter un effet (réduction, passif) mais doit appeler la base et ne pas désactiver le comportement attendu. | Surcharge qui n'appelle pas `base`, méthode qui lève une exception ou ne fait rien dans une sous-classe ; code appelant qui doit connaître la sous-classe. |
| **I** — Ségrégation des interfaces | Interfaces petites et ciblées (`IActionPointsUser`, `ISummonOwner`) ; un service (`IGridService`…) n'expose que ce que ses appelants utilisent. | Interface dont la plupart des implémentations ou des appelants n'utilisent qu'une partie ; membres jamais appelés. |
| **D** — Inversion des dépendances | Les systèmes se parlent via `Services.X` (interfaces), l'`EventBus` ou des classes C# pures ; pas de `FindObjectOfType`, pas de singleton concret, pas de référence directe d'un système de jeu vers une classe d'UI. | Un système de jeu (`Unit`, `CardData`, `GridManager`) qui appelle une UI concrète ; `new` d'un manager dans une classe métier ; accès statique à l'état d'une autre scène. |

## Architecture

### Flux de scènes
`ChampionSelectScene` → `CombatScene` (`Assets/Project/Scenes/`). L'éditeur force le Play Mode à démarrer sur ChampionSelectScene (`Scripts/Editor/PlayModeStartSceneSetup.cs`, désactivable via *Tools > Play Mode Start Scene*), car CombatScene dépend d'un état statique posé par la sélection :
- `ChampionSelectManager.SelectedChampion` (static) — `GridManager` instancie `SelectedChampion.prefab` au lancement du combat.
- `DeckSaveManager` (classe statique) — decks persistés en JSON dans `Application.persistentDataPath`, avec cache ; le deck « de base » est resynchronisé depuis les cartes de départ du champion à chaque session.

### Communication entre systèmes
- **ServiceLocator + façade `Services`** (`Scripts/Core/`) : les managers de scène s'enregistrent sous une interface dans leur `Awake` (`IGridService` ← `GridManager`, `IBattleUIService` ← `BattleUIManager`, `IHealthBarService` ← `HealthBarManager`). Accéder via `Services.Grid`, `Services.BattleUI`, etc. plutôt que via des singletons ou `FindObjectOfType`.
- **EventBus** (statique, typé) : événements = classes dérivant de `GameEvent` (`Core/GameEvent.cs`). Toujours appairer `Subscribe`/`Unsubscribe` (typiquement `OnEnable`/`OnDisable`) ; le bus est vidé au chargement du runtime.
- **Couleurs des émotions** : une seule palette, `CodexCardVisual.EmotionColor` / `EmotionName` (`Cards/CodexCardVisual.cs`) ; ne pas redéfinir de couleurs d'émotion ailleurs.
- **ComponentLocator** : helpers type `TryGetComponentSafe` pour récupérer des composants sur les unités.

### Combat
- `TurnStateMachine` (classe C# pure, pas MonoBehaviour) porte l'état du tour ; `GridManager` orchestre la grille (**carrée** 10×10, positions `Vector2Int`, **4 directions**, distance de Manhattan — toute distance, voisinage ou direction passe par `GridGeometry`, ne pas recalculer à la main, écho du Miroir fraternel compris), le spawn et la rotation des tours (un tour par unité dans l'ordre de `_units`, les unités dont `TakesTurns` est faux — les invocations — sont sautées), avec `GridRepository` pour les données de grille.
- Unités : `Unit` (base MonoBehaviour, PV, bouclier, armure/barrière, buffs) → `Champion` (abstrait, PA via `IActionPointsUser`) → une sous-classe par champion qui porte ses passifs (voir « Noms des champions » ci-dessous). Les mécaniques transverses passent par des interfaces opt-in dans `Scripts/Units/` (`IComboTracker`, `IOutgoingDamageModifier`, `IChargeLandingReactor`, `ISummonOwner`) : le code générique teste `if (unit is IXxx)` plutôt que de connaître les champions.
- Ennemis : `Enemy` + `EnemyAI` (un monstre = un `EnemyData` + un prefab, ex. `UnderBed`), qui joue aussi des `CardData`.
- Invocations : `SummonUnit : Unit` (pas de tour propre, PA/PM = 0, pilotée par les cartes de l'invocateur `ISummonOwner`) ; `LyseUnit` en dérive, avec des PV recalculés en continu depuis ceux d'Evan.
- États transverses gérés par des classes statiques plutôt que par les unités : `ResourceDebuffManager` (retraits de PA/PM appliqués au début du prochain tour de la cible, sans cumul : le plus fort l'emporte). Les enums sérialisés dans les assets (`DamageType`, `CardTargetType`, `CardAreaEffect`, `FilterChipKind`…) : ajouter les nouvelles valeurs à la fin, ne jamais renuméroter.

### Cartes
- `CardData` (ScriptableObject, `Cards/CardData.cs`, gros fichier) est **data-driven** : dégâts, ciblage (`CardTargetType`, `CardAreaEffect`), type de dégâts (`DamageType`), poussée/tirage, charge, catégorie de slot (`CardCategory` : Signature / Standard / Eveil). La résolution passe par `CardData.ExecuteEffect(source, target, tile, isAdditionalMultiTargetHit)`, appelée par `HandUIController` (joueur) et `EnemyAI`. Une nouvelle carte = en général un nouvel asset, pas une nouvelle classe ; ajouter un champ/enum seulement si l'effet n'est pas exprimable.
- `DeckManager` (sur l'unité) gère pioche/main/défausse et les coûts effectifs (`GetEffectiveCost`, overrides de coût).
- `GameActionValidator` (statique, retourne `ValidationResult`) centralise la validation « peut-on jouer cette carte / se déplacer / cibler » — y ajouter les nouvelles règles plutôt que de disperser les checks dans l'UI. C'est aussi la partie la plus couverte par les tests.
- Même principe pour la construction de deck : `DeckRules` (statique, C# pur, `Deck/`) porte les règles (couleurs du deck, Signatures, exemplaires max) ; les emplacements par catégorie sont `DeckData.SIGNATURE_SLOTS` / `STANDARD_SLOTS`.

### Données
ScriptableObjects dans `Assets/ScriptableObjects/` (champions, cartes, ennemis, `CardCollection`). Les decks sauvegardés sont rangés **par nom de champion** et référencent les cartes **par nom** : renommer un champion (`championName`) ou une carte (`cardName`) exige d'ajouter l'ancien nom dans `RenamedChampions` / `RenamedCards` de `DeckSaveManager`, sinon les decks existants perdent le champion ou la carte.

### Noms des champions
Le roster a été renommé le 24/09/2026 : seuls les noms affichés (`ChampionData.championName`) ont changé, pas les identifiants internes.

| Nom (jeu et GDD) | Classe | Fiche / prefab |
|---|---|---|
| Evan (+ invocation Lyse) | `SorenUnit` | `Soren.asset`, `Soren_Base.prefab` |
| Crux | `AlpinisteUnit` | `Alpiniste.asset`, `Alpiniste_Base.prefab` |
| Raze | `AceUnit` | `Ace.asset`, `Ace_Base.prefab` |

### Logs
Utiliser `GameLog.Log` / `GameLog.LogWarning` (strippés hors éditeur/dev build via `[Conditional]`) au lieu de `Debug.Log`. `Debug.LogError` reste direct.

### Outils éditeur
`Scripts/Editor/UISetupWizard.cs` (génération de hiérarchies UI), `DeckDebugMenu.cs` (reset/inspection des sauvegardes de decks).

## Documentation
`Docs/GDD/` : GDD découpé par système (index : `Docs/GDD/README.md`) — à consulter avant de concevoir une mécanique. Émotions de lancement : Colère, Peur, Joie ; roster MVP : Evan, Crux, Raze ; deck cible 24 cartes (2 Signature + 6 Éveil + 16 Standard, le code en gère 18 pour l'instant, 2 + 16, sans emplacements Éveil).
