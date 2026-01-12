# 👹 Ennemis - Project TDB

**Version:** 2.0
**Date:** 11 Janvier 2026
**Statut:** Reflète la structure actuelle (EnemyData)

---

## 🎯 Vue d'Ensemble

Les **Ennemis** de Project TDB utilisent un système de **deck pattern** où leurs cartes sont jouées dans un ordre fixe et séquentiel (pas de mélange). Ils sont classifiés en ennemis normaux et boss, avec des barres de vie différentes selon leur type.

---

## 📊 Structure d'un Ennemi (EnemyData)

### Données de Base

| Attribut | Type | Description |
|----------|------|-------------|
| **Nom** | Text | Nom de l'ennemi |
| **Prefab** | GameObject | Modèle 3D/2D de l'ennemi |
| **Élément** | CardElementType | Feu, Ombre, Lumière, Eau ou None |
| **Is Boss** | Boolean | Si true, barre de vie en haut de l'écran |

---

### Statistiques de Combat

| Stat | Plage Typique | Description |
|------|---------------|-------------|
| **Max Health** | Normaux: 30-80, Boss: 200+ | Points de vie maximum |
| **Movement Range** | 2-4 | Points de mouvement par tour |
| **Max Action Points** | 2-4 | Points d'action par tour |
| **Physical Defense** | 5-20 | Défense contre dégâts physiques |
| **Magical Defense** | 5-20 | Défense contre dégâts magiques |

---

### Deck Pattern (Combat Deck)

| Caractéristique | Ennemis | Champions (Comparaison) |
|-----------------|---------|-------------------------|
| **Type de Pioche** | Séquentielle | Aléatoire (mélangé) |
| **Ordre** | Fixe, se répète | Aléatoire à chaque pioche |
| **Taille Typique** | 5-10 cartes | 15-25 cartes |
| **Stratégie** | Pattern prévisible | Imprévisible |

---

### Visual Settings

| Paramètre | Description |
|-----------|-------------|
| **Health Bar Offset** | Position de la barre de vie au-dessus de la tête |
| **Health Bar Color** | Couleur de la barre de vie (typiquement rouge) |

---

## 🎭 Différence Normaux vs Boss

### Ennemis Normaux

| Caractéristique | Valeur |
|-----------------|--------|
| **Is Boss** | false |
| **Barre de Vie** | Au-dessus de la tête |
| **HP Typiques** | 30-80 |
| **PA Typiques** | 2-3 |
| **Deck Pattern** | 5-8 cartes |

**Utilisation:**
- Ennemis de base dans les rencontres
- Multiples par combat
- Patterns simples

---

### Boss

| Caractéristique | Valeur |
|-----------------|--------|
| **Is Boss** | true |
| **Barre de Vie** | En haut de l'écran (BossHealthBar) |
| **HP Typiques** | 200-500 |
| **PA Typiques** | 3-4 |
| **Deck Pattern** | 8-12 cartes |

**Utilisation:**
- Un seul par combat (typiquement)
- Fin de niveau, événements spéciaux
- Patterns complexes avec phases

---

## 🃏 Design de Deck Pattern

### Principes de Design

| Principe | Description | Exemple |
|----------|-------------|---------|
| **Prévisibilité** | Pattern se répète, joueur peut anticiper | Attaque → Buff → Attaque |
| **Variété** | Assez de cartes différentes pour ne pas être répétitif | 6-8 cartes minimum |
| **Montée en Puissance** | Cartes plus fortes en fin de pattern | Carte ultime en dernière position |
| **Thématique** | Pattern correspond à l'identité de l'ennemi | Gobelin archer : majorité d'attaques distance |

---

### Exemples de Patterns

**Pattern Agressif (Guerrier):**
1. Frappe Rapide (1 PA, 8 dégâts)
2. Frappe Rapide (1 PA, 8 dégâts)
3. Frappe Puissante (2 PA, 15 dégâts)
4. Bouclier (1 PA, +5 bouclier)
5. Frappe Dévastatrice (3 PA, 25 dégâts)

