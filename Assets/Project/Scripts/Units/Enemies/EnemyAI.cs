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

        Unit closestPlayerUnit = FindClosestPlayer(playerUnits);

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
        if (_enemy != null)
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

        // 4. Règle anti-lock : bloqué par un contrôle (PA ou PM retirés), le monstre fait son
        // attaque de base (0 PA) ; sa carte prévue reste pour le prochain tour
        if (!cardPlayed && controlled)
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
            GameLog.LogWarning($"{_enemy.name}: bloqué par un contrôle mais aucune attaque de base définie (EnemyData.basicAttack)");
            yield break;
        }

        if (!CanPlayCard(basicAttack))
        {
            GameLog.Log($"{_enemy.name}: attaque de base impossible (pas de cible à portée)");
            yield break;
        }

        GameLog.Log($"{_enemy.name} est bloqué par un contrôle : attaque de base ({basicAttack.cardName})");
        yield return StartCoroutine(ExecuteEnemyCard(basicAttack));
    }

    // Calcule un chemin complet vers la cible en utilisant tous les points de mouvement disponibles
    private List<Tile> CalculatePathTowardsTarget(Vector2Int targetPos)
    {
        List<Tile> path = new List<Tile>();
        Vector2Int currentPos = _enemyUnit.GetCurrentGridPos();
        int remainingMovement = _enemyUnit.GetCurrentMovementPoints();

        // Obtient la portée max des cartes de l'ennemi (si c'est un Enemy avec cartes)
        int maxCardRange = GetMaxCardRange();

        // Construire le chemin case par case jusqu'à épuisement des points de mouvement
        // ou jusqu'à être à portée de carte
        for (int i = 0; i < remainingMovement; i++)
        {
            // Si l'ennemi a des cartes, s'arrête à portée de carte
            if (maxCardRange > 0)
            {
                int distance = GridGeometry.Distance(currentPos, targetPos);
                if (distance <= maxCardRange)
                {
                    GameLog.Log($"À portée de carte ({maxCardRange}) après {i} mouvements, distance: {distance}");
                    break; // On est assez proche pour jouer une carte
                }
            }

            // Trouver le meilleur prochain mouvement
            Vector2Int nextMove = FindBestMoveFrom(currentPos, targetPos);

            if (nextMove == Vector2Int.zero)
            {
                GameLog.Log($"Bloqué après {i} mouvements");
                break; // Bloqué, on ne peut plus avancer
            }

            // Ajouter cette case au chemin
            Tile nextTile = Services.Grid.GetTileAtPosition(nextMove);
            path.Add(nextTile);
            currentPos = nextMove; // Mettre à jour la position simulée
        }

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

    // Trouve le meilleur mouvement depuis une position donnée vers une cible (4 directions) :
    // la case libre la plus proche en cases, départagée par la distance réelle (trajet plus droit).
    private Vector2Int FindBestMoveFrom(Vector2Int fromPos, Vector2Int targetPos)
    {
        Vector2Int bestMove = Vector2Int.zero;
        int bestDistance = GridGeometry.Distance(fromPos, targetPos);
        float bestTieBreak = float.MaxValue;

        foreach (Vector2Int dir in GridGeometry.Directions4)
        {
            Vector2Int nextPos = fromPos + dir;

            if (Services.Grid.GetTileAtPosition(nextPos) == null) continue; // Case hors grille
            if (Services.Grid.GetUnitAtGridPos(nextPos) != null) continue;   // Case occupée

            int distance = GridGeometry.Distance(nextPos, targetPos);
            float tieBreak = Vector2.Distance(nextPos, targetPos);

            // Ne recule jamais : il faut se rapprocher (ou rester à distance égale en contournant)
            if (distance > bestDistance) continue;
            if (distance < bestDistance || tieBreak < bestTieBreak)
            {
                bestDistance = distance;
                bestTieBreak = tieBreak;
                bestMove = nextPos;
            }
        }

        return bestMove;
    }

    // Trouve le joueur le plus proche
    private Unit FindClosestPlayer(List<Unit> playerUnits)
    {
        Unit closestPlayerUnit = null;
        float minDistance = float.MaxValue;

        foreach (Unit playerUnit in playerUnits)
        {
            // Distance en cases (4 directions), départagée par la distance réelle
            float distance = GridGeometry.Distance(_enemyUnit.GetCurrentGridPos(), playerUnit.GetCurrentGridPos())
                             + 0.001f * Vector2.Distance(_enemyUnit.GetCurrentGridPos(), playerUnit.GetCurrentGridPos());
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayerUnit = playerUnit;
            }
        }

        return closestPlayerUnit;
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

        Unit closestPlayer = FindClosestPlayer(playerUnits);
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
            Unit closestPlayer = FindClosestPlayer(playerUnits);

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
