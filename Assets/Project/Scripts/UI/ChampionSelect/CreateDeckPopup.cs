using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup pour créer un nouveau deck
/// Permet de choisir un nom et une couleur
/// </summary>
public class CreateDeckPopup : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Transform _colorButtonsParent;

    [Header("Couleurs disponibles")]
    [SerializeField] private string[] _availableColors = new string[]
    {
        "#3498DB", // Bleu
        "#2ECC71", // Vert
        "#9B59B6", // Violet
        "#E67E22", // Orange
        "#1ABC9C", // Turquoise
        "#E91E63"  // Rose
    };

    [Header("Configuration")]
    [SerializeField] private GameObject _colorButtonPrefab;

    private string _selectedColor;
    private Button _selectedColorButton;
    private bool _isInitialized = false;

    public System.Action<string, string> OnDeckCreated; // (nom, couleur)

    void Awake()
    {
        if (_createButton != null)
            _createButton.onClick.AddListener(OnCreatePressed);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(Hide);

        if (_nameInput != null)
            _nameInput.onValueChanged.AddListener(OnNameChanged);

        CreateColorButtons();
        _isInitialized = true;

        // Ne pas appeler Hide() ici - le popup doit être désactivé dans la scène
    }

    private void CreateColorButtons()
    {
        if (_colorButtonsParent == null || _colorButtonPrefab == null) return;

        foreach (var hexColor in _availableColors)
        {
            var buttonGO = Instantiate(_colorButtonPrefab, _colorButtonsParent);
            var button = buttonGO.GetComponent<Button>();
            var image = buttonGO.GetComponent<Image>();

            if (image != null && ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                image.color = color;
            }

            if (button != null)
            {
                string colorCopy = hexColor; // Capture pour le closure
                button.onClick.AddListener(() => SelectColor(colorCopy, button));
            }
        }
    }

    private void SelectColor(string hexColor, Button button)
    {
        _selectedColor = hexColor;

        // Désélectionner l'ancien bouton
        if (_selectedColorButton != null)
        {
            var oldOutline = _selectedColorButton.GetComponent<Outline>();
            if (oldOutline != null) oldOutline.enabled = false;
        }

        // Sélectionner le nouveau
        _selectedColorButton = button;
        var outline = button.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, 3);
        }

        UpdateCreateButtonState();
    }

    private void OnNameChanged(string value)
    {
        UpdateCreateButtonState();
    }

    private void UpdateCreateButtonState()
    {
        if (_createButton != null)
        {
            bool isValid = !string.IsNullOrWhiteSpace(_nameInput?.text) && !string.IsNullOrEmpty(_selectedColor);
            _createButton.interactable = isValid;
        }
    }

    public void Show()
    {
        Debug.Log($"CreateDeckPopup.Show() appelé. GameObject actuel: {gameObject.name}");

        // Vérifier si un parent est désactivé
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                Debug.LogError($"Parent désactivé trouvé: {parent.name}. Activation du parent.");
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }

        gameObject.SetActive(true);
        Debug.Log($"CreateDeckPopup activeSelf: {gameObject.activeSelf}, activeInHierarchy: {gameObject.activeInHierarchy}");

        // Réinitialiser
        if (_nameInput != null)
            _nameInput.text = "";

        _selectedColor = null;
        _selectedColorButton = null;

        // Désélectionner tous les boutons couleur
        if (_colorButtonsParent != null)
        {
            foreach (Transform child in _colorButtonsParent)
            {
                var outline = child.GetComponent<Outline>();
                if (outline != null) outline.enabled = false;
            }
        }

        UpdateCreateButtonState();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnCreatePressed()
    {
        if (string.IsNullOrWhiteSpace(_nameInput?.text) || string.IsNullOrEmpty(_selectedColor))
            return;

        OnDeckCreated?.Invoke(_nameInput.text.Trim(), _selectedColor);
        Hide();
    }
}
