using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Version simplifiée du ChampionSelectManager pour tester rapidement sans UI.
/// Utilise ce script si tu veux tester directement sans créer toute l'UI de sélection.
/// </summary>
public class SimpleChampionSelector : MonoBehaviour
{
    [Header("Configuration Simple")]
    [SerializeField] private ChampionData _defaultChampion; // Le champion à utiliser par défaut
    [SerializeField] private string _combatSceneName = "CombatScene";
    [SerializeField] private bool _autoStart = true; // Démarre automatiquement
    [SerializeField] private float _autoStartDelay = 0.5f; // Délai avant démarrage auto

    void Start()
    {
        if (_defaultChampion == null)
        {
            Debug.LogError("SimpleChampionSelector: Aucun champion par défaut assigné!");
            return;
        }

        // Définit le champion sélectionné (utilisé par le vrai ChampionSelectManager)
        SetSelectedChampion(_defaultChampion);

        if (_autoStart)
        {
            Debug.Log($"Démarrage automatique avec {_defaultChampion.championName} dans {_autoStartDelay}s...");
            Invoke(nameof(LoadCombatScene), _autoStartDelay);
        }
    }

    void Update()
    {
        // Permet de démarrer manuellement avec Espace si autoStart est désactivé
        if (!_autoStart && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadCombatScene();
        }
    }

    private void SetSelectedChampion(ChampionData champion)
    {
        // Utilise la réflexion pour accéder à la propriété statique privée
        var property = typeof(ChampionSelectManager).GetProperty("SelectedChampion",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (property != null)
        {
            property.SetValue(null, champion);
            Debug.Log($"Champion sélectionné: {champion.championName}");
        }
    }

    private void LoadCombatScene()
    {
        if (!string.IsNullOrEmpty(_combatSceneName))
        {
            Debug.Log($"Chargement de {_combatSceneName}...");
            SceneManager.LoadScene(_combatSceneName);
        }
        else
        {
            Debug.LogError("SimpleChampionSelector: Nom de scène de combat non défini!");
        }
    }
}
