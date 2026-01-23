# ðŸŒŠ Flux d'ExpÃ©rience Utilisateur - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## ðŸŽ¯ Philosophie UX

L'expÃ©rience utilisateur de **Project TDB** doit:
1. **Guider sans Contraindre** : SuggÃ©rer les actions optimales tout en permettant l'exploration
2. **RÃ©compenser la MaÃ®trise** : Les joueurs expÃ©rimentÃ©s doivent sentir leur progression
3. **Minimiser la Friction** : RÃ©duire les clics et confirmations inutiles
4. **Fournir un Feedback Constant** : Chaque action doit avoir une rÃ©ponse visuelle/sonore

---

## ðŸš€ PremiÃ¨re ExpÃ©rience (First Time User Experience)

### Lancement du Jeu

**1. Ã‰cran de Titre (5 secondes)**
```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘                                      â•‘
â•‘        PROJECT TDB                   â•‘
â•‘   Tactical Deck Builder              â•‘
â•‘                                      â•‘
â•‘   [Nouvelle Partie]                  â•‘
â•‘   [Continuer]         (grisÃ©)        â•‘
â•‘   [Options]                          â•‘
â•‘   [Quitter]                          â•‘
â•‘                                      â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**Musique:** ThÃ¨me principal (orchestral Ã©pique)
**Animation:** Logo fade in, particules d'arriÃ¨re-plan

**2. Nouvelle Partie â†’ SÃ©lection de Personnage (30 secondes)**
```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘  Choisissez votre premier hÃ©ros      â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘  [  ILYA  ]      [  AYLA  ]         â•‘
â•‘  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”      â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”         â•‘
â•‘  â”‚[Image] â”‚      â”‚[Image] â”‚         â•‘
â•‘  â”‚Ã‰pÃ©iste â”‚      â”‚  Mage  â”‚         â•‘
â•‘  â””â”€â”€â”€â”€â”€â”€â”€â”€â”˜      â””â”€â”€â”€â”€â”€â”€â”€â”€â”˜         â•‘
â•‘                                      â•‘
â•‘  Style: Agressif  Style: ContrÃ´le   â•‘
â•‘  DifficultÃ©: â­â­   DifficultÃ©: â­â­â­  â•‘
â•‘                                      â•‘
â•‘          [COMMENCER]                 â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**Interactions:**
- Hover sur personnage â†’ Preview animÃ© + description dÃ©taillÃ©e
- Clic sur personnage â†’ SÃ©lection (highlight)
- Bouton "Commencer" â†’ Transition vers tutoriel

**3. Tutoriel Interactif (10-15 minutes)**

Voir [Tutorial.md](Tutorial.md) pour le dÃ©tail complet.

**Ã‰tapes:**
1. Introduction Ã  la grille et au mouvement
2. Explication des cartes et de la main
3. Premier combat guidÃ© (vs 2 Gobelins)
4. RÃ©compense et amÃ©lioration de deck
5. Transition vers la campagne

---

## ðŸŽ® Boucle de Jeu Principale

### Vue d'Ensemble du Flow

```
Menu Principal
    â†“
SÃ©lection Campagne/Mode
    â†“
PrÃ©paration (Deck, Ã‰quipe)
    â†“
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘   BOUCLE DE RUN   â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘ Combat            â•‘
â•‘    â†“              â•‘
â•‘ Victoire          â•‘
â•‘    â†“              â•‘
â•‘ RÃ©compenses       â•‘
â•‘    â†“              â•‘
â•‘ Ã‰vÃ©nement (25%)   â•‘
â•‘    â†“              â•‘
â•‘ Boutique (20%)    â•‘
â•‘    â†“              â•‘
â•‘ Prochain Combat   â•‘
â•‘    â†“              â•‘
â•‘ (RÃ©pÃ©ter)         â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    â†“
Boss Final
    â†“
Victoire/DÃ©faite
    â†“
Statistiques & RÃ©compenses
    â†“
Menu Principal
```

### Ã‰cran de Carte (Map)

