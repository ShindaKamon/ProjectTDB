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
    private List<Unit> _pendingMultiTargets = new List<Unit>(); // Cibles déjà choisies pour une carte à cibles multiples (ex: Frappe rapide)
    private GameObject _selectedCardUIObject = null; // Le GameObject UI de la carte sélectionnée
    private RectTransform _selectedCardRect = null; // Cache du RectTransform pour performance
    private GameObject _hoveredCard = null; // La carte actuellement survolée
    private Vector2 _hoveredCardOriginalPos;
    private Quaternion _hoveredCardOriginalRot;
    private int _hoveredCardOriginalSiblingIndex;
    private Vector2 _lastMousePos = Vector2.zero; // Cache de la dernière position souris

    // Propriété publique pour que l'InputManager puisse accéder à la carte sélectionnée
    public CardData SelectedCard => _selectedCard;

    // Carte de déplacement d'invocation (ex: Écho évanescent) : invocation choisie à la 1re étape
    // du ciblage ; null tant que le joueur n'en a pas choisi (il doit alors cliquer une invocation).
    private SummonUnit _summonToMove;
    public SummonUnit SummonToMove => _summonToMove;

    public void DeselectCard()
    {
        if (_selectedCard != null)
        {
            GameLog.Log($"Carte {_selectedCard.cardName} désélectionnée via appel externe.");
            _selectedCard = null;
            _pendingMultiTargets.Clear();
            _summonToMove = null;
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

    /// <summary>
    /// Annule une étape de ciblage : revient au choix de l'invocation pour une carte de
    /// déplacement d'invocation, retire la dernière cible choisie pour une carte à cibles
    /// multiples, ou désélectionne complètement la carte s'il n'y a aucune étape en attente.
    /// </summary>
    public void CancelTargetingStep()
    {
        if (_summonToMove != null)
        {
            GameLog.Log($"{_summonToMove.name} n'est plus sélectionnée : choisis une invocation.");
            _summonToMove = null;
            Unit activeUnit = Services.Grid?.GetActiveUnit();
            if (activeUnit != null)
                EventBus.Publish(new ShowCardTargetsEvent(_selectedCard, activeUnit));
        }
        else if (_pendingMultiTargets.Count > 0)
        {
            Unit removed = _pendingMultiTargets[_pendingMultiTargets.Count - 1];
            _pendingMultiTargets.RemoveAt(_pendingMultiTargets.Count - 1);
            GameLog.Log($"Cible {removed.name} retirée de la sélection multi-cibles ({_pendingMultiTargets.Count}/{_selectedCard.targetCount}).");
        }
        else
        {
            DeselectCard();
        }
    }

    /// <summary>
    /// Ajoute une cible à la sélection en cours pour une carte à cibles multiples (ex: Frappe rapide).
    /// Exécute automatiquement la carte dès que le nombre de cibles requis est atteint, ou dès
    /// qu'il n'y a plus aucune cible valide restante sur le terrain (carte jouée avec moins de
    /// cibles que prévu si le terrain n'en offre pas assez).
    /// </summary>
    public void AddMultiTarget(Unit target)
    {
        if (_selectedCard == null || !_selectedCard.isMultiTarget || target == null) return;

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        if (activeUnit == null) return;

        if (_pendingMultiTargets.Contains(target))
        {
            GameLog.Log($"{target.name} est déjà sélectionné pour {_selectedCard.cardName}.");
            return;
        }

        ValidationResult targetResult = GameActionValidator.CanTargetUnit(_selectedCard, activeUnit, target);
        if (!targetResult.IsValid)
        {
            GameLog.LogWarning($"❌ Ciblage invalide : {targetResult.ErrorMessage}");
            return;
        }

        _pendingMultiTargets.Add(target);
        GameLog.Log($"🎯 Cible {target.name} ajoutée pour {_selectedCard.cardName} ({_pendingMultiTargets.Count}/{_selectedCard.targetCount}).");

        if (_pendingMultiTargets.Count >= _selectedCard.targetCount || !HasRemainingValidMultiTarget(activeUnit))
        {
            ExecutePendingMultiTargetCard(activeUnit);
        }
    }

    /// <summary>
    /// Vérifie s'il reste, sur le terrain, au moins une unité valide pour la carte en cours
    /// qui n'a pas déjà été choisie comme cible.
    /// </summary>
    private bool HasRemainingValidMultiTarget(Unit activeUnit)
    {
        foreach (Unit candidate in Services.Grid.GetAllUnits())
        {
            if (_pendingMultiTargets.Contains(candidate)) continue;
            if (GameActionValidator.CanTargetUnit(_selectedCard, activeUnit, candidate).IsValid)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Exécute la carte à cibles multiples sur toutes les cibles accumulées, puis paie le coût
    /// une seule fois (pas par cible) et nettoie l'UI, à l'identique de PlaySelectedCard.
    /// </summary>
    private void ExecutePendingMultiTargetCard(Unit activeUnit)
    {
        ValidationResult canPlayResult = GameActionValidator.CanPlayCard(activeUnit, _selectedCard);
        if (!canPlayResult.IsValid)
        {
            GameLog.LogWarning($"❌ Impossible de jouer {_selectedCard.cardName} : {canPlayResult.ErrorMessage}");
            DeselectCard();
            return;
        }

        bool isFirstTarget = true;
        foreach (Unit target in _pendingMultiTargets)
        {
            // Seule la 1ère cible déclenche les effets "une fois par carte jouée" (combo
            // tracker, invocation, dégâts sur soi, pioche, fetch, ajout au deck, écho de Lyse) —
            // voir CardData.ExecuteEffect(isAdditionalMultiTargetHit).
            _selectedCard.ExecuteEffect(activeUnit, target, default, !isFirstTarget);
            isFirstTarget = false;
        }

        PayCostsAndEndCardPlay(activeUnit);
        GameLog.Log($"✅ Carte à cibles multiples jouée avec succès");
    }

    /// <summary>
    /// Étape 1 d'une carte de déplacement d'invocation : le joueur clique une de ses invocations.
    /// La portée affichée passe alors aux cases autour de cette invocation.
    /// </summary>
    public void SelectSummonToMove(Unit target)
    {
        if (_selectedCard == null || !_selectedCard.isRepositionSummonCard) return;

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        ValidationResult result = GameActionValidator.CanSelectSummonToMove(_selectedCard, activeUnit, target);
        if (!result.IsValid)
        {
            GameLog.Log($"❌ {_selectedCard.cardName} : {result.ErrorMessage}");
            return;
        }

        _summonToMove = (SummonUnit)target;
        GameLog.Log($"🎯 {_summonToMove.name} sélectionnée : choisis sa case d'arrivée.");
        EventBus.Publish(new ShowCardTargetsEvent(_selectedCard, _summonToMove));
    }

    /// <summary>
    /// Étape 2 d'une carte de déplacement d'invocation : déplace l'invocation choisie sur la case
    /// cliquée, puis paie la carte. Case invalide : rien ne se passe, le joueur peut recliquer.
    /// </summary>
    public void PlayRepositionSummonCard(Vector2Int destination)
    {
        if (_selectedCard == null || _summonToMove == null || _playerDeckManager == null) return;

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        if (activeUnit == null) return;

        ValidationResult canPlayResult = GameActionValidator.CanPlayCard(activeUnit, _selectedCard);
        if (!canPlayResult.IsValid)
        {
            GameLog.LogWarning($"❌ Impossible de jouer {_selectedCard.cardName} : {canPlayResult.ErrorMessage}");
            DeselectCard();
            return;
        }

        bool isFree = Services.Grid.GetTileAtPosition(destination) != null && Services.Grid.GetUnitAtGridPos(destination) == null;
        ValidationResult moveResult = GameActionValidator.CanMoveSummonTo(_selectedCard, _summonToMove, destination, isFree);
        if (!moveResult.IsValid)
        {
            GameLog.Log($"❌ {_selectedCard.cardName} : {moveResult.ErrorMessage}");
            return;
        }

        _selectedCard.ExecuteEffect(activeUnit, _summonToMove, destination);
        PayCostsAndEndCardPlay(activeUnit);
        GameLog.Log($"✅ Invocation déplacée avec succès");
    }

    /// <summary>
    /// Fin commune d'une carte jouée : défausse, paiement des PA (coût effectif, overrides compris)
    /// et des PV, puis remise à zéro de la sélection et des affichages de ciblage.
    /// </summary>
    private void PayCostsAndEndCardPlay(Unit activeUnit)
    {
        // Coût effectif (tient compte d'un éventuel override, ex: Triche)
        int effectiveCostPA = _playerDeckManager.GetEffectiveCost(_selectedCard);
        _playerDeckManager.PlayCard(_selectedCard);

        if (effectiveCostPA > 0 && activeUnit is IActionPointsUser paUser)
        {
            paUser.SpendPA(effectiveCostPA);
        }

        if (_selectedCard.costHP > 0)
        {
            activeUnit.PayHealth(_selectedCard.costHP);
        }

        _pendingMultiTargets.Clear();
        _summonToMove = null;
        _selectedCard = null;
        ResetSelectedCardUIPosition();
        ResetCardHighlights();
        EventBus.Publish(new ResetTileColorsEvent());
        EventBus.Publish(new ShowMovementRangeEvent(activeUnit));
    }

    void OnEnable()
    {
        CardUIElement.OnCardClicked += HandleCardClicked; // S'abonner à l'événement de clic sur les cartes
        CardUIElement.OnCardHoverEnter += HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit += HandleCardHoverExit;
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDisable()
    {
        CardUIElement.OnCardClicked -= HandleCardClicked; // Se désabonner pour éviter les fuites de mémoire
        CardUIElement.OnCardHoverEnter -= HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit -= HandleCardHoverExit;
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    }

    private bool _isInitialized = false;
    private Champion _boundChampion; // Champion dont la main est affichée

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
            BindToUnit(Services.Grid.GetActiveUnit());
        }
    }

    /// <summary>
    /// Coop : la main affichée suit le champion dont c'est le tour. Pendant le tour d'un
    /// ennemi, on garde la main du dernier champion.
    /// </summary>
    private void OnTurnChanged(TurnChangedEvent e)
    {
        if (!(e.NewActiveUnit is Champion) || e.NewActiveUnit == _boundChampion) return;

        if (_selectedCard != null) DeselectCard();
        Unbind();
        BindToUnit(e.NewActiveUnit);
    }

    private void BindToUnit(Unit unit)
    {
        // OPTIMISATION Phase 3.3: ComponentLocator
        unit.TryGetComponentSafe(out _playerDeckManager);
        if (_playerDeckManager != null)
        {
            _playerDeckManager.OnHandChanged += UpdateHandUI; // S'abonner à l'événement de changement de main
            UpdateHandUI(); // Mettre à jour l'UI immédiatement après l'abonnement
            GameLog.Log($"HandUIController: main de {unit.name} affichée");
        }
        else
        {
            GameLog.LogWarning("DeckManager introuvable sur l'unité active du joueur.");
            return;
        }

        // S'abonner aux changements de PA si c'est un Champion
        _boundChampion = unit as Champion;
        if (_boundChampion != null)
        {
            _boundChampion.OnActionPointsChanged += HandlePAChanged;
        }

        _isInitialized = true;
    }

    private void Unbind()
    {
        if (_playerDeckManager != null)
        {
            _playerDeckManager.OnHandChanged -= UpdateHandUI; // Se désabonner pour éviter les fuites de mémoire
        }

        if (_boundChampion != null)
        {
            _boundChampion.OnActionPointsChanged -= HandlePAChanged;
        }
    }

    void OnDestroy()
    {
        // Nettoyage des events statiques (CRITIQUE pour éviter fuites mémoire)
        CardUIElement.OnCardClicked -= HandleCardClicked;
        CardUIElement.OnCardHoverEnter -= HandleCardHoverEnter;
        CardUIElement.OnCardHoverExit -= HandleCardHoverExit;

        Unbind();
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
                    GameLog.LogWarning($"Le préfab de carte UI '{_cardUIPrefab.name}' ne contient pas de composant CardUIElement.");
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

        // BUGFIX: si une carte ciblant une unité/tuile était sélectionnée (détachée du
        // HandContainer et attachée au Canvas avec un sorting order élevé) au moment du
        // rafraîchissement, son GameObject UI a été détruit et réinstancié ci-dessus : il a donc
        // perdu ce détachement et s'est retrouvé reparenté dans le HandContainer par
        // ArrangeCardsInArc(). On réapplique le détachement pour que la carte sélectionnée reste
        // visible au-dessus de tout pendant le ciblage.
        if (_selectedCard != null && _selectedCardUIObject != null)
        {
            AttachSelectedCardUIToCanvas();
        }
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
        GameLog.Log($"HandUIController a reçu un clic sur : {clickedCard.cardName}");

        // Cas spécial : une carte "cible une carte de la main" (ex: Triche) est sélectionnée
        // et on clique sur une AUTRE carte -> c'est le ciblage, pas un changement de sélection.
        if (_selectedCard != null && _selectedCard.targetsHandCard && clickedCard != _selectedCard)
        {
            PlayHandCardTargetingCard(clickedCard);
            return;
        }

        // Réinitialiser la position de la carte précédemment sélectionnée si elle suivait la souris
        ResetSelectedCardUIPosition();
        // Désélectionner visuellement toutes les cartes d'abord
        ResetCardHighlights();

        // BUGFIX: la sélection change (désélection ou nouvelle carte) : on doit nettoyer les cibles
        // multi-cibles en attente de la carte PRÉCÉDEMMENT sélectionnée. Sans ça, des cibles restaient
        // accrochées (stuck) et pouvaient être réutilisées à tort pour une autre carte multi-cibles
        // sélectionnée juste après (cf. DeselectCard/CancelTargetingStep qui le font déjà).
        // Même chose pour l'invocation choisie par une carte de déplacement d'invocation.
        _pendingMultiTargets.Clear();
        _summonToMove = null;

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
            GameLog.Log("Carte désélectionnée.");
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

            GameLog.Log($"Carte sélectionnée : {_selectedCard.cardName}");

            // Trouver le GameObject UI correspondant à la carte sélectionnée pour le faire suivre la souris
            foreach (GameObject cardUIObject in _instantiatedCardUIs)
            {
                // OPTIMISATION Phase 3.3: ComponentLocator
                if (cardUIObject.TryGetComponentSafe(out CardUIElement cardUIElement) && cardUIElement.CardData == _selectedCard)
                {
                    _selectedCardUIObject = cardUIObject;
                    AttachSelectedCardUIToCanvas();

                    break;
                }
            }

            // Affiche la zone d'effet pour les cartes AOE Self
            if (activeUnit != null && clickedCard.targetType == CardTargetType.Self && clickedCard.isAOE)
            {
                // Carte AOE Self : afficher la zone AOE autour du joueur
                Vector2Int playerPos = activeUnit.GetCurrentGridPos();
                // OPTIMISATION Phase 3.2: Utilise EventBus au lieu d'appel direct
                EventBus.Publish(new ShowAOEZoneEvent(playerPos, clickedCard.aoeRadius, clickedCard, activeUnit));
            }
            // Note: Toutes les autres cartes affichent déjà leur portée via ShowCardTargets (ligne 160)
            // Aucune carte n'est jouée automatiquement, l'utilisateur doit cliquer pour confirmer
        }
    }

    // Méthode pour jouer la carte actuellement sélectionnée
    public void PlaySelectedCard(Unit targetUnit, Vector2Int targetTile)
    {
        // Validation des préconditions
        if (_selectedCard == null)
        {
            GameLog.LogWarning("Aucune carte sélectionnée.");
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
            GameLog.LogWarning($"❌ Impossible de jouer {_selectedCard.cardName} : {canPlayResult.ErrorMessage}");
            return;
        }

        // Validation centralisée : le ciblage est-il valide ?
        // Cas spécial : les cartes de charge peuvent cibler soit une tuile vide, soit un ennemi
        if (_selectedCard.isChargeCard)
        {
            // Pour les cartes de charge, on valide la position cible (qu'elle vienne d'un ennemi ou d'une tuile)
            Vector2Int chargeTargetPos = targetUnit != null ? targetUnit.GetCurrentGridPos() : targetTile;
            ValidationResult chargeResult = GameActionValidator.CanTargetTile(_selectedCard, activeUnit, chargeTargetPos);
            if (!chargeResult.IsValid)
            {
                GameLog.LogWarning($"❌ Ciblage de charge invalide : {chargeResult.ErrorMessage}");
                return;
            }
        }
        else
        {
            if (_selectedCard.targetsUnit)
            {
                ValidationResult targetResult = GameActionValidator.CanTargetUnit(_selectedCard, activeUnit, targetUnit);
                if (!targetResult.IsValid)
                {
                    GameLog.LogWarning($"❌ Ciblage invalide : {targetResult.ErrorMessage}");
                    return;
                }
            }

            if (_selectedCard.targetsTile)
            {
                ValidationResult tileResult = GameActionValidator.CanTargetTile(_selectedCard, activeUnit, targetTile);
                if (!tileResult.IsValid)
                {
                    GameLog.LogWarning($"❌ Ciblage de tuile invalide : {tileResult.ErrorMessage}");
                    return;
                }
            }
        }

        // Toutes les validations passées, exécuter la carte
        if (_selectedCard.isChargeCard)
        {
            // Carte de charge : le lanceur se déplace vers la cible (case vide ou ennemi)
            // Si targetUnit est défini, utilise sa position comme cible
            Vector2Int chargeTarget = targetUnit != null ? targetUnit.GetCurrentGridPos() : targetTile;
            _selectedCard.ExecuteChargeEffect(activeUnit, chargeTarget);
        }
        else
        {
            _selectedCard.ExecuteEffect(activeUnit, targetUnit, targetTile);
        }

        PayCostsAndEndCardPlay(activeUnit);
        GameLog.Log($"✅ Carte jouée avec succès");
    }

    /// <summary>
    /// Joue une carte qui cible une autre carte de la main (ex: Triche sur "targetCard").
    /// Maintenir Maj pendant le clic augmente le coût de +1 PA au lieu de le réduire de -1 PA.
    /// </summary>
    private void PlayHandCardTargetingCard(CardData targetCard)
    {
        if (_selectedCard == null || _playerDeckManager == null) return;

        Unit activeUnit = Services.Grid?.GetActiveUnit();
        if (activeUnit == null)
        {
            Debug.LogError("Aucune unité active - impossible de jouer la carte.");
            return;
        }

        ValidationResult canPlayResult = GameActionValidator.CanPlayCard(activeUnit, _selectedCard);
        if (!canPlayResult.IsValid)
        {
            GameLog.LogWarning($"❌ Impossible de jouer {_selectedCard.cardName} : {canPlayResult.ErrorMessage}");
            return;
        }

        int delta = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ? 1 : -1;
        _playerDeckManager.ModifyCardCost(targetCard, delta);
        GameLog.Log($"🎭 {_selectedCard.cardName} : coût de {targetCard.cardName} modifié de {(delta > 0 ? "+" : "")}{delta} PA (maintenir Maj = +1, sinon -1).");

        int effectiveCostPA = _playerDeckManager.GetEffectiveCost(_selectedCard);

        _playerDeckManager.PlayCard(_selectedCard);

        if (effectiveCostPA > 0 && activeUnit is IActionPointsUser paUser)
        {
            paUser.SpendPA(effectiveCostPA);
        }

        if (_selectedCard.costHP > 0)
        {
            activeUnit.PayHealth(_selectedCard.costHP);
        }

        _selectedCard = null;
        ResetSelectedCardUIPosition();
        ResetCardHighlights();
        RefreshCardAffordability(); // le coût de targetCard a changé, rafraîchit son affichage en main
        EventBus.Publish(new ShowMovementRangeEvent(activeUnit));

        GameLog.Log($"✅ Carte jouée avec succès");
    }

    /// <summary>
    /// Détache le GameObject UI de la carte actuellement sélectionnée (_selectedCardUIObject) du
    /// HandContainer pour l'attacher directement au Canvas, au-dessus de tout (sorting order élevé),
    /// en mode ciblage (carte qui suit la souris / preview à gauche de l'écran). Ne fait rien si
    /// aucune carte UI n'est assignée.
    /// Doit être réappliqué après toute réinstanciation des éléments de carte (ex: UpdateHandUI
    /// suite à un tirage pendant le ciblage) : le nouveau GameObject est recréé dans le
    /// HandContainer et perd ce détachement/z-order tant qu'on ne le réapplique pas.
    /// </summary>
    private void AttachSelectedCardUIToCanvas()
    {
        if (_selectedCardUIObject == null) return;

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
        CardData card = cardUIElement.CardData;

        bool canAfford = true;

        // Coût effectif (tient compte d'un éventuel override, ex: Triche)
        int effectiveCostPA = _playerDeckManager != null ? _playerDeckManager.GetEffectiveCost(card) : card.costPA;
        cardUIElement.RefreshCost(effectiveCostPA);

        // Vérification des PA
        if (effectiveCostPA > 0)
        {
            if (activeUnit is IActionPointsUser paUser)
            {
                canAfford = paUser.GetCurrentPA() >= effectiveCostPA;
            }
            else
            {
                canAfford = false; // Unité sans PA : ne peut pas jouer de carte à coût PA
            }
        }

        // Vérification des PV (Coût HP)
        if (canAfford && card.costHP > 0)
        {
            if (activeUnit.GetHealth() < card.costHP)
            {
                canAfford = false;
            }
        }

        // Carte de déplacement d'invocation sans invocation sur le terrain : injouable
        if (canAfford && card.isRepositionSummonCard && !GameActionValidator.HasSummonToMove(activeUnit))
        {
            canAfford = false;
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