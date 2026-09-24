# Copie texte de l'Excel MVP — `TCG_Tactique_Systeme_de_calcul.xlsx`

> **Copie du 23/09/2026, pour que Claude Code puisse lire le MVP sans le fichier Excel.** L'Excel (dans le projet claude.ai) reste la référence : si tu le modifies, mets à jour cette copie (ou remplace-la en ajoutant le `.xlsx` dans `Docs/`). Les cellules calculées par formule (valeurs finales des cartes, PV par niveau, barème) ne sont pas reprises ici : seules les entrées et les règles le sont.

---

## Principe

**Valeur finale d'une carte = Baseline(coût en PA) × (1 + somme des modificateurs)**

- Plus une carte a de portée / zone / ligne de vue / contrôle, moins elle fait de dégâts bruts.
- L'Émotion (Éveil) se gagne en jouant certaines cartes et se dépense sur des cartes « ultimes ».
- Le niveau des champions ne doit **jamais** augmenter la puissance des cartes : il augmente les PV, débloque des slots — jamais les stats des cartes.

---

## Références (onglet « Références »)

**A. Baseline par coût en PA** (mêlée, cible unique, sans statut)

| PA | 1 | 2 | 3 | 4 | 5 | 6 |
|----|---|---|---|---|---|---|
| Dégâts / soin | 12 | 26 | 42 | 60 | 80 | 102 |

**B. Portée** : 1 (Mêlée) 0 · 1 à 3 cases -0.15 · 1 à 5 cases -0.25 · 1 à 6 cases -0.35

**C. Zone** : Cible unique 0 · Ligne (2-3 cases) -0.2 · Cône (3 cases) -0.25 · Cercle rayon 1 (9 cases) -0.35 · Cercle rayon 2 (25 cases) -0.5 · Équipe entière (sans portée ni ligne de vue) -0.65 · 3 cibles séparées -0.4 · Contagion (se propage aux cibles à 2 cases ou moins) -0.5 · 2 cibles séparées -0.25

**D. Ligne de vue** : Requise 0 · Non requise -0.15

**E. Statut** : Aucun 0 · -1 PM au prochain tour -0.1 · -2 PM -0.3 · -3 PM -0.35 · Perte totale de PM -1

**F. Déplacement forcé (par case)** : Aucun 0 · Poussée / Tirage -0.08 · Téléportation -0.15

**G. Émotion (Éveil)** : Aucune 0 · Génère -0.1 · Consomme 1 palier +0.3 · 2 paliers +0.5 · 3 paliers (max) +0.7

**H. Identité** : Colère = agressif · Peur = contrôle · Joie = soin / valeur · Neutre = Signature, jouable quelles que soient les émotions du deck

**I. Type de carte** : Standard (PA seuls) · Éveil (seuil d'Émotion) · Signature (fixe, liée au personnage)

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
| Piège et recul | 3 | Mêlée | Unique | -1 PM | Gén. | Dégâts | Élan +2 PM | Dégâts | |
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
| Écho de Lyse | Soren | 1 | 1-3 | Unique | Repositionne Lyse (ou une autre invocation) jusqu'à 3 cases |
| Invocation de Lyse | Soren | 2 | 1-3 | Unique | Invoque Lyse (PV = moitié des PV actuels de Soren, recalculés en continu) |
| Piolet d'ascension | L'Alpiniste | 2 | 1-5 | Unique | Grappin adjacent à une unité, déclenche le Réflexe du grimpeur |
| Corde de rappel forcé | L'Alpiniste | 4 | 1-3 | 2 cibles | Ennemi : 35 dégâts + tiré de 2 cases ; allié : tiré de 2 cases sans dégâts |
| Il triche | Ace | 1 | Mêlée | Unique | Modifie de ±1 le coût d'une carte en main (min 1) |
| Tapis | Ace | 5 | Mêlée | 2 cibles | 72 dégâts, +10 par PA déjà dépensé ce tour (à retravailler) |

---

## Suivi de deck

Cible : **24 cartes** = 2 Signature + 6 Éveil + 16 Standard.

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

**Actées :** budget PA+PM de 9 · grille « hexagonale façon Waven » *(⚠️ contredit par le Lisez-moi et le code : grille carrée)* · émotions de lancement Colère / Peur / Joie · decks mono ou bi-émotion, plusieurs decks par personnage · 24 cartes (2/6/16) · Éveil (concept ; mise en œuvre repoussée) · règle anti-lock · triangle Colère = burst sans sustain, Peur = contrôle/tempo, Joie = survie/lent · monstres de donjon (groupe) vs d'aventure (solo) · pool Standard « 36 cartes » *(⚠️ la bibliothèque en contient 49)* · 3 champions complets (Soren, l'Alpiniste, Ace) · montée en niveau (XP 100 × niveau, XP monstre = 15 % des PV).

**En attente :** équipement · système de stats (armure, résistances, critique) · oppositions d'émotions (repoussé) · faiblesses émotionnelles des monstres · cartes bi-émotion dédiées · main/pioche définitive (playtest : main de 3 + repioche à 3) · rythme de l'Éveil (base : 2 points par palier, jauge par émotion, les Signatures génèrent au choix).

---

## Champions

**Soren** — a perdu sa sœur jumelle Lyse. Passif *Miroir fraternel* : quand Soren joue une carte offensive, une invocation active ayant une cible valide peut rejouer un écho à ~40 % sur n'importe quel ennemi. Les PV de Lyse = moitié des PV actuels de Soren.

**L'Alpiniste** — accident de cordée filmé, confiance brisée. Passif *Réflexe du grimpeur* : après un déplacement rapide vers une unité, adjacent à un allié → -15 % aux prochains dégâts subis ; adjacent à un ennemi → +15 % de dégâts sur la prochaine carte.

**Ace** — a tout perdu sur une main légendaire. Passif *Main gagnante* (provisoire) : Paire (2 cartes de même coût) → la 2ᵉ ignore les réductions de dégâts en % ; Suite (N puis N+1) → +1 PA ; Bluff (2 émotions différentes, bi-émotion uniquement) → -10 % aux prochains dégâts subis. Un seul motif par tour (Bluff > Suite > Paire).

*(Détails : `CHAMPIONS_CONCEPTS.md`.)*
