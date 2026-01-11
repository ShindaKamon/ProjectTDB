# 👹 Ennemis et Boss - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## 🎯 Vue d'Ensemble

Les ennemis de **Project TDB** sont conçus pour challenger les joueurs avec des patterns tactiques variés, forçant l'adaptation et la réflexion stratégique. Chaque type d'ennemi a des forces, faiblesses et comportements distincts.

---

## 🗡️ Ennemis Communs

### Soldat Gobelin (Tier 1)

**Apparence:**
- Petit gobelin avec armure de cuir
- Épée courte ou lance
- Bouclier en bois

**Statistiques:**
```
HP:     25   ██████░░░░
Armure: 2    ████░░░░░░
PA:     3    ██████░░░░
PM:     3    ██████░░░░
Vitesse: 6   ██████░░░░
```

**Comportement IA:**
- **Type:** Agressif
- **Priorité:** Attaque le personnage le plus proche
- **Mouvement:** Se rapproche s'il n'est pas à portée
- **Capacités:**
  - Coup d'Épée (1 PA) : 5 dégâts physiques, portée mêlée
  - Charge (2 PA) : Se déplace de 2 cases et attaque pour 6 dégâts

**Stratégie du Joueur:**
- Facile à vaincre individuellement
- Dangereux en groupe
- Faible défense, cible prioritaire pour AoE

**Récompenses:**
- 15 XP
- 10-15 Or

---

### Archer Gobelin (Tier 1)

**Apparence:**
- Gobelin avec arc court
- Cape verte
- Carquois

**Statistiques:**
```
HP:     20   ████░░░░░░
Armure: 1    ██░░░░░░░░
PA:     3    ██████░░░░
PM:     2    ████░░░░░░
Vitesse: 5   █████░░░░░
```

**Comportement IA:**
- **Type:** Défensif
- **Priorité:** Reste à distance, cible les personnages à faible HP
- **Mouvement:** Recule si un ennemi s'approche
- **Capacités:**
  - Tir à l'Arc (1 PA) : 4 dégâts physiques, portée 5
  - Flèche Empoisonnée (2 PA) : 3 dégâts + Poison (1 stack, 2 tours)
  - Retraite (1 PA) : Recule de 2 cases

**Stratégie du Joueur:**
- Très fragile
- Utiliser la mobilité pour se rapprocher
- Bloquer la ligne de vue avec obstacles

**Récompenses:**
- 15 XP
- 10-15 Or

---

### Chaman Gobelin (Tier 2)

**Apparence:**
- Gobelin avec bâton totémique
- Masque tribal
- Robe ornée

**Statistiques:**
```
HP:     35   ███████░░░
Armure: 3    ███░░░░░░░
PA:     4    ████░░░░░░
PM:     2    ████░░░░░░
Vitesse: 4   ████░░░░░░
```

**Comportement IA:**
- **Type:** Support
- **Priorité:** Buff les alliés, soigne les blessés
- **Mouvement:** Reste au milieu du groupe
- **Capacités:**
  - Éclair Mineur (2 PA) : 6 dégâts magiques, portée 4
  - Soins Tribaux (2 PA) : Restaure 10 HP à un allié, portée 3
  - Cri de Guerre (1 PA) : Tous les alliés gagnent +2 dégâts pour 2 tours

**Stratégie du Joueur:**
- **CIBLE PRIORITAIRE** : Doit être éliminé rapidement
- Transforme les combats faciles en combats difficiles
- Faible mobilité, utiliser le focus burst

**Récompenses:**
- 30 XP
- 20-30 Or

---

### Brute Orque (Tier 2)

**Apparence:**
- Grand orque musclé
- Hache à deux mains
- Armure lourde

**Statistiques:**
```
HP:     60   ██████████
Armure: 6    ██████░░░░
PA:     3    ██████░░░░
PM:     2    ████░░░░░░
Vitesse: 3   ███░░░░░░░
```

