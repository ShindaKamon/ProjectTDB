using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exécute les actions des joueurs (CombatCommand) une par une, dans l'ordre reçu : chacune
/// attend la fin de la précédente (déplacements et charges animés), pour que la grille soit
/// dans le même état sur tous les PC au moment de l'exécuter. Toute règle est revalidée ici
/// (GameActionValidator) : l'interface ne fait que produire les commandes.
/// Ajouté et initialisé par GridManager ; accessible via Services.Commands.
/// </summary>
public class CombatCommandExecutor : MonoBehaviour, ICombatCommandService
{
    private readonly Queue<CombatCommand> _queue = new Queue<CombatCommand>();
    private bool _running;
    private GridManager _grid;
    private PlacementPhase _placement;

    // Numéro du tour en cours (0 = placement), compté pareil sur tous les PC : une commande ne vaut
    // que pour le tour où elle a été décidée
    private int _turn;

    public void Init(GridManager grid, PlacementPhase placement)
    {
        _grid = grid;
        _placement = placement;
        ServiceLocator.Instance.Register<ICombatCommandService>(this);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<ICombatCommandService>();
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    }

    private void OnTurnChanged(TurnChangedEvent e)
    {
        _turn++;

        // Réseau : empreinte de l'état comparée à celle de l'hôte (voir DesyncDetector)
        if (!NetworkSession.IsActive) return;
        string state = CombatStateFingerprint.Describe(_grid.GetAllUnits());
        GameLog.Log($"[État réseau] tour {_turn} : {state}");
        NetworkSession.Instance.ReportTurnState(_turn, state);
    }

    public int ActiveActor
    {
        get
        {
            if (_placement != null && _placement.IsActive) return _placement.CurrentIndex;
            Unit active = _grid != null ? _grid.GetActiveUnit() : null;
            return active is Champion champion ? CombatParty.IndexOf(champion.championData) : -1;
        }
    }

    public bool IsLocalTurn => CombatParty.IsLocal(ActiveActor);

    public void Submit(CombatCommand command)
    {
        if (command == null || !CombatParty.IsLocal(command.Actor)) return;
        command.Turn = _turn;

        // Réseau : l'hôte vérifie l'action et la renvoie à tous (y compris ce PC)
        if (NetworkSession.IsActive) NetworkSession.Instance.SubmitCommand(command);
        else Enqueue(command);
    }

    public void Enqueue(CombatCommand command)
    {
        if (command == null) return;
        _queue.Enqueue(command);
        if (!_running) StartCoroutine(ProcessQueue());
    }

    // Réseau : un PC encore occupé par le tour des monstres attend le tour où l'action a été décidée
    private const float WaitForTurnSeconds = 60f;

    private IEnumerator ProcessQueue()
    {
        _running = true;
        while (_queue.Count > 0)
        {
            CombatCommand command = _queue.Dequeue();

            // La commande précédente doit être terminée (déplacement, charge, recul…), et ce PC doit
            // avoir atteint le tour de la commande (en réseau, il peut avoir un peu de retard)
            float deadline = Time.time + WaitForTurnSeconds;
            yield return new WaitUntil(() => (NoUnitMoving() && !PendingEffects.Any && _turn >= command.Turn) || Time.time > deadline);

            // Tour déjà terminé (ex: double clic sur Fin de tour) ou pas celui de ce joueur : ignorée
            if (command.Turn != _turn || command.Actor != ActiveActor)
            {
                GameLog.LogWarning($"Commande ignorée (tour {command.Turn}, tour en cours {_turn}) : {command}");
                continue;
            }
            GameLog.Log($"▶ Commande : {command}");
            yield return Execute(command);
        }
        _running = false;
    }

    private bool NoUnitMoving()
    {
        foreach (Unit unit in _grid.GetAllUnits())
        {
            if (unit != null && unit.IsMoving()) return false;
        }
        return true;
    }

    private IEnumerator Execute(CombatCommand command)
    {
        switch (command.Type)
        {
            case CombatCommandType.Move: ExecuteMove(command); break;
            case CombatCommandType.PlayCard: yield return ExecutePlayCard(command); break;
            case CombatCommandType.DiscardForTurnEnd: ExecuteDiscard(command); break;
            case CombatCommandType.EndTurn: _grid.EndActiveTurn(); break;
            case CombatCommandType.PlacementMove: _placement.TryPlaceCurrent(command.Tiles[0]); break;
            case CombatCommandType.PlacementNext: _placement.Advance(); break;
        }
    }

