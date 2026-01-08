using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUnitUI : MonoBehaviour
{
    [SerializeField] private Slider _healthBar; // Référence à la barre de vie
    [SerializeField] private TextMeshProUGUI _healthText; // Référence au texte des PV

    private Unit _unit; // Référence à l'unité parente

    void Awake()
    {
        _unit = GetComponentInParent<Unit>(); // Tente de trouver le composant Unit sur le parent

        if (_unit == null)
        {
            Debug.LogError($"EnemyUnitUI sur {gameObject.name} n'a pas trouvé de composant Unit sur son parent. Désactivation.");
            enabled = false;
            return;
        }

        // Abonne aux événements de l'unité
        _unit.OnHealthChanged += UpdateHealthUI; // S'abonne à l'événement de changement de PV
        _unit.OnUnitDied += HandleUnitDied; // Gère la mort de l'unité

        // Mise à jour initiale
        UpdateHealthUI(_unit.GetHealth(), _unit.GetMaxHealth());
    }

    void OnDestroy()
    {
        if (_unit != null)
        {
            _unit.OnHealthChanged -= UpdateHealthUI; // Se désabonne
            _unit.OnUnitDied -= HandleUnitDied; // Se désabonne
        }
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (_healthBar != null)
        {
            _healthBar.maxValue = maxHealth;
            _healthBar.value = currentHealth;
        }

        if (_healthText != null)
        {
            _healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void HandleUnitDied(Unit diedUnit)
    {
        // Désactive l'UI quand l'unité meurt
        gameObject.SetActive(false);
    }
}
