using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Menu contextuel qui apparaît au clic sur un deck
/// Propose les options: Sélectionner, Modifier, Renommer, Dupliquer, Supprimer
/// </summary>
public class DeckContextMenu : MonoBehaviour
{
    [Header("Boutons")]
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _editButton;
    [SerializeField] private Button _renameButton;
    [SerializeField] private Button _duplicateButton;
    [SerializeField] private Button _deleteButton;

    [Header("Configuration")]
    [SerializeField] private CanvasGroup _canvasGroup;

    private DeckSlotUI _targetSlot;
    private bool _isOpen;

    // Events
    public System.Action<int> OnSelectClicked;
    public System.Action<int> OnEditClicked;
    public System.Action<int> OnRenameClicked;
    public System.Action<int> OnDuplicateClicked;
    public System.Action<int> OnDeleteClicked;

    void Awake()
    {
        if (_selectButton != null)
            _selectButton.onClick.AddListener(OnSelectPressed);
        if (_editButton != null)
            _editButton.onClick.AddListener(OnEditPressed);
        if (_renameButton != null)
            _renameButton.onClick.AddListener(OnRenamePressed);
        if (_duplicateButton != null)
            _duplicateButton.onClick.AddListener(OnDuplicatePressed);
        if (_deleteButton != null)
            _deleteButton.onClick.AddListener(OnDeletePressed);

        Hide();
    }

    void Update()
    {
        // Fermer le menu si on clique ailleurs
        if (_isOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverMenu())
            {
                Hide();
            }
        }
    }

    /// <summary>
    /// Affiche le menu à la position du slot
    /// </summary>
    public void Show(DeckSlotUI slot, bool canDuplicate)
    {
        _targetSlot = slot;
        _isOpen = true;

        // Positionner le menu près du slot
        transform.position = slot.transform.position + new Vector3(80, -50, 0);

        // Activer/désactiver le bouton dupliquer selon la limite
        if (_duplicateButton != null)
            _duplicateButton.interactable = canDuplicate;

        // Le deck de base ne peut pas être supprimé
        if (_deleteButton != null)
            _deleteButton.interactable = !slot.DeckData.isDefault;

        gameObject.SetActive(true);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Cache le menu
    /// </summary>
    public void Hide()
    {
        _isOpen = false;
        _targetSlot = null;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    private bool IsPointerOverMenu()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        var pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject.transform.IsChildOf(transform) || result.gameObject == gameObject)
                return true;
        }

        return false;
    }

    private void OnSelectPressed()
    {
        if (_targetSlot != null)
            OnSelectClicked?.Invoke(_targetSlot.DeckIndex);
        Hide();
    }

    private void OnEditPressed()
    {
        if (_targetSlot != null)
            OnEditClicked?.Invoke(_targetSlot.DeckIndex);
        Hide();
    }

    private void OnRenamePressed()
    {
        if (_targetSlot != null)
            OnRenameClicked?.Invoke(_targetSlot.DeckIndex);
        Hide();
    }

    private void OnDuplicatePressed()
    {
        if (_targetSlot != null)
            OnDuplicateClicked?.Invoke(_targetSlot.DeckIndex);
        Hide();
    }

    private void OnDeletePressed()
    {
        if (_targetSlot != null)
            OnDeleteClicked?.Invoke(_targetSlot.DeckIndex);
        Hide();
    }
}
