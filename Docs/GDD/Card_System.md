# 🃏 Système de Cartes - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Dernière mise à jour:** Reflète l'implémentation actuelle

---

## 🎯 Vue d'Ensemble

Le système de cartes de **Project TDB** est basé sur une **triple identité** : chaque carte appartient à une **Famille**, une **Classe** et un **Élément**. Ce système permet des synergies profondes et une grande variété stratégique.

### Les 3 Dimensions des Cartes

| Dimension | Nombre | Description |
|-----------|--------|-------------|
| **Familles** | 8 | Identité thématique et émotions personnalisées |
| **Classes** | 5 | Rôle tactique sur le champ de bataille |
| **Éléments** | 4 | Type de dégâts magiques et interactions |

---

## 🎨 Les 8 Familles

Chaque famille a sa propre identité thématique et son système d'émotions unique.

| Famille | Couleur | Code Hex | Identité | Style de Jeu |
|---------|---------|----------|----------|--------------|
| **Déchaînés** | Rouge | #CC0000 | Guerriers impulsifs maîtrisant la colère | Attaques physiques puissantes, transformations extrêmes |
| **Dissidents** | Vert Foncé | #006600 | Rebelles tactiques adaptables | Contrôle, manipulation du terrain |
| **Insurgents** | Jaune | #FFEB00 | Révolutionnaires charismatiques | Buffs d'équipe, mobilité, leadership |
| **Exilés** | Bleu Foncé | #000080 | Parias solitaires mystérieux | Magie sombre, invocations |
| **Réprouvés** | Violet | #800080 | Maudits cherchant rédemption | Sacrifice de PV pour puissance |
| **Gardiens** | Vert Clair | #80FF80 | Protecteurs nobles | Défense, soins, boucliers |
| **Éveillés** | Bleu Clair | #80CCFF | Illuminés spirituels | Magie spirituelle, soutien |
| **Précurseurs** | Orange | #FF8000 | Pionniers innovants | Technologie, effets uniques |

### Détails : Famille Déchaînés

**Système d'Émotions:**

| État | Seuil | Nom | Type | Caractéristiques |
|------|-------|-----|------|------------------|
| Positif | +100 | Contrariété | Tank | Défense accrue, frustration canalisée |
| Neutre | 0 | Colère | Équilibré | État de base, équilibre combat |
| Négatif | -100 | Rage | DPS | Dégâts explosifs, fureur destructrice |

**Exemples de Cartes:**
- **Frappe Furieuse** (2 PA) : 12 dégâts physiques, -10 émotion (vers Rage)
- **Hurlement de Guerre** (1 PA) : +3 dégâts à tous alliés, +5 émotion (vers Contrariété)
- **Coup de Bouclier** (2 PA) : 8 dégâts, +5 Bouclier, +8 émotion (vers Contrariété)

### Autres Familles

**Note:** Les émotions des 7 autres familles sont à définir. Chaque famille aura 3 états émotionnels personnalisés selon son thème.

---

## 🎭 Les 5 Classes

Les classes définissent le rôle tactique de la carte sur le champ de bataille.

| Classe | Rôle | Caractéristiques | Exemples de Cartes |
|--------|------|------------------|-------------------|
| **Ancre** | Tank / Point de contrôle | HP élevés, contrôle de zone, blocage | Cartes de taunt, boucliers massifs, fortifications |
| **Tisseur** | Mage / Contrôleur | Magie puissante, contrôle de foule, AOE | Sorts élémentaires, debuffs, zones de terrain |
| **Ombrelame** | Assassin / Burst DPS | Dégâts ciblés élevés, mobilité, élimination | Attaques surprises, exécutions, téléportations |
| **Veilleur** | Support / Défenseur | Soins, buffs d'équipe, protection | Soins de zone, boucliers alliés, purification |
| **Harmoniste** | Équilibriste / Hybride | Polyvalent, synergie émotionnelle | Cartes qui changent selon l'émotion, effets hybrides |

---

## 🔥 Les 4 Éléments

Les éléments déterminent le type de dégâts magiques et créent des interactions stratégiques.

