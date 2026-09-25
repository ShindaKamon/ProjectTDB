using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Ce que filtre une pastille du panneau de filtres du pool.</summary>
public enum FilterChipKind
{
    Emotion,
    Category,
    Cost,
    DamageType // valeurs sérialisées dans la scène : ajouter à la fin
}

/// <summary>
/// Pastille cliquable du panneau de filtres du pool : rond de couleur pour une émotion, texte pour
/// une catégorie ou un coût PA.
/// Porte sa valeur en données : PoolFilterBarUI retrouve toutes les pastilles de ses enfants,
/// donc ajouter un filtre = dupliquer une pastille dans la scène et changer sa valeur.
/// </summary>
[RequireComponent(typeof(Button))]
public class FilterChipUI : MonoBehaviour
{
    [SerializeField] private FilterChipKind _kind;
    [SerializeField] private EmotionType _emotion;
    [SerializeField] private CardCategory _category;
    [SerializeField] private DamageType _damageType;
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
    public DamageType DamageType => _damageType;
    public int Cost => _cost;
    public bool OrMore => _orMore;
    public Button Button { get; private set; }

    void Awake()
    {
        Button = GetComponent<Button>();
        RefreshLabel();
    }

    /// <summary>Nom lisible déduit de la valeur (évite de le saisir à la main dans la scène).
    /// Une pastille d'émotion n'a pas de texte : sa couleur suffit.</summary>
    public void RefreshLabel()
    {
        if (_label == null) return;

        _label.text = _kind switch
        {
            FilterChipKind.Emotion => "",
            FilterChipKind.Category => CodexCardVisual.CategoryName(_category),
            FilterChipKind.DamageType => CodexCardVisual.DamageTypeName(_damageType),
            _ => _orMore ? $"{_cost}+" : _cost.ToString(),
        };
    }

    public void SetActive(bool active)
    {
        if (_kind == FilterChipKind.Emotion)
        {
            // Pastille de couleur : ternie quand inactive, pleine et cerclée de clair quand active
            Color color = CodexCardVisual.EmotionColor(_emotion);
            if (_background != null)
                _background.color = active ? color : Color.Lerp(color, CodexCardVisual.CardBackground, 0.65f);
            var ring = GetComponent<Outline>();
            if (ring != null) ring.enabled = active;
            return;
        }

        if (_background != null)
            _background.color = active ? _activeColor : _inactiveColor;

        if (_label != null)
            _label.color = active ? CodexCardVisual.ReadableTextOn(_activeColor) : new Color(0.85f, 0.85f, 0.85f);
    }
}
