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

    [Header("Disposition en Arc")]
    [SerializeField] private float _arcRadius = 50f; // Profondeur de la courbe (en pixels, plus élevé = plus courbé)
    [SerializeField] private float _arcAngle = 20f; // Angle de rotation max des cartes sur les bords (en degrés)
    [SerializeField] private float _cardSpacing = 120f; // Espacement horizontal entre cartes (en pixels)
    [SerializeField] private float _maxOverlap = 0.7f; // Chevauchement max quand trop de cartes (0-1, 1 = pas de chevauchement)
    [SerializeField] private float _hoverLiftDistance = 50f; // Distance que la carte monte au hover (en pixels)
    [SerializeField] private float _verticalOffset = 0f; // Offset vertical de base de la main (en pixels)

    [Header("Preview de Ciblage")]
    [SerializeField] private Vector2 _cardPreviewPosition = new Vector2(200f, 0f); // Position de la carte en mode ciblage (relative au centre du canvas)
    [SerializeField] private float _cardPreviewScale = 1.0f; // Échelle de la carte en mode preview (1.0 = taille normale)
    [SerializeField] private Vector2 _curveStartOffset = new Vector2(300f, 0f); // Point de départ de la courbe (position fixe dans le canvas)
    [SerializeField] private TargetingCurve _targetingCurve; // Référence à la courbe de ciblage
    [SerializeField] private TargetingReticle _targetingReticle; // Référence au réticule de ciblage
    [SerializeField] private Color _curveColor = new Color(0f, 1f, 1f, 0.8f); // Couleur cyan avec transparence
    [SerializeField] private Color _reticleColor = new Color(1f, 1f, 0f, 0.9f); // Couleur jaune avec transparence

    private DeckManager _playerDeckManager;
    private List<GameObject> _instantiatedCardUIs = new List<GameObject>();
    private CardData _selectedCard = null; // La carte actuellement sélectionnée par le joueur
    private GameObject _selectedCardUIObject = null; // Le GameObject UI de la carte sélectionnée
    private RectTransform _selectedCardRect = null; // Cache du RectTransform pour performance
    private GameObject _hoveredCard = null; // La carte actuellement survolée
    private Vector2 _hoveredCardOriginalPos;
    private Quaternion _hoveredCardOriginalRot;
    private int _hoveredCardOriginalSiblingIndex;
    private Vector2 _lastMousePos = Vector2.zero; // Cache de la dernière position souris

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
        CardUIElement.OnCardHoverEnter += HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit += HandleCardHoverExit;
    }

    void OnDisable()
    {
        CardUIElement.OnCardClicked -= HandleCardClicked; // Se désabonner pour éviter les fuites de mémoire
        CardUIElement.OnCardHoverEnter -= HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit -= HandleCardHoverExit;
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
        // Nettoyage des events statiques (CRITIQUE pour éviter fuites mémoire)
        CardUIElement.OnCardClicked -= HandleCardClicked;
        CardUIElement.OnCardHoverEnter -= HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit -= HandleCardHoverExit;

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

        // Si une carte est sélectionnée et nécessite une cible, la positionner à gauche en mode preview
        if (_selectedCard != null && (_selectedCard.targetsUnit || _selectedCard.targetsTile) && _selectedCardUIObject != null)
        {
            // Cache le RectTransform si pas déjà fait
            if (_selectedCardRect == null)
            {
                _selectedCardRect = _selectedCardUIObject.GetRequiredComponent<RectTransform>("Selected card UI");
            }

            if (_selectedCardRect != null)
            {
                // Positionner la carte à gauche de l'écran (mode preview)
                _selectedCardRect.anchoredPosition = _cardPreviewPosition;
                _selectedCardRect.localScale = Vector3.one * _cardPreviewScale;
                _selectedCardRect.localRotation = Quaternion.identity; // Pas de rotation

                // Dessiner la courbe de ciblage vers la souris (seulement si la souris a bougé)
                if (_targetingCurve != null)
                {
                    // Position de la souris en coordonnées canvas
                    Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

                    // Ne recalculer que si la souris a bougé
                    if (Vector2.Distance(mousePos, _lastMousePos) > 1f)
                    {
                        _lastMousePos = mousePos;

                        Vector2 localMousePos;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, mousePos, null, out localMousePos);

                        // Point de départ : position fixe configurée dans l'Inspector
                        Vector2 curveStartPoint = _curveStartOffset;

                        // Activer et mettre à jour la courbe (en utilisant directement les coordonnées canvas)
                        _targetingCurve.gameObject.SetActive(true);
                        _targetingCurve.color = _curveColor;
                        _targetingCurve.UpdateCurve(curveStartPoint, localMousePos);

                        // Afficher le réticule de ciblage à la position de la souris
                        if (_targetingReticle != null)
                        {
                            _targetingReticle.gameObject.SetActive(true);
                            _targetingReticle.color = _reticleColor;
                            _targetingReticle.UpdatePosition(localMousePos);
                        }
                    }
                }
            }
        }
        else
        {
            // Réinitialiser le cache du RectTransform
            _selectedCardRect = null;
            _lastMousePos = Vector2.zero;

            // Cacher la courbe et le réticule si aucune carte n'est sélectionnée
            if (_targetingCurve != null && _targetingCurve.gameObject.activeSelf)
            {
                _targetingCurve.Hide();
                _targetingCurve.gameObject.SetActive(false);
            }

            if (_targetingReticle != null && _targetingReticle.gameObject.activeSelf)
            {
                _targetingReticle.Hide();
                _targetingReticle.gameObject.SetActive(false);
            }
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

        // Arranger les cartes en arc
        ArrangeCardsInArc();
    }

    /// <summary>
    /// Dispose les cartes en arc de cercle avec chevauchement
    /// </summary>
    private void ArrangeCardsInArc()
    {
        int cardCount = _instantiatedCardUIs.Count;
        if (cardCount == 0) return;

        // Calculer l'espacement effectif avec chevauchement
        float effectiveSpacing = _cardSpacing;
        if (cardCount > 1)
        {
            // Si trop de cartes, réduire l'espacement (chevauchement)
            float totalWidth = (cardCount - 1) * _cardSpacing;
            float maxWidth = _canvasRectTransform.rect.width * _maxOverlap;
            if (totalWidth > maxWidth)
            {
                effectiveSpacing = maxWidth / (cardCount - 1);
            }
        }

        // RectTransform du HandContainer pour calculer le centre
        RectTransform handRect = _handContainer.GetComponent<RectTransform>();
        if (handRect == null)
        {
            Debug.LogError("HandContainer n'a pas de RectTransform!");
            return;
        }

        float handWidth = handRect.rect.width;
        float centerX = handWidth / 2f; // Centre du HandContainer

        // Largeur totale occupée par toutes les cartes
        float totalSpread = (cardCount - 1) * effectiveSpacing;
        float startX = centerX - (totalSpread / 2f); // Commence à gauche, centré

        for (int i = 0; i < cardCount; i++)
        {
            GameObject cardUI = _instantiatedCardUIs[i];
            RectTransform cardRect = cardUI.GetComponent<RectTransform>();
            if (cardRect == null) continue;

            // Position horizontale : répartition uniforme de gauche à droite, centrée
            float x = startX + (i * effectiveSpacing);

            // Position verticale : arc (descend du centre vers les bords)
            // Normaliser la position de 0 (centre) à 1 (extrémités)
            float t = cardCount > 1 ? Mathf.Abs((i / (float)(cardCount - 1)) - 0.5f) * 2f : 0f;
            float y = _verticalOffset - (t * t * _arcRadius); // Courbe parabolique

            // Angle de rotation basé sur la position
            float angle = 0f;
            if (cardCount > 1)
            {
                float angleT = (i / (float)(cardCount - 1)) - 0.5f; // -0.5 à +0.5
                angle = angleT * _arcAngle;
            }

            // Appliquer la position et rotation
            cardRect.anchoredPosition = new Vector2(x, y);
            cardRect.localRotation = Quaternion.Euler(0, 0, -angle);
            cardRect.SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// Gère le hover sur une carte - la fait de sortir de la main
    /// </summary>
    private void HandleCardHoverEnter(GameObject cardUI)
    {
        if (_hoveredCard != null || cardUI == _selectedCardUIObject) return;

        _hoveredCard = cardUI;
        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        if (cardRect == null) return;

        // Sauvegarder la position et rotation d'origine
        _hoveredCardOriginalPos = cardRect.anchoredPosition;
        _hoveredCardOriginalRot = cardRect.localRotation;
        _hoveredCardOriginalSiblingIndex = cardRect.GetSiblingIndex();

        // Mettre la carte au-dessus des autres (dernier sibling = devant)
        cardRect.SetAsLastSibling();

        // Lever la carte vers le haut
        Vector2 targetPos = _hoveredCardOriginalPos + new Vector2(0, _hoverLiftDistance);
        cardRect.anchoredPosition = targetPos;

        // Rotation à 0 (carte droite)
        cardRect.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Gère la sortie du hover - remet la carte à sa place
    /// </summary>
    private void HandleCardHoverExit(GameObject cardUI)
    {
        if (_hoveredCard != cardUI) return;

        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        if (cardRect == null) return;

        // Restaurer la position et rotation d'origine
        cardRect.anchoredPosition = _hoveredCardOriginalPos;
        cardRect.localRotation = _hoveredCardOriginalRot;
        cardRect.SetSiblingIndex(_hoveredCardOriginalSiblingIndex);

        _hoveredCard = null;
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

                    // Détacher du HandContainer et attacher directement au Canvas
                    _selectedCardUIObject.transform.SetParent(_canvasRectTransform);

                    // Mettre la carte au-dessus de tout (dernier dans la hiérarchie = rendu en dernier = au-dessus)
                    _selectedCardUIObject.transform.SetAsLastSibling();

                    // Ajouter un Canvas sur la carte pour contrôler le sorting order
                    Canvas cardCanvas = _selectedCardUIObject.GetComponent<Canvas>();
                    if (cardCanvas == null)
                    {
                        cardCanvas = _selectedCardUIObject.AddComponent<Canvas>();
                    }
                    cardCanvas.overrideSorting = true;
                    cardCanvas.sortingOrder = 1000; // Très haut pour être au-dessus de tout

                    // Ajouter GraphicRaycaster si nécessaire pour que la carte reste cliquable
                    if (_selectedCardUIObject.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    {
                        _selectedCardUIObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }

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
            if (activeUnit != null && clickedCard.targetType == CardTargetType.Self && clickedCard.isAOE)
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

        bool canAfford = false;

        if (cardUIElement.CardData.costPA > 0)
        {
            if (activeUnit is IActionPointsUser paUser)
            {
                canAfford = paUser.GetCurrentPA() >= cardUIElement.CardData.costPA;
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
            // Supprimer d'abord le GraphicRaycaster (dépend du Canvas)
            UnityEngine.UI.GraphicRaycaster raycaster = _selectedCardUIObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
            {
                Destroy(raycaster);
            }

            // Ensuite supprimer le Canvas
            Canvas cardCanvas = _selectedCardUIObject.GetComponent<Canvas>();
            if (cardCanvas != null)
            {
                Destroy(cardCanvas);
            }

            // Réactive les raycasts sur la carte
            CanvasGroup canvasGroup = _selectedCardUIObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }

            // Réattacher la carte au HandContainer si elle avait été détachée
            _selectedCardUIObject.transform.SetParent(_handContainer);

            // Réarranger toutes les cartes en arc pour repositionner correctement
            ArrangeCardsInArc();

            _selectedCardUIObject = null;
        }
    }
}