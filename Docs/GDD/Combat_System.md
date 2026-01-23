# Système de Combat - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète l'implémentation actuelle

## Vue d'Ensemble

Le système de combat de **Project TDB** combine combat tactique sur grille 2D et gestion de cartes avec un système d'émotions unique. Les joueurs contrôlent des champions qui utilisent des cartes de leurs decks personnels pour affronter des ennemis avec des patterns d'attaque fixes. Le système d'émotions ajoute une dimension stratégique en transformant les champions selon leurs actions.


## Déroulement d'un Combat

### Machine à Etats (TurnStateMachine)

| Etat                  | Description              | Transitions               |
|-----------------------|--------------------------|---------------------------|
| **Initializing**      | Initialisation du combat | â†’ PlayerTurn            |
| **PlayerTurn**        | Tour des champions       | â†’ EnemyTurn, BattleEnd  |
| **EnemyTurn**         | Tour des ennemis         | â†’ TransitioningTurn     |
| **TransitioningTurn** | Transition entre tours   | â†’ PlayerTurn, BattleEnd |
| **BattleEnd**         | Fin du combat            | Aucune                    |


### Phase 1: Initialisation (Initializing)

| Action                  | Description                               |
|-------------------------|-------------------------------------------|
| **Setup de la grille**  | Génération de la grille de combat         |
| **Placement unités**    | Champions et ennemis placés sur la grille |
| **Init des decks**      | Mélange des decks des champions           |
| **Pioche initiale**     | Champions piochent jusqu'à 5 cartes       |
| **Resources initiales** | Attribution des PA, PM de départ          |

**Différence Champions vs Ennemis:**
- Champions : Deck mélangé, pioche aléatoire
- Ennemis : Deck pattern, pioche séquentielle


### Phase 2: Tour des Champions (PlayerTurn)

**Début de Tour:**

| Action              | Description                                        |
|---------------------|----------------------------------------------------|
| **Pioche**          | Pioche jusqu'à 5 cartes en main                    |
| **Restauration PA** | PA restaurés au maximum (3-5)                      |
| **Restauration PM** | PM restaurés au maximum (2-4)                      |
| **Effets de début** | Résolution des effets (régénération, poison, etc.) |

**Actions Disponibles (Ordre Libre):**

| Action               | Coût          | Description                                      |
|----------------------|---------------|--------------------------------------------------|
| **Jouer une Carte**  | PA variable   | Sélection, ciblage, exécution des effets         |
| **Se Déplacer**      | 1 PM par case | Déplacement sur la grille (bloqué par obstacles) |
| **Terminer le Tour** | Gratuit       | Passe au tour suivant                            |

**Fin de Tour:**
- Défausse des cartes en excès si >10 en main
- Transition vers EnemyTurn


### Phase 3: Tour des Ennemis (EnemyTurn)

**Fonctionnement des Ennemis:**

| Caractéristique  | Description                                   |
|------------------|-----------------------------------------------|
| **Deck Pattern** | Ordre fixe de cartes, pas de mélange          |
| **Pioche**       | Séquentielle, reprend au début si fin du deck |
| **IA**           | Joue la prochaine carte dans le pattern       |
| **PA**           | 2-4 selon l'ennemi                            |

**Déroulement:**
1. Pioche de la prochaine carte du pattern
2. Validation de l'action
3. Choix de la cible (si nécessaire)
4. Exécution de la carte
5. Passage à l'ennemi suivant


### Phase 4: Fin du Combat (BattleEnd)

**Conditions de Victoire:**

| Condition                      | Résultat |
|--------------------------------|----------|
| **Tous les ennemis vaincus**   | Victoire |
| **Tous les champions vaincus** | Défaite  |

**Récompenses (A Implémenter):**
- Expérience pour les champions
- Nouvelles cartes
- Ressources
- Progression de la campagne


## Ressources de Combat

### Ressources Principales

