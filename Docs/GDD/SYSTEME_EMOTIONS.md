# 🎭 SYSTÈME D'ÉMOTIONS - Émotions Tactics

**Version :** 4.4
**Date :** 30 Septembre 2026
**Changements :**
- v4.0 (10/09/2026) : le système de Classes (5 classes, multiplicateurs, matrice 8×5) a été abandonné — voir `archive/Concepts_Abandonnes.md`.
- v4.1 (23/09/2026) : l'ancienne jauge -100/+100 (Contrariété / Colère / Rage) est archivée.
- v4.2 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`) — 3 émotions de lancement (Colère, Peur, Joie), système d'Éveil, roster Evan / Crux / Raze.
- v4.3 (24/09/2026) : noms de familles abandonnés, on parle directement des émotions ; faiblesses émotionnelles des monstres actées ; palette du codex adoptée comme référence.
- v4.4 (30/09/2026) : l'Éveil sort du deck et devient une **fusion champion × émotion** (Rage, Extase, Terreur ; 9 formes) ; plus de cartes d'Éveil ni de consommation de paliers.

---

## 📊 VUE D'ENSEMBLE

Le système d'émotions repose sur **8 émotions** (roue de Plutchik), chacune avec sa **couleur**. Dans le jeu, l'émotion est avant tout une **identité de carte** : un champion peut jouer toutes les émotions, et chaque deck en choisit 1 ou 2 (mono ou bi-émotion), comme les couleurs dans Magic. Le gameplay propre à chaque champion vient de sa **mécanique signature** (passif + cartes Signature), pas d'une classe.

Ce document est la **référence** pour les noms, émotions et couleurs des familles (les autres documents renvoient ici).

---

## 🎨 LES 8 ÉMOTIONS

Basées sur la Roue de Plutchik. On parle directement des émotions : **les noms de familles (Déchaînés, Dissidents, Insurgents, etc.) sont abandonnés** (24/09/2026). Ils venaient de l'ancien lore « gouvernement dystopique » et ne collaient plus à « le monde grisonne ».

| # | Émotion | Couleur | Code Hex |
|---|---------|---------|----------|
| 1 | **Colère** | Rouge | #D64545 |
| 2 | **Dégoût** | Violet | #9A4FBF |
| 3 | **Tristesse** | Bleu | #5A6FD8 |
| 4 | **Surprise** | Bleu clair | #4FA8E8 |
| 5 | **Peur** | Vert | #3F9D5C |
| 6 | **Confiance** | Vert clair | #5CC98A |
| 7 | **Joie** | Jaune | #D9A91F |
| 8 | **Anticipation** | Orange | #E08A3A |

> **Palette de référence (24/09/2026)** : celle du codex émotionnel, lisible sur fond sombre. Elle est codée à un seul endroit, `CodexCardVisual.EmotionColor` (`Scripts/Cards/CodexCardVisual.cs`) ; ne pas redéfinir ces couleurs ailleurs. L'ancienne palette (#CC0000, #006600, #FFEB00…) est abandonnée.

**Émotions « composées »** : certains donjons parlent d'émotions qui ne sont pas l'une des 8 (ex : l'**Anxiété** du Bureau Corporatiste). Chez Plutchik, l'anxiété se situe entre Peur et Anticipation — la famille de rattachement reste à choisir.

---

## 🚀 LES 3 ÉMOTIONS DE LANCEMENT (MVP — Excel)

| Émotion | Couleur | Rôle | Force / Faiblesse (validé en playtest) | Mécaniques typiques |
|---------|---------|------|-----------------------------------------|---------------------|
| **Colère** | Rouge #D64545 | Agressif | Burst, **sans sustain** | Gros dégâts, zones, contrecoups sur soi, vol de vie |
| **Peur** | Vert #3F9D5C | Contrôle | Contrôle / tempo | **Retrait de PM** au prochain tour (-1 / -2 / -3 / total), poussée/tirage, boucliers |
| **Joie** | Jaune #D9A91F | Soin / valeur | Survie, mais **lent** | Soins, boucliers, buffs de groupe, soin + dégâts miroir |

- Pool Standard : **51 cartes** dans le code (17 par émotion ; 49 dans la bibliothèque de l'Excel) ; la Roadmap de l'Excel parle de 12 par émotion (36) — à harmoniser.
- Cartes **Neutres** : les cartes Signature des champions, jouables quelles que soient les émotions du deck.
- **Oppositions d'émotions** (paires Plutchik, ex. Colère/Peur) : repoussées volontairement, à revisiter en phase 2-3.

---

## ✨ SYSTÈME D'ÉVEIL — la fusion (concept acté le 30/09/2026, pas encore codé)

> ⏸️ **Retiré du jeu pour l'instant (02/10/2026)** : à retravailler. Les jauges et les boutons de fusion sont masqués (`FusionPanelUI._awakeningEnabled = false`), donc aucune fusion n'est possible en combat ; le code des règles (jauges, formes de fusion, tests) est conservé pour la refonte.

**Principe :** l'Éveil n'est **plus une catégorie de cartes** dans le deck (les 6 slots d'Éveil disparaissent : ils auraient bloqué la construction de deck). Quand la jauge d'une émotion est pleine, le champion **fusionne avec cette émotion** et obtient, tant que dure la fusion, un **gameplay propre à son couple champion × émotion**. Trois formes par émotion : **Colère → Rage**, **Joie → Extase**, **Peur → Terreur**.

**La jauge (inchangée) :**
- Chaque émotion a **sa propre jauge** (un deck bi-émotion doit donc remplir deux jauges : c'est le coût caché du bi-émotion).
- La plupart des cartes Standard **génèrent** de la jauge ; les Signatures en génèrent au choix du joueur. **2 points par palier, 3 paliers maximum.**
- **La jauge monte en jouant des cartes** de l'émotion. Combien par carte, et si le **coût en PA** compte (une carte chère remplit plus), reste **à tester**.
- Les cartes ne **consomment** plus de paliers : le modificateur « consomme 1 / 2 / 3 paliers » a été retiré de l'Excel (30/09/2026).
- **Pas de bi-fusion pour l'instant** : une seule fusion à la fois. **Decks bi-émotion (30/09/2026)** : pendant une fusion, jouer une carte d'une **autre émotion** retire 1 point à la jauge de la fusion (à 0, elle prend fin) tout en faisant monter la jauge de cette autre émotion ; les cartes neutres (Signatures) n'y touchent pas. On arbitre donc entre tenir sa fusion et progresser vers l'autre. À équilibrer en jeu (perte de 1 point par carte).

**La fusion — mécanique « jauge qui brûle » :**
- **Entrée** : jauge **pleine** (3 paliers) + **activation manuelle** par le joueur (une commande de combat, comme les autres actions), pour choisir le bon moment.
- **Maintien** : la jauge **perd 1 palier au début de chaque tour** du champion fusionné ; **jouer des cartes de l'émotion** la recharge. On reste donc fusionné en jouant les cartes de son émotion.
- **Fin** : jauge à 0. Pas de durée fixe, pas de paliers de forme.
- Le jeu normal n'est jamais bloqué : sans fusion, on joue ses cartes avec des PA.

**Les 9 formes** (**toutes codées le 30/09/2026** ; valeurs **à équilibrer en jeu** — ce sont des champs d'asset, voir « Valeurs retenues » ; la grille reste carrée, 4 directions, distances de Manhattan) :

| | **Rage** (Colère) | **Extase** (Joie) | **Terreur** (Peur) |
|---|---|---|---|
| **Evan** | **Deux en un** — Lyse est **absorbée** (elle disparaît) : chaque attaque d'Evan est rejouée une seconde fois depuis lui, à 100 %. À la fin de la forme, Lyse revient à côté d'Evan avec **la moitié des PV qu'elle avait à son absorption** (recommandé, voir « À trancher »). | **Écho soigneur** — l'écho de Lyse ne fait plus de dégâts : il **soigne tous les alliés** (Evan et Lyse compris) de **40 %** des dégâts de l'attaque d'origine, par allié (non réparti). | **Appât** — à l'activation, Lyse **attire tous les ennemis** vers elle (dans un ordre fixe), pose une **zone** sur sa case qui **retire 1 PM par tour** aux ennemis qu'elle contient (compté au début de leur tour : on peut les y pousser), puis se **téléporte** sur la case libre la plus proche d'Evan. Ni tank (Lyse s'en va), ni lock (−1 PM seulement). |
| **Crux** | **Avalanche** — le Réflexe du grimpeur est permanent, et chaque déplacement en ligne devient une charge qui inflige des dégâts par case parcourue. | **Ascension** — chaque déplacement de Crux **fait avec une carte** (Grappin, Bond percutant, charge…) le **soigne** par case parcourue ; les alliés à **2 cases ou moins de son arrivée** sont soignés à **50 %**. Marche simple et déplacements forcés ne comptent pas. Plafond de soin par tour. Marche en solo (il se soigne), soutien en équipe. | **Vol de mouvement** — chaque carte de Crux qui touche un ennemi lui **retire 1 PM** (retrait normal : le plus fort remplace le plus faible) et **donne 1 PM à Crux** ce tour, plafonné à 2 PM par tour. |
| **Raze** | **All-in** *(à tester, peut être abandonné)* — chaque carte compte comme une Suite pour la Main gagnante, avec des bonus de motifs doublés ; en contrepartie Raze subit un contrecoup proportionnel aux PA dépensés. | **Partage des gains** — chaque motif joué (Paire, Suite, Bluff) donne un **soin ou un bouclier à l'allié le plus blessé** (Raze compris), proportionnel aux PA dépensés dans le motif. Texte court : il s'affiche dans le panneau de forme, pas sur les cartes. | **Pioche et tempo** — le **premier ennemi touché de chaque tour** fait piocher 1 carte et rend 1 PA ; une **Suite** retire en plus 1 PM à la cible (au début de son prochain tour ; PA avant le 02/10/2026, les monstres n'en ayant plus). Limité à une fois par tour (sans quoi une carte à 1 PA serait gratuite), *à surveiller avec la Triche*. |

**Valeurs retenues (30/09/2026, champs des assets `Fusions/`, à ajuster en playtest) :**
- **Deux en un** : Lyse est retirée du combat à l'activation ; chaque attaque d'Evan (une par cible touchée) est rejouée 0,5 s plus tard, à 100 % (défense de la cible appliquée). Retour de Lyse sur la case libre la plus proche d'Evan avec la moitié de ses PV d'absorption (au moins 1). Sans Lyse à l'activation, seule la seconde frappe joue.
- **Écho soigneur** : 40 % des dégâts de l'attaque d'origine à chaque allié vivant (Evan, Lyse, autres champions).
- **Appât** : ennemis attirés du plus proche au plus lointain (puis par x, y), en au plus deux tirs, jusqu'au contact ; zone de rayon 1 autour de Lyse (ou d'Evan sans Lyse) qui retire 1 PM au début du tour des ennemis qu'elle contient, jusqu'à la fin de la fusion ; puis Lyse rejoint Evan. Les zones (`FusionZones`) sont dans l'empreinte réseau.
- **Avalanche** : 3 dégâts physiques par case parcourue à chaque ennemi au contact de la case d'arrivée ; bonus du Réflexe (+15 %) permanent et non consommé.
- **Ascension** : 2 PV par case parcourue, plafond 10 PV par tour ; alliés à 2 cases ou moins de l'arrivée soignés à 50 %.
- **All-in** : chaque carte est une Suite (+2 PA au lieu de +1) et les motifs se cumulent ; bonus chiffrés ×2 (Bluff : bouclier 16) ; contrecoup 1 PV par PA dépensé, jamais mortel (Raze reste à 1 PV au minimum).
- **Partage des gains** : 2 PV par PA dépensé dans le motif, sur l'allié au plus faible ratio de PV ; si tous sont à pleine vie, la même valeur en bouclier. Le motif compte les PA de la carte et de la précédente.
- **Pioche et tempo** : 1 carte + 1 PA à la première carte du tour qui touche un ennemi ; une carte qui forme une Suite retire 1 PM (prochain tour) à chaque ennemi touché.

**Limites connues :** aucune représentation visuelle de la zone d'Appât ; aucune propriété « indéplaçable » pour les boss (un boss est attiré comme les autres) ; Lyse garde son Miroir fraternel pendant Appât et Écho soigneur (en Écho soigneur, il devient le soin).

**Fil rouge par champion** (une seule idée, trois expressions) : Evan = **Lyse** (deux en un / écho soigneur / appât) ; Crux = **le déplacement** (dégâts par case / soin par case / PM volés) ; Raze = **les motifs de coûts** (doublés / partagés / tempo).

**Règles d'implémentation retenues :**
- Un état « fusionné » porté par le champion, activé par un type de commande (`CombatCommand`), jamais un effet appliqué depuis l'UI (prérequis du réseau) ; résultat déterministe (empreinte de désynchronisation).
- **Un asset de données par (champion, émotion)** qui liste les modificateurs ; le code générique passe par des interfaces opt-in (`IOutgoingDamageModifier`, `IComboTracker`, `IContactReactor`, `ISummonOwner`…), pas par des `if` sur le type de champion.
- Ordre fixe pour tout effet multi-cibles (attraction d'Appât : les ennemis les plus proches de Lyse d'abord, puis par coordonnées de case).
- **Ordre suivi** : cadre de fusion (jauge, commande, asset) avec **Crux en Terreur**, validé en jeu, puis les 8 autres formes.

**À trancher :**
- **Coût d'activation** de la fusion (gratuit, ou 1 PA ?).
- ~~**Appât**~~ → **codé avec des valeurs par défaut (30/09)** : rayon 1, attraction jusqu'au contact, une seule zone compte par ennemi. Reste : ennemis **indéplaçables** selon le boss (propriété du boss, non codée) et représentation visuelle de la zone.
- ~~**Ascension**~~ → **codé (30/09)** : 2 PV par case, plafond 10 par tour ; à régler en jeu selon les cartes de déplacement.
- ~~**Avalanche** : mêmes règles de déplacement que l'Ascension~~ → **tranché (30/09)** : seuls les déplacements faits avec une carte comptent ; marche simple et déplacements forcés ne comptent pas.
- ~~**Deux en un**~~ → **option (b) codée (30/09)**. PV de Lyse à son retour. Options : (a) inchangés ; (b) elle perd la moitié de ce qu'elle a. **Recommandation : (b)** — elle revient avec la moitié des PV qu'elle avait à l'absorption (≈ 25 % des PV d'Evan), au moins 1. Une fusion très forte doit avoir un coût, et (a) la rendrait gratuite et sûre ; (b) ne la tue pas, reste lisible, et ne touche pas la règle « PV de Lyse = moitié de ceux d'Evan » (seuls ses PV actuels baissent, son maximum reste recalculé).
- **Format du deck** sans emplacements d'Éveil : 20 cartes (déjà dans le code : 4 Signature + 16 Standard) ou autre — voir `GDD_Main.md`.
- Textes des formes : affichage de la jauge, de la fusion en cours et du texte de la forme dans le HUD (`UI_Design.md`).

> Ne pas confondre avec l'ancienne jauge universelle -100/+100 (états Tank/DPS), archivée. La Rage d'Ilya (hors MVP) devra devenir sa forme Colère (« Rage »).

---

## 👥 CHAMPIONS (état au 23/09/2026)

Les champions du MVP n'appartiennent pas à une émotion : leurs cartes Signature sont Neutres et ils peuvent jouer des decks de n'importe quelle émotion.

| Champion | Statut |
|----------|--------|
| **Evan** | MVP — complet (Excel) |
| **Crux** | MVP — complet (Excel) |
| **Raze** | MVP — complet (Excel) |
| **Ilya** (Colère) | Hors MVP — concept complet, Rage à réadapter à la fusion Colère |
| **Astra & Noctis** | Hors MVP — deux concepts concurrents |

---

## 🔧 GÉNÉRATION D'ÉMOTION

Dans le MVP, l'émotion se génère **en jouant des cartes** (jauge d'Éveil, voir ci-dessus). Les déclencheurs ci-dessous restent des pistes pour de futures mécaniques signatures ou pour les émotions ajoutées après le MVP.

---

## 📋 DÉCLENCHEURS PAR ÉMOTION (pistes, hors MVP)

| Émotion | Déclencheurs Émotionnels |
|---------|---------------------------|
| **Colère** | Dégâts reçus/infligés, éliminations |
| **Dégoût** | Debuffs subis, résistances, toxicité |
| **Tristesse** | Alliés blessés, temps, échecs |
| **Surprise** | Critiques, événements inattendus, hasard |
| **Peur** | HP bas, ennemis puissants, encerclement |
| **Confiance** | Soins, protections, alliés en bonne santé |
| **Joie** | Victoires, buffs, combos |
| **Anticipation** | Planification, temps, préparation |

---

## 💻 IMPLÉMENTATION UNITY

### Enums

Code actuel (`Cards/CardData.cs`) — les émotions sont nommées par émotion, pas par famille :

```csharp
public enum EmotionType
{
    None,
    Colere,         // Rouge #D64545
    Degout,         // Violet #9A4FBF
    Tristesse,      // Bleu #5A6FD8
    Surprise,       // Bleu clair #4FA8E8
    Peur,           // Vert #3F9D5C
    Confiance,      // Vert clair #5CC98A
    Joie,           // Jaune #D9A91F
    Anticipation    // Orange #E08A3A
}

// Pas d'enum CardClassType : le système de classes a été retiré le 10/09/2026.
// Chaque champion implémente sa mécanique signature directement (passif + cartes Signature).
// Le type de carte est porté par CardCategory (Standard / Eveil / Signature).
// Des assets de cartes « Family » (Dechaines, Reprouves…) existent encore dans
// Assets/ScriptableObjects/Cards/Family/ : reliquat de l'ancien système.
```

---

**Dernière mise à jour :** 23 Septembre 2026
**Créé par :** Shinda + Claude
