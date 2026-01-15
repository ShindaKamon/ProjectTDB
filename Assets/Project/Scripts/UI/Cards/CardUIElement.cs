using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class CardUIElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private Image _cardIllustrationImage; // Optionnel
    [SerializeField] private GameObject _selectionHighlight; // Surlignage visuel pour la sélection
    [SerializeField] private CanvasGroup _canvasGroup; // Pour griser la carte
    [SerializeField] private Image _cardBackground; // Fond de la carte (pour color tint)

    [Header("Visual Feedback Settings (Phase 4.3)")]
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] private float _selectScale = 1.05f;
    [SerializeField] private float _scaleSpeed = 10f;
    [SerializeField] private float _hoverRotation = 3f; // Degrés de rotation au hover
    [SerializeField] private Color _hoverTint = new Color(1.2f, 1.2f, 1.2f, 1f); // Tint plus clair
    [SerializeField] private Color _selectTint = new Color(0.8f, 1f, 0.8f, 1f); // Vert clair
    [SerializeField] private bool _enableHoverAnimation = true;
    [SerializeField] private bool _enableSelectionAnimation = true;

    [Header("Glow Effect")]
    [SerializeField] private Image _glowImage; // Image de glow autour de la carte
    [SerializeField] private Color _glowColor = new Color(1f, 1f, 0.5f, 0.8f);
    [SerializeField] private float _glowPulseSpeed = 2f;

    private CardData _cardData;
    private bool _isFollowingMouse = false; // Indique si la carte suit la souris
    private Vector3 _originalPosition; // Position d'origine de la carte
    private Vector3 _originalScale; // Échelle d'origine
    private Quaternion _originalRotation; // Rotation d'origine
    private bool _isAffordable = true; // Indique si la carte peut être jouée
    private bool _isHovered = false;
    private bool _isSelected = false;

    private Vector3 _targetScale;
    private Quaternion _targetRotation;
    private Color _targetTint;
    private Coroutine _glowPulseCoroutine;

    public CardData CardData => _cardData; // Propriété publique pour accéder aux données de la carte

    // Événement statique pour notifier quand une carte est cliquée
    public static event System.Action<CardData> OnCardClicked;

    // Événements pour le hover
    public static event System.Action<GameObject> OnCardHoverEnter;
    public static event System.Action<GameObject> OnCardHoverExit;

    void Awake()
    {
        // S'assurer que le surlignage est désactivé au démarrage
        SetSelected(false);
        _originalPosition = transform.position; // Enregistrer la position initiale
        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;

        _targetScale = _originalScale;
        _targetRotation = _originalRotation;
        _targetTint = Color.white;

        // Cache le glow par défaut
        if (_glowImage != null)
        {
            _glowImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_isFollowingMouse)
        {
            // La carte suit la position de la souris
            if (Mouse.current != null)
            {
                transform.position = Mouse.current.position.ReadValue();
            }
        }

        // Animation smooth du scale et rotation (Phase 4.3)
        if (_enableHoverAnimation || _enableSelectionAnimation)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * _scaleSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRotation, Time.deltaTime * _scaleSpeed);

            // Applique le tint au background si présent
            if (_cardBackground != null)
            {
                _cardBackground.color = Color.Lerp(_cardBackground.color, _targetTint, Time.deltaTime * _scaleSpeed);
            }
        }
    }

    public void SetCardData(CardData data)
    {
        _cardData = data;
        if (_cardData != null)
        {
            if (_cardNameText != null) _cardNameText.text = _cardData.cardName;
            if (_cardDescriptionText != null) _cardDescriptionText.text = _cardData.description;

            // Afficher le coût en PA
            if (_cardCostText != null)
            {
                _cardCostText.text = _cardData.costPA.ToString();
            }

            // Afficher le background (A MODIFIER ENSUITE)
            if (_cardBackground != null) _cardBackground.sprite = _cardData.artwork;
            // Mettre à jour l'illustration si vous en avez une
            // if (_cardIllustrationImage != null) _cardIllustrationImage.sprite = _cardData.illustration;
        }
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (_selectionHighlight != null)
        {
            _selectionHighlight.SetActive(isSelected);
        }

        // Phase 4.3: Feedbacks visuels pour la sélection
        if (_enableSelectionAnimation)
        {
            if (isSelected)
            {
                // Sélectionnée : légère augmentation de taille + tint vert
                _targetScale = _originalScale * _selectScale;
                _targetTint = _selectTint;

                // Active le glow
                if (_glowImage != null)
                {
                    _glowImage.gameObject.SetActive(true);
                    _glowImage.color = _glowColor;
                    _glowPulseCoroutine = StartCoroutine(PulseGlow());
                }
            }
            else
            {
                // Désélectionnée : retour à la normale
                if (!_isHovered)
                {
                    _targetScale = _originalScale;
                    _targetRotation = _originalRotation;
                    _targetTint = Color.white;
                }

                // Désactive le glow
                if (_glowImage != null)
                {
                    _glowImage.gameObject.SetActive(false);
                    if (_glowPulseCoroutine != null)
                    {
                        StopCoroutine(_glowPulseCoroutine);
                        _glowPulseCoroutine = null;
                    }
                }
            }
        }
    }

    public void SetFollowMouse(bool follow)
    {
        _isFollowingMouse = follow;
        if (!follow) {
            transform.position = _originalPosition; // Restaurer la position originale quand on arrête de suivre la souris
        }
    }

    /// <summary>
    /// Définit si la carte peut être jouée (assez de PA)
    /// </summary>
    public void SetAffordable(bool affordable)
    {
        _isAffordable = affordable;

        // Si on a un CanvasGroup, on ajuste l'opacité et l'interactivité
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = affordable ? 1f : 0.5f; // Grisé à 50% si pas assez de PA
            _canvasGroup.interactable = affordable; // Non cliquable si pas assez de PA
        }

        // Change la couleur du texte de coût pour indiquer qu'on ne peut pas jouer la carte
        if (_cardCostText != null)
        {
            _cardCostText.color = affordable ? Color.white : Color.red;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Vérifier si le clic est sur le bouton gauche de la souris
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Ne pas permettre de cliquer pendant le tour ennemi
            TurnStateMachine turnMachine = Services.Grid?.GetTurnStateMachine();
            if (turnMachine != null && !turnMachine.CanPlayerAct())
            {
                Debug.LogWarning("Ce n'est pas votre tour !");
                return;
            }

            // Ne pas permettre de cliquer si la carte n'est pas jouable
            if (!_isAffordable)
            {
                Debug.LogWarning($"Pas assez de PA pour jouer {_cardData.cardName}");
                return;
            }

            Debug.Log($"Carte cliquée : {_cardData.cardName}");
            OnCardClicked?.Invoke(_cardData); // Déclencher l'événement avec les données de la carte
        }
    }

    // ========== HOVER EVENTS (Phase 4.3) ==========

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_enableHoverAnimation || _isFollowingMouse) return;

        _isHovered = true;

        // Toujours notifier le HandUIController (pour monter la carte au premier plan)
        OnCardHoverEnter?.Invoke(gameObject);

        // Ne pas animer visuellement si la carte n'est pas jouable
        if (!_isAffordable) return;

        // Hover : augmentation de taille + rotation + tint
        _targetScale = _originalScale * _hoverScale;
        _targetRotation = _originalRotation * Quaternion.Euler(0, 0, _hoverRotation);
        _targetTint = _hoverTint;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_enableHoverAnimation || _isFollowingMouse) return;

        _isHovered = false;

        // Notifier le HandUIController
        OnCardHoverExit?.Invoke(gameObject);

        // Retour à la normale (ou à l'état sélectionné si sélectionnée)
        if (_isSelected)
        {
            _targetScale = _originalScale * _selectScale;
            _targetRotation = _originalRotation;
            _targetTint = _selectTint;
        }
        else
        {
            _targetScale = _originalScale;
            _targetRotation = _originalRotation;
            _targetTint = Color.white;
        }
    }

    // ========== GLOW ANIMATION (Phase 4.3) ==========

    /// <summary>
    /// Animation de pulsation du glow
    /// </summary>
    private IEnumerator PulseGlow()
    {
        if (_glowImage == null) yield break;

        Color baseColor = _glowColor;
        Color dimColor = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * 0.5f);

        while (true)
        {
            // Pulse entre baseColor et dimColor
            float t = Mathf.PingPong(Time.time * _glowPulseSpeed, 1f);
            _glowImage.color = Color.Lerp(dimColor, baseColor, t);
            yield return null;
        }
    }

    // ========== CLEANUP ==========

    void OnDisable()
    {
        // CRITIQUE: Arrêter la coroutine infinie quand la carte est désactivée
        if (_glowPulseCoroutine != null)
        {
            StopCoroutine(_glowPulseCoroutine);
            _glowPulseCoroutine = null;
        }
    }

    void OnDestroy()
    {
        // CRITIQUE: Arrêter la coroutine infinie avant destruction
        if (_glowPulseCoroutine != null)
        {
            StopCoroutine(_glowPulseCoroutine);
            _glowPulseCoroutine = null;
        }
    }
}