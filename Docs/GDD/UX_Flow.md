# 🌊 Flux d'Expérience Utilisateur - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## 🎯 Philosophie UX

L'expérience utilisateur de **Project TDB** doit:
1. **Guider sans Contraindre** : Suggérer les actions optimales tout en permettant l'exploration
2. **Récompenser la Maîtrise** : Les joueurs expérimentés doivent sentir leur progression
3. **Minimiser la Friction** : Réduire les clics et confirmations inutiles
4. **Fournir un Feedback Constant** : Chaque action doit avoir une réponse visuelle/sonore

---

## 🚀 Première Expérience (First Time User Experience)

### Lancement du Jeu

**1. Écran de Titre (5 secondes)**
```
╔══════════════════════════════════════╗
║                                      ║
║        PROJECT TDB                   ║
║   Tactical Deck Builder              ║
║                                      ║
║   [Nouvelle Partie]                  ║
║   [Continuer]         (grisé)        ║
║   [Options]                          ║
║   [Quitter]                          ║
║                                      ║
╚══════════════════════════════════════╝
```

**Musique:** Thème principal (orchestral épique)
**Animation:** Logo fade in, particules d'arrière-plan

**2. Nouvelle Partie → Sélection de Personnage (30 secondes)**
```
╔══════════════════════════════════════╗
║  Choisissez votre premier héros      ║
╠══════════════════════════════════════╣
║  [  ILYA  ]      [  AYLA  ]         ║
║  ┌────────┐      ┌────────┐         ║
║  │[Image] │      │[Image] │         ║
║  │Épéiste │      │  Mage  │         ║
║  └────────┘      └────────┘         ║
║                                      ║
║  Style: Agressif  Style: Contrôle   ║
║  Difficulté: ⭐⭐   Difficulté: ⭐⭐⭐  ║
║                                      ║
║          [COMMENCER]                 ║
╚══════════════════════════════════════╝
```

**Interactions:**
- Hover sur personnage → Preview animé + description détaillée
- Clic sur personnage → Sélection (highlight)
- Bouton "Commencer" → Transition vers tutoriel

**3. Tutoriel Interactif (10-15 minutes)**

Voir [Tutorial.md](Tutorial.md) pour le détail complet.

**Étapes:**
1. Introduction à la grille et au mouvement
2. Explication des cartes et de la main
3. Premier combat guidé (vs 2 Gobelins)
4. Récompense et amélioration de deck
5. Transition vers la campagne

---

## 🎮 Boucle de Jeu Principale

### Vue d'Ensemble du Flow

```
Menu Principal
    ↓
Sélection Campagne/Mode
    ↓
Préparation (Deck, Équipe)
    ↓
╔═══════════════════╗
║   BOUCLE DE RUN   ║
╠═══════════════════╣
║ Combat            ║
║    ↓              ║
║ Victoire          ║
║    ↓              ║
║ Récompenses       ║
║    ↓              ║
║ Événement (25%)   ║
║    ↓              ║
║ Boutique (20%)    ║
║    ↓              ║
║ Prochain Combat   ║
║    ↓              ║
║ (Répéter)         ║
╚═══════════════════╝
    ↓
Boss Final
    ↓
Victoire/Défaite
    ↓
Statistiques & Récompenses
    ↓
Menu Principal
```

### Écran de Carte (Map)

**Fonctionnement:**
- Carte avec chemins possibles
- Icônes représentant les types de rencontres:
  - ⚔️ Combat facile
  - ⚔️⚔️ Combat difficile
  - 👑 Boss
  - 🏪 Boutique
  - ❓ Événement
  - 🔥 Combat Élite
  - 💰 Trésor

**UI:**
```
╔════════════════════════════════════════════╗
║                                            ║
║      ACTE 1 - Forêt des Gobelins          ║
║      Combat 3/10                           ║
║                                            ║
║           👑 (Boss)                        ║
║          /    \                            ║
║        ⚔️      🏪                          ║
║        /  \    /                           ║
║      ⚔️⚔️  ⚔️  ❓                          ║
║        \  /  \ /                           ║
║         ⚔️    🔥                           ║
║           \  /                             ║
║            ●  ← Vous êtes ici              ║
║                                            ║
║  [Deck] [Personnages] [Progression]       ║
╚════════════════════════════════════════════╝
```

