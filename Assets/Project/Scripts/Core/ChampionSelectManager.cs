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
    [SerializeField] private LoadoutTabsUI _deckListUI;
    [SerializeField] private GameObject _deckPanelObject; // Panel contenant la zone deck (caché si aucun champion)

    [Header("Boutons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _chooseChampionButton;

    [Header("Avertissement deck incomplet")]
    [Tooltip("Liseré affiché sur le bouton Lancer le combat quand le deck actif a moins de " +
             "DeckData.TOTAL_SLOTS cartes. Le combat reste lançable : ceci est un simple avertissement.")]
    [SerializeField] private Outline _startButtonWarningOutline;
    [SerializeField] private Color _startButtonWarningColor = new Color(1f, 0.55f, 0f); // Orange

    [Header("Navigation")]
    [SerializeField] private ChampionSelectFlowController _flowController;

    [Header("Configuration")]
    [SerializeField] private string _combatSceneName = "CombatScene";
    [SerializeField] private List<ChampionData> _allChampions;
    [SerializeField] private Color _selectedButtonColor = new Color(0.95f, 0.85f, 0.55f); // Doré/bronze, cohérent avec la palette parchemin
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

        if (_chooseChampionButton != null)
        {
            _chooseChampionButton.onClick.AddListener(ConfirmChampionSelection);
            _chooseChampionButton.interactable = false;
        }

        // Cacher le panel deck au départ
        if (_deckPanelObject != null)
            _deckPanelObject.SetActive(false);

        // S'abonner aux événements du DeckEditorUI
        if (_deckListUI != null)
            _deckListUI.OnDeckSelected += OnDeckSelected;

        if (_startButtonWarningOutline != null)
        {
            _startButtonWarningOutline.effectColor = _startButtonWarningColor;
            _startButtonWarningOutline.effectDistance = new Vector2(3, 3);
            _startButtonWarningOutline.enabled = false;
        }
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

            // Avatar du bouton : portrait dédié si assigné, sinon repli sur l'illustration plein
            // corps (compromis MVP le temps que des portraits carrés soient produits).
            Transform avatarTransform = buttonGO.transform.Find("Avatar");
            if (avatarTransform != null)
            {
                Image avatarImage = avatarTransform.GetComponent<Image>();
                Sprite avatarSprite = champion.portrait != null ? champion.portrait : champion.fullBodyArt;
                if (avatarImage != null)
                {
                    avatarImage.sprite = avatarSprite;
                    avatarImage.enabled = avatarSprite != null;
                }
            }

            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                _championButtons[champion] = button;
                ChampionData capturedChampion = champion;
                button.onClick.AddListener(() => SelectChampion(capturedChampion, button));
            }
        }
    }

    /// <summary>
    /// Surligne un champion dans la grille et met à jour son aperçu (écran Sélection Champion).
    /// Ne fait pas encore avancer le flux vers l'écran Liste des Decks : voir ConfirmChampionSelection.
    /// </summary>
    private void SelectChampion(ChampionData champion, Button clickedButton)
    {
        _currentSelectedChampion = champion;

        // Mettre à jour la sélection visuelle des boutons
        UpdateChampionButtonsVisual(clickedButton);

        UpdateChampionDisplay();

        if (_chooseChampionButton != null)
            _chooseChampionButton.interactable = true;
    }

    /// <summary>
    /// Appelé par le bouton "Choisir ce champion" : valide le champion surligné,
    /// charge ses decks et fait avancer le flux vers l'écran Choix du deck.
    /// </summary>
    private void ConfirmChampionSelection()
    {
        if (_currentSelectedChampion == null) return;

        SelectedChampion = _currentSelectedChampion;

        // Afficher le panel deck et charger les decks du champion
        if (_deckPanelObject != null)
            _deckPanelObject.SetActive(true);

        if (_deckListUI != null)
            _deckListUI.ShowDecksForChampion(_currentSelectedChampion);

        // Mettre à jour le deck sélectionné
        UpdateSelectedDeck();

        if (_startButton != null)
            _startButton.interactable = true;

        if (_flowController != null)
            _flowController.ShowScreen(ChampionSelectFlowController.Screen.DeckSelect);
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

        // Mettre le bouton sélectionné en doré/bronze
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

        // Illustration en pied (fond plein écran / bandeau / badge selon l'écran actif)
        if (_flowController != null)
            _flowController.UpdateCharacterArt(_currentSelectedChampion);

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
        UpdateStartButtonWarning(deckCards?.Count ?? 0);
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

        UpdateStartButtonWarning(SelectedDeck?.Count ?? 0);
    }

    /// <summary>
    /// Decision design : un deck incomplet (moins de DeckData.TOTAL_SLOTS cartes) reste
    /// lançable, on affiche seulement un avertissement visuel sur le bouton de lancement.
    /// </summary>
    private void UpdateStartButtonWarning(int cardCount)
    {
        if (_startButtonWarningOutline == null) return;
        _startButtonWarningOutline.enabled = cardCount < DeckData.TOTAL_SLOTS;
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
