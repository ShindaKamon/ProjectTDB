using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup pour renommer un deck existant
/// </summary>
public class RenameDeckPopup : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TextMeshProUGUI _titleText;

    private int _deckIndex;

    public System.Action<int, string> OnDeckRenamed; // (index, nouveau nom)

    void Awake()
    {
        if (_confirmButton != null)
            _confirmButton.onClick.AddListener(OnConfirmPressed);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(Hide);

        if (_nameInput != null)
            _nameInput.onValueChanged.AddListener(OnNameChanged);

        Hide();
    }

    private void OnNameChanged(string value)
    {
        UpdateConfirmButtonState();
    }

    private void UpdateConfirmButtonState()
    {
        if (_confirmButton != null)
        {
            _confirmButton.interactable = !string.IsNullOrWhiteSpace(_nameInput?.text);
        }
    }

    /// <summary>
    /// Affiche la popup avec le nom actuel du deck
    /// </summary>
    public void Show(int deckIndex, string currentName)
    {
        _deckIndex = deckIndex;
        gameObject.SetActive(true);

        if (_nameInput != null)
        {
            _nameInput.text = currentName;
            _nameInput.Select();
            _nameInput.ActivateInputField();
        }

        if (_titleText != null)
            _titleText.text = "Renommer le deck";

        UpdateConfirmButtonState();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnConfirmPressed()
    {
        if (string.IsNullOrWhiteSpace(_nameInput?.text))
            return;

        OnDeckRenamed?.Invoke(_deckIndex, _nameInput.text.Trim());
        Hide();
    }
}
