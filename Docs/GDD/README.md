# 📚 GDD — Émotions Tactics (Project TDB)

**Mis à jour le 23/09/2026** — passe de cohérence : docs réalignés sur l'Excel MVP et sur l'état réel du code.

## Par où commencer

1. **[GDD_Main.md](GDD_Main.md)** — vision, roster MVP, décisions actées, questions ouvertes, tableau « source unique de vérité »
2. **[MVP_Excel_Snapshot.md](MVP_Excel_Snapshot.md)** — copie texte de l'Excel `TCG_Tactique_Systeme_de_calcul.xlsx`, référence chiffrée du MVP
3. **[Technical_Specs.md](Technical_Specs.md)** — architecture, et section **« État du code »** (ce qui est réellement implémenté)

## Tous les documents

| Document | Contenu |
|----------|---------|
| [GDD_Main.md](GDD_Main.md) | Vision, lore, roster, décisions, questions ouvertes |
| [MVP_Excel_Snapshot.md](MVP_Excel_Snapshot.md) | Budget des cartes, bibliothèque, progression, barème monstres, roadmap |
| [claude_md_coarchitect.md](claude_md_coarchitect.md) | Contrat de collaboration avec Claude : rôle et façon de travailler (importé par `CLAUDE.md`) |
| [SYSTEME_EMOTIONS.md](SYSTEME_EMOTIONS.md) | 8 émotions, 3 de lancement (Colère, Peur, Joie), Éveil |
| [CHAMPIONS_CONCEPTS.md](CHAMPIONS_CONCEPTS.md) | Evan, Crux, Raze + concepts hors MVP |
| [Card_System.md](Card_System.md) | Types de cartes, deck de 24, budget de puissance, ciblage |
| [Combat_System.md](Combat_System.md) | Tours, main, ressources, statuts, anti-lock |
| [Grid_System.md](Grid_System.md) | Grille (carrée dans le code) |
| [Enemies.md](Enemies.md) | Monstres de donjon / d'aventure, barème, boss |
| [Progression.md](Progression.md) | Niveaux, PV, XP, économie |
| [UX_Flow.md](UX_Flow.md) | Parcours joueur, raccourcis clavier |
| [UI_Design.md](UI_Design.md) | Interface, palette, désaturation des donjons |
| [Characters.md](Characters.md) | Structure d'un champion (ChampionData) |
| [ilya_deck_simple.md](ilya_deck_simple.md) | Ilya (hors MVP) |
| [astra_noctis_simple.md](astra_noctis_simple.md) | Astra & Noctis (hors MVP) |
| [personnages_a_developper.md](personnages_a_developper.md) | Réservoir de 100 concepts |
| [archive/Concepts_Abandonnes.md](archive/Concepts_Abandonnes.md) | Tout ce qui a été abandonné, mis en pause ou remplacé |

## Règle de cohérence

Chaque information a **un seul document de référence** (voir le tableau dans `GDD_Main.md`). Quand une décision change : mettre à jour la référence, puis la table « Décisions actées » du GDD. Quand le code diverge du design : le noter dans `Technical_Specs.md` § « État du code ».

Ces docs existent aussi dans le projet claude.ai « EMOTIONS TACTICS - Game Dev ». **Le repo est la version de référence** ; resynchroniser le projet claude.ai après chaque changement important.
