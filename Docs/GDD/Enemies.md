# Ennemis - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète la structure actuelle (EnemyData)

## Vue d'Ensemble

Les **Ennemis** de Project TDB utilisent un système de **deck pattern** où leurs cartes sont jouées dans un ordre fixe et séquentiel (pas de mélange). Ils sont classifiés en ennemis normaux et boss, avec des barres de vie différentes selon leur type.


## Structure d'un Ennemi (EnemyData)

### Données de Base

| Attribut    | Type            | Description                              |
|-------------|-----------------|------------------------------------------|
| **Nom**     | Text            | Nom de l'ennemi                          |
| **Prefab**  | GameObject      | Modèle 3D/2D de l'ennemi                 |
| **Is Boss** | Boolean         | Si true, barre de vie en haut de l'écran |


### Statistiques de Combat

| Stat                  | Plage Typique              | Description                     |
|-----------------------|----------------------------|---------------------------------|
| **Max Health**        | Normaux: 30-80, Boss: 200+ | Points de vie maximum           |
| **Movement Range**    | 2-4                        | Points de mouvement par tour    |
| **Max Action Points** | 2-4                        | Points d'action par tour        |
| **Physical Defense**  | 5-20                       | Défense contre dégâts physiques |
| **Magical Defense**   | 5-20                       | Défense contre dégâts magiques  |


### Deck Pattern (Combat Deck)

| Caractéristique    | Ennemis            | Champions (Comparaison)    |
|--------------------|--------------------|----------------------------|
| **Type de Pioche** | Séquentielle       | Aléatoire (mélangé)        |
| **Ordre**          | Fixe, se répète    | Aléatoire à chaque pioche |
| **Taille Typique** | 5-10 cartes        | 15-25 cartes               |
| **Stratégie**      | Pattern prévisible | Imprévisible               |


### Visual Settings

| Paramètre             | Description                                      |
|-----------------------|--------------------------------------------------|
| **Health Bar Offset** | Position de la barre de vie au-dessus de la tête |
| **Health Bar Color**  | Couleur de la barre de vie (typiquement rouge)   |


## Différence Normaux vs Boss

### Ennemis Normaux

| Caractéristique  | Valeur               |
|------------------|----------------------|
| **Is Boss**      | false                |
| **Barre de Vie** | Au-dessus de la tête |
| **HP Typiques**  | 30-80                |
| **PA Typiques**  | 2-3                  |
| **Deck Pattern** | 5-8 cartes           |

**Utilisation:**
- Ennemis de base dans les rencontres
- Multiples par combat
- Patterns simples


### Boss

| Caractéristique  | Valeur                             |
|------------------|------------------------------------|
| **Is Boss**      | true                               |
| **Barre de Vie** | En haut de l'écran (BossHealthBar) |
| **HP Typiques**  | 200-500                            |
| **PA Typiques**  | 3-4                                |
| **Deck Pattern** | 8-12 cartes                        |

**Utilisation:**
- Un seul par combat (typiquement)
- Fin de niveau, événements spéciaux
- Patterns complexes avec phases


## Design de Deck Pattern

### Principes de Design

| Principe                | Description                                            | Exemple                                       |
|-------------------------|--------------------------------------------------------|-----------------------------------------------|
| **Prévisibilité**       | Pattern se répète, joueur peut anticiper               | Attaque â†’ Buff â†’ Attaque                  |
| **Variété**             | Assez de cartes différentes pour ne pas être répétitif | 6-8 cartes minimum                            |
| **Montée en Puissance** | Cartes plus fortes en fin de pattern                   | Carte ultime en dernière position             |
| **Thématique**          | Pattern correspond à l'identité de l'ennemi            | Gobelin archer : majorité d'attaques distance |


### Exemples de Patterns

**Pattern Agressif (Guerrier):**
1. Frappe Rapide (1 PA, 8 dégâts)
2. Frappe Rapide (1 PA, 8 dégâts)
3. Frappe Puissante (2 PA, 15 dégâts)
4. Bouclier (1 PA, +5 bouclier)
5. Frappe Dévastatrice (3 PA, 25 dégâts)

**Durée du Cycle:** 5 tours, puis recommence


**Pattern Support (Chaman):**
1. Eclair (2 PA, 10 dégâts)
2. Soins (2 PA, heal 15 HP)
3. Buff Allié (2 PA, +3 dégâts à tous)
4. Eclair (2 PA, 10 dégâts)
5. Invocation (3 PA, invoque unité)

**Durée du Cycle:** 5 tours, puis recommence


**Pattern Boss (Multi-Phase):**
1. Attaque Basique (2 PA, 12 dégâts)
2. Attaque Basique (2 PA, 12 dégâts)
3. Buff Personnel (2 PA, +5 dégâts)
4. Attaque AoE (3 PA, 15 dégâts rayon 2)
5. Attaque Basique (2 PA, 12 dégâts)
6. Attaque Ultime (4 PA, 30 dégâts rayon 3)

**Durée du Cycle:** 6 tours, puis recommence

**Note:** Les patterns complexes peuvent changer selon les HP du boss (phases).


## Catégories d'Ennemis

### Par Rôle

| Rôle           | HP    | PA  | Style                 | Cartes Typiques               |
|----------------|-------|-----|-----------------------|-------------------------------|
| **Tank**       | 60-80 | 2-3 | Défensif, provocation | Bouclier, Taunt, Régénération |
| **DPS**        | 40-50 | 3-4 | Offensif, burst       | Attaques multiples, Finishers |
| **Support**    | 30-40 | 3-4 | Buff/Heal alliés      | Soins, Buffs, Invocations     |
| **Contrôleur** | 40-50 | 3-4 | Debuff, zone          | Stun, Slow, AOE               |


## Equilibrage

### Formules de Base

**HP Ennemi Normal:**
- Formule : 30 + (10 Ã— Tier)
- Tier 1 : 40 HP
- Tier 2 : 50 HP
- Tier 3 : 60 HP

**HP Boss:**
- Formule : 200 + (50 Ã— Chapter)
- Chapitre 1 : 250 HP
- Chapitre 2 : 300 HP
- Chapitre 3 : 350 HP


### Scaling de Difficulté

| Paramètre    | Facile | Normal | Difficile |
|--------------|--------|--------|-----------|
| **HP**       | -30%   | 100%   | +50%      |
| **Dégâts**   | -20%   | 100%   | +30%      |
| **Défenses** | -20%   | 100%   | +20%      |


## Ennemis Ã€ Créer

### Priorité Haute
- 5-10 ennemis normaux variés
- 1-2 boss par acte
- Un ennemi de chaque élément
- Représentation de chaque rôle (Tank/DPS/Support)

### Priorité Moyenne
- Variantes d'ennemis existants
- Ennemis élites (mini-boss)
- Ennemis avec mécaniques spéciales

### Priorité Basse
- Boss secrets
- Ennemis saisonniers/événements
- Ennemis légendaires


**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Ennemis Project TDB
