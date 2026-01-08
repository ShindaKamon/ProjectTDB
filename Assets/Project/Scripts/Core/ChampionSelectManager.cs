using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ChampionSelectManager : MonoBehaviour
{
    public static ChampionData SelectedChampion { get; private set; }

    [Header("Références UI")]
    [SerializeField] private Transform _championButtonParent; // Parent des boutons de sélection de champion
    [SerializeField] private GameObject _championButtonPrefab; // Prefab du bouton de champion
    [SerializeField] private TextMeshProUGUI _selectedChampionNameText;
    [SerializeField] private TextMeshProUGUI _selectedChampionStatsText;
    [SerializeField] private Button _startButton; // Bouton pour commencer le jeu

    [Header("Configuration")]
    [SerializeField] private string _combatSceneName = "CombatScene"; // Nom de la scène de combat
    [SerializeField] private List<ChampionData> _allChampions; // Liste des champions disponibles, à assigner manuellement

    private ChampionData _currentSelectedChampion;

    void Awake()
    {
        if (_allChampions == null || _allChampions.Count == 0)
        {
            Debug.LogError("Aucun ChampionData n'est assigné au ChampionSelectManager. Assurez-vous de les glisser-déposer dans l'Inspector.");
            _startButton.interactable = false;
            return;
        }

        GenerateChampionButtons();
        _startButton.onClick.AddListener(StartGame);
        _startButton.interactable = false; // Désactivé par défaut, activé après sélection

    }

    private void GenerateChampionButtons()
    {
        // Vérifications de sécurité
        if (_championButtonPrefab == null)
        {
            Debug.LogError("ChampionSelectManager: _championButtonPrefab n'est pas assigné dans l'Inspector!");
            return;
        }

        if (_championButtonParent == null)
        {
            Debug.LogError("ChampionSelectManager: _championButtonParent n'est pas assigné dans l'Inspector!");
            return;
        }

        foreach (ChampionData champion in _allChampions)
        {
            if (champion == null)
            {
                Debug.LogWarning("ChampionSelectManager: Un ChampionData null trouvé dans la liste!");
                continue;
            }

            GameObject buttonGO = Instantiate(_championButtonPrefab, _championButtonParent);
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = champion.championName;

            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectChampion(champion));
            }
        }
    }

    private void SelectChampion(ChampionData champion)
    {
        _currentSelectedChampion = champion;
        SelectedChampion = champion; // Stocke le champion sélectionné statiquement

        UpdateChampionDisplay();
        _startButton.interactable = true;
    }

    private void UpdateChampionDisplay()
    {
        if (_currentSelectedChampion == null) return;

        if (_selectedChampionNameText != null) _selectedChampionNameText.text = _currentSelectedChampion.championName;

        // Construire la chaîne de stats (plus d'attaque, tout passe par les cartes)
        string stats = $"PV Max: {_currentSelectedChampion.maxHealth}\n";
        stats += $"Mouvement: {_currentSelectedChampion.movementRange}\n";

        // Ajouter les stats spécifiques si disponibles
        if (_currentSelectedChampion.maxPA > 0)
        {
            stats += $"PA Max: {_currentSelectedChampion.maxPA}\n";
            stats += $"Défense P: {_currentSelectedChampion.defenseP}\n";
            stats += $"Défense M: {_currentSelectedChampion.defenseM}\n";

            // Affiche les émotions si disponibles
            if (_currentSelectedChampion.familyEmotionData != null)
            {
                stats += $"Famille: {_currentSelectedChampion.familyEmotionData.familyType}\n";
                stats += $"Émotions: {_currentSelectedChampion.familyEmotionData.positiveEmotionName}/{_currentSelectedChampion.familyEmotionData.neutralEmotionName}/{_currentSelectedChampion.familyEmotionData.negativeEmotionName}\n";
            }
        }

        if (_selectedChampionStatsText != null) _selectedChampionStatsText.text = stats;
    }

    private void StartGame()
    {
        if (SelectedChampion != null)
        {
            Debug.Log($"Lancement du jeu avec {SelectedChampion.championName}");
            SceneManager.LoadScene(_combatSceneName);
        }
        else
        {
            Debug.LogWarning("Aucun champion sélectionné pour commencer le jeu.");
        }
    }
}
