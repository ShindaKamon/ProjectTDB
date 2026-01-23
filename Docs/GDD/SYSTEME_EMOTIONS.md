# 🎭 SYSTÈME D'ÉMOTIONS - Émotions Tactics

**Version :** 3.0  
**Date :** 17 Janvier 2026

---

## 📊 VUE D'ENSEMBLE

Le système d'émotions combine **8 Familles** (émotions de Plutchik) avec **5 Classes** (gestion psychologique) pour créer 40 champions uniques.

---

## 🎨 LES 8 FAMILLES (Émotions)

Basées sur la Roue de Plutchik. Les familles définissent **quelle émotion** le champion utilise.

| # | Famille | Émotion | Couleur | Code Hex | Thème |
|---|---------|---------|---------|----------|-------|
| 1 | **Déchaînés** | Colère | Rouge | #CC0000 | Guerriers impulsifs |
| 2 | **Dissidents** | Dégoût | Violet | #800080 | Rebelles rejetant |
| 3 | **Insurgents** | Tristesse | Bleu foncé | #000080 | Révolutionnaires mélancoliques |
| 4 | **Exilés** | Surprise | Bleu clair | #80CCFF | Parias imprévisibles |
| 5 | **Réprouvés** | Peur | Vert foncé | #006600 | Maudits terrifiés |
| 6 | **Gardiens** | Confiance | Vert clair | #80FF80 | Protecteurs nobles |
| 7 | **Éveillés** | Joie | Jaune | #FFEB00 | Illuminés joyeux |
| 8 | **Précurseurs** | Anticipation | Orange | #FF8000 | Pionniers visionnaires |

---

## 🎭 LES 5 CLASSES (Gestion Psychologique)

Les classes définissent **comment** le champion gère son émotion.

| # | Classe | Verbe | Multiplicateur | Gameplay |
|---|--------|-------|----------------|----------|
| 1 | **Réprimé** | Stocke | ×0.8 | Accumulation lente → Explosion contrôlée |
| 2 | **Impulsif** | Consomme | ×1.5 | Génération rapide → Dépense immédiate |
| 3 | **Alchimiste** | Transforme | ×1.0 | Émotion → Mana/Énergie magique |
| 4 | **Émissaire** | Transfère | ×1.2 | Émotion → Invocations/Entités |
| 5 | **Évadé** | Fuit | ×1.3 | Émotion → Substances (addiction) |

**Multiplicateur :** Vitesse de génération d'émotion (×0.8 = lent, ×1.5 = rapide)

---

## 🔢 MATRICE 8×5 (40 Champions Possibles)

| Famille | Réprimé | Impulsif | Alchimiste | Émissaire | Évadé |
|---------|---------|----------|------------|-----------|-------|
| **Déchaînés** | Ilya ✅ | Kael | Pyra | Ragnar | Týr |
| **Dissidents** | À créer | À créer | À créer | À créer | À créer |
| **Insurgents** | À créer | À créer | À créer | À créer | À créer |
| **Exilés** | Alea | Chaos | Kairos | Pandora | Trip |
| **Réprouvés** | Corvus | Phobos | Umbra | Bane | Syringe |
| **Gardiens** | À créer | À créer | À créer | À créer | À créer |
| **Éveillés** | À créer | À créer | À créer | À créer | À créer |
| **Précurseurs** | À créer | À créer | À créer | À créer | À créer |

---

## 🎯 ÉTATS ÉMOTIONNELS (3 par Famille)

### ✅ Déchaînés (Colère)

| Seuil | État | Type | Description |
|-------|------|------|-------------|
| +100 | Contrariété | Tank | Frustration canalisée, défense |
| 0 | Colère | Neutre | État de base |
| -100 | Rage | DPS | Fureur destructrice |

### ⏳ Autres Familles (À Définir)

Les 7 autres familles ont leurs états émotionnels à créer.

---

## 🔧 GÉNÉRATION D'ÉMOTION

### Option Recommandée : Hybride

```
GÉNÉRATION = (Déclencheurs Famille) × (Multiplicateur Classe) + (Tweak Champion optionnel)
```

**Exemple Ilya (Déchaînés Réprimé) :**
1. Famille Déchaînés : 20 dégâts = +1 Colère (déclencheur)
2. Classe Réprimé : ×0.8 (génère lentement)
3. Signature Ilya : Génère Cartes Rage (contrôle fin)

---

## 📋 DÉCLENCHEURS PAR FAMILLE (Proposés)

| Famille | Déclencheurs Émotionnels |
|---------|--------------------------|
| **Déchaînés** | Dégâts reçus/infligés, éliminations |
| **Dissidents** | Debuffs subis, résistances, toxicité |
| **Insurgents** | Alliés blessés, temps, échecs |
| **Exilés** | Critiques, événements inattendus, RNG |
| **Réprouvés** | HP bas, ennemis puissants, encerclement |
| **Gardiens** | Soins, protections, alliés sains |
| **Éveillés** | Victoires, buffs, combos |
| **Précurseurs** | Planification, temps, préparation |

---

## 💻 IMPLÉMENTATION UNITY

### Enums

```csharp
public enum CardFamilyType
{
    Dechaines,   // Colère
    Dissidents,  // Dégoût
    Insurgents,  // Tristesse
    Exiles,      // Surprise
    Reprouves,   // Peur
    Gardiens,    // Confiance
    Eveilles,    // Joie
    Precurseurs  // Anticipation
}

public enum CardClassType
{
    Reprime,     // Stocke (×0.8)
    Impulsif,    // Consomme (×1.5)
    Alchimiste,  // Transforme (×1.0)
    Emissaire,   // Transfère (×1.2)
    Evade        // Fuit (×1.3)
}
```

---

**Dernière mise à jour :** 17 Janvier 2026  
**Créé par :** Shinda + Claude
