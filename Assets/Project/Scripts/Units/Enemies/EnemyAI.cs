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

    public void TakeTurn()
    {
        // Lance la coroutine pour gérer le tour de l'ennemi
        StartCoroutine(TakeTurnCoroutine());
    }

    private IEnumerator TakeTurnCoroutine()
    {
        GameLog.Log($"=== {_enemyUnit.name} (Ennemi) prend son tour ===");

        // PHASE 0 : les PA ont été remis à niveau par GridManager, puis les retraits appliqués.
        // Contrôlé = a perdu des PA ou des PM ce tour (règle anti-lock, voir TryBasicAttack)
        bool controlled = _enemy != null
            && (_enemy.GetCurrentPA() < _enemy.GetMaxPA() || _enemyUnit.GetCurrentMovementPoints() < _enemyUnit.GetMaxMovementPoints());

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

        if (needsToMoveCloser)
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
        if (_enemy != null && !cardCancelled)
        {
            CardData nextCard = _enemy.GetNextCard();
            if (nextCard != null && _enemy.GetCurrentPA() >= nextCard.costPA)
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
                        // Dépense les PA
                        _enemy.SpendPA(playedCard.costPA);

                        // Exécute l'effet de la carte
                        yield return StartCoroutine(ExecuteEnemyCard(playedCard));
                        cardPlayed = true;
                    }
                }
                else
                {
                    GameLog.Log($"{_enemy.name}: Ne PEUT PAS jouer {nextCard.cardName} (pas de cible/hors portée), garde la carte pour le prochain tour");
                }
            }
            else if (nextCard != null)
            {
                GameLog.Log($"{_enemy.name}: Pas assez de PA pour jouer {nextCard.cardName}");
            }
        }

        // 4. Attaque de base (0 PA) quand la carte prévue n'a pas pu être jouée ; la carte reste pour
        // le prochain tour. Boss : seulement s'il est bloqué par un contrôle (règle anti-lock).
        // Monstres ordinaires : dès que la carte est injouable, contrôlés ou non (décision du 28/09/2026)
        bool isMinion = _enemy != null && !_enemy.IsBoss();
        if (!cardPlayed && !cardCancelled && (controlled || isMinion))
        {
            yield return StartCoroutine(TryBasicAttack());
        }

        // 5. Fin du tour
        EventBus.Publish(new TurnEndRequestedEvent(_enemyUnit));
    }

    /// <summary>
    /// Joue l'attaque de base du monstre (EnemyData.basicAttack) : gratuite, insensible au contrôle,
    /// sans avancer son pattern de cartes. Rien si elle n'est pas définie ou sans cible à portée.
    /// </summary>
    private IEnumerator TryBasicAttack()
    {
        CardData basicAttack = _enemy.GetEnemyData()?.basicAttack;
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
        System.Func<Vector2Int, bool> isFree)
    {
        var path = new List<Vector2Int>();
        int range = Mathf.Max(1, attackRange); // sans carte : au contact
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
        ChooseTarget(_enemyUnit.GetCurrentGridPos(), playerUnits, attackRange);

    /// <summary>
    /// Ciblage de base des monstres (décision du 28/09/2026, pourra varier selon le boss) : toutes
    /// les cibles à portée de l'attaque comptent comme aussi proches, et parmi elles le monstre vise
    /// celle qui a le moins de PV ; si aucune n'est à portée, la plus proche (cases en 4 directions),
    /// puis le moins de PV à distance égale.
    /// </summary>
    public static Unit ChooseTarget(Vector2Int from, IEnumerable<Unit> candidates, int attackRange = 0)
    {
        Unit best = null;
        int bestDistance = int.MaxValue;

        foreach (Unit unit in candidates)
        {
            if (unit == null) continue;

            // À portée = aussi proche que n'importe quelle autre cible à portée
            int distance = Mathf.Max(attackRange, GridGeometry.Distance(from, unit.GetCurrentGridPos()));
            if (distance < bestDistance || (distance == bestDistance && unit.GetHealth() < best.GetHealth()))
            {
                bestDistance = distance;
                best = unit;
            }
        }

        return best;
    }

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

        // Vérifie la portée pour les cartes ciblant l'ennemi (4 directions, comme le joueur)
        if (card.targetType == CardTargetType.Enemy)
        {
            int distance = GridGeometry.Distance(_enemy.GetCurrentGridPos(), closestPlayer.GetCurrentGridPos());
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
                    // Vérifie la portée (4 directions)
                    int distance = GridGeometry.Distance(_enemy.GetCurrentGridPos(), closestPlayer.GetCurrentGridPos());
                    if (distance <= card.targetRange)
                    {
                        targetUnit = closestPlayer;
                    }
                    else
                    {
                        GameLog.LogWarning($"{_enemy.name}: Cible hors de portée pour {card.cardName}");
                    }
                }
                // Carte de soin sur soi-même
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
