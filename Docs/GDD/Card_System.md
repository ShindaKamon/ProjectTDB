# Système de Cartes - Émotions Tactics (Project TDB)

**Version:** 4.0
**Date:** 23 Septembre 2026
**Changements :**
- v3.0 (10/09/2026) : dimensions Classe et Élément retirées.
- v4.0 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`) — types de cartes Standard / Éveil / Signature, identité émotionnelle (Colère / Peur / Joie / Neutre), deck de 24 cartes, budget de puissance. L'ancien découpage Personnage / Famille / Neutre est archivé.

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
| **Standard** | Jouable avec des PA seulement. Pool de 49 cartes dans la bibliothèque de l'Excel et dans le code (17 Colère, 17 Peur, 15 Joie) ; la Roadmap de l'Excel parle de 36. |
| **Éveil** | Nécessite un seuil d'Éveil (jauge de l'émotion correspondante). Cartes fortes. À créer. |
| **Signature** | Fixe, liée au champion (2 par champion), identité Neutre. |

### Rôles (répartition cible dans le deck)

| Rôle | Part du deck |
|------|--------------|
| Dégâts / Mouvement | 60 % |
| Soutien / Buffs | 20 % |
| Réaction / Soin | 20 % |

---

## Deckbuilding

- **24 cartes** : **2 Signature + 6 Éveil + 16 Standard** (code actuel : 18 cartes, 2 Signature + 16 Standard, les slots Éveil ne sont pas encore implémentés)
- Deck : **1 ou 2 couleurs** choisies à sa création (cartes de ces couleurs uniquement ; un champion peut choisir n'importe lesquelles) ; **4 exemplaires max** par carte ; les **2 Signatures du champion obligatoires** (1 exemplaire chacune)
- Plusieurs decks par champion, plusieurs champions par compte
- Le **niveau** du champion débloque des **slots de cartes**, mais n'augmente jamais la puissance des cartes (voir `Progression.md`)
- Suivi de la composition : onglet « Suivi de deck » de l'Excel

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
| **Éveil** | Génère (-0.1) · Consomme 1 / 2 / 3 paliers (+0.3 / +0.5 / +0.7) |
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
| **AllyorEnemy** | Alliés ET ennemis | Aucune carte pour l'instant (prévu pour Corde de rappel, voir `Technical_Specs.md`) |
| **AnyUnit** | N'importe quelle unité | Aucune carte pour l'instant |
| **EmptyTile** | Tuiles vides uniquement | Invocation de Lyse, Écho évanescent (Evan), Bond percutant (bond) |
| **AnyTile** | N'importe quelle tuile | Piolet d'ascension (Crux, charge en ligne droite) |
| **EnemyOrTile** | Un ennemi ou une tuile | Aucune carte pour l'instant |

### Portée

Portées utilisées par les cartes MVP : **1 (mêlée)**, **1-3**, **1-5**, **1-6** cases. Distance mesurée sur la grille en 4 directions (une case en diagonale est à 2 cases).

### Zones d'Effet

Formes utilisées par les cartes MVP : cible unique, ligne (2-3 cases), cône (3 cases), cercle rayon 1 et 2, cibles multiples séparées (2 ou 3), contagion (se propage aux cibles à 2 cases ou moins), équipe entière.

Dans le code (`CardAreaEffect`) : None, OneTile, Line, Cross, Circle, Cone (ouverture 90°), WholeTeam.

### Cibles Affectées dans l'AOE

| Type | Qui est affecté |
|------|------------------|
| **Enemies** | Que les ennemis |
| **Ally** / **AllyOrSelf** | Alliés (avec ou sans soi) |
| **AllyorEnemy** / **AnyUnit** | Tout le monde (ex : contrepartie « touche aussi les alliés proches ») |

---

**Dernière mise à jour:** 23 Septembre 2026
**Version:** 4.0
**Responsable:** Shinda + Claude
