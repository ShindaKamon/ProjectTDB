# Game Design Document - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Développement actif
**Mise à jour:** Reflète l'implémentation actuelle

## Vision du Jeu

**Project TDB** (Tactical Deck Builder) est un jeu de combat tactique au tour par tour qui fusionne la profondeur stratégique des jeux de grille avec la créativité du deck building et un système d'émotions unique. Le joueur contrôle des champions appartenant à **8 familles distinctes**, chacune avec son propre système émotionnel qui transforme leur style de combat.

### Concept Unique

**Système d'Emotions Transformatives**
- Chaque champion possède une jauge émotionnelle (0 à 100)
- Plusieurs paramètres influencent cette jauge
- Atteindre les seuils déclenche des **transformations** qui changent radicalement les stats et le style de jeu


## Les 8 Familles

Chaque famille a sa propre identité thématique et son système d'émotions unique.

| Famille         | Couleur    | Code Hex  | 
|-----------------|------------|-----------|
| **Déchaînés**   | Rouge      | #CC0000 | 
| **Dissidents**  | Violet     | #800080 |
| **Insurgents**  | Bleu Foncé | #000080 | 
| **Exilés**      | Bleu Clair | #80CCFF |
| **Réprouvés**   | Vert Foncé | #006600 | 
| **Gardiens**    | Vert Clair | #80FF80 | 
| **Eveillés**    | Jaune      | #FFEB00 | 
| **Précurseurs** | Orange     | #FF8000 |


## Piliers de Design

### 1. Emotions et Transformations
- Système unique qui différencie Project TDB
- Chaque carte influence l'état émotionnel du champion
- Les émotions sont personnalisées par famille

### 2. Synergie Famille-Classe-Elément
- **8 Familles** : Identité thématique et émotions
- **5 Classes** : Gestion de l'émotion

### 3. Gestion Tactique Multi-dimensionnelle

