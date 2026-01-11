# 📘 Game Design Document - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Développement actif
**Mise à jour:** Reflète l'implémentation actuelle

---

## 🎯 Vision du Jeu

**Project TDB** (Tactical Deck Builder) est un jeu de combat tactique au tour par tour qui fusionne la profondeur stratégique des jeux de grille avec la créativité du deck building et un système d'émotions unique. Le joueur contrôle des champions appartenant à **8 familles distinctes**, chacune avec son propre système émotionnel qui transforme leur style de combat.

### Concept Unique

**Système d'Émotions Transformatives**
- Chaque champion possède une jauge émotionnelle (-100 à +100)
- Les cartes jouées influencent cette jauge
- Atteindre les seuils déclenche des **transformations** qui changent radicalement les stats et le style de jeu
- 3 états émotionnels par famille (Positif/Tank ↔ Neutre ↔ Négatif/DPS)

---

## 🎨 Les 8 Familles

| Famille | Couleur | Identité | Style de Jeu |
|---------|---------|----------|--------------|
| **Déchaînés** | Rouge | Guerriers impulsifs maîtrisant la colère | Combattants physiques avec transformations extrêmes |
| **Dissidents** | Vert Foncé | Rebelles tactiques adaptables | Contrôle et manipulation du terrain |
| **Insurgents** | Jaune | Révolutionnaires charismatiques | Buffs d'équipe et mobilité |
| **Exilés** | Bleu Foncé | Parias solitaires mystérieux | Magie sombre et invocations |
| **Réprouvés** | Violet | Maudits cherchant la rédemption | Sacrifice et rédemption |
| **Gardiens** | Vert Clair | Protecteurs nobles | Défense et soins |
| **Éveillés** | Bleu Clair | Illuminés spirituels | Magie spirituelle et soutien |
| **Précurseurs** | Orange | Pionniers innovants | Innovation et effets uniques |

**Émotions des Déchaînés (exemple):**
- **Contrariété** (Positif/Tank) : État défensif et frustré
- **Colère** (Neutre) : État de base équilibré
- **Rage** (Négatif/DPS) : Fureur destructrice offensive

Voir [Card_System.md](Card_System.md) pour les détails complets de chaque famille.

---

## 🎭 Piliers de Design

### 1. Émotions et Transformations
- Système unique qui différencie Project TDB
- Chaque carte influence l'état émotionnel du champion
- Les transformations offrent des choix stratégiques (Tank vs DPS)
- Les émotions sont personnalisées par famille

### 2. Synergie Famille-Classe-Élément
- **8 Familles** : Identité thématique et émotions
- **5 Classes** : Rôle tactique (Ancre, Tisseur, Ombrelame, Veilleur, Harmoniste)
- **4 Éléments** : Types de dégâts et interactions (Feu, Ombre, Lumière, Eau)
- Les cartes combinent ces 3 dimensions pour créer des synergies profondes

### 3. Gestion Tactique Multi-dimensionnelle

