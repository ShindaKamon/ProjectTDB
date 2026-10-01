# 🔧 Spécifications Techniques - Émotions Tactics (Project TDB)

**Version:** 2.9
**Date:** 30 Septembre 2026
**Statut:** Reflète l'architecture actuelle.
**Changements :**
- v2.1 (10/09/2026) : retrait des mentions Classes et Éléments.
- v2.2 (23/09/2026) : section Système d'Émotions mise à jour (la jauge universelle -100/+100 existe dans le code mais n'est pas utilisée par le design pour l'instant ; les mécaniques signatures comme la Rage d'Ilya sont la cible) ; plateforme mobile signalée comme question ouverte.
- v2.3 (23/09/2026) : écarts entre le code et l'Excel MVP listés (section « Adaptations à prévoir »).
- v2.4 (24/09/2026) : nettoyage hors MVP — code d'Ilya (Rage) et de Vylos (Stigmate) retiré, assets morts supprimés, prefabs champions sortis des dossiers de familles ; pool de l'éditeur de deck façon SpamDex.
- v2.5 (24/09/2026) : règle de grille unifiée dans `GridGeometry` : 4 directions (Manhattan) partout, la portée euclidienne de certaines validations est supprimée ; IA ennemie sans contrainte d'alignement ; écho du Miroir fraternel compris.
- v2.6 (24/09/2026) : scripts rangés par domaine (Core = infrastructure, Grid, Combat, Units/Champions|Enemies|Summons, UI/Combat…) ; palette des émotions unique (`CodexCardVisual`).
- v2.7 (26/09/2026) : protections (bouclier en PV, armure / résistance magique, `DamageType`), texte des cartes généré (`CardRulesText`) et pastilles d'icônes, noms du code en anglais, revue Colère / Joie / Peur, anti-lock et Ténacité des monstres, ATQ option B (`nextAttackBonus`), bond (`leapToTarget`, Bond percutant), Tapis à 2 cibles ; écarts restants avec l'Excel dans « Adaptations à prévoir » (ligne Cartes MVP).
- v2.8 (28/09/2026) : identifiants internes des champions renommés (`EvanUnit`, `CruxUnit`, `RazeUnit`, fiches, prefabs, matériaux) ; zone `Line` partant de la case visée, aperçu de zone fidèle à la forme réelle ; ciblage en ligne droite (`targetInStraightLine`) ; ciblage par case (les unités ne masquent plus la case derrière elles) ; bouclier sans durée, affiché sur l'orbe de vie ; textes flottants des bonus/malus (`UnitEffectAppliedEvent`), faux critiques retirés ; Signatures en 2 exemplaires (deck de 20 cartes), deck incomplet non jouable ; Invocation de Lyse cible Lyse pour la soigner ; poussée sans demi-tour.
- v2.9 (30/09/2026) : donjon d'exploration (`Dungeon/`), modèles Quaternius animés, HUD de combat, ATQ / armure / RM par niveau, suppression du rôle affiché sur les cartes ; audit : scènes et taille de main corrigées, méthodes mortes retirées (`CanEnemyAct`, `GetGridDimensions`, `UnitState.CanAct`, `CombatFeedbackManager.ShowDamage/ShowHeal/ShowImmune`), tests `GridRepository` et `PendingEffects`.

---

## 🎮 Moteur et Technologies

### Plateforme de Développement

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Moteur** | Unity | 6000.4.0f1 (Unity 6) |
| **Langage** | C# | .NET Standard 2.1 |
| **Version Control** | Git | Dernière version |
| **IDE** | Visual Studio / JetBrains Rider | 2022+ |

### Packages Unity Utilisés

| Package | Utilisation | Statut |
|---------|-------------|--------|
| **TextMeshPro** | Rendu de texte haute qualité | Actif |
| **Input System** | Gestion des entrées utilisateur | Actif |
| **Universal Render Pipeline (URP)** | URP 17.3 (post-processing ; piste pour la désaturation des donjons, voir `UI_Design.md`) | Actif |
| **Test Framework** | Tests EditMode NUnit (`Assets/Project/Tests/EditMode`) | Actif |
| **Cinemachine** | Gestion de caméra | Optionnel |

---

## 🏗️ Architecture du Projet

### Structure des Dossiers