| Ressource                    | Description | Utilisation                      |
|------------------------------|-------------|----------------------------------|
| **PA** (Points d'Action)     | 3-5         | Pour jouer des cartes            |
| **PM** (Points de Mouvement) | 2-4         | Pour se déplacer (1 PM = 1 case) |
| **HP** (Points de Vie)       | Variable    | Tombe à 0 = vaincu               |
| **Défense**                  | 10+         | Réduit dégâts physiques          |
| **Jauge Emotionnelle**       | 0 à 100     | Déclenche transformations        |

### 4. Positionnement Tactique sur Grille
- Grille avec système de coordonnées 2D
- Portée des cartes variable (mêlée à distance infinie)
- Zones d'effet : Circle, Line, Cross, Cone
- Ciblage précis : unités, tuiles vides, zones


## Boucle de Combat

```
1. INITIALISATION
   â†“
2. TOUR DU CHAMPION
   â€¢ Pioche de cartes (jusqu'à 5 en main)
   â€¢ Restauration des PA et PM
   â€¢ Actions du joueur :
     - Jouer des cartes
     - Se déplacer sur la grille
     - Gérer ses émotions
   â€¢ Fin de tour
   â†“
3. TOUR ENNEMI
   â€¢ IA choisit une action
   â€¢ Joue une carte (selon pattern)
   â€¢ Exécution de l'action
   â†“
4. VERIFICATION
   â€¢ Tous les ennemis vaincus ? â†’ VICTOIRE
   â€¢ Tous les champions vaincus ? â†’ DEFAITE
   â€¢ Sinon â†’ Retour au tour du champion
   â†“
5. RECOMPENSES (si victoire)
```


## Systèmes Principaux

### Cartes et Deck Building

**Caractéristiques des Cartes:**
- Chaque carte appartient à une Famille, Classe et Neutre
- Coût en PA : 0 à 5+
- Portée : 0 (soi-même) à infini
- Zone d'effet : Aucune, Circle, Line, Cross, Cone

**Types de Ciblage:**

| Type            | Description             | Cas d'Usage       | Exemple                            |
|-----------------|-------------------------|-------------------|------------------------------------|
| **None**        | Aucune cible            | Effet automatique | Buff personnel instantané          |
| **Self**        | Soi-même uniquement     | Auto-ciblage      | Se soigner, se buffer              |
| **Enemy**       | Un ou plusieurs ennemis | Attaques standard | Frappe, Sort offensif              |
| **Ally**        | Alliés (sauf soi)       | Support d'équipe  | Soigner un allié                   |
| **AllyOrSelf**  | Alliés ET soi-même      | Support flexible  | Soins de groupe                    |
| **AllyorEnemy** | Alliés ET ennemis       | Effets mixtes     | Explosion qui touche tout le monde |
| **AnyUnit**     | N'importe quelle unité  | Polyvalent        | Télékinésie, déplacement forcé     |
| **EmptyTile**   | Tuiles vides uniquement | Placement         | Invocation, piège, zone            |
| **AnyTile**     | N'importe quelle tuile  | Zone centrée      | Météore, explosion ciblée          |


## Architecture Technique

### Patterns de Conception Utilisés

| Pattern                | Utilisation                                 | Bénéfice                         |
|------------------------|---------------------------------------------|----------------------------------|
| **Service Locator**    | Accès global aux services (Grid, etc.)      | Découplage, testabilité          |
| **Event Bus**          | Communication entre systèmes                | Découplage total                 |
| **State Machine**      | Gestion des tours et états d'unités         | Code clair, transitions validées |
| **Component Pattern**  | Composition (ActionPointsComponent)         | Réutilisation de code            |
| **Repository Pattern** | Accès optimisé aux données (GridRepository) | Performance                      |
| **ScriptableObject**   | Données (CardData, ChampionData, etc.)      | Séparation données/logique       |

### Structure des Données

**Champions:**
- Nom, Prefab
- Famille, Classe
- Stats : HP, PA, PM, Défense
- Deck de départ (liste de cartes)
- Données d'émotions et transformations

**Ennemis:**
- Nom, Prefab
- Type : Normal ou Boss
- Stats : HP, PA, PM, Défense
- Deck pattern (ordre fixe de cartes)

**Cartes:**
- Nom, Description, Illustration
- Famille, Classe
- Coût en PA
- Type de cible et portée
- Zone d'effet
- Effets : Dégâts, Soins, Mouvement


## Etat Actuel du Projet

### Systèmes Implémentés

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
- [x] Système de cartes complet (8 familles, 5 classes)
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

### En Cours de Développement

- [ ] Contenu de cartes (création des 8 familles complètes)
- [ ] IA ennemie avancée (patterns complexes)
- [ ] Effets de statut (poison, brûlure, gel, etc.)
- [ ] Système de progression
- [ ] Méta-progression
- [ ] Campagne et niveaux

### Planifier

- [ ] Définition des émotions pour les 7 familles restantes
- [ ] Modes de jeu additionnels
- [ ] Tutoriel intégré
- [ ] Polish audio et effets visuels
- [ ] Equilibrage complet


## Objectifs de Design

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
- Equilibrage fin et retours communauté


## Inspirations

### Jeux de Référence

| Jeu                      | Inspiration          | Eléments Repris                             |
|--------------------------|----------------------|---------------------------------------------|
| **Waven**                | Deck building        | Construction de deck, progression           |
| **Dofus**                | Tactique sur grille  | Positionnement précis, conséquences claires |
| **Darkest Dungeon**      | Gestion stress       | Système de stress/émotions                  |
| **Chaos Zero Nightmare** | UI et émotions       | Layout cartes, système EGO                  |
| **Magic the Gathering**  | Création de deck     | Ressources limitées, décisions tactiques    |

### Ce qui Rend Project TDB Unique

1. **8 Familles** avec systèmes d'émotions personnalisés
2. **Transformations** Système de puissance
3. **Double identité** des cartes (Famille + Classe)
4. **Fusion** Deck Building + Grille Tactique + Emotions
5. **Profondeur stratégique** : chaque carte influence 3 systèmes (combat, position, émotions)


## Vision Future

### Extensibilité
- Système de familles permet ajout facile de nouvelles familles
- ScriptableObjects facilitent création de contenu
- Architecture modulaire pour nouveaux modes

### Rejouabilité
- 8 familles Ã— multiples archétypes = grande variété
- Emotions ajoutent imprévisibilité et adaptation
- Synergies entre familles dans équipes mixtes

### Potentiel Compétitif
- Mode PvP envisageable (équipes de champions)
- Classements pour modes challenge
- Meta évolutive avec patches de contenu


**Dernière mise à jour:** 11 Janvier 2026
**Version GDD:** 2.0
**Responsable:** Equipe Project TDB

