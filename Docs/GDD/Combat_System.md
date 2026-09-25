# Système de Combat - Émotions Tactics (Project TDB)

**Version:** 3.1
**Date:** 23 Septembre 2026
**Statut:** **Référence pour les règles de combat** (tour, main, ressources, statuts, contrôle, difficulté). Les **chiffres** (budget des cartes, profils PA/PM, barème monstres) font foi dans l'Excel `TCG_Tactique_Systeme_de_calcul.xlsx`.
**v3.1 (23/09/2026) :** réalignement sur l'Excel — budget PA+PM de 9 par profil, échelle de dégâts = baseline de l'Excel, statuts de contrôle de la Peur, règle anti-lock, règle de main redevenue question ouverte (hypothèse de playtest : main de 3).
**v3.0 (23/09/2026) :** encodage réparé ; ancienne jauge -100/+100 retirée. **v3.2 :** ordre des tours et règles de main alignés sur le code réel (voir « État du code » dans `Technical_Specs.md`).

## Vue d'Ensemble

Le système de combat combine combat tactique sur **grille carrée** (voir `Grid_System.md`) et gestion de cartes. Les joueurs contrôlent des champions qui utilisent les cartes de leurs decks personnels pour affronter des ennemis aux patterns d'attaque fixes. Chaque champion ajoute sa **mécanique signature** (passif + cartes Signature, voir `CHAMPIONS_CONCEPTS.md`).


## Déroulement d'un Combat

### Machine à États (TurnStateMachine)

**Code actuel :** **un tour par unité**, dans l'ordre de la liste des unités (le champion, puis chaque ennemi ; les invocations comme Lyse sont sautées). Les états `PlayerTurn` / `EnemyTurn` indiquent à qui appartient l'unité active. Pas de barre d'initiative. ✅ Acté le 24/09/2026 : chaque unité joue à son tour (pas de phases « tous les champions puis tous les monstres »).

| État                  | Description              | Transitions               |
|-----------------------|--------------------------|---------------------------|
| **Initializing**      | Initialisation du combat | → PlayerTurn              |
| **PlayerTurn**        | Phase des champions      | → EnemyTurn, BattleEnd    |
| **EnemyTurn**         | Phase des ennemis        | → TransitioningTurn       |
| **TransitioningTurn** | Transition entre tours   | → PlayerTurn, BattleEnd   |
| **BattleEnd**         | Fin du combat            | Aucune                    |


### Phase 1 : Initialisation (Initializing)

| Action                  | Description                               |
|-------------------------|-------------------------------------------|
| **Setup de la grille**  | Génération de la grille de combat         |
| **Placement unités**    | Champions et ennemis placés sur la grille |
| **Init des decks**      | Mélange des decks des champions           |
| **Pioche initiale**     | Main de départ (code : 5 cartes — règle à trancher) |
| **Ressources initiales**| Attribution des PA, PM de départ          |

**Différence Champions vs Ennemis :**
- Champions : deck mélangé, pioche aléatoire
- Ennemis : deck pattern, pioche séquentielle


### Phase 2 : Phase des Champions (PlayerTurn)

**Début de Tour :**

| Action              | Description                                        |
|---------------------|----------------------------------------------------|
| **Pioche**          | 1 carte (si la main n'est pas pleine)             |
| **Restauration PA** | PA restaurés selon le profil du champion           |
| **Restauration PM** | PM restaurés selon le profil du champion (moins les retraits de PM subis) |
| **Effets de début** | Résolution des effets en cours                     |

**Règles de main — ✅ actée le 24/09/2026 : celle du code**
- **Code actuel** (`DeckManager`) : main de départ **5**, maximum **5**, **1 carte piochée** au début de chaque tour, pioche sautée si la main est pleine
- Écartée : main de 3, repioche jusqu'à 3 à chaque tour (hypothèse du playtest papier de l'Excel)
- Ancienne règle (conçue pour Ilya) : main de départ 5, 7 max, pioche 1 carte/tour, pioche bloquée si main pleine — archivée

**Actions Disponibles (Ordre Libre) :**

| Action               | Coût          | Description                                      |
|----------------------|---------------|--------------------------------------------------|
| **Jouer une Carte**  | PA variable   | Sélection, ciblage, exécution des effets         |
| **Se Déplacer**      | 1 PM par case | Déplacement fractionnable (bloqué par obstacles et ennemis) |
| **Terminer le Tour** | Gratuit       | Passe à la phase ennemie                         |

