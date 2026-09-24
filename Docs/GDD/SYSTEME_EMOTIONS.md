# 🎭 SYSTÈME D'ÉMOTIONS - Émotions Tactics

**Version :** 4.3
**Date :** 24 Septembre 2026
**Changements :**
- v4.0 (10/09/2026) : le système de Classes (5 classes, multiplicateurs, matrice 8×5) a été abandonné — voir `archive/Concepts_Abandonnes.md`.
- v4.1 (23/09/2026) : l'ancienne jauge -100/+100 (Contrariété / Colère / Rage) est archivée.
- v4.2 (23/09/2026) : réalignement sur l'Excel MVP (`TCG_Tactique_Systeme_de_calcul.xlsx`) — 3 émotions de lancement (Colère, Peur, Joie), système d'Éveil, roster Evan / Crux / Raze.
- v4.3 (24/09/2026) : noms de familles abandonnés, on parle directement des émotions ; faiblesses émotionnelles des monstres actées.

---

## 📊 VUE D'ENSEMBLE

Le système d'émotions repose sur **8 émotions** (roue de Plutchik), chacune avec sa **couleur**. Dans le jeu, l'émotion est avant tout une **identité de carte** : un champion peut jouer toutes les émotions, et chaque deck en choisit 1 ou 2 (mono ou bi-émotion), comme les couleurs dans Magic. Le gameplay propre à chaque champion vient de sa **mécanique signature** (passif + cartes Signature), pas d'une classe.

Ce document est la **référence** pour les noms, émotions et couleurs des familles (les autres documents renvoient ici).

---

## 🎨 LES 8 ÉMOTIONS

Basées sur la Roue de Plutchik. On parle directement des émotions : **les noms de familles (Déchaînés, Dissidents, Insurgents, etc.) sont abandonnés** (24/09/2026). Ils venaient de l'ancien lore « gouvernement dystopique » et ne collaient plus à « le monde grisonne ».

| # | Émotion | Couleur | Code Hex |
|---|---------|---------|----------|
| 1 | **Colère** | Rouge | #CC0000 |
| 2 | **Dégoût** | Violet | #800080 |
| 3 | **Tristesse** | Bleu foncé | #000080 |
| 4 | **Surprise** | Bleu clair | #80CCFF |
| 5 | **Peur** | Vert foncé | #006600 |
| 6 | **Confiance** | Vert clair | #80FF80 |
| 7 | **Joie** | Jaune | #FFEB00 |
| 8 | **Anticipation** | Orange | #FF8000 |

**Émotions « composées »** : certains donjons parlent d'émotions qui ne sont pas l'une des 8 (ex : l'**Anxiété** du Bureau Corporatiste). Chez Plutchik, l'anxiété se situe entre Peur et Anticipation — la famille de rattachement reste à choisir.

---

## 🚀 LES 3 ÉMOTIONS DE LANCEMENT (MVP — Excel)

| Émotion | Couleur | Rôle | Force / Faiblesse (validé en playtest) | Mécaniques typiques |
|---------|---------|------|-----------------------------------------|---------------------|
| **Colère** | Rouge #CC0000 | Agressif | Burst, **sans sustain** | Gros dégâts, zones, contrecoups sur soi, vol de vie |
| **Peur** | Vert foncé #006600 | Contrôle | Contrôle / tempo | **Retrait de PM** au prochain tour (-1 / -2 / -3 / total), poussée/tirage, boucliers |
| **Joie** | Jaune #FFEB00 | Soin / valeur | Survie, mais **lent** | Soins, boucliers, buffs de groupe, soin + dégâts miroir |

- Pool Standard : **49 cartes** dans la bibliothèque de l'Excel et dans le code (17 Colère, 17 Peur, 15 Joie) ; la Roadmap de l'Excel parle de 12 par émotion (36) — à harmoniser.
- Cartes **Neutres** : les cartes Signature des champions, jouables quelles que soient les émotions du deck.
- **Oppositions d'émotions** (paires Plutchik, ex. Colère/Peur) : repoussées volontairement, à revisiter en phase 2-3.

---

## ✨ SYSTÈME D'ÉVEIL (concept acté, mise en œuvre repoussée)

- Chaque émotion a **sa propre jauge d'Éveil** (un deck bi-émotion doit donc remplir deux jauges : c'est le coût caché du bi-émotion).
- La plupart des cartes Standard **génèrent** de l'Éveil (modificateur -0.1 sur leur budget).
- Base actée : **2 points par palier d'Éveil**. Les cartes Signature génèrent de la jauge au choix du joueur.
- Les **cartes d'Éveil** (6 par deck) nécessitent un seuil pour être jouées ; les cartes qui **consomment** 1, 2 ou 3 paliers gagnent +0.3 / +0.5 / +0.7 de budget.
- L'Éveil ne bloque jamais le jeu normal : on peut toujours jouer ses cartes Standard avec des PA.
- À enrichir : aller au-delà d'un simple compteur linéaire (interactions entre jauges, lien avec les oppositions, seuils qui débloquent des choix).

> Ne pas confondre avec l'ancienne jauge universelle -100/+100 (états Tank/DPS), archivée, ni avec la Rage d'Ilya (hors MVP).

---

## 👥 CHAMPIONS (état au 23/09/2026)

Les champions du MVP n'appartiennent pas à une émotion : leurs cartes Signature sont Neutres et ils peuvent jouer des decks de n'importe quelle émotion.

| Champion | Statut |
|----------|--------|
| **Evan** | MVP — complet (Excel) |
| **Crux** | MVP — complet (Excel) |
| **Raze** | MVP — complet (Excel) |
| **Ilya** (Colère) | Hors MVP — concept complet, Rage à réadapter à l'Éveil |
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
    Colere,         // Rouge #CC0000
    Degout,         // Violet #800080
    Tristesse,      // Bleu foncé #000080
    Surprise,       // Bleu clair #80CCFF
    Peur,           // Vert foncé #006600
    Confiance,      // Vert clair #80FF80
    Joie,           // Jaune #FFEB00
    Anticipation    // Orange #FF8000
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
