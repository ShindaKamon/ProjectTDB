# ðŸ”§ SpÃ©cifications Techniques - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** ReflÃ¨te l'architecture actuelle

---

## ðŸŽ® Moteur et Technologies

### Plateforme de DÃ©veloppement

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Moteur** | Unity | 2022.3 LTS |
| **Langage** | C# | .NET Standard 2.1 |
| **Version Control** | Git | DerniÃ¨re version |
| **IDE** | Visual Studio / JetBrains Rider | 2022+ |

### Packages Unity UtilisÃ©s

| Package | Utilisation | Statut |
|---------|-------------|--------|
| **TextMeshPro** | Rendu de texte haute qualitÃ© | Actif |
| **Input System** | Gestion des entrÃ©es utilisateur | Actif |
| **Universal Render Pipeline (URP)** | Pipeline de rendu moderne | Actif |
| **Cinemachine** | Gestion de camÃ©ra | Optionnel |

---

## ðŸ—ï¸ Architecture du Projet

### Structure des Dossiers

| Dossier | Contenu | Description |
|---------|---------|-------------|
| **Assets/Project/Scripts/Cards/** | CardData, Deck, etc. | SystÃ¨me de cartes complet |
| **Assets/Project/Scripts/Combat/** | TurnStateMachine, etc. | Combat tour par tour |
| **Assets/Project/Scripts/Core/** | Services, EventBus | SystÃ¨mes centraux |
| **Assets/Project/Scripts/Grid/** | GridManager, Tile | Grille tactique |
| **Assets/Project/Scripts/UI/Cards/** | HandUIController, CardUIElement | Interface cartes |
| **Assets/Project/Scripts/UI/Combat/** | BossHealthBar, etc. | Interface combat |
| **Assets/Project/Scripts/Units/** | Champion, Enemy, EmotionSystem | Personnages et ennemis |
| **Assets/Project/Prefabs/** | Prefabs Unity | Champions, ennemis, UI |
| **Assets/Project/Scenes/** | ScÃ¨nes Unity | Niveaux, menus |
| **Docs/GDD/** | Documentation Markdown | Design documents |

### Conventions de Nommage

#### Scripts C#

| Type | Convention | Exemple |
|------|------------|---------|
| **Classes** | PascalCase | CardUIElement, HandUIController |
| **MÃ©thodes** | PascalCase | UpdateCurve, HandleCardClicked |
| **Variables privÃ©es** | _camelCase avec underscore | _cardData, _isSelected |
| **Variables publiques** | camelCase | cardName, costPA |
| **Constantes** | UPPER_SNAKE_CASE | MAX_HAND_SIZE, DEFAULT_PA |

#### Fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **ScÃ¨nes** | PascalCase | MainMenu, Combat_Level01 |
| **Prefabs** | PascalCase | CardUI_Template, HexTile |
| **ScriptableObjects** | PascalCase avec suffixe | Card_FireballData, Character_IlyaData |

---

## ðŸ“¦ SystÃ¨mes Principaux

### Patterns de Conception UtilisÃ©s

| Pattern | Utilisation | BÃ©nÃ©fice | Fichiers |
|---------|-------------|----------|----------|
| **Service Locator** | AccÃ¨s global aux services | DÃ©couplage, testabilitÃ© | Services.cs |
| **Event Bus** | Communication entre systÃ¨mes | DÃ©couplage total | EventBus.cs, GameEvent.cs |
| **State Machine** | Gestion des tours et Ã©tats | Transitions validÃ©es | TurnStateMachine.cs, UnitState.cs |
| **Component Pattern** | Composition d'entitÃ©s | RÃ©utilisation de code | ActionPointsComponent.cs |
| **Repository Pattern** | AccÃ¨s optimisÃ© aux donnÃ©es | Performance | GridRepository.cs |
| **ScriptableObject** | DonnÃ©es sÃ©parÃ©es du code | Ã‰dition facile, partage | CardData, ChampionData, etc. |

---

### 1. SystÃ¨me de Grille (Grid System)

**Fichiers principaux:**

| Fichier | ResponsabilitÃ© |
|---------|----------------|
| GridManager.cs | Gestion de la grille, placement des unitÃ©s |
| GridRepository.cs | AccÃ¨s optimisÃ© aux donnÃ©es de grille |
| Tile.cs | ReprÃ©sentation d'une tuile |

**ResponsabilitÃ©s:**
- GÃ©nÃ©ration de la grille de combat
- Calcul de distance entre tuiles
- Gestion des unitÃ©s sur les tuiles
- Validation de placement

**Optimisations:**
- Repository pattern pour accÃ¨s rapide
- Cache des positions
- Service Locator pour accÃ¨s global

---

### 2. SystÃ¨me de Cartes (Card System)

**Fichiers principaux:**

| Fichier | ResponsabilitÃ© |
|---------|----------------|
| CardData.cs | ScriptableObject avec donnÃ©es de carte |
| DeckManager.cs | Gestion deck, pioche, dÃ©fausse |
| CardEffectExecutor.cs | ExÃ©cution des effets de cartes |

**ResponsabilitÃ©s:**
- DÃ©finition des cartes (8 Familles Ã— 5 Classes Ã— 4 Ã‰lÃ©ments)
- Gestion du deck (pioche sÃ©quentielle ou mÃ©langÃ©e)
- Application des effets (dÃ©gÃ¢ts, soins, mouvement)
- Validation des cibles

**Optimisations:**
- ScriptableObjects pour donnÃ©es statiques
- Events pour communication avec l'UI
- Validation rapide de ciblage

---

### 3. SystÃ¨me d'UI de Cartes (Card UI System)

**Fichiers principaux:**

| Fichier | ResponsabilitÃ© |
|---------|----------------|
| HandUIController.cs | ContrÃ´leur principal de la main |
| CardUIElement.cs | Ã‰lÃ©ment visuel d'une carte |
| TargetingCurve.cs | Courbe de ciblage (Unity Graphic) |
| TargetingReticle.cs | RÃ©ticule de ciblage (Unity Graphic) |

**ResponsabilitÃ©s:**
- Layout en arc des cartes (style Limbus Company)
- Animations de hover et sÃ©lection
- SystÃ¨me de ciblage visuel avec courbe de BÃ©zier
- Feedbacks visuels (glow, tint, scale)

**Optimisations Critiques:**
- Cache du RectTransform pour Ã©viter GetComponent chaque frame
- Seuil de mouvement pour Ã©viter recalculs inutiles
- PrÃ©-allocation des arrays pour courbe de BÃ©zier
- Ã‰vÃ©nements nettoyÃ©s dans OnDestroy pour Ã©viter fuites mÃ©moire

**Ã‰vÃ©nements Importants:**
- OnCardClicked : Carte cliquÃ©e par le joueur
- OnCardHoverEnter : Souris entre sur une carte
- OnCardHoverExit : Souris quitte une carte

---

### 4. SystÃ¨me de Combat (Combat System)

**Fichiers principaux:**

| Fichier | ResponsabilitÃ© |
|---------|----------------|
| TurnStateMachine.cs | Machine Ã  Ã©tats des tours |
| CombatManager.cs | Orchestration du combat |
| ActionValidator.cs | Validation des actions |

**ResponsabilitÃ©s:**
- Gestion de l'ordre des tours (Champions â†’ Ennemis)
- RÃ©solution des actions de cartes
- Application des dÃ©gÃ¢ts et effets
- Conditions de victoire/dÃ©faite

**Ã‰tats du Combat:**

| Ã‰tat | Description | Transitions Possibles |
|------|-------------|-----------------------|
| **Initializing** | Initialisation du combat | â†’ PlayerTurn |
| **PlayerTurn** | Tour du joueur | â†’ EnemyTurn, BattleEnd |
| **EnemyTurn** | Tour des ennemis | â†’ TransitioningTurn |
| **TransitioningTurn** | Transition entre tours | â†’ PlayerTurn, BattleEnd |
| **BattleEnd** | Fin du combat | Aucune |

---

### 5. SystÃ¨me d'Ã‰motions (Emotion System)

**Fichiers principaux:**

| Fichier | ResponsabilitÃ© |
|---------|----------------|
| EmotionSystem.cs | Gestion de la jauge Ã©motionnelle |
| FamilyEmotionData.cs | Noms des Ã©motions par famille |
| TransformationData.cs | Modificateurs de transformation |

**ResponsabilitÃ©s:**
- Gestion de la jauge (-100 Ã  +100)
- DÃ©clenchement des transformations (Tank/DPS)
- Application des modificateurs de stats
- Affichage visuel de l'Ã©tat Ã©motionnel

**Ã‰tats Ã‰motionnels:**

| Ã‰tat | Seuil | Type | CaractÃ©ristiques |
|------|-------|------|------------------|
| **Positive** | +100 | Tank | Bonus dÃ©fensifs, HP accrus |
| **Neutral** | 0 | Ã‰quilibrÃ© | Stats de base |
| **Negative** | -100 | DPS | Bonus offensifs, dÃ©gÃ¢ts accrus |

---

## ðŸŽ¨ SystÃ¨mes de Rendu

### Unity UI System

| Composant | Utilisation | Configuration |
|-----------|-------------|---------------|
| **Canvas** | Rendu de tous les Ã©lÃ©ments UI | CanvasScaler avec Scale With Screen Size |
| **RectTransform** | Positionnement des Ã©lÃ©ments UI | anchoredPosition pour positions relatives |
| **CanvasRenderer** | Rendu custom graphics | UtilisÃ© par TargetingCurve et TargetingReticle |

**CoordonnÃ©es:**
- Centre du canvas Ã  (0, 0)
- RectTransformUtility pour conversions screen â†’ local
- anchoredPosition pour positions relatives Ã  l'anchor

---

### Custom Graphics

**Composants Custom Unity:**

| Composant | Type | Fonction |
|-----------|------|----------|
| **TargetingCurve** | Unity Graphic | Courbe de BÃ©zier pour ciblage |
| **TargetingReticle** | Unity Graphic | RÃ©ticule de ciblage animÃ© |

**Principe:**
- HÃ©ritent de Unity Graphic
- ImplÃ©mentent OnPopulateMesh pour gÃ©nÃ©rer les vertices
- Utilisent VertexHelper pour construire les triangles

**Optimisations:**
- PrÃ©-allocation des arrays de points
- SetVerticesDirty seulement si nÃ©cessaire
- Calculs gÃ©omÃ©triques optimisÃ©s (BÃ©zier quadratique)

---

### Shaders

| Shader | Utilisation | Notes |
|--------|-------------|-------|
| **UI_SwirlingLiquid** | Effet liquide pour fond de cartes | Shader UI compatible avec Canvas |

**Corrections importantes:**
- Utilisation correcte d'UnityObjectToClipPos
- Pas de rÃ©fÃ©rence Ã  des variables non initialisÃ©es

---

## âš¡ Optimisations de Performance

### Principes d'Optimisation AppliquÃ©s

| Principe | Description | Impact |
|----------|-------------|--------|
| **PrÃ©-allocation d'Arrays** | Allocation une fois Ã  l'initialisation | Ã‰vite GC chaque frame |
| **Cache des Composants** | GetComponent une fois dans Awake | Ã‰vite appels rÃ©pÃ©tÃ©s coÃ»teux |
| **Seuils de Mise Ã  Jour** | Update seulement si changement significatif | RÃ©duit calculs inutiles |
| **Event Cleanup** | DÃ©sabonnement dans OnDestroy | Ã‰vite fuites mÃ©moire |
| **Coroutine Cleanup** | Stop dans OnDisable/OnDestroy | Ã‰vite coroutines orphelines |

---

### PrÃ©vention des Allocations GC

**ProblÃ¨me:** Allocations rÃ©pÃ©tÃ©es dans Update ou OnPopulateMesh causent du Garbage Collection frÃ©quent.

**Solutions AppliquÃ©es:**

| Technique | ProblÃ¨me RÃ©solu | ImplÃ©mentation |
|-----------|-----------------|----------------|
| **PrÃ©-allocation d'Arrays** | CrÃ©er List/Array chaque frame | Allouer dans Awake, rÃ©utiliser |
| **Cache des Composants** | GetComponent rÃ©pÃ©tÃ© | Cacher dans variable privÃ©e |
| **Seuils de Mouvement** | Update Ã  chaque pixel | Seuil minimum de dÃ©placement |
| **RÃ©utilisation de Variables** | CrÃ©er new Vector2 chaque fois | Variables rÃ©utilisables |

**Exemple de Seuil:**
- Seuil de mouvement souris : 1 pixel
- Update de la courbe seulement si dÃ©placement > seuil
- RÃ©duit calculs de 90%+

---

### Gestion de MÃ©moire Critique

**Nettoyage Obligatoire:**

| Type | Nettoyage | ConsÃ©quence si OubliÃ© |
|------|-----------|----------------------|
| **Ã‰vÃ©nements Statiques** | DÃ©sabonnement dans OnDestroy | Fuites mÃ©moire, rÃ©fÃ©rences mortes |
| **Coroutines** | StopCoroutine dans OnDisable | Coroutines continuent aprÃ¨s destruction |
| **Timers** | Annulation dans OnDestroy | Callbacks sur objets dÃ©truits |
| **References** | Null dans OnDestroy | EmpÃªche GC de nettoyer |

**Points de Nettoyage:**
- OnDisable : Pour dÃ©sactivation temporaire
- OnDestroy : Pour destruction dÃ©finitive

---

### Object Pooling

**SystÃ¨mes Ã  Pooler:**

| Objet | FrÃ©quence de Spawn | PrioritÃ© |
|-------|-------------------|----------|
| **CardUIElement** | Chaque pioche | Haute |
| **Effets Visuels** | Chaque action | Haute |
| **Texte de DÃ©gÃ¢ts** | Chaque attaque | Moyenne |
| **Projectiles** | Chaque attaque | Moyenne |

**Principe du Pool:**
- File d'objets dÃ©sactivÃ©s prÃªts Ã  l'emploi
- Get : Active et retourne un objet
- Return : DÃ©sactive et remet dans la file
- Ã‰vite Instantiate/Destroy coÃ»teux

---

## ðŸ”’ Null Safety et Error Handling

### Null Checks Obligatoires

| Situation | Check Requis | Raison |
|-----------|--------------|--------|
| **AprÃ¨s GetComponent** | VÃ©rifier si null | Composant peut Ãªtre absent |
| **AprÃ¨s Find/FindObjectOfType** | VÃ©rifier si null | Objet peut ne pas exister |
| **Avant Utilisation de RÃ©fÃ©rences** | VÃ©rifier si null | RÃ©fÃ©rence peut Ãªtre dÃ©truite |
| **ParamÃ¨tres de MÃ©thodes** | Valider non-null | PrÃ©venir NullReferenceException |

**StratÃ©gies de Gestion:**
- Debug.LogError avec contexte (gameObject)
- Return early si composant critique manquant
- Valeurs par dÃ©faut sÃ©curisÃ©es
- Validation dans l'Inspector avec RequireComponent

---

## ðŸ“Š Data Management

### ScriptableObjects UtilisÃ©s

| ScriptableObject | DonnÃ©es Contenues | Menu de CrÃ©ation |
|------------------|-------------------|------------------|
| **CardData** | Cartes (nom, coÃ»t, effets) | Cards/Card Data |
| **ChampionData** | Champions (stats, deck) | Champion/Champion Data |
| **EnemyData** | Ennemis (stats, pattern) | Enemy/Enemy Data |
| **FamilyEmotionData** | Ã‰motions par famille | Champion/Family Emotion Data |
| **TransformationData** | Modificateurs de transformation | Champion/Transformation Data |

**Avantages:**
- DonnÃ©es sÃ©parÃ©es du code
- Partageables entre instances
- Ã‰ditables dans l'Inspector Unity
- Pas de duplication en mÃ©moire
- Faciles Ã  balance (modifications instantanÃ©es)

---

### Serialization

**Utilisations:**

| Type | Format | Usage |
|------|--------|-------|
| **Sauvegardes** | JSON | Progression joueur |
| **Configuration** | ScriptableObject | DonnÃ©es de design |
| **Statistiques** | JSON | MÃ©triques de jeu |

**Format PrivilÃ©giÃ©:** JSON via JsonUtility pour simplicitÃ© et compatibilitÃ© Unity

---

## ðŸ§ª Testing et Debug

### Outils de Debug

| Outil | Utilisation | Exemple |
|-------|-------------|---------|
| **Debug.Log** | Messages informatifs | Carte cliquÃ©e, action validÃ©e |
| **Debug.LogWarning** | Avertissements non-critiques | Pas assez de PA |
| **Debug.LogError** | Erreurs critiques | Composant manquant |
| **Gizmos** | Visualisation en Ã©diteur | Grille, portÃ©e, zones |

**Bonnes Pratiques:**
- Toujours inclure contexte (gameObject) dans les logs
- Utiliser string interpolation pour clartÃ©
- Gizmos pour visualiser donnÃ©es spatiales
- Debug conditionnels pour Ã©viter spam

---

### Profiling

**Objectifs de Performance (Unity Profiler):**

| SystÃ¨me | MÃ©thode Critique | Budget | Actuel |
|---------|------------------|--------|--------|
| **Card UI** | HandUIController.Update | <0.5ms | OptimisÃ© âœ“ |
| **Targeting** | TargetingCurve.OnPopulateMesh | <0.2ms | OptimisÃ© âœ“ |
| **GC Allocations** | Hot paths (Update, OnPopulate) | 0 bytes | OptimisÃ© âœ“ |

**Zones Ã  Surveiller:**
- Update loops dans l'UI
- OnPopulateMesh pour Custom Graphics
- Allocations GC dans les mÃ©thodes frÃ©quentes

---

## ðŸš€ Build et DÃ©ploiement

### Plateformes Cibles

| Plateforme | PrioritÃ© | Statut |
|------------|----------|--------|
| **Windows** | Primaire | TestÃ© |
| **macOS** | Secondaire | Ã€ tester |
| **Linux** | Optionnel | Ã€ tester |

---

### Build Settings

| Setting | DÃ©veloppement | Release |
|---------|---------------|---------|
| **Compression** | LZ4 (rapide) | LZMA (petite taille) |
| **Stripping Level** | Low | Medium |
| **Script Backend** | Mono (debug rapide) | IL2CPP (performance) |
| **Code Optimization** | Debug | Master |

---

## ðŸ“ Notes Techniques Importantes

### Points d'Attention

| Sujet | DÃ©tail |
|-------|--------|
| **Ã‰vÃ©nements Statiques** | TOUJOURS dÃ©sabonner dans OnDestroy |
| **Coroutines** | Stopper dans OnDisable ET OnDestroy |
| **GetComponent** | Cacher dans Awake, jamais dans Update |
| **Allocations** | PrÃ©-allouer arrays, rÃ©utiliser objets |
| **Null Checks** | Toujours valider avant utilisation |

---

**DerniÃ¨re mise Ã  jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Ã‰quipe Technique Project TDB
