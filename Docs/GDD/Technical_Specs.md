# 🔧 Spécifications Techniques - Émotions Tactics (Project TDB)

**Version:** 2.5
**Date:** 24 Septembre 2026
**Statut:** Reflète l'architecture actuelle.
**Changements :**
- v2.1 (10/09/2026) : retrait des mentions Classes et Éléments.
- v2.2 (23/09/2026) : section Système d'Émotions mise à jour (la jauge universelle -100/+100 existe dans le code mais n'est pas utilisée par le design pour l'instant ; les mécaniques signatures comme la Rage d'Ilya sont la cible) ; plateforme mobile signalée comme question ouverte.
- v2.3 (23/09/2026) : écarts entre le code et l'Excel MVP listés (section « Adaptations à prévoir »).
- v2.4 (24/09/2026) : nettoyage hors MVP — code d'Ilya (Rage) et de Vylos (Stigmate) retiré, assets morts supprimés, prefabs champions sortis des dossiers de familles ; pool de l'éditeur de deck façon SpamDex.
- v2.5 (24/09/2026) : règle de grille unifiée dans `GridGeometry` : 4 directions (Manhattan) partout, la portée euclidienne de certaines validations est supprimée ; IA ennemie sans contrainte d'alignement ; écho du Miroir fraternel compris.

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
| **Assets/Project/Scripts/Cards/** | `CardData` (ScriptableObject data-driven), `DeckManager`, `DeckDiscardUI` |
| **Assets/Project/Scripts/Core/** | `ServiceLocator`/`Services`, `EventBus`/`GameEvent`, `TurnStateMachine`, `GridManager`, `GridRepository`, `BattleUIManager`, `HealthBarManager`, `ChampionSelectManager`, `GameLog` |
| **Assets/Project/Scripts/Grid/** | `Tile` |
| **Assets/Project/Scripts/Units/** | `Unit` → `Champion` → `AceUnit`, `AlpinisteUnit`, `SorenUnit` ; `SummonUnit`/`LyseUnit` ; `Enemy`, `EnemyAI` ; interfaces `IComboTracker`, `IOutgoingDamageModifier`, `IChargeLandingReactor`, `ISummonOwner` ; `ChampionData`, `EnemyData` |
| **Assets/Project/Scripts/Deck/** | `DeckData`, `DeckSaveManager`, `ChampionDecksData`, `AllDecksData`, `CardCollection` |
| **Assets/Project/Scripts/Marks/** | Marques (`IMarkable`, `UnitMark`, `MarkType` : Poison, AllMarks) |
| **Assets/Project/Scripts/Debuffs/** | `ResourceDebuffManager` (retraits de PA/PM) |
| **Assets/Project/Scripts/Validation/** | `GameActionValidator`, `ValidationResult` |
| **Assets/Project/Scripts/Input/** | `InputManager` |
| **Assets/Project/Scripts/UI/** | Main de cartes (`HandUIController`, `CardUIElement`, `TargetingCurve`, `TargetingReticle`), sélection de champion, éditeur de deck, HUD de combat |
| **Assets/Project/Scripts/Editor/** | `UISetupWizard`, `DeckDebugMenu`, `PlayModeStartSceneSetup` |
| **Assets/Project/Tests/EditMode/** | Tests NUnit (`GameActionValidatorTests`, `DeckManagerCostOverrideTests`, `ValidationResultTests`) |
| **Assets/Project/Scenes/** | `ChampionSelectScene`, `CombatScene` |
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

> Taille de main : non tranchée (voir `Combat_System.md`) — la garder paramétrable. Budget PA+PM = 9, deck = 24 cartes (Excel).

#### Fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **Scènes** | PascalCase | MainMenu, Combat_Level01 |
| **Prefabs** | PascalCase | CardUI_Template, HexTile |
| **ScriptableObjects** | PascalCase avec suffixe | Card_CoupDeColereData, Character_SorenData |

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

- `CardData` : ScriptableObject **data-driven** (dégâts, ciblage `CardTargetType`, zone `CardAreaEffect`, effets `CardEffectType`, marques, charge, émotion `EmotionType`, catégorie `CardCategory` Standard/Eveil/Signature). Résolution via `CardData.ExecuteEffect(...)`, appelée par `HandUIController` et `EnemyAI`. Une nouvelle carte = un nouvel asset.
- `DeckManager` (sur l'unité) : pioche, main, défausse, coûts effectifs (`GetEffectiveCost`, overrides de coût pour Raze).
- `DeckData` : 2 slots Signature + 16 Standard (les 6 slots Éveil ne sont pas encore ajoutés) ; `DeckSaveManager` : sauvegarde JSON, 1 deck de base + 3 decks perso par champion. Les decks référencent les cartes **par nom**.

### 3. UI de cartes

- `HandUIController`, `CardUIElement` : main en arc (style Limbus Company), hover, sélection.
- `TargetingCurve`, `TargetingReticle` : Unity Graphic custom (courbe de Bézier, réticule), sans allocation GC dans `OnPopulateMesh`.

### 4. Combat

- `TurnStateMachine` (classe C# pure) : états Initializing / PlayerTurn / EnemyTurn / TransitioningTurn / BattleEnd.
- Rotation : un tour par unité dans l'ordre de `_units` (invocations sautées). Au début du tour : PM et PA rafraîchis, 1 carte piochée.
- `GameActionValidator` : centralise les règles « peut-on jouer / cibler / se déplacer » (le plus couvert par les tests).

### 5. Émotions

> ⚠️ Il **n'y a pas** de `EmotionSystem` dans le code (contrairement aux anciennes versions de ce document). Les émotions existent comme **identité des cartes** (`EmotionType` sur `CardData`) ; chaque deck a 1 ou 2 couleurs (choisies à sa création) qui filtrent le pool du gestionnaire (`DeckRules.DeckColors`). La **jauge d'Éveil** (une jauge par émotion, paliers de 2 points) est **à implémenter**.

---

## 🧭 État du code (vérifié le 23/09/2026)

Cette section remplace l'ancienne « Mise à jour implémentation » de `claude_md_coarchitect.md` et fait foi pour décrire ce qui existe **dans le code**.

**Roster jouable :** Raze (« Le Tricheur »), Crux (« Le Grimpeur »), Evan (« Le Frère », + invocation Lyse), référencés dans `ChampionSelectManager._allChampions`. Seuls ces 3 champions existent (fiches dans `Assets/ScriptableObjects/Characters/Champion/`, prefabs dans `Assets/Project/Prefabs/Champions/`). Les 3 champions MVP ont 100 PV, 5 PA, 4 PM.

**Ilya, Vylos, Calyx (hors MVP) : retirés du code le 24/09/2026.** Ilya y avait une version différente de `ilya_deck_simple.md` (carte Rage ajoutée à la main tous les 10 dégâts subis, stock max 5) ; Vylos portait la marque Stigmate. Le code reste consultable dans l'historique git (commit `00afe5d`, dernier état avant le nettoyage) si Ilya revient, sa Rage étant à réadapter à l'Éveil.

**Cartes :** 49 cartes Standard (17 Colère, 17 Peur, 15 Joie), mêmes noms que la bibliothèque de l'Excel ; Signatures des 3 champions (les anciens assets « Family » ont été supprimés).

**Deck :** 18 cartes (2 Signature + 16 Standard, `DeckData`) ; multi-deck : 1 deck de base (non supprimable, resynchronisé depuis les cartes de départ du champion à chaque session) + jusqu'à 3 decks perso (`MAX_CUSTOM_DECKS = 3`) . Règles (`DeckRules`) : cartes des couleurs du deck uniquement (1 ou 2, choisies à sa création ; le deck de base prend celles de ses cartes), 4 exemplaires max par carte, les 2 Signatures du champion obligatoires (1 exemplaire chacune) ; les Signatures des autres champions sont interdites ; un deck existant non conforme est corrigé à son chargement (cartes hors couleurs et exemplaires en trop retirés, Signatures ajoutées).

**Combat :** grille carrée 10×10 en 4 directions (Manhattan, pour le déplacement, la portée, les zones et les charges) ; un tour par unité ; main de départ 5, max 5, 1 carte piochée par tour ; 1 ennemi (UnderBed, 500 PV, cartes « Attaque Range » et « Heal Self »).

**Écrans construits :**
- Sélection de champion (`Screen_ChampionSelect`) : roster à gauche, illustration au centre (`ChampionData.fullBodyArt`, art provisoire), panneau de droite (`ChampionStatsUI`) avec nom, titre, histoire (`ChampionData.description`, reprise du trauma de `CHAMPIONS_CONCEPTS.md`), statistiques et bouton « Choisir ce champion ».
- Page Choix du deck (`Screen_DeckSelect`) : tuiles des decks du champion (`LoadoutTabsUI` + `DeckSlotUI` : bande et noms des couleurs du deck, nombre de cartes), clic = sélectionner, re-clic ou « Modifier » = ouvrir, « Renommer » / « Supprimer » (indisponibles pour le deck de base), « + » = nouveau deck ; « Commencer » lance le combat avec le deck sélectionné ; Retour vers la sélection du champion.
- Gestionnaire de deck (`Screen_DeckManager`, style MTG Arena) : plus d'onglets (le choix du deck se fait sur la page précédente), sert uniquement à modifier le deck : Retour en bas à gauche vers le choix du deck (la dernière modification est sauvegardée en quittant l'écran), pas de bouton Commencer, pool filtrable (~73 % de la largeur) dont les cartes reprennent le design du codex émotionnel (`CardPoolItemUI` + `CodexCardVisual` : rond de coût à la couleur de l'émotion, schéma de portée 9×9, pastilles d'effets avec les icônes du codex dans `Textures/UI/CodexIcons`, description ; exemple et valeurs Excel non repris), liste du deck en colonne façon MTG Arena (une ligne par carte : couleur de l'émotion, coût PA, nom, ×N ; clic = retirer un exemplaire ; prefab `DeckListRow` généré par `UISetupWizard`), courbe de coût en PA, pas d'illustration du personnage, sauvegarde automatique. Le combat se lance depuis la page Choix du deck (tolère un deck incomplet). Variante mobile (onglets Pool/Deck) : pas encore faite.
- HUD de combat (`CombatScene`) : stats du personnage en haut à gauche sous l'indicateur de tour, carte ennemie en haut à droite.

**À savoir :**
- `ChampionData.portrait` (buste) existe mais n'est assigné à aucun champion.
- Pool de cartes façon SpamDex : filtre et tri dans `CardPoolQuery` (C# pur, `Scripts/Deck/`, testé en EditMode), piloté par le panneau `PoolFilterBarUI` (ligne `FiltersRow` de la zone pool) : recherche nom/effet insensible aux accents, pastilles `FilterChipUI` d'émotion (seulement celles du deck, masquées si une seule) et de catégorie en multi-sélection, coût PA 0/1/2/3/4+ (un à la fois), tri au clic coût → nom → émotion avec ordre réversible, bouton « Réinitialiser » visible si un filtre est actif, message si aucun résultat. Le panneau reste utilisable sur le deck de base (lecture seule). Tout le pool est affiché dans la zone de défilement ; la pagination reste optionnelle (active seulement si les boutons de page sont câblés). À venir : zoom de la grille, fiche détaillée de carte.
- Connu : Rugissement destructeur (cible `Self`) donne +7 DEF au lanceur via `ModifyStats`, alors que sa description annonce une armure réduite pour les ennemis autour (données ou code à corriger).
- L'écran de sélection affiche encore ATK et DEF, alors que le design ne définit que PV / PA / PM.

---

## 🔀 Adaptations à prévoir (code actuel → Excel MVP)

| Sujet | Code actuel | Design (Excel) |
|-------|-------------|----------------|
| Ressources | `maxActionPoints` / `movementRange` par champion (Raze, Crux, Evan : 5 / 4) | Profil PA/PM avec budget total de 9 — déjà respecté, la règle n'est pas vérifiée par le code |
| Émotion | Identité de carte (`EmotionType`), pas de jauge | Jauges d'Éveil par émotion, paliers |
| Cartes | `CardData` avec émotion et catégorie | + génération/consommation d'Éveil |
| Deck | 18 cartes (2 Signature + 16 Standard), 1 base + 3 perso | 24 cartes (2 / 6 / 16) |
| Statuts | Non implémentés | Retrait de PM (le plus fort remplace le plus faible), poussée/tirage, boucliers %, vulnérabilité |
| IA ennemie | Deck pattern | + Attaque de base anti-lock, cycle de boss Zone/Basique/Heal |
| Champions | Sous-classes `AceUnit`, `AlpinisteUnit`, `SorenUnit` (+ `LyseUnit`) avec passifs | Valeurs des passifs à valider en playtest |
| Main | Départ 5, max 5, pioche 1/tour | Idem (acté le 24/09) |
| Stats | `attackDamage` et `defense` dans `ChampionData` (ATK non utilisé par les champions MVP) | Pas de stats au-delà de PV/PA/PM (à trancher) |
| Grille | Carrée 10×10, 4 directions (`GridGeometry`) | Idem (acté le 24/09) ; budget des zones de l'Excel (9 / 25 cases) à revoir |

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
**Version :** 2.2
**Responsable :** Shinda + Claude
