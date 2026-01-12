# 👥 Champions - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète la structure actuelle (ChampionData)

---

## 🎯 Vue d'Ensemble

Les **Champions** de Project TDB appartiennent à l'une des **8 Familles**, chacune avec son propre système d'émotions et style de jeu unique. Les champions utilisent des decks personnalisés de cartes et peuvent se transformer au cours du combat selon leur état émotionnel.

---

## 📊 Structure d'un Champion (ChampionData)

### Données de Base

| Attribut | Type | Description |
|----------|------|-------------|
| **Nom** | Text | Nom du champion |
| **Prefab** | GameObject | Modèle 3D/2D du champion |
| **Famille** | CardFamillyType | Une des 8 familles |
| **Élément** | CardElementType | Feu, Ombre, Lumière, Eau ou None |

---

### Statistiques de Combat

| Stat | Plage Typique | Description |
|------|---------------|-------------|
| **Max Health** | 50-150 | Points de vie maximum |
| **Movement Range** | 2-4 | Points de mouvement par tour |
| **Max Action Points** | 3-5 | Points d'action par tour |
| **Physical Defense** | 10-30 | Défense contre dégâts physiques |
| **Magical Defense** | 10-30 | Défense contre dégâts magiques |

---

### Deck de Départ

| Composant | Description |
|-----------|-------------|
| **Starting Deck** | Liste de CardData (10-20 cartes) |
| **Pioche** | Mélangé au début du combat |
| **Main** | Jusqu'à 5 cartes |
| **Défausse** | Cartes jouées et défaussées |

---

### Système d'Émotions

| Attribut | Type | Description |
|----------|------|-------------|
| **Family Emotion Data** | FamilyEmotionData | Noms des 3 émotions (Positif/Neutre/Négatif) |
| **Positive Transformation** | TransformationData | Modificateurs à +100 (Tank) |
| **Negative Transformation** | TransformationData | Modificateurs à -100 (DPS) |

---

## 🎨 Les 8 Familles de Champions

### Déchaînés (Rouge #CC0000)

**Identité:** Guerriers impulsifs maîtrisant la colère

**Émotions:**
| État | Seuil | Nom | Type |
|------|-------|-----|------|
| Positif | +100 | Contrariété | Tank |
| Neutre | 0 | Colère | Équilibré |
| Négatif | -100 | Rage | DPS |

**Style de Jeu:**
- Attaques physiques puissantes
- Transformations extrêmes
- Gestion intense des émotions
- DPS physique ou Tank selon transformation

**Exemple de Champion:** Ilya (Déchaîné, Élément: None)

---

### Dissidents (Vert Foncé #006600)

**Identité:** Rebelles tactiques adaptables

**Émotions:** À définir (Positif/Neutre/Négatif selon thème de rébellion)

**Style de Jeu:**
- Contrôle du terrain
- Manipulation tactique
- Adaptabilité
- Cartes versatiles

---

### Insurgents (Jaune #FFEB00)

**Identité:** Révolutionnaires charismatiques

**Émotions:** À définir (Positif/Neutre/Négatif selon thème de leadership)

**Style de Jeu:**
- Buffs d'équipe
- Mobilité accrue
- Leadership
- Synergie avec alliés

---

### Exilés (Bleu Foncé #000080)

**Identité:** Parias solitaires mystérieux

