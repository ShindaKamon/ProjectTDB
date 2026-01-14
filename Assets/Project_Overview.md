```markdown
# 1. Project Overview
This Unity project is a **turn-based tactical combat game prototype** separating character selection and battle phases through distinct scenes. The intent appears to be building a combat system focused on emotional-driven character systems, tactical cards, and turn sequencing.

- **Project Type:** Game (Turn-Based Tactical RPG Prototype)
- **Target Platforms:** Standalone Windows 64-bit
- **Core Features / Pillars:**
  - Champion selection and team management interface.
  - Turn-based combat grid system.
  - Card-based skill system using `CardData` scriptable objects.
  - Emotion and transformation system for units and enemies.
  - Modular combat UI and feedback handling.
  - URP-based rendering pipeline supporting smooth UI and particle visuals.

# 2. Gameplay Flow / User Loop
The gameplay loop is organized around two main scenes:

- **ChampionSelectScene:** Player starts here to choose a Champion (Ilya, Astra, Nova, etc.). UI panels display statistics and background lore using TextMeshPro text elements.
- **CombatScene:** After Champion selection, combat occurs between the selected Champion and enemy (UnderBed). During combat, the player interacts with cards, performs attacks, and ends turns until victory or defeat.

**Major States:**
- Champion Selection
- Combat Phase
- Turn Resolution
- Battle End (Results / Return to Menu)

**Core Loop:**
1. Select Champion → Load Combat Scene → Initialize grid and units.
2. Draw and play cards from `DeckManager`.
3. Actions consume Action Points (see `ActionPointsComponent`).
4. Enemies act via `EnemyAI` logic.
5. End turn transitions controlled by `TurnStateMachine`.
6. Repeat until one side's health reaches zero → return or progress.

# 3. Architecture (Runtime + Editor)

- **Runtime Systems:**
  - `ChampionSelectManager` controls champion choice and scene transitions.
  - `GridManager` handles spatial logic for tile-based movement and positioning.
  - `BattleUIManager` oversees dynamic UI panels (hand, targeting, health, boss status).
  - `HealthBarManager`, `HealthOrbController` visualize unit health using layered sprites and masks.
  - `CombatFeedbackManager` shows effects like damage popups and hits.
  - `TurnStateMachine` structures game flow.
  - `EventBus` / `EventBusMonoBehaviour` provide decoupled inter-system communication.
  - `ServiceLocator` and `ComponentLocator` assist runtime dependency resolution.

- **Editor Tooling:** Basic debug and testing helpers under `Project/Scripts/Debug`:
  - `AutoSetupBattleUI` configures scene quickly for test runs.
  - `ForceShowBossUI` and `UIDebugChecker` help visualize UI states.

- **Entry Points:**
  - Default scene: `ChampionSelectScene`.
  - Scene transition trigger: `ChampionSelectManager.OnStartButtonClick()` likely loads the `CombatScene`.

- **Patterns & Communication:**
  - Component-based architecture (Unity MonoBehaviours).
  - Event-driven via custom `EventBus`.
  - Service Locator pattern for modular dependencies.
  - MVC-like separation for UI logic: data (`CardData`, `ChampionData`), view (`UI prefabs`), control (`UI managers`).

# 4. Scene Overview & Responsibilities

| Scene | Purpose | Responsibilities |
|-------|----------|------------------|
| **ChampionSelectScene** | Entry menu allowing champion choice | Displays champion data via `ChampionSelectCanvas`; loads selected champion’s prefab and transitions to combat. Uses `ChampionSelectManager`. |
| **CombatScene** | Core gameplay scene | Hosts grid logic (`GridManager`), unit data initialization, combat UI, targeting, and feedback systems. Persistent managers: `BattleUIManager`, `HealthBarManager`, `CombatFeedbackManager`. |
| **TextMesh Pro Example Scenes** | Imported demo assets | Used for testing TMP text rendering features; unrelated to gameplay logic. |

**Loading Strategy:** Single-scene load from ChampionSelect → Combat via SceneManager API.

**Constraints:** CombatScene objects expect runtime initialization scripts (e.g., deck setup). Avoid opening directly in Editor without setup prefabs.

# 5. UI System

- **Framework:** UGUI + TextMeshPro hybrid system (com.unity.ugui + TMP).
- **Navigation:** Manual event-based UI navigation. `EventSystem` components in both main scenes.
- **Binding Logic:**
  - UI classes bind through GameObject references (`BattleUIManager`, `EnemyCardPreviewUI`, `BossHealthBarUI`).
  - Data-driven via ScriptableObjects (e.g. `ChampionData`, `CardData`).

- **UI Style:**
  - Fonts: LiberationSans SDF from `Assets/Resources/Fonts & Materials`.
  - TextMeshPro for crisp rendering.
  - Sprite-based decorative UI for health orb and card visuals.
  - Layout relies on structured canvases (ChampionSelectCanvas, BattleUICanvas).
  - Consistent shadow and outline using TMP materials.

# 6. Asset & Data Model

- **Asset Style:** Stylized 2D sprites combined with grid elements. Minimalistic combat UI emphasizing clarity.
- **Data Formats:**
  - `ScriptableObjects` for champions, enemies, emotions, and cards.
  - `Prefabs` for UI, champions, enemies, and grid tiles.
  - Textures stored as PNG/JPG; used via Sprite references.
- **Asset Organization:** Strict hierarchical order under `Project/Prefabs`, `Project/Scripts`, `ScriptableObjects`. Each domain: Cards, Characters, Emotions.
- **Naming & Versioning Rules:** PascalCase for ScriptableObjects, same base names as related prefabs; assets grouped by category (ChampionType → FamilyEmotion / Cards / Prefabs).

# 7. Project Structure (Repo & Folder Taxonomy)

**High-Level Layout:**
- `Assets/Project/Scripts` — Gameplay logic grouped per domain.
- `Assets/Project/Prefabs` — GameObject definitions for champions, UI, grid.
- `Assets/ScriptableObjects` — Data definitions for characters and cards.
- `Assets/Settings` — Rendering and volume configurations.
- `Assets/TextMesh Pro` — TMP examples, fonts, materials.
- `Assets/InputSystem_Actions.inputactions` — Input bindings for gameplay/UI.

**Conventions:**
- One script type per file; namespaces group subdomains (Core, Units, UI).
- Prefab naming: hierarchical by category → `ChampionButtonTemplate.prefab`, `HealthBar.prefab`.
- Data/Prefab alignment: `<EntityName>_Base.prefab` matches `<EntityName>.asset` data.

# 8. Technical Dependencies

- **Unity & Pipeline:**
  - Unity Version: 6000.3.2f1 (Unity 6)
  - Render Pipeline: Universal Render Pipeline (URP)
  - Active RP Asset: `PC_RPAsset`

- **Primary Packages:**
  - `com.unity.inputsystem`, `com.unity.ugui`, `com.unity.render-pipelines.universal`
  - `com.unity.shadergraph`, `com.unity.mathematics`, `com.unity.ai.navigation`
  - `com.unity.test-framework`, `com.unity.multiplayer.center` (potential future use)

- **Third-Party Dependencies:** None seen—standard Unity packages only.

- **External Services:** Cloud Build enabled (`com.unity.services.cloud-build`).

# 9. Build & Deployment

- **Build Steps:**
  1. Open Unity Editor → `File > Build Settings`.
  2. Set Platform: Windows 64-bit.
  3. Scenes in Build: `ChampionSelectScene`, `CombatScene`.
  4. Apply PC URP settings and volume profile.
  5. Build artifact created in `/BuildProfiles` per environment.

- **CI/CD:** Cloud Build via Unity Services integrated (build profiles under `Assets/Settings/Build Profiles`).

- **Environment Requirements:** None special; uses standard URP packages and fonts.

# 10. Style, Quality & Testing

- **Code Style:**
  - C# standards: PascalCase for classes, camelCase for fields, explicit private/public modifiers.
  - Folders per domain (UI, Units, Core, Input).
  - Avoid cross-domain references except through service/event bus.

- **Performance Guidelines:**
  - Keep per-frame updates lightweight; use coroutines or event-driven UI updates.
  - Avoid real-time expensive allocations in combat UI.
  - URP optimization via Screen Space Ambient Occlusion disabled for mobile profiles.

- **Testing Strategy:**
  - Unit tests expected under `com.unity.test-framework`.
  - Manual testing in `CombatScene` using Debug scripts.
  - PlayMode tests recommended for turn sequence validation.

- **Validation Rules:** `GameActionValidator.cs` and `ValidationResult.cs` provide precondition enforcement for card or action execution.

# 11. Notes, Caveats & Gotchas

- **Known Issues:** Loading CombatScene directly may result in missing Champion setup (must go through ChampionSelect).
- **Dependency Rules:** Modifying data structures like `ChampionData` requires updating associated prefab setup (`ChampionButtonTemplate`).
- **Deprecated Systems:** None formally marked; TMP example scenes unrelated to gameplay should not be referenced.
- **Platform Caveats:** URP configurations differ between PC and Mobile; preferred asset is `PC_RPAsset` for full lighting and SSAO.
```