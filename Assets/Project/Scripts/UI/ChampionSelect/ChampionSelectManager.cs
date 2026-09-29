using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ChampionSelectManager : MonoBehaviour
{
    // Champion validé et deck choisi du joueur en cours ; l'équipe du combat est dans CombatParty
    private ChampionData _selectedChampion;
    private List<CardData> _selectedDeck;

    [Header("References UI - Zone Champion (Gauche)")]
    [SerializeField] private Transform _championButtonParent;
    [SerializeField] private GameObject _championButtonPrefab;
    [SerializeField] private ChampionStatsUI _championStatsUI;

    [Header("Références UI - Zone Deck (Droite)")]
    [SerializeField] private LoadoutTabsUI _deckListUI;
    [SerializeField] private GameObject _deckPanelObject; // Panel contenant la zone deck (caché si aucun champion)

    [Header("Boutons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _chooseChampionButton;

    [Header("Deck incomplet")]
    [Tooltip("Liseré affiché sur le bouton Lancer le combat quand le deck actif a moins de " +
             "DeckData.TOTAL_SLOTS cartes ; le bouton est alors grisé et indique le nombre de cartes.")]
    [SerializeField] private Outline _startButtonWarningOutline;
    [SerializeField] private Color _startButtonWarningColor = new Color(1f, 0.55f, 0f); // Orange

    [Header("Navigation")]
    [SerializeField] private ChampionSelectFlowController _flowController;

    [Header("Configuration")]
    [SerializeField] private string _combatSceneName = "CombatScene";
    [SerializeField] private List<ChampionData> _allChampions;
    [SerializeField] private Color _selectedButtonColor = new Color(0.95f, 0.85f, 0.55f); // Doré/bronze, cohérent avec la palette parchemin
    [SerializeField] private Color _normalButtonColor = Color.white;

    [Header("Multijoueur (salon local, plusieurs joueurs sur un seul PC)")]
    [SerializeField] private LobbyUI _lobby;
    [Tooltip("Écran Sélection du champion : retour au salon (multijoueur) ou au menu principal (solo).")]
    [SerializeField] private Button _backFromChampionSelectButton;
    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
    [Tooltip("Libellé du bouton de l'écran Choix du deck en multijoueur (il inscrit le joueur au lieu de lancer le combat).")]
    [SerializeField] private string _confirmPlayerLabel = "Valider le joueur";

    // Libellé du bouton de lancement, remplacé par le nombre de cartes tant que le deck est incomplet
    private TextMeshProUGUI _startLabel;
    private string _startLabelText;

    private ChampionData _currentSelectedChampion;
    private Button _selectedChampionButton;
    private Dictionary<ChampionData, Button> _championButtons = new Dictionary<ChampionData, Button>();

    // Multijoueur : place du salon en cours de choix (CombatParty.Count = nouveau joueur)
    private int _editingSlot;

    void Awake()
    {
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

        if (_backFromChampionSelectButton != null)
            _backFromChampionSelectButton.onClick.AddListener(BackFromChampionSelect);

        _startLabel = _startButton != null ? _startButton.GetComponentInChildren<TextMeshProUGUI>() : null;
        if (CombatParty.IsMultiplayer && _startLabel != null) _startLabel.text = _confirmPlayerLabel;
        _startLabelText = _startLabel != null ? _startLabel.text : "";

        if (_lobby != null)
        {
            _lobby.OnEditSlot += EditSlot;
            _lobby.OnRemoveSlot += RemoveSlot;
            _lobby.OnStart += StartMultiplayerGame;
            _lobby.OnBack += BackToMainMenu;
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
        // Multijoueur réseau : le salon suit la session (arrivées, départs, choix de chacun)
        if (NetworkSession.IsActive)
        {
            NetworkSession.Instance.OnLobbyChanged += OnNetworkLobbyChanged;
            OnNetworkLobbyChanged();
            ShowLobby();
        }
        // Multijoueur : on arrive sur le salon, le champion se choisit en ouvrant une case
        else if (CombatParty.IsMultiplayer)
            _lobby?.Refresh();
        else
            SelectFirstAvailableChampion();
    }

    /// <summary>
    /// Surligne le premier champion pas encore pris par un autre joueur de l'équipe.
    /// </summary>
    private void SelectFirstAvailableChampion()
    {
        if (_allChampions == null) return;

        foreach (ChampionData champion in _allChampions)
        {
            if (champion == null || IsTakenByAnotherPlayer(champion)) continue;
            if (_championButtons.TryGetValue(champion, out Button button))
            {
                SelectChampion(champion, button);
                return;
            }
        }
    }

    private bool IsTakenByAnotherPlayer(ChampionData champion)
    {
        if (NetworkSession.IsActive)
            return NetworkSession.Instance.Lobby.IsTakenByOther(champion.championName, NetworkSession.Instance.LocalClientId);
        if (!CombatParty.IsMultiplayer) return false;
        int owner = CombatParty.IndexOf(champion);
        return owner >= 0 && owner != _editingSlot;
    }

    void OnDestroy()
    {
        if (NetworkSession.Instance != null)
            NetworkSession.Instance.OnLobbyChanged -= OnNetworkLobbyChanged;

        if (_deckListUI != null)
            _deckListUI.OnDeckSelected -= OnDeckSelected;

        if (_lobby != null)
        {
            _lobby.OnEditSlot -= EditSlot;
            _lobby.OnRemoveSlot -= RemoveSlot;
            _lobby.OnStart -= StartMultiplayerGame;
            _lobby.OnBack -= BackToMainMenu;
        }
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
        if (_currentSelectedChampion == null || IsTakenByAnotherPlayer(_currentSelectedChampion)) return;

        _selectedChampion = _currentSelectedChampion;

        // Afficher le panel deck et charger les decks du champion
        if (_deckPanelObject != null)
            _deckPanelObject.SetActive(true);

        if (_deckListUI != null)
            _deckListUI.ShowDecksForChampion(_currentSelectedChampion);

        // Mettre à jour le deck sélectionné (active le bouton de lancement si le deck est complet)
        UpdateSelectedDeck();

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

        if (_championStatsUI != null)
        {
            _championStatsUI.ShowChampion(_currentSelectedChampion);
        }

        // Illustration en pied (fond plein écran / bandeau / badge selon l'écran actif)
        if (_flowController != null)
            _flowController.UpdateCharacterArt(_currentSelectedChampion);
    }

    private void OnDeckSelected(List<CardData> deckCards)
    {
        _selectedDeck = deckCards;
        UpdateStartButtonState(deckCards);
        GameLog.Log($"Deck sélectionné avec {deckCards.Count} cartes.");
    }

    private void UpdateSelectedDeck()
    {
        if (_deckListUI != null)
        {
            _selectedDeck = _deckListUI.GetSelectedDeckCards();
        }
        else if (_currentSelectedChampion != null)
        {
            // Fallback: utiliser le startingDeck du champion
            _selectedDeck = new List<CardData>(_currentSelectedChampion.startingDeck);
        }

        UpdateStartButtonState(_selectedDeck);
    }

    /// <summary>
    /// Décisions du 28/09/2026 : un deck incomplet (moins de DeckData.TOTAL_SLOTS cartes) ou qui
    /// garde des cartes hors de ses couleurs (après un changement de couleurs) ne peut pas être
    /// choisi pour le combat. Le bouton est grisé, entouré du liseré orange, et son libellé dit pourquoi.
    /// </summary>
    private void UpdateStartButtonState(IList<CardData> cards)
    {
        int cardCount = cards?.Count ?? 0;
        int offColor = DeckRules.CountOffColor(cards, _deckListUI != null ? _deckListUI.SelectedDeckColors : null);
        bool playable = DeckRules.IsComplete(cardCount) && offColor == 0;

        if (_startButton != null)
            _startButton.interactable = _selectedChampion != null && playable;

        if (_startButtonWarningOutline != null)
            _startButtonWarningOutline.enabled = !playable;

        if (_startLabel != null)
        {
            if (playable) _startLabel.text = _startLabelText;
            else if (offColor > 0) _startLabel.text = $"Deck invalide : {offColor} carte{(offColor > 1 ? "s" : "")} hors couleurs";
            else _startLabel.text = $"Deck incomplet : {cardCount}/{DeckData.TOTAL_SLOTS} cartes";
        }
    }

    /// <summary>
    /// Bouton de l'écran Choix du deck : lance le combat en solo, inscrit le joueur et
    /// revient au salon en multijoueur.
    /// </summary>
    private void StartGame()
    {
        if (_selectedChampion == null)
        {
            GameLog.LogWarning("Aucun champion sélectionné pour ce joueur.");
            return;
        }

        // S'assurer qu'on a un deck
        if (_selectedDeck == null || _selectedDeck.Count == 0)
        {
            UpdateSelectedDeck();
        }

        if (!DeckRules.IsPlayable(_selectedDeck, _deckListUI != null ? _deckListUI.SelectedDeckColors : null))
        {
            GameLog.LogWarning($"Deck injouable ({_selectedDeck?.Count ?? 0}/{DeckData.TOTAL_SLOTS} cartes, ou cartes hors couleurs) : combat refusé.");
            return;
        }

        if (CombatParty.IsMultiplayer)
        {
            ConfirmPlayer();
            return;
        }

        CombatParty.Clear();
        CombatParty.TryAdd(_selectedChampion, _selectedDeck);
        GameLog.Log($"Lancement du combat avec {_selectedChampion.championName} et un deck de {_selectedDeck?.Count ?? 0} cartes.");
        SceneManager.LoadScene(_combatSceneName);
    }

    // ========== MULTIJOUEUR (salon local) ==========

    /// <summary>Inscrit (ou modifie) le joueur de la case en cours puis revient au salon.</summary>
    private void ConfirmPlayer()
    {
        // Réseau : le choix part chez l'hôte, qui le valide et le renvoie à tous
        if (NetworkSession.IsActive)
        {
            var deckNames = _selectedDeck != null ? _selectedDeck.ConvertAll(c => c.cardName) : new List<string>();
            NetworkSession.Instance.SubmitPick(_selectedChampion.championName, deckNames);
            ShowLobby();
            return;
        }

        bool ok = _editingSlot >= CombatParty.Count
            ? CombatParty.TryAdd(_selectedChampion, _selectedDeck)
            : CombatParty.TryReplace(_editingSlot, _selectedChampion, _selectedDeck);

        if (!ok)
        {
            GameLog.LogWarning($"Impossible d'inscrire {_selectedChampion.championName} : déjà pris ou salon complet.");
            return;
        }

        GameLog.Log($"Joueur {_editingSlot + 1} : {_selectedChampion.championName}, deck de {_selectedDeck?.Count ?? 0} cartes.");
        ShowLobby();
    }

    /// <summary>Case du salon cliquée (Ajouter ou Changer) : choix du champion pour ce joueur.</summary>
    private void EditSlot(int slot)
    {
        _editingSlot = slot;
        _selectedChampion = null;
        _selectedDeck = null;

        foreach (var kvp in _championButtons)
            kvp.Value.interactable = !IsTakenByAnotherPlayer(kvp.Key);

        // Afficher l'écran avant de sélectionner : la fiche du champion a besoin d'être active
        if (_flowController != null)
            _flowController.ShowScreen(ChampionSelectFlowController.Screen.ChampionSelect);

        // Changer : on repart du champion actuel du joueur ; Ajouter : du premier libre
        ChampionData current = slot < CombatParty.Count ? CombatParty.Members[slot].Champion : null;
        if (NetworkSession.IsActive)
        {
            var lobby = NetworkSession.Instance.Lobby;
            int own = lobby.IndexOf(NetworkSession.Instance.LocalClientId);
            current = own >= 0 && lobby.Members[own].HasPicked ? FindChampion(lobby.Members[own].ChampionName) : null;
        }
        if (current != null && _championButtons.TryGetValue(current, out Button button))
            SelectChampion(current, button);
        else
            SelectFirstAvailableChampion();
    }

    private void RemoveSlot(int slot)
    {
        CombatParty.RemoveAt(slot);
        _lobby?.Refresh();
    }

    private void ShowLobby()
    {
        RefreshLobby();
        if (_flowController != null)
            _flowController.ShowScreen(ChampionSelectFlowController.Screen.Lobby);
    }

    private void RefreshLobby()
    {
        if (_lobby == null) return;

        if (NetworkSession.IsActive)
        {
            NetworkSession session = NetworkSession.Instance;
            string info = session.IsHost ? $"Adresse à donner : {NetworkSession.LocalIPv4()}" : "En attente de l'hôte";
            _lobby.RefreshNetwork(session.Lobby, session.LocalClientId, session.IsHost, FindChampion, info);
        }
        else
        {
            _lobby.Refresh();
        }
    }

    // ========== MULTIJOUEUR (réseau local) ==========

    private ChampionData FindChampion(string championName) =>
        _allChampions != null ? _allChampions.Find(c => c != null && c.championName == championName) : null;

    /// <summary>
    /// Le salon réseau a changé : l'équipe du combat (CombatParty) suit les joueurs qui ont choisi,
    /// dans l'ordre d'arrivée (= ordre des tours) ; elle est prête quand l'hôte lance le combat.
    /// </summary>
    private void OnNetworkLobbyChanged()
    {
        if (!NetworkSession.IsActive) return;

        CombatParty.Clear();
        CombatParty.IsMultiplayer = true;
        CardCollection collection = _deckListUI != null ? _deckListUI.Collection : null;
        foreach (LobbyState.Member member in NetworkSession.Instance.Lobby.Members)
        {
            ChampionData champion = member.HasPicked ? FindChampion(member.ChampionName) : null;
            if (champion == null) continue;
            CombatParty.TryAdd(champion, DeckSaveManager.GetCardsFromNames(member.DeckCardNames, collection));
        }

        // Rafraîchit le salon seulement s'il est affiché (pas pendant le choix du champion)
        if (_flowController == null || _flowController.CurrentScreen == ChampionSelectFlowController.Screen.Lobby)
            RefreshLobby();
    }

    private void StartMultiplayerGame()
    {
        if (NetworkSession.IsActive)
        {
            if (NetworkSession.Instance.StartCombat(_combatSceneName))
                GameLog.Log($"Réseau : lancement du combat avec {CombatParty.Count} joueurs.");
            return;
        }

        if (CombatParty.Count < 2) return;
        GameLog.Log($"Lancement du combat avec {CombatParty.Count} joueurs.");
        SceneManager.LoadScene(_combatSceneName);
    }

    private void BackFromChampionSelect()
    {
        if (CombatParty.IsMultiplayer)
            ShowLobby();
        else
            BackToMainMenu();
    }

    private void BackToMainMenu()
    {
        // Réseau : on quitte la partie (l'hôte la ferme pour tout le monde)
        if (NetworkSession.Instance != null) NetworkSession.Instance.Shutdown();
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