**Durée du Cycle:** 5 tours, puis recommence

---

**Pattern Support (Chaman):**
1. Éclair (2 PA, 10 dégâts)
2. Soins (2 PA, heal 15 HP)
3. Buff Allié (2 PA, +3 dégâts à tous)
4. Éclair (2 PA, 10 dégâts)
5. Invocation (3 PA, invoque unité)

**Durée du Cycle:** 5 tours, puis recommence

---

**Pattern Boss (Multi-Phase):**
1. Attaque Basique (2 PA, 12 dégâts)
2. Attaque Basique (2 PA, 12 dégâts)
3. Buff Personnel (2 PA, +5 dégâts)
4. Attaque AoE (3 PA, 15 dégâts rayon 2)
5. Attaque Basique (2 PA, 12 dégâts)
6. Attaque Ultime (4 PA, 30 dégâts rayon 3)

**Durée du Cycle:** 6 tours, puis recommence

**Note:** Les patterns complexes peuvent changer selon les HP du boss (phases).

---

## 🎯 Catégories d'Ennemis

### Par Rôle

| Rôle | HP | PA | Style | Cartes Typiques |
|------|----|----|-------|-----------------|
| **Tank** | 60-80 | 2-3 | Défensif, provocation | Bouclier, Taunt, Régénération |
| **DPS** | 40-50 | 3-4 | Offensif, burst | Attaques multiples, Finishers |
| **Support** | 30-40 | 3-4 | Buff/Heal alliés | Soins, Buffs, Invocations |
| **Contrôleur** | 40-50 | 3-4 | Debuff, zone | Stun, Slow, AOE |

---

### Par Élément

| Élément | Caractéristiques | Faiblesses |
|---------|-----------------|------------|
| **Feu** | Dégâts AOE, brûlure | Eau |
| **Ombre** | Drain de vie, debuffs | Lumière |
| **Lumière** | Soins, purification | Ombre |
| **Eau** | Gel, ralentissement | Feu |
| **None** | Équilibré, physique | Aucune |

---

## 🏗️ Création d'Ennemis

### Processus dans Unity

| Étape | Action |
|-------|--------|
| **1. Créer SO** | Clic droit → Enemy/Enemy Data |
| **2. Identité** | Nom, Prefab, Élément |
| **3. Classification** | Cocher Is Boss si boss |
| **4. Stats** | HP, PA, PM, Défenses |
| **5. Deck Pattern** | Glisser-déposer cartes dans Combat Deck (ordre important!) |
| **6. Visuels** | Health Bar Offset, Color |

---

### Checklist de Validation

| Élément | Vérifié |
|---------|---------|
| Prefab assigné | ☐ |
| Élément choisi | ☐ |
| Is Boss configuré correctement | ☐ |
| Stats équilibrées | ☐ |
| Combat Deck de 5-12 cartes | ☐ |
| Pattern teste et équilibré | ☐ |

---

## 🎲 Exemples d'Ennemis

### Ennemi Normal: Gobelin Guerrier

| Attribut | Valeur |
|----------|--------|
| **Nom** | Gobelin Guerrier |
| **Élément** | None |
| **Is Boss** | false |
| **Max Health** | 40 |
| **Movement Range** | 3 |
| **Max Action Points** | 3 |
| **Physical Defense** | 10 |
| **Magical Defense** | 5 |

**Combat Deck (Pattern):**
1. Frappe Rapide (1 PA, 6 dégâts)
2. Frappe Rapide (1 PA, 6 dégâts)
3. Frappe Puissante (2 PA, 12 dégâts)
4. Bouclier (1 PA, +5 bouclier)
5. Charge (2 PA, 15 dégâts + déplacement)

**Stratégie pour Joueur:**
- Pattern prévisible, anticiper Charge au 5e tour
- Éliminer rapidement (HP faibles)
- Dangereux en groupe

---

### Boss: Dragon des Cendres

