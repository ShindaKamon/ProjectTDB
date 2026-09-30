# Système de Cartes - Émotions Tactics (Project TDB)

**Version:** 4.1
**Date:** 30 Septembre 2026
**Changements :**
- v3.0 (10/09/2026) : dimensions Classe et Élément retirées.
- v4.0 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`) — types de cartes Standard / Éveil / Signature, identité émotionnelle (Colère / Peur / Joie / Neutre), deck de 24 cartes, budget de puissance. L'ancien découpage Personnage / Famille / Neutre est archivé.
- v4.1 (30/09/2026) : relecture d'audit — cohérent avec le code (Éveil = fusion, deck de 20, pool de 51 Standard).

> **Les chiffres font foi dans l'Excel** (onglets « Références », « Calculateur », « Bibliothèque de cartes », « Suivi de deck »). Ce document explique les règles.

---

## Vue d'Ensemble

Chaque carte a :
- une **identité émotionnelle** : Colère, Peur, Joie, ou Neutre
- un **type** : Standard, Éveil ou Signature
- un **coût en PA** (1 à 6)
- des caractéristiques tactiques (portée, zone, ligne de vue, statut, déplacement forcé, contrepartie)
- une **valeur finale** (dégâts, soin ou points de buff) calculée par le **budget de puissance**

### Identité émotionnelle

| Identité | Style |
|----------|-------|
| **Colère** | Agressif — burst sans sustain |
| **Peur** | Contrôle — retrait de PM, poussée/tirage |
| **Joie** | Soin / valeur — survie, mais lent |
| **Neutre** | Cartes Signature — jouables quel que soit le deck |

Détail des émotions : `SYSTEME_EMOTIONS.md`.

### Types de cartes

| Type | Règle |
|------|-------|
| **Standard** | Jouable avec des PA seulement. Pool de **51 cartes** dans le code (17 par émotion : les 49 de la bibliothèque de l'Excel + Étincelle et Rire lumineux, dont 6 refondues le 29/09) ; la Roadmap de l'Excel parle de 36. |
| **Éveil** | **Supprimé comme type de carte le 30/09/2026** : l'Éveil devient une fusion du champion avec l'émotion (jauge pleine), voir `SYSTEME_EMOTIONS.md`. `CardCategory.Awakening` n'est plus utilisée (ne pas renuméroter l'enum). |
| **Signature** | Fixe, liée au champion (2 par champion), identité Neutre. |

### Rôles (répartition cible dans le deck)

| Rôle | Part du deck |
|------|--------------|
| Dégâts / Mouvement | 60 % |
| Soutien / Buffs | 20 % |
| Réaction / Soin | 20 % |

---

## Deckbuilding

- **20 cartes** : **4 Signature (2 exemplaires de chacune des 2 Signatures) + 16 Standard**. L'Excel prévoyait 24 cartes (2 Signature + 6 Éveil + 16 Standard) ; les cartes d'Éveil ont été retirées du deck le 30/09/2026 (l'Éveil est devenu une fusion, voir `SYSTEME_EMOTIONS.md`), le format cible reste à confirmer
- Deck : **1 ou 2 couleurs** choisies à sa création (cartes de ces couleurs uniquement ; un champion peut choisir n'importe lesquelles) ; **4 exemplaires max** par carte ; les **2 Signatures du champion obligatoires** (2 exemplaires chacune)
- Plusieurs decks par champion, plusieurs champions par compte
- Le **niveau** du champion débloque des **slots de cartes**, mais n'augmente jamais la puissance des cartes (voir `Progression.md`)
- Suivi de la composition : onglet « Suivi de deck » de l'Excel

### Viabilité des couleurs et decks d'exemple (29/09/2026)

Objectif : **chaque couleur est jouable seule** (chacune garde son point fort : Colère = dégâts, Peur = contrôle, Joie = soin) et **chaque champion a plusieurs decks viables aux façons de jouer différentes**. Pour que la Joie seule puisse finir un combat, deux cartes offensives bon marché lui ont été ajoutées : **Étincelle** (1 PA, 10 dégâts, portée 1-3) et **Rire lumineux** (2 PA, 13 dégâts au contact, vol de vie 13 — « soin + miroir dégâts » de l'Excel).

Decks d'exemple (4 Signatures + 16 Standard). Le **deck de base** de chaque champion (ses cartes de départ, `ChampionData.startingDeck`) est son archétype principal : Écho de Colère (Evan), Grimpeur furieux (Crux), Suite (Raze) ; les deux autres sont enregistrés comme decks perso :

| Champion | Deck | Couleurs | Façon de jouer | Cœur du deck |
|---|---|---|---|---|
| Evan | Écho de Colère | Colère | Multiplier les échos de Lyse (un par cible) | Frappe rapide ×4, Jet de rage ×3, Explosion de rage ×2, Éclat de rage ×2 |
| Evan | Cauchemar partagé | Peur | Contrôle à plusieurs cibles, Lyse double les coups | Frappe hésitante ×4, Cauchemar collectif ×2, Aura de terreur ×2, Rumination ×2, Sidération ×1 |
| Evan | Frère et sœur | Joie + Peur | Survie : soigner Evan et Lyse, grignoter à distance | Souffle apaisant ×3, Souvenir heureux ×2, Élan partagé ×1, Étincelle ×3, Ombre rampante ×3 |
| Crux | Grimpeur furieux | Colère | Plonger au contact et frapper en zone | Poing ardent ×3, Balayage furieux ×3, Charge brutale ×3, Rage aveugle ×2, Bond percutant ×2 |
| Crux | Harceleur | Peur | Frapper, ralentir, reculer | Piège et recul ×3, Piqûre d'angoisse ×4, Effroi partagé ×2, Réflexe de survie ×3 |
| Crux | Premier de cordée | Joie | Soutien mobile : rejoindre l'équipe et la soigner en zone | Vague de bien-être ×3, Éclat de joie ×3, Rire lumineux ×3, Élan partagé ×2 |
| Raze | Suite | Colère | Coûts en escalier (1, 2, 3) pour enchaîner les +1 PA | Coup de colère ×4, Frappe rapide ×3, Charge brutale ×2, Sang pour sang ×2, Rage totale ×1 |
| Raze | Bluff | Colère + Peur | Alterner les émotions pour gagner un bouclier de 8 à chaque changement | Coup de colère ×3, Piqûre d'angoisse ×3, Frappe hésitante ×3, Silence glaçant ×2, Sang pour sang ×1 |
| Raze | Paire | Joie + Colère | Cartes de même coût (3 PA) : la 2ᵉ ignore les réductions en % | Flamme de l'espoir ×3, Élan de joie ×2, Charge brutale ×3, Sang pour sang ×2, Jet de rage ×2 |


### Identité des émotions dans les cartes (29/09/2026)

Deux émotions ne partagent pas la même carte : **Colère** = les plus gros dégâts, payés par une contrepartie (contrecoup, PV), sans soin ni vol de vie ; **Peur** = dégâts moindres, mais chaque attaque contrôle (perte de PM, poussée) ; **Joie** = dégâts les plus faibles, mais chaque attaque soigne (vol de vie) ou apporte de la valeur (pioche).

| Carte | Avant | Après |
|---|---|---|
| Ombre rampante (Peur) | 9 dégâts | 9 dégâts + perd 1 PM |
| Étincelle (Joie) | 10 dégâts | 8 dégâts + vol de vie 8 |
| Sang bouillonnant (Colère) | 18 + vol de vie 18 | 32 dégâts, contrecoup 7 |
| Armure de rage (Colère) | Bouclier 23 | Armure +5 (1 tour) + 12 dégâts sur la prochaine carte offensive |
| Flamme de l'espoir (Joie) | 38 dégâts | 32 dégâts + pioche 1 |
| Onde radieuse (Joie) | 33 en ligne | 24 + vol de vie 6 par ennemi touché |
| Rayonnement de joie (Joie) | 35 en cercle de 2 | 26 + vol de vie 5 par ennemi touché |
| Éclat de rage / Explosion de rage / Jet de rage (Colère) | 27 / 32 / 23 | 33 / 40 / 27 (remontées au budget de l'Excel) |

### Pool de base restreint et refontes (29/09/2026)

Pas de nouvelles cartes : les cartes peu utiles ou en doublon sont **refondues** pour porter les mécaniques de profondeur (pioche, PA, défausse, annulation d'intention). Les idées de cartes supplémentaires iront dans une **extension**.

| Ancienne carte | Nouvelle carte | Effet |
|---|---|---|
| Frénésie incontrôlée (Colère) | **Sang pour sang** | 0 PA, coûte 10 PV : +2 PA ce tour (au-delà du maximum) |
| Déferlante (Colère) | **Rage aveugle** | 2 PA : défausse ta main, +8 dégâts sur ta prochaine carte offensive par carte défaussée |
| Voile d'ombre (Peur) | **Rumination** | 1 PA : pioche 2, tu perds 1 PM pendant 1 tour |
| Regard glaçant (Peur) | **Sidération** | 4 PA, 1-6 cases : le monstre ne joue pas sa prochaine carte (ni attaque de base) et passe à la suivante ; son aperçu l'affiche barrée |
| Lumière bienveillante (Joie) | **Souvenir heureux** | 2 PA, toi ou un allié à 1-5 cases : soigne 12 et pioche 1 |
| Renfort du cœur (Joie) | **Élan partagé** | 2 PA, toi ou un allié à 1-5 cases : +2 PA au prochain tour (au-delà du maximum, sans cumul) |

---

## Budget de Puissance

**Valeur finale = Baseline(coût en PA) × (1 + somme des modificateurs)**

### Baseline (mêlée, cible unique, sans statut)

| Coût PA | 1 | 2 | 3 | 4 | 5 | 6 |
|---------|---|---|---|---|---|---|
| **Dégâts / soin** | 12 | 26 | 42 | 60 | 80 | 102 |

### Modificateurs (résumé — valeurs exactes dans l'Excel)

| Catégorie | Options (modificateur) |
|-----------|------------------------|
| **Portée** | Mêlée (0) · 1-3 cases (-0.15) · 1-5 (-0.25) · 1-6 (-0.35) |
| **Zone** | Cible unique (0) · Ligne 2-3 (-0.2) · Cône 3 (-0.25) · 2 cibles séparées (-0.25) · Cercle rayon 1 (-0.35) · 3 cibles séparées (-0.4) · Cercle rayon 2 (-0.5) · Contagion (-0.5) · Équipe entière (-0.65) |
| **Ligne de vue** | Requise (0) · Non requise (-0.15) |
| **Statut (Peur)** | -1 PM (-0.1) · -2 PM (-0.3) · -3 PM (-0.35) · perte totale de PM (-1) |
| **Déplacement forcé** | Poussée/Tirage (-0.08 par case) · Téléportation (-0.15 par case) |
| **Éveil** | Génère (-0.1) · ~~Consomme 1 / 2 / 3 paliers~~ (retiré le 30/09 : plus de cartes d'Éveil) |
| **Type d'effet** | Dégâts (0) · Soin (-0.1) · Soin + miroir dégâts (-0.5) · Buff/Debuff (0) · Pioche (0) |
| **Contrepartie** | Auto-dégâts (+0.25) · Vulnérabilité sur soi (+0.15) · Déplacement aléatoire sur soi (+0.2) · Touche aussi les alliés (+0.3) · Perd 1 PM au prochain tour (+0.15) · Vol de vie (-0.2) · Élan +2 PM (-0.2) · Bond offensif (-0.15) · Repli automatique (-0.15) |

- Plus une carte a de portée, de zone, de contrôle ou de déplacement, moins elle fait de dégâts bruts.
- Une contrepartie négative pour le lanceur **augmente** le budget.
- Cellule rouge dans l'Excel = la carte cumule trop de modificateurs négatifs.
- **Cartes Buff/Debuff** : la valeur finale est un nombre de points à répartir entre intensité et durée (repère : ~10 points ≈ +10 % d'un effet pendant 1 tour).

> ⚠️ **Grille** (24/09/2026) : carrée en **4 directions** (distance de Manhattan). Un cercle de rayon 1 fait 5 cases, de rayon 2, 13 cases, alors que le budget de l'Excel suppose 9 et 25 cases : le coût des cartes à zone est à revoir. Voir `Grid_System.md`.

---

## Système de Ciblage

### Types de Cible (code actuel, `CardTargetType`)

| Type | Description | Exemple |
|------|--------------|---------|
| **None** | Aucune cible | Buff personnel instantané |
| **Self** | Soi-même uniquement | Se soigner, se buffer |
| **Enemy** | Un ou plusieurs ennemis | Attaque |
| **Ally** | Alliés (sauf soi) | Soigner un allié |
| **AllyOrSelf** | Alliés ET soi-même | Soins de groupe |
| **AllyorEnemy** | Alliés ET ennemis | Grappin (Crux) |
| **AnyUnit** | N'importe quelle unité | Aucune carte pour l'instant |
| **EmptyTile** | Tuiles vides uniquement | Invocation de Lyse (si Lyse est déjà là : cible Lyse pour la soigner), Écho évanescent (Evan), Bond percutant (bond) |
| **AnyTile** | N'importe quelle tuile | Aucune carte pour l'instant (Grappin, ex-Piolet d'ascension, cible désormais une unité : AllyorEnemy, charge en ligne droite jusqu'à elle) |
| **EnemyOrTile** | Un ennemi ou une tuile | Éclat de rage, Balayage furieux |

Option **ciblage en ligne droite** (`targetInStraightLine`) : la cible doit être sur la même ligne ou la même colonne que le lanceur, sans diagonale (Éclat de rage). On vise toujours une **case** : une unité ne masque jamais la case derrière elle.

### Portée

Portées utilisées par les cartes MVP : **1 (mêlée)**, **1-3**, **1-5**, **1-6** cases. Distance mesurée sur la grille en 4 directions (une case en diagonale est à 2 cases).

### Zones d'Effet

Formes utilisées par les cartes MVP : cible unique, ligne (2-3 cases), cône (3 cases), cercle rayon 1 et 2, cibles multiples séparées (2 ou 3), contagion (se propage aux cibles à 2 cases ou moins), équipe entière.

Dans le code (`CardAreaEffect`) : None, OneTile, Line, Cross, Circle, Cone (ouverture 90°), WholeTeam. La **ligne** part de la case visée et s'éloigne du lanceur : une ligne de 3 couvre la cible et les 2 cases derrière elle (le lanceur n'est pas touché) — décision du 28/09/2026. Le **cône** part aussi de la case visée et s'élargit : rangées de 1, 3, 5… cases, une rangée par point de rayon (28/09/2026).

### Cibles Affectées dans l'AOE

| Type | Qui est affecté |
|------|------------------|
| **Enemies** | Que les ennemis |
| **Ally** / **AllyOrSelf** | Alliés (avec ou sans soi) |
| **AllyorEnemy** / **AnyUnit** | Tout le monde (ex : contrepartie « touche aussi les alliés proches ») |

---

**Dernière mise à jour:** 23 Septembre 2026
**Version:** 4.1
**Responsable:** Shinda + Claude
