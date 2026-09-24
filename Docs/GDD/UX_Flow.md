# 🌊 Flux d'Expérience Utilisateur - Émotions Tactics (Project TDB)

**Version:** 1.3
**Date:** 23 Septembre 2026
**Changements :**
- v1.1 (10/09/2026) : Ayla retirée de l'écran de sélection.
- v1.2 (23/09/2026) : boucle roguelike (carte à nœuds, runs, événements aléatoires) remplacée par la **campagne façon Waven** ; écran de sélection avec les 3 champions du MVP ; barre d'initiative retirée (ordre par phases) ; raccourcis clavier unifiés (Espace = fin de tour) ; lien cassé vers `Tutorial.md` corrigé. Ce document est la **référence pour les raccourcis clavier**.
- v1.3 (23/09/2026) : roster Evan / Crux / Raze (Excel MVP) ; deck de 24 cartes et plusieurs decks par champion ; donjon en équipe de 3 ; règle de main à trancher.

---

## 🎯 Philosophie UX

L'expérience utilisateur d'**Émotions Tactics** doit :
1. **Guider sans Contraindre** : suggérer les actions optimales tout en permettant l'exploration
2. **Récompenser la Maîtrise** : les joueurs expérimentés doivent sentir leur progression
3. **Minimiser la Friction** : réduire les clics et confirmations inutiles
4. **Fournir un Feedback Constant** : chaque action doit avoir une réponse visuelle/sonore

---

## 🚀 Première Expérience (First Time User Experience)

### Lancement du Jeu

**1. Écran de Titre (5 secondes)**
```
╔══════════════════════════════════════╗
║                                      ║
║        ÉMOTIONS TACTICS              ║
║   (nom de code: Project TDB)         ║
║                                      ║
║   [Nouvelle Partie]                  ║
║   [Continuer]         (grisé)        ║
║   [Options]                          ║
║   [Quitter]                          ║
║                                      ║
╚══════════════════════════════════════╝
```

**Musique :** Thème principal (orchestral épique)
**Animation :** Logo fade in, particules d'arrière-plan

**2. Nouvelle Partie → Sélection de Personnage (30 secondes)**

> Roster MVP : Evan, Crux, Raze (Excel). Les donjons se jouent en **équipe de 3** : à terme cet écran sert à composer l'équipe (et à choisir un champion pour l'aventure solo). Coop ou un seul joueur qui contrôle les 3 : question ouverte (`GDD_Main.md`).
> **Code actuel** : on choisit **un seul** champion (`Screen_ChampionSelect` : roster à gauche, illustration du champion au centre, histoire et statistiques à droite avec le bouton « Choisir ce champion »), puis son deck sur la page Choix du deck (`Screen_DeckSelect` : sélectionner un deck, puis Modifier / Renommer / Supprimer, + pour en créer un, Commencer pour lancer le combat) ; Modifier ouvre le gestionnaire de deck (`Screen_DeckManager`, style MTG Arena), dont on revient avec Retour — voir `Technical_Specs.md` § « État du code ».

```
╔══════════════════════════════════════════════╗
║  Composez votre équipe                       ║
╠══════════════════════════════════════════════╣
║  [ EVAN ]    [   CRUX   ]   [   RAZE   ]    ║
║  ┌───────┐    ┌───────┐       ┌───────┐      ║
║  │[Image]│    │[Image]│       │[Image]│      ║
║  └───────┘    └───────┘       └───────┘      ║
║  Invocateur   Grappin         Combos de coûts║
║  (écho Lyse)  (tank/assassin) (Main gagnante)║
║                                              ║
║               [COMMENCER]                    ║
╚══════════════════════════════════════════════╝
```

**Interactions :**
- Hover sur personnage → Preview animé + description détaillée
- Clic sur personnage → Sélection (highlight)
- Bouton « Commencer » → Transition vers tutoriel

**3. Tutoriel Interactif (10-15 minutes)**

Document de tutoriel détaillé : **à créer** (pas encore de doc dédié dans le projet).

