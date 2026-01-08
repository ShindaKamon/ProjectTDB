using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit _enemyUnit;
    private Enemy _enemy; // Référence spécifique pour les ennemis avec cartes

    void Awake()
    {
        _enemyUnit = GetComponent<Unit>();
        if (_enemyUnit == null)
        {
            Debug.LogError("EnemyAI nécessite un composant Unit sur le même GameObject.");
            enabled = false;
            return;
        }

        // Vérifie si c'est un Enemy avec système de cartes
        _enemy = GetComponent<Enemy>();
    }

    public void TakeTurn()
    {
        // Lance la coroutine pour gérer le tour de l'ennemi
        StartCoroutine(TakeTurnCoroutine());
    }

    private IEnumerator TakeTurnCoroutine()
    {
        Debug.Log($"=== {_enemyUnit.name} (Ennemi) prend son tour ===");

        // PHASE 0: Rafraîchit les PA au début du tour
        if (_enemy != null)
        {
            _enemy.RefreshPA(); // Rafraîchit les PA au début du tour
        }

        // 1. Trouver le joueur le plus proche
        List<Unit> playerUnits = GridManager.Instance.GetAllPlayerUnits();

        if (playerUnits == null || playerUnits.Count == 0)
        {
            Debug.LogWarning($"{_enemyUnit.name}: Aucune unité joueur trouvée. Passe son tour.");
            GridManager.Instance.OnEndTurnButtonClick();
            yield break;
        }

        Unit closestPlayerUnit = FindClosestPlayer(playerUnits);

        if (closestPlayerUnit == null)
        {
            Debug.LogWarning($"{_enemyUnit.name}: Aucun joueur valide trouvé.");
            GridManager.Instance.OnEndTurnButtonClick();
            yield break;
        }

        Debug.Log($"{_enemyUnit.name} cible {closestPlayerUnit.name}");

        // 2. DÉPLACEMENT (seulement si la prochaine carte nécessite de cibler un ennemi)
        bool needsToMoveCloser = false;

        if (_enemy != null)
        {
            CardData nextCard = _enemy.GetNextCard();

            // Ne bouge que si la carte cible un ennemi
            if (nextCard != null && nextCard.targetType == CardTargetType.Enemy)
            {
                int maxCardRange = nextCard.targetRange;
                float currentDistance = Vector2.Distance(_enemyUnit.GetCurrentGridPos(), closestPlayerUnit.GetCurrentGridPos());

                if (currentDistance > maxCardRange)
                {
                    needsToMoveCloser = true;
                    Debug.Log($"{_enemyUnit.name} doit se rapprocher (distance: {currentDistance}, portée carte: {maxCardRange})");
                }
                else
                {
                    Debug.Log($"{_enemyUnit.name} est déjà à portée de carte ({currentDistance} <= {maxCardRange}), pas de déplacement");
                }
            }
            else if (nextCard != null)
            {
                Debug.Log($"{_enemyUnit.name} a une carte {nextCard.targetType}, pas besoin de se rapprocher");
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
                Debug.Log($"{_enemyUnit.name} se déplace de {movementCost} cases vers {closestPlayerUnit.name}");

                // Déplace l'unité le long du chemin (fluide, comme le joueur)
                _enemyUnit.MoveToTile(fullPath);
                _enemyUnit.SpendMovement(movementCost);

                // Attendre que le mouvement soit terminé
                yield return new WaitUntil(() => !_enemyUnit.IsMoving());
            }
            else
            {
                Debug.LogWarning($"{_enemyUnit.name}: Aucun chemin valide trouvé.");
            }
        }

        // 3. PHASE CARTES: Tente de jouer une carte APRÈS le déplacement (si Enemy avec cartes)
        if (_enemy != null)
        {
            CardData nextCard = _enemy.GetNextCard();
            if (nextCard != null && _enemy.GetCurrentPA() >= nextCard.costPA)
            {
                Debug.Log($"{_enemy.name} veut jouer la carte: {nextCard.cardName}");

                // Vérifie si la carte peut être jouée (cible valide, portée, etc.)
                bool canPlayCard = CanPlayCard(nextCard);

                if (canPlayCard)
                {
                    Debug.Log($"{_enemy.name} PEUT jouer {nextCard.cardName}, pioche la carte");

                    // MAINTENANT on pioche la carte (avance l'index)
                    CardData playedCard = _enemy.DrawAndPlayNextCard();
                    if (playedCard != null)
                    {
                        // Dépense les PA
                        _enemy.SpendPA(playedCard.costPA);

                        // Exécute l'effet de la carte
                        yield return StartCoroutine(ExecuteEnemyCard(playedCard));
                    }
                }
                else
                {
                    Debug.Log($"{_enemy.name}: Ne PEUT PAS jouer {nextCard.cardName} (pas de cible/hors portée), garde la carte pour le prochain tour");
                }
            }
            else if (nextCard != null)
            {
                Debug.Log($"{_enemy.name}: Pas assez de PA pour jouer {nextCard.cardName}");
            }
        }

        // 4. Fin du tour (plus d'attaque de base, tout passe par les cartes)
        GridManager.Instance.OnEndTurnButtonClick();
    }

    // Calcule un chemin complet vers la cible en utilisant tous les points de mouvement disponibles
    private List<Tile> CalculatePathTowardsTarget(Vector2 targetPos)
    {
        List<Tile> path = new List<Tile>();
        Vector2 currentPos = _enemyUnit.GetCurrentGridPos();
        int remainingMovement = _enemyUnit.GetRemainingMovement();

        // Obtient la portée max des cartes de l'ennemi (si c'est un Enemy avec cartes)
        int maxCardRange = GetMaxCardRange();

        // Construire le chemin case par case jusqu'à épuisement des points de mouvement
        // ou jusqu'à être à portée de carte
        for (int i = 0; i < remainingMovement; i++)
        {
            // Si l'ennemi a des cartes, s'arrête à portée de carte
            if (maxCardRange > 0)
            {
                float distanceToTarget = Vector2.Distance(currentPos, targetPos);
                if (distanceToTarget <= maxCardRange)
                {
                    Debug.Log($"À portée de carte ({maxCardRange}) après {i} mouvements");
                    break; // On est assez proche pour jouer une carte
                }
            }

            // Trouver le meilleur prochain mouvement
            Vector2 nextMove = FindBestMoveFrom(currentPos, targetPos);

            if (nextMove == Vector2.zero)
            {
                Debug.Log($"Bloqué après {i} mouvements");
                break; // Bloqué, on ne peut plus avancer
            }

            // Ajouter cette case au chemin
            Tile nextTile = GridManager.Instance.GetTileAtPosition(nextMove);
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

    // Trouve le meilleur mouvement depuis une position donnée vers une cible
    private Vector2 FindBestMoveFrom(Vector2 fromPos, Vector2 targetPos)
    {
        Vector2 direction = targetPos - fromPos;

        // Liste des directions à essayer (par ordre de priorité)
        // SEULEMENT des mouvements orthogonaux (pas de diagonales)
        List<Vector2> directionsToTry = new List<Vector2>();

        // Direction principale (vers le joueur)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Le joueur est plus à gauche/droite qu'en haut/bas
            directionsToTry.Add(new Vector2(Mathf.Sign(direction.x), 0)); // Horizontal d'abord
            directionsToTry.Add(new Vector2(0, Mathf.Sign(direction.y))); // Vertical ensuite
        }
        else
        {
            // Le joueur est plus en haut/bas qu'à gauche/droite
            directionsToTry.Add(new Vector2(0, Mathf.Sign(direction.y))); // Vertical d'abord
            directionsToTry.Add(new Vector2(Mathf.Sign(direction.x), 0)); // Horizontal ensuite
        }

        // Ajouter les autres directions orthogonales comme alternatives
        directionsToTry.Add(new Vector2(-Mathf.Sign(direction.x), 0)); // Horizontal opposé
        directionsToTry.Add(new Vector2(0, -Mathf.Sign(direction.y))); // Vertical opposé

        // Essayer chaque direction
        Vector2 bestMove = Vector2.zero;
        float bestDistanceToTarget = float.MaxValue;

        foreach (Vector2 dir in directionsToTry)
        {
            Vector2 nextPos = fromPos + dir;

            // Vérifier que la case est valide
            Tile nextTile = GridManager.Instance.GetTileAtPosition(nextPos);
            if (nextTile == null) continue; // Case hors grille

            Unit unitOnTile = GridManager.Instance.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null) continue; // Case occupée

            // Calculer la distance au joueur depuis cette case
            float distToTarget = Vector2.Distance(nextPos, targetPos);

            if (distToTarget < bestDistanceToTarget)
            {
                bestDistanceToTarget = distToTarget;
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
            float distance = Vector2.Distance(_enemyUnit.GetCurrentGridPos(), playerUnit.GetCurrentGridPos());
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
        List<Unit> playerUnits = GridManager.Instance.GetAllPlayerUnits();
        if (playerUnits == null || playerUnits.Count == 0)
        {
            Debug.LogWarning($"{_enemy.name}: Aucun joueur pour cibler {card.cardName}");
            return false; // Pas de cible disponible
        }

        Unit closestPlayer = FindClosestPlayer(playerUnits);
        if (closestPlayer == null)
        {
            return false;
        }

        // Vérifie la portée pour les cartes ciblant l'ennemi
        if (card.targetType == CardTargetType.Enemy)
        {
            float distance = Vector2.Distance(_enemy.GetCurrentGridPos(), closestPlayer.GetCurrentGridPos());
            if (distance > card.targetRange)
            {
                Debug.Log($"{_enemy.name}: {card.cardName} hors de portée ({distance} > {card.targetRange})");
                return false; // Hors de portée
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
        Debug.Log($"{_enemy.name} exécute la carte: {card.cardName}");

        // Détermine la cible en fonction du type de carte
        Unit targetUnit = null;
        Vector2 targetTile = Vector2.zero;

        // Trouve le joueur le plus proche pour les cartes offensives
        List<Unit> playerUnits = GridManager.Instance.GetAllPlayerUnits();
        if (playerUnits != null && playerUnits.Count > 0)
        {
            Unit closestPlayer = FindClosestPlayer(playerUnits);

            // Vérifie si la carte cible une unité
            if (card.targetsUnit)
            {
                // Carte offensive contre joueur
                if (card.targetType == CardTargetType.Enemy)
                {
                    // Vérifie la portée
                    float distance = Vector2.Distance(_enemy.GetCurrentGridPos(), closestPlayer.GetCurrentGridPos());
                    if (distance <= card.targetRange)
                    {
                        targetUnit = closestPlayer;
                    }
                    else
                    {
                        Debug.LogWarning($"{_enemy.name}: Cible hors de portée pour {card.cardName}");
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