**Fin de Tour :**
- Effets de fin de tour
- Transition vers EnemyTurn


### Phase 3 : Phase des Ennemis (EnemyTurn)

**Fonctionnement des Ennemis :**

| Caractéristique  | Description                                   |
|------------------|-----------------------------------------------|
| **Deck Pattern** | Ordre fixe de cartes, pas de mélange          |
| **Pioche**       | Séquentielle, reprend au début en fin de deck |
| **IA**           | Joue la prochaine carte de son pattern (1 carte par tour) |
| **Anti-lock**    | Si l'action du monstre est bloquée par un contrôle, il fait son **Attaque de base** (insensible au contrôle) à la place |
| **PA**           | 2-4 selon l'ennemi                            |
| **Intention**    | La prochaine carte est affichée à l'avance au joueur (preview des cartes ennemies) |

**Déroulement :**
1. Pioche de la prochaine carte du pattern
2. Validation de l'action
3. Choix de la cible (si nécessaire)
4. Exécution de la carte
5. Passage à l'ennemi suivant


### Phase 4 : Fin du Combat (BattleEnd)

**Conditions de Victoire :**

| Condition                      | Résultat |
|--------------------------------|----------|
| **Tous les ennemis vaincus**   | Victoire |
| **Tous les champions vaincus** | Défaite  |

**Récompenses (à implémenter)** : XP, cartes, Or — voir `Progression.md`. Dans un donjon, la victoire contribue au retour de la couleur (voir `UI_Design.md`).


## Ressources de Combat

### Ressources Principales

| Ressource | Champions | Ennemis | Régénération | Description |
|-----------|-----------|---------|--------------|-------------|
| **PA + PM** | **Budget de 9 points**, réparti par profil (min 3 PA, min 2 PM) | 2-4 PA, 2-4 PM | Complète par tour | PA pour jouer des cartes, PM pour se déplacer (1 PM = 1 case) |
| **PV** | 100 au niveau 1, +15 par niveau | Selon le barème (`Enemies.md`) | Via cartes/effets | Tombe à 0 = vaincu |
| **Éveil** | Une jauge par émotion | — | Générée en jouant des cartes | Débloque les cartes d'Éveil (voir `SYSTEME_EMOTIONS.md`) |
| **Armure / Résistance magique** | 0 par défaut (fiche champion) | Selon la fiche monstre | Buffs/malus de cartes (durée `effectDuration`) | Soustraction fixe : l'armure réduit les dégâts **physiques**, la résistance magique les **magiques** (type choisi par carte) ; minimum 1 dégât ; une valeur négative augmente les dégâts. Ordre : armure/résistance magique → réductions en % → bouclier → PV |
| **Autres stats** (résistances, critique) | **Non définies** — à trancher | | | |