| Attribut | Valeur |
|----------|--------|
| **Nom** | Dragon des Cendres |
| **Élément** | Feu |
| **Is Boss** | true |
| **Max Health** | 300 |
| **Movement Range** | 2 |
| **Max Action Points** | 4 |
| **Physical Defense** | 20 |
| **Magical Defense** | 15 |

**Combat Deck (Pattern):**
1. Griffe (2 PA, 15 dégâts physiques)
2. Souffle de Feu (3 PA, 20 dégâts feu rayon 3)
3. Griffe (2 PA, 15 dégâts physiques)
4. Vol (2 PA, déplacement 4 cases)
5. Souffle de Feu (3 PA, 20 dégâts feu rayon 3)
6. Météore Enflammé (4 PA, 35 dégâts feu rayon 4)

**Stratégie pour Joueur:**
- Anticiper Météore au 6e tour
- Se disperser avant Souffle de Feu (tours 2 et 5)
- Utiliser cartes Eau pour bonus de dégâts

---

## ⚖️ Équilibrage

### Formules de Base

**HP Ennemi Normal:**
- Formule : 30 + (10 × Tier)
- Tier 1 : 40 HP
- Tier 2 : 50 HP
- Tier 3 : 60 HP

**HP Boss:**
- Formule : 200 + (50 × Chapter)
- Chapitre 1 : 250 HP
- Chapitre 2 : 300 HP
- Chapitre 3 : 350 HP

---

### Budget PA par Pattern

| Type | PA Total Pattern | Tours de Cycle |
|------|------------------|----------------|
| **Normal Faible** | 6-8 PA | 3-4 tours |
| **Normal Moyen** | 8-12 PA | 4-5 tours |
| **Normal Fort** | 12-15 PA | 5-6 tours |
| **Boss** | 18-24 PA | 6-8 tours |

**Principe:** Le total de PA dans le pattern doit être équilibré avec les HP.

---

### Scaling de Difficulté

| Paramètre | Facile | Normal | Difficile |
|-----------|--------|--------|-----------|
| **HP** | -30% | 100% | +50% |
| **Dégâts** | -20% | 100% | +30% |
| **Défenses** | -20% | 100% | +20% |

---

## 📝 Ennemis À Créer

### Priorité Haute
- 5-10 ennemis normaux variés
- 1-2 boss par acte
- Un ennemi de chaque élément
- Représentation de chaque rôle (Tank/DPS/Support)

### Priorité Moyenne
- Variantes d'ennemis existants
- Ennemis élites (mini-boss)
- Ennemis avec mécaniques spéciales

### Priorité Basse
- Boss secrets
- Ennemis saisonniers/événements
- Ennemis légendaires

---

## 🎨 Design d'Ennemis

### Principes de Design

| Principe | Description |
|----------|-------------|
| **Identité Claire** | Apparence et pattern cohérents |
| **Contrepartie** | Forces compensées par faiblesses |
| **Prévisibilité** | Pattern lisible, joueur peut stratégiser |
| **Challenge** | Doit forcer adaptation tactique |

---

### Thématiques Recommandées

| Thème | Style | Exemples |
|-------|-------|----------|
| **Gobelins** | Agressif, nombreux | Guerrier, Archer, Chaman |
| **Morts-Vivants** | Lent, régénération | Squelette, Zombie, Nécromancien |
| **Élémentaires** | Spécialisés élément | Feu, Eau, Air, Terre |
| **Dragons** | Boss, puissants | Dragon Rouge, Noir, Bleu |
| **Démons** | Corruptions, drain | Succube, Démon Majeur |

---

**Dernière mise à jour:** 11 Janvier 2026
**Version:** 2.0
**Responsable:** Design Ennemis Project TDB

**Documents Connexes:**
- [Combat_System.md](Combat_System.md) - Système de combat
- [Card_System.md](Card_System.md) - Cartes utilisées par ennemis
- [GDD_Main.md](GDD_Main.md) - Vision globale