**Interactions:**
- Clic sur un nœud accessible → Preview du combat/événement
- Confirmation → Transition vers la rencontre

---

## ⚔️ Flow de Combat

### Phase 1: Chargement et Placement

**Durée:** 2-3 secondes

**Séquence:**
1. Fade in de la scène de combat
2. Grille apparaît (animation de matérialisation)
3. Personnages se téléportent sur leurs positions
4. Ennemis apparaissent (animation d'entrée)
5. Calcul de l'initiative (barre d'initiative apparaît)
6. Mélange et pioche des cartes (animation)

**UI Visible:**
- Grille de combat
- HUD des personnages (côté gauche)
- HUD des ennemis (au-dessus d'eux)
- Barre d'initiative (haut)
- Main vide (bas, en attente de pioche)

### Phase 2: Début du Tour Joueur

**Séquence:**
1. Message "À VOTRE TOUR" (0.5s)
2. Restauration des ressources (PA, PM)
3. Pioche de cartes (animation 1s)
4. Effets de début de tour (poison, régénération)
5. Activation des contrôles

**Feedback Visuel:**
- Flash de couleur sur le portrait du personnage actif
- Son de début de tour
- Cartes volent depuis le deck vers la main

### Phase 3: Actions du Joueur

**Flow d'Action:**

**Option A: Jouer une Carte**
```
Clic sur Carte
    ↓
Carte sélectionnée (glow, déplacée à gauche)
    ↓
Ciblage activé (courbe + réticule)
    ↓
Hover sur cible valide → Preview des effets
    ↓
Clic sur cible → Confirmation
    ↓
Animation de jeu de carte
    ↓
Résolution des effets
    ↓
Carte dans la défausse
    ↓
Retour à la main
```

**Option B: Se Déplacer**
```
Clic sur Personnage (ou sélectionné par défaut)
    ↓
Cases de mouvement highlighted (vert)
    ↓
Clic sur case de destination
    ↓
Preview du chemin (flèches)
    ↓
Confirmation (clic ou Enter)
    ↓
Animation de mouvement
    ↓
Déduction des PM
    ↓
Fin de l'action
```

**Option C: Fin de Tour**
```
Clic sur "Fin de Tour" (ou touche Enter)
    ↓
Confirmation si PA/PM non utilisés (optionnel)
    ↓
Effets de fin de tour (trigger)
    ↓
Transition vers le tour suivant
```

### Phase 4: Tour de l'Ennemi

**Séquence:**
1. Message "[NOM ENNEMI] AGIT" (0.5s)
2. Ennemi réfléchit (0.5-1s, animation "thinking")
3. Décision de l'IA
4. Exécution de l'action (mouvement + attaque)
5. Effets résolus
6. Fin du tour ennemi

**Feedback Visuel:**
- Portrait de l'ennemi highlighted
- Intention affichée (icône au-dessus: attaque, mouvement, buff)
- Animation d'action
- Dégâts/effets appliqués

**Vitesse:**
- Rapide par défaut (1-2s par tour ennemi)
- Option pour ralentir (utile pour apprentissage)

### Phase 5: Fin du Combat

**Victoire:**
```
Dernier ennemi vaincu
    ↓
Animation de victoire (0.5s)
    ↓
Message "VICTOIRE!" (1s)
    ↓
Statistiques du combat (5s)
    ↓
Écran de récompenses
```

**Défaite:**
```
Tous les alliés vaincus
    ↓
Animation de défaite (0.5s)
    ↓
Message "DÉFAITE" (1s)
    ↓
Statistiques du combat
    ↓
Options:
    - Recommencer le combat (-50 Or)
    - Abandonner le run (retour menu)
```

---

## 🎁 Écran de Récompenses

### UI des Récompenses

```
╔══════════════════════════════════════════════╗
║          VICTOIRE !                          ║
╠══════════════════════════════════════════════╣
║                                              ║
║  +150 XP     +60 Or     +10 Gemmes          ║
║                                              ║
║  Choisissez une carte à ajouter:            ║
║  ┌─────────┐  ┌─────────┐  ┌─────────┐     ║
║  │ [Carte] │  │ [Carte] │  │ [Carte] │     ║
║  │  RARE   │  │ COMMUNE │  │  RARE   │     ║
║  └─────────┘  └─────────┘  └─────────┘     ║
║                                              ║
║           [Ignorer] [Confirmer]             ║
╚══════════════════════════════════════════════╝
```

**Flow:**
1. Affichage des récompenses passives (XP, Or)
2. Animations de compteur (nombre qui augmente)
3. Affichage des choix de cartes (révélation progressive)
4. Sélection du joueur (hover pour voir détails)
5. Confirmation
6. Carte ajoutée au deck (animation)
7. Transition vers la carte du monde

**Raccourcis Clavier:**
- Touches 1, 2, 3 pour sélectionner les cartes
- Espace pour ignorer
- Enter pour confirmer

---

## 🏪 Boutique

### UI de la Boutique

```
╔══════════════════════════════════════════════╗
║          BOUTIQUE DU VOYAGEUR                ║
║          Or disponible: 120                  ║
╠══════════════════════════════════════════════╣
║                                              ║
║  CARTES DISPONIBLES:                        ║
║  ┌─────────┐  ┌─────────┐  ┌─────────┐     ║
║  │ [Carte] │  │ [Carte] │  │ [Carte] │     ║
║  │  50 Or  │  │  75 Or  │  │ 100 Or  │     ║
║  └─────────┘  └─────────┘  └─────────┘     ║
║                                              ║
║  SERVICES:                                   ║
║  [Améliorer une carte - 100 Or]             ║
║  [Supprimer une carte - 50 Or]              ║
║  [Acheter une Potion - 30 Or]               ║
║                                              ║
║           [Quitter la Boutique]             ║
╚══════════════════════════════════════════════╝
```

**Interactions:**
- Clic sur carte → Preview détaillée
- Clic sur "Acheter" → Confirmation si assez d'Or
- Or déduit immédiatement
- Carte ajoutée au deck

**Feedback:**
- Animation de pièces qui disparaissent
- Son de transaction
- Carte qui vole vers le deck

---

## 🎲 Événements Aléatoires

### Types d'Événements

**1. Choix Binaire:**
```
╔══════════════════════════════════════════════╗
║          RENCONTRE MYSTÉRIEUSE               ║
╠══════════════════════════════════════════════╣
║                                              ║
║  Vous trouvez un autel ancien avec une      ║
║  inscription: "Sacrifice pour pouvoir".     ║
║                                              ║
║  Que faites-vous?                           ║
║                                              ║
║  [Sacrifier 20 HP]                          ║
║  → Gagnez une carte Épique                  ║
║                                              ║
║  [Ignorer l'autel]                          ║
║  → Rien ne se passe                         ║
║                                              ║
╚══════════════════════════════════════════════╝
```

**2. Événement de Combat:**
- Mini-boss surprise
- Récompenses accrues

**3. Événement de Ressources:**
- Trésor
- Perte/Gain d'Or

**Flow:**
1. Transition vers l'écran d'événement
2. Lecture de la description (5-10s)
3. Présentation des choix
4. Sélection du joueur
5. Résolution immédiate
6. Transition vers la suite

---

## 📊 Écran de Deck

### UI de Consultation du Deck

```
╔══════════════════════════════════════════════╗
║          DECK DE ILYA (25 cartes)           ║
╠══════════════════════════════════════════════╣
║                                              ║
║  [Toutes] [Attaque] [Défense] [Util]        ║
║                                              ║
║  ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐            ║
║  │ 5×│ │ 3×│ │ 2×│ │ 2×│ │ 1×│            ║
║  └───┘ └───┘ └───┘ └───┘ └───┘            ║
║                                              ║
║  [Trier: Coût] [Trier: Nom] [Trier: Type]  ║
║                                              ║
║           [Retour]                           ║
╚══════════════════════════════════════════════╝
```

**Fonctionnalités:**
- Filtrage par type
- Tri par différents critères
- Affichage du nombre de copies
- Clic sur carte → Détails complets
- Accessible depuis la carte du monde

---

## ⚙️ Options et Paramètres

### Menu Options

```
╔══════════════════════════════════════════════╗
║               OPTIONS                        ║
╠══════════════════════════════════════════════╣
║                                              ║
║  AUDIO:                                      ║
║  Musique:     ▓▓▓▓▓▓▓░░░ 70%                ║
║  Effets:      ▓▓▓▓▓▓▓▓▓░ 90%                ║
║  Ambiance:    ▓▓▓▓▓░░░░░ 50%                ║
║                                              ║
║  GRAPHIQUES:                                 ║
║  Résolution:  [1920×1080 ▼]                 ║
║  Plein écran: [✓]                           ║
║  VSync:       [✓]                           ║
║  Qualité:     [Élevée ▼]                    ║
║                                              ║
║  GAMEPLAY:                                   ║
║  Vitesse IA:  [Normale ▼]                   ║
║  Confirmations: [✓]                         ║
║  Tutoriels:   [✓]                           ║
║                                              ║
║  ACCESSIBILITÉ:                              ║
║  Taille texte: [Normal ▼]                   ║
║  Daltonisme:  [Aucun ▼]                     ║
║                                              ║
║      [Appliquer]  [Retour]                  ║
╚══════════════════════════════════════════════╝
```

**Paramètres Sauvegardés:**
- Automatiquement dans PlayerPrefs
- Application immédiate pour la plupart
- Confirmation pour changements majeurs (résolution)

---

## 🔄 Transitions et Chargements

### Types de Transitions

**1. Fade In/Out (Standard):**
- Durée: 0.3-0.5s
- Utilisé pour: Menus → Jeu, Combats → Carte

**2. Wipe (Balayage):**
- Durée: 0.5s
- Direction: Gauche → Droite
- Utilisé pour: Changement d'acte

**3. Zoom In (Carte → Combat):**
- Durée: 0.8s
- Zoom sur le nœud sélectionné
- Fondu vers la scène de combat

**4. Instant (Récompenses → Carte):**
- Pas de transition
- Changement immédiat
- Moins de friction

### Écrans de Chargement

**Court (<2s):**
- Barre de progression simple
- Pas de texte, juste l'icône du jeu

**Moyen (2-5s):**
- Barre de progression
- Tips de gameplay aléatoires
- Illustration d'arrière-plan

**Long (>5s, rare):**
- Barre de progression
- Tips de gameplay
- Mini-jeu optionnel (ex: cliquer pour bonus mineur)

---

## 🎯 Feedback et Satisfaction

### Moments de Satisfaction (Juicy Moments)

**1. Élimination d'Ennemi:**
- Animation de mort spectaculaire
- Particules d'explosion
- Son impactant
- Texte "ÉLIMINÉ!" qui pop
- Shake screen léger

**2. Combo de Cartes:**
- Enchaînement rapide (3+ cartes)
- Multiplicateur de dégâts affiché
- Effet visuel spécial (lightning entre les cartes)
- Son de combo crescendo

**3. Coup Critique:**
- Freeze frame (0.1s)
- Flash lumineux
- Son métallique
- Texte "CRITIQUE!" en gros

**4. Victoire de Boss:**
- Slow motion de l'attaque finale
- Explosion massive
- Écran blanc flash
- Fanfare musicale
- Décompte de récompenses théâtral

### Prévention de la Frustration

**1. Undo (Annulation):**
- Possibilité d'annuler le dernier mouvement (avant confirmation)
- Coût: Aucun
- Limite: 1 annulation par tour

**2. Preview Omniprésent:**
- Toujours afficher l'effet avant confirmation
- Preview de dégâts, portée, zone d'effet
- Pas de surprise négative

**3. Confirmations Optionnelles:**
- Désactivables dans les options
- Activées par défaut pour nouveaux joueurs
- Exemples:
  - "Terminer le tour avec des PA inutilisés?"
  - "Ignorer cette récompense?"

**4. Sauvegarde Automatique:**
- Avant chaque combat
- Après chaque récompense
- Jamais de perte de progression

---

## 📱 Raccourcis Clavier

### Combats

- **Espace** : Fin de tour
- **Entrée** : Confirmer l'action
- **Échap** : Annuler la sélection
- **1-9** : Sélectionner carte dans la main
- **Tab** : Cycler entre les ennemis
- **Z** : Annuler le dernier mouvement

### Navigation

- **Échap** : Menu pause / Retour
- **M** : Carte du monde
- **D** : Deck
- **C** : Personnages
- **O** : Options

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Design UX Project TDB