**Comportement IA:**
- **Type:** Agressif
- **Priorité:** Attaque le personnage avec le plus de HP
- **Mouvement:** Avance lentement mais sûrement
- **Capacités:**
  - Frappe de Hache (2 PA) : 12 dégâts physiques, portée mêlée
  - Coup Écrasant (3 PA) : 18 dégâts + Push 1 case
  - Hurlement (1 PA) : Provocation (force les ennemis adjacents à l'attaquer)

**Stratégie du Joueur:**
- Tank ennemi, difficile à tuer rapidement
- Kite avec mobilité et attaques à distance
- Utiliser dégâts magiques (ignore partiellement l'armure)

**Récompenses:**
- 40 XP
- 30-40 Or

---

### Nécromancien Squelette (Tier 3)

**Apparence:**
- Squelette en robe noire
- Bâton avec crâne
- Aura sombre

**Statistiques:**
```
HP:     45   █████████░
Armure: 2    ██░░░░░░░░
PA:     5    ██████████
PM:     2    ████░░░░░░
Vitesse: 5   █████░░░░░
```

**Comportement IA:**
- **Type:** Tactique
- **Priorité:** Invoque des sbires, attaque à distance
- **Mouvement:** Reste en arrière, se téléporte si menacé
- **Capacités:**
  - Rayon Nécrotique (2 PA) : 8 dégâts magiques, portée 5
  - Invocation de Squelettes (3 PA) : Invoque 2 Squelettes Guerriers
  - Drain de Vie (3 PA) : 10 dégâts, restaure 5 HP
  - Téléportation d'Ombre (2 PA) : Se téléporte jusqu'à 4 cases

**Stratégie du Joueur:**
- **TRÈS PRIORITAIRE** : Invocations rendent le combat ingérable
- Utiliser le contrôle de foule (Stun, Gel)
- Focus burst avant qu'il n'invoque trop
- Éliminer les invocations avec AoE

**Récompenses:**
- 60 XP
- 50-70 Or

---

### Élémentaire de Feu (Tier 3)

**Apparence:**
- Forme humanoïde de flammes
- Yeux de braise
- Aura de chaleur

**Statistiques:**
```
HP:     50   ██████████
Armure: 0    ░░░░░░░░░░
PA:     4    ████░░░░░░
PM:     3    ██████░░░░
Vitesse: 6   ██████░░░░
```

**Résistances:**
- **Feu:** Immune
- **Glace:** Faiblesse (×2 dégâts)
- **Physique:** Résistance (-50% dégâts)

**Comportement IA:**
- **Type:** Agressif / AoE
- **Priorité:** Groupes de personnages proches
- **Mouvement:** Se rapproche pour maximiser l'AoE
- **Capacités:**
  - Boule de Feu (2 PA) : 10 dégâts feu, rayon 2
  - Nova de Flammes (3 PA) : 12 dégâts feu AoE autour de lui, rayon 2
  - Immolation (Passif) : 3 dégâts feu aux ennemis adjacents à la fin de son tour

**Stratégie du Joueur:**
- Utiliser dégâts de glace (double dégâts)
- Éviter de se regrouper (AoE)
- Attaques à distance recommandées

**Récompenses:**
- 70 XP
- 60-80 Or

---

## 👑 Boss

### Warlord Grakk - Chef des Gobelins (Boss Acte 1)

**Apparence:**
- Gobelin massif (2× taille normale)
- Armure d'os et de métal
- Grande hache enchantée
- Cape de fourrure

**Statistiques:**
```
HP:     200  ██████████ (Barre de vie en 2 phases)
Armure: 8    ████████░░
PA:     5    ██████████
PM:     3    ██████░░░░
Vitesse: 7   ███████░░░
```

**Phases du Combat:**

**Phase 1 (100% → 50% HP):**
- **Comportement:** Agressif, attaque frontale
- **Capacités:**
  - Frappe du Warlord (2 PA) : 15 dégâts physiques, portée mêlée
  - Lancer de Hache (3 PA) : 12 dégâts en ligne, portée 5, traverse les ennemis
  - Cri de Ralliement (2 PA) : Invoque 2 Soldats Gobelins
  - Charge Brutale (3 PA) : Se déplace de 3 cases, 18 dégâts + Stun 1 tour

**Phase 2 (< 50% HP):**
- **Trigger:** Grakk hurle de rage, aura rouge apparaît
- **Changements:**
  - +50% dégâts sur toutes les attaques
  - +1 PM
  - Nouvelles capacités débloquées
- **Nouvelles Capacités:**
  - Tourbillon de Rage (4 PA) : 20 dégâts AoE rayon 2, +1 Rage par ennemi touché
  - Frappe Dévastatrice (5 PA) : 30 dégâts, -5 Armure à la cible pour 2 tours

**Mécanique Spéciale:**
- **Rage du Chef** : À chaque Soldat Gobelin vaincu, Grakk gagne +2 dégâts permanent
- **Invocations:** Grakk invoque 2 Soldats tous les 3 tours

**Stratégie du Joueur:**
1. Éliminer rapidement les invocations pour limiter les bonus de Grakk
2. Utiliser le contrôle pour éviter la Charge Brutale
3. En Phase 2, focus burst pour terminer rapidement
4. Kael peut tanker la plupart des attaques
5. Ayla doit AoE les invocations

**Récompenses:**
- 200 XP
- 200 Or
- 1 Carte Rare garantie (choix parmi 3)
- Clé de l'Acte 2

---

### L'Archimage Corrompu - Zephyros (Boss Acte 2)

**Apparence:**
- Mage humain vieilli
- Robe pourpre corrompue
- Bâton flottant de runes noires
- Aura de magie sombre

**Statistiques:**
```
HP:     150  ██████████ (Santé plus faible mais mécanique défensive)
Armure: 3    ███░░░░░░░
PA:     6    ██████████
PM:     2    ████░░░░░░
Vitesse: 5   █████░░░░░
```

**Mécanique Unique: Boucliers Élémentaires**

Zephyros possède 3 Boucliers Élémentaires qui orbitent autour de lui:
- **Bouclier de Feu** : Absorbe 30 HP de dégâts de feu
- **Bouclier de Glace** : Absorbe 30 HP de dégâts de glace
- **Bouclier de Foudre** : Absorbe 30 HP de dégâts de foudre

**Règle:** Les dégâts du type correspondant détruisent le bouclier. Les autres types de dégâts sont réduits de 50%.

**Phases du Combat:**

**Phase 1 (3 Boucliers actifs):**
- **Comportement:** Défensif, reste à distance
- **Capacités:**
  - Projectile Élémentaire (2 PA) : 8 dégâts (type aléatoire), portée infinie
  - Invocation Élémentaire (3 PA) : Invoque 1 Élémentaire aléatoire (Feu, Glace, Foudre)
  - Nova Magique (4 PA) : 10 dégâts AoE rayon 3, type = bouclier actif

**Phase 2 (1-2 Boucliers actifs):**
- **Comportement:** Agressif, sorts plus puissants
- **Capacités supplémentaires:**
  - Chaîne d'Éclairs (4 PA) : 15 dégâts à 3 cibles, portée 6
  - Téléportation Arcane (2 PA) : Se téléporte, laisse une zone de dégâts à l'ancien emplacement

**Phase 3 (Tous boucliers détruits):**
- **Trigger:** Zephyros crie "ASSEZ!", explosion magique
- **Changements:**
  - Gagne 50 HP de Bouclier permanent (non-élémentaire)
  - Tous les sorts coûtent -1 PA
  - Dégâts augmentés de 50%
- **Capacité Ultime:**
  - Météore Apocalyptique (6 PA) : 25 dégâts AoE rayon 4, crée zones de feu durables

**Stratégie du Joueur:**
1. Diversifier les types de dégâts pour détruire les boucliers
2. Éliminer les Élémentaires invoqués rapidement
3. En Phase 3, burst maximal avant le Météore
4. Ayla excelle dans ce combat (variété élémentaire)
5. Utiliser la mobilité pour éviter les AoE

**Récompenses:**
- 300 XP
- 400 Or
- 1 Carte Épique garantie (choix parmi 3)
- Déblocage de la zone de l'Acte 3

---

### Le Roi Liche - Malachar (Boss Final Acte 3)

**Apparence:**
- Squelette en armure de liche
- Couronne de fer rouillé
- Cape éthérée
- Épée maudite et grimoire

**Statistiques:**
```
HP:     300  ██████████ (Combat en 3 phases)
Armure: 10   ██████████
PA:     7    ██████████
PM:     3    ██████░░░░
Vitesse: 6   ██████░░░░
```

**Immunités:**
- Poison (Mort-vivant)
- Drain de Vie (Pas de vie à drainer)

**Phases du Combat:**

**Phase 1 (100% → 66% HP) - Le Nécromancien:**
- **Focus:** Invocations et magie nécrotique
- **Capacités:**
  - Rayon de Mort (3 PA) : 12 dégâts magiques, portée infinie, ignore 50% de l'Armure
  - Armée des Morts (4 PA) : Invoque 3 Squelettes Guerriers
  - Malédiction (3 PA) : -30% dégâts infligés à tous les ennemis pour 3 tours
  - Drain d'Âme (4 PA) : 15 dégâts, Malachar gagne 15 HP

**Phase 2 (66% → 33% HP) - Le Guerrier Maudit:**
- **Trigger:** Malachar dégaine son épée maudite
- **Changements:**
  - +2 PM
  - Passe en combat de mêlée
  - Nouvelle barre d'Armure (+10 Armure)
- **Capacités:**
  - Frappe Maudite (3 PA) : 18 dégâts physiques + Malédiction (réduction de soins 50%)
  - Charge Fantomatique (3 PA) : Téléportation jusqu'à 5 cases, 20 dégâts à l'arrivée
  - Aura de Terreur (Passif) : -1 PA à tous les ennemis dans un rayon de 3 cases
  - Invocation Réduite (3 PA) : Invoque 2 Squelettes

**Phase 3 (< 33% HP) - Forme Finale:**
- **Trigger:** Malachar fusionne avec son trône, transformation
- **Changements:**
  - Immobile (0 PM) mais portée infinie sur tous les sorts
  - Bouclier régénératif (+20 Bouclier par tour)
  - Double Actions (peut jouer 2 tours de suite tous les 3 tours)
- **Capacités:**
  - Tsunami Nécrotique (5 PA) : 20 dégâts magiques AoE sur TOUTE la grille
  - Résurrection des Héros (6 PA) : Invoque 2 Champions Squelettes (mini-boss)
  - Griffes de l'Au-Delà (4 PA) : 25 dégâts à une cible, Stun 1 tour
  - Renaissance (Passif) : À 0 HP, ressuscite une fois avec 50 HP (peut être empêché avec damage burst)

**Mécanique Spéciale: Les Quatre Phylactères**

Au début du combat, 4 Phylactères apparaissent aux coins de la grille.
- **HP:** 20 chacun
- **Effet:** Tant qu'un Phylactère est actif, Malachar régénère 10 HP par tour
- **Stratégie:** Détruire les Phylactères est optionnel mais recommandé

**Stratégie du Joueur:**
1. **Début:** Décider si on détruit les Phylactères (ralentit mais facilite)
2. **Phase 1:** Focus sur les invocations, burst Malachar
3. **Phase 2:** Kite ou tanker selon la composition, attention à l'Aura de Terreur
4. **Phase 3:** BURST MAXIMAL, éliminer les Champions immédiatement
5. **Équipe recommandée:** 4 personnages, composition équilibrée

**Récompenses:**
- 500 XP
- 1000 Or
- 1 Carte Légendaire (choix parmi 3)
- Déblocage du Mode Difficile
- Fin de la Campagne Principale

---

## 🎲 Patterns d'IA

### Types de Comportement

**Agressif:**
- Attaque la cible la plus proche ou la plus faible
- Utilise toutes les ressources pour maximiser les dégâts
- Ignore la défense personnelle

**Défensif:**
- Reste à distance
- Utilise les boucliers et soins
- Recule quand menacé

**Tactique:**
- Évalue la situation avant d'agir
- Utilise le terrain à son avantage
- Priorise les cibles selon le contexte

**Support:**
- Aide les alliés (buffs, soins)
- Évite le combat direct
- Cible prioritaire pour le joueur

---

## 📊 Scaling de Difficulté

### Formule de Scaling

**HP Ennemis:**
```
HP Scaled = HP Base × (1 + 0.1 × Niveau du Joueur)
```

**Dégâts Ennemis:**
```
Dégâts Scaled = Dégâts Base × (1 + 0.08 × Niveau du Joueur)
```

**XP et Or:**
```
XP/Or Scaled = Valeur Base × (1 + 0.05 × Niveau du Joueur)
```

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Design Ennemis Project TDB
