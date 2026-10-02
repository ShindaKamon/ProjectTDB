using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit _enemyUnit;
    private Enemy _enemy; // Référence spécifique pour les ennemis avec cartes

    void Awake()
    {
        // OPTIMISATION Phase 3.3: ComponentLocator
        _enemyUnit = this.GetRequiredComponent<Unit>("EnemyAI nécessite Unit");
        if (_enemyUnit == null)
        {
            enabled = false;
            return;
        }

        // Vérifie si c'est un Enemy avec système de cartes (OPTIMISATION Phase 3.3: ComponentLocator)
        this.TryGetComponentSafe(out _enemy);
    }

    private void OnEnable() => EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
    private void OnDisable() => EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);

    // Nouvelle phase : le pattern repart du début, les trajets d'Au lit ! affichés ne valent plus
    private void OnBossPhaseChanged(BossPhaseChangedEvent e)
    {
        if (e.Boss == _enemy) PublishSweepPreview();
    }

    public void TakeTurn()
    {
        // Lance la coroutine pour gérer le tour de l'ennemi
        StartCoroutine(TakeTurnCoroutine());
    }

    private IEnumerator TakeTurnCoroutine()
    {
        GameLog.Log($"=== {_enemyUnit.name} (Ennemi) prend son tour ===");

        // Lancer annoncé au tour précédent : ses zones tombent maintenant, avant tout le reste
        if (_enemy != null && _enemy.HasPendingThrow) yield return StartCoroutine(ResolveThrow());

        // Boss dans un lit (caché ou fusionné) : il frappe de là sans se déplacer
        BedHiding hiding = GetComponent<BedHiding>();
        bool hidden = hiding != null && hiding.IsHiding;

        // Passif « Tapi dans le noir » (dans un lit ou terrain assombri), puis l'ombre qu'il avait posée se dissipe
        if (_enemy != null && _enemy.OnOwnTurnStart(hidden || TerrainDarkness.IsDark) > 0) yield return new WaitForSeconds(0.5f);
        TerrainDarkness.OnTurnStart(_enemyUnit);

        // Embuscade annoncée au tour précédent (ex. Frayeur) : il surgit au contact du champion le plus faible
        if (_enemy != null && _enemy.PendingAmbush != null) yield return StartCoroutine(ResolveAmbush());

        // PHASE 0 : les PM ont été remis à niveau par GridManager, puis les retraits appliqués.
        // Contrôlé = a perdu des PM ce tour (règle anti-lock, voir TryBasicAttack ; pas de PA chez les monstres)
        bool controlled = _enemy != null && _enemyUnit.GetCurrentMovementPoints() < _enemyUnit.GetMaxMovementPoints();

        // 1. Trouver le joueur le plus proche
        List<Unit> playerUnits = Services.Grid.GetAllPlayerUnits();

        if (playerUnits == null || playerUnits.Count == 0)
        {
            GameLog.LogWarning($"{_enemyUnit.name}: Aucune unité joueur trouvée. Passe son tour.");
            EventBus.Publish(new TurnEndRequestedEvent(_enemyUnit));
            yield break;
        }

        Unit closestPlayerUnit = FindClosestPlayer(playerUnits, GetMaxCardRange());

        if (closestPlayerUnit == null)
        {
            GameLog.LogWarning($"{_enemyUnit.name}: Aucun joueur valide trouvé.");
            EventBus.Publish(new TurnEndRequestedEvent(_enemyUnit));
            yield break;
        }

        GameLog.Log($"{_enemyUnit.name} cible {closestPlayerUnit.name}");

        // 2. DÉPLACEMENT (seulement si la prochaine carte nécessite de cibler un ennemi)
        bool needsToMoveCloser = false;

        if (_enemy != null)
        {
            CardData nextCard = _enemy.GetNextCard();

            // Ne bouge que si la carte cible un ennemi
            if (nextCard != null && nextCard.targetType == CardTargetType.Enemy)
            {
                int maxCardRange = nextCard.targetRange;
                int currentDistance = GridGeometry.Distance(_enemyUnit.GetCurrentGridPos(), closestPlayerUnit.GetCurrentGridPos());

                // Même règle que le joueur : portée en 4 directions (Manhattan), sans contrainte d'alignement
                if (currentDistance > maxCardRange)
                {
                    needsToMoveCloser = true;
                    GameLog.Log($"{_enemyUnit.name} doit se rapprocher (distance: {currentDistance}, portée: {maxCardRange})");
                }
                else
                {
                    GameLog.Log($"{_enemyUnit.name} est déjà à portée ({currentDistance} <= {maxCardRange}), pas de déplacement");
                }
            }
            else if (nextCard != null)
            {
                GameLog.Log($"{_enemyUnit.name} a une carte {nextCard.targetType}, pas besoin de se rapprocher");
            }
        }
        else
        {
            // Pas de système de cartes, comportement par défaut (se rapproche)
            needsToMoveCloser = true;
        }

        // Garde du corps (ex. soldat de bois) : se place entre le mob qu'il protège et le champion qui le menace
        List<Vector2Int> guardPath = null;
        bool guarding = !hidden && TryGuardAlly(playerUnits, out guardPath);
        if (guarding && guardPath.Count > 0)
        {
            List<Tile> tiles = guardPath.ConvertAll(p => Services.Grid.GetTileAtPosition(p));
            _enemyUnit.MoveToTile(tiles);
            _enemyUnit.SpendMovement(tiles.Count);
            yield return new WaitUntil(() => !_enemyUnit.IsMoving());
        }

        if (needsToMoveCloser && !hidden && !guarding)
        {
            // Calculer le chemin complet vers le joueur en utilisant tous les points de mouvement
            List<Tile> fullPath = CalculatePathTowardsTarget(closestPlayerUnit.GetCurrentGridPos());

            if (fullPath != null && fullPath.Count > 0)
            {
                int movementCost = fullPath.Count;
                GameLog.Log($"{_enemyUnit.name} se déplace de {movementCost} cases vers {closestPlayerUnit.name}");

                // Déplace l'unité le long du chemin (fluide, comme le joueur)
                _enemyUnit.MoveToTile(fullPath);
                _enemyUnit.SpendMovement(movementCost);

                // Attendre que le mouvement soit terminé
                yield return new WaitUntil(() => !_enemyUnit.IsMoving());
            }
            else
            {
                GameLog.LogWarning($"{_enemyUnit.name}: Aucun chemin valide trouvé.");
            }
        }

        // 3. PHASE CARTES: Tente de jouer une carte APRÈS le déplacement (si Enemy avec cartes)
        bool cardPlayed = false;
        // Carte annulée (ex: Sidération) : pas de carte ni d'attaque de base ce tour
        bool cardCancelled = _enemy != null && _enemy.ConsumeCancelledCard();
        // Carte entravée (ex: Aura de terreur) : attaque de base à la place, la carte revient au tour suivant
        bool cardHindered = _enemy != null && _enemy.ConsumeHinderedCard() && !cardCancelled;
        if (_enemy != null && !cardCancelled && !cardHindered)
        {
            CardData nextCard = _enemy.GetNextCard();
            if (nextCard != null)
            {
                GameLog.Log($"{_enemy.name} veut jouer la carte: {nextCard.cardName}");

                // Vérifie si la carte peut être jouée (cible valide, portée, etc.)
                bool canPlayCard = CanPlayCard(nextCard);

                if (canPlayCard)
                {
                    GameLog.Log($"{_enemy.name} PEUT jouer {nextCard.cardName}, pioche la carte");

                    // MAINTENANT on pioche la carte (avance l'index)
                    CardData playedCard = _enemy.DrawAndPlayNextCard();
                    if (playedCard != null)
                    {
                        // Ex. Au lit ! : les tas de débris filent d'abord vers son lit
                        yield return StartCoroutine(SweepPiles(playedCard, hiding));
                        // Exécute l'effet de la carte ; un lancer annoncé ne frappe qu'au prochain tour
                        if (playedCard.isAmbush) _enemy.AnnounceAmbush(playedCard); // il frappera au prochain tour
                        else if (playedCard.telegraphedZoneCount > 0) yield return StartCoroutine(AnnounceThrow(playedCard));
                        else yield return StartCoroutine(ExecuteEnemyCard(playedCard));
                        cardPlayed = true;
                        ApplyCardSideEffects(playedCard, hiding);
                    }
                }
                else if (_enemy.IsBoss())
                {
                    // Boss : la carte part dans le vide et le pattern avance (décision du 02/10/2026), sinon un joueur
                    // resté hors de portée le bloquerait sur la même carte et on ne verrait jamais la suite du pattern.
                    // Ses effets sans cible (ombre, changement de lit, invocation) ont lieu quand même.
                    _enemy.DrawAndPlayNextCard();
                    yield return StartCoroutine(SweepPiles(nextCard, hiding));
                    ApplyCardSideEffects(nextCard, hiding);
                    GameLog.Log($"{_enemy.name}: {nextCard.cardName} sans cible à portée, joue dans le vide (pattern suivant)");
                }
                else
                {
                    GameLog.Log($"{_enemy.name}: Ne PEUT PAS jouer {nextCard.cardName} (pas de cible/hors portée), garde la carte pour le prochain tour");
                }
            }
        }

        // 4. Attaque de base : carte entravée (ex. Aura de terreur), ou boss bloqué par un contrôle (règle anti-lock).
        // Monstre ordinaire qui ne peut pas jouer sa carte : il passe son tour et la garde (décision du 02/10/2026)
        bool isMinion = _enemy != null && !_enemy.IsBoss();
        if (!cardPlayed && !cardCancelled && (cardHindered || (controlled && !isMinion)))
        {
            yield return StartCoroutine(TryBasicAttack());
        }

        // 5. Fin du tour : si sa prochaine carte est Au lit !, les trajets des tas s'affichent dès maintenant
        PublishSweepPreview();
        EventBus.Publish(new TurnEndRequestedEvent(_enemyUnit));
    }

    /// <summary>
    /// Joue l'attaque de base du monstre (EnemyData.basicAttack) : gratuite, insensible au contrôle,
    /// sans avancer son pattern de cartes. Rien si elle n'est pas définie ou sans cible à portée.
    /// </summary>
    private IEnumerator TryBasicAttack()
    {
        CardData basicAttack = _enemy.BasicAttack; // celle de la phase en cours
        if (basicAttack == null)
        {
            GameLog.LogWarning($"{_enemy.name}: carte injouable mais aucune attaque de base définie (EnemyData.basicAttack)");
            yield break;
        }

        if (!CanPlayCard(basicAttack))
        {
            GameLog.Log($"{_enemy.name}: attaque de base impossible (pas de cible à portée)");
            yield break;
        }

        GameLog.Log($"{_enemy.name} ne peut pas jouer sa carte : attaque de base ({basicAttack.cardName})");
        yield return StartCoroutine(ExecuteEnemyCard(basicAttack));
    }

    // Lancer annoncé : les zones s'affichent au sol et tomberont au début du prochain tour du monstre
    private IEnumerator AnnounceThrow(CardData card)
    {
        // Ex. Bric-à-brac : il ramasse un tas de débris au hasard (il disparaît) ; sans débris au sol, la carte est
        // morte (décision du 02/10/2026)
        if (card.throwsDebris)
        {
            List<Unit> piles = Services.Grid.GetAllUnits().FindAll(u => u is DebrisUnit);
            if (piles.Count == 0)
            {
                GameLog.Log($"{_enemy.name} : {card.cardName}, plus rien à lancer");
                yield break;
            }
            piles[_enemy.Rng.Next(piles.Count)].Despawn();
        }

        List<Vector2Int> playerCells = Services.Grid.GetAllPlayerUnits().ConvertAll(u => u.GetCurrentGridPos());
        // Rien ne tombe sur un lit ni sur des débris (Enemies.md)
        List<Vector2Int> cells = Services.Grid.GetAllCells().FindAll(c => !(Services.Grid.GetUnitAtGridPos(c) is BedUnit || Services.Grid.GetUnitAtGridPos(c) is DebrisUnit));
        _enemy.AnnounceThrow(card, cells, playerCells);
        yield return new WaitForSeconds(0.5f);
    }

    // Les zones annoncées au tour précédent tombent : la carte s'applique sur chacune
    private IEnumerator ResolveThrow()
    {
        var epicenters = new List<Vector2Int>();
        EnemyData toy = _enemy.PendingToy;
        Vector2Int toyCell = _enemy.PendingToyCell;
        CardData card = _enemy.TakePendingThrow(epicenters);
        GameLog.Log($"{_enemy.name} : {card.cardName} tombe sur {epicenters.Count} zone(s)");
        for (int i = 0; i < epicenters.Count; i++)
            card.ExecuteEffect(_enemy, null, epicenters[i], i > 0);

        // Le jouet s'anime s'il est tombé sur une case vide (sinon il a frappé comme les autres)
        if (toy != null && Services.Grid.GetTileAtPosition(toyCell) != null && Services.Grid.GetUnitAtGridPos(toyCell) == null)
            Services.Grid.SpawnEnemy(toy, toyCell);

        // Ex. Pluie de jouets : ce qui est tombé sur une case vide y reste en tas (munitions de Bric-à-brac)
        if (card.pileOnEmptyCell != null)
            foreach (Vector2Int cell in epicenters)
                if (Services.Grid.GetTileAtPosition(cell) != null && Services.Grid.GetUnitAtGridPos(cell) == null)
                    Services.Grid.SpawnDebris(card.pileOnEmptyCell, new[] { cell }, Quaternion.Euler(0f, _enemy.Rng.Next(4) * 90f, 0f));
        yield return new WaitForSeconds(0.5f);
    }

    // Ex. Au lit ! : les draps ramènent à son lit tous les tas de débris du plateau, qui disparaissent ; un champion sur
    // le trajet d'un tas (PileSweep) est touché, une fois par tas qui le traverse
    private IEnumerator SweepPiles(CardData card, BedHiding hiding)
    {
        if (card.pulledPileDamage <= 0) yield break;
        List<Unit> piles = Services.Grid.GetAllUnits().FindAll(u => u is DebrisUnit);
        EventBus.Publish(new ThrowZonesChangedEvent(_enemy, new List<Vector2Int>(), isSweep: true));
        if (piles.Count == 0) yield break;

        List<Vector2Int> lit = SweepTargetCells(hiding);
        Vector3 litPosition = hiding != null && hiding.CurrentBed != null ? hiding.CurrentBed.transform.position : transform.position;
        var starts = new List<Vector3>();
        foreach (Unit pile in piles) starts.Add(pile.transform.position);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f)
        {
            for (int i = 0; i < piles.Count; i++) piles[i].transform.position = Vector3.Lerp(starts[i], litPosition, t);
            yield return null;
        }

        foreach (Unit pile in piles)
        {
            List<Vector2Int> path = PileSweep.Path(new List<Vector2Int>(pile.OccupiedCells), lit);
            foreach (Unit champion in Services.Grid.GetAllPlayerUnits())
                if (path.Contains(champion.GetCurrentGridPos())) champion.TakeDamageFrom(card.pulledPileDamage, _enemyUnit);
            pile.Despawn();
        }
        GameLog.Log($"{_enemy.name} : {card.cardName} ramène {piles.Count} tas de débris");
        yield return new WaitForSeconds(0.3f);
    }

    // Cases vers lesquelles filent les tas : son lit (boss caché ou fusionné), sinon ses propres cases
    private List<Vector2Int> SweepTargetCells(BedHiding hiding) =>
        new List<Vector2Int>(hiding != null && hiding.CurrentBed != null ? hiding.CurrentBed.OccupiedCells : _enemyUnit.OccupiedCells);

    // Trajets des tas affichés au sol tant que sa prochaine carte ramène les débris (vide sinon)
    private void PublishSweepPreview()
    {
        if (_enemy == null) return;
        var cells = new List<Vector2Int>();
        CardData next = _enemy.GetNextCard();
        if (next != null && next.pulledPileDamage > 0)
        {
            List<Vector2Int> lit = SweepTargetCells(GetComponent<BedHiding>());
            foreach (Unit pile in Services.Grid.GetAllUnits().FindAll(u => u is DebrisUnit))
                foreach (Vector2Int cell in PileSweep.Path(new List<Vector2Int>(pile.OccupiedCells), lit))
                    if (!cells.Contains(cell)) cells.Add(cell);
        }
        EventBus.Publish(new ThrowZonesChangedEvent(_enemy, cells, isSweep: true));
    }

    // Embuscade (ex. Frayeur) : il surgit au contact du champion qui a le moins de PV et lui applique la carte
    private IEnumerator ResolveAmbush()
    {
        CardData card = _enemy.TakePendingAmbush();
        List<Unit> champions = Services.Grid.GetAllPlayerUnits();
        Unit target = null;
        foreach (Unit champion in champions)
            if (target == null || champion.GetHealth() < target.GetHealth()) target = champion;
        if (target == null) yield break;

        Vector2Int pos = _enemyUnit.GetCurrentGridPos();
        var others = champions.FindAll(c => c != target).ConvertAll(c => c.GetCurrentGridPos());
        Vector2Int? cell = AmbushCell(target.GetCurrentGridPos(), others,
            p => p == pos || (Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null));
        if (cell.HasValue) _enemyUnit.TeleportTo(cell.Value);
        GameLog.Log($"{_enemy.name} surgit au contact de {target.name} ({card.cardName})");

        yield return StartCoroutine(_enemy.LookAtCoroutine(target.transform.position));
        card.ExecuteEffect(_enemy, target);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Case de l'embuscade : la case libre au contact de la cible la plus éloignée des autres champions (ordre fixe des
    /// 4 directions à égalité) ; si la cible est encerclée, la case libre la plus proche. Null s'il n'y en a aucune.
    /// </summary>
    public static Vector2Int? AmbushCell(Vector2Int target, IList<Vector2Int> otherChampions, System.Func<Vector2Int, bool> isFree)
    {
        Vector2Int? best = null;
        int bestScore = int.MinValue;
        foreach (Vector2Int dir in GridGeometry.Directions4)
        {
            Vector2Int cell = target + dir;
            if (!isFree(cell)) continue;
            int score = int.MaxValue; // sans autre champion, toutes les cases se valent
            foreach (Vector2Int other in otherChampions) score = Mathf.Min(score, GridGeometry.Distance(cell, other));
            if (score > bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }
        return best ?? NearestFreeCell(target, isFree);
    }

    // Effets d'une carte de monstre qui ne visent personne : ils ont lieu même si la carte part dans le vide
    private void ApplyCardSideEffects(CardData card, BedHiding hiding)
    {
        // Ex. Invocation de mouton : le monstre sort d'un lit (boss caché) ou apparaît à côté du lanceur
        if (card.spawnedEnemy != null)
        {
            Vector2Int origin = hiding != null && hiding.IsHiding ? hiding.SpawnOrigin() : _enemyUnit.GetCurrentGridPos();
            Vector2Int? cell = NearestFreeCell(origin, p => Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null);
            if (cell.HasValue) Services.Grid.SpawnEnemy(card.spawnedEnemy, cell.Value);
        }

        // Ex. Marée d'ombre : tout le terrain s'assombrit jusqu'à son prochain tour
        if (card.darkensTerrain) TerrainDarkness.Darken(_enemyUnit);

        // Ex. Marée d'ombre : le boss caché passe sous un autre lit
        if (card.changesHidingSpot && hiding != null) hiding.MoveToAnotherBed();
    }

    // Garde du corps : mob à protéger (le plus proche, ni boss ni autre garde) et chemin du tour vers sa case de garde.
    // False s'il n'est pas garde du corps ou n'a personne à protéger (il agit alors comme les autres).
    private bool TryGuardAlly(List<Unit> playerUnits, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        if (_enemy == null || _enemy.GetEnemyData() == null || !_enemy.GetEnemyData().guardsAllies) return false;

        Vector2Int pos = _enemyUnit.GetCurrentGridPos();
        var protegees = Services.Grid.GetAllEnemyUnits().FindAll(u =>
            u != _enemyUnit && u is Enemy e && !e.IsBoss() && e.GetEnemyData() != null && !e.GetEnemyData().guardsAllies);
        Unit protegee = ChooseTarget(pos, protegees);
        if (protegee == null) return false;

        Unit threat = ChooseTarget(protegee.GetCurrentGridPos(), playerUnits);
        if (threat == null) return false;

        bool IsFree(Vector2Int p) => p == pos || (Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null);
        Vector2Int? cell = GuardCell(protegee.GetCurrentGridPos(), threat.GetCurrentGridPos(), pos, IsFree);
        if (cell.HasValue && cell.Value != pos)
            path = PathTowards(pos, cell.Value, 0, _enemyUnit.GetCurrentMovementPoints(), IsFree, reachTarget: true);
        GameLog.Log($"{_enemyUnit.name} protège {protegee.name} de {threat.name}");
        return true;
    }

    /// <summary>
    /// Case de garde : la case libre au contact du protégé la plus proche du champion qui le menace (à égalité,
    /// la plus proche du garde). guard est la case actuelle du garde (comptée libre). Null si le protégé est encerclé.
    /// </summary>
    public static Vector2Int? GuardCell(Vector2Int protegee, Vector2Int threat, Vector2Int guard, System.Func<Vector2Int, bool> isFree)
    {
        Vector2Int? best = null;
        int bestScore = int.MaxValue;
        foreach (Vector2Int dir in GridGeometry.Directions4)
        {
            Vector2Int cell = protegee + dir;
            if (cell != guard && !isFree(cell)) continue;
            int score = GridGeometry.Distance(cell, threat) * 100 + GridGeometry.Distance(cell, guard);
            if (score < bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }
        return best;
    }

    // Calcule le chemin du tour vers la cible, en contournant les unités (voir PathTowards)
    private List<Tile> CalculatePathTowardsTarget(Vector2Int targetPos)
    {
        List<Vector2Int> cells = PathTowards(_enemyUnit.GetCurrentGridPos(), targetPos, GetMaxCardRange(),
            _enemyUnit.GetCurrentMovementPoints(),
            p => Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null);
        return cells.ConvertAll(p => Services.Grid.GetTileAtPosition(p));
    }

    /// <summary>
    /// Chemin (sans la case de départ, au plus « movement » cases) vers la case libre la plus
    /// proche de la cible — à portée d'attaque si possible —, en contournant les unités (recherche
    /// en largeur, 4 directions) : un monstre bloqué derrière un autre fait le tour. Vide s'il est
    /// déjà à portée ou n'a aucun chemin.
    /// </summary>
    public static List<Vector2Int> PathTowards(Vector2Int start, Vector2Int target, int attackRange, int movement,
        System.Func<Vector2Int, bool> isFree, bool reachTarget = false)
    {
        var path = new List<Vector2Int>();
        // sans carte : au contact ; reachTarget : sur la case elle-même (case libre visée, ex. case de garde)
        int range = reachTarget ? 0 : Mathf.Max(1, attackRange);
        int Gap(Vector2Int p) => Mathf.Max(0, GridGeometry.Distance(p, target) - range);
        if (movement <= 0 || Gap(start) == 0) return path;

        // Largeur d'abord : la première case trouvée à un écart donné est aussi la plus proche en pas
        var parent = new Dictionary<Vector2Int, Vector2Int> { [start] = start };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        Vector2Int best = start;
        int bestGap = Gap(start);
        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            int gap = Gap(p);
            if (gap < bestGap)
            {
                best = p;
                bestGap = gap;
                if (gap == 0) break;
            }
            foreach (Vector2Int dir in GridGeometry.Directions4)
            {
                Vector2Int next = p + dir;
                if (parent.ContainsKey(next) || !isFree(next)) continue;
                parent[next] = p;
                queue.Enqueue(next);
            }
        }

        for (Vector2Int p = best; p != start; p = parent[p]) path.Add(p);
        path.Reverse();
        if (path.Count > movement) path.RemoveRange(movement, path.Count - movement);
        return path;
    }

    /// <summary>
    /// Case libre la plus proche de origin (origin exclue), en largeur d'abord sur 4 directions, dans un ordre
    /// fixe (même résultat sur tous les PC) ; null si aucune case libre n'est atteignable à moins de 10 pas.
    /// </summary>
    public static Vector2Int? NearestFreeCell(Vector2Int origin, System.Func<Vector2Int, bool> isFree)
    {
        var seen = new HashSet<Vector2Int> { origin };
        var queue = new Queue<(Vector2Int cell, int steps)>();
        queue.Enqueue((origin, 0));
        while (queue.Count > 0)
        {
            (Vector2Int p, int steps) = queue.Dequeue();
            if (steps >= 10) continue;
            foreach (Vector2Int dir in GridGeometry.Directions4)
            {
                Vector2Int next = p + dir;
                if (!seen.Add(next)) continue;
                if (isFree(next)) return next;
                queue.Enqueue((next, steps + 1)); // on traverse les cases occupées (lits, unités)
            }
        }
        return null;
    }

    /// <summary>
    /// Obtient la portée maximale des cartes de l'ennemi
    /// </summary>
    private int GetMaxCardRange()
    {
        if (_enemy == null || _enemy.GetNextCard() == null) return 0;

        // Pour l'instant, utilise la portée de la prochaine carte
        // (on pourrait aussi chercher la carte avec la plus grande portée du deck)
        CardData nextCard = _enemy.GetNextCard();
        return nextCard.targetRange;
    }

    // Cible du monstre pour une attaque de portée attackRange (voir ChooseTarget)
    private Unit FindClosestPlayer(List<Unit> playerUnits, int attackRange) =>
        ChooseTarget(AttackOrigins(), playerUnits, attackRange);

    // Cases d'où le monstre frappe : la sienne, ou n'importe quel lit quand le boss est dans un lit (BedHiding)
    private List<Vector2Int> AttackOrigins()
    {
        BedHiding hiding = GetComponent<BedHiding>();
        if (hiding != null && hiding.IsHiding)
        {
            List<Vector2Int> beds = hiding.AttackOrigins();
            if (beds.Count > 0) return beds;
        }
        return new List<Vector2Int> { _enemyUnit.GetCurrentGridPos() };
    }

    private int DistanceToTarget(Unit target) => DistanceFrom(AttackOrigins(), target.GetCurrentGridPos());

    /// <summary>Distance (4 directions) de la case d'origine la plus proche à la cellule donnée.</summary>
    public static int DistanceFrom(IList<Vector2Int> origins, Vector2Int cell)
    {
        int best = int.MaxValue;
        foreach (Vector2Int origin in origins) best = Mathf.Min(best, GridGeometry.Distance(origin, cell));
        return best;
    }

    /// <summary>Comme ChooseTarget, la distance étant mesurée depuis la plus proche des cases d'origine.</summary>
    public static Unit ChooseTarget(IList<Vector2Int> origins, IEnumerable<Unit> candidates, int attackRange = 0)
    {
        Unit best = null;
        int bestDistance = int.MaxValue;
        foreach (Unit unit in candidates)
        {
            if (unit == null) continue;
            int distance = Mathf.Max(attackRange, DistanceFrom(origins, unit.GetCurrentGridPos()));
            if (distance < bestDistance || (distance == bestDistance && unit.GetHealth() < best.GetHealth()))
            {
                bestDistance = distance;
                best = unit;
            }
        }
        return best;
    }

    /// <summary>
    /// Ciblage de base des monstres (décision du 28/09/2026, pourra varier selon le boss) : toutes
    /// les cibles à portée de l'attaque comptent comme aussi proches, et parmi elles le monstre vise
    /// celle qui a le moins de PV ; si aucune n'est à portée, la plus proche (cases en 4 directions),
    /// puis le moins de PV à distance égale.
    /// </summary>
    public static Unit ChooseTarget(Vector2Int from, IEnumerable<Unit> candidates, int attackRange = 0) =>
        ChooseTarget(new[] { from }, candidates, attackRange); // à portée = aussi proche que toute autre cible à portée

    /// <summary>
    /// Vérifie si une carte peut être jouée (cible valide, portée OK, etc.)
    /// </summary>
    private bool CanPlayCard(CardData card)
    {
        // Carte sans cible ou auto-ciblée: toujours jouable
        if (card.targetType == CardTargetType.None || card.targetType == CardTargetType.Self)
        {
            return true;
        }

        // Trouve le joueur le plus proche pour vérifier la portée
        List<Unit> playerUnits = Services.Grid.GetAllPlayerUnits();
        if (playerUnits == null || playerUnits.Count == 0)
        {
            GameLog.LogWarning($"{_enemy.name}: Aucun joueur pour cibler {card.cardName}");
            return false; // Pas de cible disponible
        }

        Unit closestPlayer = FindClosestPlayer(playerUnits, card.targetRange);
        if (closestPlayer == null)
        {
            return false;
        }

        // Vérifie la portée pour les cartes ciblant l'ennemi (4 directions, comme le joueur ; boss dans un lit : depuis n'importe quel lit)
        if (card.targetType == CardTargetType.Enemy)
        {
            int distance = DistanceToTarget(closestPlayer);
            if (distance > card.targetRange)
            {
                GameLog.Log($"{_enemy.name}: {card.cardName} hors de portée (distance: {distance}, portée: {card.targetRange})");
                return false;
            }
        }

        // La carte peut être jouée!
        return true;
    }

    /// <summary>
    /// Exécute l'effet d'une carte ennemie avec intelligence de ciblage
    /// </summary>
    private IEnumerator ExecuteEnemyCard(CardData card)
    {
        GameLog.Log($"{_enemy.name} exécute la carte: {card.cardName}");

        // Détermine la cible en fonction du type de carte
        Unit targetUnit = null;
        Vector2Int targetTile = Vector2Int.zero;

        // Trouve le joueur le plus proche pour les cartes offensives
        List<Unit> playerUnits = Services.Grid.GetAllPlayerUnits();
        if (playerUnits != null && playerUnits.Count > 0)
        {
            Unit closestPlayer = FindClosestPlayer(playerUnits, card.targetRange);

            // Vérifie si la carte cible une unité
            if (card.targetsUnit)
            {
                // Carte offensive contre joueur
                if (card.targetType == CardTargetType.Enemy)
                {
                    // Vérifie la portée (4 directions ; boss dans un lit : depuis n'importe quel lit)
                    int distance = DistanceToTarget(closestPlayer);
                    if (distance <= card.targetRange)
                    {
                        targetUnit = closestPlayer;
                    }
                    else
                    {
                        GameLog.LogWarning($"{_enemy.name}: Cible hors de portée pour {card.cardName}");
                    }
                }
                // Carte sur soi-même (ex. soin)
                else if (card.targetType == CardTargetType.Self)
                {
                    targetUnit = _enemy;
                }
            }
            // Carte ciblant une tuile
            else if (card.targetsTile)
            {
                // Pour l'instant, cible la position du joueur le plus proche
                targetTile = closestPlayer.GetCurrentGridPos();
            }
        }

        // Tourne pour faire face à la cible avant d'exécuter l'effet
        if (targetUnit != null && targetUnit != _enemy) // Ne tourne pas pour s'auto-cibler
        {
            yield return StartCoroutine(_enemy.LookAtCoroutine(targetUnit.transform.position));
        }
        else if (card.targetsTile)
        {
            Tile tile = Services.Grid.GetTileAtPosition(targetTile);
            if (tile != null)
            {
                // Cible le centre de la tuile
                Vector3 targetWorldPos = tile.transform.position + new Vector3(0, 0.5f, 0);
                yield return StartCoroutine(_enemy.LookAtCoroutine(targetWorldPos));
            }
        }

        // Exécute l'effet de la carte
        if (card.targetsUnit && targetUnit != null)
        {
            card.ExecuteEffect(_enemy, targetUnit);
        }
        else if (card.targetsTile)
        {
            card.ExecuteEffect(_enemy, null, targetTile);
        }
        else if (card.targetType == CardTargetType.None || card.targetType == CardTargetType.Self)
        {
            // Carte sans cible ou auto-ciblée
            card.ExecuteEffect(_enemy, _enemy);
        }

        // Petit délai pour l'effet visuel
        yield return new WaitForSeconds(0.5f);
    }
}
