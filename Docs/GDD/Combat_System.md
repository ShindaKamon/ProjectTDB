# ⚔️ Système de Combat - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## 🎯 Vue d'Ensemble

Le système de combat de **Project TDB** combine combat tactique sur grille hexagonale et gestion de cartes pour créer une expérience stratégique profonde où chaque décision compte. Les joueurs doivent équilibrer positionnement, gestion de ressources et construction de combos pour vaincre leurs adversaires.

---

## 🔄 Déroulement d'un Combat

### Phase 1: Initialisation du Combat

**Actions:**
1. Chargement de la scène de combat
2. Placement des unités (joueurs et ennemis) sur la grille
3. Calcul de l'initiative pour déterminer l'ordre des tours
4. Mélange des decks de chaque personnage
5. Pioche des mains initiales (5 cartes par personnage)
6. Attribution des ressources de départ (PA, PM, ressources spéciales)

**Règles d'Initiative:**
- Basé sur la statistique de Vitesse de chaque unité
- Modificateurs possibles (cartes, effets de terrain)
- L'ordre est recalculé chaque tour

### Phase 2: Tour d'un Personnage Joueur

**1. Début de Tour**
- Pioche de cartes jusqu'à avoir 5 cartes en main
- Restauration des PA (3-5 selon le personnage)
- Restauration des PM (2-4 selon le personnage)
- Résolution des effets de début de tour (poison, régénération, etc.)

**2. Phase d'Action (Libre)**

Le joueur peut effectuer les actions suivantes dans l'ordre de son choix:

- **Jouer des Cartes**
  - Sélectionner une carte dans la main
  - Payer le coût en PA
  - Choisir une cible valide (si nécessaire)
  - Résoudre les effets de la carte

- **Se Déplacer**
  - Dépenser des PM pour se déplacer sur la grille
  - 1 PM = 1 case hexagonale
  - Mouvement bloqué par les obstacles et ennemis

- **Utiliser des Capacités Passives**
  - Certaines capacités ne coûtent pas de PA
  - Effets déclenchés automatiquement

**3. Fin de Tour**
- Résolution des effets de fin de tour
- Défausse des cartes en excès (limite de main: 10 cartes)
- Passage au personnage suivant dans l'ordre d'initiative

### Phase 3: Tour des Ennemis

**IA Ennemie:**
1. Évaluation de la situation (distance aux joueurs, santé, etc.)
2. Choix de l'action optimale:
   - Attaquer un joueur à portée
   - Se déplacer vers un joueur
   - Utiliser une capacité spéciale
   - Se protéger si santé basse
3. Exécution de l'action

**Patterns d'IA:**
- **Agressif** : Attaque toujours la cible la plus proche
- **Défensif** : Privilégie la protection et reste à distance
- **Tactique** : Utilise le terrain et les capacités de manière optimale
- **Support** : Aide les autres ennemis (buffs, soins)

### Phase 4: Fin du Combat

**Conditions de Victoire:**
- Tous les ennemis sont vaincus → Victoire
- Tous les personnages joueurs sont vaincus → Défaite

**Récompenses (Victoire):**
- Expérience pour les personnages
- Or et gemmes
- Nouvelles cartes (choix parmi 3 options)
- Possibilité d'améliorer une carte existante
- Items/équipement (rare)

---

## 📊 Ressources de Combat

### Points d'Action (PA)

**Caractéristiques:**
- Ressource principale pour jouer des cartes
- Régénération complète au début de chaque tour
- Quantité variable selon le personnage (3-5 PA)
- Ne se cumule PAS entre les tours

**Coût des Cartes:**
- **0 PA** : Cartes gratuites (rares, souvent conditionnelles)
- **1 PA** : Cartes basiques (attaques faibles, déplacements)
- **2 PA** : Cartes standards (attaques moyennes, buffs)
- **3 PA** : Cartes puissantes (gros dégâts, AoE)
- **4+ PA** : Cartes ultimes (effets dévastateurs)

**Gestion:**
- Impossible de jouer une carte si PA insuffisants
- Cartes non jouables sont grisées dans l'UI
- Texte de coût devient rouge si insuffisant

### Points de Mouvement (PM)

**Caractéristiques:**
- Ressource pour le déplacement sur la grille
- Régénération complète au début de chaque tour
- Quantité variable selon le personnage (2-4 PM)
- 1 PM = 1 case hexagonale

