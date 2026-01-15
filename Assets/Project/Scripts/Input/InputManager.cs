using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Ajouté pour EventSystem
using UnityEngine.InputSystem; // Nouveau Input System

public class InputManager : MonoBehaviour
{
    [SerializeField] private HandUIController _handUIController; // Référence au HandUIController
    private CardData _previousSelectedCard = null; // Pour tracker les changements de sélection
    private Vector2 _lastHoveredTilePos = new Vector2(-1, -1); // Position de la dernière tuile survolée

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
                EventBus.Publish(new ShowCardTargetsEvent(currentSelectedCard, activeUnit));
            }
            else if (_previousSelectedCard != null)
            {
                // Si on vient de désélectionner une carte, réafficher la portée de mouvement
                Debug.Log("InputManager: Carte désélectionnée, réaffichage de la portée de mouvement");
                // OPTIMISATION Phase 3.2: EventBus
                EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
            }
            _previousSelectedCard = currentSelectedCard;
            _lastHoveredTilePos = new Vector2(-1, -1); // Reset hover
        }

        // Preview hover pour les cartes avec cibles (AOE ou non)
        if (currentSelectedCard != null && (currentSelectedCard.targetsUnit || currentSelectedCard.targetsTile))
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                GameObject hoveredObject = hit.collider.gameObject;
                Vector2 hoveredPos = Vector2.zero;
                bool isValidHoverTarget = false;

                // Vérifie si on survole une unité (OPTIMISATION Phase 3.3: ComponentLocator)
                if (hoveredObject.TryGetComponentSafe(out Unit hoveredUnit) && currentSelectedCard.targetsUnit)
                {
                    // Vérifie d'abord que l'unité est dans la portée
                    Vector2 sourcePos = activeUnit.GetCurrentGridPos();
                    Vector2 targetPos = hoveredUnit.GetCurrentGridPos();
                    List<Tile> tilesInRange = Services.Grid.GetAttackTiles(sourcePos, currentSelectedCard.targetRange, activeUnit);
                    Tile targetTile = Services.Grid.GetTileAtPosition(targetPos);

                    // On survole une unité, elle est dans la portée ET c'est une cible valide
                    if (tilesInRange.Contains(targetTile) && currentSelectedCard.IsValidTarget(activeUnit, hoveredUnit))
                    {
                        hoveredPos = hoveredUnit.GetCurrentGridPos();
                        isValidHoverTarget = true;
                    }
                }
                else
                {
                    // Vérifie si on survole une tuile
                    string[] nameParts = hoveredObject.name.Split(' ');
                    if (nameParts.Length == 3 && nameParts[0] == "Tile")
                    {
                        if (int.TryParse(nameParts[1], out int gridX) && int.TryParse(nameParts[2], out int gridY))
                        {
                            hoveredPos = new Vector2(gridX, gridY);
                            Tile hoveredTile = Services.Grid.GetTileAtPosition(hoveredPos);

                            // Calcule la distance depuis l'unité active en utilisant le pathfinding (nombre de cases)
                            Vector2 sourcePos = activeUnit.GetCurrentGridPos();
                            List<Tile> tilesInRange = Services.Grid.GetAttackTiles(sourcePos, currentSelectedCard.targetRange, activeUnit);

                            // Vérifie si la tuile est dans la portée ET que c'est une cible valide
                            if (currentSelectedCard.targetsTile &&
                                tilesInRange.Contains(hoveredTile) &&
                                currentSelectedCard.IsValidTileTarget(hoveredTile))
                            {
                                isValidHoverTarget = true;
                            }
                        }
                    }
                }

                // Si on survole une cible valide et que c'est une nouvelle position
                if (isValidHoverTarget && hoveredPos != _lastHoveredTilePos)
                {
                    _lastHoveredTilePos = hoveredPos;

                    // Réaffiche les cibles de base (OPTIMISATION Phase 3.2: EventBus)
                    EventBus.Publish(new ShowCardTargetsEvent(currentSelectedCard, activeUnit));

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
                else if (!isValidHoverTarget && _lastHoveredTilePos != new Vector2(-1, -1))
                {
                    // Si on ne survole plus de cible valide, réaffiche juste les cibles de base (OPTIMISATION Phase 3.2: EventBus)
                    _lastHoveredTilePos = new Vector2(-1, -1);
                    EventBus.Publish(new ShowCardTargetsEvent(currentSelectedCard, activeUnit));
                }
            }
            else if (_lastHoveredTilePos != new Vector2(-1, -1))
            {
                // Si le raycast ne touche rien, réinitialise (OPTIMISATION Phase 3.2: EventBus)
                _lastHoveredTilePos = new Vector2(-1, -1);
                EventBus.Publish(new ShowCardTargetsEvent(currentSelectedCard, activeUnit));
            }
        }

        // --- GESTION DU CLIC DROIT (Désélection prioritaire) ---
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (_handUIController != null && _handUIController.SelectedCard != null)
            {
                Debug.Log("Clic droit détecté : désélection de la carte.");
                _handUIController.DeselectCard();
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
                    if (hoveredUI == null || hoveredUI.GetComponentInParent<HandUIController>() == null)
                    {
                        Debug.Log("Clic gauche vers le monde, carte sélectionnée : tentative de jeu");
                        // on continue vers HandleCardPlay
                    }
                    else
                    {
                        Debug.Log("Clic gauche sur la main UI consommé.");
                        return;
                    }
                }
            }

            // Si le pointeur n'est PAS sur l'UI, alors c'est une interaction avec le monde
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                GameObject clickedObject = hit.collider.gameObject;
                
                // Si une carte est sélectionnée, tenter de la jouer sur l'objet monde
                if (_handUIController != null && _handUIController.SelectedCard != null)
                {
                    Debug.Log($"Clic gauche sur monde avec carte sélectionnée : {clickedObject.name}");
                    HandleCardPlay(clickedObject, activeUnit);
                }
                else
                {
                    // Sinon (pas de carte sélectionnée), gérer l'interaction normale avec l'unité
                    Debug.Log($"Clic gauche sur monde sans carte sélectionnée : {clickedObject.name}");
                    HandleUnitInteraction(clickedObject, activeUnit);
                }
            }
            else
            {
                // Clic sur le vide (pas d'UI, pas de monde)
                if (_handUIController != null && _handUIController.SelectedCard != null)
                {
                    Debug.Log("Clic sur le vide avec carte sélectionnée. Désélection de la carte.");
                    _handUIController.DeselectCard();
                }
            }
            return; // Consomme le clic gauche après avoir traité l'interaction monde/vide
        }
    }

    private void HandleCardPlay(GameObject clickedObject, Unit activeUnit)
    {
        CardData selectedCard = _handUIController.SelectedCard;
        if (selectedCard == null) return; // Par sécurité

        // OPTIMISATION Phase 3.3: ComponentLocator
        clickedObject.TryGetComponentSafe(out Unit targetUnit);

        Vector2 targetTilePos = Vector2.zero;
        Tile targetTile = null;

        // Détermine la position cible
        if (targetUnit != null)
        {
            // Si on clique sur une unité, utilise sa position
            targetTilePos = targetUnit.GetCurrentGridPos();
        }
        else
        {
            // Si on clique sur une tuile
            string[] nameParts = clickedObject.name.Split(' ');
            if (nameParts.Length == 3 && nameParts[0] == "Tile")
            {
                if (int.TryParse(nameParts[1], out int gridX) &&
                    int.TryParse(nameParts[2], out int gridY))
                {
                    targetTilePos = new Vector2(gridX, gridY);
                    targetTile = Services.Grid.GetTileAtPosition(targetTilePos);
                }
            }
        }

        // Cas spécial : Carte Self (doit cliquer sur le joueur lui-même)
        if (selectedCard.targetType == CardTargetType.Self)
        {
            // Vérifie qu'on a cliqué sur l'unité active (le joueur)
            if (targetUnit != null && targetUnit == activeUnit)
            {
                StartCoroutine(PlayCardSequence(selectedCard, activeUnit, activeUnit, default));
                return; // La coroutine gère la suite
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
            StartCoroutine(PlayCardSequence(selectedCard, activeUnit, null, default));
            return; // La coroutine gère la suite
        }
        // Vérifie la portée de la carte pour les autres types
        else
        {
            Vector2 sourcePos = activeUnit.GetCurrentGridPos();
            float distance = Vector2.Distance(sourcePos, targetTilePos);
            if (distance > selectedCard.targetRange)
            {
                // Hors de portée, désélectionne la carte
                _handUIController.DeselectCard();
                return;
            }
        }

        // Logique pour jouer la carte avec validation stricte (sauf si déjà jouée)
        if (selectedCard.targetsUnit && targetUnit != null)
        {
            // Vérifie que l'unité est une cible valide selon le type de carte
            if (selectedCard.IsValidTarget(activeUnit, targetUnit))
            {
                StartCoroutine(PlayCardSequence(selectedCard, activeUnit, targetUnit, default));
                return; // La coroutine gère la suite
            }
            else
            {
                // Cible invalide, désélectionne la carte
                _handUIController.DeselectCard();
            }
        }
        else if (selectedCard.targetsTile && targetTile != null)
        {
            // Vérifie que la tuile est une cible valide selon le type de carte
            if (selectedCard.IsValidTileTarget(targetTile))
            {
                StartCoroutine(PlayCardSequence(selectedCard, activeUnit, null, targetTilePos));
                return; // La coroutine gère la suite
            }
            else
            {
                // Tuile invalide, désélectionne la carte
                _handUIController.DeselectCard();
            }
        }
        else if (!selectedCard.targetsUnit && !selectedCard.targetsTile)
        {
            // Carte sans cible, joue immédiatement
            StartCoroutine(PlayCardSequence(selectedCard, activeUnit, null, default));
            return; // La coroutine gère la suite
        }

        // Si on arrive ici, c'est que la cible était invalide ou hors de portée
        // (car les cas valides font un 'return' après avoir lancé la coroutine)
        _handUIController.DeselectCard();
    }

    private void HandleUnitInteraction(GameObject clickedObject, Unit activeUnit)
    {
        // Vérifie si c'est IlyaUnit ou Unit de base
        IlyaUnit ilyaUnit = activeUnit as IlyaUnit;
        
        // Récupère les points de mouvement disponibles (PM pour toutes les unités)
        int availablePoints = activeUnit.GetCurrentMovementPoints();

        // Vérifie si on clique sur une unité (OPTIMISATION Phase 3.3: ComponentLocator)
        clickedObject.TryGetComponentSafe(out Unit clickedUnit);

        // Les attaques se font uniquement via les cartes, plus d'attaque de base
        if (clickedUnit == activeUnit)
        {
            Debug.Log("Clic sur l'unité active.");
        }
        else
        {
            // DÉPLACEMENT
            string[] nameParts = clickedObject.name.Split(' ');
            if (nameParts.Length == 3 && nameParts[0] == "Tile")
            {
                if (int.TryParse(nameParts[1], out int gridX) && 
                    int.TryParse(nameParts[2], out int gridY))
                {
                    Vector2 targetGridPos = new Vector2(gridX, gridY);

                    // Vérifie si l'unité a encore des points de mouvement
                    if (availablePoints <= 0)
                    {
                        Debug.LogWarning($"{activeUnit.name} n'a plus de PM ! (PM: {availablePoints})");
                        return;
                    }

                    // Récupère les tuiles atteignables
                    Dictionary<Tile, int> reachableTilesWithCost = Services.Grid.GetMovementTiles(
                        activeUnit.GetCurrentGridPos(), 
                        availablePoints, 
                        activeUnit
                    );

                    Tile targetTile = Services.Grid.GetTileAtPosition(targetGridPos);

                    if (targetTile != null && reachableTilesWithCost.ContainsKey(targetTile))
                    {
                        // Calcule le chemin
                        List<Tile> pathToTarget = Services.Grid.GetPathToTile(
                            activeUnit.GetCurrentGridPos(), 
                            targetGridPos, 
                            availablePoints, 
                            activeUnit
                        );

                        if (pathToTarget != null && pathToTarget.Count > 0)
                        {
                            int movementCost = pathToTarget.Count;

                            if (availablePoints >= movementCost)
                            {
                                Debug.Log($"{activeUnit.name} se déplace vers {targetGridPos} (coût: {movementCost})");

                                // Efface l'ancien affichage (OPTIMISATION Phase 3.2: EventBus)
                                EventBus.Publish(new ResetTileColorsEvent());
                                
                                // Déplace l'unité
                                activeUnit.MoveToTile(pathToTarget);
                                
                                // IMPORTANT : Tous les déplacements dépensent des PM (Points de Mouvement)
                                activeUnit.SpendMovement(movementCost);
                                Debug.Log($"PM dépensés : {movementCost}. Restant : {activeUnit.GetCurrentMovementPoints()}/{activeUnit.GetMaxMovementPoints()}");
                                
                                // Met à jour l'UI
                                Services.Grid.UpdateUnitUI();
                                
                                // Rafraîchit la portée après mouvement
                                StartCoroutine(RefreshRangeAfterMovement(activeUnit));
                            }
                            else
                            {
                                Debug.LogWarning($"{activeUnit.name} n'a pas assez de points ({availablePoints}) pour {targetGridPos} (coût {movementCost})");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Aucun chemin valide vers {targetGridPos}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"La tuile {targetGridPos} n'est pas atteignable.");
                    }
                }
            }
        }
    }

    // Rafraîchit la portée après le mouvement
    private IEnumerator RefreshRangeAfterMovement(Unit unit)
    {
        yield return new WaitForSeconds(0.1f);

        // Tous les déplacements utilisent les PM (Points de Mouvement)
        int remainingPM = unit.GetCurrentMovementPoints();

        if (unit != null && remainingPM > 0)
        {
            // OPTIMISATION Phase 3.2: EventBus
            EventBus.Publish(new ShowMovementRangeEvent(unit));
            Debug.Log($"Portée rafraîchie : {remainingPM} PM restants");
        }
        else if (unit != null)
        {
            // Plus de points de mouvement, on réinitialise juste l'affichage (OPTIMISATION Phase 3.2: EventBus)
            EventBus.Publish(new ResetTileColorsEvent());
            Debug.Log($"{unit.name} n'a plus de PM.");
        }
    }

    private IEnumerator PlayCardSequence(CardData card, Unit source, Unit target, Vector2 targetTilePos)
    {
        // 1. Tourne pour faire face à la cible
        if (target != null && target != source)
        {
            yield return StartCoroutine(source.LookAtCoroutine(target.transform.position));
        }
        else if (card.targetsTile)
        {
            Tile tile = Services.Grid.GetTileAtPosition(targetTilePos);
            if (tile != null)
            {
                // Cible le centre de la tuile
                Vector3 targetWorldPos = tile.transform.position + new Vector3(0, 0.5f, 0);
                yield return StartCoroutine(source.LookAtCoroutine(targetWorldPos));
            }
        }

        // 2. Joue la carte après la rotation
        _handUIController.PlaySelectedCard(target, targetTilePos);
    }
}