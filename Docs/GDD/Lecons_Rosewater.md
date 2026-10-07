# Les 20 leçons de Mark Rosewater appliquées à Project TDB

**Ajouté le 07/10/2026.** Mark Rosewater, designer principal de *Magic: The Gathering*, a résumé vingt ans de design en 20 leçons (conférence GDC 2016, « Twenty Years, Twenty Lessons »). Ce document les sert de **grille de relecture** : avant d'acter une mécanique, une carte ou un écran, on les passe en revue. Pour chaque leçon : son sens, puis ce qu'elle veut dire pour notre jeu.

> Ce document ne décide rien : les décisions restent dans leur document de référence (`GDD_Main.md` § « Décisions actées »). Les pistes ci-dessous sont des questions à trancher, pas des règles.

## Comprendre le joueur

**1. Ne luttez pas contre la nature humaine.** Les joueurs adoptent naturellement certains comportements ; on adapte le jeu à leur façon de penser plutôt que l'inverse.
→ Les joueurs garderont leurs meilleures cartes « pour plus tard » et fuiront les risques. Un mécanisme qui les y pousse doit être voulu (la Rage d'Ilya récompense le fait de prendre des coups : vérifier en playtest que les joueurs ne l'évitent pas par réflexe).

**3. Utilisez ce que les gens connaissent déjà.** Les références culturelles et émotionnelles créent une connexion immédiate.
→ C'est la force du thème : tout le monde connaît la peur du monstre sous le lit, la colère, la joie. L'Orphelinat marche parce que ses monstres (moutons de poussière, soldats de bois, monstre sous le lit) sont des peurs d'enfance.

**4. Le « piggybacking » facilite l'apprentissage.** S'appuyer sur ce que le joueur sait déjà pour lui apprendre une mécanique.
→ Raze s'appuie sur le poker (Paire, Suite, Bluff, Tapis) ; PA/PM sur Dofus ; les couleurs de deck sur Magic. Un nouveau champion devrait avoir le même ancrage : un vocabulaire que le joueur comprend avant d'avoir lu la carte.

**15. Concevez chaque élément pour son public cible.** Certains cherchent l'excitation, d'autres la créativité, d'autres la compétition.
→ Les trois profils de Magic (Timmy le spectaculaire, Johnny le combinard, Spike le compétiteur) se retrouvent dans le roster : Ilya et les gros coups de Colère pour Timmy, Raze et ses motifs pour Johnny, le contrôle de Peur pour Spike. Chaque nouvelle carte : pour lequel des trois ?

**19. Votre public sait identifier les problèmes, pas forcément les résoudre.** Les retours disent ce qui ne va pas ; la cause profonde est à chercher soi-même.
→ À appliquer pendant la passe de playtest : noter le ressenti (« le boss est frustrant ») plutôt que la solution proposée (« baisse ses PV »), puis chercher la cause (lisibilité ? durée ? hasard ?).

## Créer une émotion

**5. Comprenez l'émotion que vous voulez provoquer.** Avant de créer quelque chose : « qu'est-ce que le joueur doit ressentir ? »
→ Le jeu parle d'émotions : chaque carte devrait faire *ressentir* la sienne. Colère = l'élan et le risque, Peur = la tension et le contrôle, Joie = le soulagement. Une carte de Peur qui ne crée aucune tension est mal placée.

**11. Si tout le monde aime mais que personne n'adore, vous échouerez.** Mieux vaut être adoré par certains que tiédi par tous.
→ Les champions doivent avoir des fans. Un champion « correct en tout » est un échec ; chacun doit avoir un moment où l'on se dit « c'est *mon* champion ».

**10. Ne cherchez pas à plaire à tout le monde avec chaque élément.** Un élément doit passionner une partie du public plutôt qu'être moyen pour tous.
→ Accepter des cartes de niche (une carte de combo pour Raze que la plupart ne joueront pas) tant que le deck de base reste accessible.

**16. Ayez davantage peur d'ennuyer que de challenger.** Une prise de risque qui échoue vaut mieux qu'un jeu ennuyeux.
→ Les phases du boss (lits, ombre qui se déplace, Au lit !) vont dans ce sens. Garder ce réflexe pour les monstres ordinaires : un monstre qui ne fait qu'avancer et frapper est à revoir.

