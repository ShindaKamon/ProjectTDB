using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup pour créer un nouveau deck
/// Permet de choisir un nom et deux émotions (couleurs)
/// </summary>
public class CreateDeckPopup : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Transform _colorButtonsParent;

    [Header("Configuration")]
    [SerializeField] private GameObject _colorButtonPrefab;
    private const int MAX_EMOTIONS = 2;

    private List<EmotionType> _selectedEmotions = new List<EmotionType>();
    private List<Button> _selectedEmotionButtons = new List<Button>();
    private Dictionary<Button, EmotionType> _buttonToEmotion = new Dictionary<Button, EmotionType>();

    public System.Action<string, EmotionType, EmotionType> OnDeckCreated; // (nom, emotion1, emotion2)

    void Awake()
    {
        if (_createButton != null)
            _createButton.onClick.AddListener(OnCreatePressed);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(Hide);

        if (_nameInput != null)
            _nameInput.onValueChanged.AddListener(OnNameChanged);

        CreateEmotionButtons();

        // Ne pas appeler Hide() ici - le popup doit être désactivé dans la scène
    }

    private void CreateEmotionButtons()
    {
        if (_colorButtonsParent == null || _colorButtonPrefab == null) return;

        // Créer un bouton pour chaque émotion (sauf None)
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            if (emotion == EmotionType.None) continue;

            var buttonGO = Instantiate(_colorButtonPrefab, _colorButtonsParent);
            var button = buttonGO.GetComponent<Button>();
            var image = buttonGO.GetComponent<Image>();

            // Appliquer la couleur de l'émotion
            if (image != null)
            {
                image.color = CardVisualHelper.GetEmotionColor(emotion);
            }

            // Ajouter un texte avec le nom de l'émotion si possible
            var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = CardVisualHelper.GetEmotionName(emotion);
            }

            if (button != null)
            {
                _buttonToEmotion[button] = emotion;
                EmotionType capturedEmotion = emotion;
                button.onClick.AddListener(() => SelectEmotion(capturedEmotion, button));
            }
        }
    }

    private void SelectEmotion(EmotionType emotion, Button button)
    {
        int existingIndex = _selectedEmotions.IndexOf(emotion);

        // Si l'émotion est déjà sélectionnée, la désélectionner
        if (existingIndex >= 0)
        {
            _selectedEmotions.RemoveAt(existingIndex);
            _selectedEmotionButtons.RemoveAt(existingIndex);

            var outline = button.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }
        else
        {
            // Si on a déjà 2 émotions, retirer la première
            if (_selectedEmotions.Count >= MAX_EMOTIONS)
            {
                var oldButton = _selectedEmotionButtons[0];
                var oldOutline = oldButton.GetComponent<Outline>();
                if (oldOutline != null) oldOutline.enabled = false;

                _selectedEmotions.RemoveAt(0);
                _selectedEmotionButtons.RemoveAt(0);
            }

            // Ajouter la nouvelle émotion
            _selectedEmotions.Add(emotion);
            _selectedEmotionButtons.Add(button);

            var outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                outline.effectColor = Color.white;
                outline.effectDistance = new Vector2(3, 3);
            }
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
            // Un deck peut être créé avec 1 OU 2 émotions. Depuis le 24/09/2026 elles ne limitent plus
            // les cartes (toutes les émotions sont autorisées, voir DeckRules) : elles ne servent qu'à
            // la couleur de l'onglet du deck.
            bool isValid = !string.IsNullOrWhiteSpace(_nameInput?.text) &&
                           _selectedEmotions.Count >= 1 && _selectedEmotions.Count <= MAX_EMOTIONS;
            _createButton.interactable = isValid;
        }
    }

    public void Show()
    {
        GameLog.Log($"CreateDeckPopup.Show() appelé. GameObject actuel: {gameObject.name}");

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
        GameLog.Log($"CreateDeckPopup activeSelf: {gameObject.activeSelf}, activeInHierarchy: {gameObject.activeInHierarchy}");

        // Réinitialiser
        if (_nameInput != null)
        {
            _nameInput.text = "";
            _nameInput.ActivateInputField();
            _nameInput.Select();
        }

        _selectedEmotions.Clear();
        _selectedEmotionButtons.Clear();

        // Désélectionner tous les boutons émotion
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
        if (string.IsNullOrWhiteSpace(_nameInput?.text) ||
            _selectedEmotions.Count < 1 || _selectedEmotions.Count > MAX_EMOTIONS)
            return;

        EmotionType emotion2 = _selectedEmotions.Count > 1 ? _selectedEmotions[1] : EmotionType.None;
        OnDeckCreated?.Invoke(_nameInput.text.Trim(), _selectedEmotions[0], emotion2);
        Hide();
    }
}
