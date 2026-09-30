using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Ajouté pour EventSystem
using UnityEngine.InputSystem; // Nouveau Input System

public class InputManager : MonoBehaviour
{
    [SerializeField] private HandUIController _handUIController; // Référence au HandUIController
    private CardData _previousSelectedCard = null; // Pour tracker les changements de sélection
    private Vector2Int _lastHoveredTilePos = new Vector2Int(-1, -1); // Position de la dernière tuile survolée

    private static void ClearDamagePreview() => EventBus.Publish(new DamagePreviewEvent(new List<DamagePreview.Entry>()));

    // Tour terminé (le script est désactivé) : plus de prévision à l'écran
    void OnDisable() => ClearDamagePreview();

    /// <summary>
    /// Tente d'extraire la position de grille d'un GameObject (tuile ou unité)
    /// </summary>
    /// <param name="gameObject">L'objet à analyser</param>
    /// <param name="gridPos">La position de grille si trouvée</param>
    /// <returns>True si une position valide a été trouvée</returns>
    private bool TryGetGridPosition(GameObject gameObject, out Vector2Int gridPos)
    {
        gridPos = Vector2Int.zero;

        // Vérifie d'abord si c'est une tuile par son composant
        if (gameObject.TryGetComponentSafe(out Tile tile))
        {
            gridPos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);
            return true;
        }

