using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Jauge de vie du champion, en tête du panneau de stats : une barre qui se vide de droite à gauche
/// selon les PV, avec « PV / PV max » au centre.
/// Tant que le champion a du bouclier, la jauge passe en bleu clair et le montant du bouclier
/// s'affiche à la suite des PV.
/// </summary>
public class HealthOrbController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _fillImage;            // Barre en Image Filled, Horizontal, origine à gauche
    [SerializeField] private TextMeshProUGUI _healthText; // « 70/100 » (+ bouclier à la suite)

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
            // Même couleur que les PV : la jauge est déjà bleu clair, un chiffre bleu y serait illisible
            _healthText.text = shielded ? $"{hp}  (+{Mathf.CeilToInt(shield)})" : hp;
        }
    }
}
