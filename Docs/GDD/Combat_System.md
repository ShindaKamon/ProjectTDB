# ⚔️ Système de Combat - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète l'implémentation actuelle

---

## 🎯 Vue d'Ensemble

Le système de combat de **Project TDB** combine combat tactique sur grille 2D et gestion de cartes avec un système d'émotions unique. Les joueurs contrôlent des champions qui utilisent des cartes de leurs decks personnels pour affronter des ennemis avec des patterns d'attaque fixes. Le système d'émotions ajoute une dimension stratégique en transformant les champions selon leurs actions.

---

## 🔄 Déroulement d'un Combat

### Machine à États (TurnStateMachine)

| État | Description | Transitions |
|------|-------------|-------------|
| **Initializing** | Initialisation du combat | → PlayerTurn |
| **PlayerTurn** | Tour des champions | → EnemyTurn, BattleEnd |
| **EnemyTurn** | Tour des ennemis | → TransitioningTurn |
| **TransitioningTurn** | Transition entre tours | → PlayerTurn, BattleEnd |
| **BattleEnd** | Fin du combat | Aucune |

---

### Phase 1: Initialisation (Initializing)

| Action | Description |
|--------|-------------|
| **Setup de la grille** | Génération de la grille de combat |
| **Placement unités** | Champions et ennemis placés sur la grille |
| **Init des decks** | Mélange des decks des champions |
| **Pioche initiale** | Champions piochent jusqu'à 5 cartes |
| **Resources initiales** | Attribution des PA, PM de départ |

**Différence Champions vs Ennemis:**
- Champions : Deck mélangé, pioche aléatoire
- Ennemis : Deck pattern, pioche séquentielle

---

### Phase 2: Tour des Champions (PlayerTurn)

**Début de Tour:**

| Action | Description |
|--------|-------------|
| **Pioche** | Pioche jusqu'à 5 cartes en main |
| **Restauration PA** | PA restaurés au maximum (3-5) |
| **Restauration PM** | PM restaurés au maximum (2-4) |
| **Effets de début** | Résolution des effets (régénération, poison, etc.) |

**Actions Disponibles (Ordre Libre):**

| Action | Coût | Description |
|--------|------|-------------|
| **Jouer une Carte** | PA variable | Sélection, ciblage, exécution des effets |
| **Se Déplacer** | 1 PM par case | Déplacement sur la grille (bloqué par obstacles) |
| **Terminer le Tour** | Gratuit | Passe au tour suivant |

**Fin de Tour:**
- Défausse des cartes en excès si >10 en main
- Transition vers EnemyTurn

---

### Phase 3: Tour des Ennemis (EnemyTurn)

**Fonctionnement des Ennemis:**

| Caractéristique | Description |
|-----------------|-------------|
| **Deck Pattern** | Ordre fixe de cartes, pas de mélange |
| **Pioche** | Séquentielle, reprend au début si fin du deck |
| **IA** | Joue la prochaine carte dans le pattern |
| **PA** | 2-4 selon l'ennemi |

**Déroulement:**
1. Pioche de la prochaine carte du pattern
2. Validation de l'action
3. Choix de la cible (si nécessaire)
4. Exécution de la carte
5. Passage à l'ennemi suivant

---

### Phase 4: Fin du Combat (BattleEnd)

**Conditions de Victoire:**

| Condition | Résultat |
|-----------|----------|
| **Tous les ennemis vaincus** | Victoire ✓ |
| **Tous les champions vaincus** | Défaite ✗ |

**Récompenses (À Implémenter):**
- Expérience pour les champions
- Nouvelles cartes
- Ressources
- Progression de la campagne

---

## 📊 Ressources de Combat

### Ressources Principales