        // Fallback: parsing du nom "Tile X Y"
        string[] nameParts = gameObject.name.Split(' ');
        if (nameParts.Length == 3 && nameParts[0] == "Tile")
        {
            if (int.TryParse(nameParts[1], out int gridX) && int.TryParse(nameParts[2], out int gridY))
            {
                gridPos = new Vector2Int(gridX, gridY);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Objet pointé par la souris, choisi par case : le rayon traverse les unités, qui ne masquent
    /// donc jamais la case derrière elles. Renvoie l'unité posée sur la case visée s'il y en a une,
    /// sinon la case ; false si aucune case n'est sous le curseur.
    /// </summary>
    public static bool TryGetPointedObject(out GameObject pointed)
    {
        pointed = null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Tile closestTile = null;
        float closestDistance = float.MaxValue;
        foreach (RaycastHit hit in Physics.RaycastAll(ray, 1000f))
        {
            if (hit.distance < closestDistance && hit.collider.TryGetComponentSafe(out Tile tile))
            {
                closestDistance = hit.distance;
                closestTile = tile;
            }
        }
        if (closestTile == null) return false;

        Unit unitOnTile = Services.Grid.GetUnitAtGridPos(Services.Grid.GetGridPosFromWorldPos(closestTile.transform.position));
        pointed = unitOnTile != null ? unitOnTile.gameObject : closestTile.gameObject;
        return true;
    }

    /// <summary>
    /// Réaffiche la portée de la carte sélectionnée, puis garde en rouge le sol des cibles déjà
    /// choisies d'une carte à cibles multiples (ex: Frappe rapide), pour qu'on les voie en
    /// choisissant les suivantes.
    /// </summary>
    private void ShowCardTargets(CardData card, Unit activeUnit)
    {
        EventBus.Publish(new ShowCardTargetsEvent(card, activeUnit));

        foreach (Unit chosen in _handUIController.PendingMultiTargets)
        {
            if (chosen != null) Services.Grid.HighlightTile(chosen.GetCurrentGridPos(), Color.red);
        }
    }

    private static bool TryGetUnitGridPosition(GameObject gameObject, out Vector2Int gridPos)
    {
        gridPos = Vector2Int.zero;
        if (!gameObject.TryGetComponentSafe(out Unit unit)) return false;

        gridPos = unit.GetCurrentGridPos();
        return true;
    }

    void Update()
    {
        // Phase 3.5: Utilise Services.Grid au lieu de Services.Grid
        if (!Services.IsGridServiceAvailable())
        {
            return;
        }

        Unit activeUnit = Services.Grid.GetActiveUnit();

        if (activeUnit == null)
        {
            return; // Pas d'unité active, ne fait rien
        }

        // Vérifie si la carte sélectionnée a changé pour afficher les cibles
        CardData currentSelectedCard = _handUIController?.SelectedCard;
        if (currentSelectedCard != _previousSelectedCard)
        {
            if (currentSelectedCard != null && (currentSelectedCard.targetsUnit || currentSelectedCard.targetsTile))
            {
                // Affiche les cibles valides pour la nouvelle carte sélectionnée (OPTIMISATION Phase 3.2: EventBus)
                ShowCardTargets(currentSelectedCard, activeUnit);
            }
            else if (_previousSelectedCard != null)
            {
                // Si on vient de désélectionner une carte, réafficher la portée de mouvement
                GameLog.Log("InputManager: Carte désélectionnée, réaffichage de la portée de mouvement");
                // OPTIMISATION Phase 3.2: EventBus
                EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
            }
            _previousSelectedCard = currentSelectedCard;
            _lastHoveredTilePos = new Vector2Int(-1, -1); // Reset hover
            ClearDamagePreview();
        }

        // Preview hover d'une carte de déplacement d'invocation (ciblage en 2 étapes)
        if (currentSelectedCard != null && currentSelectedCard.isRepositionSummonCard)
        {
            HandleRepositionSummonHover(currentSelectedCard, activeUnit);
        }
        // Preview hover pour les cartes avec cibles (AOE ou non)
        else if (currentSelectedCard != null && (currentSelectedCard.targetsUnit || currentSelectedCard.targetsTile))
        {
            if (TryGetPointedObject(out GameObject hoveredObject))
            {
                Vector2Int hoveredPos = Vector2Int.zero;
                bool isValidHoverTarget = false;

                // Vérifie si on survole une unité (OPTIMISATION Phase 3.3: ComponentLocator)
                // Note: Les cartes de charge peuvent aussi cibler des ennemis même si targetsTile est true
                bool canTargetUnit = currentSelectedCard.targetsUnit ||
                    (currentSelectedCard.isChargeCard && hoveredObject.TryGetComponentSafe(out Unit _));

                if (hoveredObject.TryGetComponentSafe(out Unit hoveredUnit) && canTargetUnit)
                {
                    Vector2Int sourcePos = activeUnit.GetCurrentGridPos();
                    Vector2Int targetPos = hoveredUnit.GetCurrentGridPos();

                    // Pour les cartes de charge, vérifie la ligne droite (4 directions) et la portée
                    if (currentSelectedCard.isChargeCard)
                    {
                        Tile unitTile = Services.Grid.GetTileAtPosition(targetPos);
                        int lineDist = GridGeometry.Distance(sourcePos, targetPos);

                        // Valide si en ligne droite, dans la portée, et une cible valide (ennemi ; allié aussi si la carte cible une unité)
                        if (currentSelectedCard.IsValidChargeTarget(unitTile, activeUnit) &&
                            lineDist <= currentSelectedCard.targetRange)
                        {
                            hoveredPos = targetPos;
                            isValidHoverTarget = true;
                        }
                    }
                    else
                    {
                        // Comportement normal pour les autres cartes
                        List<Tile> tilesInRange = Services.Grid.GetAttackTiles(sourcePos, currentSelectedCard.targetRange, activeUnit);
                        Tile targetTile = Services.Grid.GetTileAtPosition(targetPos);

                        bool inLine = !currentSelectedCard.targetInStraightLine || GridGeometry.TryGetLine(sourcePos, targetPos, out _, out _);

                        // On survole une unité, elle est dans la portée ET c'est une cible valide
                        // (soi-même : sa case n'est pas dans la portée mais reste ciblable, voir IsValidTarget)
                        bool inRange = tilesInRange.Contains(targetTile) || hoveredUnit == activeUnit;
                        if (inRange && inLine && currentSelectedCard.IsValidTarget(activeUnit, hoveredUnit))
                        {
                            hoveredPos = hoveredUnit.GetCurrentGridPos();
                            isValidHoverTarget = true;
                        }
                    }
                }
                // Une unité survolée par une carte qui cible une case compte comme sa case (ex: EnemyOrTile)
                else if (TryGetGridPosition(hoveredObject, out hoveredPos) || TryGetUnitGridPosition(hoveredObject, out hoveredPos))
                {
                    // Vérifie si on survole une tuile valide
                    Tile hoveredTile = Services.Grid.GetTileAtPosition(hoveredPos);

                    // Calcule la distance depuis l'unité active
                    Vector2Int sourcePos = activeUnit.GetCurrentGridPos();
                    List<Tile> tilesInRange = Services.Grid.GetAttackTiles(sourcePos, currentSelectedCard.targetRange, activeUnit);

                    // Vérifie si la tuile est dans la portée ET que c'est une cible valide
                    bool isValidTarget = currentSelectedCard.isChargeCard
                        ? currentSelectedCard.IsValidChargeTarget(hoveredTile, activeUnit)
                        : currentSelectedCard.IsValidTileTarget(hoveredTile);
                    bool inLine = !currentSelectedCard.targetInStraightLine || GridGeometry.TryGetLine(sourcePos, hoveredPos, out _, out _);

                    // Carte d'invocation alors que l'invocation est déjà là : seule sa case est valide (soin)
                    bool isValidTile = GameActionValidator.HealsActiveSummon(currentSelectedCard, activeUnit)
                        ? GameActionValidator.CanTargetTile(currentSelectedCard, activeUnit, hoveredPos).IsValid
                        : currentSelectedCard.targetsTile && tilesInRange.Contains(hoveredTile) && inLine && isValidTarget;

                    if (isValidTile)
                    {
                        isValidHoverTarget = true;
                    }
                }

                // Si on survole une cible valide et que c'est une nouvelle position
                if (isValidHoverTarget && hoveredPos != _lastHoveredTilePos)
                {
                    _lastHoveredTilePos = hoveredPos;

                    // Réaffiche les cibles de base (OPTIMISATION Phase 3.2: EventBus)
                    ShowCardTargets(currentSelectedCard, activeUnit);
                    EventBus.Publish(new DamagePreviewEvent(DamagePreview.Compute(currentSelectedCard, activeUnit, hoveredPos)));

                    // Si la carte est AOE, affiche la zone AOE
                    if (currentSelectedCard.isAOE && currentSelectedCard.aoeRadius > 0)
                    {
                        // OPTIMISATION Phase 3.2: EventBus
                        EventBus.Publish(new ShowAOEZoneEvent(hoveredPos, currentSelectedCard.aoeRadius, currentSelectedCard, activeUnit));
                    }
                    else
                    {
                        // Sinon, surligne juste la tuile en rouge
                        Services.Grid.HighlightTile(hoveredPos, Color.red);
                    }
                }
                else if (!isValidHoverTarget && _lastHoveredTilePos != new Vector2Int(-1, -1))
                {
                    // Si on ne survole plus de cible valide, réaffiche juste les cibles de base (OPTIMISATION Phase 3.2: EventBus)
                    _lastHoveredTilePos = new Vector2Int(-1, -1);
                    ShowCardTargets(currentSelectedCard, activeUnit);
                    ClearDamagePreview();
                }
            }
            else if (_lastHoveredTilePos != new Vector2Int(-1, -1))
            {
                // Si le raycast ne touche rien, réinitialise (OPTIMISATION Phase 3.2: EventBus)
                _lastHoveredTilePos = new Vector2Int(-1, -1);
                ShowCardTargets(currentSelectedCard, activeUnit);
                ClearDamagePreview();
            }
        }
        else if (currentSelectedCard == null)
        {
            HandleMovementHover(activeUnit);
        }

        // --- GESTION DU CLIC DROIT (Désélection prioritaire) ---
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (_handUIController != null && _handUIController.SelectedCard != null)
            {
                GameLog.Log("Clic droit détecté : annulation d'une étape de ciblage.");
                _handUIController.CancelTargetingStep();

                // Carte à cibles multiples encore en main : la cible retirée perd son sol rouge
                CardData stillSelected = _handUIController.SelectedCard;
                if (stillSelected != null && stillSelected.isMultiTarget)
                {
                    _lastHoveredTilePos = new Vector2Int(-1, -1);
                    ShowCardTargets(stillSelected, activeUnit);
                }
                return; // Consomme le clic droit et bloque toute autre interaction
            }
        }

        // --- GESTION DU CLIC GAUCHE ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                var hoveredUI = EventSystem.current.currentSelectedGameObject;
                // Autorise le clic sur le monde si ce n’est pas la main UI
                if (_handUIController != null && _handUIController.SelectedCard != null)
                {
                    // Carte qui cible une carte de la main (ex: Triche) : un clic sur l'interface
                    // (une carte de la main, le choix du coût) ne doit pas la désélectionner
                    if (_handUIController.SelectedCard.targetsHandCard) return;

                    if (hoveredUI == null || hoveredUI.GetComponentInParent<HandUIController>() == null)
                    {
                        GameLog.Log("Clic gauche vers le monde, carte sélectionnée : tentative de jeu");
                        // on continue vers HandleCardPlay
                    }
                    else
                    {
                        GameLog.Log("Clic gauche sur la main UI consommé.");
                        return;
                    }
                }
            }

            // Si le pointeur n'est PAS sur l'UI, alors c'est une interaction avec le monde
            if (TryGetPointedObject(out GameObject clickedObject))
            {

                // Si une carte est sélectionnée, tenter de la jouer sur l'objet monde
                if (_handUIController != null && _handUIController.SelectedCard != null)
                {
                    GameLog.Log($"Clic gauche sur monde avec carte sélectionnée : {clickedObject.name}");
                    HandleCardPlay(clickedObject, activeUnit);
                }
                else
                {
                    // Sinon (pas de carte sélectionnée), gérer l'interaction normale avec l'unité
                    GameLog.Log($"Clic gauche sur monde sans carte sélectionnée : {clickedObject.name}");
                    HandleUnitInteraction(clickedObject, activeUnit);
                }
            }
            else
            {
                // Clic sur le vide (pas d'UI, pas de monde)
                if (_handUIController != null && _handUIController.SelectedCard != null)
                {
                    GameLog.Log("Clic sur le vide avec carte sélectionnée. Désélection de la carte.");
                    _handUIController.DeselectCard();
                }
            }
            return; // Consomme le clic gauche après avoir traité l'interaction monde/vide
        }
    }

    /// <summary>
    /// Survol d'une carte de déplacement d'invocation : surligne l'invocation survolée (étape 1)
    /// ou la case d'arrivée survolée (étape 2) si elle est valide.
    /// </summary>
    private void HandleRepositionSummonHover(CardData card, Unit activeUnit)
    {
        SummonUnit chosen = _handUIController.SummonToMove;
        Unit targetingSource = chosen != null ? (Unit)chosen : activeUnit;
        bool isValid = false;
        Vector2Int hoveredPos = new Vector2Int(-1, -1);

        if (TryGetPointedObject(out GameObject hovered))
        {
            hovered.TryGetComponentSafe(out Unit hoveredUnit);

            if (hoveredUnit != null)
                hoveredPos = hoveredUnit.GetCurrentGridPos();
            else if (TryGetGridPosition(hovered, out Vector2Int tilePos))
            {
                hoveredPos = tilePos;
                hoveredUnit = Services.Grid.GetUnitAtGridPos(tilePos);
            }

            if (chosen == null)
            {
                isValid = hoveredUnit != null && GameActionValidator.CanSelectSummonToMove(card, activeUnit, hoveredUnit).IsValid;
            }
            else if (hoveredPos.x >= 0)
            {
                bool isFree = Services.Grid.GetTileAtPosition(hoveredPos) != null && hoveredUnit == null;
                isValid = GameActionValidator.CanMoveSummonTo(card, chosen, hoveredPos, isFree).IsValid;
            }
        }

        if (isValid && hoveredPos != _lastHoveredTilePos)
        {
            _lastHoveredTilePos = hoveredPos;
            EventBus.Publish(new ShowCardTargetsEvent(card, targetingSource));
            Services.Grid.HighlightTile(hoveredPos, Color.red);
        }
        else if (!isValid && _lastHoveredTilePos != new Vector2Int(-1, -1))
        {
            _lastHoveredTilePos = new Vector2Int(-1, -1);
            EventBus.Publish(new ShowCardTargetsEvent(card, targetingSource));
        }
    }

    private void HandleMovementHover(Unit activeUnit)
    {
        if (TryGetPointedObject(out GameObject hoveredObject))
        {
            Vector2Int hoveredPos = Vector2Int.zero;
            bool isValidHover = false;

            // Vérifie si c'est une tuile ou une unité
            if (TryGetGridPosition(hoveredObject, out hoveredPos))
            {
                isValidHover = true;
            }
            else if (hoveredObject.TryGetComponentSafe(out Unit hoveredUnit))
            {
                hoveredPos = hoveredUnit.GetCurrentGridPos();
                isValidHover = true;
            }

            if (isValidHover)
            {
                if (hoveredPos != _lastHoveredTilePos)
                {
                    _lastHoveredTilePos = hoveredPos;

                    // Réaffiche la portée de mouvement de base
                    EventBus.Publish(new ShowMovementRangeEvent(activeUnit));

                    // Vérifie si la tuile est accessible
                    int availablePoints = activeUnit.GetCurrentMovementPoints();
                    Dictionary<Tile, int> reachableTiles = Services.Grid.GetMovementTiles(
                        activeUnit.GetCurrentGridPos(),
                        availablePoints,
                        activeUnit
                    );

                    Tile hoveredTile = Services.Grid.GetTileAtPosition(hoveredPos);
                    if (hoveredTile != null && reachableTiles.ContainsKey(hoveredTile))
                    {
                        // Surligne la destination en Cyan pour indiquer le mouvement
                        Services.Grid.HighlightTile(hoveredPos, Color.cyan);
                    }
                }
            }
            else if (_lastHoveredTilePos != new Vector2Int(-1, -1))
            {
                _lastHoveredTilePos = new Vector2Int(-1, -1);
                EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
            }
        }
        else if (_lastHoveredTilePos != new Vector2Int(-1, -1))
        {
            _lastHoveredTilePos = new Vector2Int(-1, -1);
            EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
        }
    }

    private void HandleCardPlay(GameObject clickedObject, Unit activeUnit)
    {
        CardData selectedCard = _handUIController.SelectedCard;
        if (selectedCard == null) return; // Par sécurité

        // OPTIMISATION Phase 3.3: ComponentLocator
        clickedObject.TryGetComponentSafe(out Unit targetUnit);

        Vector2Int targetTilePos = Vector2Int.zero;
        Tile targetTile = null;

        // Détermine la position cible
        if (targetUnit != null)
        {
            // Si on clique sur une unité, utilise sa position (et sa case, pour les cartes qui ciblent une case)
            targetTilePos = targetUnit.GetCurrentGridPos();
            targetTile = Services.Grid.GetTileAtPosition(targetTilePos);
        }
        else if (TryGetGridPosition(clickedObject, out targetTilePos))
        {
            // Si on clique sur une tuile
            targetTile = Services.Grid.GetTileAtPosition(targetTilePos);

            // On regarde s'il y a une unité dessus (permet de cibler l'unité en cliquant sur le bord de sa case)
            if (targetTile != null)
            {
                targetUnit = Services.Grid.GetUnitAtGridPos(targetTilePos);
            }
        }

        // Cas spécial : carte de déplacement d'invocation (ex: Écho évanescent), en 2 étapes :
        // 1er clic = une invocation du lanceur, 2e clic = sa case d'arrivée. Un clic invalide est
        // ignoré sans désélectionner la carte (clic droit pour revenir en arrière).
        if (selectedCard.isRepositionSummonCard)
        {
            if (_handUIController.SummonToMove == null)
                _handUIController.SelectSummonToMove(targetUnit);
            else if (targetUnit == _handUIController.SummonToMove)
                _handUIController.CancelTargetingStep(); // recliquer l'invocation = en choisir une autre
            else if (targetTile != null || targetUnit != null)
                _handUIController.PlayRepositionSummonCard(targetTilePos);
            return;
        }

        // Cas spécial : carte à cibles multiples (ex: Frappe rapide) - accumule les cibles une par
        // une au lieu d'exécuter au premier clic ; HandUIController valide chaque cible et déclenche
        // l'exécution automatiquement une fois le nombre requis atteint (ou plus de cible valide).
        if (selectedCard.isMultiTarget)
        {
            if (targetUnit != null)
            {
                _handUIController.AddMultiTarget(targetUnit);

                // Toujours en cours de ciblage : le sol de la cible choisie reste rouge
                if (_handUIController.SelectedCard == selectedCard)
                    ShowCardTargets(selectedCard, activeUnit);
            }
            // Clic invalide (pas d'unité cliquée) : ignoré sans désélectionner, le joueur peut recliquer.
            return;
        }

        // BUGFIX: carte ciblant une carte de la main (ex: Triche, targetsHandCard = true,
        // targetsUnit = targetsTile = false). Le ciblage se fait EXCLUSIVEMENT en cliquant une
        // autre carte de la main (HandUIController.HandleCardClicked / PlayHandCardTargetingCard).
        // Sans ce garde-fou, un clic sur le monde tombait dans le cas "carte sans cible" ci-dessous
        // et jouait la carte immédiatement sans cible choisie (effet perdu, PA dépensés pour rien).
        if (selectedCard.targetsHandCard)
        {
            GameLog.Log("Clic sur le monde ignoré : cette carte doit cibler une carte de la main.");
            return;
        }

        // Cas spécial : Carte Self (doit cliquer sur le joueur lui-même)
        if (selectedCard.targetType == CardTargetType.Self)
        {
            // Vérifie qu'on a cliqué sur l'unité active (le joueur)
            if (targetUnit != null && targetUnit == activeUnit)
            {
                _handUIController.PlaySelectedCard(activeUnit, default);
                return;
            }
            else
            {
                // Clic invalide, désélectionne la carte
                _handUIController.DeselectCard();
                return;
            }
        }
        // Cas spécial : Carte sans cible spécifique (doit quand même cliquer quelque part de valide)
        else if (!selectedCard.targetsUnit && !selectedCard.targetsTile)
        {
            // Pour les cartes sans cible, on peut cliquer n'importe où
            _handUIController.PlaySelectedCard(null, default);
            return;
        }
        // Cas spécial : carte d'invocation alors que l'invocation est déjà là (ex: Invocation de
        // Lyse) : cliquer l'invocation la soigne, tout autre clic annule
        if (GameActionValidator.HealsActiveSummon(selectedCard, activeUnit))
        {
            if (GameActionValidator.CanTargetTile(selectedCard, activeUnit, targetTilePos).IsValid)
                _handUIController.PlaySelectedCard(null, targetTilePos);
            else
                _handUIController.DeselectCard();
            return;
        }

        // Cas spécial : carte de charge ciblant une unité (ennemi, ou allié si la carte cible une unité) — avant la vérification de portée classique
        if (selectedCard.isChargeCard && targetUnit != null && targetUnit != activeUnit)
        {
            Vector2Int sourcePos = activeUnit.GetCurrentGridPos();
            Vector2Int enemyPos = targetUnit.GetCurrentGridPos();

            // Vérifie que l'ennemi est en ligne droite
            Tile enemyTile = Services.Grid.GetTileAtPosition(enemyPos);
            if (!selectedCard.IsValidChargeTarget(enemyTile, activeUnit))
            {
                GameLog.Log($"Charge invalide : {targetUnit.name} n'est pas en ligne droite");
                _handUIController.DeselectCard();
                return;
            }

            // Vérifie la portée (4 directions, voir GridGeometry)
            int lineDistance = GridGeometry.Distance(sourcePos, enemyPos);
            if (lineDistance > selectedCard.targetRange)
            {
                GameLog.Log($"Charge invalide : {targetUnit.name} est hors de portée (distance: {lineDistance}, portée: {selectedCard.targetRange})");
                _handUIController.DeselectCard();
                return;
            }

            GameLog.Log($"Charge valide sur ennemi : {targetUnit.name} à distance {lineDistance}");
            // Joue la carte avec la position de l'ennemi comme cible
            _handUIController.PlaySelectedCard(targetUnit, enemyPos);
            return;
        }

        // Vérifie la portée de la carte pour les autres types
        else if (selectedCard.targetsUnit || selectedCard.targetsTile)
        {
            Vector2Int sourcePos = activeUnit.GetCurrentGridPos();

            // Portée en 4 directions (voir GridGeometry), charge comprise
            if (GridGeometry.Distance(sourcePos, targetTilePos) > selectedCard.targetRange)
            {
                // Hors de portée, désélectionne la carte
                _handUIController.DeselectCard();
                return;
            }
        }

        // Logique pour jouer la carte avec validation stricte

        if (selectedCard.targetsUnit && targetUnit != null)
        {
            // Vérifie que l'unité est une cible valide selon le type de carte
            if (selectedCard.IsValidTarget(activeUnit, targetUnit))
            {
                _handUIController.PlaySelectedCard(targetUnit, targetTilePos);
                return;
            }
        }
        
        // FIX: Fallback sur le ciblage de tuile si le ciblage d'unité n'a pas fonctionné (ou n'était pas applicable)
        if (selectedCard.targetsTile && targetTile != null)
        {
            // Vérifie que la tuile est une cible valide selon le type de carte
            // Pour les cartes de charge, utilise la validation spécifique (ligne droite)
            bool isValidTarget = selectedCard.isChargeCard
                ? selectedCard.IsValidChargeTarget(targetTile, activeUnit)
                : selectedCard.IsValidTileTarget(targetTile);

            if (isValidTarget)
            {
                _handUIController.PlaySelectedCard(null, targetTilePos);
                return;
            }
        }
        else if (!selectedCard.targetsUnit && !selectedCard.targetsTile)
        {
            // Carte sans cible, joue immédiatement
            _handUIController.PlaySelectedCard(null, default);
            return;
        }

        // Si on arrive ici, c'est que la cible était invalide ou hors de portée
        // (car les cas valides font un 'return' après avoir lancé la coroutine)
        _handUIController.DeselectCard();
    }

    private void HandleUnitInteraction(GameObject clickedObject, Unit activeUnit)
    {
        // Récupère les points de mouvement disponibles (PM pour toutes les unités)
        int availablePoints = activeUnit.GetCurrentMovementPoints();

        // Vérifie si on clique sur une unité (OPTIMISATION Phase 3.3: ComponentLocator)
        clickedObject.TryGetComponentSafe(out Unit clickedUnit);

        // Les attaques se font uniquement via les cartes, plus d'attaque de base
        if (clickedUnit == activeUnit)
        {
            GameLog.Log("Clic sur l'unité active.");
        }
        else if (TryGetGridPosition(clickedObject, out Vector2Int targetGridPos))
        {
            if (availablePoints <= 0)
            {
                GameLog.LogWarning($"{activeUnit.name} n'a plus de PM ! (PM: {availablePoints})");
                return;
            }

            // Déplacement : action du joueur, validée et exécutée par CombatCommandExecutor
            ICombatCommandService commands = Services.Commands;
            if (commands != null && commands.ActiveActor >= 0)
                commands.Submit(CombatCommand.Move(commands.ActiveActor, targetGridPos));
        }
    }
}