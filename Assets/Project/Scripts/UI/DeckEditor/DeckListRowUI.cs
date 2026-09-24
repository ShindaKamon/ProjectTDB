using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Ligne de la liste du deck (façon MTG Arena) : bande de couleur de l'émotion, coût PA, nom
/// et quantité. Un clic retire un exemplaire de la carte (désactivable via SetInteractable,
/// utilisé pour le deck de base en lecture seule).
/// </summary>
public class DeckListRowUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _emotionStrip;     // Bande verticale à gauche : couleur de l'émotion
    [SerializeField] private Image _costBadge;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Couleurs")]
    [SerializeField] private Color _normalColor = CodexCardVisual.CardBackground;   // palette du codex émotionnel
    [SerializeField] private Color _hoverColor = CodexCardVisual.CardBorder;

    private CardData _cardData;
    private int _count;
    private bool _interactable = true;

    public CardData CardData => _cardData;
    public int Count => _count;

    /// <summary>Émis au clic (si interactable) pour retirer un exemplaire de cette carte.</summary>
    public System.Action<CardData> OnCardClicked;

    public void Setup(CardData card, int count)
    {
        _cardData = card;
        _count = count;
        SetInteractable(true);

        if (_nameText != null)
            _nameText.text = card.cardName;

        if (_countText != null)
            _countText.text = $"×{count}";

        if (_costText != null)
            _costText.text = card.costPA.ToString();

        if (_costBadge != null)
            _costBadge.color = CardVisualHelper.GetCostColor(card.costPA);

        if (_emotionStrip != null)
            _emotionStrip.color = CodexCardVisual.EmotionColor(card.emotionType); // Signatures : gris « Neutre »

        if (_background != null)
            _background.color = _normalColor;
    }

    /// <summary>Active/désactive le retrait par clic ; la ligne est estompée quand désactivée.</summary>
    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
        if (_canvasGroup != null)
            _canvasGroup.alpha = interactable ? 1f : 0.6f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;
        OnCardClicked?.Invoke(_cardData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_interactable && _background != null)
            _background.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_background != null)
            _background.color = _normalColor;
    }
}