**Règles de Mouvement:**
- Déplacement bloqué par les obstacles
- Impossible de traverser une case occupée par un ennemi
- Certaines cartes permettent de se téléporter (ignorent les obstacles)
- Le terrain peut modifier le coût (terrain difficile = 2 PM)

### Santé (HP)

**Caractéristiques:**
- Points de vie du personnage
- Tombe à 0 → Personnage vaincu
- Peut être restaurée via cartes ou compétences
- Maximum fixe selon le personnage

**Dégâts:**
- **Physiques** : Dégâts directs, réduits par l'armure
- **Magiques** : Ignorent partiellement l'armure
- **Vrais** : Ignorent toutes les défenses (rares)

**Protection:**
- **Armure** : Réduit les dégâts physiques
- **Bouclier** : Points de vie temporaires (absorbent les dégâts)
- **Esquive** : Chance d'éviter complètement les dégâts

### Ressources Spéciales

**Rage (Ilya):**
- Génération: +1 Rage par attaque effectuée
- Maximum: 10 Rage
- Utilisation: Certaines cartes puissantes consomment la Rage
- Stratégie: Enchaîner les attaques pour monter la Rage, puis utiliser une carte ultime

**Mana (Ayla):**
- Génération: +2 Mana par tour
- Maximum: 10 Mana
- Utilisation: Sorts de mage consomment du Mana
- Régénération passive chaque tour

**Émotion (Système avancé):**
- États émotionnels affectant les statistiques
- **Joyeux** : +10% dégâts, +1 PA
- **Triste** : -10% dégâts, +1 défense
- **Colérique** : +20% dégâts, -1 défense
- **Calme** : +1 PM, +10% précision

---

## 🎯 Système de Ciblage

### Types de Cibles

**1. Cible Unique (Single Target)**
- Sélection d'une seule cible (allié ou ennemi)
- Doit être à portée
- Ligne de vue requise (optionnel selon la carte)

**2. Zone d'Effet (AoE)**
- **Cercle** : Rayon autour d'un point (ex: 2 cases de rayon)
- **Ligne** : Toutes les cases en ligne droite
- **Cône** : Zone conique dans une direction
- **Croix** : 4 cases adjacentes

**3. Auto-ciblage (Self)**
- Cible automatiquement le lanceur
- Pas de sélection nécessaire

**4. Toutes les Cibles (All Enemies / All Allies)**
- Affecte automatiquement toutes les cibles valides
- Pas de sélection nécessaire

### Portée des Cartes

**Distances:**
- **Mêlée** : 1 case (cases adjacentes uniquement)
- **Courte** : 2-3 cases
- **Moyenne** : 4-5 cases
- **Longue** : 6+ cases
- **Infinie** : Toute la grille

**Ligne de Vue:**
- Certaines cartes nécessitent une ligne de vue dégagée
- Obstacles bloquent la ligne de vue
- Alliés ne bloquent PAS la ligne de vue

### Interface de Ciblage

**Visuel:**
1. **Sélection de Carte** :
   - Carte sélectionnée s'agrandit légèrement
   - Glow vert autour de la carte
   - Cases valides s'illuminent sur la grille

2. **Hover sur Cible**:
   - Courbe de Bézier de la carte jusqu'à la souris
   - Réticule circulaire avec croix à la position de la souris
   - Preview des dégâts/effets sur la cible

3. **Validation**:
   - Clic gauche pour confirmer
   - Clic droit pour annuler
   - Carte retourne dans la main si annulée

**Implémentation (voir [Technical_Specs.md](Technical_Specs.md)):**
- `TargetingCurve.cs` : Courbe de Bézier quadratique
- `TargetingReticle.cs` : Réticule circulaire avec crosshair
- `HandUIController.cs` : Gestion de la sélection et ciblage

---

## 💥 Résolution des Effets

### Ordre de Résolution

1. **Validation de la Cible**
   - Vérifier que la cible est toujours valide
   - Vérifier la portée et ligne de vue

2. **Calcul des Dégâts/Effets**
   - Appliquer les bonus/malus du lanceur
   - Appliquer les modificateurs de la cible
   - Calcul final des dégâts

