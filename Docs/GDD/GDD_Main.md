# Game Design Document - Émotions Tactics (nom de code : Project TDB)

**Version :** 3.4
**Date :** 23 Septembre 2026
**Statut :** Document canon central. Réaligné le 23/09/2026 sur le classeur **`TCG_Tactique_Systeme_de_calcul.xlsx`**, qui fait office de **référence du MVP** (roster, émotions, deck, budget des cartes, progression, monstres).

> Ce document est le point d'entrée du projet. Pour le détail, voir les documents listés ci-dessous. Les concepts abandonnés, mis en pause ou sortis du MVP sont conservés dans `archive/Concepts_Abandonnes.md` — rien n'est perdu, juste rangé.

## 🗺️ Carte des documents du projet

**Référence MVP**
- `TCG_Tactique_Systeme_de_calcul.xlsx` (projet claude.ai ; copie texte dans le repo : `MVP_Excel_Snapshot.md`) — **le MVP chiffré** : calculateur de cartes (budget PA + modificateurs), bibliothèque de cartes (49 Standard + Signatures), suivi de deck, progression des champions, barème des monstres, fiches des 3 champions, roadmap des décisions.

**Index et état du code**
- `README.md` — index des documents (repo).
- `MVP_Excel_Snapshot.md` — copie texte de l'Excel MVP, lisible par Claude Code.
- `Technical_Specs.md` § « État du code » — ce qui est réellement implémenté dans Unity.

**Vision et cadrage**
- `GDD_Main.md` *(ce document)* — vision globale, décisions actées, questions ouvertes. Point de départ.
- `claude_md_coarchitect.md` — le « contrat de collaboration » avec Claude (rôle et façon de travailler, sans résumé du jeu).

**Émotions et personnages**
- `SYSTEME_EMOTIONS.md` — les 8 émotions/familles (Plutchik), les 3 émotions de lancement, le système d'Éveil.
- `CHAMPIONS_CONCEPTS.md` — roster MVP (Evan, Crux, Raze) et concepts hors MVP.
- `ilya_deck_simple.md` — fiche d'Ilya (**hors MVP**, conservé comme concept complet).
- `astra_noctis_simple.md` — fiche des Jumeaux Astra & Noctis (**hors MVP**).
- `Characters.md` — structure technique d'un champion (ChampionData).
- `personnages_a_developper.md` — réservoir de 100 concepts de personnages.

**Systèmes de jeu**
- `Card_System.md` — types de cartes (Standard / Éveil / Signature), identité émotionnelle, budget de puissance, ciblage, zones.
- `Combat_System.md` — règles de combat (tour, ressources, statuts, contrôle, main).
- `Grid_System.md` — grille, déplacement, ligne de vue.
- `Enemies.md` — monstres (donjon / aventure), barème, deck pattern.
- `Progression.md` — niveaux, PV, slots, XP, économie.

**Interface et technique**
- `UX_Flow.md` — parcours joueur écran par écran.
- `UI_Design.md` — design de l'interface, palette, désaturation narrative.
- `Technical_Specs.md` — architecture Unity, patterns de code.

**Archive**
- `archive/Concepts_Abandonnes.md` — Ayla, classes, Éléments, gacha, ancien lore, ancienne jauge -100/+100, ancienne boucle roguelike, anciennes règles de progression et de cartes remplacées par l'Excel.

## 📌 Source unique de vérité

Chaque information n'a qu'**un seul document de référence**. En cas de doute, c'est la source qui fait foi.

| Information | Référence |
|-------------|-----------|
| **Chiffres du MVP** : budget des cartes, liste des cartes, profils PA/PM, PV par niveau, XP, barème monstres, champions MVP | **`TCG_Tactique_Systeme_de_calcul.xlsx`** |
| Vision, lore, décisions, questions ouvertes | `GDD_Main.md` |
| Émotions/familles, couleurs, Éveil | `SYSTEME_EMOTIONS.md` |
| Règles de combat (tour, main, statuts, contrôle) | `Combat_System.md` |
| Règles de deck et de cartes (explication) | `Card_System.md` |
| Grille, déplacement | `Grid_System.md` |
| Parcours joueur, raccourcis | `UX_Flow.md` |
| Visuel, palette, désaturation | `UI_Design.md` |
| Architecture et code | `Technical_Specs.md` |
| **Écarts entre le code Unity et le design** | `Technical_Specs.md`, section « État du code » |

Les documents texte **expliquent** les règles ; l'Excel **porte les chiffres**. Quand un chiffre change dans l'Excel, il ne faut pas le recopier ailleurs — seulement renvoyer vers l'Excel.

## Vision du Jeu

**Émotions Tactics** est un jeu de combat tactique au tour par tour sur grille qui fusionne la profondeur stratégique des jeux de grille (Dofus, Waven) avec le deck building. Le joueur contrôle des champions, construit des decks autour d'**émotions** (Colère, Peur, Joie au lancement) et chaque champion apporte une **mécanique signature** (passif + cartes Signature).

