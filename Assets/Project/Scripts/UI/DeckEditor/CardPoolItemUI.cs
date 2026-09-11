using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Element UI representant une carte dans le pool de cartes disponibles
/// Style moderne avec animations et couleur de famille
/// </summary>
public class CardPoolItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Structure")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _familyBorder;        // Bordure coloree selon famille
    [SerializeField] private Image _cardIcon;

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;

    [Header("Badges")]
    [SerializeField] private Image _costBadge;           // Fond du cout

    [Header("Couleurs")]
    [SerializeField] private Color _normalColor = new Color(0.15f, 0.15f, 0.2f);
    [SerializeField] private Color _hoverColor = new Color(0.25f, 0.25f, 0.3f);
    [SerializeField] private Color _clickColor = new Color(0.35f, 0.35f, 0.4f);

    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.02f;
    [SerializeField] private float _animDuration = 0.1f;

    private CardData _cardData;
    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    public System.Action<CardData> OnCardClicked;
    public CardData CardData => _cardData;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
            _originalScale = _rectTransform.localScale;
    }

    /// <summary>
    /// Initialise l'element avec les donnees de la carte
    /// </summary>
    public void Setup(CardData card)
    {
        _cardData = card;

        // Stoppe une éventuelle animation de hover en cours et réinitialise l'échelle
        // (l'item peut être recyclé par le pool pendant qu'il était survolé/agrandi)
        StopAllCoroutines();
        if (_rectTransform != null)
            _rectTransform.localScale = _originalScale;

        // Nom
        if (_cardNameText != null)
            _cardNameText.text = card.cardName;

        // Cout PA
        if (_cardCostText != null)
            _cardCostText.text = card.costPA.ToString();

        // Badge de cout avec couleur
        if (_costBadge != null)
            _costBadge.color = CardVisualHelper.GetCostColor(card.costPA);

        // Description
        if (_cardDescriptionText != null)
            _cardDescriptionText.text = card.description;

        // Artwork
        if (_cardIcon != null)
        {
            if (card.artwork != null)
            {
                _cardIcon.sprite = card.artwork;
                _cardIcon.color = Color.white;
            }
            else
            {
                _cardIcon.sprite = null;
                _cardIcon.color = CardVisualHelper.GetEmotionColor(card.emotionType);
            }
        }

        // Bordure d'émotion
        if (_familyBorder != null)
            _familyBorder.color = CardVisualHelper.GetEmotionColor(card.emotionType);

        // Fond
        if (_backgroundImage != null)
            _backgroundImage.color = _normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Effet visuel de clic
        if (_backgroundImage != null)
            _backgroundImage.color = _clickColor;

        OnCardClicked?.Invoke(_cardData);

        // Retour a la couleur hover apres le clic
        StartCoroutine(ResetToHoverAfterClick());
    }

    private System.Collections.IEnumerator ResetToHoverAfterClick()
    {
        yield return new WaitForSeconds(0.1f);
        if (_backgroundImage != null)
            _backgroundImage.color = _hoverColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _hoverColor;

        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale * _hoverScale, _animDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _normalColor;

        if (_rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIHoverAnimator.ScaleTo(_rectTransform, _originalScale, _animDuration));
        }
    }
}
