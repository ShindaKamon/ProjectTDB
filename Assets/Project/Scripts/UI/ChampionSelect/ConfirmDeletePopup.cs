using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup de confirmation avant suppression d'un deck
/// </summary>
public class ConfirmDeletePopup : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _cancelButton;

    private int _deckIndex;

    public System.Action<int> OnDeleteConfirmed;

    void Awake()
    {
        if (_deleteButton != null)
            _deleteButton.onClick.AddListener(OnDeletePressed);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(Hide);

        Hide();
    }

    /// <summary>
    /// Affiche la popup avec le nom du deck à supprimer
    /// </summary>
    public void Show(int deckIndex, string deckName)
    {
        _deckIndex = deckIndex;
        gameObject.SetActive(true);

        if (_messageText != null)
            _messageText.text = $"Supprimer le deck \"{deckName}\" ?\n\nCette action est irréversible.";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDeletePressed()
    {
        OnDeleteConfirmed?.Invoke(_deckIndex);
        Hide();
    }
}
