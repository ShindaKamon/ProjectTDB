# Chiffres du MVP

> **Référence chiffrée du MVP depuis le 07/10/2026.** Ce document est né le 23/09/2026 comme copie texte de l'Excel `TCG_Tactique_Systeme_de_calcul.xlsx` (projet claude.ai). L'Excel n'est plus tenu à jour : **c'est ce fichier qui fait foi**, et c'est ici qu'on reporte les chiffres (playtest, refontes de cartes). Les valeurs finales des cartes ne sont pas recopiées : elles se calculent avec la formule ci-dessous et vivent dans les assets des cartes.
>
> ⚠️ **Pas encore reporté ici** (décidé dans `GDD_Main.md` § « Décisions actées », les assets font foi en attendant) : refontes et renommages de cartes du 29/09 (pool de 51 Standard, `Card_System.md`) ; ajustements d'identité des émotions (29/09) ; refontes de la passe de diversité (Rage dévastatrice, Effroi partagé…) ; chiffres du boss et des monstres de l'Orphelinat (`Enemies.md`). La bibliothèque ci-dessous reste donc celle de l'Excel au 30/09.

---

## Principe

**Valeur finale d'une carte = Baseline(coût en PA) × (1 + somme des modificateurs)**

- Plus une carte a de portée / zone / ligne de vue / contrôle, moins elle fait de dégâts bruts.
- La jauge d'Émotion se remplit en jouant des cartes ; jauge pleine, le champion fusionne avec l'émotion (Éveil, voir `SYSTEME_EMOTIONS.md`). Plus de cartes « ultimes » ni de consommation de paliers (30/09/2026).
- Le niveau des champions ne doit **jamais** augmenter la puissance des cartes : il augmente les PV, débloque des slots — jamais les stats des cartes.

---

## Références (onglet « Références »)

**A. Baseline par coût en PA** (mêlée, cible unique, sans statut)

| PA | 1 | 2 | 3 | 4 | 5 | 6 |
|----|---|---|---|---|---|---|
| Dégâts / soin | 12 | 26 | 42 | 60 | 80 | 102 |

**B. Portée** : 1 (Mêlée) 0 · 1 à 3 cases -0.15 · 1 à 5 cases -0.25 · 1 à 6 cases -0.35

**C. Zone** (recalculée le 07/10/2026 pour la grille en 4 directions) :

| Zone | Cases | Modificateur |
|------|-------|--------------|
| Cible unique | 1 | 0 |
| Ligne (2-3 cases) | 2-3 | -0.2 |
| Cône rayon 2 | 4 | -0.25 |
| Cercle rayon 1 | 5 | **-0.25** *(était -0.35 pour 9 cases)* |
| Cône rayon 3 | 9 | **-0.35** *(était compté comme un cône de 3 cases, -0.25)* |
| Croix rayon 2 | 9 | -0.35 |
| Cercle rayon 2 | 13 | **-0.4** *(était -0.5 pour 25 cases)* |
| 2 cibles séparées | — | -0.25 |
| 3 cibles séparées | — | -0.4 |
| Contagion (se propage aux cibles à 2 cases ou moins) | — | -0.5 |
| Équipe entière (sans portée ni ligne de vue) | — | -0.65 |

**Comment les zones sont calculées.** L'Excel supposait une grille en 8 directions, où un cercle de rayon 1 fait 9 cases et un cercle de rayon 2 en fait 25. En 4 directions, ces cercles sont des losanges de **5 et 13 cases** (`CardData.IsInAOEShape`). Les trois zones de l'Excel suivent une courbe régulière selon le nombre de cases *n* : **modificateur ≈ −(0,05 + 0,15 × log₃ n)**. Elle redonne ses valeurs à 0,01 près : ligne de 3 → −0,20, 9 cases → −0,35, 25 cases → −0,50. On applique la même courbe aux formes réelles du code, arrondie à 0,05 : 4 cases → −0,25 ; 5 → −0,25 ; 9 → −0,35 ; 13 → −0,40. Les cibles séparées, la contagion et l'équipe entière ne dépendent pas de la grille : inchangées.

**D. Ligne de vue** : Requise 0 · Non requise -0.15

**E. Statut** : Aucun 0 · -1 PM au prochain tour -0.1 · -2 PM -0.3 · -3 PM -0.35 · Perte totale de PM -1

**F. Déplacement forcé (par case)** : Aucun 0 · Poussée / Tirage -0.08 · Téléportation -0.15

**G. Jauge d'émotion (fusion)** : Aucune 0 · Génère -0.1 *(les options « Consomme N paliers » ont été retirées le 30/09/2026)*

