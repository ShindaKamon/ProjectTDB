using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orbe de vie du champion : un disque qui se vide de haut en bas selon les PV, avec « PV / PV max »
/// au centre. Version simple (sans texture), en attendant une orbe plus travaillée.
/// </summary>
public class HealthOrbController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _fillImage;            // Disque en Image Filled, Vertical, origine en bas
    [SerializeField] private TextMeshProUGUI _healthText; // « 70/100 »

    // Appelée par BattleUIManager à chaque changement de PV
    public void UpdateHealth(float currentHealth, float maxHealth, Color emotionColor)
    {
        float ratio = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        if (_fillImage != null)
        {
            _fillImage.fillAmount = ratio;
            _fillImage.color = emotionColor;
        }

        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }
}
