# 📈 Système de Progression - Émotions Tactics (Project TDB)

**Version:** 2.0
**Date:** 23 Septembre 2026
**Changements :**
- v1.2 (23/09/2026) : structure campagne façon Waven (progression persistante, pas de roguelike).
- v2.0 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`, onglets « Progression champions » et « Barème monstres ») — niveau max 20, PV +15/niveau, PA/PM fixés par profil, XP = 100 × niveau, XP monstre = 15 % de ses PV. L'ancienne table de niveaux d'Ilya et l'arbre de talents sont archivés (ils augmentaient les PA, contraire à la règle d'or).

> **Chiffres de référence : l'Excel.** Ce document explique les règles.
> Rappel : pas de gacha (10/09/2026). Les Gemmes ne sont pas un tirage aléatoire mais restent à revalider avec le modèle de monétisation.

---

## 🎯 Vue d'Ensemble

La progression suit une **campagne façon Waven** : donjons fixes enchaînés, progression **persistante** (pas de remise à zéro).

---

## 📊 Progression des Champions

### Règle d'or

Le niveau d'un champion ne fait progresser **que** :
- les **PV**
- les **passifs**
- les **slots de cartes** débloqués

Il n'augmente **jamais** la puissance des cartes ni les **PA/PM par tour** : ceux-ci sont fixés une fois pour toutes par le **profil du personnage** (budget PA + PM = 9, min 3 PA, min 2 PM). Un champion niveau 1 a exactement le même total PA+PM qu'au niveau 20, seulement réparti selon son gameplay.

### Profils PA/PM (exemples)

| Profil | PA | PM |
|--------|----|----|
| Brutal | 6 | 3 |
| Équilibré | 5 | 4 |
| Mobile | 4 | 5 |

Profils de Soren, l'Alpiniste et Ace : à renseigner dans l'Excel.

### PV et niveaux

- **Niveau maximum : 20**
- **PV** : 100 au niveau 1, **+15 par niveau** (soit 385 au niveau 20)
- **XP pour passer au niveau suivant** : 100 × niveau actuel
- **Slots de cartes débloqués** : par niveau (table à remplir dans l'Excel)

### Sources d'XP

- **XP d'un monstre = 15 % de ses PV** (barème complet par niveau dans l'onglet « Barème monstres » de l'Excel)
- Bonus éventuels (objectifs secondaires, combat parfait) : à redéfinir sur cette base

### Passifs

Chaque champion a un passif (voir `CHAMPIONS_CONCEPTS.md`). Comment le niveau fait évoluer les passifs : **à définir**.

---

## 🃏 Progression du Deck

### Acquisition de Cartes

**Pendant la Campagne :**

1. **Récompense de Combat :**
   - Après chaque combat gagné
   - Choix parmi 3 cartes aléatoires
   - Rareté basée sur la difficulté du combat (les raretés ne sont pas encore définies dans l'Excel — à confirmer) :
     - Combat facile : 70 % Commune, 25 % Rare, 5 % Épique
     - Combat moyen : 50 % Commune, 35 % Rare, 15 % Épique
     - Combat difficile : 30 % Commune, 40 % Rare, 25 % Épique, 5 % Légendaire

2. **Boutique :**
   - Accessible entre deux donjons (écran de préparation), et à mi-parcours des donjons longs
   - 6 cartes disponibles à l'achat
   - Prix : 50-200 Or selon la rareté

3. **Événements scénarisés :**
   - Moments narratifs placés dans les donjons (pas de tirage aléatoire de nœuds)
   - Parfois avec choix moraux
   - Récompenses uniques

4. **Boss :**
   - Cartes Rares/Épiques garanties
   - Boss finaux donnent des Légendaires

### Amélioration de Cartes

**Upgrade (+) :**
- Coût : 100 Or
- Disponible à la boutique ou après certains combats
- Une carte peut être améliorée une seule fois
- Effets de l'upgrade :
  - +30 % dégâts
  - OU -1 coût en PA
  - OU effet additionnel
- ⚠️ À réconcilier avec le budget de puissance de l'Excel : une carte améliorée sort de son budget (question ouverte)

**Exemples d'Upgrades** (cartes Standard de l'Excel, valeurs de base à lire dans l'Excel) :
```
Coup de colère (1 PA)  → Coup de colère+ (1 PA, +30 % dégâts)
Souffle apaisant (1 PA) → Souffle apaisant+ (1 PA, +30 % soin)
```

### Suppression de Cartes

**Système :**
- Coût : 50 Or
- Disponible à la boutique
- Permet d'affiner le deck
- Retire définitivement la carte

**Stratégie :**
- Supprimer les cartes de départ faibles au fil de la campagne
- Affiner le deck autour de son émotion (mono ou bi-émotion)
- Respecter le format 24 cartes (2 Signature + 6 Éveil + 16 Standard)

### Transformation de Cartes

**Système :**
- Rare, événements spéciaux uniquement
- Change complètement la carte en une autre
- Peut changer de rareté

**Exemples (noms génériques) :**
```
Frappe Rapide → Lame Tourbillonnante (rare)
Bouclier → Contre-Attaque (rare)
```

---

## 💰 Système Économique

### Monnaies

**Or :**
- Monnaie principale
- Obtenu après chaque combat (10-50 Or)
- Utilisé pour :
  - Acheter des cartes
  - Améliorer des cartes
  - Supprimer des cartes
  - Acheter des consommables

**Gemmes (monnaie rare) :**
- Obtenues :
  - Récompense de boss (10-30 Gemmes)
  - Succès (5-50 Gemmes)
  - Défis quotidiens (10 Gemmes)
- Utilisées pour :
  - Débloquer des personnages (500 Gemmes)
  - Acheter des cartes Légendaires (200 Gemmes)
  - Cosmétiques (100-500 Gemmes)

> À revalider avec le modèle de monétisation (question ouverte).

### Récompenses par Combat

| Combat | XP | Or | Cartes | Gemmes |
|--------|----|----|--------|--------|
| Monstres d'aventure (solo) | 15 % des PV des monstres | 20-30 | 1 (choix parmi 3) | — |
| Groupe de donjon | 15 % des PV des monstres | 40-100 | 1-2 (choix parmi 3) | Chance 5-10 |
| Boss de donjon | 15 % des PV du boss | 200-500 | 1 Rare/Épique/Légendaire garantie | 20-50 |

---

## 🎁 Système de Récompenses

### Types de Récompenses

**1. Cartes :**
- Choix parmi 3 options
- Rareté variable
- Peut refuser (skip)

**2. Or :**
- Montant fixe selon le combat
- Bonus selon la performance

**3. Équipement :**
- **À trancher** (Roadmap de l'Excel) : existe-t-il ? Si oui, impact sur quoi (stats, PA/PM, cartes) ?
- S'il existe, il ne doit pas contourner la règle d'or (pas de PA/PM ni de puissance de carte en plus)

**4. Potions (consommables) :**
- Utilisables pendant le combat
- Exemples :
  - **Potion de Vie** : restaure 50 HP
  - **Potion de Force** : +50 % dégâts pour ce combat
  - **Potion de Vitesse** : +2 PA pour ce tour *(à confirmer : sort du budget PA+PM)*

**5. Gemmes :**
- Monnaie rare
- Récompense rare

### Bonus de Performance

**Multiplicateurs d'XP et Or :**
- **Combat Parfait** (aucun dégât reçu) : ×1.5
- **Victoire Rapide** (moins de 5 tours) : ×1.2
- **Aucune Carte Jouée** (défi) : ×2.0
- **Aucun Allié Vaincu** : ×1.3

---

## 🏆 Méta-Progression

### Déblocage de Personnages

**Personnages de Base (MVP) :**
- Soren, l'Alpiniste et Ace : jouables dès le départ

> Ilya et les Jumeaux : hors MVP, candidats au déblocage plus tard.

**Personnages Débloquables (placeholders, à réviser) :**
- **[Personnage 4] :** Terminer l'Acte 1 OU 500 Gemmes
- **[Personnage 5] :** Terminer l'Acte 2 OU 500 Gemmes
- **[Personnage 6] :** Terminer l'Acte 3 OU 500 Gemmes
- **[Personnage 7] :** Terminer le Mode Difficile OU 1000 Gemmes

### Collection de Cartes

**Catalogue :**
- Toutes les cartes obtenues sont sauvegardées dans un catalogue
- Peut consulter les cartes à tout moment
- Statistiques d'utilisation (nombre de fois jouée, dégâts infligés, etc.)

**Objectif :**
- Collectionner toutes les cartes du jeu
- Récompense : Carte Légendaire unique « Collection Complète »

### Succès (Achievements)

**Exemples :**
- **Premier Sang** : Vaincre votre premier ennemi (10 Gemmes)
- **Invincible** : Terminer un combat sans prendre de dégâts (25 Gemmes)
- **Collectionneur** : Posséder 50 cartes différentes (50 Gemmes)
- **Maître Tacticien** : Gagner 10 combats de suite sans perdre un allié (100 Gemmes)
- **Légende** : Terminer la campagne en Mode Difficile (200 Gemmes)

### Modes de Jeu

**Mode Histoire (de base) :**
- Campagne principale : donjons enchaînés, regroupés en Actes

**Mode Difficile (débloqué en terminant le Mode Histoire) :**
- Modificateurs : voir la difficulté « Difficile » dans `Combat_System.md` (+50 % HP ennemis, +30 % dégâts, -1 PA, ×1.5 Or et XP)

**Mode Arène (post-MVP) :**
- Combats enchaînés à difficulté croissante
- Classement (V2+, nécessite du online)
- Récompenses tous les 5 combats

**Défi Quotidien (post-MVP ; débloqué en terminant l'Acte 1) :**
- Combat spécial avec règles modifiées chaque jour
- Exemples :
  - « Pas de cartes de défense »
  - « Ennemis commencent avec +50 % HP »
  - « Toutes les cartes coûtent 1 PA »
- Récompenses : 50 Or + 10 Gemmes

---

## 📈 Courbe de Progression

- **20 niveaux** ; les PV des monstres suivent ceux des joueurs (barème de l'Excel : aventure solo = 1× PV joueur, groupe de donjon = 3× PV joueur, boss = 1.33× PV de l'équipe, superboss = 3.3× PV de l'équipe)
- Les donjons sont prévus pour une **équipe de 3**
- Découpage en Actes et estimation de temps de jeu : à refaire sur cette base

---

## 🎯 Objectifs de Design

### Sensation de Progression

1. **Court Terme (Par Combat) :**
   - Nouvelle carte ou amélioration
   - Or pour futur achat
   - Progression vers le prochain niveau

2. **Moyen Terme (Par Donjon) :**
   - Personnage qui monte en niveau (PV, slots)
   - Deck qui s'améliore
   - Un lieu qui retrouve sa couleur

3. **Long Terme (Méta) :**
   - Déblocage de personnages
   - Collection de cartes
   - Succès et cosmétiques

### Équilibrage

- Progression significative mais pas écrasante
- Le skill reste important même avec meilleur équipement
- Nouvelles mécaniques introduites progressivement
- Rejouabilité grâce à la variété des champions et des builds

---

**Dernière mise à jour :** 23 Septembre 2026
**Responsable :** Shinda + Claude
