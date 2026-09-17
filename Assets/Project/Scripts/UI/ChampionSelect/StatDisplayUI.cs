using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Petit composant pour afficher une stat individuelle
/// Structure: [Icone] [Valeur] [Label]
/// </summary>
public class StatDisplayUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private Image _background;

    [Header("Couleurs selon valeur")]
    [SerializeField] private Color _lowColor = new Color(0.8f, 0.3f, 0.3f);
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highColor = new Color(0.3f, 0.8f, 0.3f);

    /// <summary>
    /// Definit la valeur et le label de cette stat
    /// </summary>
    public void SetValue(int value, string label = null)
    {
        if (_valueText != null)
            _valueText.text = value.ToString();

        if (_labelText != null && !string.IsNullOrEmpty(label))
            _labelText.text = label;
    }

    /// <summary>
    /// Definit la valeur avec une indication de modification
    /// </summary>
    public void SetValueWithChange(int baseValue, int currentValue, string label = null)
    {
        if (_valueText != null)
        {
            _valueText.text = currentValue.ToString();

            if (currentValue > baseValue)
                _valueText.color = _highColor;
            else if (currentValue < baseValue)
                _valueText.color = _lowColor;
            else
                _valueText.color = _normalColor;
        }

        if (_labelText != null && !string.IsNullOrEmpty(label))
            _labelText.text = label;
    }
}
