using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Une case du salon local : un joueur inscrit (portrait, champion, deck, Changer / Retirer)
/// ou une case vide « Ajouter un joueur ».
/// </summary>
public class LobbySlotUI : MonoBehaviour
{
    [SerializeField] private GameObject _filledRoot;
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _playerLabel;
    [SerializeField] private TextMeshProUGUI _championName;
    [SerializeField] private TextMeshProUGUI _deckInfo;
    [SerializeField] private Button _changeButton;
    [SerializeField] private Button _removeButton;
    [Tooltip("Bouton de la case vide.")]
    [SerializeField] private Button _addButton;

    public Button ChangeButton => _changeButton;
    public Button RemoveButton => _removeButton;
    public Button AddButton => _addButton;

    public void ShowMember(int playerIndex, ChampionData champion, int deckCardCount)
    {
        gameObject.SetActive(true);
        if (_filledRoot != null) _filledRoot.SetActive(true);
        if (_addButton != null) _addButton.gameObject.SetActive(false);

        if (_playerLabel != null) _playerLabel.text = $"Joueur {playerIndex + 1}";
        if (_championName != null) _championName.text = champion.championName;
        if (_deckInfo != null) _deckInfo.text = $"Deck : {deckCardCount} cartes";

        if (_portrait != null)
        {
            // Même repli que les boutons de la sélection : portrait, sinon illustration en pied
            Sprite sprite = champion.portrait != null ? champion.portrait : champion.fullBodyArt;
            _portrait.sprite = sprite;
            _portrait.enabled = sprite != null;
        }
    }

    public void ShowEmpty()
    {
        gameObject.SetActive(true);
        if (_filledRoot != null) _filledRoot.SetActive(false);
        if (_addButton != null) _addButton.gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