**H. Identité** : Colère = agressif · Peur = contrôle · Joie = soin / valeur · Neutre = Signature, jouable quelles que soient les émotions du deck

**I. Type de carte** : Standard (PA seuls) · Signature (fixe, liée au personnage) *(type « Éveil » retiré le 30/09/2026)*

**J. Type d'effet** : Dégâts 0 · Soin -0.1 · Soin + miroir dégâts -0.5 · Buff/Debuff 0 · Pioche 0

**K. Contrepartie** : Aucune 0 · Auto-dégâts +0.25 · Vulnérabilité sur soi +0.15 · Déplacement forcé aléatoire sur soi +0.2 · Touche aussi les alliés proches +0.3 · Vol de vie -0.2 · Élan tactique (+2 PM ce tour) -0.2 · Bond offensif (jusqu'à 3 cases, dégâts à l'atterrissage) -0.15 · Perd 1 PM au prochain tour +0.15 · Repli automatique (recule de 2 cases) -0.15

**L. Rôle (ratio cible du deck)** : Dégâts/Mouvement 60 % · Soutien/Buffs 20 % · Réaction/Soin 20 %

**Buff/Debuff** : la valeur finale = points à répartir entre intensité et durée (~10 points ≈ +10 % pendant 1 tour).

**Peur** : -1/-2/-3 PM au prochain tour selon le coût. Se cumule avec la poussée/tirage. Les retraits ne se cumulent pas entre eux : un plus fort remplace un plus faible ; un plus faible n'écrase jamais un plus fort en cours. Un monstre dont l'action est bloquée fait quand même son Attaque de base.

---

## Bibliothèque de cartes

Colonnes : coût PA · portée · zone · statut · déplacement forcé · Éveil · effet · contrepartie · rôle. Ligne de vue « Requise » pour toutes. « Gén. » = génère de l'Éveil.

### Colère (17)

| Carte | PA | Portée | Zone | Statut / dépl. | Éveil | Effet | Contrepartie | Rôle | Note |
|-------|----|--------|------|----------------|-------|-------|--------------|------|------|
| Coup de colère | 1 | Mêlée | Unique | — | Gén. | Dégâts | — | Dégâts | Référence 1 PA |
| Rugissement destructeur | 1 | Mêlée | Cercle r1 | — | Gén. | Buff/Debuff | — | Soutien | Réduit l'armure des ennemis proches |
| Frappe rapide | 2 | 1-3 | 2 cibles | — | — | Dégâts | — | Dégâts | |
| Poing ardent | 2 | Mêlée | Cercle r1 | — | Gén. | Dégâts | Touche les alliés | Dégâts | Frappe aveugle |
| Montée d'adrénaline | 2 | Mêlée | Unique | — | Gén. | Buff/Debuff | — | Soutien | Bonus dégâts prochaine carte |
| Sang bouillonnant | 2 | Mêlée | Unique | — | Gén. | Dégâts | Vol de vie | Réaction/Soin | |
| Armure de rage | 2 | Mêlée | Unique | — | Gén. | Buff/Debuff | — | Réaction/Soin | Bouclier sur soi |
| Charge brutale | 3 | Mêlée | Unique | Poussée 1 | — | Dégâts | — | Dégâts | |
| Frénésie incontrôlée | 3 | Mêlée | Unique | — | Gén. | Dégâts | Auto-dégâts | Dégâts | |
| Jet de rage | 3 | 1-6 | Unique | — | Gén. | Dégâts | — | Dégâts | Très longue portée |
| Bond percutant | 3 | Mêlée | Cercle r1 | — | Gén. | Dégâts | Bond offensif | Dégâts | Zone à l'atterrissage |
| Balayage furieux | 4 | Mêlée | Cercle r1 | — | Gén. | Dégâts | — | Dégâts | Référence 4 PA (zone) |
| Éclat de rage | 4 | 1-5 | Ligne | — | Gén. | Dégâts | — | Dégâts | |
| Explosion de rage | 5 | 1-3 | Cercle r1 | — | Gén. | Dégâts | — | Dégâts | |
| Déferlante | 5 | Mêlée | Cercle r2 | — | Gén. | Dégâts | — | Dégâts | |
| Rage dévastatrice | 6 | Mêlée | Unique | — | Gén. | Dégâts | — | Dégâts | Finisher |
| Rage totale | 6 | Mêlée | Unique | — | Gén. | Dégâts | Auto-dégâts | Dégâts | Finisher, gros recul |

### Peur (17)