**Fonctionnement:**
- Carte avec chemins possibles
- IcÃ´nes reprÃ©sentant les types de rencontres:
  - âš”ï¸ Combat facile
  - âš”ï¸âš”ï¸ Combat difficile
  - ðŸ‘‘ Boss
  - ðŸª Boutique
  - â“ Ã‰vÃ©nement
  - ðŸ”¥ Combat Ã‰lite
  - ðŸ’° TrÃ©sor

**UI:**
```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘                                            â•‘
â•‘      ACTE 1 - ForÃªt des Gobelins          â•‘
â•‘      Combat 3/10                           â•‘
â•‘                                            â•‘
â•‘           ðŸ‘‘ (Boss)                        â•‘
â•‘          /    \                            â•‘
â•‘        âš”ï¸      ðŸª                          â•‘
â•‘        /  \    /                           â•‘
â•‘      âš”ï¸âš”ï¸  âš”ï¸  â“                          â•‘
â•‘        \  /  \ /                           â•‘
â•‘         âš”ï¸    ðŸ”¥                           â•‘
â•‘           \  /                             â•‘
â•‘            â—  â† Vous Ãªtes ici              â•‘
â•‘                                            â•‘
â•‘  [Deck] [Personnages] [Progression]       â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**Interactions:**
- Clic sur un nÅ“ud accessible â†’ Preview du combat/Ã©vÃ©nement
- Confirmation â†’ Transition vers la rencontre

---

## âš”ï¸ Flow de Combat

### Phase 1: Chargement et Placement

**DurÃ©e:** 2-3 secondes

**SÃ©quence:**
1. Fade in de la scÃ¨ne de combat
2. Grille apparaÃ®t (animation de matÃ©rialisation)
3. Personnages se tÃ©lÃ©portent sur leurs positions
4. Ennemis apparaissent (animation d'entrÃ©e)
5. Calcul de l'initiative (barre d'initiative apparaÃ®t)
6. MÃ©lange et pioche des cartes (animation)

**UI Visible:**
- Grille de combat
- HUD des personnages (cÃ´tÃ© gauche)
- HUD des ennemis (au-dessus d'eux)
- Barre d'initiative (haut)
- Main vide (bas, en attente de pioche)

### Phase 2: DÃ©but du Tour Joueur

**SÃ©quence:**
1. Message "Ã€ VOTRE TOUR" (0.5s)
2. Restauration des ressources (PA, PM)
3. Pioche de cartes (animation 1s)
4. Effets de dÃ©but de tour (poison, rÃ©gÃ©nÃ©ration)
5. Activation des contrÃ´les

**Feedback Visuel:**
- Flash de couleur sur le portrait du personnage actif
- Son de dÃ©but de tour
- Cartes volent depuis le deck vers la main

### Phase 3: Actions du Joueur

**Flow d'Action:**

**Option A: Jouer une Carte**
```
Clic sur Carte
    â†“
Carte sÃ©lectionnÃ©e (glow, dÃ©placÃ©e Ã  gauche)
    â†“
Ciblage activÃ© (courbe + rÃ©ticule)
    â†“
Hover sur cible valide â†’ Preview des effets
    â†“
Clic sur cible â†’ Confirmation
    â†“
Animation de jeu de carte
    â†“
RÃ©solution des effets
    â†“
Carte dans la dÃ©fausse
    â†“
Retour Ã  la main
```

**Option B: Se DÃ©placer**
```
Clic sur Personnage (ou sÃ©lectionnÃ© par dÃ©faut)
    â†“
Cases de mouvement highlighted (vert)
    â†“
Clic sur case de destination
    â†“
Preview du chemin (flÃ¨ches)
    â†“
Confirmation (clic ou Enter)
    â†“
Animation de mouvement
    â†“
DÃ©duction des PM
    â†“
Fin de l'action
```

**Option C: Fin de Tour**
```
Clic sur "Fin de Tour" (ou touche Enter)
    â†“
Confirmation si PA/PM non utilisÃ©s (optionnel)
    â†“
Effets de fin de tour (trigger)
    â†“
