using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'affichage UI des stats d'une unité
/// Compatible avec Unit de base ET IlyaUnit + EmotionSystem
/// </summary>
public class UnitStatsUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _paText;
    [SerializeField] private TextMeshProUGUI _pmText;
    [SerializeField] private TextMeshProUGUI _defensePText;
    [SerializeField] private TextMeshProUGUI _defenseMText;
    [SerializeField] private TextMeshProUGUI _emotionText;

    [Header("Barres (optionnel)")]
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Slider _emotionBar;

    [Header("Icônes état (optionnel)")]
    [SerializeField] private GameObject _transformIcon;

    private IlyaUnit _ilyaUnit;
    private Unit _genericUnit;
    private EmotionSystem _emotionSystem;

    /// <summary>
    /// Initialise l'UI pour une unité donnée
    /// </summary>
    public void SetUnit(Unit unit)
    {
        // Désabonnement de l'ancienne unité (si présente)
        if (_ilyaUnit != null)
        {
            _ilyaUnit.OnActionPointsChanged -= UpdatePA;
        }
        if (_emotionSystem != null)
        {
            _emotionSystem.OnEmotionChanged -= UpdateEmotion;
            _emotionSystem.OnTransformationChanged -= UpdateTransformIcon;
        }

        _genericUnit = unit;
        _ilyaUnit = unit as IlyaUnit;
        unit.TryGetComponentSafe(out _emotionSystem);

        if (_ilyaUnit != null)
        {
            _ilyaUnit.OnActionPointsChanged += UpdatePA;

            if (_emotionSystem != null)
            {
                _emotionSystem.OnEmotionChanged += UpdateEmotion;
                _emotionSystem.OnTransformationChanged += UpdateTransformIcon;
            }

            UpdateAll();

            if (_emotionText != null) _emotionText.gameObject.SetActive(true);
            if (_emotionBar != null) _emotionBar.gameObject.SetActive(true);
            if (_defensePText != null) _defensePText.gameObject.SetActive(true);
            if (_defenseMText != null) _defenseMText.gameObject.SetActive(true);
        }
        else
        {
            UpdateBasicStats();

            if (_emotionText != null) _emotionText.gameObject.SetActive(false);
            if (_emotionBar != null) _emotionBar.gameObject.SetActive(false);
            if (_defensePText != null) _defensePText.gameObject.SetActive(false);
            if (_defenseMText != null) _defenseMText.gameObject.SetActive(false);
            if (_transformIcon != null) _transformIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Met à jour tous les éléments UI
    /// </summary>
    public void UpdateAll()
    {
        if (_ilyaUnit != null)
        {
            UpdateName();
            UpdateHP();
            UpdatePA(_ilyaUnit.GetCurrentPA(), _ilyaUnit.GetMaxPA());
            UpdatePM(_ilyaUnit.GetCurrentMovementPoints(), _ilyaUnit.GetMaxMovementPoints());
            UpdateDefenseP();
            UpdateDefenseM();

            if (_emotionSystem != null)
            {
                UpdateEmotion(_emotionSystem.GetCurrentEmotion(), _emotionSystem.GetPositiveThreshold());
                UpdateTransformIcon(_emotionSystem.GetCurrentState());
            }
        }
        else if (_genericUnit != null)
        {
            UpdateBasicStats();
        }
    }

    private void UpdateBasicStats()
    {
        if (_genericUnit == null) return;

        UpdateName();
        UpdateHP();
        UpdatePM(_genericUnit.GetCurrentMovementPoints(), _genericUnit.GetMaxMovementPoints());
    }

    private void UpdateName()
    {
        if (_nameText != null && _genericUnit != null)
        {
            _nameText.text = _genericUnit.name.Replace("(Clone)", "");
        }
    }

    private void UpdateHP()
    {
        if (_genericUnit == null) return;

        int currentHP = _genericUnit.GetHealth();
        int maxHP = _genericUnit.GetMaxHealth();

        if (_hpText != null)
        {
            _hpText.text = $"PV: {currentHP}/{maxHP}";
        }

        if (_hpBar != null)
        {
            _hpBar.maxValue = maxHP;
            _hpBar.value = currentHP;
        }
    }

    private void UpdatePA(int current, int max)
    {
        if (_paText != null)
        {
            _paText.text = $"PA: {current}/{max}";
        }
    }

    private void UpdatePM(int current, int max)
    {
        if (_pmText != null)
        {
            _pmText.text = $"PM: {current}/{max}";
        }
    }

    private void UpdateDefenseP()
    {
        if (_defensePText != null && _ilyaUnit != null)
        {
            _defensePText.text = $"DEF: {_ilyaUnit.GetPhysicalDefense()}";
        }
    }

    private void UpdateDefenseM()
    {
        if (_defenseMText != null && _ilyaUnit != null)
        {
            _defenseMText.text = $"DEF: {_ilyaUnit.GetMagicalDefense()}";
        }
    }

    private void UpdateEmotion(float currentEmotion, float maxEmotion)
    {
        if (_emotionText != null && _emotionSystem != null)
        {
            string emotionName = _emotionSystem.GetCurrentEmotionName();
            _emotionText.text = $"Émotion: {emotionName}";
        }

        if (_emotionBar != null)
        {
            _emotionBar.minValue = -maxEmotion;
            _emotionBar.maxValue = maxEmotion;
            _emotionBar.value = currentEmotion;
        }
    }

    private void UpdateTransformIcon(EmotionSystem.TransformationState state)
    {
        if (_transformIcon != null)
        {
            _transformIcon.SetActive(state != EmotionSystem.TransformationState.Neutral);
        }
    }
}