| Ressource                    | Champions | Ennemis  | Régénération      | Description                      |
|------------------------------|-----------|----------|-------------------|----------------------------------|
| **PA** (Points d'Action)     | 3-5       | 2-4      | Complète par tour | Pour jouer des cartes            |
| **PM** (Points de Mouvement) | 2-4       | 2-4      | Complète par tour | Pour se déplacer (1 PM = 1 case) |
| **HP** (Points de Vie)       | Variable  | Variable | Via cartes/effets | Tombe à 0 = vaincu               |
| **Défense**                  | 10+       | 10+      | Fixe              | Réduit dégâts physiques          |


### Points d'Action (PA)

**Coût des Cartes par PA:**

| Coût      | Type de Carte | Puissance Typique                 |
|-----------|---------------|-----------------------------------|
| **0 PA**  | Gratuites     | Faibles effets ou conditionnelles |
| **1 PA**  | Basiques      | 6 dégâts, petits effets           |
| **2 PA**  | Standards     | 12 dégâts, buffs moyens           |
| **3 PA**  | Puissantes    | 18 dégâts, AoE, gros effets       |
| **4+ PA** | Ultimes       | 24+ dégâts, effets dévastateurs   |

**Gestion:**
- Restauration complète chaque tour
- Ne se cumule PAS entre tours
- Cartes impossibles à jouer si PA insuffisants
- Feedback UI : cartes grisées si non jouables


### Points de Mouvement (PM)

**Règles de Déplacement:**

| Règle             | Description                              |
|-------------------|------------------------------------------|
| **Coût**          | 1 PM = 1 case                            |
| **Blocage**       | Obstacles et ennemis bloquent le passage |
| **Restauration**  | Complète au début du tour                |
| **Non-cumulatif** | Ne se garde pas entre tours              |

**Mouvement via Cartes:**
- Certaines cartes donnent des PM bonus
- Téléportation possible (ignore obstacles)
- Déplacement forcé (push/pull)


### Santé (HP)

**Barre de Vie:**
- Champions : Orbe de vie à gauche de l'écran
- Ennemis normaux : Au-dessus de la tête
- Boss : En haut de l'écran (BossHealthBar)


### Interface de Ciblage Visuel

**Etapes de Ciblage:**

| Etape                  | Feedback Visuel             | Action Joueur  |
|------------------------|-----------------------------|----------------|
| **1. Sélection Carte** | Carte s'agrandit, glow vert | Clic sur carte |
| **2. Hover Cible**     | Courbe de Bézier + réticule | Déplace souris |
| **3. Validation**      | Animation de lancement      | Clic gauche    |
| **4. Annulation**      | Carte retourne en main      | Clic droit     |

**Composants Visuels:**
- TargetingCurve : Courbe de Bézier quadratique de la carte vers la souris
- TargetingReticle : Réticule circulaire avec croix
- HandUIController : Gestion de la sélection et validation


## Résolution des Effets

### Ordre de Résolution d'une Carte

| Etape               | Action                      | Vérifications                  |
|---------------------|-----------------------------|--------------------------------|
| **1. Validation**   | Vérifier cible valide       | Portée, PA suffisants          |
| **2. Coût**         | Dépenser les PA             | Déduction immédiate            |
| **3. Calcul**       | Calculer dégâts/effets      | Appliquer modificateurs        |
| **4. Application**  | Appliquer effets            | Dégâts, soins, mouvements      |
| **5. Emotion**      | Modifier jauge émotionnelle | Selon modificateur de la carte |
| **6. Vérification** | Vérifier morts              | Retirer unités vaincues        |


## Effets de Statut (A Implémenter)

### Debuffs Prévus

| Effet         | Durée   | Effet Mécanique                | Stackable |
|---------------|---------|--------------------------------|-----------|
| **Poison**    | 3 tours | Perte HP au début du tour      | Oui (0—5 max) |
| **Brûlure**   | 2 tours | Perte HP + réduit soins de 50% | Oui (0—3 max) |
| **Stun**      | 1 tour  | Saute son tour                 | Non           |
| **Gel**       | 2 tours | PM réduits à 0                 | Non           |
| **Faiblesse** | 2 tours | Dégâts -30%                    | Non           |

### Buffs Prévus

| Effet            | Durée               | Effet Mécanique         | Stackable     |
|------------------|---------------------|-------------------------|---------------|
| **Bouclier**     | Jusqu'à destruction | Absorbe dégâts avant HP | Non           |
| **Force**        | 2 tours             | Dégâts physiques +30%   | Non           |
| **Hâte**         | 3 tours             | +1 PM par tour          | Non           |
| **Régénération** | 3 tours             | Soins au début du tour  | Oui (0—5 max) |

**Note:** Le système de statuts est prévu mais pas encore implémenté dans le code actuel.


## Stratégies et Synergies

### Positionnement Tactique

**Importance de la Position:**

| Facteur          | Impact                                    |
|------------------|-------------------------------------------|
| **Portée**       | Cartes mêlée (1) nécessitent proximité    |
| **AOE**          | Regroupement amplifie dégâts ennemis      |
| **Mobilité**     | PM limités = planifier mouvement          |
| **Ligne de Vue** | Obstacles peuvent bloquer (si implémenté) |

**Formations Recommandées:**
- Ligne : Couverture maximale du terrain
- Dispersée : Evite les AOE ennemies
- Tactique : Protéger les champions fragiles derrière les tanks


## Difficulté et Equilibrage (A Implémenter)

### Système de Difficulté Prévu

| Difficulté    | HP Ennemis | Dégâts Ennemis | Modificateurs Joueur |
|---------------|------------|----------------|----------------------|
| **Facile**    | -30%       | -20%           | +1 PA                |
| **Normal**    | 100%       | 100%           | Aucun                |
| **Difficile** | +50%       | +30%           | -1 PA, meilleur loot |


### Equilibrage des Cartes

**Principes de Design:**
- Coût PA proportionnel aux effets
- AOE coûte plus cher que cible unique
- Emotions extrêmes = cartes plus puissantes
- Cartes neutres (0 émotion) = polyvalentes


## Systèmes A Développer

### Priorité Haute
- Effets de statut (poison, stun, buffs)
- IA ennemie avancée (patterns complexes)
- Animations de combat
- Feedbacks visuels améliorés

### Priorité Moyenne
- Système de difficulté
- Terrain avec effets (lave, glace)
- Ligne de vue
- Hauteur et couverture

### Priorité Basse
- Réactions (contre-attaque, riposte)
- Combos automatiques
- Achievements de combat


**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Combat Project TDB