### Univers narratif *(11/09/2026, ajusté le 23/09/2026)*

Le monde ne s'effondre pas d'un coup — il **grisonne**. À force que les gens négligent le monde et leurs propres émotions, un surplus s'accumule et déborde. Cette accumulation ronge la couleur : chaque émotion a la sienne, et ceux qui laissent leurs émotions déborder sans jamais les affronter perdent la leur, jusqu'à devenir gris, vides, absents à eux-mêmes. Dans les cas extrêmes, ce trop-plein se cristallise en un **donjon intérieur** — l'esprit de cette personne, peuplé par ses émotions devenues manifestations physiques.

Pas de gouvernement oppressif, pas d'organisation secrète. Les champions sont des gens qui ont gardé — ou reconquis — **leur propre couleur**, qui leur permet de percevoir et d'entrer dans ces espaces gris. **Tout champion peut entrer dans n'importe quel donjon** (règle « uniquement sa propre famille » retirée le 23/09/2026).

Chaque champion du MVP porte un **trauma** qui fonde sa mécanique (voir `CHAMPIONS_CONCEPTS.md`) : Evan n'arrive pas à laisser partir sa sœur jumelle Lyse, Crux ne supporte plus de laisser quelqu'un hors de portée, Raze ne laisse plus jamais le hasard décider.

**Exemples de donjons** :
- **Orphelinat** : Enfants prisonniers de la Peur → Ennemis : Ombres du Placard, Monstres Sous le Lit — **donjon du MVP**
- **Bureau Corporatiste** : Employé en burnout (Anxiété) → Ennemis : Dossiers oppressants, Horloges tyranniques
- **Maison Familiale** : Adulte traumatisé (Colère) → Ennemis : Mots blessants, Poings spectraux

**Thème central** : L'équilibre émotionnel. La victoire = transformation de l'émotion négative en positive (Peur → Prudence, Colère → Affirmation, Tristesse → Acceptation) — et, visuellement, le retour de la couleur dans un lieu devenu gris.

## Les Émotions

**Vision long terme : 8 émotions** (roue de Plutchik), chacune avec sa couleur — voir `SYSTEME_EMOTIONS.md`.

