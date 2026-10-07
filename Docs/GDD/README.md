# 📚 GDD — Émotions Tactics (Project TDB)

**Mis à jour le 07/10/2026** — ajout des leçons de Rosewater, retrait d'Ilya du jeu (sa fiche reste en archive). Dernière passe de cohérence complète : 23/09/2026.

## Par où commencer

1. **[GDD_Main.md](GDD_Main.md)** — vision, roster MVP, décisions actées, questions ouvertes, tableau « source unique de vérité »
2. **[MVP_Chiffres.md](MVP_Chiffres.md)** — **référence chiffrée du MVP** (née copie texte de l'Excel, qui n'est plus tenu à jour)
3. **[Technical_Specs.md](Technical_Specs.md)** — architecture, et section **« État du code »** (ce qui est réellement implémenté)

## Tous les documents

| Document | Contenu |
|----------|---------|
| [GDD_Main.md](GDD_Main.md) | Vision, lore, roster, décisions, questions ouvertes |
| [MVP_Chiffres.md](MVP_Chiffres.md) | Budget des cartes, bibliothèque, progression, barème monstres, roadmap |
| [claude_md_coarchitect.md](claude_md_coarchitect.md) | Contrat de collaboration avec Claude : rôle et façon de travailler (importé par `CLAUDE.md`) |
| [Playtest_Grille.md](Playtest_Grille.md) | Grille de la passe de playtest du MVP : plan des sessions, fiche de partie, points à observer, synthèse |
| [Lecons_Rosewater.md](Lecons_Rosewater.md) | Les 20 leçons de design de Mark Rosewater (Magic), appliquées au jeu : grille de relecture d'une mécanique ou d'une carte |
| [SYSTEME_EMOTIONS.md](SYSTEME_EMOTIONS.md) | 8 émotions, 3 de lancement (Colère, Peur, Joie), Éveil (fusion champion × émotion, 9 formes) |
| [CHAMPIONS_CONCEPTS.md](CHAMPIONS_CONCEPTS.md) | Evan, Crux, Raze + concepts hors MVP |
| [Card_System.md](Card_System.md) | Types de cartes, deck de 20, budget de puissance, ciblage |
| [Combat_System.md](Combat_System.md) | Tours, main, ressources, statuts, anti-lock |
| [Grid_System.md](Grid_System.md) | Grille (carrée dans le code) |
| [Enemies.md](Enemies.md) | Monstres de donjon / d'aventure, barème, boss |
| [Progression.md](Progression.md) | Niveaux, PV, XP, économie |
| [UX_Flow.md](UX_Flow.md) | Parcours joueur, raccourcis clavier |
| [UI_Design.md](UI_Design.md) | Interface, palette, désaturation des donjons |
| [Characters.md](Characters.md) | Structure d'un champion (ChampionData) |
| [ilya_deck_simple.md](archive/ilya_deck_simple.md) | Ilya (hors MVP, retiré du jeu le 07/10/2026) |
| [astra_noctis_simple.md](archive/astra_noctis_simple.md) | Astra & Noctis (hors MVP) |
| [personnages_a_developper.md](archive/personnages_a_developper.md) | Réservoir de 100 concepts |
| [archive/Concepts_Abandonnes.md](archive/Concepts_Abandonnes.md) | Tout ce qui a été abandonné, mis en pause ou remplacé |

## Règle de cohérence

Chaque information a **un seul document de référence** (voir le tableau dans `GDD_Main.md`). Quand une décision change : mettre à jour la référence, puis la table « Décisions actées » du GDD. Quand le code diverge du design : le noter dans `Technical_Specs.md` § « État du code ».

Ces docs existent aussi dans le projet claude.ai « EMOTIONS TACTICS - Game Dev ». **Le repo est la version de référence** ; resynchroniser le projet claude.ai après chaque changement important.
