using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ChampionSelectManager : MonoBehaviour
{
    public static ChampionData SelectedChampion { get; private set; }
    public static List<CardData> SelectedDeck { get; private set; }

    [Header("References UI - Zone Champion (Gauche)")]
    [SerializeField] private Transform _championButtonParent;
    [SerializeField] private GameObject _championButtonPrefab;
    [SerializeField] private TextMeshProUGUI _selectedChampionNameText;
    [SerializeField] private TextMeshProUGUI _selectedChampionStatsText;
    [SerializeField] private ChampionStatsUI _championStatsUI;  // Nouveau composant moderne (optionnel)

    [Header("Références UI - Zone Deck (Droite)")]
    [SerializeField] private DeckListUI _deckListUI;
    [SerializeField] private GameObject _deckPanelObject; // Panel contenant la zone deck (caché si aucun champion)

    [Header("Boutons")]
    [SerializeField] private Button _startButton;

    [Header("Configuration")]
    [SerializeField] private string _combatSceneName = "CombatScene";
    [SerializeField] private List<ChampionData> _allChampions;
    [SerializeField] private Color _selectedButtonColor = new Color(0.8f, 0.2f, 0.2f); // Rouge
    [SerializeField] private Color _normalButtonColor = Color.white;

    private ChampionData _currentSelectedChampion;
    private Button _selectedChampionButton;
    private Dictionary<ChampionData, Button> _championButtons = new Dictionary<ChampionData, Button>();

    void Awake()
    {
        // Réinitialiser les données statiques
        SelectedChampion = null;
        SelectedDeck = null;

        if (_allChampions == null || _allChampions.Count == 0)
        {
            Debug.LogError("Aucun ChampionData n'est assigné au ChampionSelectManager.");
            if (_startButton != null) _startButton.interactable = false;
            return;
        }

        GenerateChampionButtons();

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(StartGame);
            _startButton.interactable = false;
        }

        // Cacher le panel deck au départ
        if (_deckPanelObject != null)
            _deckPanelObject.SetActive(false);

        // S'abonner aux événements du DeckEditorUI
        if (_deckListUI != null)
            _deckListUI.OnDeckSelected += OnDeckSelected;
    }

    void Start()
    {
        // Sélectionner automatiquement le premier champion au démarrage
        if (_allChampions != null && _allChampions.Count > 0)
        {
            ChampionData firstChampion = _allChampions[0];
            if (_championButtons.TryGetValue(firstChampion, out Button button))
            {
                SelectChampion(firstChampion, button);
            }
        }
    }

    void OnDestroy()
    {
        if (_deckListUI != null)
            _deckListUI.OnDeckSelected -= OnDeckSelected;
    }

    private void GenerateChampionButtons()
    {
        if (_championButtonPrefab == null)
        {
            Debug.LogError("ChampionSelectManager: _championButtonPrefab n'est pas assigné!");
            return;
        }

        if (_championButtonParent == null)
        {
            Debug.LogError("ChampionSelectManager: _championButtonParent n'est pas assigné!");
            return;
        }

        foreach (ChampionData champion in _allChampions)
        {
            if (champion == null)
            {
                GameLog.LogWarning("ChampionSelectManager: Un ChampionData null trouvé dans la liste!");
                continue;
            }

            GameObject buttonGO = Instantiate(_championButtonPrefab, _championButtonParent);
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = champion.championName;

            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                _championButtons[champion] = button;
                ChampionData capturedChampion = champion;
                button.onClick.AddListener(() => SelectChampion(capturedChampion, button));
            }
        }
    }

    private void SelectChampion(ChampionData champion, Button clickedButton)
    {
        _currentSelectedChampion = champion;
        SelectedChampion = champion;

        // Mettre à jour la sélection visuelle des boutons
        UpdateChampionButtonsVisual(clickedButton);

        UpdateChampionDisplay();

        // Afficher le panel deck et charger les decks du champion
        if (_deckPanelObject != null)
            _deckPanelObject.SetActive(true);

        if (_deckListUI != null)
            _deckListUI.ShowDecksForChampion(champion);

        // Mettre à jour le deck sélectionné
        UpdateSelectedDeck();

        if (_startButton != null)
            _startButton.interactable = true;
    }

    private void UpdateChampionButtonsVisual(Button selectedButton)
    {
        // Réinitialiser tous les boutons à la couleur normale
        foreach (var kvp in _championButtons)
        {
            var button = kvp.Value;
            var colors = button.colors;
            colors.normalColor = _normalButtonColor;
            colors.selectedColor = _normalButtonColor;
            button.colors = colors;
        }

        // Mettre le bouton sélectionné en rouge
        if (selectedButton != null)
        {
            _selectedChampionButton = selectedButton;
            var colors = selectedButton.colors;
            colors.normalColor = _selectedButtonColor;
            colors.selectedColor = _selectedButtonColor;
            selectedButton.colors = colors;
        }
    }

    private void UpdateChampionDisplay()
    {
        if (_currentSelectedChampion == null) return;

        // Nouveau systeme: utiliser ChampionStatsUI si disponible
        if (_championStatsUI != null)
        {
            _championStatsUI.ShowChampion(_currentSelectedChampion);
        }

        // Ancien systeme (fallback): textes simples
        if (_selectedChampionNameText != null)
            _selectedChampionNameText.text = _currentSelectedChampion.championName;

        if (_selectedChampionStatsText != null)
        {
            string stats = $"HP: {_currentSelectedChampion.maxHealth}\n";
            stats += $"PM: {_currentSelectedChampion.movementRange}\n";
            stats += $"PA: {_currentSelectedChampion.maxActionPoints}\n";
            stats += $"ATK: {_currentSelectedChampion.attackDamage}\n";
            stats += $"DEF: {_currentSelectedChampion.defense}";
            _selectedChampionStatsText.text = stats;
        }
    }

    private void OnDeckSelected(List<CardData> deckCards)
    {
        SelectedDeck = deckCards;
        GameLog.Log($"Deck sélectionné avec {deckCards.Count} cartes.");
    }

    private void UpdateSelectedDeck()
    {
        if (_deckListUI != null)
        {
            SelectedDeck = _deckListUI.GetSelectedDeckCards();
        }
        else if (_currentSelectedChampion != null)
        {
            // Fallback: utiliser le startingDeck du champion
            SelectedDeck = new List<CardData>(_currentSelectedChampion.startingDeck);
        }
    }

    private void StartGame()
    {
        if (SelectedChampion == null)
        {
            GameLog.LogWarning("Aucun champion sélectionné pour commencer le jeu.");
            return;
        }

        // S'assurer qu'on a un deck
        if (SelectedDeck == null || SelectedDeck.Count == 0)
        {
            UpdateSelectedDeck();
        }

        GameLog.Log($"Lancement du jeu avec {SelectedChampion.championName} et un deck de {SelectedDeck?.Count ?? 0} cartes.");
        SceneManager.LoadScene(_combatSceneName);
    }
}