Transition vers le tour suivant
```

### Phase 4: Tour de l'Ennemi

**SÃ©quence:**
1. Message "[NOM ENNEMI] AGIT" (0.5s)
2. Ennemi rÃ©flÃ©chit (0.5-1s, animation "thinking")
3. DÃ©cision de l'IA
4. ExÃ©cution de l'action (mouvement + attaque)
5. Effets rÃ©solus
6. Fin du tour ennemi

**Feedback Visuel:**
- Portrait de l'ennemi highlighted
- Intention affichÃ©e (icÃ´ne au-dessus: attaque, mouvement, buff)
- Animation d'action
- DÃ©gÃ¢ts/effets appliquÃ©s

**Vitesse:**
- Rapide par dÃ©faut (1-2s par tour ennemi)
- Option pour ralentir (utile pour apprentissage)

### Phase 5: Fin du Combat

**Victoire:**
```
Dernier ennemi vaincu
    â†“
Animation de victoire (0.5s)
    â†“
Message "VICTOIRE!" (1s)
    â†“
Statistiques du combat (5s)
    â†“
Ã‰cran de rÃ©compenses
```

**DÃ©faite:**
```
Tous les alliÃ©s vaincus
    â†“
Animation de dÃ©faite (0.5s)
    â†“
Message "DÃ‰FAITE" (1s)
    â†“
Statistiques du combat
    â†“
Options:
    - Recommencer le combat (-50 Or)
    - Abandonner le run (retour menu)
```

---

## ðŸŽ Ã‰cran de RÃ©compenses

### UI des RÃ©compenses

```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘          VICTOIRE !                          â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘                                              â•‘
â•‘  +150 XP     +60 Or     +10 Gemmes          â•‘
â•‘                                              â•‘
â•‘  Choisissez une carte Ã  ajouter:            â•‘
â•‘  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”     â•‘
â•‘  â”‚ [Carte] â”‚  â”‚ [Carte] â”‚  â”‚ [Carte] â”‚     â•‘
â•‘  â”‚  RARE   â”‚  â”‚ COMMUNE â”‚  â”‚  RARE   â”‚     â•‘
â•‘  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜     â•‘
â•‘                                              â•‘
â•‘           [Ignorer] [Confirmer]             â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**Flow:**
1. Affichage des rÃ©compenses passives (XP, Or)
2. Animations de compteur (nombre qui augmente)
3. Affichage des choix de cartes (rÃ©vÃ©lation progressive)
4. SÃ©lection du joueur (hover pour voir dÃ©tails)
5. Confirmation
6. Carte ajoutÃ©e au deck (animation)
7. Transition vers la carte du monde

**Raccourcis Clavier:**
- Touches 1, 2, 3 pour sÃ©lectionner les cartes
- Espace pour ignorer
- Enter pour confirmer

---

## ðŸª Boutique

### UI de la Boutique

