using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orbe de vie du champion : un disque qui se vide de haut en bas selon les PV, avec « PV / PV max »
/// au centre. Version simple (sans texture), en attendant une orbe plus travaillée.
/// Tant que le champion a du bouclier, l'orbe passe en bleu clair et le montant du bouclier
/// s'affiche sous les PV.
/// </summary>
public class HealthOrbController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _fillImage;            // Disque en Image Filled, Vertical, origine en bas
    [SerializeField] private TextMeshProUGUI _healthText; // « 70/100 » (+ bouclier en dessous)

    private static Color ShieldColor => CodexCardVisual.ChipColor(ChipKind.Shield);

    // Appelée par BattleUIManager à chaque changement de PV ou de bouclier
    public void UpdateHealth(float currentHealth, float maxHealth, Color emotionColor, float shield = 0f)
    {
        float ratio = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        bool shielded = shield > 0f;

        if (_fillImage != null)
        {
            _fillImage.fillAmount = ratio;
            _fillImage.color = shielded ? ShieldColor : emotionColor;
        }

        if (_healthText != null)
        {
            string hp = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            // Même couleur que les PV : l'orbe est déjà bleu clair, un chiffre bleu y serait illisible
            _healthText.text = shielded ? $"{hp}\n+{Mathf.CeilToInt(shield)}" : hp;
        }
    }
}