3. **Application des Effets**
   - Dégâts/soins
   - Effets de statut (poison, brûlure, étourdissement)
   - Déplacements forcés
   - Buffs/Debuffs

4. **Résolution des Déclencheurs**
   - Capacités "On Hit" (ex: riposte)
   - Capacités "On Damage" (ex: absorption)
   - Effets en chaîne

5. **Vérification des Morts**
   - Personnages/ennemis vaincus sont retirés
   - Déclenchement des effets "On Death"
   - Vérification des conditions de victoire/défaite

### Formules de Calcul

**Dégâts Physiques:**
```
Dégâts Finaux = (Dégâts Base × Multiplicateur) - Armure Cible
Minimum = 1 (toujours au moins 1 dégât)
```

**Dégâts Magiques:**
```
Dégâts Finaux = (Dégâts Base × Multiplicateur) - (Armure Cible × 0.5)
```

**Soins:**
```
Soins Finaux = Soins Base × (1 + Bonus de Soin %)
Maximum = HP Max - HP Actuel
```

---

## 🎲 Effets de Statut

### Debuffs

**Poison:**
- Perte de HP au début de chaque tour
- Dégâts = Stacks × 2
- Durée: 3 tours
- Stackable jusqu'à 5 fois

**Brûlure:**
- Perte de HP au début de chaque tour
- Dégâts = Stacks × 3
- Durée: 2 tours
- Stackable jusqu'à 3 fois
- Réduit les soins reçus de 50%

**Étourdissement (Stun):**
- Le personnage saute son tour
- Durée: 1 tour
- Non-stackable

**Gel (Freeze):**
- PM réduits à 0
- Durée: 2 tours
- Brisé si le personnage reçoit des dégâts de feu

**Faiblesse:**
- Dégâts infligés réduits de 30%
- Durée: 2 tours

### Buffs

**Bouclier:**
- Points de vie temporaires
- Absorbe les dégâts avant la santé
- Disparaît à la fin du combat

**Force:**
- Dégâts physiques augmentés de 30%
- Durée: 2 tours

**Hâte:**
- +1 PM par tour
- Durée: 3 tours

**Régénération:**
- Récupère HP au début de chaque tour
- Soins = Stacks × 3
- Durée: 3 tours
- Stackable jusqu'à 5 fois

---

## 🧠 Stratégies et Synergies

### Combos de Base

**Combo Mêlée (Ilya):**
1. Se rapprocher de l'ennemi (PM)
2. Attaque basique (1 PA) → +1 Rage
3. Attaque basique (1 PA) → +1 Rage
4. Frappe puissante (2 PA, consomme 2 Rage) → Gros dégâts

**Combo AoE (Ayla):**
1. Boule de feu (2 PA, 3 Mana) → Dégâts + Brûlure
2. Explosion de mana (3 PA, 5 Mana) → Double dégâts sur cibles brûlées

**Combo Défensif:**
1. Bouclier (1 PA) → +5 Bouclier
2. Retraite tactique (1 PA) → Recul de 2 cases
3. Contre-attaque (0 PA, réaction) → Riposte si attaqué

### Positionnement Tactique

**Formations:**
- **Ligne** : Maximise la couverture de terrain
- **Triangle** : Protection mutuelle, bon contre focus
- **Écartée** : Évite les AoE ennemies

**Utilisation du Terrain:**
- **Hauteur** : Bonus de dégâts depuis les cases élevées
- **Couverture** : Réduction des dégâts derrière les obstacles
- **Zones Dangereuses** : Lave, poison, pièges

---

## 📈 Difficulté et Équilibrage

### Scaling des Ennemis

**Facteurs:**
- Niveau du joueur
- Nombre de personnages dans l'équipe
- Progression dans la campagne

**Ajustements:**
- HP des ennemis: × (1 + 0.1 × Niveau)
- Dégâts des ennemis: × (1 + 0.08 × Niveau)
- Nombre d'ennemis: Variable selon le combat

### Système de Difficulté

**Facile:**
- Ennemis -30% HP
- Ennemis -20% dégâts
- +1 PA pour tous les personnages

**Normal:**
- Valeurs de base

**Difficile:**
- Ennemis +50% HP
- Ennemis +30% dégâts
- -1 PA pour tous les personnages
- Meilleur loot

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Design Combat Project TDB