| Dossier | Contenu |
|---------|---------|
| **Assets/Project/Scripts/Core/** | Infrastructure seulement : `ServiceLocator`/`Services`, `EventBus`/`GameEvent`, `GameLog`/`GameLogConfig`, `ComponentLocator` |
| **Assets/Project/Scripts/Grid/** | `GridManager` (grille, spawn, rotation des tours), `GridRepository`, `GridGeometry` (distance en 4 directions), `IGridService`, `Tile` |
| **Assets/Project/Scripts/Combat/** | `TurnStateMachine`, `ResourceDebuffManager` (retraits de PA/PM) |
| **Assets/Project/Scripts/Cards/** | `CardData` (ScriptableObject data-driven + enums de cartes), `ChargeHelper`, `CodexCardVisual` (visuels et palette des émotions), `DeckManager` (pioche/main/défausse en combat) |
| **Assets/Project/Scripts/Deck/** | Construction et sauvegarde des decks : `DeckData`, `DeckRules`, `DeckSaveManager`, `CardPoolQuery`, `ChampionDecksData`, `AllDecksData`, `CardCollection` |
| **Assets/Project/Scripts/Units/** | `Unit`, `UnitState`, `ActionPointsComponent`, interfaces (`IActionPointsUser`, `IComboTracker`, `IOutgoingDamageModifier`, `IContactReactor`, `ISummonOwner`) ; `Champions/` (`Champion`, `ChampionData`, `RazeUnit`, `CruxUnit`, `EvanUnit`), `Enemies/` (`Enemy`, `EnemyAI`, `EnemyData`), `Summons/` (`SummonUnit`, `LyseUnit`) |
| **Assets/Project/Scripts/Validation/** | `GameActionValidator`, `ValidationResult` |
| **Assets/Project/Scripts/Input/** | `InputManager` |
| **Assets/Project/Scripts/Network/** | `NetworkSession` (Netcode for GameObjects 2.13 + Unity Transport, port 7777 : héberger / rejoindre, messages nommés du salon, chargement des scènes pour tous), `LobbyState` (salon réseau, C# pur, testé). Salon et lancement commun, commandes de combat relayées par l’hôte, empreinte d’état par tour, désynchronisation (interruption) et déconnexion en combat (message `tdb.combat.left`, `NetworkSession.LeaveToMenu`) |
| **Assets/Project/Scripts/UI/** | `Combat/` (HUD, `BattleUIManager`, `HealthBarManager`, orbe de vie, barres de vie, retours de combat), `ChampionSelect/` (écrans de sélection et de decks, `ChampionSelectManager`, `ChampionSelectFlowController`), `Cards/` (main de cartes, ciblage), `DeckEditor/`, `Common/` |
| **Assets/Project/Scripts/Editor/** | `UISetupWizard`, `DeckDebugMenu`, `PlayModeStartSceneSetup` |
| **Assets/Project/Tests/EditMode/** | Tests NUnit (`GameActionValidatorTests`, `DeckManagerCostOverrideTests`, `ValidationResultTests`) |
| **Assets/Project/Scenes/** | `MainMenuScene`, `ChampionSelectScene`, `ExplorationScene`, `CombatScene` |
| **Assets/ScriptableObjects/** | Champions, cartes (Standard/Colere, Peur, Joie ; Champion/… ; Enemy/… ; Family/… reliquat), ennemis |
| **Docs/GDD/** | Ce GDD |

Guide technique détaillé pour Claude Code : `CLAUDE.md` à la racine du repo.

### Conventions de Nommage

#### Scripts C#

| Type | Convention | Exemple |
|------|------------|---------|
| **Classes** | PascalCase | CardUIElement, HandUIController |
| **Méthodes** | PascalCase | UpdateCurve, HandleCardClicked |
| **Variables privées** | _camelCase avec underscore | _cardData, _isSelected |
| **Variables publiques** | camelCase | cardName, costPA |
| **Constantes** | UPPER_SNAKE_CASE | MAX_HAND_SIZE, DEFAULT_PA |

> Taille de main : 5 (actée le 24/09, voir `Combat_System.md`) — gardée paramétrable (`DeckManager._maxHandSize`). Budget PA+PM = 9 ; deck de 20 cartes dans le code (24 dans l’Excel, voir `Card_System.md`).

#### Fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **Scènes** | PascalCase | MainMenu, Combat_Level01 |
| **Prefabs** | PascalCase | CardUI_Template, HexTile |
| **ScriptableObjects** | PascalCase avec suffixe | Card_CoupDeColereData, Character_EvanData |

---

## 📦 Systèmes Principaux

### Patterns de Conception Utilisés

| Pattern | Utilisation | Bénéfice | Fichiers |
|---------|-------------|----------|----------|
| **Service Locator** | Accès global aux services | Découplage, testabilité | Services.cs |
| **Event Bus** | Communication entre systèmes | Découplage total | EventBus.cs, GameEvent.cs |
| **State Machine** | Gestion des tours et états | Transitions validées | TurnStateMachine.cs, UnitState.cs |
| **Component Pattern** | Composition d'entités | Réutilisation de code | ActionPointsComponent.cs |
| **Repository Pattern** | Accès optimisé aux données | Performance | GridRepository.cs |
| **ScriptableObject** | Données séparées du code | Édition facile, partage | CardData, ChampionData, etc. |

---

### 1. Grille

- `GridManager` (Core) : génère une **grille carrée 10×10**, instancie le champion sélectionné et les ennemis, orchestre la rotation des tours ; enregistré comme `IGridService`.
- `GridRepository` : accès aux tuiles et unités par `Vector2Int`.
- `Tile` : une case.
- Distances : 4 directions (Manhattan), via `GridGeometry`, écho du Miroir fraternel compris (voir `Grid_System.md`).

### 2. Cartes et decks

- `CardData` : ScriptableObject **data-driven** (dégâts, ciblage `CardTargetType`, zone `CardAreaEffect`, type de dégâts `DamageType`, poussée/tirage (`knockbackDistance`), charge (ligne droite), bond (`leapToTarget` : saut sur une case vide, zone à l'arrivée), émotion `EmotionType`, catégorie `CardCategory` Standard/Eveil/Signature). Résolution via `CardData.ExecuteEffect(...)`, appelée par `HandUIController` et `EnemyAI`. Une nouvelle carte = un nouvel asset.
- `DeckManager` (sur l'unité) : pioche, main, défausse, coûts effectifs (`GetEffectiveCost`, overrides de coût pour Raze).
- `DeckData` : 4 slots Signature (2 exemplaires de chacune des 2 Signatures) + 16 Standard (les 6 slots Éveil ne sont pas encore ajoutés) ; `DeckSaveManager` : sauvegarde JSON, 1 deck de base + 3 decks perso par champion. Les decks référencent les cartes **par nom**.

### 3. UI de cartes

- `HandUIController`, `CardUIElement` : main en arc (style Limbus Company), hover, sélection.
- `TargetingCurve`, `TargetingReticle` : Unity Graphic custom (courbe de Bézier, réticule), sans allocation GC dans `OnPopulateMesh`.

### 4. Combat

- `TurnStateMachine` (classe C# pure) : états Initializing / PlayerTurn / EnemyTurn / TransitioningTurn / BattleEnd.
- Rotation : un tour par unité dans l'ordre de `_units` (invocations sautées). Au début du tour : PM et PA rafraîchis, 1 carte piochée.
- `GameActionValidator` : centralise les règles « peut-on jouer / cibler / se déplacer » (le plus couvert par les tests), dont le ciblage en ligne droite (`CardData.targetInStraightLine`, ex. Éclat de rage) et le soin de l'invocation déjà présente par sa carte d'invocation (`HealsActiveSummon`).
- Zones (`CardData.IsInAOEShape`, seule source de la forme, pour l'effet comme pour l'aperçu de `GridManager.ShowAOEZone`) : `Line` part de la case visée et s'éloigne du lanceur (`aoeRadius` cases, cible comprise).
- Saisie (`InputManager.TryGetPointedObject`) : le rayon de la souris traverse les unités et vise la case ; l'unité posée dessus est la cible.
- Déplacements subis (poussée, tirage, recul) : l'unité garde son orientation.
- Bouclier (`Unit.AddShield`) : sans durée, consommé par les dégâts avant les PV ; seul le bouclier réactif non déclenché expire au prochain tour du lanceur.
- Retours visuels (`CombatFeedbackManager`) : dégâts en rouge, soins en vert, bonus/malus en texte flottant (`UnitEffectAppliedEvent`, couleurs des pastilles), empilés au-dessus du chiffre de dégâts ; l'écho de Lyse s'affiche sur la cible et Lyse se tourne vers elle.

### 5. Émotions

> ⚠️ Il **n'y a pas** de `EmotionSystem` dans le code (contrairement aux anciennes versions de ce document). Les émotions existent comme **identité des cartes** (`EmotionType` sur `CardData`) ; chaque deck a 1 ou 2 couleurs (choisies à sa création) qui filtrent le pool du gestionnaire (`DeckRules.DeckColors`). `CardCategory.Awakening` n'est plus utilisée.

**Éveil (fusion champion × émotion) — les 9 formes codées (30/09/2026)** :
- `EmotionGauge` (`Combat/`, classe pure) : une jauge par émotion (2 points par palier, 3 paliers max), une seule fusion à la fois, −1 palier au début du tour du champion fusionné, fin à 0. Portée par `Champion.Gauge` ; `Champion.OnCardPlayed` ajoute `GaugePointsPerCard` (1) point à l'émotion de chaque carte payée (`CombatCommandExecutor.PayCosts`) et, pendant une fusion, retire ce même nombre de points à la jauge fusionnée pour une carte d'une autre émotion (`EmotionGauge.DrainActiveFusion` ; fin de fusion à 0 ; cartes neutres exemptées).
- `FusionData` (`Units/Champions/`, ScriptableObject abstrait) : un asset par couple champion × émotion, listé dans `ChampionData.fusions` ; hooks `OnActivated`, `OnEnded`, `OnTurnStart`, `OnEnemiesHit`, `OnEnemyAttacked` (chaque attaque sur un ennemi, avant défense), `OnDisplacement` (charge, bond : case d'arrivée et nombre de cases), `OnComboPattern` (motif de Main gagnante, via `IComboTracker.CurrentPattern`) et `ReplaceSummonEcho` (remplace l'écho de Lyse), appelés par `CardData` et `RazeUnit`. Formes : `DualStrikeFusion` (Deux en un), `EchoHealFusion`, `LureFusion` (Appât), `AvalancheFusion`, `AscensionFusion`, `AllInFusion`, `ShareGainsFusion`, `TempoFusion`, `MovementStealFusion` ; un asset par couple dans `Assets/ScriptableObjects/Characters/Fusions/`. `FusionZones` (`Combat/`, statique) : zones d'Appât appliquées au début du tour par `GridManager` ; `Unit.Despawn` / `SetCurrentHealth` et `EvanUnit.AbsorbSummon` / `ReleaseSummon` servent Deux en un. Valeurs et limites : `SYSTEME_EMOTIONS.md`.
- Activation gratuite par la commande `CombatCommand.ActivateFusion` (validée par `GameActionValidator.CanActivateFusion`), boutons dans `FusionPanelUI` (posé sur `ChampionStatsPanel`), un par émotion présente dans le deck (`DeckManager.DeckEmotions`), empilés l'un au-dessus de l'autre pour un deck bi-émotion ; `FusionChangedEvent` rafraîchit l'affichage ; `FusionAura` (posée à la volée sur le champion) dessine une aura aux couleurs de l’émotion (lueur autour du corps et au sol : douce quand la fusion est prête, plus vive et plus haute pendant la fusion, éclat à l’activation) ; l'empreinte réseau (`CombatStateFingerprint`) inclut jauges et compteur de tour.
- **Reste à coder** : les 8 autres formes (Evan ×3, Crux Ascension/Avalanche, Raze ×3), probablement avec de nouveaux hooks (modification de dégâts, absorption d'invocation, soins de déplacement).

---

## 🧭 État du code (vérifié le 30/09/2026)

Cette section remplace l'ancienne « Mise à jour implémentation » de `claude_md_coarchitect.md` et fait foi pour décrire ce qui existe **dans le code**.

**Roster jouable :** Raze (« Le Tricheur »), Crux (« Le Grimpeur »), Evan (« Le Frère », + invocation Lyse), référencés dans `ChampionSelectManager._allChampions`. Seuls ces 3 champions existent (fiches dans `Assets/ScriptableObjects/Characters/Champion/`, prefabs dans `Assets/Project/Prefabs/Champions/`). Les 3 champions MVP ont 100 PV ; PA/PM : Evan 5/4, Crux 4/5, Raze 6/3. Chacun a un passif (`ChampionData.passiveName` / `passiveDescription`).

**Ilya, Vylos, Calyx (hors MVP) : retirés du code le 24/09/2026.** Ilya y avait une version différente de `archive/ilya_deck_simple.md` (carte Rage ajoutée à la main tous les 10 dégâts subis, stock max 5) ; Vylos portait la marque Stigmate. Le code reste consultable dans l'historique git (commit `00afe5d`, dernier état avant le nettoyage) si Ilya revient, sa Rage étant à réadapter à l'Éveil.

**Marques (poison…), partage de dégâts, recherche/ajout de cartes (hors MVP) : retirés du code le 25/09/2026**, aucune carte ne les utilisait. Récupérables dans l'historique git (dernier état : commit `d169a9d`) le jour où les statuts arriveront (voir « Statuts prévus hors MVP » dans `Combat_System.md`).
**Nettoyage du 25/09/2026** : retirés aussi l'état « étourdi » et l'état « en action » de `UnitState` (statuts hors MVP), `CardEffectType` (la poussée dépend seulement de `knockbackDistance`), `movementAmount` (sans effet), les anciennes lignes de stats de la sélection (`StatDisplayUI`), une trentaine de méthodes jamais appelées, et les packages inutilisés (AI Navigation, Rider, modules Terrain, Cloth, Vehicles, Wind, VR/XR, Video, Tilemap, Physics 2D, Umbra, Vector Graphics, Adaptive Performance, Analytics, Android JNI). Gardés volontairement : `TurnStateMachine.EndBattle` / `IsBattleOver` (victoire et défaite à venir) et `costHP` (mécanique fonctionnelle, aucune carte ne l'utilise encore). Corrigé au passage : les retraits de PA/PM n'étaient jamais appliqués.

**Cartes :** 51 cartes Standard (17 Colère, 17 Peur, 17 Joie) : la bibliothèque de l'Excel plus Étincelle et Rire lumineux, et 6 cartes refondues et renommées le 29/09 (voir `Card_System.md`) ; Signatures des 3 champions (les anciens assets « Family » ont été supprimés).

**Deck :** 20 cartes (4 Signature + 16 Standard, `DeckData`) ; multi-deck : 1 deck de base (non supprimable, resynchronisé depuis les cartes de départ du champion à chaque session) + jusqu'à 3 decks perso (`MAX_CUSTOM_DECKS = 3`) . Règles (`DeckRules`) : cartes des couleurs du deck uniquement (1 ou 2, choisies à sa création parmi `DeckRules.AvailableEmotions` = Colère, Peur, Joie pour le MVP ; le deck de base prend celles de ses cartes), 4 exemplaires max par carte, les 2 Signatures du champion obligatoires (2 exemplaires chacune, `DeckRules.MAX_SIGNATURE_COPIES`) ; les Signatures des autres champions sont interdites ; un deck existant non conforme est corrigé à son chargement (exemplaires en trop retirés, Signatures ajoutées) ; les couleurs d'un deck perso se changent par le bouton « Couleurs » (`CreateDeckPopup.ShowEdit`, `DeckSaveManager.SetDeckColors`) et les cartes hors couleurs sont gardées, en rouge dans l'éditeur, le deck restant injouable tant qu'il en reste (`DeckRules.CountOffColor` / `IsPlayable`).

**Combat :** grille carrée 10×10 en 4 directions (Manhattan, pour le déplacement, la portée, les zones et les charges) ; un tour par unité ; main de départ 5, max 5 en fin de tour (l'excédent se défausse au choix, `HandDiscardRequiredEvent`), pioche sans limite pendant le tour, 1 carte piochée par tour ; 1 boss (UnderBed, 175 PV, armure et résistance magique 4, pattern visible Marée d'ombre → Agrippe → Marée d'ombre → Tapi dans le noir, l'attaque de base restant aussi sa riposte anti-contrôle ; aperçu des cartes ennemies retourné à chaque carte jouée) et 2 Moutons de poussière (50 PV) ; champions à 100 PV, PA/PM selon leur profil (Evan 5/4, Crux 4/5, Raze 6/3). **Coop locale** (27/09/2026) : 1 à 3 joueurs sur un seul PC, un champion et un deck chacun, champions uniques (`CombatParty`) ; les champions jouent dans l'ordre d'inscription, un champion mort est retiré et son tour sauté ; PV et dégâts des monstres adaptés au nombre de joueurs (`EnemyScaling`, via `Enemy.ScaleForPlayers` et `IOutgoingDamageModifier`). Phase de placement avant le premier tour (`PlacementPhase` + règles en C# pur `PlacementBoard`) : 6 cases de départ (`GridManager._startCells`), placement à tour de rôle : le champion du joueur courant va sur la case rouge libre cliquée, « Joueur suivant » puis « Lancer le combat », « Lancer le combat » ; pas d'unité active pendant le placement ; l'interface de combat (main, pioche/défausse, fin de tour, HUD, orbe, indicateur de tour : liste `PlacementPhase._combatOnlyUI`) est masquée, seules la barre du boss et sa carte prévue restent visibles. Victoire (tous les ennemis vaincus) et défaite (tous les champions vaincus) : `BattleOutcome`, écran de fin `BattleEndUI` avec récapitulatif des dégâts et soins (`CombatStats`).

**Donjon et exploration** (30/09/2026, `Scripts/Dungeon/`) : l'Orphelinat est jouable en solo et en coop locale (pas en réseau). « Commencer » lance `ExplorationScene` (via `DungeonRun.BeginFromResources`, donjon chargé depuis `Resources/Dungeons/Orphelinat`) au lieu de `CombatScene`. `ExplorationController` construit la salle courante à l'exécution (grille 9×7 avec le prefab de case, la caméra isométrique, le fond et la lumière du combat ; portes en formes simples ; décor `RoomDecor` : murs en pierre (Quaternius Modular Dungeons `Wall_Modular`, pilier d'angle `Column`, fenêtres House Interior `Window_Small2`, vitre bleu nuit `Materials/WindowGlass` par remap d'import ; porte Furniture `Door3` ; passés par les champs `_wallModel` / `_cornerModel` / `_windowModel` / `_doorModel` d'`ExplorationController`) sur les deux côtés éloignés de la caméra — la salle de combat a les mêmes murs, sans porte (`GridManager.GenerateGrid` appelle `RoomDecor.Build`, mêmes champs) ; à chaque porte un battant fermé tant que la salle n’est pas vidée (porte `Door3` ; une porte d'un côté ouvert, proche de la caméra, n'est pas dessinée), monté sur un pivot « Leaf » posé sur ses gonds, qui pivote d'un quart de tour vers l'extérieur (`RoomDecor.OpenRotation`) en 0,9 s juste après le dernier combat (`DungeonRun.ConsumeDoorsJustOpened`), puis dalle jaune et étiquette rendent la porte cliquable ; une salle déjà vidée s’affiche directement ouverte) : un pion d'équipe (le modèle « Model » des prefabs de chaque champion du groupe ; le groupe de monstres, celui de son premier ennemi) marche case par case au clic (`ExplorationPathfinder`, BFS en 4 directions, les groupes de monstres bloquent) ; cliquer un groupe fait marcher le pion à côté puis lance le combat, cliquer une porte change de salle. `DungeonRun` (statique, comme `CombatParty`) garde le donjon, la salle, la case du pion et les rencontres vaincues ; `GridManager.SpawnEncounter` remplace les monstres de la scène par ceux de `EncounterData` (ennemi + case). Après une victoire, `BattleEndUI` propose « Continuer » (retour à l'exploration, groupe retiré) ; une défaite propose « Rejouer » (même combat). Un panneau latéral (`MonsterPanel`) affiche une fiche par type de monstre des groupes restants (« Mouton de poussière × 2 ») : icône (le modèle rendu une fois dans une texture, `EnemyData` n’ayant pas d’illustration), PV (adaptés au nombre de joueurs), PA, PM, armure et résistance magique. Le prefab de case (`Prefabs/Grid/Tile.prefab`, combat et exploration) porte un sol en pierre (enfant « Floor », Quaternius `Floor_Modular` à l'échelle 0,5) sous un calque translucide « Highlight » (`TileMaterial` transparent) : le damier est une teinte légère (blanc 6 % / noir 25 %), les surbrillances de `Tile.SetColor` le recouvrent. Les modèles 3D sont des assets low-poly CC0 de Quaternius (`Assets/ThirdParty/Quaternius/`) : Cute Animated Monsters (Cthulhu pour UnderBed, Ghost pour le Mouton de poussière), Ultimate Modular Men/Women (Evan, Crux, Raze, Lyse) Modular Dungeons (sol, murs, pilier), Ultimate House Interior (fenêtre) et Furniture (porte) ; ils sont animés en Idle / Walk / Hit / Death : un `AnimatorController` par modèle (`Assets/Project/Animations/`, généré par le menu *Tools > Animations > Configurer les modèles* = `AnimationSetup`, qui pose aussi `Animator` + `UnitAnimator` sur l'enfant « Model » des prefabs ; Cthulhu, sans clip de repos ni de marche, joue « Flying » dans les deux cas). `UnitAnimator` suit l'unité parente (marche via `IsMoving()`, coup reçu via `UnitDamagedEvent`, mort via `UnitDiedEvent` : le modèle se détache, joue Death et disparaît 2 s plus tard) ; en exploration, `ExplorationController` pilote la marche des pions avec `SetWalking`. L'écran de sélection affiche le modèle en Idle animé (`ChampionModelPreview` : caméra + `RenderTexture` hors scène, dans une `RawImage` posée sur l'illustration) ; les PNG `Textures/Champions/FullBody` (rendus de ces modèles) servent de secours. Les portraits carrés (`Textures/Portraits/`, buste pour les champions, corps entier pour les ennemis) sont générés par *Tools > Portraits > Générer* (`PortraitGenerator`) et affectés à `ChampionData.portrait` / `EnemyData.portrait` : ils servent dans la frise des tours, le panneau d'équipe (`UnitPortraitView`) et les listes de la sélection. Les portes d'une salle n'apparaissent qu'une fois tous ses groupes vaincus (`DungeonRun.IsRoomCleared`). Donjon terminé quand tous les groupes sont vaincus (écran IMGUI provisoire → Menu principal). Contenu : 3 salles en enfilade, une seule porte par salle vers la suivante, sans retour en arrière (le dortoir : 2 moutons ; le couloir : 3 moutons ; sous le lit : UnderBed + 2 moutons), créé par le menu `Tools > Donjon > Créer l'Orphelinat` (`DungeonSetup`). PV remis à niveau à chaque combat (pas de report). Pas encore : désaturation/couleur, récompenses, décors, exploration en réseau, interface définitive.

**Écrans construits :**
- Sélection de champion (`Screen_ChampionSelect`) : roster à gauche, illustration au centre (`ChampionData.fullBodyArt`, art provisoire), panneau de droite (`ChampionStatsUI`) avec nom, titre, passif (`ChampionData.passiveName` / `passiveDescription`, résumé aussi affiché sous les stats en combat ; l'histoire `description` n'est plus affichée), statistiques et bouton « Choisir ce champion ». Bouton « Retour » : menu principal en solo, salon en multijoueur.
- Menu principal (`MainMenuScene`, `MainMenuController`) : Jouer (solo), Multijoueur → Même PC (salon local, coop sur un seul PC) / Héberger / Rejoindre (réseau local, adresse IP de l'hôte, port 7777 ; `NetworkSession`), Quitter. Première scène des Build Settings et scène de départ du Play Mode.
- Salon local (`Screen_Lobby`, `LobbyUI` + 3 `LobbySlotUI`) : une case par joueur (portrait, champion, nombre de cartes du deck, Changer / Retirer) et une case « Ajouter un joueur » tant qu'il reste de la place ; Ajouter / Changer ouvre la sélection du champion (champions des autres joueurs grisés) puis le choix du deck, dont le bouton devient « Valider le joueur » et ramène au salon ; « Commencer » dès 2 joueurs ; ordre des cases = ordre des tours.
- Page Choix du deck (`Screen_DeckSelect`) : tuiles des decks du champion (`LoadoutTabsUI` + `DeckSlotUI` : bande et noms des couleurs du deck, nombre de cartes), clic = sélectionner, re-clic ou « Modifier » = ouvrir, « Renommer » / « Supprimer » (indisponibles pour le deck de base), « + » = nouveau deck ; « Commencer » lance le combat avec le deck sélectionné ; Retour vers la sélection du champion.
- Gestionnaire de deck (`Screen_DeckManager`, style MTG Arena) : plus d'onglets (le choix du deck se fait sur la page précédente), sert uniquement à modifier le deck : Retour en bas à gauche vers le choix du deck (la dernière modification est sauvegardée en quittant l'écran), pas de bouton Commencer, pool filtrable (~73 % de la largeur) dont les cartes reprennent le design du codex émotionnel (`CardPoolItemUI` + `CodexCardVisual` : rond de coût à la couleur de l'émotion, schéma de portée 9×9, pastilles d'effets avec les icônes du codex dans `Textures/UI/CodexIcons`, description ; exemple et valeurs Excel non repris), liste du deck en colonne façon MTG Arena (une ligne par carte : couleur de l'émotion, coût PA, nom, ×N ; clic = retirer un exemplaire ; prefab `DeckListRow` généré par `UISetupWizard`), courbe de coût en PA, pas d'illustration du personnage, sauvegarde automatique. Le combat se lance depuis la page Choix du deck, seulement avec un deck complet (`DeckRules.IsComplete`, 20 cartes : sinon bouton grisé « Deck incomplet : 17/20 cartes » et compteur orange sur l'onglet du deck). Variante mobile (onglets Pool/Deck) : pas encore faite.
- Jauge de vie du champion (`HealthOrbController`, ex-orbe) : en tête du panneau de stats ; passe en bleu clair tant qu'il a du bouclier, montant du bouclier sous les PV ; barre de vie de Lyse en rouge. Toutes les barres de vie (au-dessus des unités, panneau de stats) sont **rectangulaires**, comme celle du boss (30/09/2026) : sprites vides dans `HealthBar.prefab` et `ChampionStatsPanel/HealthContainer`, `HealthOrbController.Awake` donne un carré blanc à l'Image Filled pour qu'elle continue de se vider.
- HUD de combat (`CombatScene`, 30/09/2026) : en bas à gauche, un panneau unique (`ChampionHUD` : jauge de vie, PA / PM / ATQ / armure / RM en icônes du codex, passif) surmonté d'une rangée de statuts (`UnitStatusChips` : bonus de stats temporaires, bouclier réactif, retraits / gains de PA-PM au prochain tour, Ténacité ; masquée si vide) et, dès 2 champions alliés, du panneau d'équipe (`TeamPortraitsUI` : portrait, nom, PV et bouclier de chaque allié, tour en cours en or). En haut à gauche, sous l'indicateur de tour, la frise des tours (`TurnOrderUI` + `UnitPortraitView`) : un portrait par unité (portrait du champion si `ChampionData.portrait` est renseigné, sinon initiale), cadre bleu allié / rouge ennemi / or pour le tour en cours, mini-jauge de PV. Carte ennemie en haut à droite. Bouton « Fin de tour » rond, au-dessus de la pioche.
- Prévision des dégâts (30/09/2026) : au survol d'une cible avec une carte sélectionnée, `InputManager` publie `DamagePreviewEvent` (calculé par `DamagePreview`, C# pur : dégâts + bonus de prochaine attaque, multiplicateur du lanceur, armure / résistance magique) et `DamagePreviewUI` affiche « -N » (et « KO » si fatal) au-dessus de chaque unité touchée. Volontairement non prévues (chiffre trop dépendant du déroulé) : cartes à combo de PA dépensés (`scalesWithPASpentThisTurn`), charges (Tapis…), bonus par carte défaussée.

**À savoir :**
- `ChampionData.portrait` (buste) existe mais n'est assigné à aucun champion.
- Pool de cartes façon SpamDex : filtre et tri dans `CardPoolQuery` (C# pur, `Scripts/Deck/`, testé en EditMode), piloté par le panneau `PoolFilterBarUI` (ligne `FiltersRow` de la zone pool) : recherche nom/effet insensible aux accents, pastilles `FilterChipUI` d'émotion (seulement celles du deck, masquées si une seule), de catégorie et de type de dégâts (Physique / Magique : cartes à dégâts de ce type seulement) en multi-sélection, coût PA 0/1/2/3/4+ (un à la fois), tri au clic coût → nom → émotion avec ordre réversible, bouton « Réinitialiser » visible si un filtre est actif, message si aucun résultat. Le panneau reste utilisable sur le deck de base (lecture seule). Tout le pool est affiché dans la zone de défilement ; la pagination reste optionnelle (active seulement si les boutons de page sont câblés). À venir : zoom de la grille, fiche détaillée de carte.
- Texte des cartes : généré par `CardRulesText.Build` et affiché par `CardTextView` (icônes en ligne via le Sprite Asset TMP `Resources/CodexIcons/CodexIcons`, atlas des icônes du codex, qui sert aussi aux pastilles) sur les cartes du pool, les cartes en main et l'aperçu de la carte ennemie ; `specialText` pour les règles hors champs (Triche).
- Pastilles d'effets : calculées par `CodexCardVisual.UnitChips` (armure, résistance magique, bouclier d'une unité), construites par `CardChipsView` avec les sprites du même atlas (dont armure, résistance magique et magique, ajoutées le 25/09/2026). Ajouter une icône : la dessiner dans `CodexIcons.png` (64×64, blanc sur transparent), la découper et la nommer dans le Sprite Editor, puis mettre à jour le Sprite Asset TMP. Affichées sous la barre de vie du boss (`UnitChips` : attaque, armure, résistance magique, bouclier non nuls) et sur la fiche de champion de l'écran de sélection (deux rangées, zéros compris : `ChampionResourceChips` PV/PM/PA et `ChampionChips` attaque/armure/résistance magique ; remplacent les anciennes lignes de stats `StatsRow1`/`StatsRow2`, désactivées dans la scène). Couleurs (une seule palette, `CodexCardVisual.ChipColor`, jauge de bouclier des barres de vie comprise) : PV et soin en rose-rouge, PM en vert, PA en bleu, bouclier en cyan, dégâts et ATQ en rouge saumon, armure et résistance magique en gris, déplacements (bond, recul, poussée, tirage) en violet, contreparties en orange ; libellés (PV, PM, PA, ATQ, ARM, RM) devant les icônes de la fiche champion ; ordre sous la barre du boss : ATQ, armure, résistance magique, bouclier, PA, PM, Ténacité.
- Rugissement destructeur : retire 7 d'armure aux ennemis autour (champ `armorAmount`), corrigé le 25/09/2026.
- Commandes de combat (29/09/2026) : les actions des joueurs sont des `CombatCommand` (Combat/) exécutées par `CombatCommandExecutor` (ajouté par `GridManager`, `Services.Commands`), dans l'ordre et après la fin des animations ; `GridManager.OnEndTurnButtonClick` soumet une commande, `EndActiveTurn` termine le tour. Monstres de la scène triés par position (ordre des tours identique sur tous les PC). Réseau (2b) : `Submit` envoie à l'hôte (`NetworkSession.SubmitCommand`), qui renvoie à tous (`Enqueue`) ; chaque commande porte son numéro de tour ; `CombatParty.Seed` (graine des mélanges, tirée par l'hôte, dans `LobbyState`) et `CombatParty.IsLocal` (champion joué sur ce PC) ; en réseau, une ligne « [État réseau] » par tour (`CombatStateFingerprint`) est comparée par l'hôte à celle de chaque client (`DesyncDetector`, messages `tdb.combat.state` / `tdb.combat.desync`) ; écart = `NetworkDesyncEvent` : `DesyncWarningUI` affiche « Partie interrompue » (retour au menu) et `CombatCommandExecutor` cesse tout (`_halted`). Déconnexion en combat (étape 3) : l’hôte marque la place comme partie (`NetworkSession.IsDeparted`, `NetworkPlayerLeftEvent` + bandeau), son champion reste sur la grille et `CombatCommandExecutor` (hôte, `Update` → `PassDepartedTurns`) soumet à sa place `PlacementNext`, les défausses de l’excédent puis `EndTurn` via `SubmitCommandForDeparted` ; `LobbyState.RemovePlayer` n’est jamais appelé en combat (les places de `CombatParty` sont figées). Effets différés (écho, charge, bond) suivis par `PendingEffects`, attendus par l'exécuteur avant chaque commande. Interface personnelle (stats `ChampionHUD`, orbe, main, pioche/défausse) : champion choisi par `LocalView` (le champion actif s'il joue sur ce PC, sinon celui de ce PC) ; `TurnOrderUI` (frise des tours à portraits), `TeamPortraitsUI` (PV des alliés), barre de vie au-dessus des champions (`Champion._healthBarOffset`). Outil de test `NetworkDevAutoplay` (éditeur et builds de développement) : arguments `-tdb-join <ip>`, `-tdb-pick <champion>`, `-tdb-autoplay`.
- Refonte du pool de base (29/09/2026) : 6 cartes refondues et renommées (assets renommés, mêmes GUID ; decks sauvegardés qui les contenaient à refaire, pas d'entrée dans `RenamedCards` car ce sont d'autres cartes). Nouveaux champs de `CardData` : `nextTurnActionGain` (PA au prochain tour de la cible, `ResourceDebuffManager.ApplyActionBonus`), `cancelsEnemyNextCard` (`Enemy.CancelNextCard` / `ConsumeCancelledCard` : le monstre saute sa carte et son attaque de base, aperçu barré), `discardHandAttackBonusPerCard` (`DeckManager.DiscardHandExcept`). `casterActionGain` et le gain au prochain tour dépassent le maximum de PA (`IActionPointsUser.AddPA(amount, canExceedMax)`) ; `costHP` est désormais utilisé (Sang pour sang).
- L'écran de sélection affiche encore ATK et DEF, alors que le design ne définit que PV / PA / PM.

---

## 🔀 Adaptations à prévoir (code actuel → Excel MVP)

| Sujet | Code actuel | Design (Excel) |
|-------|-------------|----------------|
| Ressources | `maxActionPoints` / `movementRange` par champion (Raze, Crux, Evan : 5 / 4) | Profil PA/PM avec budget total de 9 — déjà respecté, la règle n'est pas vérifiée par le code |
| Émotion | Identité de carte (`EmotionType`), pas de jauge | Jauges d'Éveil par émotion, paliers |
| Cartes | `CardData` avec émotion et catégorie | + génération d'Éveil (plus de consommation ni de cartes d'Éveil depuis le 30/09) |
| Deck | 20 cartes (4 Signature + 16 Standard), 1 base + 3 perso | Idem : plus de slots d'Éveil (30/09), format cible à confirmer |
| Statuts | Bouclier en PV (`Unit.AddShield`, `shieldAmount` des cartes, jauge bleue de `HealthBar`) ; armure/résistance magique (`Unit.ReduceByDefense`, `CardData.damageType`, `armorAmount`/`magicResistanceAmount`) ; bouclier réactif, vulnérabilité (`casterArmorAmount`), recul et élan du lanceur, dégâts autour de la cible ; durées en tours du lanceur (`Unit.TickEffectsOnTurnStartOf`) ; retraits de PA/PM ; le reste non implémenté | Retrait de PM (le plus fort remplace le plus faible), poussée/tirage, boucliers en PV, vulnérabilité |
| IA ennemie | Deck pattern | Attaque de base anti-lock et Ténacité faites (`EnemyAI.TryBasicAttack`, `ResourceDebuffManager`) ; reste le cycle de boss Zone/Basique/Heal |
| Champions | Sous-classes `RazeUnit`, `CruxUnit`, `EvanUnit` (+ `LyseUnit`) avec passifs | Valeurs des passifs à valider en playtest ; + fusion (état, commande d'activation, un asset par champion × émotion) |
| Main | Départ 5, max 5 en fin de tour (excédent défaussé au choix), pioche 1/tour | Idem (acté le 24/09) |
| Stats | `attackDamage`, `armor`, `magicResistance` dans `ChampionData` + gains par niveau `attackPerLevel`, `armorPerLevel`, `magicResistancePerLevel` (`AttackAtLevel(level)` etc., arrondi inférieur ; `Champion.Initialize(data, pos, level = 1)` — le niveau réel viendra avec l’XP) ; **ATQ additive** : `CardData.ExecuteEffect` (et la charge) ajoute `source.GetAttack()` aux dégâts de chaque touche d’une carte offensive, après le bonus de prochaine attaque et avant le multiplicateur % puis l’armure (aussi dans `DamagePreview` et l’écho de Lyse, qui en découle) ; armure / résistance magique actives | ATQ additive et gains par niveau actés le 30/09 ; valeurs provisoires |
| Grille | Carrée 10×10, 4 directions (`GridGeometry`) | Idem (acté le 24/09) ; budget des zones de l'Excel (9 / 25 cases) à revoir |
| Cartes MVP | Audit du 25/09 : Standard conformes sauf Effroi partagé ; Bond percutant (bond) et Tapis (2 cibles) corrigés | Corde de rappel : 1 ennemi seulement (Excel : 2 cibles, un allié est tiré sans dégâts) ; Effroi partagé : la « contagion » est approchée par un cercle de rayon 2 (à garder ou à coder) ; Signatures et cartes des monstres pas encore relues |

---

## 🎨 Systèmes de Rendu

### Unity UI System

| Composant | Utilisation | Configuration |
|-----------|-------------|----------------|
| **Canvas** | Rendu de tous les éléments UI | CanvasScaler avec Scale With Screen Size |
| **RectTransform** | Positionnement des éléments UI | anchoredPosition pour positions relatives |
| **CanvasRenderer** | Rendu custom graphics | Utilisé par TargetingCurve et TargetingReticle |

**Coordonnées :**
- Centre du canvas à (0, 0)
- RectTransformUtility pour conversions screen → local
- anchoredPosition pour positions relatives à l'anchor

---

### Custom Graphics

**Composants Custom Unity :**

| Composant | Type | Fonction |
|-----------|------|----------|
| **TargetingCurve** | Unity Graphic | Courbe de Bézier pour ciblage |
| **TargetingReticle** | Unity Graphic | Réticule de ciblage animé |

**Principe :**
- Héritent de Unity Graphic
- Implémentent OnPopulateMesh pour générer les vertices
- Utilisent VertexHelper pour construire les triangles

**Optimisations :**
- Pré-allocation des arrays de points
- SetVerticesDirty seulement si nécessaire
- Calculs géométriques optimisés (Bézier quadratique)

---

### Shaders

| Shader | Utilisation | Notes |
|--------|-------------|-------|
| **UI_SwirlingLiquid** | Effet liquide pour fond de cartes | ⚠️ Introuvable dans le repo au 23/09/2026 (supprimé ou jamais commité) |

**Corrections importantes :**
- Utilisation correcte d'UnityObjectToClipPos
- Pas de référence à des variables non initialisées

---

## ⚡ Optimisations de Performance

### Principes d'Optimisation Appliqués

| Principe | Description | Impact |
|----------|-------------|--------|
| **Pré-allocation d'Arrays** | Allocation une fois à l'initialisation | Évite GC chaque frame |
| **Cache des Composants** | GetComponent une fois dans Awake | Évite appels répétés coûteux |
| **Seuils de Mise à Jour** | Update seulement si changement significatif | Réduit calculs inutiles |
| **Event Cleanup** | Désabonnement dans OnDestroy | Évite fuites mémoire |
| **Coroutine Cleanup** | Stop dans OnDisable/OnDestroy | Évite coroutines orphelines |

---

### Prévention des Allocations GC

**Problème :** allocations répétées dans Update ou OnPopulateMesh causent du Garbage Collection fréquent.

**Solutions Appliquées :**

| Technique | Problème Résolu | Implémentation |
|-----------|------------------|-----------------|
| **Pré-allocation d'Arrays** | Créer List/Array chaque frame | Allouer dans Awake, réutiliser |
| **Cache des Composants** | GetComponent répété | Cacher dans variable privée |
| **Seuils de Mouvement** | Update à chaque pixel | Seuil minimum de déplacement |
| **Réutilisation de Variables** | Créer new Vector2 chaque fois | Variables réutilisables |

**Exemple de Seuil :**
- Seuil de mouvement souris : 1 pixel
- Update de la courbe seulement si déplacement > seuil
- Réduit calculs de 90 %+

---

### Gestion de Mémoire Critique

**Nettoyage Obligatoire :**

| Type | Nettoyage | Conséquence si Oublié |
|------|-----------|------------------------|
| **Événements Statiques** | Désabonnement dans OnDestroy | Fuites mémoire, références mortes |
| **Coroutines** | StopCoroutine dans OnDisable | Coroutines continuent après destruction |
| **Timers** | Annulation dans OnDestroy | Callbacks sur objets détruits |
| **References** | Null dans OnDestroy | Empêche GC de nettoyer |

**Points de Nettoyage :**
- OnDisable : pour désactivation temporaire
- OnDestroy : pour destruction définitive

---

### Object Pooling

**Systèmes à Pooler :**

| Objet | Fréquence de Spawn | Priorité |
|-------|---------------------|----------|
| **CardUIElement** | Chaque pioche | Haute |
| **Effets Visuels** | Chaque action | Haute |
| **Texte de Dégâts** | Chaque attaque | Moyenne |
| **Projectiles** | Chaque attaque | Moyenne |

**Principe du Pool :**
- File d'objets désactivés prêts à l'emploi
- Get : active et retourne un objet
- Return : désactive et remet dans la file
- Évite Instantiate/Destroy coûteux

---

## 🛡️ Null Safety et Error Handling

### Null Checks Obligatoires

| Situation | Check Requis | Raison |
|-----------|--------------|--------|
| **Après GetComponent** | Vérifier si null | Composant peut être absent |
| **Après Find/FindObjectOfType** | Vérifier si null | Objet peut ne pas exister |
| **Avant Utilisation de Références** | Vérifier si null | Référence peut être détruite |
| **Paramètres de Méthodes** | Valider non-null | Prévenir NullReferenceException |

**Stratégies de Gestion :**
- Debug.LogError avec contexte (gameObject)
- Return early si composant critique manquant
- Valeurs par défaut sécurisées
- Validation dans l'Inspector avec RequireComponent

---

## 📊 Data Management

### ScriptableObjects Utilisés

| ScriptableObject | Données Contenues | Menu de Création |
|-------------------|---------------------|-------------------|
| **CardData** | Cartes (nom, coût, effets) | Cards/Card Data |
| **ChampionData** | Champions (stats, deck) | Champion/Champion Data |
| **EnemyData** | Ennemis (stats, pattern) | Enemy/Enemy Data |
| **DeckData / CardCollection** | Decks et collection de cartes | — |

**Avantages :**
- Données séparées du code
- Partageables entre instances
- Éditables dans l'Inspector Unity
- Pas de duplication en mémoire
- Faciles à équilibrer (modifications instantanées)

---

### Serialization

**Utilisations :**

| Type | Format | Usage |
|------|--------|-------|
| **Sauvegardes** | JSON | Progression joueur (persistante, campagne façon Waven) |
| **Configuration** | ScriptableObject | Données de design |
| **Statistiques** | JSON | Métriques de jeu |

**Format Privilégié :** JSON via JsonUtility pour simplicité et compatibilité Unity

---

## 🧪 Testing et Debug

### Outils de Debug

| Outil | Utilisation | Exemple |
|-------|-------------|---------|
| **Debug.Log** | Messages informatifs | Carte cliquée, action validée |
| **Debug.LogWarning** | Avertissements non-critiques | Pas assez de PA |
| **Debug.LogError** | Erreurs critiques | Composant manquant |
| **Gizmos** | Visualisation en éditeur | Grille, portée, zones |

**Bonnes Pratiques :**
- Toujours inclure contexte (gameObject) dans les logs
- Utiliser string interpolation pour clarté
- Gizmos pour visualiser données spatiales
- Debug conditionnels pour éviter spam

---

### Profiling

**Objectifs de Performance (Unity Profiler) :**

| Système | Méthode Critique | Budget | Actuel |
|---------|--------------------|--------|--------|
| **Card UI** | HandUIController.Update | <0.5ms | Optimisé ✓ |
| **Targeting** | TargetingCurve.OnPopulateMesh | <0.2ms | Optimisé ✓ |
| **GC Allocations** | Hot paths (Update, OnPopulate) | 0 bytes | Optimisé ✓ |

**Zones à Surveiller :**
- Update loops dans l'UI
- OnPopulateMesh pour Custom Graphics
- Allocations GC dans les méthodes fréquentes

---

## 🚀 Build et Déploiement

### Plateformes Cibles

| Plateforme | Priorité | Statut |
|------------|----------|--------|
| **Windows** | Primaire | Testé |
| **macOS** | Secondaire | À tester |
| **Linux** | Optionnel | À tester |
| **Mobile (Android/iOS)** | **À trancher** | Mentionné dans `claude_md_coarchitect.md`, non planifié ici (UI, contrôles et raccourcis sont pensés pour PC) — question ouverte dans `GDD_Main.md` |

---

### Build Settings

| Setting | Développement | Release |
|---------|----------------|---------|
| **Compression** | LZ4 (rapide) | LZMA (petite taille) |
| **Stripping Level** | Low | Medium |
| **Script Backend** | Mono (debug rapide) | IL2CPP (performance) |
| **Code Optimization** | Debug | Master |

---

## 📝 Notes Techniques Importantes

### Points d'Attention

| Sujet | Détail |
|-------|--------|
| **Événements Statiques** | TOUJOURS désabonner dans OnDestroy |
| **Coroutines** | Stopper dans OnDisable ET OnDestroy |
| **GetComponent** | Cacher dans Awake, jamais dans Update |
| **Allocations** | Pré-allouer arrays, réutiliser objets |
| **Null Checks** | Toujours valider avant utilisation |

---

**Dernière mise à jour :** 23 Septembre 2026
**Version :** 2.6
**Responsable :** Shinda + Claude