**Émotions:** À définir (Positif/Neutre/Négatif selon thème d'exil)

**Style de Jeu:**
- Magie sombre
- Invocations
- Solitaire mais puissant
- Effets mystérieux

---

### Réprouvés (Violet #800080)

**Identité:** Maudits cherchant la rédemption

**Émotions:** À définir (Positif/Neutre/Négatif selon thème de malédiction)

**Style de Jeu:**
- Sacrifice de HP pour puissance
- Rédemption par le combat
- Risk vs Reward
- Effets de corruption

---

### Gardiens (Vert Clair #80FF80)

**Identité:** Protecteurs nobles

**Émotions:** À définir (Positif/Neutre/Négatif selon thème de protection)

**Style de Jeu:**
- Défense et soins
- Boucliers pour alliés
- Protection
- Support défensif

---

### Éveillés (Bleu Clair #80CCFF)

**Identité:** Illuminés spirituels

**Émotions:** À définir (Positif/Neutre/Négatif selon thème spirituel)

**Style de Jeu:**
- Magie spirituelle
- Soutien et buffs
- Harmonie
- Effets bénéfiques

---

### Précurseurs (Orange #FF8000)

**Identité:** Pionniers innovants

**Émotions:** À définir (Positif/Neutre/Négatif selon thème d'innovation)

**Style de Jeu:**
- Technologie et innovation
- Effets uniques
- Expérimentation
- Mécaniques nouvelles

---

## ⚡ Transformations

### Transformation Data

| Modificateur | Description | Exemple (Rage) | Exemple (Contrariété) |
|--------------|-------------|----------------|----------------------|
| **Max Health Modifier** | Bonus/malus HP max | +0 | +20 |
| **Max PA Modifier** | Bonus/malus PA max | +0 | +0 |
| **Defense P Modifier** | Bonus défense physique | -5 | +10 |
| **Defense M Modifier** | Bonus défense magique | -5 | +10 |
| **Movement Modifier** | Bonus/malus PM | +0 | -1 |

---

### Effets Passifs des Transformations

| Effet | Description | Exemple |
|-------|-------------|---------|
| **Bonus Damage** | Dégâts bonus sur attaques | Rage: +10 |
| **Lifesteal** | Vol de vie (0.0 à 1.0) | Rage: 0.15 (15%) |
| **HP Regen Per Turn** | Régénération par tour | Contrariété: +5 |

---

### Visuels des Transformations

| Composant | Description |
|-----------|-------------|
| **Glow Color** | Couleur de l'aura du champion |
| **Visual Effect Prefab** | Effet de particules (optionnel) |

---

## 🎮 Exemples de Champions

### Champion: Ilya (Déchaîné)

| Attribut | Valeur |
|----------|--------|
| **Famille** | Déchaînés |
| **Élément** | None |
| **Max Health** | 100 |
| **Movement Range** | 3 |
| **Max Action Points** | 4 |
| **Physical Defense** | 15 |
| **Magical Defense** | 10 |

**Deck de Départ (Exemple):**
- 5× Frappe Rapide (1 PA, 8 dégâts, -5 émotion)
- 3× Frappe Puissante (2 PA, 12 dégâts, -10 émotion)
- 2× Bouclier (1 PA, soigne 10 HP, +8 émotion)

**Transformations:**
- **Rage** (-100) : +10 dégâts, 15% lifesteal, -5 défenses
- **Contrariété** (+100) : +20 HP, +10 défenses, +5 HP/tour

---

## 🔄 Système de Progression (À Implémenter)

### Gain d'Expérience Prévu

| Source | XP Typique |
|--------|------------|
| **Combat gagné** | 100 XP |
| **Ennemi vaincu** | 10-50 XP |
| **Objectifs secondaires** | 50 XP |

---

### Récompenses par Niveau Prévues

| Niveau | Récompense |
|--------|-----------|
| **Chaque Niveau** | Amélioration de stats |
| **Niveaux Clés** | Nouvelles cartes débloquées |
| **Niveau 10** | Carte ultime |

**Note:** Le système de progression n'est pas encore implémenté dans le code actuel.

---

## 📋 Création de Champions

### Processus dans Unity

1. **Créer ScriptableObject:** Clic droit → Champion/Champion Data
2. **Remplir Identité:** Nom, Prefab, Famille, Élément
3. **Définir Stats:** HP, PA, PM, Défenses
4. **Assigner Deck:** Glisser-déposer cartes dans Starting Deck
5. **Configurer Émotions:** Assigner FamilyEmotionData
6. **Définir Transformations:** Créer TransformationData pour +100 et -100

---

### Checklist de Validation

| Élément | Vérifié |
|---------|---------|
| Prefab assigné | ☐ |
| Famille choisie (8 familles) | ☐ |
| Stats dans les plages correctes | ☐ |
| Deck de 10-20 cartes | ☐ |
| FamilyEmotionData assigné | ☐ |
| 2 TransformationData créés | ☐ |

---

## 🎯 Design des Champions

### Principes de Design

| Principe | Description |
|----------|-------------|
| **Identité Claire** | Champion incarneun thème de sa famille |
| **Transformations Significatives** | Tank vs DPS bien différenciés |
| **Synergies de Deck** | Cartes cohérentes avec la famille |
| **Équilibrage** | Stats comparables entre champions |

---

### Équilibrage des Stats

**Budget Total Typique:** ~180-200 points

| Distribution | Tank | DPS | Support |
|--------------|------|-----|---------|
| **HP** | 120-150 | 80-100 | 90-110 |
| **Défenses** | 25-30 | 10-15 | 15-20 |
| **PA** | 3-4 | 4-5 | 4-5 |
| **PM** | 2-3 | 3-4 | 2-3 |

---

## 📝 Champions À Créer

### Priorité Haute
- 1 champion par famille (8 total)
- Émotions définies pour chaque famille
- Decks de départ complets

### Priorité Moyenne
- Champions alternatifs par famille
- Variations de builds
- Spécialisations

### Priorité Basse
- Champions hybrides
- Champions légendaires
- Champions événementiels

---

**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Champions Project TDB

**Documents Connexes:**
- [Card_System.md](Card_System.md) - Système de cartes et familles
- [Combat_System.md](Combat_System.md) - Système de combat
- [GDD_Main.md](GDD_Main.md) - Vision globale
