# 🔧 Spécifications Techniques - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète l'architecture actuelle

---

## 🎮 Moteur et Technologies

### Plateforme de Développement

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Moteur** | Unity | 2022.3 LTS |
| **Langage** | C# | .NET Standard 2.1 |
| **Version Control** | Git | Dernière version |
| **IDE** | Visual Studio / JetBrains Rider | 2022+ |

### Packages Unity Utilisés

| Package | Utilisation | Statut |
|---------|-------------|--------|
| **TextMeshPro** | Rendu de texte haute qualité | Actif |
| **Input System** | Gestion des entrées utilisateur | Actif |
| **Universal Render Pipeline (URP)** | Pipeline de rendu moderne | Actif |
| **Cinemachine** | Gestion de caméra | Optionnel |

---

## 🏗️ Architecture du Projet

### Structure des Dossiers

| Dossier | Contenu | Description |
|---------|---------|-------------|
| **Assets/Project/Scripts/Cards/** | CardData, Deck, etc. | Système de cartes complet |
| **Assets/Project/Scripts/Combat/** | TurnStateMachine, etc. | Combat tour par tour |
| **Assets/Project/Scripts/Core/** | Services, EventBus | Systèmes centraux |
| **Assets/Project/Scripts/Grid/** | GridManager, Tile | Grille tactique |
| **Assets/Project/Scripts/UI/Cards/** | HandUIController, CardUIElement | Interface cartes |
| **Assets/Project/Scripts/UI/Combat/** | BossHealthBar, etc. | Interface combat |
| **Assets/Project/Scripts/Units/** | Champion, Enemy, EmotionSystem | Personnages et ennemis |
| **Assets/Project/Prefabs/** | Prefabs Unity | Champions, ennemis, UI |
| **Assets/Project/Scenes/** | Scènes Unity | Niveaux, menus |
| **Docs/GDD/** | Documentation Markdown | Design documents |

### Conventions de Nommage

#### Scripts C#

| Type | Convention | Exemple |
|------|------------|---------|
| **Classes** | PascalCase | CardUIElement, HandUIController |
| **Méthodes** | PascalCase | UpdateCurve, HandleCardClicked |
| **Variables privées** | _camelCase avec underscore | _cardData, _isSelected |
| **Variables publiques** | camelCase | cardName, costPA |
| **Constantes** | UPPER_SNAKE_CASE | MAX_HAND_SIZE, DEFAULT_PA |

#### Fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **Scènes** | PascalCase | MainMenu, Combat_Level01 |
| **Prefabs** | PascalCase | CardUI_Template, HexTile |
| **ScriptableObjects** | PascalCase avec suffixe | Card_FireballData, Character_IlyaData |

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

### 1. Système de Grille (Grid System)

**Fichiers principaux:**

| Fichier | Responsabilité |
|---------|----------------|
| GridManager.cs | Gestion de la grille, placement des unités |
| GridRepository.cs | Accès optimisé aux données de grille |
| Tile.cs | Représentation d'une tuile |

**Responsabilités:**
- Génération de la grille de combat
- Calcul de distance entre tuiles
- Gestion des unités sur les tuiles
- Validation de placement

**Optimisations:**
- Repository pattern pour accès rapide
- Cache des positions
- Service Locator pour accès global

---

### 2. Système de Cartes (Card System)

**Fichiers principaux:**

| Fichier | Responsabilité |
|---------|----------------|
| CardData.cs | ScriptableObject avec données de carte |
| DeckManager.cs | Gestion deck, pioche, défausse |
| CardEffectExecutor.cs | Exécution des effets de cartes |

**Responsabilités:**
- Définition des cartes (8 Familles × 5 Classes × 4 Éléments)
- Gestion du deck (pioche séquentielle ou mélangée)
- Application des effets (dégâts, soins, mouvement)
- Validation des cibles

**Optimisations:**
- ScriptableObjects pour données statiques
- Events pour communication avec l'UI
- Validation rapide de ciblage

---

### 3. Système d'UI de Cartes (Card UI System)

**Fichiers principaux:**

| Fichier | Responsabilité |
|---------|----------------|
| HandUIController.cs | Contrôleur principal de la main |
| CardUIElement.cs | Élément visuel d'une carte |
| TargetingCurve.cs | Courbe de ciblage (Unity Graphic) |
| TargetingReticle.cs | Réticule de ciblage (Unity Graphic) |

**Responsabilités:**
- Layout en arc des cartes (style Limbus Company)
- Animations de hover et sélection
- Système de ciblage visuel avec courbe de Bézier
- Feedbacks visuels (glow, tint, scale)

**Optimisations Critiques:**
- Cache du RectTransform pour éviter GetComponent chaque frame
- Seuil de mouvement pour éviter recalculs inutiles
- Pré-allocation des arrays pour courbe de Bézier
- Événements nettoyés dans OnDestroy pour éviter fuites mémoire

**Événements Importants:**
- OnCardClicked : Carte cliquée par le joueur
- OnCardHoverEnter : Souris entre sur une carte
- OnCardHoverExit : Souris quitte une carte

---

### 4. Système de Combat (Combat System)

**Fichiers principaux:**

| Fichier | Responsabilité |
|---------|----------------|
| TurnStateMachine.cs | Machine à états des tours |
| CombatManager.cs | Orchestration du combat |
| ActionValidator.cs | Validation des actions |

**Responsabilités:**
- Gestion de l'ordre des tours (Champions → Ennemis)
- Résolution des actions de cartes
- Application des dégâts et effets
- Conditions de victoire/défaite

**États du Combat:**

| État | Description | Transitions Possibles |
|------|-------------|-----------------------|
| **Initializing** | Initialisation du combat | → PlayerTurn |
| **PlayerTurn** | Tour du joueur | → EnemyTurn, BattleEnd |
| **EnemyTurn** | Tour des ennemis | → TransitioningTurn |
| **TransitioningTurn** | Transition entre tours | → PlayerTurn, BattleEnd |
| **BattleEnd** | Fin du combat | Aucune |

---

### 5. Système d'Émotions (Emotion System)

**Fichiers principaux:**

| Fichier | Responsabilité |
|---------|----------------|
| EmotionSystem.cs | Gestion de la jauge émotionnelle |
| FamilyEmotionData.cs | Noms des émotions par famille |
| TransformationData.cs | Modificateurs de transformation |

**Responsabilités:**
- Gestion de la jauge (-100 à +100)
- Déclenchement des transformations (Tank/DPS)
- Application des modificateurs de stats
- Affichage visuel de l'état émotionnel

**États Émotionnels:**

| État | Seuil | Type | Caractéristiques |
|------|-------|------|------------------|
| **Positive** | +100 | Tank | Bonus défensifs, HP accrus |
| **Neutral** | 0 | Équilibré | Stats de base |
| **Negative** | -100 | DPS | Bonus offensifs, dégâts accrus |

---

## 🎨 Systèmes de Rendu

### Unity UI System

| Composant | Utilisation | Configuration |
|-----------|-------------|---------------|
| **Canvas** | Rendu de tous les éléments UI | CanvasScaler avec Scale With Screen Size |
| **RectTransform** | Positionnement des éléments UI | anchoredPosition pour positions relatives |
| **CanvasRenderer** | Rendu custom graphics | Utilisé par TargetingCurve et TargetingReticle |

**Coordonnées:**
- Centre du canvas à (0, 0)
- RectTransformUtility pour conversions screen → local
- anchoredPosition pour positions relatives à l'anchor

---

### Custom Graphics

**Composants Custom Unity:**

| Composant | Type | Fonction |
|-----------|------|----------|
| **TargetingCurve** | Unity Graphic | Courbe de Bézier pour ciblage |
| **TargetingReticle** | Unity Graphic | Réticule de ciblage animé |

**Principe:**
- Héritent de Unity Graphic
- Implémentent OnPopulateMesh pour générer les vertices
- Utilisent VertexHelper pour construire les triangles

**Optimisations:**
- Pré-allocation des arrays de points
- SetVerticesDirty seulement si nécessaire
- Calculs géométriques optimisés (Bézier quadratique)

---

### Shaders

| Shader | Utilisation | Notes |
|--------|-------------|-------|
| **UI_SwirlingLiquid** | Effet liquide pour fond de cartes | Shader UI compatible avec Canvas |

**Corrections importantes:**
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

**Problème:** Allocations répétées dans Update ou OnPopulateMesh causent du Garbage Collection fréquent.

**Solutions Appliquées:**

| Technique | Problème Résolu | Implémentation |
|-----------|-----------------|----------------|
| **Pré-allocation d'Arrays** | Créer List/Array chaque frame | Allouer dans Awake, réutiliser |
| **Cache des Composants** | GetComponent répété | Cacher dans variable privée |
| **Seuils de Mouvement** | Update à chaque pixel | Seuil minimum de déplacement |
| **Réutilisation de Variables** | Créer new Vector2 chaque fois | Variables réutilisables |

**Exemple de Seuil:**
- Seuil de mouvement souris : 1 pixel
- Update de la courbe seulement si déplacement > seuil
- Réduit calculs de 90%+

---

### Gestion de Mémoire Critique

**Nettoyage Obligatoire:**

| Type | Nettoyage | Conséquence si Oublié |
|------|-----------|----------------------|
| **Événements Statiques** | Désabonnement dans OnDestroy | Fuites mémoire, références mortes |
| **Coroutines** | StopCoroutine dans OnDisable | Coroutines continuent après destruction |
| **Timers** | Annulation dans OnDestroy | Callbacks sur objets détruits |
| **References** | Null dans OnDestroy | Empêche GC de nettoyer |

**Points de Nettoyage:**
- OnDisable : Pour désactivation temporaire
- OnDestroy : Pour destruction définitive

---

### Object Pooling

**Systèmes à Pooler:**

| Objet | Fréquence de Spawn | Priorité |
|-------|-------------------|----------|
| **CardUIElement** | Chaque pioche | Haute |
| **Effets Visuels** | Chaque action | Haute |
| **Texte de Dégâts** | Chaque attaque | Moyenne |
| **Projectiles** | Chaque attaque | Moyenne |

**Principe du Pool:**
- File d'objets désactivés prêts à l'emploi
- Get : Active et retourne un objet
- Return : Désactive et remet dans la file
- Évite Instantiate/Destroy coûteux

---

## 🔒 Null Safety et Error Handling

### Null Checks Obligatoires

| Situation | Check Requis | Raison |
|-----------|--------------|--------|
| **Après GetComponent** | Vérifier si null | Composant peut être absent |
| **Après Find/FindObjectOfType** | Vérifier si null | Objet peut ne pas exister |
| **Avant Utilisation de Références** | Vérifier si null | Référence peut être détruite |
| **Paramètres de Méthodes** | Valider non-null | Prévenir NullReferenceException |

**Stratégies de Gestion:**
- Debug.LogError avec contexte (gameObject)
- Return early si composant critique manquant
- Valeurs par défaut sécurisées
- Validation dans l'Inspector avec RequireComponent

---

## 📊 Data Management

### ScriptableObjects Utilisés

| ScriptableObject | Données Contenues | Menu de Création |
|------------------|-------------------|------------------|
| **CardData** | Cartes (nom, coût, effets) | Cards/Card Data |
| **ChampionData** | Champions (stats, deck) | Champion/Champion Data |
| **EnemyData** | Ennemis (stats, pattern) | Enemy/Enemy Data |
| **FamilyEmotionData** | Émotions par famille | Champion/Family Emotion Data |
| **TransformationData** | Modificateurs de transformation | Champion/Transformation Data |

**Avantages:**
- Données séparées du code
- Partageables entre instances
- Éditables dans l'Inspector Unity
- Pas de duplication en mémoire
- Faciles à balance (modifications instantanées)

---

### Serialization

**Utilisations:**

| Type | Format | Usage |
|------|--------|-------|
| **Sauvegardes** | JSON | Progression joueur |
| **Configuration** | ScriptableObject | Données de design |
| **Statistiques** | JSON | Métriques de jeu |

**Format Privilégié:** JSON via JsonUtility pour simplicité et compatibilité Unity

---

## 🧪 Testing et Debug

### Outils de Debug

| Outil | Utilisation | Exemple |
|-------|-------------|---------|
| **Debug.Log** | Messages informatifs | Carte cliquée, action validée |
| **Debug.LogWarning** | Avertissements non-critiques | Pas assez de PA |
| **Debug.LogError** | Erreurs critiques | Composant manquant |
| **Gizmos** | Visualisation en éditeur | Grille, portée, zones |

**Bonnes Pratiques:**
- Toujours inclure contexte (gameObject) dans les logs
- Utiliser string interpolation pour clarté
- Gizmos pour visualiser données spatiales
- Debug conditionnels pour éviter spam

---

### Profiling

**Objectifs de Performance (Unity Profiler):**

| Système | Méthode Critique | Budget | Actuel |
|---------|------------------|--------|--------|
| **Card UI** | HandUIController.Update | <0.5ms | Optimisé ✓ |
| **Targeting** | TargetingCurve.OnPopulateMesh | <0.2ms | Optimisé ✓ |
| **GC Allocations** | Hot paths (Update, OnPopulate) | 0 bytes | Optimisé ✓ |

**Zones à Surveiller:**
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

---

### Build Settings

| Setting | Développement | Release |
|---------|---------------|---------|
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

**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Équipe Technique Project TDB