| Élément | Couleur | Code Hex | Effets Principaux | Fort Contre | Faible Contre |
|---------|---------|----------|-------------------|-------------|---------------|
| **Feu** | Rouge-Orange | #FF4D00 | Dégâts directs, brûlure, zones persistantes | Ombre | Eau |
| **Ombre** | Violet Foncé | #33004D | Drain de vie, affaiblissement, camouflage | Lumière | Feu |
| **Lumière** | Jaune Clair | #FFFFB3 | Soins, purification, révélation | Eau | Ombre |
| **Eau** | Bleu | #0080FF | Gel, ralentissement, zones glissantes | Feu | Lumière |

### Mécaniques par Élément

**Feu:**
- Dégâts sur la durée (Brûlure)
- Zones de feu persistantes (3 tours)
- Explosions et AOE

**Ombre:**
- Vol de vie (lifesteal)
- Réduction de stats ennemies
- Camouflage et invisibilité

**Lumière:**
- Soins directs et régénération
- Retrait de debuffs (purification)
- Révélation (anti-camouflage)

**Eau:**
- Gel (immobilisation temporaire)
- Ralentissement (-PM)
- Zones glissantes (mouvement forcé)

---

## 📋 Caractéristiques des Cartes

### Informations de Base

| Attribut | Valeurs | Description |
|----------|---------|-------------|
| **Nom** | Texte | Nom de la carte |
| **Description** | Texte | Explication de l'effet |
| **Illustration** | Image | Art de la carte |
| **Famille** | 1 parmi 8 | Déchaînés, Dissidents, etc. |
| **Classe** | 1 parmi 5 | Ancre, Tisseur, etc. |
| **Élément** | 1 parmi 4 ou None | Feu, Ombre, Lumière, Eau |

### Coût et Ressources

| Attribut | Plage | Utilisation |
|----------|-------|-------------|
| **Coût PA** | 0 à 5+ | Points d'Action pour jouer la carte |
| **Modificateur Émotion** | -50 à +50 | Influence sur la jauge émotionnelle |

---

## 🎯 Système de Ciblage

### Types de Cible

| Type | Description | Cas d'Usage | Exemple |
|------|-------------|-------------|---------|
| **None** | Aucune cible | Effet automatique | Buff personnel instantané |
| **Self** | Soi-même uniquement | Auto-ciblage | Se soigner, se buffer |
| **Enemy** | Un ou plusieurs ennemis | Attaques standard | Frappe, Sort offensif |
| **Ally** | Alliés (sauf soi) | Support d'équipe | Soigner un allié |
| **AllyOrSelf** | Alliés ET soi-même | Support flexible | Soins de groupe |
| **AllyorEnemy** | Alliés ET ennemis | Effets mixtes | Explosion qui touche tout le monde |
| **AnyUnit** | N'importe quelle unité | Polyvalent | Télékinésie, déplacement forcé |
| **EmptyTile** | Tuiles vides uniquement | Placement | Invocation, piège, zone |
| **AnyTile** | N'importe quelle tuile | Zone centrée | Météore, explosion ciblée |

### Portée des Cartes

| Portée | Distance | Usage Type |
|--------|----------|------------|
| **0** | Soi-même ou adjacents | Buffs personnels, AOE autour de soi |
| **1** | Cases adjacentes | Mêlée, attaques au corps-à-corps |
| **2-3** | Portée courte | Sorts courts, armes de jet |
| **4-6** | Portée moyenne | Sorts standards, arcs |
| **7+** | Portée longue | Sorts puissants, artillerie |
| **99** | Portée infinie | Sorts globaux |

---

## 💥 Zones d'Effet (AOE)

### Formes d'AOE

| Forme | Description | Schéma | Usage |
|-------|-------------|--------|-------|
| **None** | Cible unique | • | Cartes ciblées précises |
| **OneTile** | Une seule case | • | Piège, zone minimale |
| **Circle** | Cercle (rayon variable) | ○ | Explosions, sorts de zone |
| **Line** | Ligne droite | \| | Rayon, souffle, charge |
| **Cross** | Croix (4 directions) | + | Onde de choc, croix élémentaire |
| **Cone** | Cône directionnel | ▷ | Souffle, arc de feu |

### Visualisation des Formes