## Forme et lisibilité

**2. L'esthétique compte.** Les gens recherchent la cohérence, la symétrie, les associations logiques.
→ Une seule palette d'émotions (`CodexCardVisual`), des coûts et des chiffres ronds, des noms de cartes qui disent ce qu'elles font. Une incohérence de forme (deux couleurs pour la même émotion, un libellé faux) se voit plus qu'on ne le croit.

**14. N'ayez pas peur d'être explicite.** Quand la subtilité ne passe pas, montrer clairement quoi faire.
→ Déjà appliqué : bandeau d'objectif du boss, « Vide ! » sur un lit vide, zone de dégâts au survol d'un monstre. Même réflexe pour les passifs (Main gagnante, Réflexe du grimpeur) : le joueur doit voir quand ils se déclenchent.

**7. Les petits détails peuvent créer l'amour du produit.** Un détail insignifiant devient important pour une partie du public.
→ Le texte d'ambiance d'une carte, l'animation de l'ombre qui file sous un lit, une réplique de champion. Prévoir un peu de temps pour ces détails dans le MVP, sans en faire une priorité.

## Liberté du joueur

**6. Laissez les joueurs personnaliser leur expérience.** Plus on fait de choix personnels, plus le jeu nous appartient.
**8. Donnez aux joueurs un sentiment de propriété.** On s'attache à ce qu'on a construit.
→ C'est le rôle du deck-building : 1 ou 2 couleurs par deck, 3 decks perso par champion. Ce qui manque encore pour la propriété : une progression qui se voit (XP et niveaux, reportés en V2) et des cartes gagnées plutôt que toutes disponibles d'emblée.

**9. Laissez de l'espace pour l'exploration.** Ne pas tout montrer ; laisser découvrir les combinaisons.
→ Les synergies entre une Signature et une émotion (Crux + Peur pour tirer et percuter, Evan + Colère pour doubler par l'écho) n'ont pas besoin d'être expliquées : le joueur doit pouvoir les trouver seul. Ne pas tout écrire dans les tutoriels.

## Règles et stratégie

**13. Le plaisir doit être la bonne stratégie.** Les joueurs chercheront à gagner, même si la stratégie optimale est ennuyeuse.
→ Question à poser à chaque playtest : quelle est la stratégie gagnante, et est-elle amusante ? Exemples de dérives à guetter : rester hors de portée et attendre, ne jouer que des cartes à 1 PA, retenir l'Éveil indéfiniment. Si la stratégie optimale est ennuyeuse, c'est la règle qu'il faut changer, pas le joueur.

**17. Vous n'avez pas besoin de beaucoup changer pour tout changer.** De petites modifications transforment une expérience ; se demander ce qu'on peut retirer.
→ Avant d'ajouter une mécanique pour corriger un problème, essayer d'abord de changer un chiffre ou de retirer une règle (le retrait des emplacements d'Éveil du deck, le 30/09, en est un exemple).

**18. Les contraintes stimulent la créativité.** Moins de possibilités produit des idées plus originales.
→ Le budget de PA des cartes, la grille 4 directions, les 3 émotions de lancement et le deck de 20 cartes sont des contraintes utiles : les garder plutôt que de les assouplir au premier problème.

**12. Ne créez pas quelque chose uniquement pour prouver que vous pouvez le faire.** La seule question : est-ce que cela améliore l'expérience du joueur ?
→ Rejoint les signaux d'alerte du contrat de co-architecte : une mécanique techniquement brillante mais invisible pour le joueur ne mérite pas sa semaine de travail.

## Synthèse

**20. Toutes les leçons sont connectées.** Elles forment un système cohérent autour de la compréhension du joueur et de la création d'une expérience forte.

**Questions rapides avant d'acter une mécanique ou une carte :**
- Quelle émotion le joueur doit-il ressentir ? (5)
- Pour qui est-ce fait : Timmy, Johnny, Spike ? (15)
- Est-ce que ça s'appuie sur quelque chose que le joueur connaît déjà ? (3, 4)
- La façon optimale de jouer est-elle amusante ? (13)
- Le joueur voit-il clairement ce qui se passe ? (14)
- Peut-on obtenir le même effet en changeant ou en retirant quelque chose plutôt qu'en ajoutant ? (17)