    // ========== DÉPLACEMENT ==========

    private void ExecuteMove(CombatCommand command)
    {
        Unit unit = _grid.GetActiveUnit();
        Vector2Int target = command.Tiles[0];
        int points = unit.GetCurrentMovementPoints();

        Tile targetTile = _grid.GetTileAtPosition(target);
        if (points <= 0 || targetTile == null || !_grid.GetMovementTiles(unit.GetCurrentGridPos(), points, unit).ContainsKey(targetTile))
        {
            GameLog.LogWarning($"{unit.name} ne peut pas aller en {target}.");
            return;
        }

        List<Tile> path = _grid.GetPathToTile(unit.GetCurrentGridPos(), target, points, unit);
        if (path == null || path.Count == 0 || path.Count > points)
        {
            GameLog.LogWarning($"Aucun chemin valide vers {target}");
            return;
        }

        EventBus.Publish(new ResetTileColorsEvent());
        unit.MoveToTile(path);
        unit.SpendMovement(path.Count);
        StartCoroutine(RefreshRangeAfterMovement(unit));
    }

    private IEnumerator RefreshRangeAfterMovement(Unit unit)
    {
        yield return new WaitUntil(() => unit == null || !unit.IsMoving());
        if (unit == null || unit != _grid.GetActiveUnit()) yield break;
        if (unit.GetCurrentMovementPoints() > 0) EventBus.Publish(new ShowMovementRangeEvent(unit));
        else EventBus.Publish(new ResetTileColorsEvent());
    }

    // ========== CARTES ==========

    private IEnumerator ExecutePlayCard(CombatCommand command)
    {
        Unit actor = _grid.GetActiveUnit();
        if (!actor.TryGetComponentSafe(out DeckManager deck)) yield break;

        CardData card = deck.GetHand().Find(c => c.cardName == command.CardName);
        if (card == null)
        {
            GameLog.LogWarning($"{command.CardName} n'est pas dans la main de {actor.name}.");
            yield break;
        }

        ValidationResult canPlay = GameActionValidator.CanPlayCard(actor, card);
        if (!canPlay.IsValid)
        {
            GameLog.LogWarning($"❌ Impossible de jouer {card.cardName} : {canPlay.ErrorMessage}");
            yield break;
        }

        if (card.targetsHandCard) PlayHandCardTargetingCard(actor, deck, card, command);
        else if (card.isRepositionSummonCard) PlayRepositionSummonCard(actor, deck, card, command);
        else if (card.isMultiTarget) yield return PlayMultiTargetCard(actor, deck, card, command);
        else yield return PlaySingleTargetCard(actor, deck, card, command);

        EventBus.Publish(new ResetTileColorsEvent());
        EventBus.Publish(new ShowMovementRangeEvent(actor));
    }

    // Unité ciblée par la case d'une commande, pour les cartes qui visent une unité (ou une charge)
    private Unit UnitAt(CardData card, Vector2Int tile) =>
        card.targetsUnit || card.isChargeCard ? _grid.GetUnitAtGridPos(tile) : null;

    private IEnumerator PlaySingleTargetCard(Unit actor, DeckManager deck, CardData card, CombatCommand command)
    {
        Vector2Int tile = command.Tiles.Count > 0 ? command.Tiles[0] : default;
        Unit target = command.Tiles.Count > 0 ? UnitAt(card, tile) : null;

        if (!IsValidTarget(actor, card, target, tile)) yield break;

        // Le lanceur se tourne vers sa cible (ou la case visée) avant de jouer
        if (target != null && target != actor)
            yield return actor.LookAtCoroutine(target.transform.position);
        else if (card.targetsTile && _grid.GetTileAtPosition(tile) != null)
            yield return actor.LookAtCoroutine(_grid.GetTileAtPosition(tile).transform.position + new Vector3(0, 0.5f, 0));

        if (card.isChargeCard) card.ExecuteChargeEffect(actor, target != null ? target.GetCurrentGridPos() : tile);
        else if (card.leapToTarget) card.ExecuteLeapEffect(actor, tile);
        else card.ExecuteEffect(actor, target, tile);

        PayCosts(actor, deck, card);
    }

