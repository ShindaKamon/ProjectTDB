using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Ce que filtre une pastille du panneau de filtres du pool.</summary>
public enum FilterChipKind
{
    Emotion,
    Category,
    Cost
}

/// <summary>
/// Pastille cliquable du panneau de filtres du pool (émotion, catégorie ou coût PA).
/// Porte sa valeur en données : PoolFilterBarUI retrouve toutes les pastilles de ses enfants,
/// donc ajouter un filtre = dupliquer une pastille dans la scène et changer sa valeur.
/// </summary>
[RequireComponent(typeof(Button))]
public class FilterChipUI : MonoBehaviour
{
    [SerializeField] private FilterChipKind _kind;
    [SerializeField] private EmotionType _emotion;
    [SerializeField] private CardCategory _category;
    [Tooltip("Coût PA filtré ; avec 'Or More', toutes les cartes de ce coût ou plus (ex. 4+).")]
    [SerializeField] private int _cost;
    [SerializeField] private bool _orMore;

    [Header("Visuel")]
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Color _inactiveColor = new Color(0.22f, 0.22f, 0.26f);
    [SerializeField] private Color _activeColor = new Color(1f, 0.84f, 0f);

    public FilterChipKind Kind => _kind;
    public EmotionType Emotion => _emotion;
    public CardCategory Category => _category;
    public int Cost => _cost;
    public bool OrMore => _orMore;
    public Button Button { get; private set; }

    void Awake()
    {
        Button = GetComponent<Button>();
        RefreshLabel();
    }

    /// <summary>Nom lisible déduit de la valeur (évite de le saisir à la main dans la scène).</summary>
    public void RefreshLabel()
    {
        if (_label == null) return;

        _label.text = _kind switch
        {
            FilterChipKind.Emotion => CardVisualHelper.GetEmotionName(_emotion),
            FilterChipKind.Category => _category == CardCategory.Eveil ? "Éveil" : _category.ToString(),
            _ => _orMore ? $"{_cost}+" : _cost.ToString(),
        };
    }

    public void SetActive(bool active)
    {
        // Émotion : couleur de l'émotion quand active, pour que le filtre se lise d'un coup d'œil
        Color on = _kind == FilterChipKind.Emotion ? CardVisualHelper.GetEmotionColor(_emotion) : _activeColor;

        if (_background != null)
            _background.color = active ? on : _inactiveColor;

        if (_label != null)
            _label.color = active ? ReadableTextOn(on) : new Color(0.85f, 0.85f, 0.85f);
    }

    /// <summary>Texte noir sur fond clair (Joie, or), blanc sur fond sombre.</summary>
    private static Color ReadableTextOn(Color background)
    {
        float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
        return luminance > 0.6f ? Color.black : Color.white;
    }
}
