# ILYA - « Le Dévoué Enchaîné »

**Statut :** Fiche de référence d'Ilya (source unique de vérité pour ses stats, son système Rage et son deck — voir `GDD_Main.md`). Mise à jour le 23/09/2026 : la liste détaillée des 12 cartes et les spécifications Rage, auparavant dans `claude_md_coarchitect.md`, ont été déplacées ici.
**Place dans le MVP :** **hors MVP** (roster MVP = Evan, Crux, Raze — voir `GDD_Main.md`).

> ⚠️ **Conçu avant l'Excel MVP** (`TCG_Tactique_Systeme_de_calcul.xlsx`). Plusieurs éléments ne respectent plus les règles actuelles et devront être adaptés s'il revient :
> - stats ATK/DEF (le système ne définit que PV, PA, PM) et PA/PM qui changent avec la forme (le budget PA+PM est fixé à 9 par profil) ;
> - la Rage devrait devenir une variante de l'**Éveil Colère** ;
> - deck de 12 cartes Personnage/Famille/Neutre → format **24 cartes** (2 Signature + 6 Éveil + 16 Standard) ;
> - dégâts des cartes à recalculer avec le budget (ex : 1 PA = 12 dégâts de base, pas 20).
>
> ⚠️ **Une autre version d'Ilya a existé dans le code** (`IlyaUnit`, retiré le 24/09/2026, récupérable via le commit `00afe5d`) : 1 carte Rage ajoutée à la **main** tous les **10** dégâts subis ou PV payés, stock max 5, et des cartes différentes (Coup Déchaîné, Défi du Colosse, Exutoire Brutal, Frappe Téméraire, Hurlement de Guerre, Mouvement Forcé, Saignée Volontaire, Second Souffle, Soif de Sang, Tourbillon Sanglant). À réconcilier avec ce document s'il revient.

**Famille** : 🔴 Déchaînés — Colère *(appelée « Rouge (Incarnat) » dans les tout premiers brouillons — même personnage)*
**Concept** : « Le Dévoué qui se sacrifie par amour, mais enchaîne sa colère »

> Note (10/09/2026) : pas de classe formelle. Ilya reste conceptuellement un tank protecteur, décrit par sa mécanique signature (Rage).

---

## STATS DE BASE (niveau 1)

### Forme Enchaînée (Défensive)
- **PV** : 100
- **PA/tour** : 3
- **Mouvement** : 3 hex
- **ATK** : 15 | **DEF** : 10
- Gameplay : tank, taunt, protège les alliés

### Forme Déchaînée (Offensive)
- **PA/tour** : 4 (+1)
- **Mouvement** : 4 hex (+1)
- **ATK** : 25 (+10) | **DEF** : 5 (-5)
- **Lifesteal** : 25 % sur toutes les attaques
- **Perte** : -10 PV/tour (doit attaquer pour survivre)
- **Durée** : 3 tours max, puis retour forcé en forme Enchaînée

> L'ancienne table de progression par niveau et les talents d'Ilya sont archivés (`archive/Concepts_Abandonnes.md`) : ils contredisent la règle actuelle « le niveau ne change ni les PA/PM ni la puissance des cartes ».

---

## SYSTÈME RAGE

### Génération
- 20 dégâts subis → +1 carte Rage ajoutée au deck (mélangée)
- Limite : 6 à 8 Rages max dans le deck (à fixer lors des tests)
- Les Rages sont des cartes normales, piochables

### Utilisation (2 modes)
**Mode 1 — Remplir la jauge de transformation**
- Jouer 1 carte Rage = 1 PA → +1 jauge (0→5)
- À 5 → la carte Chaînes Brisées devient jouable

**Mode 2 — Booster d'autres cartes**
- Certaines cartes ont un effet bonus si on dépense des Rages
- 0 PA quand la Rage est utilisée comme boost
- Coûts variables : 1, 2 ou 3 Rages selon le boost (ordre de grandeur : +20 dégâts par Rage, à équilibrer carte par carte)

### Transformation (Chaînes Brisées)
- Condition : jauge à 5 (consommée, remise à 0)
- Effet d'activation : +20 PV + AOE rayon 2 (30 dégâts)
- Retire TOUTES les Rages du deck et de la main
- Coût en PA de la carte Chaînes Brisées : à définir (0 PA proposé, la jauge étant déjà le coût)

### Cartes « Fetch Rage »
Cartes qui piochent des Rages du deck, pour éviter la dilution :
- Canaliser Colère (dans le deck de départ) : 1 PA → pioche 2 Rages
- Rage Intérieure (hors deck de départ, à obtenir en récompense) : 0 PA → pioche 1 Rage (ou en génère 1 si le deck n'en contient pas)

**Décision tactique clé** : transformer (burst) ou booster des cartes (dégâts réguliers).

---

## DECK DE DÉPART (12 cartes)

**Structure : 7 Personnage + 3 Famille + 2 Neutres**

### Cartes Personnage (7)
1. **Dévotion** — 2 PA : Taunt 2 tours, +20 DEF
2. **Frappe Enchaînée** — 2 PA : attaque mêlée ; si Ilya a été touché avant → +50 % dégâts
3. **Chaînes Brisées** — jauge Rage 5 : Transformation
4. **Canaliser Colère** — 1 PA : pioche 2 Rages du deck
5. **Brasier Intérieur** — 2 PA : +30 % ATK pendant 2 tours
6. **Lame Ardente** — 3 PA : 30 dégâts + Brûlure (5 dégâts/tour × 2 tours)
7. **Passion Sacrificielle** — 2 PA : soigne un allié de 30 PV, Ilya perd 15 PV

### Cartes Famille — Déchaînés (3)
8. **Garde Inébranlable** — 1 PA : mouvement + +50 DEF jusqu'au prochain tour
9. **Riposte** — 2 PA : attaque faible ; contre-attaque si Ilya est touché ce tour *(nécessite le système de réactions, voir `Combat_System.md`)*
10. **Mur Vivant** — 2 PA : Taunt sur 3 hex autour d'Ilya

### Cartes Neutres (2)
11. **Sprint** — 1 PA : +2 PM ce tour
12. **Frappe Basique** — 1 PA : 20 dégâts, portée 1

---

## CONCEPT GAMEPLAY

**Thématique** : Ilya protège ses alliés par amour et dévouement. Il enchaîne sa colère pour ne pas blesser ceux qu'il aime. Quand il accumule trop de Rage (dégâts subis), il brise ses chaînes et libère sa fureur dans une forme berserker.

**Gameplay clé** :
- Forme Enchaînée : défensif, tanke, génère de la Rage
- Forme Déchaînée : offensif, burst, lifesteal
- Décision tactique : transformer (burst) ou booster des cartes (régulier)

---

**Dernière mise à jour :** 23 Septembre 2026