| Carte | PA | Portée | Zone | Statut / dépl. | Éveil | Effet | Contrepartie | Rôle | Note |
|-------|----|--------|------|----------------|-------|-------|--------------|------|------|
| Piqûre d'angoisse | 1 | Mêlée | Unique | -1 PM | Gén. | Dégâts | — | Dégâts | |
| Ombre rampante | 1 | 1-3 | Unique | — | Gén. | Dégâts | — | Dégâts | Poke |
| Voile d'ombre | 1 | Mêlée | Unique | — | Gén. | Buff/Debuff | — | Soutien | Esquive / réduction |
| Frappe hésitante | 2 | Mêlée | 2 cibles | -1 PM | Gén. | Dégâts | — | Dégâts | |
| Aura de terreur | 2 | 1-3 | Unique | — | Gén. | Buff/Debuff | — | Soutien | Réduit les PA de la cible au prochain tour |
| Réflexe de survie | 2 | Mêlée | Unique | — | Gén. | Buff/Debuff | — | Réaction/Soin | Réduit les dégâts si ciblé ce tour |
| Bouclier de la terreur | 2 | Mêlée | Unique | — | Gén. | Buff/Debuff | Perd 1 PM | Réaction/Soin | Gros bouclier |
| Vision cauchemardesque | 3 | 1-5 | Unique | Poussée 2 | — | Dégâts | — | Dégâts | |
| Vertige | 3 | Mêlée | Ligne | -1 PM | Gén. | Dégâts | — | Dégâts | |
| Frappe et repli | 3 | Mêlée | Unique | -1 PM | Gén. | Dégâts | Élan +2 PM | Dégâts | |
| Silence glaçant | 4 | Mêlée | Unique | -2 PM | Gén. | Dégâts | — | Dégâts | |
| Fuite panique | 4 | Mêlée | Unique | -2 PM, poussée 1 | Gén. | Dégâts | Repli auto | Dégâts | |
| Onde de terreur | 4 | 1-3 | Cône | Poussée 1 | Gén. | Dégâts | — | Dégâts | |
| Terreur paralysante | 5 | Mêlée | Unique | Perte totale PM | Gén. | Dégâts | — | Dégâts | Contrôle pur, aucun dégât |
| Regard glaçant | 5 | 1-6 | Cercle r2 | -2 PM | Gén. | Dégâts | — | Dégâts | Contrôle de zone, aucun dégât |
| Cauchemar collectif | 6 | 1-3 | 3 cibles | — | Gén. | Dégâts | — | Dégâts | |
| Effroi partagé | 6 | Mêlée | Contagion | -2 PM | Gén. | Dégâts | — | Dégâts | |

### Joie (15)

| Carte | PA | Portée | Zone | Éveil | Effet | Contrepartie | Rôle | Note |
|-------|----|--------|------|-------|-------|--------------|------|------|
| Souffle apaisant | 1 | Mêlée | Unique | Gén. | Soin | — | Réaction/Soin | |
| Renfort du cœur | 2 | Mêlée | 2 cibles | — | Soin | — | Réaction/Soin | |
| Éclat de joie | 2 | Mêlée | Unique | Gén. | Soin + miroir | — | Réaction/Soin | Un ennemi adjacent subit autant de dégâts |
| Chant d'encouragement | 2 | Mêlée | Cercle r1 | Gén. | Buff/Debuff | — | Soutien | Buff offensif de groupe |
| Élan de joie | 3 | Mêlée | Unique | Gén. | Soin | — | Réaction/Soin | Référence 3 PA |
| Flamme de l'espoir | 3 | Mêlée | Unique | Gén. | Dégâts | — | Dégâts | |
| Lumière bienveillante | 3 | 1-5 | Unique | Gén. | Soin | — | Réaction/Soin | |
| Bouclier bienveillant | 3 | 1-3 | Unique | Gén. | Buff/Debuff | — | Soutien | Bouclier sur un allié |
| Vague de bien-être | 4 | Mêlée | Cercle r1 | Gén. | Soin | — | Réaction/Soin | |
| Onde radieuse | 4 | 1-3 | Ligne | Gén. | Dégâts | — | Dégâts | |
| Euphorie aveuglante | 4 | Mêlée | Unique | Gén. | Soin | Vulnérabilité | Réaction/Soin | |
| Vague de guérison | 5 | 1-3 | Cercle r1 | Gén. | Soin | — | Réaction/Soin | |
| Bénédiction radieuse | 6 | Mêlée | Unique | Gén. | Soin | — | Réaction/Soin | Référence 6 PA |
| Rayonnement de joie | 6 | Mêlée | Cercle r2 | Gén. | Dégâts | — | Dégâts | |
| Communion joyeuse | 6 | Mêlée | Équipe entière | Gén. | Soin | Vulnérabilité | Réaction/Soin | |