| Ressource | Description | Utilisation |
|-----------|-------------|-------------|
| **PA** (Points d'Action) | 3-5 par tour | Jouer des cartes |
| **PM** (Points de Mouvement) | 2-4 par tour | Se déplacer sur la grille |
| **HP** (Santé) | Variable selon champion | Points de vie |
| **Défense Physique** | 10+ | Réduit dégâts physiques |
| **Défense Magique** | 10+ | Réduit dégâts magiques |
| **Jauge Émotionnelle** | -100 à +100 | Déclenche transformations |

### 4. Positionnement Tactique sur Grille
- Grille avec système de coordonnées 2D
- Portée des cartes variable (mêlée à distance infinie)
- Zones d'effet : Circle, Line, Cross, Cone
- Ciblage précis : unités, tuiles vides, zones

---

## 🎮 Boucle de Combat

```
1. INITIALISATION
   ↓
2. TOUR DU CHAMPION
   • Pioche de cartes (jusqu'à 5 en main)
   • Restauration des PA et PM
   • Actions du joueur :
     - Jouer des cartes
     - Se déplacer sur la grille
     - Gérer ses émotions
   • Fin de tour
   ↓
3. TOUR ENNEMI
   • IA choisit une action
   • Joue une carte (selon pattern)
   • Exécution de l'action
   ↓
4. VÉRIFICATION
   • Tous les ennemis vaincus ? → VICTOIRE
   • Tous les champions vaincus ? → DÉFAITE
   • Sinon → Retour au tour du champion
   ↓
5. RÉCOMPENSES (si victoire)
```

---

## ⚔️ Systèmes Principaux

### Cartes et Deck Building

**Caractéristiques des Cartes:**
- Chaque carte appartient à une Famille, Classe et Élément
- Coût en PA : 0 à 5+
- Portée : 0 (soi-même) à infini
- Zone d'effet : Aucune, Circle, Line, Cross, Cone
- Modificateur d'émotion : -50 à +50

**Types de Ciblage:**

| Type | Description | Exemple |
|------|-------------|---------|
| None | Aucune cible | Buff automatique personnel |
| Self | Soi-même uniquement | Se soigner |
| Enemy | Un ou plusieurs ennemis | Attaque |
| Ally | Alliés (sauf soi) | Buff allié |
| AllyOrSelf | Alliés ET soi | Soins de groupe |
| EmptyTile | Tuiles vides | Invocation, piège |
| AnyTile | N'importe quelle tuile | Zone d'effet centrée |

**Types de Dégâts:**
- **Physique** : Réduit par défense physique (minimum 1 dégât)
- **Magique** : Réduit par défense magique (minimum 1 dégât)

### Système d'Émotions

**Fonctionnement:**
- Jauge de -100 (DPS) à +100 (Tank)
- Les cartes modifient la jauge (+/- 1 à 50 par carte)
- 3 états : Positif, Neutre, Négatif
- Les noms des états varient selon la famille

**Transformations:**
- Déclenchées à +100 (Tank) ou -100 (DPS)
- Une seule transformation par combat
- Modificateurs possibles :
  - Stats : HP max, PA max, Défense, Mouvement
  - Effets passifs : Dégâts bonus, Vol de vie, Régénération
  - Visuels : Glow, effets de particules

**Exemple : Déchaînés**

| État | Seuil | Nom | Orientation | Bonus Typiques |
|------|-------|-----|-------------|----------------|
| Positif | +100 | Contrariété | Tank | +HP, +Défense |
| Neutre | 0 | Colère | Équilibré | Stats de base |
| Négatif | -100 | Rage | DPS | +Dégâts, +Critique |

### Champions vs Ennemis

| Caractéristique | Champions | Ennemis |
|-----------------|-----------|---------|
| **Deck** | Personnel, mélangé | Pattern fixe, séquentiel |
| **PA** | 3-5 par tour | 2-4 par tour |
| **Émotions** | Oui (optionnel) | Non |
| **Contrôle** | Joueur | IA |
| **Barre de vie** | Au-dessus de la tête | Au-dessus ou en haut (boss) |

---

## 🏗️ Architecture Technique

### Patterns de Conception Utilisés

| Pattern | Utilisation | Bénéfice |
|---------|-------------|----------|
| **Service Locator** | Accès global aux services (Grid, etc.) | Découplage, testabilité |
| **Event Bus** | Communication entre systèmes | Découplage total |
| **State Machine** | Gestion des tours et états d'unités | Code clair, transitions validées |
| **Component Pattern** | Composition (ActionPointsComponent) | Réutilisation de code |
| **Repository Pattern** | Accès optimisé aux données (GridRepository) | Performance |
| **ScriptableObject** | Données (CardData, ChampionData, etc.) | Séparation données/logique |

### Structure des Données

**Champions:**
- Nom, Prefab
- Famille, Élément
- Stats : HP, PA, PM, Défenses
- Deck de départ (liste de cartes)
- Données d'émotions et transformations

**Ennemis:**
- Nom, Prefab, Élément
- Type : Normal ou Boss
- Stats : HP, PA, PM, Défenses
- Deck pattern (ordre fixe de cartes)

**Cartes:**
- Nom, Description, Illustration
- Famille, Classe, Élément
- Coût en PA
- Type de cible et portée
- Zone d'effet
- Effets : Dégâts, Soins, Mouvement
- Modificateur d'émotion

---

## 📊 État Actuel du Projet

### ✅ Systèmes Implémentés

**Systèmes Core:**
- [x] Service Locator pour services globaux
- [x] Event Bus pour communication
- [x] Turn State Machine (5 états)
- [x] GridManager et GridRepository
- [x] Système de tuiles (Tile)

**Systèmes de Combat:**
- [x] Classe de base Unit
- [x] Champions avec gestion PA
- [x] Ennemis avec pattern deck
- [x] DeckManager (pioche, défausse, mélange)
- [x] Système de cartes complet (8 familles, 5 classes, 4 éléments)
- [x] Ciblage avancé (9 types de cibles, AOE variées)
- [x] EmotionSystem avec transformations

**Interface Utilisateur:**
- [x] Main de cartes en arc (style Limbus Company)
- [x] Cartes avec hover et sélection
- [x] Courbe de ciblage (Bézier)
- [x] Réticule de ciblage
- [x] Barre de vie des boss (haut écran)
- [x] Preview des cartes ennemies
- [x] Pop-up de dégâts
- [x] Feedbacks de combat

**Validation et Debug:**
- [x] Validation des actions de jeu
- [x] Outils de debug UI
- [x] Setup automatique de l'UI

### 🔄 En Cours de Développement

- [ ] Contenu de cartes (création des 8 familles complètes)
- [ ] IA ennemie avancée (patterns complexes)
- [ ] Effets de statut (poison, brûlure, gel, etc.)
- [ ] Système de progression
- [ ] Méta-progression
- [ ] Campagne et niveaux

### ⏳ À Planifier

- [ ] Définition des émotions pour les 7 familles restantes
- [ ] Modes de jeu additionnels
- [ ] Tutoriel intégré
- [ ] Polish audio et effets visuels
- [ ] Équilibrage complet

---

## 🎯 Objectifs de Design

### Court Terme (Version Alpha)
- Finaliser les 8 familles avec leurs émotions uniques
- Créer 10-15 cartes par famille (120 cartes total)
- Implémenter 5-10 ennemis de base + 1 boss
- Tutorial pour le système d'émotions
- Interface complète et fonctionnelle

### Moyen Terme (Version Bêta)
- 20+ cartes par famille (160 cartes)
- 20+ types d'ennemis + 3-5 boss
- Système de progression complet
- Méta-progression
- 3 actes de campagne

### Long Terme (Version 1.0)
- 30+ cartes par famille (240+ cartes)
- Campagne complète avec histoire
- Modes de jeu variés (Arène, Défis, etc.)
- Polish complet (audio, VFX, animations)
- Équilibrage fin et retours communauté

---

## 📚 Inspirations

### Jeux de Référence

| Jeu | Inspiration | Éléments Repris |
|-----|-------------|-----------------|
| **Slay the Spire** | Deck building roguelike | Construction de deck, progression |
| **Into the Breach** | Tactique sur grille | Positionnement précis, conséquences claires |
| **Darkest Dungeon** | Gestion stress | Système de stress/émotions |
| **Limbus Company** | UI et émotions | Layout cartes, système EGO |
| **XCOM** | Combat tour par tour | Ressources limitées, décisions tactiques |

### Ce qui Rend Project TDB Unique

1. **8 Familles** avec systèmes d'émotions personnalisés
2. **Transformations permanentes** (choix Tank vs DPS pendant combat)
3. **Triple identité** des cartes (Famille + Classe + Élément)
4. **Fusion** Deck Building + Grille Tactique + Émotions
5. **Profondeur stratégique** : chaque carte influence 3 systèmes (combat, position, émotions)

---

## 🔮 Vision Future

### Extensibilité
- Système de familles permet ajout facile de nouvelles familles
- ScriptableObjects facilitent création de contenu
- Architecture modulaire pour nouveaux modes

### Rejouabilité
- 8 familles × multiples archétypes = grande variété
- Émotions ajoutent imprévisibilité et adaptation
- Synergies entre familles dans équipes mixtes

### Potentiel Compétitif
- Mode PvP envisageable (équipes de champions)
- Classements pour modes challenge
- Meta évolutive avec patches de contenu

---

**Dernière mise à jour:** 11 Janvier 2026
**Version GDD:** 2.0
**Responsable:** Équipe Project TDB

**Documents Connexes:**
- [Technical_Specs.md](Technical_Specs.md) - Architecture technique détaillée
- [Card_System.md](Card_System.md) - Système de cartes complet
- [Combat_System.md](Combat_System.md) - Mécanique de combat
- [Characters.md](Characters.md) - Champions et données
- [Enemies.md](Enemies.md) - Ennemis et boss