    private bool IsValidTarget(Unit actor, CardData card, Unit target, Vector2Int tile)
    {
        ValidationResult result = ValidationResult.Success();
        if (card.isChargeCard)
            result = GameActionValidator.CanTargetTile(card, actor, target != null ? target.GetCurrentGridPos() : tile);
        else
        {
            if (card.targetsUnit) result = GameActionValidator.CanTargetUnit(card, actor, target);
            if (result.IsValid && card.targetsTile) result = GameActionValidator.CanTargetTile(card, actor, tile);
        }
        if (!result.IsValid) GameLog.LogWarning($"❌ Ciblage invalide pour {card.cardName} : {result.ErrorMessage}");
        return result.IsValid;
    }

    private IEnumerator PlayMultiTargetCard(Unit actor, DeckManager deck, CardData card, CombatCommand command)
    {
        var targets = new List<Unit>();
        foreach (Vector2Int tile in command.Tiles)
        {
            Unit target = _grid.GetUnitAtGridPos(tile);
            if (target == null || targets.Contains(target) || !GameActionValidator.CanTargetUnit(card, actor, target).IsValid)
            {
                GameLog.LogWarning($"❌ Cible invalide en {tile} pour {card.cardName}");
                yield break;
            }
            targets.Add(target);
        }
        if (targets.Count == 0) yield break;

        yield return actor.LookAtCoroutine(targets[0].transform.position);

        // Seule la 1re cible déclenche les effets « une fois par carte » (voir CardData.ExecuteEffect)
        for (int i = 0; i < targets.Count; i++)
            card.ExecuteEffect(actor, targets[i], default, i > 0);

        PayCosts(actor, deck, card);
    }

    private void PlayRepositionSummonCard(Unit actor, DeckManager deck, CardData card, CombatCommand command)
    {
        if (command.Tiles.Count < 2) return;
        Unit summon = _grid.GetUnitAtGridPos(command.Tiles[0]);
        Vector2Int destination = command.Tiles[1];

        ValidationResult select = GameActionValidator.CanSelectSummonToMove(card, actor, summon);
        bool isFree = _grid.GetTileAtPosition(destination) != null && _grid.GetUnitAtGridPos(destination) == null;
        ValidationResult move = select.IsValid ? GameActionValidator.CanMoveSummonTo(card, (SummonUnit)summon, destination, isFree) : select;
        if (!move.IsValid)
        {
            GameLog.LogWarning($"❌ {card.cardName} : {move.ErrorMessage}");
            return;
        }

        card.ExecuteEffect(actor, summon, destination);
        PayCosts(actor, deck, card);
    }

    // Carte qui modifie le coût d'une autre carte de la main (ex: Triche)
    private void PlayHandCardTargetingCard(Unit actor, DeckManager deck, CardData card, CombatCommand command)
    {
        CardData target = deck.GetHand().Find(c => c.cardName == command.TargetCardName && c != card);
        if (target == null || (command.Delta != 1 && command.Delta != -1))
        {
            GameLog.LogWarning($"❌ {card.cardName} : cible {command.TargetCardName} absente de la main ou choix invalide");
            return;
        }

        // Carte jouée comme les autres pour la Main gagnante de Raze (Suite, Paire, PA dépensés)
        IComboTracker combo = actor as IComboTracker;
        combo?.OnCardAboutToExecute(card);
        deck.ModifyCardCost(target, command.Delta);
        combo?.OnCardResolved(card); // avant la défausse : son coût réel est encore connu

        PayCosts(actor, deck, card);
    }

    // Défausse de la carte jouée, paiement des PA (coût effectif, Triche comprise) et des PV
    private static void PayCosts(Unit actor, DeckManager deck, CardData card)
    {
        int costPA = deck.GetEffectiveCost(card);
        deck.PlayCard(card);
        if (costPA > 0 && actor is IActionPointsUser paUser) paUser.SpendPA(costPA);
        if (card.costHP > 0) actor.PayHealth(card.costHP);
    }

    // ========== FIN DE TOUR ==========

    private void ExecuteDiscard(CombatCommand command)
    {
        Unit actor = _grid.GetActiveUnit();
        if (!actor.TryGetComponentSafe(out DeckManager deck) || deck.ExcessCards == 0) return;

        CardData card = deck.GetHand().Find(c => c.cardName == command.CardName);
        if (card == null) return;

        deck.DiscardFromHand(card);
        if (deck.ExcessCards == 0) _grid.EndActiveTurn();
        else EventBus.Publish(new HandDiscardRequiredEvent(actor, deck.ExcessCards));
    }
}
