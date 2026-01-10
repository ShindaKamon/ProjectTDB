using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HandUIController : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private GameObject _cardUIPrefab; // Le préfab UI d'une carte individuelle
    [SerializeField] private Transform _handContainer; // Le parent où les cartes seront instanciées
    [SerializeField] private RectTransform _canvasRectTransform; // Référence au RectTransform du Canvas

    private DeckManager _playerDeckManager;
    private List<GameObject> _instantiatedCardUIs = new List<GameObject>();
    private CardData _selectedCard = null; // La carte actuellement sélectionnée par le joueur
    private GameObject _selectedCardUIObject = null; // Le GameObject UI de la carte sélectionnée

    // Propriété publique pour que l'InputManager puisse accéder à la carte sélectionnée
    public CardData SelectedCard => _selectedCard;

    public void DeselectCard()
    {
        if (_selectedCard != null)
        {
            Debug.Log($"Carte {_selectedCard.cardName} désélectionnée via appel externe.");
            _selectedCard = null;
            ResetSelectedCardUIPosition();
            ResetCardHighlights();

            // Invalide le cache d'attaque (OPTIMISATION: plus de carte sélectionnée)
            Services.Grid?.InvalidateAttackTilesCache();
        }

        // Toujours réafficher la portée de mouvement après désélection
        // (même si aucune carte n'était sélectionnée, pour être sûr)
        Unit activeUnit = Services.Grid?.GetActiveUnit();
        if (activeUnit != null)
        {
            // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
            EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
        }
    }

    void OnEnable()
    {
        CardUIElement.OnCardClicked += HandleCardClicked; // S'abonner à l'événement de clic sur les cartes
    }

    void OnDisable()
    {
        CardUIElement.OnCardClicked -= HandleCardClicked; // Se désabonner pour éviter les fuites de mémoire
    }

    private bool _isInitialized = false;

    void Start()
    {
        // Essaie d'initialiser immédiatement
        TryInitialize();
    }

    private void TryInitialize()
    {
        // Trouver le DeckManager du joueur actif
        if (Services.Grid != null && Services.Grid.GetActiveUnit() != null)
        {
            // OPTIMISATION Phase 3.3: ComponentLocator
            Services.Grid.GetActiveUnit().TryGetComponentSafe(out _playerDeckManager);
            if (_playerDeckManager != null)
            {
                _playerDeckManager.OnHandChanged += UpdateHandUI; // S'abonner à l'événement de changement de main
                UpdateHandUI(); // Mettre à jour l'UI immédiatement après l'abonnement
                Debug.Log("HandUIController: Initialisé avec succès!");
            }
            else
            {
                Debug.LogWarning("DeckManager introuvable sur l'unité active du joueur.");
                return;
            }

            // S'abonner aux changements de PA si c'est IlyaUnit
            IlyaUnit ilyaUnit = Services.Grid.GetActiveUnit() as IlyaUnit;
            if (ilyaUnit != null)
            {
                ilyaUnit.OnActionPointsChanged += HandlePAChanged;
            }

            _isInitialized = true;
        }
    }

    void OnDestroy()
    {
        if (_playerDeckManager != null)
        {
            _playerDeckManager.OnHandChanged -= UpdateHandUI; // Se désabonner pour éviter les fuites de mémoire
        }

        // Se désabonner des changements de PA
        // Vérifie que le service est disponible avant d'y accéder (évite erreurs lors de la destruction de scène)
        if (Services.IsGridServiceAvailable())
        {
            Unit activeUnit = Services.Grid.GetActiveUnit();
            if (activeUnit != null)
            {
                IlyaUnit ilyaUnit = activeUnit as IlyaUnit;
                if (ilyaUnit != null)
                {
                    ilyaUnit.OnActionPointsChanged -= HandlePAChanged;
                }
            }
        }
    }

    /// <summary>
    /// Appelé quand les PA changent pour mettre à jour l'état des cartes
    /// </summary>
    private void HandlePAChanged(int current, int max)
    {
        RefreshCardAffordability();
    }

    void Update()
    {
        // Continue d'essayer jusqu'à ce que l'initialisation réussisse
        if (!_isInitialized)
        {
            TryInitialize();
        }

        // Si une carte est sélectionnée et nécessite une cible, la faire suivre la souris
        if (_selectedCard != null && (_selectedCard.targetsUnit || _selectedCard.targetsTile) && _selectedCardUIObject != null)
        {
            Vector2 localPoint;
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, mousePos, null, out localPoint);
            // OPTIMISATION Phase 3.3: ComponentLocator
            _selectedCardUIObject.GetRequiredComponent<RectTransform>("Selected card UI").localPosition = localPoint;
        }
    }

    private void UpdateHandUI()
    {
        // Supprimer toutes les cartes UI existantes
        foreach (GameObject cardUI in _instantiatedCardUIs)
        {
            Destroy(cardUI);
        }
        _instantiatedCardUIs.Clear();
        _selectedCardUIObject = null; // Réinitialiser le GameObject UI de la carte sélectionnée

        // Instancier de nouvelles cartes UI pour chaque carte dans la main
        if (_playerDeckManager != null)
        {
            foreach (CardData cardData in _playerDeckManager.GetHand())
            {
                GameObject cardUI = Instantiate(_cardUIPrefab, _handContainer);
                _instantiatedCardUIs.Add(cardUI);

                // OPTIMISATION Phase 3.3: ComponentLocator
                CardUIElement cardUIElement = cardUI.GetRequiredComponent<CardUIElement>("Card UI instantiated");
                if (cardUIElement != null)
                {
                    cardUIElement.SetCardData(cardData);

                    // Vérifier si le joueur peut se permettre cette carte
                    UpdateCardAffordability(cardUIElement);

                    // Si c'est la carte précédemment sélectionnée, la réassigner et la surligner
                    if (_selectedCard != null && cardUIElement.CardData == _selectedCard)
                    {
                        _selectedCardUIObject = cardUI;
                        cardUIElement.SetSelected(true);
                    }
                }
                else
                {
                    Debug.LogWarning($"Le préfab de carte UI '{_cardUIPrefab.name}' ne contient pas de composant CardUIElement.");
                }
            }
        }

        // Assurez-vous que la carte sélectionnée (si elle existe toujours) est visuellement sélectionnée
        if (_selectedCard != null && _selectedCardUIObject == null)
        {
            // Si la carte sélectionnée existe mais que son GameObject UI n'a pas été trouvé (par exemple, main rafraîchie),
            // nous devons la retrouver et la surligner.
            HighlightSelectedCard(_selectedCard);
        }
    }

    private void HandleCardClicked(CardData clickedCard)
    {
        Debug.Log($"HandUIController a reçu un clic sur : {clickedCard.cardName}");

        // Réinitialiser la position de la carte précédemment sélectionnée si elle suivait la souris
        ResetSelectedCardUIPosition();
        // Désélectionner visuellement toutes les cartes d'abord
        ResetCardHighlights();

        if (_selectedCard == clickedCard)
        {
            // Si la même carte est cliquée à nouveau, la désélectionner
            _selectedCard = null;
            _selectedCardUIObject = null;

            // Invalide le cache d'attaque (OPTIMISATION: nouvelle carte = nouveau calcul)
            Services.Grid.InvalidateAttackTilesCache();

            // Masquer les cibles de carte et réafficher la portée de mouvement
            Unit activeUnit = Services.Grid.GetActiveUnit();
            if (activeUnit != null)
            {
                // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
                EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
            }
            Debug.Log("Carte désélectionnée.");
        }
        else
        {
            // Sélectionner la nouvelle carte
            _selectedCard = clickedCard;
            HighlightSelectedCard(_selectedCard);

            // Invalide le cache d'attaque (OPTIMISATION: nouvelle carte = nouveau calcul)
            Services.Grid.InvalidateAttackTilesCache();

            // Récupérer l'unité active une fois pour toute la méthode
            Unit activeUnit = Services.Grid.GetActiveUnit();

            // Afficher les cibles valides pour cette carte
            if (activeUnit != null)
            {
                // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
                EventBus.Publish(new ShowCardTargetsEvent(_selectedCard, activeUnit));
            }

            Debug.Log($"Carte sélectionnée : {_selectedCard.cardName}");

            // Trouver le GameObject UI correspondant à la carte sélectionnée pour le faire suivre la souris
            foreach (GameObject cardUIObject in _instantiatedCardUIs)
            {
                // OPTIMISATION Phase 3.3: ComponentLocator
                if (cardUIObject.TryGetComponentSafe(out CardUIElement cardUIElement) && cardUIElement.CardData == _selectedCard)
                {
                    _selectedCardUIObject = cardUIObject;
                    _selectedCardUIObject.transform.SetParent(transform.root); // Détacher du HandContainer

                    // Désactive le raycast sur cette carte pour qu'elle ne bloque pas les clics sur le monde
                    // OPTIMISATION Phase 3.3: ComponentLocator
                    if (!_selectedCardUIObject.TryGetComponentSafe(out CanvasGroup canvasGroup))
                    {
                        canvasGroup = _selectedCardUIObject.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.blocksRaycasts = false;

                    break;
                }
            }

            // Affiche la zone d'effet pour les cartes AOE Self
            if (clickedCard.targetType == CardTargetType.Self && clickedCard.isAOE)
            {
                // Carte AOE Self : afficher la zone AOE autour du joueur
                Vector2 playerPos = activeUnit.GetCurrentGridPos();
                // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
                EventBus.Publish(new ShowAOEZoneEvent(playerPos, clickedCard.aoeRadius, clickedCard, activeUnit));
            }
            // Note: Toutes les autres cartes affichent déjà leur portée via ShowCardTargets (ligne 160)
            // Aucune carte n'est jouée automatiquement, l'utilisateur doit cliquer pour confirmer
        }
    }

    // Méthode pour jouer la carte actuellement sélectionnée
    public void PlaySelectedCard(Unit targetUnit, Vector2 targetTile)
    {
        // Validation des préconditions
        if (_selectedCard == null)
        {
            Debug.LogWarning("Aucune carte sélectionnée.");
            return;
        }

        if (_playerDeckManager == null)
        {
            Debug.LogError("DeckManager introuvable - impossible de jouer la carte.");
            return;
        }

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        if (activeUnit == null)
        {
            Debug.LogError("Aucune unité active - impossible de jouer la carte.");
            return;
        }

        // Validation centralisée : peut-on jouer cette carte ?
        ValidationResult canPlayResult = GameActionValidator.CanPlayCard(activeUnit, _selectedCard);
        if (!canPlayResult.IsValid)
        {
            Debug.LogWarning($"❌ Impossible de jouer {_selectedCard.cardName} : {canPlayResult.ErrorMessage}");
            return;
        }

        // Validation centralisée : le ciblage est-il valide ?
        if (_selectedCard.targetsUnit)
        {
            ValidationResult targetResult = GameActionValidator.CanTargetUnit(_selectedCard, activeUnit, targetUnit);
            if (!targetResult.IsValid)
            {
                Debug.LogWarning($"❌ Ciblage invalide : {targetResult.ErrorMessage}");
                return;
            }
        }

        if (_selectedCard.targetsTile)
        {
            ValidationResult tileResult = GameActionValidator.CanTargetTile(_selectedCard, activeUnit, targetTile);
            if (!tileResult.IsValid)
            {
                Debug.LogWarning($"❌ Ciblage de tuile invalide : {tileResult.ErrorMessage}");
                return;
            }
        }

        // Toutes les validations passées, exécuter la carte
        _selectedCard.ExecuteEffect(activeUnit, targetUnit, targetTile);
        _playerDeckManager.PlayCard(_selectedCard);

        // Déduire le coût en PA (validation déjà faite par CanPlayCard)
        if (_selectedCard.costPA > 0 && activeUnit is IActionPointsUser paUser)
        {
            paUser.SpendPA(_selectedCard.costPA);
        }

        // Nettoyage UI
        Services.Grid.UpdateUnitUI();
        _selectedCard = null;
        ResetSelectedCardUIPosition();
        ResetCardHighlights();
        // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
        EventBus.Publish(new ResetTileColorsEvent());
        EventBus.Publish(new ShowMovementRangeEvent(activeUnit));

        Debug.Log($"✅ Carte jouée avec succès");
    }

    // Méthode pour surligner visuellement la carte sélectionnée
    private void HighlightSelectedCard(CardData cardToHighlight)
    {
        foreach (GameObject cardUIObject in _instantiatedCardUIs)
        {
            CardUIElement cardUIElement = cardUIObject.GetComponent<CardUIElement>();
            if (cardUIElement != null)
            {
                cardUIElement.SetSelected(cardUIElement.CardData == cardToHighlight);
            }
        }
    }

    /// <summary>
    /// Vérifie si le joueur peut se permettre une carte et met à jour son affichage
    /// </summary>
    private void UpdateCardAffordability(CardUIElement cardUIElement)
    {
        if (cardUIElement == null || cardUIElement.CardData == null) return;

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        IlyaUnit ilyaUnit = activeUnit as IlyaUnit;

        bool canAfford = false;

        if (cardUIElement.CardData.costPA > 0)
        {
            if (ilyaUnit != null)
            {
                canAfford = ilyaUnit.GetCurrentPA() >= cardUIElement.CardData.costPA;
            }
            else
            {
                canAfford = false; // Les unités non-Ilya ne peuvent pas jouer de cartes avec coût PA
            }
        }
        else
        {
            // Carte gratuite (0 PA)
            canAfford = true;
        }

        cardUIElement.SetAffordable(canAfford);
    }

    /// <summary>
    /// Met à jour l'état de toutes les cartes en main (appelé quand les PA changent)
    /// </summary>
    public void RefreshCardAffordability()
    {
        foreach (GameObject cardUIObject in _instantiatedCardUIs)
        {
            CardUIElement cardUIElement = cardUIObject.GetComponent<CardUIElement>();
            if (cardUIElement != null)
            {
                UpdateCardAffordability(cardUIElement);
            }
        }
    }

    // Méthode pour réinitialiser le surlignage de toutes les cartes
    private void ResetCardHighlights()
    {
        foreach (GameObject cardUIObject in _instantiatedCardUIs)
        {
            CardUIElement cardUIElement = cardUIObject.GetComponent<CardUIElement>();
            if (cardUIElement != null)
            {
                cardUIElement.SetSelected(false);
            }
        }
    }

    // Réinitialise la position du GameObject UI de la carte sélectionnée
    private void ResetSelectedCardUIPosition()
    {
        if (_selectedCardUIObject != null)
        {
            // Réactive les raycasts sur la carte
            CanvasGroup canvasGroup = _selectedCardUIObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }

            // Réattacher la carte au HandContainer si elle avait été détachée
            _selectedCardUIObject.transform.SetParent(_handContainer);
            // S'assurer que la mise en page se recalcule
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handContainer.GetComponent<RectTransform>());
            _selectedCardUIObject = null;
        }
    }
}