### Signatures (identité Neutre)

| Carte | Champion | PA | Portée | Zone | Effet |
|-------|----------|----|--------|------|-------|
| Écho évanescent | Evan | 1 | 1-3 | Unique | Repositionne Lyse (ou une autre invocation) jusqu'à 3 cases |
| Invocation de Lyse | Evan | 2 | 1-3 | Unique | Invoque Lyse (PV = moitié des PV actuels d'Evan, recalculés en continu) |
| Piolet d'ascension | Crux | 2 | 1-5 | Unique | Grappin adjacent à une unité, déclenche le Réflexe du grimpeur |
| Corde de rappel | Crux | 4 | 1-3 | 2 cibles | Ennemi : 35 dégâts + tiré de 2 cases ; allié : tiré de 2 cases sans dégâts |
| Triche | Raze | 1 | Mêlée | Unique | Modifie de ±1 le coût d'une carte en main (min 1) |
| Tapis | Raze | 5 | Mêlée | 2 cibles | 72 dégâts, +10 par PA déjà dépensé ce tour (à retravailler) |

---

## Suivi de deck

Cible : **20 cartes** = 4 Signature (2 exemplaires de chacune des 2 Signatures) + 16 Standard (30/09/2026 ; l'Excel disait 24 = 2/6/16).

---

## Progression champions

- **Règle d'or** : le niveau ne fait progresser que les PV, les passifs et les slots de cartes. PA et PM sont fixés par le profil du personnage.
- **Budget PA + PM = 9** (min 3 PA, min 2 PM). Profils : brutal 6/3, équilibré 5/4, mobile 4/5.
- **PV** : 100 au niveau 1, +15 par niveau. Niveaux 1 à 20.
- **XP d'un monstre** = 15 % de ses PV. XP pour le niveau suivant = 100 × niveau.

---

## Barème monstres (ratios validés par playtest)

- Taille d'équipe de référence : 3
- PV : aventure solo = 1 × PV joueur ; groupe de donjon léger = 3 × PV joueur ; boss = 1.33 × PV de l'équipe ; superboss = 3.3 × PV de l'équipe
- Dégâts/tour : aventure ≈ 15 % des PV joueur ; groupe de donjon ≈ 45 % des PV joueur cumulés ; boss/superboss en cycle de 3 tours : Zone ≈ 25 %, Basique ≈ 30 % des PV joueur, Heal ≈ 30 % des PV du boss par cycle

---

## Roadmap (décisions)

**Actées :** budget PA+PM de 9 · grille carrée 4 directions · émotions de lancement Colère / Peur / Joie · decks mono ou bi-émotion, plusieurs decks par personnage · 20 cartes (4 Signature + 16 Standard) · Éveil = fusion champion × émotion (concept ; pas codé) · règle anti-lock · triangle Colère = burst sans sustain, Peur = contrôle/tempo, Joie = survie/lent · monstres de donjon (groupe) vs d'aventure (solo) · pool Standard « 36 cartes » *(⚠️ la bibliothèque en contient 49)* · 3 champions complets (Evan, Crux, Raze) · montée en niveau (XP 100 × niveau, XP monstre = 15 % des PV).

**En attente :** équipement · système de stats (armure, résistances, critique) · oppositions d'émotions (repoussé) · faiblesses émotionnelles des monstres · cartes bi-émotion dédiées · main/pioche définitive (playtest : main de 3 + repioche à 3) · rythme de l'Éveil (base : 2 points par palier, jauge par émotion, les Signatures génèrent au choix).

---

## Champions

**Evan** — a perdu sa sœur jumelle Lyse. Passif *Miroir fraternel* : quand Evan joue une carte offensive, une invocation active ayant une cible valide peut rejouer un écho à ~40 % sur n'importe quel ennemi. Les PV de Lyse = moitié des PV actuels d'Evan.

**Crux** — accident de cordée filmé, confiance brisée. Passif *Réflexe du grimpeur* : après un déplacement rapide vers une unité, adjacent à un allié → -15 % aux prochains dégâts subis ; adjacent à un ennemi → +15 % de dégâts sur la prochaine carte.

**Raze** — a tout perdu sur une main légendaire. Passif *Main gagnante* (provisoire) : Paire (2 cartes de même coût) → la 2ᵉ ignore les réductions de dégâts en % ; Suite (N puis N+1) → +1 PA ; Bluff (2 émotions différentes, bi-émotion uniquement) → -10 % aux prochains dégâts subis. Un seul motif par tour (Bluff > Suite > Paire).

*(Détails : `CHAMPIONS_CONCEPTS.md`.)*