```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘          BOUTIQUE DU VOYAGEUR                â•‘
â•‘          Or disponible: 120                  â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘                                              â•‘
â•‘  CARTES DISPONIBLES:                        â•‘
â•‘  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”     â•‘
â•‘  â”‚ [Carte] â”‚  â”‚ [Carte] â”‚  â”‚ [Carte] â”‚     â•‘
â•‘  â”‚  50 Or  â”‚  â”‚  75 Or  â”‚  â”‚ 100 Or  â”‚     â•‘
â•‘  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜     â•‘
â•‘                                              â•‘
â•‘  SERVICES:                                   â•‘
â•‘  [AmÃ©liorer une carte - 100 Or]             â•‘
â•‘  [Supprimer une carte - 50 Or]              â•‘
â•‘  [Acheter une Potion - 30 Or]               â•‘
â•‘                                              â•‘
â•‘           [Quitter la Boutique]             â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**Interactions:**
- Clic sur carte â†’ Preview dÃ©taillÃ©e
- Clic sur "Acheter" â†’ Confirmation si assez d'Or
- Or dÃ©duit immÃ©diatement
- Carte ajoutÃ©e au deck

**Feedback:**
- Animation de piÃ¨ces qui disparaissent
- Son de transaction
- Carte qui vole vers le deck

---

## ðŸŽ² Ã‰vÃ©nements AlÃ©atoires

### Types d'Ã‰vÃ©nements

**1. Choix Binaire:**
```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘          RENCONTRE MYSTÃ‰RIEUSE               â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘                                              â•‘
â•‘  Vous trouvez un autel ancien avec une      â•‘
â•‘  inscription: "Sacrifice pour pouvoir".     â•‘
â•‘                                              â•‘
â•‘  Que faites-vous?                           â•‘
â•‘                                              â•‘
â•‘  [Sacrifier 20 HP]                          â•‘
â•‘  â†’ Gagnez une carte Ã‰pique                  â•‘
â•‘                                              â•‘
â•‘  [Ignorer l'autel]                          â•‘
â•‘  â†’ Rien ne se passe                         â•‘
â•‘                                              â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**2. Ã‰vÃ©nement de Combat:**
- Mini-boss surprise
- RÃ©compenses accrues

**3. Ã‰vÃ©nement de Ressources:**
- TrÃ©sor
- Perte/Gain d'Or

**Flow:**
1. Transition vers l'Ã©cran d'Ã©vÃ©nement
2. Lecture de la description (5-10s)
3. PrÃ©sentation des choix
4. SÃ©lection du joueur
5. RÃ©solution immÃ©diate
6. Transition vers la suite

---

## ðŸ“Š Ã‰cran de Deck

### UI de Consultation du Deck

```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘          DECK DE ILYA (25 cartes)           â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘                                              â•‘
â•‘  [Toutes] [Attaque] [DÃ©fense] [Util]        â•‘
â•‘                                              â•‘
â•‘  â”Œâ”€â”€â”€â” â”Œâ”€â”€â”€â” â”Œâ”€â”€â”€â” â”Œâ”€â”€â”€â” â”Œâ”€â”€â”€â”            â•‘
â•‘  â”‚ 5Ã—â”‚ â”‚ 3Ã—â”‚ â”‚ 2Ã—â”‚ â”‚ 2Ã—â”‚ â”‚ 1Ã—â”‚            â•‘
â•‘  â””â”€â”€â”€â”˜ â””â”€â”€â”€â”˜ â””â”€â”€â”€â”˜ â””â”€â”€â”€â”˜ â””â”€â”€â”€â”˜            â•‘
â•‘                                              â•‘
â•‘  [Trier: CoÃ»t] [Trier: Nom] [Trier: Type]  â•‘
â•‘                                              â•‘
â•‘           [Retour]                           â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**FonctionnalitÃ©s:**
- Filtrage par type
- Tri par diffÃ©rents critÃ¨res
- Affichage du nombre de copies
- Clic sur carte â†’ DÃ©tails complets
- Accessible depuis la carte du monde

---

## âš™ï¸ Options et ParamÃ¨tres

### Menu Options

```
â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—
â•‘               OPTIONS                        â•‘
â• â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•£
â•‘                                              â•‘
â•‘  AUDIO:                                      â•‘
â•‘  Musique:     â–“â–“â–“â–“â–“â–“â–“â–‘â–‘â–‘ 70%                â•‘
â•‘  Effets:      â–“â–“â–“â–“â–“â–“â–“â–“â–“â–‘ 90%                â•‘
â•‘  Ambiance:    â–“â–“â–“â–“â–“â–‘â–‘â–‘â–‘â–‘ 50%                â•‘
â•‘                                              â•‘
â•‘  GRAPHIQUES:                                 â•‘
â•‘  RÃ©solution:  [1920Ã—1080 â–¼]                 â•‘
â•‘  Plein Ã©cran: [âœ“]                           â•‘
â•‘  VSync:       [âœ“]                           â•‘
â•‘  QualitÃ©:     [Ã‰levÃ©e â–¼]                    â•‘
â•‘                                              â•‘
â•‘  GAMEPLAY:                                   â•‘
â•‘  Vitesse IA:  [Normale â–¼]                   â•‘
â•‘  Confirmations: [âœ“]                         â•‘
â•‘  Tutoriels:   [âœ“]                           â•‘
â•‘                                              â•‘
â•‘  ACCESSIBILITÃ‰:                              â•‘
â•‘  Taille texte: [Normal â–¼]                   â•‘
â•‘  Daltonisme:  [Aucun â–¼]                     â•‘
â•‘                                              â•‘
â•‘      [Appliquer]  [Retour]                  â•‘
â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
```

**ParamÃ¨tres SauvegardÃ©s:**
- Automatiquement dans PlayerPrefs
- Application immÃ©diate pour la plupart
- Confirmation pour changements majeurs (rÃ©solution)

---

## ðŸ”„ Transitions et Chargements

### Types de Transitions

**1. Fade In/Out (Standard):**
- DurÃ©e: 0.3-0.5s
- UtilisÃ© pour: Menus â†’ Jeu, Combats â†’ Carte

**2. Wipe (Balayage):**
- DurÃ©e: 0.5s
- Direction: Gauche â†’ Droite
- UtilisÃ© pour: Changement d'acte

**3. Zoom In (Carte â†’ Combat):**
- DurÃ©e: 0.8s
- Zoom sur le nÅ“ud sÃ©lectionnÃ©
- Fondu vers la scÃ¨ne de combat

**4. Instant (RÃ©compenses â†’ Carte):**
- Pas de transition
- Changement immÃ©diat
- Moins de friction

### Ã‰crans de Chargement

**Court (<2s):**
- Barre de progression simple
- Pas de texte, juste l'icÃ´ne du jeu

**Moyen (2-5s):**
- Barre de progression
- Tips de gameplay alÃ©atoires
- Illustration d'arriÃ¨re-plan

**Long (>5s, rare):**
- Barre de progression
- Tips de gameplay
- Mini-jeu optionnel (ex: cliquer pour bonus mineur)

---

## ðŸŽ¯ Feedback et Satisfaction

### Moments de Satisfaction (Juicy Moments)

**1. Ã‰limination d'Ennemi:**
- Animation de mort spectaculaire
- Particules d'explosion
- Son impactant
- Texte "Ã‰LIMINÃ‰!" qui pop
- Shake screen lÃ©ger

**2. Combo de Cartes:**
- EnchaÃ®nement rapide (3+ cartes)
- Multiplicateur de dÃ©gÃ¢ts affichÃ©
- Effet visuel spÃ©cial (lightning entre les cartes)
- Son de combo crescendo

**3. Coup Critique:**
- Freeze frame (0.1s)
- Flash lumineux
- Son mÃ©tallique
- Texte "CRITIQUE!" en gros

**4. Victoire de Boss:**
- Slow motion de l'attaque finale
- Explosion massive
- Ã‰cran blanc flash
- Fanfare musicale
- DÃ©compte de rÃ©compenses thÃ©Ã¢tral

### PrÃ©vention de la Frustration

**1. Undo (Annulation):**
- PossibilitÃ© d'annuler le dernier mouvement (avant confirmation)
- CoÃ»t: Aucun
- Limite: 1 annulation par tour

**2. Preview OmniprÃ©sent:**
- Toujours afficher l'effet avant confirmation
- Preview de dÃ©gÃ¢ts, portÃ©e, zone d'effet
- Pas de surprise nÃ©gative

**3. Confirmations Optionnelles:**
- DÃ©sactivables dans les options
- ActivÃ©es par dÃ©faut pour nouveaux joueurs
- Exemples:
  - "Terminer le tour avec des PA inutilisÃ©s?"
  - "Ignorer cette rÃ©compense?"

**4. Sauvegarde Automatique:**
- Avant chaque combat
- AprÃ¨s chaque rÃ©compense
- Jamais de perte de progression

---

## ðŸ“± Raccourcis Clavier

### Combats

- **Espace** : Fin de tour
- **EntrÃ©e** : Confirmer l'action
- **Ã‰chap** : Annuler la sÃ©lection
- **1-9** : SÃ©lectionner carte dans la main
- **Tab** : Cycler entre les ennemis
- **Z** : Annuler le dernier mouvement

### Navigation

- **Ã‰chap** : Menu pause / Retour
- **M** : Carte du monde
- **D** : Deck
- **C** : Personnages
- **O** : Options

---

**DerniÃ¨re mise Ã  jour:** 11 Janvier 2026
**Responsable:** Design UX Project TDB
