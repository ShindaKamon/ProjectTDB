# Ennemis - Émotions Tactics (Project TDB)

**Version:** 3.1
**Date:** 24 Septembre 2026
**Statut:** Reflète la structure actuelle (EnemyData)
**Changements :** v2.1 (23/09/2026) encodage réparé, ennemis replacés dans le lore. **v3.0 (23/09/2026)** : réalignement sur l'Excel MVP (onglet « Barème monstres ») — monstres de donjon vs d'aventure, PV et dégâts en ratio des PV joueur, XP = 15 % des PV, cycle de boss Zone / Basique / Heal, règle anti-lock. Les anciennes formules de PV (tiers, chapitres) sont archivées. **v3.1 (24/09/2026)** : stats des monstres selon le nombre de joueurs, faiblesse émotionnelle.

> **Chiffres de référence : l'Excel.** Ce document explique les règles.

## Vue d'Ensemble

Dans le lore, les ennemis sont les **émotions d'une personne devenues manifestations physiques** dans son donjon intérieur (voir `GDD_Main.md`). Chaque donjon a donc des ennemis liés à son émotion dominante.

Côté mécanique, les ennemis utilisent un système de **deck pattern** : leurs cartes sont jouées dans un ordre fixe et séquentiel (pas de mélange), une carte par tour.

### Deux familles de monstres (Excel)

| Type | Pour qui | PV (× PV d'un joueur du même niveau) | Dégâts / tour |
|------|----------|--------------------------------------|---------------|
| **Monstre d'aventure** | Solo-friendly | 1× | ≈ 15 % des PV du joueur |
| **Groupe de donjon léger** | Équipe obligatoire (3) | 3× | ≈ 45 % des PV d'un joueur, cumulés |
| **Boss de donjon** | Équipe de 3 | 1.33× les PV de l'équipe | Cycle sur 3 tours (voir ci-dessous) |
| **Superboss** | Équipe de 3 | 3.3× les PV de l'équipe | Cycle sur 3 tours |

- **XP d'un monstre = 15 % de ses PV**
- Barème complet par niveau (1 à 20) : onglet « Barème monstres » de l'Excel
- Ratios validés par playtest

### Monstres selon le nombre de joueurs *(acté le 24/09/2026)*

Un joueur contrôle un seul champion. Un même donjon doit donc marcher à 1 joueur (MVP) comme à 3 (multijoueur, V2) : **les stats des monstres augmentent avec le nombre de joueurs**. Le barème de base est celui d'**un joueur** (monstre d'aventure : 1× PV joueur, ≈ 15 % de ses PV en dégâts par tour).

