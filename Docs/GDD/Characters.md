# Champions - Émotions Tactics (Project TDB)

**Version:** 3.0
**Date:** 23 Septembre 2026
**Statut:** Reflète la structure actuelle (ChampionData)

## Vue d'Ensemble

Chaque **champion** a un trauma, un **passif** et **2 cartes Signature** (identité Neutre), plus un **profil PA/PM**. Il peut jouer n'importe quelle émotion : chaque deck en choisit 1 ou 2. Roster MVP : Evan, Crux, Raze (voir `CHAMPIONS_CONCEPTS.md` et l'Excel `TCG_Tactique_Systeme_de_calcul.xlsx`).

> **Pas de champ Classe** (décision du 10/09/2026). **Champ Famille** : les champions du MVP n'ont pas d'émotion attitrée (leurs Signatures sont Neutres) — le champ `Famille` de ChampionData devient optionnel ou disparaît.

## Structure d'un Champion (ChampionData)

### Données de Base

| Attribut | Type | Description |
|----------|------|--------------|
| **Nom** | Text | Nom du champion |
| **Prefab** | GameObject | Modèle 3D/2D du champion |
| **Passif** | Référence | Mécanique signature (Miroir fraternel, Réflexe du grimpeur, Main gagnante…) |
| **Cartes Signature** | 2 × CardData | Cartes propres au champion |


### Statistiques de Combat

| Stat | Description |
|------|--------------|
| **Max Health** | 100 au niveau 1, +15 par niveau |
| **Max Action Points** | Selon le profil (budget PA + PM = 9, min 3) |
| **Movement Range** | Selon le profil (min 2) |
| **Defense** | Champ du code ; aucune stat de défense n'est définie dans le design actuel |


### Deck de Départ

| Composant | Description |
|-----------|-------------|
| **Deck** | 24 CardData : 2 Signature + 6 Éveil + 16 Standard ; plusieurs decks possibles par champion |
| **Pioche** | Mélangée au début du combat |
| **Main** | Règle à trancher (voir `Combat_System.md`) |
| **Défausse** | Cartes jouées et défaussées |


## Système de Progression (À Implémenter)

Niveaux (max 20), PV, slots de cartes, XP : voir **`Progression.md`** et l'Excel. Le niveau ne change jamais les PA/PM ni la puissance des cartes.

**Note:** Le système de progression n'est pas encore implémenté dans le code actuel.


**Dernière mise à jour:** 23 Septembre 2026
**Version:** 3.0
**Responsable:** Shinda + Claude
