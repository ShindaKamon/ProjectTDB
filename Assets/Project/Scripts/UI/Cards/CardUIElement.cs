using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardUIElement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private Image _cardIllustrationImage; // Optionnel
    [SerializeField] private GameObject _selectionHighlight; // Surlignage visuel pour la sélection
    [SerializeField] private CanvasGroup _canvasGroup; // Pour griser la carte

    private CardData _cardData;
    private bool _isFollowingMouse = false; // Indique si la carte suit la souris
    private Vector3 _originalPosition; // Position d'origine de la carte
    private bool _isAffordable = true; // Indique si la carte peut être jouée

    public CardData CardData => _cardData; // Propriété publique pour accéder aux données de la carte

    // Événement statique pour notifier quand une carte est cliquée
    public static event System.Action<CardData> OnCardClicked;

    void Awake()
    {
        // S'assurer que le surlignage est désactivé au démarrage
        SetSelected(false);
        _originalPosition = transform.position; // Enregistrer la position initiale
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
                _cardCostText.text = _cardData.costPA > 0 ? $"{_cardData.costPA} PA" : "0";
            }

            // Mettre à jour l'illustration si vous en avez une
            // if (_cardIllustrationImage != null) _cardIllustrationImage.sprite = _cardData.illustration;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectionHighlight != null)
        {
            _selectionHighlight.SetActive(isSelected);
        }
        // TODO: Ajouter d'autres feedbacks visuels ici (agrandissement, changement de couleur, etc.)
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
}