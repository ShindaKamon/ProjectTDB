using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Ligne de la liste du deck (façon MTG Arena) : coût PA dans un rond à la couleur de l'émotion
/// (blanc pour une Signature), nom et quantité. Un clic retire un exemplaire de la carte, sauf si
/// la ligne est en lecture seule (deck de base, SetInteractable) ou verrouillée (Signature
/// obligatoire, SetLocked).
/// </summary>
public class DeckListRowUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _background;
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
    private bool _locked;

    public CardData CardData => _cardData;
    public int Count => _count;

    /// <summary>Émis au clic (si la ligne peut être retirée) pour retirer un exemplaire de la carte.</summary>
    public System.Action<CardData> OnCardClicked;

    private bool CanRemove => _interactable && !_locked;

    public void Setup(CardData card, int count)
    {
        _cardData = card;
        _count = count;
        _locked = false;
        SetInteractable(true);

        if (_nameText != null)
            _nameText.text = card.cardName;

        if (_countText != null)
            _countText.text = $"×{count}";

        Color costColor = CodexCardVisual.CostColor(card);
        if (_costBadge != null)
            _costBadge.color = costColor;

        if (_costText != null)
        {
            _costText.text = card.costPA.ToString();
            _costText.color = CodexCardVisual.ReadableTextOn(costColor);
        }

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

    /// <summary>Carte obligatoire (Signature du champion) : affichée normalement, mais non retirable.</summary>
    public void SetLocked(bool locked)
    {
        _locked = locked;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanRemove) return;
        OnCardClicked?.Invoke(_cardData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanRemove && _background != null)
            _background.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_background != null)
            _background.color = _normalColor;
    }
}