**Circle (Rayon 2):**
```
    . . X . .
    . X X X .
    X X ● X X    ● = Épicentre
    . X X X .    X = Cases affectées
    . . X . .
```

**Line:**
```
    . . ● . .
    . . X . .
    . . X . .
    . . X . .
```

**Cross:**
```
    . . X . .
    . . X . .
    X X ● X X
    . . X . .
    . . X . .
```

**Cone:**
```
    . X X X .
    . . X . .
    . . ● . .
```

### Cibles Affectées dans l'AOE

| Type | Qui est Affecté | Usage | Exemple |
|------|-----------------|-------|---------|
| **None** | Personne | Zone décorative | Mur de feu visuel |
| **Self** | Soi-même uniquement | Boost personnel | Aura personnelle |
| **Enemies** | Que les ennemis | Attaques offensives | Boule de Feu |
| **Ally** | Que les alliés | Support pur | Aura de soins |
| **AllyOrSelf** | Alliés ET soi | Support inclusif | Bénédiction de groupe |
| **AllyorEnemy** | Alliés ET ennemis | Effet neutre/risqué | Explosion suicidaire |
| **AnyUnit** | Tout le monde | Effet universel | Onde de choc totale |

---

## 🎲 Types de Dégâts et Effets

### Types de Dégâts

| Type | Description | Formule | Minimum |
|------|-------------|---------|---------|
| **Physical** | Dégâts physiques | Dégâts - Défense Physique | 1 |
| **Magical** | Dégâts magiques | Dégâts - Défense Magique | 1 |

**Note:** Les dégâts infligent toujours au minimum 1 HP pour éviter les situations d'immunité totale.

### Effets Principaux

| Effet | Attribut | Plage | Description |
|-------|----------|-------|-------------|
| **Dégâts** | damageAmount | 0 à 50+ | Points de dégâts infligés |
| **Soins** | healAmount | 0 à 50+ | Points de vie restaurés |
| **Mouvement** | movementAmount | 0 à 5+ | PM bonus accordés |

---

## 😊 Système d'Émotions sur les Cartes

### Modificateur d'Émotion

Chaque carte peut influencer la jauge émotionnelle du champion qui la joue.

| Valeur | Direction | Effet | Exemple de Carte |
|--------|-----------|-------|------------------|
| **+1 à +50** | Vers Positif/Tank | Pousse vers transformation défensive | Bouclier (+10), Mur (+20) |
| **0** | Neutre | Aucun impact émotionnel | Cartes utilitaires |
| **-1 à -50** | Vers Négatif/DPS | Pousse vers transformation offensive | Frappe (-10), Rage (-30) |

### Impact sur le Gameplay

- **Gestion Consciente:** Les joueurs doivent choisir entre efficacité immédiate et contrôle émotionnel
- **Synergie de Deck:** Construire un deck qui penche vers Tank ou DPS
- **Adaptabilité:** Cartes neutres permettent de rester flexible

---

## 📊 Exemples de Cartes Complètes

### Exemple 1: Frappe de Rage (Déchaînés)

| Attribut | Valeur |
|----------|--------|
| **Nom** | Frappe de Rage |
| **Famille** | Déchaînés |
| **Classe** | Ombrelame |
| **Élément** | Aucun |
| **Coût PA** | 2 |
| **Type Cible** | Enemy |
| **Portée** | 1 (Mêlée) |
| **AOE** | None |
| **Type Dégâts** | Physical |
| **Dégâts** | 12 |
| **Modificateur Émotion** | -10 (vers Rage) |

**Effet:** Attaque de mêlée qui inflige 12 dégâts physiques et pousse le champion vers la Rage (-10 émotion).

---

### Exemple 2: Boule de Feu (Précurseurs)

| Attribut | Valeur |
|----------|--------|
| **Nom** | Boule de Feu |
| **Famille** | Précurseurs |
| **Classe** | Tisseur |
| **Élément** | Feu |
| **Coût PA** | 3 |
| **Type Cible** | AnyTile |
| **Portée** | 6 |
| **AOE** | Circle (rayon 2) |
| **Cibles Affectées** | Enemies |
| **Type Dégâts** | Magical |
| **Dégâts** | 15 |
| **Modificateur Émotion** | 0 (neutre) |

