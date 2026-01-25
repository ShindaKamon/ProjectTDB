using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Élément UI représentant une carte dans le pool de cartes disponibles
/// </summary>
public class CardPoolItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Références UI")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _cardIcon;
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;

    [Header("Couleurs")]
    [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _hoverColor = new Color(0.3f, 0.3f, 0.3f);

    private CardData _cardData;

    public System.Action<CardData> OnCardClicked;
    public CardData CardData => _cardData;

    /// <summary>
    /// Initialise l'élément avec les données de la carte
    /// </summary>
    public void Setup(CardData card)
    {
        _cardData = card;

        if (_cardNameText != null)
            _cardNameText.text = card.cardName;

        if (_cardCostText != null)
            _cardCostText.text = $"{card.costPA} PA";

        if (_cardDescriptionText != null)
            _cardDescriptionText.text = card.description;

        if (_cardIcon != null && card.artwork != null)
            _cardIcon.sprite = card.artwork;

        if (_backgroundImage != null)
            _backgroundImage.color = _normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(_cardData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _normalColor;
    }
}
