# Champions - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète la structure actuelle (ChampionData)


## Vue d'Ensemble

Les **Champions** de Project TDB appartiennent à l'une des **8 Familles**, chacune avec son propre système d'émotions et style de jeu unique. Les champions utilisent des decks personnalisés de cartes et peuvent se transformer au cours du combat selon leur état émotionnel.


## Structure d'un Champion (ChampionData)

### Données de Base

| Attribut    | Type            | Description                      |
|-------------|-----------------|----------------------------------|
| **Nom**     | Text            | Nom du champion                  |
| **Prefab**  | GameObject      | Modèle 3D/2D du champion         |
| **Famille** | CardFamillyType | Une des 8 familles               |


### Statistiques de Combat

| Stat                  | Description                  |
|-----------------------|------------------------------|
| **Max Health**        | Points de vie maximum        |
| **Movement Range**    | Points de mouvement par tour |
| **Max Action Points** | Points d'action par tour     |
| **Defense**           | Défense                      |


### Deck de Départ

| Composant | Description |
|-----------|-------------|
| **Starting Deck** | Liste de CardData (10-20 cartes) |
| **Pioche** | Mélangé au début du combat |
| **Main** | Jusqu'à 5 cartes |
| **Défausse** | Cartes jouées et défaussées |


## Système de Progression (A Implémenter)

### Gain d'Expérience Prévu

| Source                    | XP Typique |
|---------------------------|------------|
| **Combat gagné**          | 100 XP     |
| **Ennemi vaincu**         | 10-50 XP   |
| **Objectifs secondaires** | 50 XP      |


### Récompenses par Niveau Prévues

| Niveau            | Récompense                  |
|-------------------|-----------------------------|
| **Chaque Niveau** | Amélioration de stats       |
| **Niveaux Clés**  | Nouvelles cartes débloquées |
| **Niveau 10**     | Carte ultime                |

**Note:** Le système de progression n'est pas encore implémenté dans le code actuel.


**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Champions Project TDB