**Effet:** Lance une boule de feu explosive qui inflige 15 dégâts magiques à tous les ennemis dans un rayon de 2 cases.

---

### Exemple 3: Bénédiction du Gardien (Gardiens)

| Attribut | Valeur |
|----------|--------|
| **Nom** | Bénédiction du Gardien |
| **Famille** | Gardiens |
| **Classe** | Veilleur |
| **Élément** | Lumière |
| **Coût PA** | 2 |
| **Type Cible** | AllyOrSelf |
| **Portée** | 4 |
| **AOE** | None |
| **Soins** | 20 |
| **Modificateur Émotion** | +5 (vers Tank) |

**Effet:** Soigne un allié ou soi-même de 20 HP et apaise légèrement les émotions (+5 vers Tank).

---

## 🎯 Design par Famille

### Déchaînés (Rouge) - Thèmes de Cartes

**Archétypes:**
- Attaques physiques puissantes
- Cartes qui manipulent fortement les émotions
- Sacrifice de défense pour plus de dégâts
- Effets qui s'amplifient avec la Rage

**Exemples:**
- "Frappe Dévastatrice" (4 PA) : 25 dégâts, -20 émotion
- "Calme Forcé" (1 PA) : +15 émotion, +5 Défense
- "Explosion de Rage" (3 PA, nécessite état Rage) : 30 dégâts AOE

---

### Gardiens (Vert Clair) - Thèmes de Cartes

**Archétypes:**
- Soins et protection
- Boucliers d'équipe
- Purification de debuffs
- Cartes qui bénéficient des émotions positives

**Exemples:**
- "Aura de Protection" (2 PA) : +10 Bouclier à tous les alliés
- "Sanctuaire" (3 PA) : Crée une zone qui soigne 5 HP/tour pendant 3 tours
- "Purification" (1 PA) : Retire tous les debuffs d'un allié

---

## 📈 Équilibrage

### Formules de Coût en PA

**Dégâts Simples (Cible Unique):**
- Formule: Coût PA ≈ Dégâts / 6
- Exemples:
  - 6 dégâts = 1 PA
  - 12 dégâts = 2 PA
  - 18 dégâts = 3 PA

**AOE (Zone d'Effet):**
- Multiplicateur: ×1.5 à ×2 selon la taille
- Exemples:
  - 12 dégâts rayon 1 = 2-3 PA
  - 12 dégâts rayon 2 = 3-4 PA

**Soins:**
- Formule: Coût PA ≈ Soins / 10
- Exemples:
  - 10 soins = 1 PA
  - 20 soins = 2 PA

**Modificateur Émotion:**
- Ajout: +0.5 PA si |modificateur| > 10
- Les cartes avec forte influence émotionnelle coûtent légèrement plus

---

## 🎨 Visuels et Couleurs

### Couleurs par Famille

Les bordures et fonds des cartes utilisent les couleurs de famille pour identification rapide:

| Famille | Hex | Visuel |
|---------|-----|--------|
| Déchaînés | #CC0000 | Bordure rouge intense |
| Dissidents | #006600 | Bordure vert foncé |
| Insurgents | #FFEB00 | Bordure jaune vif |
| Exilés | #000080 | Bordure bleu foncé |
| Réprouvés | #800080 | Bordure violet profond |
| Gardiens | #80FF80 | Bordure vert clair |
| Éveillés | #80CCFF | Bordure bleu clair |
| Précurseurs | #FF8000 | Bordure orange |

### Couleurs par Élément

Les effets visuels et particules utilisent les couleurs élémentaires:

| Élément | Hex | Effets Visuels |
|---------|-----|----------------|
| Feu | #FF4D00 | Flammes, braise, explosions |
| Ombre | #33004D | Fumée sombre, tentacules |
| Lumière | #FFFFB3 | Rayons dorés, halos |
| Eau | #0080FF | Vagues, gouttelettes, glace |

---

**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Cartes Project TDB

**Documents Connexes:**
- [GDD_Main.md](GDD_Main.md) - Vision globale du projet
- [Combat_System.md](Combat_System.md) - Système de combat
- [Characters.md](Characters.md) - Champions et familles
