using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Affiche une carte dans la grille de deck avec sa quantite (affichage groupé, façon MTG
/// Arena). Composant unique pour l'aperçu et l'édition : un clic retire un exemplaire de la
/// carte (désactivable via SetInteractable, utilisé pour le deck de base en lecture seule).
/// </summary>
public class DeckGridCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Structure principale")]
    [SerializeField] private Image _cardFrame;        // Cadre/fond de la carte
    [SerializeField] private Image _cardImage;        // Artwork de la carte
    [SerializeField] private Image _familyBorder;     // Bordure colorée selon l'émotion

    [Header("Badges")]
    [SerializeField] private Image _costBadge;        // Fond du coût (coin haut gauche)
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _countBadge;       // Fond de la quantité (coin bas droite)
    [SerializeField] private TextMeshProUGUI _countText;

    [Header("Informations")]
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private Image _nameBackground;   // Fond semi-transparent pour le nom

    [Header("Effets visuels")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _hoverDuration = 0.1f;

    private CardData _cardData;
    private int _count;
    private bool _interactable = true;
    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    public CardData CardData => _cardData;
    public int Count => _count;

    /// <summary>Émis au clic (si interactable) pour retirer un exemplaire de cette carte.</summary>
    public System.Action<CardData> OnCardClicked;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;
    }

    /// <summary>
    /// Active/désactive le retrait par clic (deck de base en lecture seule : désactivé).
    /// Assombrit légèrement la carte quand non interactable.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = interactable ? 1f : 0.6f;
        }
        else if (_cardImage != null)
        {
            var color = _cardImage.color;
            color.a = interactable ? 1f : 0.6f;
            _cardImage.color = color;
        }
    }

    /// <summary>
    /// Configure l'affichage de la carte avec sa quantite
    /// </summary>
    public void Setup(CardData card, int count)
    {
        _cardData = card;
        _count = count;
        SetInteractable(true);

        // Artwork
        if (_cardImage != null)
        {
            if (card.artwork != null)
            {
                _cardImage.sprite = card.artwork;
                _cardImage.color = Color.white;
            }
            else
            {
                // Fallback: afficher une couleur basée sur l'émotion
                _cardImage.sprite = null;
                _cardImage.color = CardVisualHelper.GetEmotionColor(card.emotionType);
            }
        }

        // Bordure colorée selon l'émotion
        if (_familyBorder != null)
        {
            _familyBorder.color = CardVisualHelper.GetEmotionColor(card.emotionType);
        }

        // Nom de la carte
        if (_cardNameText != null)
            _cardNameText.text = card.cardName;

        // Badge de quantité (visible seulement si > 1)
        if (_countText != null)
            _countText.text = $"x{count}";

        if (_countBadge != null)
            _countBadge.gameObject.SetActive(count > 1);

        // Badge de coût PA
        if (_costText != null)
            _costText.text = card.costPA.ToString();

        // Couleur du badge de coût selon le coût
        if (_costBadge != null)
        {
            _costBadge.color = CardVisualHelper.GetCostColor(card.costPA);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;
        OnCardClicked?.Invoke(_cardData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale * _hoverScale, _hoverDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale, _hoverDuration));
        }
    }
}
