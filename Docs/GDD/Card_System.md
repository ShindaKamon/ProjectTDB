# Système de Cartes - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Dernière mise à jour:** Reflète l'implémentation actuelle

---

## Vue d'Ensemble

Le système de cartes de **Project TDB** est basé sur une **double identité** : chaque carte appartient à une **Famille**, une **Classe** (triple identité : et un **Elément**). Ce système permet des synergies profondes et une grande variété stratégique.

### Les 3 Dimensions des Cartes

A voir si on n'utilise pas juste la famille comme émotion sans avoir de cartes identitaire.

| Dimension    | Nombre | Description                                    |
|--------------|--------|------------------------------------------------|
| **Familles** | ?      | Identité thématique et émotions personnalisées |
| **Classes**  | ?      | Rôle tactique sur le champ de bataille         |
| **Neutre**   | ?      | Utilisable par tous                            |

---

## Les 8 Familles

Chaque famille a sa propre identité thématique et son système d'émotions unique.

| Famille         | Couleur    | Code Hex  | 
|-----------------|------------|-----------|
| **Déchaînés**   | Rouge      | #CC0000 | 
| **Dissidents**  | Violet     | #800080 |
| **Insurgents**  | Bleu Foncé | #000080 | 
| **Exilés**      | Bleu Clair | #80CCFF |
| **Réprouvés**   | Vert Foncé | #006600 | 
| **Gardiens**    | Vert Clair | #80FF80 | 
| **Eveillés**    | Jaune      | #FFEB00 | 
| **Précurseurs** | Orange     | #FF8000 |


## Les 5 Classes

Les classes définissent **comment le champion gère son émotion** - la psychologie derrière leur pouvoir.

| Classe           | Gestion Émotionnelle     | Mécanique de Jeu                             | Style de Combat      |
|------------------|--------------------------|----------------------------------------------|----------------------|
| **Exutoire**     | **Consomme** l'émotion   | Il cherche à se débarrasser de son émotion   | Attaque dévastatrice |
| **Refoulé**      | **Stocke** l'émotion     | Il empile son émotion pour constuire un mur  | Renforcement         |
| **Façonneur**    | **Transforme** l'émotion | Il forme son émotion comme de l'argile       | Manipulation         |
| **Evadé**        | **Anesthésie** l'émotion | Il éteint son émotion pour trouver le calme  | Support              |
| **Parasite**     | **Déplace** l'émotion    | Il donne son émotion dans des invocation     | Gestion d'unité      |


## Les 4 Eléments

Les éléments ne sont pas encore mis en place pour l'instant cela reste à examiner pour savoir si on l'incrémente ou pas dans le jeu.

| Elément     | Couleur    | Code Hex  | Elément Fort |
|-------------|------------|-----------|--------------|
| **Feu**     | Rouge      | #CC0000 | Brasier      |
| **Poison**  | Violet     | #800080 | Toxine       |
| **Eau**     | Bleu foncé | #000080 | Glace        |
| **Foudre**  | Bleu clair | #80CCFF | Electricité  |
| **Ombre**   | Vert foncé | #006600 | Obscurité    |
| **Pierre**  | Vert clair | #80FF80 | Terre        |
| **Lumière** | Jaune      | #FFEB00 | Soleil       |
| **Vent**    | Orange     | #FF8000 | Tempête      |


## Caractéristiques des Cartes

### Informations de Base

| Attribut         | Valeurs | Description            |
|------------------|---------|------------------------|
| **Nom**          | Texte   | Nom de la carte        |
| **Description**  | Texte   | Explication de l'effet |
| **Illustration** | Image   | Art de la carte        |
| **Coût en PA**   | Texte   | Points d'Action        |

## Système de Ciblage

### Types de Cible

| Type            | Description             | Cas d'Usage       | Exemple                            |
|-----------------|-------------------------|-------------------|------------------------------------|
| **None**        | Aucune cible            | Effet automatique | Buff personnel instantané          |
| **Self**        | Soi-même uniquement     | Auto-ciblage      | Se soigner, se buffer              |
| **Enemy**       | Un ou plusieurs ennemis | Attaques standard | Frappe, Sort offensif              |
| **Ally**        | Alliés (sauf soi)       | Support d'équipe  | Soigner un allié                   |
| **AllyOrSelf**  | Alliés ET soi-même      | Support flexible  | Soins de groupe                    |
| **AllyorEnemy** | Alliés ET ennemis       | Effets mixtes     | Explosion qui touche tout le monde |
| **AnyUnit**     | N'importe quelle unité  | Polyvalent        | Télékinésie, déplacement forcé     |
| **EmptyTile**   | Tuiles vides uniquement | Placement         | Invocation, piège, zone            |
| **AnyTile**     | N'importe quelle tuile  | Zone centrée      | Météore, explosion ciblée          |


### Portée des Cartes

| Portée  | Distance              | Usage Type                          |
|---------|-----------------------|-------------------------------------|
| **0**   | Soi-même ou adjacents | Buffs personnels, AOE autour de soi |
| **1**   | Cases adjacentes      | Mêlée, attaques au corps-à-corps    |
| **2-3** | Portée courte         | Sorts courts, armes de jet          |
| **4-6** | Portée moyenne        | Sorts standards, arcs               |
| **7+**  | Portée longue         | Sorts puissants, artillerie         |
| **99**  | Portée infinie        | Sorts globaux                       |


## Zones d'Effet (AOE)

### Formes d'AOE

| Forme       | Description             |
|-------------|-------------------------|
| **None**    | Cible unique            | 
| **OneTile** | Une seule case          | 
| **Circle**  | Cercle (rayon variable) | 
| **Line**    | Ligne droite            | 
| **Cross**   | Croix (4 directions)    |
| **Cone**    | Cône directionnel       |         

### Visualisation des Formes

**Circle (Rayon 2):**
```
    . . X . .
    . X X X .
    X X o X X    o = Epicentre
    . X X X .    X = Cases affectées
    . . X . .
```

**Line:**
```
    . . o . .
    . . X . .
    . . X . .
    . . X . .
```

**Cross:**
```
    . . X . .
    . . X . .
    X X o X X
    . . X . .
    . . X . .
```

**Cone:**
```
    . X X X .
    . . X . .
    . . o . .
```

### Cibles Affectées dans l'AOE

| Type            | Qui est Affecté     | Usage               | Exemple               |
|-----------------|---------------------|---------------------|-----------------------|
| **None**        | Personne            | Zone décorative     | Mur de feu visuel     |
| **Self**        | Soi-même uniquement | Boost personnel     | Aura personnelle      |
| **Enemies**     | Que les ennemis     | Attaques offensives | Boule de Feu          |
| **Ally**        | Que les alliés      | Support pur         | Aura de soins         |
| **AllyOrSelf**  | Alliés ET soi       | Support inclusif    | Bénédiction de groupe |
| **AllyorEnemy** | Alliés ET ennemis   | Effet neutre/risqué | Explosion suicidaire  |
| **AnyUnit**     | Tout le monde       | Effet universel     | Onde de choc totale   |

## Effets

### Effets Principaux

| Effet         | Attribut       | Plage   | Description               |
|---------------|----------------|---------|---------------------------|
| **Dégâts**    | damageAmount   | 0 à 50+ | Points de dégâts infligés |
| **Soins**     | healAmount     | 0 à 50+ | Points de vie restaurés   |
| **Mouvement** | movementAmount | 0 à 5+  | PM bonus accordés         |