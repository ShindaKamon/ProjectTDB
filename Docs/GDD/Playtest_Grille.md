# Grille de playtest — passe MVP

**Créée le 07/10/2026.** Sert la dernière grosse étape du MVP (`GDD_Main.md` § « Reste à faire ») : la passe de playtest sur les passifs, l'attaque et les défenses, les fusions et les monstres. Les chiffres validés vont ensuite dans `MVP_Chiffres.md`, et les décisions dans « Décisions actées » de `GDD_Main.md`.

## La règle d'or : noter le ressenti, pas la solution

Leçon 19 de Rosewater (`Lecons_Rosewater.md`) : on sait très bien dire ce qui ne va pas, beaucoup moins bien comment le corriger. Pendant la partie, on note **ce qu'on ressent et ce qui se passe** (« j'ai attendu 3 tours sans rien faire », « je n'ai pas compris pourquoi il m'a touché »), **jamais le correctif** (« baisser ses PV »). On cherchera la cause après, à froid.

Si une idée de correctif vient quand même, on l'écrit dans la case « Idées », séparée.

---

## Plan des sessions

Une partie = l'Orphelinat en entier (3 salles, boss compris). Avec 3 champions et 3 émotions, il y a **9 formes de fusion** : on les couvre en 3 sessions sans rejouer la même.

| Session | Partie 1 | Partie 2 | Partie 3 | Objectif |
|---|---|---|---|---|
| **1 — Solo mono-émotion** | Evan · Colère | Crux · Peur | Raze · Joie | Passifs, monstres, boss, premières fusions |
| **2 — Solo, autres couples** | Evan · Peur | Crux · Joie | Raze · Colère | Fusions restantes, comparer avec la session 1 |
| **3 — Coop et bi-émotion** | 2 joueurs : Evan · Joie + Crux · Colère | 3 joueurs : Raze · Peur + Evan + Crux (bi-émotion) | *(libre)* | Mise à l'échelle des monstres, decks bi-émotion |

**Suivi des 9 formes** (cocher quand la forme a été activée au moins une fois) :

| | Rage (Colère) | Extase (Joie) | Terreur (Peur) |
|---|---|---|---|
| **Evan** | ☐ Deux en un | ☐ Écho soigneur | ☐ Appât |
| **Crux** | ☐ Avalanche | ☐ Ascension | ☐ Vol de mouvement |
| **Raze** | ☐ All-in | ☐ Partage des gains | ☐ Pioche et tempo |

---

## Fiche d'une partie (à copier pour chaque partie)

```
Date :            Session / partie :
Champion(s) :              Couleurs du deck :
Joueurs : 1 / 2 / 3        Résultat : victoire / défaite (à quel combat ?)

COMBATS                        Tours   PV restants   Fusion activée ? (tour)
  Salle 1 — dortoir (2 moutons)
  Salle 2 — couloir (3 moutons + soldat)
  Salle 3 — boss : phase 1 / 2 / 3

MOMENTS
  Le meilleur moment (un « wow » ?) :
  Le moment le plus ennuyeux :
  Un moment où je n'ai pas compris ce qui se passait :
  Une carte que je n'ai jamais voulu jouer :
  Une carte que je jouais à chaque fois :

RESSENTI (1 = pas du tout, 5 = tout à fait)
  J'ai ressenti l'émotion de mon deck (élan / tension / soulagement)   1 2 3 4 5
  La façon la plus efficace de jouer était aussi amusante               1 2 3 4 5
  Je comprenais ce que les monstres allaient faire                      1 2 3 4 5
  Le combat avait la bonne durée                                        1 2 3 4 5
  J'ai envie de rejouer ce champion                                     1 2 3 4 5

IDÉES (à part, à trier plus tard) :
```

---

## Ce qu'on observe, système par système

Les valeurs entre parenthèses sont celles du jeu au 07/10/2026, pour savoir de quoi on part.

### 1. Passifs des champions
| Champion | Passif | Question à se poser | Notes |
|---|---|---|---|
| Evan | Miroir fraternel (écho de Lyse à ~40 %) | L'écho se voit-il ? Garde-t-on Lyse en vie, ou est-elle un poids ? | |
| Crux | Réflexe du grimpeur (bouclier avec un allié, +15 % avec un ennemi) | Fait-on vraiment le choix « tank ou assassin » à chaque déplacement, ou toujours le même ? | |
| Raze | Main gagnante (Paire / Suite / Bluff) | Construit-on son tour autour des motifs ou tombe-t-on dessus par hasard ? Le Bluff sert-il en mono-émotion ? | |