**Étapes :**
1. Introduction à la grille et au mouvement
2. Explication des cartes et de la main
3. Premier combat guidé (vs 2 ennemis du thème Peur, dans l'Orphelinat)
4. Récompense et amélioration de deck
5. Transition vers la campagne

---

## 🎮 Boucle de Jeu Principale — Campagne façon Waven

### Vue d'Ensemble du Flow

```
Menu Principal
    ↓
Écran de Campagne (liste des donjons débloqués)
    ↓
Préparation (Équipe de 3, choix du deck de chaque champion, Boutique)
    ↓
╔══════════════════════════╗
║   DONJON (linéaire)      ║
╠══════════════════════════╣
║ Combat 1                 ║
║    ↓                     ║
║ Récompenses              ║
║    ↓                     ║
║ Combat 2 … (3 à 5)       ║
║    ↓                     ║
║ (Événement scénarisé)    ║
║    ↓                     ║
║ Boss du donjon           ║
╚══════════════════════════╝
    ↓
Victoire → le donjon retrouve sa couleur
    ↓
Statistiques & Récompenses (conservées)
    ↓
Donjon suivant débloqué → Écran de Campagne
```

Tout ce qui est gagné (XP, cartes, Or) est **conservé** — pas de remise à zéro. En cas de défaite, on recommence le combat ou on revient à l'écran de campagne sans rien perdre de sa progression.

### Écran de Campagne

**Fonctionnement :**
- Liste (ou carte illustrée) des donjons, débloqués dans l'ordre
- Chaque donjon affiche son émotion dominante, sa couleur (grise tant qu'il n'est pas rééquilibré) et sa progression (combats terminés)
- Un donjon terminé apparaît en couleur

**UI :**
```
╔════════════════════════════════════════════╗
║                                            ║
║      ACTE 1                                ║
║                                            ║
║   [Orphelinat — Peur]      ░░░ 2/5 combats ║
║   [Bureau — Anxiété]       🔒              ║
║   [Maison — Colère]        🔒              ║
║                                            ║
║  [Deck] [Personnages] [Progression]        ║
╚════════════════════════════════════════════╝
```

**Interactions :**
- Clic sur un donjon débloqué → Écran de préparation
- Confirmation → Combat suivant du donjon

---

## ⚔️ Flow de Combat

### Phase 1 : Chargement et Placement

**Durée :** 2-3 secondes

**Séquence :**
1. Fade in de la scène de combat (désaturée — voir `UI_Design.md`)
2. Grille apparaît (animation de matérialisation)
3. Personnages se téléportent sur leurs positions
4. Ennemis apparaissent (animation d'entrée)
5. Mélange et pioche de la main de départ (taille à trancher, animation)

**UI Visible :**
- Grille de combat
- HUD des personnages (côté gauche)
- HUD des ennemis (au-dessus d'eux)
- Indicateur de phase et de tour (haut)
- Main (bas)

### Phase 2 : Début de la Phase Joueur

**Séquence :**
1. Message « À VOTRE TOUR » (0.5s)
2. Restauration des ressources (PA, PM)
3. Pioche selon la règle de main (à trancher — hypothèse de playtest : repioche jusqu'à 3)
4. Effets de début de tour (poison, régénération)
5. Activation des contrôles

**Feedback Visuel :**
- Flash de couleur sur le portrait du personnage actif
- Son de début de tour
- Cartes volent depuis le deck vers la main

### Phase 3 : Actions du Joueur

**Flow d'Action :**

**Option A : Jouer une Carte**
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

**Option B : Se Déplacer**
```
Clic sur Personnage (ou sélectionné par défaut)
    ↓
Cases de mouvement highlighted (vert)
    ↓
Clic sur case de destination
    ↓
Preview du chemin (flèches)
    ↓
Confirmation (clic ou Entrée)
    ↓
Animation de mouvement
    ↓
Déduction des PM
    ↓
Fin de l'action
```

**Option C : Fin de Tour**
```
Clic sur « Fin de Tour » (ou touche Espace)
    ↓
Confirmation si PA/PM non utilisés (optionnel)
    ↓
Effets de fin de tour (trigger)
    ↓
Transition vers la phase ennemie
```

### Phase 4 : Phase Ennemie

**Séquence (pour chaque ennemi, l'un après l'autre) :**
1. Message « [NOM ENNEMI] AGIT » (0.5s)
2. Courte pause (0.5-1s, animation « thinking »)
3. L'ennemi joue la prochaine carte de son pattern
4. Exécution de l'action (mouvement + attaque)
5. Effets résolus
6. Ennemi suivant

**Feedback Visuel :**
- Portrait de l'ennemi highlighted
- Intention affichée à l'avance (icône au-dessus : attaque, mouvement, buff — preview des cartes ennemies)
- Animation d'action
- Dégâts/effets appliqués

**Vitesse :**
- Rapide par défaut (1-2s par ennemi)
- Option pour ralentir (utile pour apprentissage)

### Phase 5 : Fin du Combat

**Victoire :**
```
Dernier ennemi vaincu
    ↓
Animation de victoire (0.5s)
    ↓
La couleur revient (fondu désaturé → couleur de la famille)
    ↓
Message « VICTOIRE ! » (1s)
    ↓
Statistiques du combat (5s)
    ↓
Écran de récompenses
```

**Défaite :**
```
Tous les alliés vaincus
    ↓
Animation de défaite (0.5s)
    ↓
Message « DÉFAITE » (1s)
    ↓
Statistiques du combat
    ↓
Options :
    - Recommencer le combat (-50 Or)
    - Retour à l'écran de campagne (progression conservée)
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

**Flow :**
1. Affichage des récompenses passives (XP, Or)
2. Animations de compteur (nombre qui augmente)
3. Affichage des choix de cartes (révélation progressive)
4. Sélection du joueur (hover pour voir détails)
5. Confirmation
6. Carte ajoutée au deck (animation)
7. Transition vers le combat suivant du donjon (ou l'écran de campagne si le donjon est terminé)

Montants et raretés : voir `Progression.md`.

---

## 🏪 Boutique

Accessible depuis l'écran de préparation, entre deux donjons (et à mi-parcours des donjons longs).

### UI de la Boutique

```
╔══════════════════════════════════════════════╗
║          BOUTIQUE                            ║
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

**Interactions :**
- Clic sur carte → Preview détaillée
- Clic sur « Acheter » → Confirmation si assez d'Or
- Or déduit immédiatement
- Carte ajoutée au deck

**Feedback :**
- Animation de pièces qui disparaissent
- Son de transaction
- Carte qui vole vers le deck

---

## 🎭 Événements Scénarisés

Moments narratifs **placés à la main** dans un donjon (pas de tirage aléatoire). Ils racontent l'esprit de la personne dont c'est le donjon.

### Exemple : Choix Binaire
```
╔══════════════════════════════════════════════╗
║          UN SOUVENIR ENFOUI                  ║
╠══════════════════════════════════════════════╣
║                                              ║
║  Au fond d'un placard, une vieille          ║
║  peluche, encore colorée, au milieu du gris. ║
║                                              ║
║  Que faites-vous?                           ║
║                                              ║
║  [La prendre avec vous (-20 HP)]            ║
║  → Gagnez une carte Épique                  ║
║                                              ║
║  [La laisser où elle est]                   ║
║  → Rien ne se passe                         ║
║                                              ║
╚══════════════════════════════════════════════╝
```

**Autres types :**
- Événement de combat (mini-boss, récompenses accrues)
- Trésor (gain d'Or)

**Flow :**
1. Transition vers l'écran d'événement
2. Lecture de la description (5-10s)
3. Présentation des choix
4. Sélection du joueur
5. Résolution immédiate
6. Transition vers la suite du donjon

---

## 📊 Écran de Deck

### UI de Consultation du Deck

```
╔══════════════════════════════════════════════╗
║     DECK « [NOM] » DE [CHAMPION] (24 cartes)║
╠══════════════════════════════════════════════╣
║                                              ║
║  [Signature 2] [Éveil 6] [Standard 16]      ║
║  Émotions : Colère / Peur (bi-émotion)      ║
║                                              ║
║  ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐            ║
║  │ 5×│ │ 3×│ │ 2×│ │ 1×│ │ 1×│            ║
║  └───┘ └───┘ └───┘ └───┘ └───┘            ║
║                                              ║
║  [Trier: Coût] [Trier: Nom] [Trier: Type]  ║
║                                              ║
║           [Retour]                           ║
╚══════════════════════════════════════════════╝
```

**Fonctionnalités :**
- Plusieurs decks par champion (sélection / création)
- Filtrage par type (Signature / Éveil / Standard) et par émotion
- Compteur de composition (cible 2 + 6 + 16)
- Tri par différents critères
- Affichage du nombre de copies
- Clic sur carte → Détails complets
- Accessible depuis l'écran de campagne

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

**Paramètres Sauvegardés :**
- Automatiquement dans PlayerPrefs
- Application immédiate pour la plupart
- Confirmation pour changements majeurs (résolution)

---

## 🔄 Transitions et Chargements

### Types de Transitions

**1. Fade In/Out (Standard) :**
- Durée : 0.3-0.5s
- Utilisé pour : Menus → Jeu, Combat → Combat suivant

**2. Wipe (Balayage) :**
- Durée : 0.5s
- Direction : Gauche → Droite
- Utilisé pour : Changement d'acte

**3. Zoom In (Campagne → Donjon) :**
- Durée : 0.8s
- Zoom sur le donjon sélectionné
- Fondu vers la scène de combat

**4. Instant (Récompenses → Suite) :**
- Pas de transition
- Changement immédiat
- Moins de friction

### Écrans de Chargement

**Court (<2s) :**
- Barre de progression simple
- Pas de texte, juste l'icône du jeu

**Moyen (2-5s) :**
- Barre de progression
- Tips de gameplay aléatoires
- Illustration d'arrière-plan

**Long (>5s, rare) :**
- Barre de progression
- Tips de gameplay
- Mini-jeu optionnel (ex : cliquer pour bonus mineur)

---

## 🎯 Feedback et Satisfaction

### Moments de Satisfaction (Juicy Moments)

**1. Élimination d'Ennemi :**
- Animation de mort spectaculaire
- Particules d'explosion (et une touche de couleur qui revient)
- Son impactant
- Texte « ÉLIMINÉ ! » qui pop
- Shake screen léger

**2. Combo de Cartes :**
- Enchaînement rapide (3+ cartes)
- Multiplicateur de dégâts affiché
- Effet visuel spécial (lightning entre les cartes)
- Son de combo crescendo

**3. Coup Critique :**
- Freeze frame (0.1s)
- Flash lumineux
- Son métallique
- Texte « CRITIQUE ! » en gros

**4. Victoire de Boss :**
- Slow motion de l'attaque finale
- Retour de la couleur pleine sur tout le donjon
- Fanfare musicale
- Décompte de récompenses théâtral

### Prévention de la Frustration

**1. Undo (Annulation) :**
- Possibilité d'annuler le dernier mouvement (avant confirmation)
- Coût : Aucun
- Limite : 1 annulation par tour

**2. Preview Omniprésent :**
- Toujours afficher l'effet avant confirmation
- Preview de dégâts, portée, zone d'effet
- Pas de surprise négative

**3. Confirmations Optionnelles :**
- Désactivables dans les options
- Activées par défaut pour nouveaux joueurs
- Exemples :
  - « Terminer le tour avec des PA inutilisés ? »
  - « Ignorer cette récompense ? »

**4. Sauvegarde Automatique :**
- Avant chaque combat
- Après chaque récompense
- Jamais de perte de progression

---

## 📱 Raccourcis Clavier *(référence unique)*

### Combats

- **Espace** : Fin de tour
- **Entrée** : Confirmer l'action (déplacement, ciblage)
- **Échap** : Annuler la sélection
- **1-9** : Sélectionner une carte dans la main
- **Tab** : Cycler entre les ennemis
- **Z** : Annuler le dernier mouvement

### Écran de Récompenses

- **1, 2, 3** : Sélectionner une carte
- **Entrée** : Confirmer
- **Échap** : Ignorer la récompense

### Navigation

- **Échap** : Menu pause / Retour
- **M** : Écran de campagne
- **D** : Deck
- **C** : Personnages
- **O** : Options

---

**Dernière mise à jour :** 23 Septembre 2026
**Responsable :** Shinda + Claude
