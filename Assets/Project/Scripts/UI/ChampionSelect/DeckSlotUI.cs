using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Composant UI pour un slot de deck visuel
/// Affiche une carte colorée représentant le deck
/// </summary>
public class DeckSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Références UI")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _selectionBorder;
    [SerializeField] private TextMeshProUGUI _deckNameText;
    [SerializeField] private TextMeshProUGUI _cardCountText;

    [Header("Couleurs")]
    [SerializeField] private Color _selectedBorderColor = new Color(1f, 0.84f, 0f); // Or
    [SerializeField] private Color _normalBorderColor = new Color(0.3f, 0.3f, 0.3f);

    private DeckData _deckData;
    private int _deckIndex;
    private bool _isSelected;

    public System.Action<DeckSlotUI, int> OnSlotClicked;

    public DeckData DeckData => _deckData;
    public int DeckIndex => _deckIndex;
    public bool IsSelected => _isSelected;

    /// <summary>
    /// Initialise le slot avec les données du deck
    /// </summary>
    public void Setup(DeckData deckData, int index)
    {
        _deckData = deckData;
        _deckIndex = index;

        UpdateDisplay();
    }

    /// <summary>
    /// Met à jour l'affichage du slot
    /// </summary>
    public void UpdateDisplay()
    {
        if (_deckData == null) return;

        // Nom du deck
        if (_deckNameText != null)
            _deckNameText.text = _deckData.deckName;

        // Nombre de cartes
        if (_cardCountText != null)
            _cardCountText.text = $"{_deckData.cardNames.Count} cartes";

        // Couleur de fond
        if (_backgroundImage != null)
        {
            if (ColorUtility.TryParseHtmlString(_deckData.deckColor, out Color color))
                _backgroundImage.color = color;
        }

        UpdateSelectionVisual();
    }

    /// <summary>
    /// Définit l'état de sélection du slot
    /// </summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (_selectionBorder != null)
        {
            _selectionBorder.color = _isSelected ? _selectedBorderColor : _normalBorderColor;
            _selectionBorder.gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this, _deckIndex);
    }
}