**Profils PA/PM (exemples de l'Excel) :** brutal 6 PA / 3 PM · équilibré 5 PA / 4 PM · mobile 4 PA / 5 PM. Le profil est fixé par le personnage et **ne change pas avec le niveau**.


### Points d'Action (PA)

**Échelle de puissance** — baseline de l'Excel (mêlée, cible unique, sans statut) :

| Coût | 1 PA | 2 PA | 3 PA | 4 PA | 5 PA | 6 PA |
|------|------|------|------|------|------|------|
| Dégâts / soin | 12 | 26 | 42 | 60 | 80 | 102 |

La valeur réelle d'une carte = baseline × (1 + modificateurs de portée, zone, statut, etc.) — voir `Card_System.md`. Il n'y a pas de stat ATK : la puissance est portée par la carte.

**Gestion :**
- Restauration complète chaque tour
- Ne se cumule PAS entre tours
- Cartes impossibles à jouer si PA insuffisants
- Feedback UI : cartes grisées si non jouables


### Points de Mouvement (PM)

**Règles de Déplacement :**

| Règle             | Description                              |
|-------------------|------------------------------------------|
| **Coût**          | 1 PM = 1 case (terrain standard)         |
| **Fractionnable** | On peut bouger, jouer une carte, puis rebouger |
| **Blocage**       | Obstacles et ennemis bloquent le passage |
| **Restauration**  | Complète au début du tour                |
| **Non-cumulatif** | Ne se garde pas entre tours              |

**MVP :** grille plate sans obstacles (terrains spéciaux : V2, voir `Grid_System.md`). Les cartes ont une option « ligne de vue requise / non requise ».

**Mouvement via Cartes :**
- Certaines cartes donnent des PM bonus (contrepartie « Élan tactique » : +2 PM)
- Téléportation possible (ignore obstacles)
- Déplacement forcé (poussée/tirage), bond offensif, repli automatique
- Grappin (Piolet d'ascension de Crux)


### Santé (HP)

**Barre de Vie :**
- Champions : panneau de portrait à gauche de l'écran (voir `UI_Design.md`)
- Ennemis normaux : au-dessus de la tête
- Boss : en haut de l'écran (BossHealthBar)


### Interface de Ciblage Visuel

**Étapes de Ciblage :**

| Étape                  | Feedback Visuel             | Action Joueur  |
|------------------------|-----------------------------|----------------|
| **1. Sélection Carte** | Carte s'agrandit, glow doré pulsant | Clic sur carte |
| **2. Hover Cible**     | Courbe de Bézier + réticule | Déplace souris |
| **3. Validation**      | Animation de lancement      | Clic gauche    |
| **4. Annulation**      | Carte retourne en main      | Clic droit     |

**Composants Visuels :**
- TargetingCurve : courbe de Bézier quadratique de la carte vers la souris
- TargetingReticle : réticule circulaire avec croix
- HandUIController : gestion de la sélection et validation


## Résolution des Effets

### Ordre de Résolution d'une Carte

| Étape               | Action                      | Vérifications                  |
|---------------------|-----------------------------|--------------------------------|
| **1. Validation**   | Vérifier cible valide       | Portée, PA suffisants          |
| **2. Coût**         | Dépenser les PA (et paliers d'Éveil si la carte en consomme) | Déduction immédiate |
| **3. Calcul**       | Calculer dégâts/effets      | Appliquer modificateurs        |
| **4. Application**  | Appliquer effets            | Dégâts, soins, mouvements      |
| **5. Éveil / passif** | Ajouter l'Éveil généré ; déclencher les passifs (écho d'Evan, Réflexe du grimpeur, Main gagnante de Raze) | Selon la carte et le champion |
| **6. Vérification** | Vérifier morts              | Retirer unités vaincues        |


## Effets de Statut (À Implémenter)

### Statuts du MVP (cartes de l'Excel)

| Effet | Source | Règle |
|-------|--------|-------|
| **Retrait de PM** | Peur | -1 / -2 / -3 PM ou perte totale au **prochain tour** de la cible. Ne se cumulent pas : un retrait plus fort remplace un plus faible ; un plus faible n'écrase jamais un plus fort en cours |
| **Poussée / Tirage** | Peur, Crux | Déplacement forcé de N cases (se cumule avec le retrait de PM) ; une carte à zone pousse toutes les unités touchées (les plus éloignées d'abord) |
| **Bouclier (PV)** | Colère (Armure de rage), Peur, Joie | Absorbe les dégâts avant les PV (jauge bleue sur la barre de vie) ; cumulable ; dure jusqu'au début du prochain tour du lanceur ; ignoré par la Paire de Raze |
| **Réduction de dégâts (%)** | Passifs de Crux et de Raze (Bluff) | Réduit les prochains dégâts subis d'un pourcentage |
| **Vulnérabilité** | Contrepartie (Joie) | Le lanceur perd de l'**armure** (−5 par défaut, à équilibrer) jusqu'à son prochain tour : il subit plus de dégâts physiques |
| **Bouclier réactif** | Peur (Réflexe de survie) | Le bouclier ne se déclenche qu'au premier coup ennemi reçu avant le prochain tour du lanceur, et absorbe ce coup |
| **Recul et élan** | Peur (Fuite panique, Piège et recul) | Le lanceur recule de N cases à l'opposé de sa cible ; gain de PM (ou de PA) pour le tour en cours |
| **Buffs / Debuffs** | Toutes émotions | Points de buff répartis entre intensité et durée (~10 pts ≈ +10 % pendant 1 tour). Durée comptée en tours du lanceur (voir « Décisions actées » de `GDD_Main.md`) |
| **Réduction de PA** | Peur (Aura de terreur) | Réduit les PA de la cible au prochain tour : −1 PA pour Aura de terreur (calcul Excel : 26 × (1 − 0,15 portée − 0,10 Éveil) ≈ 20 pts ≈ −20 % des PA pendant 1 tour ≈ 1 PA) |

**Règle anti-lock** : un monstre dont l'action est bloquée par un contrôle fait quand même son **Attaque de base**. Codée (25/09/2026) : si le monstre a perdu des PA ou des PM ce tour et ne peut pas jouer sa carte prévue, il joue la carte `EnemyData.basicAttack` (0 PA, sans avancer son pattern), s'il a une cible à portée.

**Ténacité** : un monstre qui perd **tous** ses PM ignore les retraits de PM à son tour suivant (pastille « tenace » sous la barre du boss). On peut l'immobiliser, mais pas indéfiniment, même à plusieurs joueurs.

### Statuts prévus hors MVP

| Effet         | Durée   | Effet Mécanique                | Stackable     |
|---------------|---------|--------------------------------|---------------|
| **Poison**    | 3 tours | Perte PV au début du tour      | Oui (0-5 max) |
| **Brûlure**   | 2 tours | Perte PV + réduit soins de 50 % | Oui (0-3 max) |
| **Stun**      | 1 tour  | Saute son tour (→ Attaque de base, anti-lock) | Non |
| **Taunt**     | 1-2 tours | Doit cibler le lanceur       | Non           |
| **Régénération** | 3 tours | Soins au début du tour      | Oui (0-5 max) |

**Note :** le système de statuts n'est pas encore implémenté dans le code actuel.


## Stratégies et Synergies

### Positionnement Tactique

| Facteur          | Impact                                    |
|------------------|-------------------------------------------|
| **Portée**       | Cartes mêlée (1) nécessitent proximité    |
| **AOE**          | Regroupement amplifie dégâts ennemis      |
| **Mobilité**     | PM limités = planifier mouvement          |
| **Ligne de Vue** | Obstacles peuvent bloquer (V2)            |

**Formations Recommandées :**
- Ligne : couverture maximale du terrain
- Dispersée : évite les AOE ennemies
- Tactique : protéger les champions fragiles ; Crux peut extraire un allié en danger (Corde de rappel)


## Difficulté et Équilibrage (À Implémenter)

### Système de Difficulté *(référence unique — `Enemies.md` et `Progression.md` renvoient ici)*

| Difficulté    | HP Ennemis | Dégâts Ennemis | Défense Ennemis | Modificateurs Joueur |
|---------------|------------|----------------|-----------------|----------------------|
| **Facile**    | -30 %      | -20 %          | -20 %           | +1 PA                |
| **Normal**    | 100 %      | 100 %          | 100 %           | Aucun                |
| **Difficile** | +50 %      | +30 %          | +20 %           | -1 PA, meilleur loot (×1.5 Or et XP) |


> Note : les modificateurs joueur ±1 PA de la difficulté sortent du budget PA+PM de 9 — à confirmer. La colonne « Défense » suppose une stat d'armure qui n'est pas encore définie.

### Équilibrage des Cartes

**Principes de Design :**
- Coût PA proportionnel aux effets (voir échelle ci-dessus)
- Tout est piloté par le budget de l'Excel (baseline × modificateurs)
- Triangle validé en playtest : Colère = burst sans sustain, Peur = contrôle/tempo, Joie = survie mais lent
- Le niveau ne modifie jamais la puissance des cartes


## Systèmes À Développer

### Priorité Haute
- Profils PA/PM (budget 9) et types de cartes Standard / Éveil / Signature
- Statuts de contrôle (retrait de PM, poussée/tirage) + règle anti-lock
- Invocations (Evan), grappin (Crux), détection de motifs de coûts (Raze)
- Réactions (effets « si ciblé ce tour », ex : Réflexe de survie)
- IA ennemie : patterns + cycle de boss Zone / Basique / Heal

### Priorité Moyenne
- Système de difficulté
- Terrain avec effets (lave, glace)
- Ligne de vue
- Hauteur et couverture

### Priorité Basse
- Combos automatiques
- Achievements de combat


**Dernière mise à jour :** 23 Septembre 2026
**Version :** 3.0
**Responsable :** Shinda + Claude