| Ressource | Champions | Ennemis | Régénération | Description |
|-----------|-----------|---------|--------------|-------------|
| **PA** (Points d'Action) | 3-5 | 2-4 | Complète par tour | Pour jouer des cartes |
| **PM** (Points de Mouvement) | 2-4 | 2-4 | Complète par tour | Pour se déplacer (1 PM = 1 case) |
| **HP** (Points de Vie) | Variable | Variable | Via cartes/effets | Tombe à 0 = vaincu |
| **Défense Physique** | 10+ | 10+ | Fixe | Réduit dégâts physiques |
| **Défense Magique** | 10+ | 10+ | Fixe | Réduit dégâts magiques |

---

### Points d'Action (PA)

**Coût des Cartes par PA:**

| Coût | Type de Carte | Puissance Typique |
|------|---------------|-------------------|
| **0 PA** | Gratuites | Faibles effets ou conditionnelles |
| **1 PA** | Basiques | 6 dégâts, petits effets |
| **2 PA** | Standards | 12 dégâts, buffs moyens |
| **3 PA** | Puissantes | 18 dégâts, AoE, gros effets |
| **4+ PA** | Ultimes | 24+ dégâts, effets dévastateurs |

**Gestion:**
- Restauration complète chaque tour
- Ne se cumule PAS entre tours
- Cartes impossibles à jouer si PA insuffisants
- Feedback UI : cartes grisées si non jouables

---

### Points de Mouvement (PM)

**Règles de Déplacement:**

| Règle | Description |
|-------|-------------|
| **Coût** | 1 PM = 1 case |
| **Blocage** | Obstacles et ennemis bloquent le passage |
| **Restauration** | Complète au début du tour |
| **Non-cumulatif** | Ne se garde pas entre tours |

**Mouvement via Cartes:**
- Certaines cartes donnent des PM bonus
- Téléportation possible (ignore obstacles)
- Déplacement forcé (push/pull)

---

### Santé (HP)

**Système de Dégâts:**

| Type | Formule | Minimum |
|------|---------|---------|
| **Physical** | Dégâts - Défense Physique | 1 |
| **Magical** | Dégâts - Défense Magique | 1 |

**Important:** Les dégâts infligent toujours au minimum 1 HP pour éviter les immunités totales.

**Barre de Vie:**
- Champions : Au-dessus de la tête
- Ennemis normaux : Au-dessus de la tête
- Boss : En haut de l'écran (BossHealthBar)

---

### Système d'Émotions (Champions Uniquement)

**Jauge Émotionnelle:**

| Caractéristique | Valeur |
|-----------------|--------|
| **Minimum** | -100 (DPS) |
| **Neutre** | 0 |
| **Maximum** | +100 (Tank) |

**Modificateurs par Carte:**
- Chaque carte modifie la jauge (-50 à +50)
- Les cartes offensives poussent vers négatif (DPS)
- Les cartes défensives poussent vers positif (Tank)

**Transformations:**

| Seuil | Type | Effet | Permanent |
|-------|------|-------|-----------|
| **+100** | Positif/Tank | Bonus défensifs, HP accrus | Oui (1× par combat) |
| **-100** | Négatif/DPS | Bonus offensifs, dégâts accrus | Oui (1× par combat) |

Voir [Card_System.md](Card_System.md) pour les émotions spécifiques de chaque famille.

---

## 🎯 Système de Ciblage

### Types de Cible (CardTargetType)

| Type | Description | Exemple d'Usage |
|------|-------------|-----------------|
| **None** | Aucune cible | Buff automatique |
| **Self** | Soi-même uniquement | Se soigner |
| **Enemy** | Un ou plusieurs ennemis | Attaques |
| **Ally** | Alliés (sauf soi) | Buff allié |
| **AllyOrSelf** | Alliés ET soi | Soins de groupe |
| **AllyorEnemy** | Alliés ET ennemis | Explosion qui affecte tous |
| **AnyUnit** | N'importe quelle unité | Télékinésie |
| **EmptyTile** | Tuiles vides | Invocation, pièges |
| **AnyTile** | N'importe quelle tuile | Zone d'effet centrée |

Voir [Card_System.md](Card_System.md) pour les détails complets.

---

### Portée des Cartes

| Portée | Distance | Type d'Usage |
|--------|----------|--------------|
| **0** | Soi-même | Buffs personnels |
| **1** | Mêlée (adjacents) | Attaques au corps-à-corps |
| **2-3** | Courte | Sorts courts, armes de jet |
| **4-6** | Moyenne | Sorts standards, arcs |
| **7+** | Longue | Sorts puissants, artillerie |
| **99** | Infinie | Sorts globaux |

---

### Zones d'Effet (AOE)

| Forme | Description | Visualisation | Usage |
|-------|-------------|---------------|-------|
| **None** | Cible unique | • | Attaques précises |
| **OneTile** | Une seule case | • | Piège minimal |
| **Circle** | Cercle (rayon variable) | ○ | Explosions |
| **Line** | Ligne droite | \| | Rayon, souffle |
| **Cross** | Croix (4 directions) | + | Onde de choc |
| **Cone** | Cône directionnel | ▷ | Arc de feu |

Voir les schémas ASCII détaillés dans [Card_System.md](Card_System.md).

---

### Interface de Ciblage Visuel

**Étapes de Ciblage:**

| Étape | Feedback Visuel | Action Joueur |
|-------|----------------|---------------|
| **1. Sélection Carte** | Carte s'agrandit, glow vert | Clic sur carte |
| **2. Hover Cible** | Courbe de Bézier + réticule | Déplace souris |
| **3. Validation** | Animation de lancement | Clic gauche |
| **4. Annulation** | Carte retourne en main | Clic droit |

**Composants Visuels:**
- TargetingCurve : Courbe de Bézier quadratique de la carte vers la souris
- TargetingReticle : Réticule circulaire avec croix
- HandUIController : Gestion de la sélection et validation

Voir [Technical_Specs.md](Technical_Specs.md) pour l'implémentation technique.

---

## 💥 Résolution des Effets

### Ordre de Résolution d'une Carte

| Étape | Action | Vérifications |
|-------|--------|---------------|
| **1. Validation** | Vérifier cible valide | Portée, PA suffisants |
| **2. Coût** | Dépenser les PA | Déduction immédiate |
| **3. Calcul** | Calculer dégâts/effets | Appliquer modificateurs |
| **4. Application** | Appliquer effets | Dégâts, soins, mouvements |
| **5. Émotion** | Modifier jauge émotionnelle | Selon modificateur de la carte |
| **6. Vérification** | Vérifier morts | Retirer unités vaincues |

---

### Formules de Calcul

**Dégâts Physiques:**
- Formule : Dégâts Base - Défense Physique de la Cible
- Minimum : 1 (toujours au moins 1 dégât)

**Dégâts Magiques:**
- Formule : Dégâts Base - Défense Magique de la Cible
- Minimum : 1 (toujours au moins 1 dégât)

**Soins:**
- Formule : Soins Base
- Maximum : HP Max - HP Actuel (ne peut pas dépasser HP max)

**Mouvement Bonus:**
- Formule : PM Actuels + Bonus de Mouvement
- Utilisable immédiatement sur le même tour

---

## 🎲 Effets de Statut (À Implémenter)

### Debuffs Prévus

| Effet | Durée | Effet Mécanique | Stackable |
|-------|-------|-----------------|-----------|
| **Poison** | 3 tours | Perte HP au début du tour | Oui (×5 max) |
| **Brûlure** | 2 tours | Perte HP + réduit soins de 50% | Oui (×3 max) |
| **Stun** | 1 tour | Saute son tour | Non |
| **Gel** | 2 tours | PM réduits à 0 | Non |
| **Faiblesse** | 2 tours | Dégâts -30% | Non |

---

### Buffs Prévus

| Effet | Durée | Effet Mécanique | Stackable |
|-------|-------|-----------------|-----------|
| **Bouclier** | Jusqu'à destruction | Absorbe dégâts avant HP | Non |
| **Force** | 2 tours | Dégâts physiques +30% | Non |
| **Hâte** | 3 tours | +1 PM par tour | Non |
| **Régénération** | 3 tours | Soins au début du tour | Oui (×5 max) |

**Note:** Le système de statuts est prévu mais pas encore implémenté dans le code actuel.

---

## 🧠 Stratégies et Synergies

### Synergie Famille-Classe-Élément

**Exemple de Synergie:**
- Famille Déchaînés + Classe Ombrelame + Élément Aucun = DPS physique pur
- Famille Précurseurs + Classe Tisseur + Élément Feu = Mage AoE explosif
- Famille Gardiens + Classe Veilleur + Élément Lumière = Support soins

**Construction de Deck:**

| Objectif | Familles Recommandées | Classes | Stratégie |
|----------|----------------------|---------|-----------|
| **Tank** | Gardiens, Déchaînés (Contrariété) | Ancre, Veilleur | Transformations positives (+100) |
| **DPS** | Déchaînés (Rage), Réprouvés | Ombrelame, Tisseur | Transformations négatives (-100) |
| **Support** | Éveillés, Gardiens | Veilleur, Harmoniste | Buffs et soins |
| **Control** | Dissidents, Exilés | Tisseur, Ancre | Manipulation terrain |

---

### Positionnement Tactique

**Importance de la Position:**

| Facteur | Impact |
|---------|--------|
| **Portée** | Cartes mêlée (1) nécessitent proximité |
| **AOE** | Regroupement amplifie dégâts ennemis |
| **Mobilité** | PM limités = planifier mouvement |
| **Ligne de Vue** | Obstacles peuvent bloquer (si implémenté) |

**Formations Recommandées:**
- Ligne : Couverture maximale du terrain
- Dispersée : Évite les AOE ennemies
- Tactique : Protéger les champions fragiles derrière les tanks

---

## 📈 Difficulté et Équilibrage (À Implémenter)

### Système de Difficulté Prévu

| Difficulté | HP Ennemis | Dégâts Ennemis | Modificateurs Joueur |
|------------|------------|----------------|---------------------|
| **Facile** | -30% | -20% | +1 PA |
| **Normal** | 100% | 100% | Aucun |
| **Difficile** | +50% | +30% | -1 PA, meilleur loot |

---

### Équilibrage des Cartes

**Principes de Design:**
- Coût PA proportionnel aux effets
- AOE coûte plus cher que cible unique
- Émotions extrêmes = cartes plus puissantes
- Cartes neutres (0 émotion) = polyvalentes

**Formules de Référence:** Voir [Card_System.md](Card_System.md) section Équilibrage.

---

## 📝 Systèmes À Développer

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

---

**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Combat Project TDB
