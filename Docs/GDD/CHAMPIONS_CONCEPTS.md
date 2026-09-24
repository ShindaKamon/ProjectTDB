# 👥 CONCEPTS DE CHAMPIONS - Émotions Tactics

**Version :** 5.0
**Date :** 23 Septembre 2026
**Changements :**
- v4.0 (10/09/2026) : dimension Classe retirée de la méthodologie.
- v5.0 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`, onglet « Champions ») — roster **Soren, l'Alpiniste, Ace**. Ilya et les Jumeaux passent hors MVP.

> Les valeurs chiffrées (coûts, points de budget, pourcentages) font foi dans l'Excel. Ce document résume.

---

## 🎯 MÉTHODOLOGIE

Chaque champion =
1. **Un trauma** (son histoire) qui justifie sa mécanique
2. **Un passif** (sa mécanique signature)
3. **2 cartes Signature** : un « cœur de gameplay » et un « coup de maître » ou utilitaire
4. **Un profil PA/PM** (budget total de 9 points, voir `Combat_System.md`)

Les cartes Signature ont l'identité **Neutre** : un champion peut jouer un deck de n'importe quelle émotion (mono ou bi-émotion). Chaque champion doit jouer sur **un axe de gameplay distinct** des autres.

---

## 🎮 ROSTER MVP

### 1. SOREN — l'écho d'invocation

**Trauma :** a perdu sa sœur jumelle, Lyse, dont la présence n'a jamais vraiment disparu — un idéal éphémère qu'il n'arrive pas à lâcher, et qu'il fait revivre un instant à chaque combat.

**Passif — Miroir fraternel :** quand Soren joue une carte offensive, si une de ses invocations actives (Lyse, ou toute autre invocation future) a une cible valide à sa propre portée — n'importe quel ennemi — le joueur peut lui faire rejouer un **écho de la carte à ~40 % de puissance**, gratuitement. L'invocation peut viser une autre cible que Soren (répartition des dégâts plutôt que doublement). Les PV de Lyse = **moitié des PV actuels de Soren**, recalculés en continu : elle peut mourir sans être ciblée.

| Carte Signature | PA | Effet |
|-----------------|----|-------|
| **Invocation de Lyse** (cœur de gameplay) | 2 | Invoque le fantôme de Lyse sur une case, portée 2-3 |
| **Écho de Lyse** (utilitaire) | 1 | Repositionne Lyse (ou une autre invocation) jusqu'à 3 cases |

**Gameplay :** bien placer le fantôme par rapport aux cibles probables. Mécanique d'écho réutilisable pour d'autres invocateurs.
**À trancher en playtest :** choix de cible quand plusieurs sont valides ; complexité globale.

---

### 2. L'ALPINISTE — le choix de cible au déplacement

**Trauma :** lors d'une ascension filmée et sponsorisée, un mousqueton a cédé ; toute son équipe est restée suspendue deux jours avant les secours. Personne n'est mort, mais l'humiliation publique et la perte de confiance en son jugement l'ont brisé. Depuis, il sécurise tout deux fois et ne supporte plus de laisser quelqu'un hors de sa portée.

**Passif — Réflexe du grimpeur :** après un déplacement rapide vers une unité, s'il atterrit **adjacent à un allié** : bouclier (-15 % aux prochains dégâts subis jusqu'à son prochain tour) ; **adjacent à un ennemi** : +15 % de dégâts sur sa prochaine carte. Le joueur choisit à chaque déplacement s'il joue tank ou assassin. Valeurs à confirmer en playtest.

| Carte Signature | PA | Effet |
|-----------------|----|-------|
| **Piolet d'ascension** (cœur de gameplay) | 2 | Se propulse au grappin adjacent à une unité (alliée ou ennemie), portée 4-5 ; déclenche le Réflexe du grimpeur |
| **Corde de rappel forcé** (coup de maître) | 4 | Portée 2-3, tire la cible de 2 cases vers lui. Ennemi : 35 dégâts + tiré au contact. Allié : aucun dégât, juste tiré (sauvetage) |

---

### 3. ACE — le combo de coûts

**Trauma :** a tout misé sur une main légendaire et a tout perdu en un instant — fortune, réputation, confiance de ses proches. Depuis, il ne laisse plus jamais le hasard décider : il triche, compte les cartes, calcule chaque probabilité.

**Passif — Main gagnante (provisoire) :** analyse les coûts en PA et les émotions des cartes jouées ce tour :
- **Paire** (2 cartes de même coût) → la 2ᵉ carte ignore les réductions de dégâts en pourcentage de la cible (boucliers)
- **Suite** (coûts N puis N+1) → +1 PA immédiat
- **Bluff** (2 cartes d'émotions différentes — decks bi-émotion uniquement) → -10 % aux prochains dégâts subis jusqu'à son prochain tour
- Un seul motif par tour, le plus exigeant l'emporte (Bluff > Suite > Paire)

| Carte Signature | PA | Effet |
|-----------------|----|-------|
| **Il triche** (cœur de gameplay) | 1 | Modifie de ±1 le coût en PA d'une carte de la main (min 1) pour forcer un motif. Pas de dégâts. |
| **Tapis** (coup de maître) | 5 | 72 dégâts de base, +10 par PA déjà dépensé ce tour. Finisher « all-in » à retravailler (ajouter un vrai risque) |

**Note :** le bonus du passif n'est pas calculable dans le tableur de budget ; il s'ajoute en jeu. La fiche de Soren mentionne aussi un « Full » et un ancien nom « l'Ingénieur » — à harmoniser dans l'Excel.

---

## 🗄️ CHAMPIONS HORS MVP

### ILYA — « Le Dévoué Enchaîné »
Concept complet (Colère, système Rage, transformation Enchaîné ↔ Déchaîné, 12 cartes) : `ilya_deck_simple.md`. **Conçu avant l'Excel** : sa Rage devra devenir une variante de l'Éveil Colère, et son deck de 12 cartes passer au format 24 cartes (2 Signature + 6 Éveil + 16 Standard).

### LES JUMEAUX — ASTRA & NOCTIS
Deux concepts concurrents :
- **Version 1** (dualité fusionnelle, 2 unités, PA partagés, fusion Éclipse) : `astra_noctis_simple.md`
- **Version 2** (1 unité, 2 phases) : Astra (tank, Confiance, 120 HP / 15 ATK / 15 DEF) ↔ Noctis (DPS, Peur, 80 HP / 25 ATK / 5 DEF) ; changement de phase à définir ; deck 3 Astra + 3 Noctis + 3 Neutres.

> Les stats ATK/DEF de ces concepts ne correspondent pas au système actuel (seuls PV, PA, PM sont définis) — à revoir si ces champions reviennent.

---

### VYLOS et CALYX
Existaient dans le code (`VylosUnit` avec les cartes Flagellation, Lien Vital et la marque Stigmate ; fiche `Calyx`) **sans document de design** ; retirés du code le 24/09/2026 (récupérables via le commit `00afe5d`). À documenter ici ou à archiver s'ils reviennent.

---

## ✅ CHECKLIST VALIDATION CHAMPION

- [ ] Mécanique existe ailleurs ? → Modifier
- [ ] Compréhensible en 1 phrase ?
- [ ] Axe de gameplay distinct des autres champions ?
- [ ] Trauma qui justifie la mécanique ?
- [ ] Faiblesse claire ?
- [ ] Profil PA/PM qui respecte le budget de 9 ?
- [ ] Amusant à jouer ?

---

**Dernière mise à jour :** 23 Septembre 2026
**Créé par :** Shinda + Claude