### 2. Fusions (Éveil)
| Point | Question | Notes |
|---|---|---|
| Remplissage de la jauge (2 points par palier, 3 paliers) | À quel tour la jauge est-elle pleine ? Trop tôt, trop tard ? Une carte chère devrait-elle remplir plus ? | |
| Activation (gratuite pour l'instant) | Le moment d'activer est-il une vraie décision ? Faut-il un coût (1 PA) ? | |
| Maintien (−1 palier par tour) | Combien de tours dure une fusion ? Arrive-t-on à la tenir ? | |
| Deux en un (Lyse revient avec la moitié des PV qu'elle avait) | Le prix de l'absorption paraît-il juste ? | |
| Ascension (2 PV par case, plafond 10 par tour) | Le soin compte-t-il ? | |
| Avalanche (3 dégâts par case) | Vaut-il le coup de charger exprès ? | |
| All-in (marqué « à tester, peut être abandonné ») | **Le garder ?** Le contrecoup (1 PV par PA) fait-il peur ou est-il ignoré ? | |
| Appât | Comprend-on la zone sans qu'elle soit affichée ? | |

### 3. Attaque et défenses
| Question | Notes |
|---|---|
| L'armure et la résistance magique se sentent-elles, ou les dégâts semblent-ils toujours les mêmes ? | |
| Le boss (armure et RM 4) est-il nettement plus dur à entamer que les mobs ? | |
| Choisit-on ses cartes physiques ou magiques selon la cible ? | |

### 4. Cartes à zone (recalculées le 07/10)
| Carte | Nouvelle valeur | Trop forte / juste / trop faible ? |
|---|---|---|
| Rayonnement de joie | 36 (était 26) | |
| Explosion de rage | 48 (était 40) | |
| Onde de terreur | **19 (était 25, en baisse)** | |
| Poing ardent · Bond percutant | 25 · 21 | |
| Effroi partagé | 16 | |
| Vague de bien-être · Vague de guérison | soin 33 · 32 | |

### 5. Monstres de l'Orphelinat (valeurs provisoires)
| Monstre | Valeurs actuelles | Question | Notes |
|---|---|---|---|
| Mouton de poussière | 100 PV, 3 PM ; Mordille ×2 puis Embrumé | Menace réelle ou simple sac de PV ? | |
| Soldat de bois | 60 PV, 2 PM ; Baïonnette ×2 puis Garde-à-vous ; protège les mobs | Sa protection change-t-elle nos cibles ? | |
| Boss, phase 1 (6 lits de 40, 200 PV) | Il change de lit chaque tour | Chercher le bon lit est-il amusant ou frustrant ? | |
| Boss, phase 2 (le Lit, 250 PV) | Au lit ! ramène les tas de jouets | Voit-on venir les dégâts des tas ? | |
| Boss, phase 3 (300 PV) | Il sort et bouge | Chaque phase est-elle au moins aussi longue que la précédente ? Fin trop longue ? | |
| Pluie de jouets (16 dégâts, 4 zones annoncées) | | Les zones annoncées sont-elles lisibles ? A-t-on le temps de les éviter ? | |

### 6. Coop (session 3)
| Question | Notes |
|---|---|
| À 2 et à 3 joueurs, les monstres sont-ils aussi dangereux qu'en solo ? (dégâts × 1,5 à 2 joueurs, × 2 à 3 : facteur « à valider » dans `Enemies.md`) | |
| Les passifs se combinent-ils (bouclier de Crux près d'un allié, soins partagés de Raze) ? | |
| Les temps morts pendant le tour des autres sont-ils gênants ? | |

---

## Après les sessions : synthèse

Pour chaque problème noté plusieurs fois :

| Problème ressenti (tel que noté) | Combien de fois | Cause probable (à froid) | Plus petit changement possible (leçon 17 : changer ou retirer avant d'ajouter) | Décision |
|---|---|---|---|---|
| | | | | |

**Où reporter :** les chiffres dans `MVP_Chiffres.md` (et dans les assets), les décisions dans « Décisions actées » de `GDD_Main.md`, les questions encore ouvertes dans « Questions ouvertes ».
