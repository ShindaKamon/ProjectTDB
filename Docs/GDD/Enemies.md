# Ennemis - Émotions Tactics (Project TDB)

**Version:** 3.3
**Date:** 1er Octobre 2026
**Statut:** Reflète la structure actuelle (EnemyData)
**Changements :** v2.1 (23/09/2026) encodage réparé, ennemis replacés dans le lore. **v3.0 (23/09/2026)** : réalignement sur l'Excel MVP (onglet « Barème monstres ») — monstres de donjon vs d'aventure, PV et dégâts en ratio des PV joueur, XP = 15 % des PV, cycle de boss Zone / Basique / Heal, règle anti-lock. Les anciennes formules de PV (tiers, chapitres) sont archivées. **v3.1 (24/09/2026)** : stats des monstres selon le nombre de joueurs, faiblesse émotionnelle.
**v3.2 (30/09/2026) :** relecture d'audit — mise à l'échelle des monstres selon le nombre de joueurs (coop locale) rappelée.
**v3.3 (01/10/2026) :** boss de l'Orphelinat repensé en combat à 3 phases (le Monstre sous le lit), remplace le pattern en boucle d'UnderBed.

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

Codé le 27/09/2026 pour la coop locale : `EnemyScaling` porte les facteurs, appliqués par `Enemy.ScaleForPlayers` au lancement du combat (1 joueur = barème inchangé), sans toucher aux assets `EnemyData`.

### Règle anti-lock

Un monstre dont l'action est bloquée par un contrôle (retrait de PM, etc.) fait quand même son **Attaque de base**, insensible au contrôle. Le contrôle de la Peur ralentit donc les monstres sans jamais les neutraliser complètement. Codée le 25/09/2026 : chaque monstre référence sa carte d'attaque de base (`EnemyData.basicAttack`, 0 PA) ; UnderBed : 10 dégâts physiques, portée 1-2 (moitié de son attaque normale). Complétée par la **Ténacité** : après une perte totale de PM, le monstre ignore les retraits de PM à son tour suivant.

## Ennemis du MVP — Donjon Orphelinat (Peur)

| Ennemi | Type | Statut |
|--------|----------------|--------|
| **Ombres du Placard** | Groupe de donjon | Stats et pattern à designer |
| **Monstres Sous le Lit** | Groupe de donjon | Stats et pattern à designer |
| **3ᵉ ennemi** | À définir | Concept à trouver |
| **Moutons de poussière** | Mobs du boss | Codés (valeurs provisoires) ; fusion à venir |
| **Boss : le Monstre sous le lit** | Boss de donjon | Combat en 3 phases (ci-dessous) — design du 01/10/2026, pas encore codé |

Stats à tirer du barème de l'Excel selon le niveau visé pour l'Orphelinat.

### Boss de l'Orphelinat : le Monstre sous le lit *(design du 01/10/2026)*

**Idée :** on ne combat pas un monstre, on **le force à sortir**. Il se cache sous les lits du dortoir ; on lui retire ses cachettes une à une, il fusionne avec le dernier lit, puis il en sort. Le combat change de forme à chaque phase et le terrain se remplit (débris, objets lancés).

**Carte propre au boss :** un dortoir, pas forcément 10×10 (par exemple en longueur), avec plusieurs lits.

| | Phase 1 « Sous les lits » | Phase 2 « Le Lit » | Phase 3 « Il sort » |
|---|---|---|---|
| **Le monstre** | Passe d'un lit à l'autre, sous les lits | Fusionne avec le **dernier lit** : immobile, grand | Sort du lit détruit : il **bouge**, plus agressif au contact |
| **Objectif du joueur** | **Casser les lits** jusqu'à ce qu'il n'en reste qu'un. Le lit où il se trouve **résiste et riposte** (ses draps attirent l'attaquant) : il faut casser les autres | Détruire le Lit | Le vaincre |
| **Lancers d'objets** (zones annoncées un tour avant) | Beaucoup de zones d'**1 case** | Moins de zones, en **cercle de 1** | **Un gros objet** en **cercle de 2**, qui **reste** sur la grille comme obstacle |
| **Draps** | Ligne sur toute la carte depuis son lit : les champions touchés sont **attirés** vers lui (pas de dégâts), jusqu'au premier obstacle | Idem, depuis le Lit | Arrachés : attirent et frappent au contact |
| **Moutons de poussière** | Sortent des lits ; deux moutons côte à côte au tour du boss **fusionnent** en Gros mouton (plus de PV, frappe en zone) | Idem | Idem |

**Les débris :** un lit cassé laisse des **débris** sur sa case (obstacle). Le monstre **ramasse les débris pour les lancer** : un lancer peut consommer un tas de débris (l'obstacle disparaît, la zone visée est touchée). Les débris sont donc à la fois un abri et des munitions pour le boss.

**Étape 1 codée (01/10/2026) — lancers annoncés :** carte **Pluie de jouets** (2 PA, 4 zones d'1 case, une sur chaque champion puis au hasard, 16 dégâts — valeurs provisoires), en tête du pattern actuel d'UnderBed : Pluie de jouets → Agrippe → Marée d'ombre → Tapi dans le noir.

**Phase 1 codée (01/10/2026) — sous les lits :** la rencontre du boss pose **6 lits simples** (une case chacun, tête contre les murs du fond : 3 au nord, 3 à l'est) ; les **PV du boss sont répartis entre les lits** (175 → 30 + 5 × 29) et sa barre affiche leur somme. Le monstre est **invisible**, seule une **ombre** sous son lit le trahit ; au début de chacun de ses tours il passe sous un **autre lit** (tirage) puis joue son pattern depuis là, sans se déplacer ; son lit **résiste** à tous les dégâts, et un soin du boss soigne son lit. Quand il ne reste que son lit, le lit cède et le monstre **sort** sur sa case avec les PV de ce lit (provisoire : les phases 2 et 3 viendront ensuite). Pas encore : la riposte du lit occupé (draps), Sidération sur le monstre caché, les débris.

**Règles de lisibilité :** toute zone d'objet lancé est **annoncée un tour à l'avance** (zone rouge au sol) et tombe au tour suivant du boss ; Sidération (annulation de la prochaine carte) annule aussi un lancer préparé. Les tirages (lit visé, zones) utilisent la graine partagée du combat, pour le réseau.

**Mis de côté (à réfléchir) :** les yeux dans le noir (attaque), le vol de carte, le lit qui avale, la lumière qui s'éteint, les Cauchemars face cachée.

**Chiffres :** les ratios du cycle Zone / Basique / Heal ci-dessous restent la référence des dégâts et soins ; PV des lits, du Lit et du monstre à caler en playtest, puis à reporter dans l'Excel.


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
| **Pattern**      | **Cycle de 3 tours : Zone / Basique / Heal** (référence des dégâts ; le boss de l'Orphelinat se joue en phases, voir plus haut) |

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
