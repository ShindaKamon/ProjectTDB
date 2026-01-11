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

### Prévention des Allocations GC

**Problème:** Allocations répétées dans Update() ou OnPopulateMesh() causent du Garbage Collection fréquent.

**Solutions:**

1. **Pré-allocation d'Arrays:**
```csharp
// ❌ MAUVAIS - Allocation chaque frame
void OnPopulateMesh(VertexHelper vh) {
    List<Vector2> points = new List<Vector2>(); // GC!
}

// ✅ BON - Pré-allocation
private Vector2[] _curvePoints;
void Awake() {
    _curvePoints = new Vector2[_segmentCount + 1];
}
```

2. **Cache des Composants:**
```csharp
// ❌ MAUVAIS - GetComponent chaque frame
void Update() {
    RectTransform rt = GetComponent<RectTransform>(); // GC!
}

// ✅ BON - Cache à l'initialisation
private RectTransform _rectTransform;
void Awake() {
    _rectTransform = GetComponent<RectTransform>();
}
```

3. **Seuils de Mise à Jour:**
```csharp
// ✅ BON - Update seulement si nécessaire
if (Vector2.Distance(mousePos, _lastMousePos) > MOUSE_MOVE_THRESHOLD)
{
    UpdateTargeting(mousePos);
    _lastMousePos = mousePos;
}
```

### Gestion de Mémoire

**Event Cleanup (CRITIQUE):**
```csharp
void OnDestroy()
{
    // Nettoyer TOUS les événements statiques
    CardUIElement.OnCardClicked -= HandleCardClicked;
    CardUIElement.OnCardHoverEnter -= HandleCardHoverEnter;
    CardUIElement.OnCardHoverExit -= HandleCardHoverExit;
}
```

**Coroutine Cleanup (CRITIQUE):**
```csharp
void OnDisable()
{
    if (_glowPulseCoroutine != null)
    {
        StopCoroutine(_glowPulseCoroutine);
        _glowPulseCoroutine = null;
    }
}

void OnDestroy()
{
    // Même cleanup pour sécurité
    if (_glowPulseCoroutine != null)
    {
        StopCoroutine(_glowPulseCoroutine);
        _glowPulseCoroutine = null;
    }
}
```

### Object Pooling

**À implémenter pour:**
- Instances de cartes (CardUIElement)
- Effets visuels (particules, sprites)
- Projectiles
- Texte de dégâts flottant

**Pattern de base:**
```csharp
public class ObjectPool<T> where T : Component
{
    private Queue<T> _pool = new Queue<T>();
    private T _prefab;

    public T Get() {
        if (_pool.Count > 0) {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        return Object.Instantiate(_prefab);
    }

    public void Return(T obj) {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

---

## 🔒 Null Safety et Error Handling

### Null Checks Obligatoires

**Après GetComponent:**
```csharp
RectTransform rt = GetComponent<RectTransform>();
if (rt == null)
{
    Debug.LogError("Component RectTransform manquant!");
    return;
}
```

**Après Find/FindObjectOfType:**
```csharp
GameObject obj = GameObject.Find("HandContainer");
if (obj == null)
{
    Debug.LogError("GameObject 'HandContainer' introuvable!");
    return;
}
```

**Avant Utilisation de Références:**
```csharp
if (_targetingCurve != null)
{
    _targetingCurve.UpdateCurve(startPos, endPos);
}
```

---

## 📊 Data Management

### ScriptableObjects

**Avantages:**
- Données séparées du code
- Partageables entre instances
- Éditables dans l'Inspector
- Pas de duplication en mémoire

**Exemples:**
```csharp
[CreateAssetMenu(fileName = "NewCard", menuName = "Project TDB/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string description;
    public int costPA;
    public CardType cardType;
    public TargetType targetType;
    public List<CardEffect> effects;
}
```

### Serialization

**Utilisé pour:**
- Sauvegardes de progression
- Configuration de niveaux
- Statistiques de personnages

**Format:** JSON via `JsonUtility` (simple) ou Newtonsoft.Json (complexe)

---

## 🧪 Testing et Debug

### Debug Tools

**Logs avec Contexte:**
```csharp
Debug.Log($"Carte cliquée : {_cardData.cardName}", gameObject);
Debug.LogWarning($"Pas assez de PA pour {_cardData.cardName}", gameObject);
Debug.LogError($"RectTransform manquant sur {gameObject.name}!", gameObject);
```

**Gizmos pour Visualisation:**
```csharp
void OnDrawGizmos()
{
    // Dessiner la grille en mode éditeur
    Gizmos.color = Color.green;
    foreach (var tile in tiles)
    {
        Gizmos.DrawWireCube(tile.worldPosition, Vector3.one * 0.9f);
    }
}
```

### Profiling

**Unity Profiler - Zones à Surveiller:**
- **Update()** dans HandUIController : <0.5ms par frame
- **OnPopulateMesh()** dans TargetingCurve : <0.2ms
- **GC Allocations** : 0 dans hot paths (Update, OnPopulateMesh)

---

## 🚀 Build et Déploiement

### Plateformes Cibles
- **Windows** (Primaire)
- **macOS** (Secondaire)
- **Linux** (Optionnel)

### Build Settings
- **Compression** : LZ4 (fast) pour développement, LZMA (small) pour release
- **Stripping Level** : Medium (balance taille/compatibilité)
- **Script Backend** : IL2CPP pour performance

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Équipe Technique Project TDB