| Stat | Évolution avec N joueurs | À 3 joueurs | Pourquoi |
|------|--------------------------|-------------|----------|
| **PV** | × N | × 3 (= barème « groupe de donjon » de l'Excel) | Plus de joueurs, plus de dégâts entrants : durée de combat constante |
| **Dégâts (ATK et dégâts des cartes)** | × (1 + 0,5 × (N − 1)) *(à valider)* | × 2 | Un coup ne touche en général qu'un joueur : l'augmenter × 3 rendrait chaque coup mortel. À 3 joueurs, un monstre fait ≈ 30 % des PV d'un joueur par tour, moins que les 45 % du barème de groupe de l'Excel : facteur à régler en playtest |
| **PA, PM, portée, contrôle, pattern** | Inchangés | Inchangés | Le monstre se comporte pareil quel que soit le nombre de joueurs : on apprend son pattern une fois |

Pour le MVP (1 joueur), le multiplicateur vaut 1 : **rien à coder tout de suite**. Le jour du multijoueur, il suffira d'appliquer ces facteurs à l'apparition du monstre (`Enemy.InitializeEnemy`), sans toucher aux assets `EnemyData`.

### Règle anti-lock

Un monstre dont l'action est bloquée par un contrôle (retrait de PM, etc.) fait quand même son **Attaque de base**, insensible au contrôle. Le contrôle de la Peur ralentit donc les monstres sans jamais les neutraliser complètement.

## Ennemis du MVP — Donjon Orphelinat (Peur)

| Ennemi | Type | Statut |
|--------|----------------|--------|
| **Ombres du Placard** | Groupe de donjon | Stats et pattern à designer |
| **Monstres Sous le Lit** | Groupe de donjon | Stats et pattern à designer |
| **3ᵉ ennemi** | À définir | Concept à trouver |
| **Boss** | Boss de donjon | Cycle Zone / Basique / Heal |

Stats à tirer du barème de l'Excel selon le niveau visé pour l'Orphelinat.


## Structure d'un Ennemi (EnemyData)

### Données de Base

| Attribut    | Type            | Description                              |
|-------------|-----------------|------------------------------------------|
| **Nom**     | Text            | Nom de l'ennemi                          |
| **Prefab**  | GameObject      | Modèle 3D/2D de l'ennemi                 |
| **Is Boss** | Boolean         | Si true, barre de vie en haut de l'écran |


### Statistiques de Combat

| Stat                  | Règle                      |
|-----------------------|----------------------------|
| **Max Health**        | Selon le barème (ratio × PV joueur du niveau) |
| **Movement Range**    | 2-4                        |
| **Max Action Points** | 2-4                        |
| **Attaque de base**   | Action de repli insensible au contrôle (anti-lock) |
| **Physical / Magical Defense** | Champs présents dans le code ; **aucune stat de défense n'est définie** dans le design actuel (question ouverte « système de stats ») |

### Faiblesse émotionnelle *(actée le 24/09/2026)*

- Chaque monstre peut avoir **une faiblesse** : une émotion (Colère, Peur ou Joie pour le MVP).
- Les cartes de cette émotion lui infligent **+25 % de dégâts** (valeur de départ, à valider en playtest). Les Signatures, sans émotion, ne la déclenchent jamais.
- Pas de résistance pour le MVP (une faiblesse seule suffit à orienter le deck, sans punir un deck mono-couleur).
- La faiblesse est **visible** sur le monstre (icône de la couleur de l'émotion), pour que le joueur puisse choisir son deck en connaissance de cause.
- Le choix de la faiblesse d'un monstre se fait en le concevant (ex. : un monstre de l'Orphelinat, donjon de la Peur, pourrait être faible à la Joie).

À implémenter : un champ `weakness` (EmotionType) dans `EnemyData` et le bonus dans le calcul des dégâts de `CardData`.

### Deck Pattern (Combat Deck)

| Caractéristique    | Ennemis            | Champions (Comparaison)    |
|--------------------|--------------------|----------------------------|
| **Type de Pioche** | Séquentielle       | Aléatoire (mélangé)        |
| **Ordre**          | Fixe, se répète    | Aléatoire à chaque pioche  |
| **Taille Typique** | 3-12 cartes        | 24 cartes                  |
| **Stratégie**      | Pattern prévisible | Imprévisible               |


### Visual Settings

| Paramètre             | Description                                      |
|-----------------------|--------------------------------------------------|
| **Health Bar Offset** | Position de la barre de vie au-dessus de la tête |
| **Health Bar Color**  | Couleur de la barre de vie (typiquement rouge)   |


## Différence Normaux vs Boss

### Ennemis Normaux (aventure et groupes de donjon)

| Caractéristique  | Valeur               |
|------------------|----------------------|
| **Is Boss**      | false                |
| **Barre de Vie** | Au-dessus de la tête |
| **PV**           | Selon le barème      |
| **Deck Pattern** | 5-8 cartes           |


### Boss

| Caractéristique  | Valeur                             |
|------------------|------------------------------------|
| **Is Boss**      | true                               |
| **Barre de Vie** | En haut de l'écran (BossHealthBar) |
| **PV**           | 1.33× (boss) ou 3.3× (superboss) les PV de l'équipe |
| **Pattern**      | **Cycle de 3 tours : Zone / Basique / Heal** |

**Cycle de boss (ratios validés par playtest) :**
1. **Zone** : ≈ 25 % des PV d'un joueur (à chaque cible touchée)
2. **Basique** : ≈ 30 % des PV d'un joueur
3. **Heal** : le boss récupère ≈ 30 % de ses PV sur le cycle


## Design de Deck Pattern

### Principes de Design

| Principe                | Description                                            | Exemple                                       |
|-------------------------|--------------------------------------------------------|-----------------------------------------------|
| **Prévisibilité**       | Pattern se répète, joueur peut anticiper               | Attaque → Buff → Attaque                      |
| **Variété**             | Assez de cartes différentes pour ne pas être répétitif | 6-8 cartes minimum                            |
| **Montée en Puissance** | Cartes plus fortes en fin de pattern                   | Carte ultime en dernière position             |
| **Thématique**          | Pattern correspond à l'émotion incarnée par l'ennemi   | Une Ombre du Placard frappe depuis l'obscurité puis se cache |


### Exemples de Patterns

> ⚠️ Exemples **hérités des premiers brouillons** : noms génériques (Guerrier, Chaman) et dégâts en valeurs absolues qui ne suivent pas le barème de l'Excel (dégâts en % des PV joueur). À refaire pour l'Orphelinat.

**Pattern Agressif (gabarit « Guerrier ») :**
1. Frappe Rapide (1 PA, 8 dégâts)
2. Frappe Rapide (1 PA, 8 dégâts)
3. Frappe Puissante (2 PA, 15 dégâts)
4. Bouclier (1 PA, +5 bouclier)
5. Frappe Dévastatrice (3 PA, 25 dégâts)

**Durée du Cycle :** 5 tours, puis recommence


**Pattern Support (gabarit « Chaman ») :**
1. Éclair (2 PA, 10 dégâts)
2. Soins (2 PA, heal 15 HP)
3. Buff Allié (2 PA, +3 dégâts à tous)
4. Éclair (2 PA, 10 dégâts)
5. Invocation (3 PA, invoque unité)

**Durée du Cycle :** 5 tours, puis recommence


> Le pattern de boss à 6 cartes des premiers brouillons est remplacé par le **cycle Zone / Basique / Heal** ci-dessus (archivé).


## Catégories d'Ennemis

### Par Rôle

| Rôle           | PV (dans le budget du barème) | PA  | Style                 | Cartes Typiques               |
|----------------|-------|-----|-----------------------|-------------------------------|
| **Tank**       | Haut | 2-3 | Défensif, provocation | Bouclier, Taunt, Régénération |
| **DPS**        | Bas | 3-4 | Offensif, burst       | Attaques multiples, Finishers |
| **Support**    | Bas | 3-4 | Buff/Heal alliés      | Soins, Buffs, Invocations     |
| **Contrôleur** | Moyen | 3-4 | Debuff, zone          | Stun, Slow, AOE               |


## Équilibrage

Barème par niveau et ratios : onglet « Barème monstres » de l'Excel. Difficulté : `Combat_System.md`.


## Ennemis À Créer

### Priorité Haute
- Les monstres de l'Orphelinat (Peur) + boss, selon le barème
- Des monstres d'aventure (solo-friendly) pour jouer avec un seul champion
- Au moins une variante par émotion de donjon prévue en Bêta

### Priorité Moyenne
- Variantes d'ennemis existants
- Ennemis élites (mini-boss)
- Ennemis avec mécaniques spéciales

### Priorité Basse
- Boss secrets
- Ennemis saisonniers/événements
- Ennemis légendaires


**Dernière mise à jour :** 23 Septembre 2026
**Version :** 2.1
**Responsable :** Shinda + Claude