**MVP : 3 émotions de lancement** (acté dans l'Excel) :

| Émotion | Rôle | Force / Faiblesse |
|---------|------|-------------------|
| **Colère** | Agressif | Burst, sans sustain |
| **Peur** | Contrôle (retrait de PM, poussée/tirage) | Contrôle / tempo |
| **Joie** | Soin / valeur | Survie, mais lent |

Un champion peut jouer **toutes les émotions**, mais **chaque deck a 1 ou 2 couleurs**, choisies à sa création, et ne contient que des cartes de ces couleurs (décision du 24/09/2026). **4 exemplaires maximum** par carte. Les cartes Signature sont **Neutres**, réservées à leur champion et **obligatoires** dans son deck (**2 exemplaires chacune**, décision du 28/09/2026).

## Roster MVP *(Excel, 23/09/2026)*

| Champion | Trauma | Passif | Cartes Signature |
|----------|--------|--------|------------------|
| **Evan** | A perdu sa sœur jumelle Lyse | Miroir fraternel (ses invocations rejouent un écho de ses cartes offensives à ~40 %) | Invocation de Lyse (2 PA), Écho évanescent (1 PA) |
| **Crux** | Accident de cordée filmé, confiance brisée | Réflexe du grimpeur (quand une de ses cartes le met au contact d'une unité — Grappin, Bond percutant, tirage de Corde de rappel : bouclier de 15 avec un allié, +15 % de dégâts sur la prochaine carte avec un ennemi) | Grappin (2 PA), Corde de rappel (2 PA) |
| **Raze** | A tout perdu sur une main légendaire | Main gagnante (bonus selon le motif des coûts joués : Paire / Suite / Bluff) | Triche (1 PA), Tapis (3 PA) |

Détail : `CHAMPIONS_CONCEPTS.md` et onglet « Champions » de l'Excel.

**Hors MVP :** Ilya (concept complet, sa mécanique Rage est à réadapter au système Éveil) et les Jumeaux Astra & Noctis.

## Piliers de Design

### 1. Émotions et Couleur
- Les cartes ont une **identité émotionnelle** ; le deck se construit autour d'1 ou 2 émotions
- Les cartes génèrent de l'**Éveil** (une jauge par émotion) qui débloque des cartes fortes — concept acté, mise en œuvre concrète repoussée
- **La couleur est un pilier visuel autant que narratif** : un donjon est désaturé à l'entrée et retrouve sa couleur à la victoire (voir `UI_Design.md`)

### 2. Mécanique Signature par Champion (pas de système de classe)
- Chaque champion = 1 passif + 2 cartes Signature, pensés autour de son trauma
- Pas de grille Famille × Classe (décision du 10/09/2026)

### 3. Ressources et Équilibrage

| Ressource | Règle |
|-----------|-------|
| **PA + PM** | **Budget fixe de 9 points par tour**, réparti par profil de personnage (min 3 PA, min 2 PM). Ex : brutal 6/3, équilibré 5/4, mobile 4/5. Identique à tous les niveaux. |
| **PV** | 100 au niveau 1, +15 par niveau |
| **Éveil** | Une jauge par émotion, alimentée par les cartes (voir `SYSTEME_EMOTIONS.md`) |
| **Armure / Résistance magique** | Réduction **fixe** des dégâts physiques (armure) ou magiques (résistance magique) ; type de dégâts choisi par carte ; stats de base des champions et monstres (0 par défaut), modifiables par les cartes (voir `Combat_System.md`) |
| **Autres stats** (résistances, critique…) | **Non définies** — à trancher |

**Règle d'or** : le niveau d'un champion n'augmente **jamais** la puissance des cartes ni les PA/PM. Il n'augmente que les PV, les passifs et les slots de cartes.

### 4. Budget de puissance des cartes

Valeur finale d'une carte = **Baseline(coût en PA) × (1 + somme des modificateurs)**. Baseline : 1 PA = 12, 2 PA = 26, 3 PA = 42, 4 PA = 60, 5 PA = 80, 6 PA = 102. Plus une carte a de portée, de zone, de contrôle ou de déplacement forcé, moins elle fait de dégâts bruts. Détail : `Card_System.md` et l'Excel.

### 5. Positionnement Tactique sur Grille
- **Grille carrée en 4 directions** *(24/09/2026)* : 10×10, pas de diagonales (distance de Manhattan) pour le déplacement, la portée, les zones, les charges et l'adjacence ; un cercle de rayon 1 = 5 cases, de rayon 2 = 13 cases. ⚠️ Le budget de l'Excel suppose des zones de 9 et 25 cases (8 directions) : le coût des cartes à zone est à revoir dans l'Excel. La Roadmap parle encore d'une grille hexagonale : à corriger. Détail : `Grid_System.md`
- Portées de 1 (mêlée) à 6 cases
- Zones : ligne, cône, cercle, cibles multiples, contagion, équipe entière

## Structure de jeu *(23/09/2026)*

**Campagne façon Waven** : donjons fixes enchaînés, progression persistante (pas de roguelike). L'Excel distingue deux types de contenus :
- **Donjons** : monstres de groupe, prévus pour une **équipe de 3** (jouer en groupe est obligatoire)
- **Aventure** : monstres « solo-friendly », jouables avec un seul champion

**Un champion par joueur** *(24/09/2026)* : le joueur contrôle un seul champion ; l'équipe de 3 viendra avec le multijoueur (V2). Les stats des monstres (PV, dégâts) augmentent avec le nombre de joueurs : un même donjon marche à 1 comme à 3 (voir `Enemies.md`).

Détail : `UX_Flow.md`, `Enemies.md`, `Progression.md`.

## Boucle de Combat

Dans le code actuel, chaque unité joue **un tour par unité**, dans l'ordre de la liste des unités (le champion, puis chaque ennemi ; les invocations comme Lyse sont sautées). L'ordre définitif (tours individuels ou phases) reste à confirmer.

```
1. INITIALISATION (main de départ)
   ↓
2. TOUR D'UN CHAMPION
   • Pioche 1 carte (règle définitive à trancher — voir Combat_System.md)
   • PA et PM restaurés selon le profil du champion
   • Jouer des cartes / se déplacer (ordre libre)
   ↓
3. TOUR D'UN MONSTRE (chacun à son tour)
   • Joue la prochaine carte de son pattern
   • Action bloquée par un contrôle → Attaque de base (règle anti-lock)
   ↓
4. VERIFICATION
   • Tous les ennemis vaincus ? → VICTOIRE
   • Tous les champions vaincus ? → DEFAITE
   ↓
5. RECOMPENSES (si victoire)
```

## Architecture Technique

Détail complet dans `Technical_Specs.md`. Patterns : Service Locator, Event Bus, State Machine (TurnStateMachine), Component Pattern, Repository Pattern, ScriptableObjects (CardData, ChampionData, EnemyData).

## État Actuel du Projet

### Systèmes Implémentés (code Unity, vérifié le 23/09/2026)

Détail et écarts avec le design : `Technical_Specs.md`, section « État du code ».

**Core :** Unity 6 (6000.4), Service Locator + façade `Services`, EventBus typé, TurnStateMachine, GridManager/GridRepository (**grille carrée 10×10**), tests EditMode.
**Champions jouables :** Raze, Crux, Evan (+ invocation Lyse) ; Ilya, Vylos et Calyx ont été retirés du code le 24/09/2026 (récupérables via le commit `00afe5d`).
**Cartes :** `CardData` data-driven avec catégorie Standard / Éveil / Signature et émotion ; 49 cartes Standard (17 Colère, 17 Peur, 15 Joie) + Signatures des 3 champions.
**Decks :** 20 cartes (4 Signature, soit 2 exemplaires de chacune des 2 Signatures du champion, + 16 Standard — les 6 slots Éveil ne sont pas encore implémentés), 1 ou 2 couleurs par deck (le deck de base en a 3), 4 exemplaires max, Signatures obligatoires, 1 deck de base + 3 decks perso par champion, sauvegarde JSON.
**Ennemis :** deck pattern + IA ; 1 ennemi (UnderBed).
**UI :** écran de sélection de champion, éditeur de deck façon MTG Arena, HUD de combat, main en arc, ciblage (courbe + réticule), barre de vie de boss, preview des cartes ennemies, pop-ups de dégâts.
**Pas encore dans le code :** jauge d'Éveil, cartes d'Éveil, profils PA/PM (les 3 champions sont en 5 PA / 4 PM, soit le profil « équilibré »), désaturation des donjons.

### Design fait (Excel)
- [x] Système de budget de cartes + calculateur
- [x] 49 cartes Standard (17 Colère, 17 Peur, 15 Joie) — la Roadmap de l'Excel dit encore 36
- [x] 3 champions complets (passif + 2 Signatures)
- [x] Progression (PV, XP, budget PA/PM)
- [x] Barème des monstres par niveau
- [x] Playtest papier (triangle Colère/Peur/Joie validé)

### Reste à faire pour le MVP
- [ ] Cartes d'Éveil (6 par deck) — mise en œuvre de l'Éveil
- [x] Règle de main/pioche : celle du code (départ 5, max 5, pioche 1/tour) — 24/09
- [x] Grille carrée 4 directions (24/09) — reste à corriger la Roadmap de l'Excel (« hexagonale ») et le budget des zones (5 / 13 cases au lieu de 9 / 25)
- [ ] Monstres de l'Orphelinat (stats selon le barème, patterns)
- [ ] Adapter le code : Éveil (jauge + 6 slots de deck), cycle de boss (statuts de contrôle et anti-lock : faits le 25/09)
- [ ] Désaturation visuelle des donjons

## Objectifs de Design

### Court Terme (MVP)
- 3 champions : Evan, Crux, Raze
- 3 émotions : Colère, Peur, Joie
- 1 donjon complet : l'Orphelinat (Peur)
- Monstres de donjon + boss (cycle Zone / Basique / Heal)
- Premier passage désaturé → couleur

### Moyen Terme (Bêta)
- Plusieurs donjons (Bureau/Anxiété, Maison/Colère…)
- Cartes bi-émotion dédiées, oppositions d'émotions (paires Plutchik)
- Nouveaux champions (Ilya, Jumeaux…)

### Long Terme (1.0)
- Extension vers les 8 émotions
- Campagne complète, modes additionnels, polish

## Inspirations

| Jeu | Éléments Repris |
|-----|------------------|
| **Waven** | Donjons de groupe, campagne, deck building |
| **Dofus** | Tactique PA/PM, contrôle par retrait de PM, positionnement |
| **Slay the Spire** | Lisibilité des intentions ennemies, récompenses de cartes |
| **Magic the Gathering** | Identités de couleur (mono/bi-émotion), budget de cartes |
| **Chaos Zero Nightmare** | UI des cartes, système EGO |
| **Darkest Dungeon** | Émotions et traumas comme moteur |

---

## Décisions actées

| Sujet | Décision | Date |
|-------|----------|------|
| Nom de code | Project TDB (titre final non tranché ; l'Excel dit « TCG Tactique ») | 10/09 |
| Système de classe | Abandonné — mécaniques signatures individuelles | 10/09 |
| Ayla | Abandonnée | 10/09 |
| Gacha | Abandonné, monétisation à définir | 10/09 |
| Lore | « Le monde grisonne », les champions ont gardé leur couleur | 11/09 |
| Désaturation visuelle | Donjons désaturés, couleur restaurée à la victoire | 11/09 |
| Accès aux donjons | Tout champion peut entrer dans tout donjon | 23/09 |
| Ancienne jauge -100/+100 | En pause (archivée) — remplacée par l'Éveil | 23/09 |
| Structure de jeu | Campagne façon Waven, pas de roguelike | 23/09 |
| **Référence MVP** | **`TCG_Tactique_Systeme_de_calcul.xlsx`** | **23/09** |
| **Roster MVP** | **Evan, Crux, Raze** (Ilya et Jumeaux hors MVP) | **23/09** |
| **Émotions de lancement** | **Colère, Peur, Joie** | Excel |
| **Deck** | **24 cartes : 2 Signature + 6 Éveil + 16 Standard ; 1 ou 2 couleurs par deck, choisies à sa création ; plusieurs decks par champion** | Excel |
| **Ressources** | **Budget PA+PM = 9 par profil, fixe quel que soit le niveau** | Excel |
| **Progression** | **Le niveau n'augmente que PV, passifs, slots ; XP = 100 × niveau ; XP monstre = 15 % de ses PV** | Excel |
| **Budget de cartes** | **Baseline par PA × (1 + modificateurs)** | Excel |
| **Anti-lock** | **Un monstre bloqué fait une Attaque de base** | Excel |
| **Monstres** | **Donjon (groupe obligatoire) vs Aventure (solo-friendly)** | Excel |
| **Grille** | **Carrée, 4 directions** (pas de diagonales, distance de Manhattan) pour le déplacement, la portée, les zones, les charges et l'adjacence, écho du Miroir fraternel compris. Le budget de l'Excel (zones de 9 / 25 cases) est à revoir | 24/09 |
| **Main et pioche** | **Règle du code** : main de départ 5, 5 cartes max, 1 carte piochée par tour ; **depuis le 29/09** : pioche sans limite pendant le tour (pour que les cartes de pioche servent), défausse au choix de l'excédent au-delà de 5 en fin de tour (message « Main pleine : défausse N cartes »). Vision cauchemardesque renommée **Hantise** (nom trop long) | 24/09, 29/09 |
| **Ordre des tours** | **Chaque unité joue à son tour** (pas de phases) | 24/09 |
| **Joueurs** | **Un seul champion par joueur** ; les équipes de 3 viendront avec le multijoueur (V2) | 24/09 |
| **Monstres et nombre de joueurs** | **PV × N joueurs, dégâts × (1 + 0,5 × (N − 1))** (à valider) ; PA, PM, portée et pattern inchangés — barème de base = 1 joueur (voir `Enemies.md`) | 24/09 |
| **Faiblesse émotionnelle** | **Une émotion par monstre, +25 % de dégâts des cartes de cette émotion** (à valider), visible sur le monstre ; pas de résistance au MVP | 24/09 |
| **Pool Standard** | **49 cartes** (17 Colère, 17 Peur, 15 Joie) pour l'instant — la Roadmap de l'Excel dit 36, à corriger | 24/09 |
| **Noms de familles** | **Abandonnés** (Déchaînés, Réprouvés, Éveillés…) : on parle directement des émotions | 24/09 |
| **Palette des émotions** | **Celle du codex** (Colère #D64545, Peur #3F9D5C, Joie #D9A91F…), seule référence, codée dans `CodexCardVisual` ; l'ancienne (#CC0000, #006600, #FFEB00) est abandonnée | 24/09 |
| Éveil | Mis de côté pour l'instant, à réfléchir plus tard | 24/09 |
| Signatures renommées | « Il triche » → **Triche**, « Corde de rappel forcé » → **Corde de rappel**, « Écho de Lyse » → **Écho évanescent** (renommer aussi dans l'Excel) | 24/09 |
| Écho évanescent | Ciblage en 2 étapes : choisir une invocation, puis une case libre à 1-3 cases d'elle (4 directions) ; injouable sans invocation | 24/09 |
| Invocation de Lyse | Rejouée quand Lyse est déjà sur le terrain : la soigne de 15 PV au lieu de la réinvoquer ; elle cible alors **Lyse elle-même**, où qu'elle soit (28/09) | 24/09, 28/09 |
| Textes des cartes | **Générés depuis les données** de la carte (`CardRulesText.Build`) : une ligne par effet avec son icône (« ↗ Inflige 33 »), puis « Cible : 1 ennemi · au contact », « Zone : cercle de 1 (ennemis) », la contrepartie en orange et, si besoin, « Spécial : … » (champ `specialText`, seulement pour une règle que les champs ne décrivent pas). Pas de texte d'ambiance pour l'instant. Le texte du codex reste dans `description` (recherche) mais n'est plus affiché ; Aura de terreur retire **1 PA** (calcul Excel) ; Bouclier de la terreur : le lanceur perd 1 PM à son prochain tour (`casterMovementLoss`) | 25/09 |
| Miroir fraternel | Portée = celle de la carte jouée, mesurée depuis Lyse en 4 directions (comme toute la grille) ; cible = celle d'Evan si à portée, sinon la plus proche ; automatique (MVP). Carte à cibles multiples (ex. Frappe rapide) : **un écho par cible**, chacun sur un ennemi différent, selon les mêmes règles de portée — Lyse renvoie la même attaque qu'Evan. L'écho vaut **40 % de l'attaque d'origine d'Evan** (avant la défense de sa cible), puis **la défense de la cible de Lyse** s'applique, sans minimum (ex. Evan 30 sur UnderBed armure 10 → UnderBed subit 20 ; écho 12 → 2 sur UnderBed, 12 sur un mob sans armure) ; pas d'écho si le coup d'Evan n'a rien infligé ; il tombe **0,5 s après** le coup d'Evan (le bouclier l'absorbe) | 24/09, 28/09 |
| Construction de deck | Un champion peut jouer toutes les émotions, mais chaque deck a 1 ou 2 couleurs (choisies à la création) et ne contient que ces couleurs ; 4 exemplaires max par carte ; Signatures du champion obligatoires, **2 exemplaires chacune** (28/09 ; 1 auparavant) ; Signatures des autres champions interdites ; **seul un deck complet (20 cartes) peut partir au combat** (28/09) ; **les couleurs d'un deck perso se changent sans le supprimer** (bouton « Couleurs », toujours 1 ou 2) : les cartes qui ne sont plus de ses couleurs restent, en rouge, et bloquent le deck jusqu'à ce qu'on les retire (28/09) | 24/09, 28/09 |
| Bouclier | En **PV** (et non en %) : absorbe les dégâts avant les PV, jauge bleue sur la barre de vie (orbe de vie du champion en bleu clair, montant sous les PV), cumulable, **sans durée : reste tant qu'il n'est pas consommé** (28/09 ; bouclier de 23, 20 dégâts → reste 3), ignoré par la Paire de Raze. Les réductions en % restent pour les passifs de Crux et Raze ; Aura de terreur retire **1 PA** (calcul Excel) ; Bouclier de la terreur : le lanceur perd 1 PM à son prochain tour (`casterMovementLoss`) | 25/09 |
| Armure et résistance magique | Deux stats distinctes du bouclier : l'armure réduit les dégâts physiques, la résistance magique les magiques, par **soustraction fixe** (min. 1) ; le type de dégâts est choisi **par carte** ; stats de base sur champions et monstres (0 par défaut) ; la Paire de Raze perce le bouclier mais **pas** l'armure ni la résistance magique ; Aura de terreur retire **1 PA** (calcul Excel) ; Bouclier de la terreur : le lanceur perd 1 PM à son prochain tour (`casterMovementLoss`) | 25/09 |
| Durée des effets | Comptée **en tours du lanceur** : « 1 tour » = jusqu'au début du prochain tour de celui qui a joué la carte (buffs, malus, vulnérabilité, bouclier réactif pas encore déclenché ; le bouclier, lui, n'a pas de durée) ; un effet dont le lanceur meurt expire. Les retraits de PA/PM restent « au prochain tour de la cible » | 25/09 |
| Revue Joie / Peur | Vulnérabilité = armure du lanceur (Communion joyeuse et Euphorie aveuglante : −5, valeur à équilibrer) ; Voile d'ombre = bouclier de 11, Armure de rage (23) et Bouclier de la terreur (27) remis en bouclier comme le codex ; Éclat de joie blesse **tous** les ennemis au contact de l'allié soigné ; Réflexe de survie = bouclier réactif qui absorbe le 1er coup ; nouveaux champs `casterRetreat`, `casterMovementGain` / `casterActionGain`, `casterArmorAmount`, `damageAroundTarget`, `reactiveShield`, `removeAllMovement` ; les PM/PA du boss sont affichés (restants pendant son tour, sinon ceux de son prochain tour) ; Aura de terreur retire **1 PA** (calcul Excel) ; Bouclier de la terreur : le lanceur perd 1 PM à son prochain tour (`casterMovementLoss`) | 25/09 |
| Anti-lock et Ténacité | Monstre contrôlé (PA ou PM retirés) qui ne peut pas jouer sa carte → **attaque de base** gratuite (`EnemyData.basicAttack`, UnderBed : 10 dégâts, portée 1-2). Après une **perte totale de PM**, le monstre ignore les retraits de PM à son tour suivant (option A, contre le blocage permanent) | 25/09 |
| ATQ (option B) | Montée d'adrénaline / Chant d'encouragement : **+N dégâts fixes sur la prochaine carte offensive** de la cible (`nextAttackBonus`), consommés par la première carte qui inflige des dégâts ; cumulable, sans expiration ; ajoutés avant le % de Crux, puis l'armure / résistance magique de la cible. La stat ATQ reste descriptive (n'entre dans aucun calcul) ; le HUD affiche le bonus en attente (« ATQ : 15 (+23) ») | 25/09 |
| Bond percutant et Tapis | **Bond** (`leapToTarget`) : le lanceur saute sur une **case vide** (pour l'instant il s'y **téléporte**, l'animation viendra avec les assets — 28/09) à portée (par-dessus les unités, pas forcément en ligne droite, contrairement à la charge), puis la zone se déclenche autour de son point d'arrivée ; il déclenche le Réflexe du grimpeur comme une charge. Tapis vise **2 ennemis** (Excel) | 25/09 |
| Coop locale | **1 à 3 joueurs sur un seul PC** (hot-seat), via Menu principal → Multijoueur → Créer → **salon** (une case par joueur, Changer / Retirer, Commencer dès 2 joueurs ; Rejoindre réservé au jeu en ligne), un champion et un deck chacun, **champions uniques** ; ordre des tours = ordre d'inscription ; un champion à 0 PV est hors-jeu, les autres continuent. Monstres : PV × N, dégâts × (1 + 0,5 × (N − 1)), codés tels quels (à valider en playtest). Le multijoueur en ligne reste en V2 | 27/09 |
| Phase de placement | **Façon Dofus, en solo comme en coop** : 6 cases de départ (rouges), chaque champion y apparaît, puis chaque joueur place le sien à tour de rôle (clic sur une case rouge libre, sans toucher aux champions des autres), « Joueur suivant » puis « Lancer le combat » ; « Lancer le combat » démarre le premier tour ; boss à position fixe ; monstres ordinaires sur cases bleues quand il y en aura (voir `Grid_System.md`) | 27/09 |
| Zone en cône | Le cône part de la **case visée** et s'élargit en s'éloignant du lanceur : rangées de **1, 3, 5… cases** (2 de plus par rangée), autant de rangées que le rayon de zone (Onde de terreur, rayon 3 : 9 cases) | 28/09 |
| Zone en ligne | La ligne part de la **case visée** et s'éloigne du lanceur (ligne de 3 = la cible + 2 cases derrière, lanceur non touché), pour toutes les cartes « Ligne » ; l'aperçu de zone montre la forme réelle | 28/09 |
| Éclat de rage | Cible **1 ennemi ou 1 case, en croix** (même ligne ou même colonne qu'Evan, pas de diagonale), portée 1-5, puis ligne de 3 cases | 28/09 |
| Ciblage par case | On vise toujours une case : une unité ne masque jamais la case derrière elle (déplacement et cartes) ; l'unité posée sur la case visée est la cible | 28/09 |
| Mobs du boss | UnderBed est accompagné de **2 Moutons de poussière** (à sa gauche et à sa droite, une case entre chacun) : 50 PV, 3 PM, 2 PA, carte Mordille (2 PA : 8 dégâts au contact, vol de vie de 4 PV) et attaque de base gratuite Roulé-boulé (4 dégâts au contact), jouée **dès que leur carte est injouable** (le boss, lui, seulement s'il est contrôlé) — valeurs provisoires, visuel = UnderBed réduit. Leur prochaine carte s'affiche en petit (moitié de celle du boss), côte à côte sous celle du boss | 28/09 |
| Équilibrage de départ | Champions : **100 PV**, PA/PM selon le profil de l'Excel (budget 9) : **Evan 5/4** (équilibré), **Crux 4/5** (mobile), **Raze 6/3** (brutal) ; ATQ à 0 (elle ne sert qu'aux bonus de prochaine attaque). UnderBed **175 PV**, armure et RM **4** ; Moutons **50 PV**. Cible : combat solo de 5 à 6 tours, un champion ciblé tombe en 3 à 4 tours ennemis — à valider en playtest | 28/09 |
| Passe de diversité des cartes | **Soins et soutiens d'allié → « toi ou un allié »** (Souffle apaisant, Élan de joie, Bénédiction radieuse, Lumière bienveillante, Bouclier bienveillant, Renfort du cœur, Éclat de joie, Vague de bien-être, Vague de guérison : injouables en solo auparavant). Refontes : **Rage dévastatrice** croix de 2 au contact, 58 ; **Effroi partagé** 4 PA, autour de toi (cercle 2), 10 dégâts, −2 PM ; **Terreur paralysante** portée 1-3 ; **Balayage furieux** cône de 2 au contact, 36 ; **Rayonnement de joie** portée 1-3, 35 ; **Élan de joie** portée 1-3, soin 29. Chiffres selon la formule de l'Excel — **à reporter dans l'Excel** | 28/09 |
| Multijoueur en réseau local | Ajouté tout de suite pour faciliter les tests, **approche A** (l'hôte fait tourner le combat et envoie l'état aux autres PC). Menu Multijoueur : **Même PC** (coop sur un seul PC), **Héberger**, **Rejoindre** (adresse IP de l'hôte, port 7777). Un joueur par PC, choix du champion et du deck sur son PC, l'hôte lance le combat pour tous. **Étape 1 (faite)** : connexion, salon synchronisé, lancement commun ; **étape 2** : synchronisation du combat ; **étape 3** : désynchronisation et déconnexions | 28/09 |
| Fin de combat | **Victoire** quand tous les ennemis sont vaincus, **défaite** quand tous les champions le sont (invocations exclues) ; égalité (mort simultanée) = victoire. Écran de fin MVP : Rejouer ou Menu principal (récompenses et retour de la couleur plus tard) | 28/09 |
| Ciblage des monstres | Par défaut (pourra varier selon le boss) : les cibles **à portée de l'attaque** comptent comme aussi proches, et parmi elles le monstre vise celle qui a **le moins de PV** (ex. portée 2 : Lyse à 2 cases passe avant Evan à 1 case si elle a moins de PV) ; si personne n'est à portée, la **plus proche**, puis le moins de PV (`EnemyAI.ChooseTarget`) | 28/09 |
| Retours visuels | Tous les bonus/malus s'affichent en texte flottant comme les dégâts et les soins (« +23 bouclier », « -1 PA »…), aux couleurs des pastilles ; pas de coups critiques ; l'écho de Lyse s'affiche en rouge sur la cible, Lyse se tourne vers elle sans bouger ; une unité poussée ne se retourne pas | 28/09 |
| Pool de base restreint | **Pas de nouvelles cartes** dans le jeu de base : les cartes peu utiles sont refondues pour porter la profondeur (pioche, PA, défausse, annulation d'intention) — Sang pour sang, Rage aveugle, Rumination, Sidération, Souvenir heureux, Élan partagé (détail : `Card_System.md`). Les autres idées de cartes iront dans une **extension**. Decks d'exemple mis à jour. Chiffres **à reporter dans l'Excel** | 29/09 |
| Triche : choix du coût | Après avoir ciblé une carte de la main, le joueur choisit **−1 PA** ou **+1 PA** dans un petit menu au-dessus de la carte (coût avant → après, Annuler ; −1 grisé à 1 PA) — plus de touche Maj cachée. Une carte dont le coût a été modifié garde la **couleur de son émotion** et reçoit un **liseré jaune** autour de son coût. Une carte non jouée **revient à son coût normal au début du tour suivant**. Le **coût modifié** compte pour la Main gagnante de Raze (Suite, Paire) et pour les PA dépensés de Tapis ; **Triche elle-même compte comme une carte jouée** (1 PA : Triche puis une carte à 2 = Suite) | 29/09 |
| Passif de Crux : mise en contact | Le **Réflexe du grimpeur** se déclenche dès qu'**une carte de Crux crée un contact** : il se déplace jusqu'à une unité (Grappin, Bond percutant) ou la tire contre lui (Corde de rappel). Allié → bouclier de 15, ennemi → +15 % sur la prochaine carte de dégâts. La marche seule ne compte pas. Objectif : les 2 Signatures et plusieurs decks en profitent (option A, à revoir en playtest) | 29/09 |
| Signatures de Crux et Raze | **Piolet d'ascension → Grappin** (2 PA : se hisse à côté d'une unité alliée ou ennemie à 1-5 cases en ligne droite, cible une unité et plus une case). **Réflexe du grimpeur** : près d'un allié, **bouclier de 15** (comme celui des cartes, sans durée) au lieu de −15 % de dégâts subis ; près d'un ennemi, +15 % sur la prochaine carte de dégâts, affiché à côté de l'ATQ. **Bluff de Raze** : bouclier de 8 à chaque Bluff au lieu de −10 % de dégâts subis. **Corde de rappel** 4 → 2 PA : tire de 2 cases **un allié ou un ennemi à 1-5 cases** ; seul un ennemi subit les dégâts (15) — une carte « allié ou ennemi » ne blesse jamais un allié ; **Tapis** 5 → 3 PA, **un seul ennemi** au contact (tout miser sur une cible) : 40 dégâts, +8 par PA déjà dépensé. **Balayage furieux** cible une case ou un ennemi au contact. Pas de pioche au premier tour | 29/09 |
| Pattern du boss | L'**attaque de base d'UnderBed est une carte de son pattern** (lisible dans l'aperçu) : Marée d'ombre → Griffe du dessous → Marée d'ombre → Tapi dans le noir, en boucle ; elle reste aussi sa riposte quand il est contrôlé. **Attaque Range → Marée d'ombre** (attaque de zone) (2 PA, 16 dégâts en cercle de 1 autour d'un champion à 1-2 cases). L'aperçu de carte du boss et des mobs **se retourne** à chaque carte jouée, même si la suivante est identique. Valeurs provisoires | 29/09 |
| Identité des émotions | Pas de carte équivalente entre deux émotions : **Colère** = plus gros dégâts avec contrepartie, sans soin ni vol de vie ; **Peur** = chaque attaque contrôle ; **Joie** = dégâts les plus faibles, chaque attaque soigne ou pioche. 10 cartes ajustées (détail : `Card_System.md`), **à reporter dans l'Excel** | 29/09 |

## Questions ouvertes

**Issues de l'Excel (onglet Roadmap) :**
- **Éveil** : rythme de remplissage (base : 2 points par palier, jauge par émotion) et contenu des 6 cartes d'Éveil — **mis de côté le 24/09/2026**, à réfléchir plus tard
- **Équipement** : existe-t-il ? Impact sur quoi ?
- **Stats au-delà de PV/PA/PM/armure/résistance magique** (résistances, critique…) ; valeurs d'armure et de résistance magique des champions, des monstres et des cartes (type physique/magique de chaque carte) à caler en playtest
- **Cartes bi-émotion dédiées**
- **Oppositions d'émotions** (paires Plutchik) — repoussé volontairement

**Relevées lors de la passe de cohérence :**
- **Ilya** : le garder pour la suite ? Sa Rage devra devenir une variante de l'Éveil Colère.
- **Plateforme** : PC seul ou PC + Mobile ?

---

**Dernière mise à jour :** 24 Septembre 2026
**Version GDD :** 3.4
**Responsable :** Shinda + Claude
