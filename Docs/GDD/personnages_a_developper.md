# PERSONNAGES À DÉVELOPPER - ÉMOTIONS TACTICS

**Mis à jour :** 23 Septembre 2026 (notes de statut uniquement — contenu créatif original inchangé)
**Statut :** 2 personnages développés (Ilya, Jumeaux), 7 favoris en attente. Roster MVP : Soren, l'Alpiniste, Ace (voir l'Excel `TCG_Tactique_Systeme_de_calcul.xlsx` et `CHAMPIONS_CONCEPTS.md`).

> ℹ️ **Note de statut :** ce document date d'avant plusieurs décisions structurelles (voir `GDD_Main.md`). Quatre choses à garder en tête en le lisant :
> 1. **Les tags « Famille » utilisent l'ancien système à 3 familles** (🔴 Rouge/Incarnat, 🔵 Bleu/Sérénite, 🟡 Jaune/Exalté). Le système visé est à 8 familles façon Plutchik (voir `SYSTEME_EMOTIONS.md`). Personne n'a encore fait la correspondance, donc ces tags sont une **inspiration thématique approximative**.
> 2. **Les tags « Archétype » (Tisseur, Ombrelame, Harmoniste...) renvoient à l'ancien système de classes, abandonné le 10/09/2026.** Ils restent utiles comme description de rôle, pas comme mécanique à implémenter.
> 3. **Le roster MVP (Soren, l'Alpiniste, Ace)** est documenté dans l'Excel et `CHAMPIONS_CONCEPTS.md`. Ace reprend l'esprit de #12 KAIROS (joueur, destin) — attention au doublon.
> 4. Tout nouveau champion doit suivre le format actuel : trauma + passif + 2 cartes Signature + profil PA/PM (budget 9), deck de 24 cartes.
>
> Rien n'a été supprimé ci-dessous : c'est une matière première créative précieuse (100 concepts), elle reste intacte.

---

## ✅ PERSONNAGES DÉVELOPPÉS (2/∞)

### 1. ILYA - « Le Dévoué Enchaîné »
- **Famille :** 🔴 Rouge (Incarnat) - Colère ↔ Amour *(= Déchaînés dans le système à 8 familles)*
- **Archétype :** Ancre (Tank)
- **Mécanique :** Système Rage (dégâts → cartes Rage → Transformation)
- **Formes :** Enchaîné (Défensif) ↔ Déchaîné (Offensif Berserker)
- **Deck :** 12 cartes
- **Statut :** ✅ concept complet — **hors MVP**, à adapter au format actuel
- **Fichier :** `ilya_deck_simple.md`

### 2. ASTRA & NOCTIS - « Les Jumeaux de la Dualité » *(hors MVP)*
- **Famille :** ⚪⚫ Dualité (Hors-famille) - Toutes émotions Positif/Négatif
- **Archétype :** Ancre (Astra) + Veilleur (Noctis)
- **Mécanique :** 2 personnages, 1 esprit (PA partagés), Résonance Distance
- **Formes :** Séparés (Versatile) ↔ Éclipse (Fusion Ultra-puissante)
- **Deck :** 18 cartes (6 Astra + 6 Noctis + 6 Jumeaux)
- **Statut :** Concept posé, stats, passifs, distance et Éclipse **à définir** (voir `astra_noctis_simple.md`). Second concept (« Version 2 ») dans `CHAMPIONS_CONCEPTS.md`. Thème proche de Soren (MVP).

---

## ⭐ FAVORIS PRIORITAIRES (7 concepts)

### #9 : MIRA - « L'Artiste des Émotions »
**Concept :** Deck se transforme dynamiquement selon émotions ennemies détectées

**Mécanique Signature :**
- Détecte émotion dominante ennemis (Peur/Colère/Tristesse/Joie/Confusion)
- Deck de 12 cartes « neutres » se transforme en cartes de cette émotion
- Change en temps réel pendant combat
- Exemple : Boss Peur → 12 cartes deviennent anti-Peur

**Famille :** 🔵 Bleu (Sérénite) - Tristesse ↔ Calme
**Archétype :** Tisseur (Mage/Contrôle)
**Complexité :** ⭐⭐⭐ (Moyenne)
**Vibe :** Empathie extrême, peinture émotionnelle, adaptation pure

**Notes :**
- Très versatile (contrer n'importe quel ennemi)
- Skill expression : Reconnaître patterns ennemis
- Potentiel narratif : Artiste qui « peint » les émotions

---

### #12 : KAIROS - « Le Marchand du Destin »
**Concept :** Gambling extrême, parie ressources (PV/PA/Cartes) pour effets aléatoires puissants

**Mécanique Signature :**
- Cartes = Paris (ex : « Parie 20 PV : 50% chance 80 dmg, 50% chance heal ennemi 30 PV »)
- Plus le pari est risqué, plus le reward potentiel est énorme
- Peut tout perdre... ou tout gagner
- Jauge « Chance » : Augmente avec paris réussis, diminue avec échecs

**Famille :** 🟡 Jaune (Exalté) - Anxiété ↔ Optimisme
**Archétype :** Ombrelame (Voleur/Burst)
**Complexité :** ⭐⭐⭐ (Moyenne, mais RNG frustrant)
**Vibe :** Casino cosmique, adrénaline, addiction, destin manipulé

**Notes :**
- High risk/high reward ultime
- Peut carry game ou throw instantanément
- Narratif : Joueur compulsif cherchant rédemption

---

### #13 : NEXUS - « La Ruche Mentale »
**Concept :** 5 corps identiques sur plateau, 1 pool PV partagé, meurent 1 par 1, le dernier = dieu

**Mécanique Signature :**
- Spawn 5 unités Nexus (chacune 24 PV = 120 total partagé)
- Pool PV unique : Si 1 prend 30 dmg → Pool -30 (n'importe lequel peut « mourir »)
- Quand pool atteint seuils : 96/72/48/24 PV → 1 corps disparaît
- Corps restants gagnent stats du disparu (concentration pouvoir)
- Dernier Nexus (24 PV) = ×5 stats (dieu)

**Famille :** 🔵 Bleu (Sérénite) - Tristesse ↔ Calme (perte progressive)
**Archétype :** Tisseur (Mage/Swarm)
**Complexité :** ⭐⭐⭐⭐ (Élevée - gérer 5 unités)
**Vibe :** Conscience collective, sacrifice fraternel, essence concentrée

**Notes :**
- Unique : Essaim dégressif (inverse du swarm classique)
- Trade-off : Versatilité (5 corps) vs Power (1 dieu)
- Narratif : 5 frères/sœurs fusionnés mentalement

---

### #16 : MASQ - « Le Caméléon Émotionnel »
**Concept :** 6 masques émotionnels = 6 personnages différents, switch dynamique en combat

**Mécanique Signature :**
- 6 Masques : Joie/Colère/Tristesse/Peur/Dégoût/Sérénité
- Chaque masque = Stats différentes + Deck différent (6 cartes/masque)
- Peut changer masque (2 PA + défausse 1 carte)
- Total : 6 cartes neutres (toujours) + 6 cartes masque actif = 12 accessibles

**Famille :** ⚪⚫🔴🔵🟡 Multi/Hors-famille (transcende émotions)
**Archétype :** Caméléon (tous archétypes selon masque)
**Complexité :** ⭐⭐⭐⭐ (Élevée - 6 formes à maîtriser)
**Vibe :** Trouble dissociatif, acteur théâtral, 6 émotions incarnées

**Notes :**
- Versatilité maximale (6 rôles en 1)
- Deck 18 cartes total (le plus gros)
- Narratif : Acteur qui a perdu son identité

---

### #36 : BINAIRE - « Le Codeur Quantique »
**Concept :** Seulement 2 cartes (« 0 » et « 1 »), joue séquences binaires pour créer sorts

**Mécanique Signature :**
- Deck : 6× carte « 0 » + 6× carte « 1 » = 12 cartes total
- Joue séquences (ex : 1-0-1 = Attaque, 1-1-0 = Heal, 1-1-1 = Ultimate)
- Différentes séquences = différents effets
- Plus la séquence est longue, plus l'effet est puissant
- Doit mémoriser « code » de chaque sort

**Famille :** 🟡 Jaune (Exalté) - Anxiété ↔ Optimisme (logique pure)
**Archétype :** Tisseur (Mage programmeur)
**Complexité :** ⭐⭐⭐⭐⭐ (Très difficile - mémorisation patterns)
**Vibe :** Hacker, Matrix, minimalisme extrême, puzzles mathématiques

**Notes :**
- Skill ceiling infini (combos patterns)
- Unique : Codage en combat temps réel
- Narratif : IA devenue consciente, parle en binaire

---

### #50 : ASCENSION - « L'Immortel Progressif »
**Concept :** Gagne niveaux permanents en jouant cartes, perd niveaux en prenant dégâts (montagnes russes)

**Mécanique Signature :**
- Jauge « Niveau » (0-10, start à 5)
- Jouer 5 cartes → +1 Niveau (+10% stats)
- Subir 20 dmg → -1 Niveau (-10% stats)
- Niveau 10 = Dieu (×2 stats), Niveau 0 = Déchu (×0.2 stats)
- Montagnes russes permanentes de puissance

**Famille :** 🟡 Jaune (Exalté) - Anxiété ↔ Optimisme (montée/chute)
**Archétype :** Ombrelame (Burst scaling)
**Complexité :** ⭐⭐⭐ (Moyenne)
**Vibe :** Ascension divine, hubris, gloire éphémère, chute tragique

**Notes :**
- Gameplay yo-yo (jamais stable)
- Risk/reward : Spam cartes (niveaux) mais expose (dmg)
- Narratif : Mortel cherchant transcendance

---

### #52 : CHEF D'ORCHESTRE - « Le Maestro Tactique »
**Concept :** Ne combat pas directement, dirige alliés avec ordres boostés, inutile solo/dieu en team

**Mécanique Signature :**
- 0 cartes d'attaque personnelles
- 12 cartes « Ordres » (ex : « Attaque! », « Défends! », « Bouge! »)
- Ordres ciblent alliés et boostent leurs actions (+50% ATK, +2 Mouv, etc.)
- Peut donner 2-3 ordres/tour (distribue PA entre alliés)
- Solo = inutile, Team = surpuissant

**Famille :** 🔵 Bleu (Sérénite) - Tristesse ↔ Calme (coordination)
**Archétype :** Harmoniste (Support pur)
**Complexité :** ⭐⭐⭐⭐ (Élevée en coop)
**Vibe :** Stratège, commandant, symphonie tactique, RTS vivant

**Notes :**
- 100% support (0 dégâts perso)
- Nécessite team (PvE coop ou multi)
- Narratif : Chef d'orchestre réincarné en tacticien

---

## 🗂️ CONCEPTS EN RÉSERVE (28+ idées)

### Catégorie : Transformations & Formes Multiples

**#17 : REFLET** - Copie dernière carte jouée (n'importe qui) et rejoue gratuit

**#19 : SYMBIOTE** - Fusionne avec allié OU ennemi (contrôle/boost)

**#23 : ARCHIVE** - Rejoue N'IMPORTE quelle carte déjà jouée ce combat (historique)

**#25 : AURORE/CRÉPUSCULE** - Cycle Jour/Nuit automatique (2 formes alternées)

**#29 : QUANTIQUE** - Superposition : Existe à 2 positions simultanément

**#38 : ÉCHO INVERSÉ** - Actions se produisent 2 tours APRÈS (planification futur)

**#39 : FISSION** - Se divise en 2 copies (50% PV), peut re-diviser (4, 8 copies)

**#43 : MUTATION** - Cartes mutent aléatoirement après chaque utilisation

**#44 : ECLIPSE** - 2 decks (Lumière/Ombre), alterne automatiquement chaque tour

**#46 : PHÉNIX** - Meurt après chaque carte, ressuscite avec stats aléatoires

**#69 : AMALGAME** - Fusionne avec ennemis morts (gagne capacités/stats)

---

### Catégorie : Systèmes de Ressources Uniques

**#18 : MÉTRONOME** - Cartes chargent (1-5 tours), plus attends = plus puissant

**#20 : MARIONNETTISTE** - Invoque 3 marionnettes, distribue PA entre elles

**#30 : SACRIFICE** - Cartes coûtent 0 PA mais nécessitent sacrifices (PV, cartes, stats)

**#34 : PARASITE** - Vole PA ennemis (infecte → -1 PA ennemi, +1 PA soi)

**#37 : DETTE** - Emprunte ressources du futur (6 PA maintenant, 0 PA dans 2 tours)

**#40 : KARMA** - Actions ont conséquence opposée (attaque = subir dmg aussi, mais ×2 effet)

**#47 : COLLECTION** - Deck grandit (ennemis tués ajoutent leurs cartes définitivement)

**#66 : LEGACY** - Actions d'un combat affectent combat suivant (PV, deck, buffs persistent)

**#71 : ZÉRO** - Toutes stats 0, immortel, gagne stats en absorbant actions ennemies

---

### Catégorie : Contrôle & Support

**#27 : PACIFISTE** - Ne peut PAS attaquer, gagne en faisant ennemis s'entretuer

**#35 : CATALYSEUR** - Cartes modifient prochaine carte jouée (multiplicateurs, pas effet direct)

**#51 : FANTÔME** - Déjà mort (0 PV), gagne Essence en drainant (plus Essence = plus tangible)

**#53 : CONTRAT** - Passe contrats avec ennemis (deals diplomatiques)

**#63 : NÉMÉSIS** - Crée « counters parfaits » pour chaque ennemi (feu → eau)

**#82 : CENSURE** - Supprime concepts du combat (« Feu » censuré → Plus aucune carte Feu existe)

---

### Catégorie : Temps & Probabilités

**#21 : ENTROPIE** - Effets cartes aléatoires CHAQUE fois piochées (chaos pur)

**#22 : MIROIR** - PV toujours = PV ennemi ciblé (symbiose forcée)

**#33 : ORACLE** - Voit 5 tours futurs, peut « rewind » 1 tour (undo)

**#64 : HORIZON** - Voit avenir (5 tours) mais NE PEUT PAS changer passé

**#67 : OUROBOROS** - Si meurt, combat restart (garde buffs/debuffs actuels, loop)

**#68 : LIMINAL** - Agit ENTRE tours (interrompt tour ennemi, 1 PA seulement)

**#72 : SCHRÖDINGER** - Superposition quantique (effets A ET B simultanés jusqu'à « observation »)

---

### Catégorie : Concepts Expérimentaux

**#24 : VIDE** - Commence 0 carte, CRÉE cartes custom en combat (menu création)

**#26 : FRACTALE** - Cartes se dupliquent (1→2→4→8), chaque copie plus faible (50%)

**#28 : ARTEFACT** - Pas de cartes, équipe 5 Artefacts (items passifs), combine synergies

**#31 : HORLOGE** - 12 cartes numérotées 1-12, DOIT jouer en ordre (1→2→3...→12)

**#32 : RUCHE** - Invoque Drones (5 PV), chaque Drone boost les autres (+5% stats/Drone)

**#36 : BINAIRE** - [FAVORI] Codage 0/1

**#41 : SCRIPT** - Programme 10 actions AVANT combat, exécution auto (0 contrôle combat)

**#42 : MÉMENTO** - Perd mémoire chaque tour, laisse notes à soi-même

**#48 : SINGULARITÉ** - 12 cartes fusionnent en 1 SEULE (effet combiné massif)

**#49 : DÉMIURGE** - Crée hexagones, ennemis, alliés (façonne battlefield)

**#50 : ASCENSION** - [FAVORI] Niveaux gain/loss

**#54 : VESTIGE** - Main ne se défausse JAMAIS (7→15→30 cartes, ingérable)

**#55 : PRISME** - Chaque carte a 3 versions (Rouge/Bleu/Jaune), choix à la volée

**#56 : RÉCURSION** - Joue carte sur elle-même (Attaque→Double Attaque→Quad→Octo, infini)

**#57 : ÉQUILIBRE** - Stats s'équilibrent auto (ATK trop haut → perd ATK, gagne DEF)

**#58 : MALÉDICTION** - Chaque carte jouée applique malédiction permanente (à soi ET ennemis)

**#59 : PRÉDATEUR/PROIE** - 2 formes switch auto selon PV relatifs (PV>ennemi = Prédateur)

**#60 : NŒUD** - Cartes liées par chaînes (jouer A active B gratuit, combos massifs)

**#61 : RÉSONANCE** - Cartes identiques résonnent (jouer 2× même carte = ×3 effet, pas ×2)

**#62 : GÉNÉRATION** - Clones créent clones (Gen 1→Gen 2→Gen 3, chaque gen plus faible/nombreuse)

**#65 : VIRUS** - Infecte ennemis, code viral se propage, corrompt progressivement

**#70 : ARCHIVE COSMIQUE** - Deck = TOUTES cartes du jeu (500+), pioche aléatoire

**#73 : TESSELLATION** - Cartes créent patterns géométriques (figures complètes = effet massif)

**#74 : MOMENTUM** - Chaque action augmente vélocité (+10% speed), s'arrêter = reset

**#75 : DÉCHIRURE** - Sépare battlefield en 2 dimensions parallèles (A et B)

**#76 : ÉPITAPHE** - Écrit prédictions mort ennemis (« Tu mourras par feu Turn 5 »), récompense si vrai

**#77 : INERTIE** - Plus immobile, plus lourd/résistant (+10 DEF/tour, -1 Mouv)

**#78 : SYNAPSE** - Deck = réseau neural, cartes renforcent connexions (IA auto-apprenante)

**#79 : EXTINCTION** - Types ennemis tués = éteints (ne respawn plus jamais)

**#80 : LEVIER** - Actions effet ×10 mais délai 2 tours (investissement long terme)

**#81 : MARÉE** - Stats fluctuent (Tour impair = Marée Haute ×2 ATK, Pair = Marée Basse ×2 DEF)

**#83 : APOGÉE** - Jauge Gloire (0-100), actions héroïques +Gloire, 100 = Légendaire

**#84 : SYMÉTRIE** - Battlefield doit rester symétrique (si ennemi meurt gauche, allié meurt droite)

**#85 : COMBUSTION** - Brûle cartes pour PA (+2 PA/carte, deck diminue)

**#86 : CHRYSALIDE** - Métamorphose forcée (Larve→Cocon→Papillon, 3 phases)

**#87 : ANCRAGE** - Choisit 1 hex « ancre », stats selon distance (proche = fort, loin = faible)

**#88 : PARTITION** - Deck = partition musicale, joue en séquence rythmique (mesures)

**#89 : SÉQUENCE** - Cartes Fibonacci (1,1,2,3,5,8,13...), ordre mathématique obligatoire

**#90 : PARASITE TEMPOREL** - Vit « 1 tour en retard » (voit futur, agit dans passé)

**#91 : CULTE** - Convertit ennemis au lieu de tuer (army building)

**#92 : NEXUS DIMENSIONNEL** - Ouvre portails 3 dimensions (Feu/Glace/Foudre), invoque entités

**#93 : ÉDITEUR** - Édite cartes ennemies temps réel (« Fireball 50 » → « Fireball 5 »)

**#94 : FRACTIONNEMENT** - Divise stats en fractions (100 PV → 50 PV + 50 Shield)

**#95 : RÉMANENCE** - Laisse images résiduelles en bougeant (fantômes attaquent 10 dmg)

**#96 : CONSTELLATION** - Ennemis tués = étoiles, connecte constellations (buffs permanents)

**#97 : ÉROSION** - Chaque attaque -1% stats ennemi permanent (stack, 100 attaques = 0 stats)

**#98 : SINGULARITÉ GRAVITATIONNELLE** - Trou noir attire tout vers centre (annihilation)

**#99 : LÉGENDE VIVANTE** - Chaque combat gagné +5% stats permanent (scaling infini cross-combat)

**#100 : ∞ (INFINI)** - Pas de limite ressources, mais Entropie augmente (100 Entropie = mort)

---

## 📝 NOTES DE DESIGN

### Priorités Développement
1. **Diversité Gameplay :** Chaque personnage doit avoir mécanique signature unique
2. **Équilibrage Famille :** Couvrir progressivement les 8 familles émotionnelles (voir `SYSTEME_EMOTIONS.md`)
3. **Synergies Coop :** Penser interactions entre personnages
4. **Courbe Apprentissage :** Alterner complexité (Simple → Difficile → Moyen)
5. **Narratif :** Chaque personnage = histoire émotionnelle forte

### Ordre Suggéré Développement (après le roster MVP : Soren, l'Alpiniste, Ace)
1. **MIRA** (Moyen, Bleu Tisseur) - Complète archétype Mage
2. **KAIROS** (Moyen, Jaune Ombrelame) - Gameplay RNG fun *(proche d'Ace — à différencier ou fusionner)*
3. **NEXUS** (Difficile, Bleu Tisseur alt) - Concept swarm unique
4. **MASQ** (Difficile, Multi) - Versatilité maximale
5. **BINAIRE** (Très difficile, Jaune Tisseur alt) - Pour experts
6. **ASCENSION** (Moyen, Jaune Ombrelame alt) - Scaling dynamique
7. **CHEF D'ORCHESTRE** (Difficile, Bleu Harmoniste) - Pour coop/multi

### Questions Récurrentes à Poser
- Famille émotionnelle (dans le système à 8 familles) ?
- Mécanique signature en 1 phrase ?
- Complexité cible (débutant/moyen/expert) ?
- Deck size (12/15/18 cartes) ?
- Transformation/Formes (Oui/Non, combien) ?
- Synergies avec les champions du MVP ?

---

## 🎯 IDÉES FUTURES

### Concepts à Creuser
- **Personnage Temporel :** Voyage dans le temps du combat (rejoue tours)
- **Personnage Miroir :** Copie ennemis (devient ce qu'il combat)
- **Personnage Void :** N'existe pas vraiment (intangible, abstrait)
- **Personnage Collectif :** Multitude (100 mini-unités 1 PV chacune)
- **Personnage Parasite :** Vit dans autre personnage (symbiose forcée)

### Mécaniques à Explorer
- **Deck Évolutif Cross-Combat :** Cartes upgradent définitivement
- **Émotions Mixtes :** Personnages multi-familles (Rouge+Bleu = Violet)
- **Anti-Archétypes :** Inversions (Tank qui fuit, Support qui tue)
- **Cartes Vivantes :** Cartes ont PV/Stats propres (mini-unités)

---

**FIN DU FICHIER**
