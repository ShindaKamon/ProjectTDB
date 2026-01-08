using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si tu utilises TextMeshPro, sinon utilise UnityEngine.UI.Text

/// <summary>
/// Gère l'affichage UI des stats d'une unité Émotions Tactics
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
    [SerializeField] private TextMeshProUGUI _emotionText; // Affiche l'émotion actuelle (ex: "Colère", "Contrariété")

    [Header("Barres (optionnel)")]
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Slider _emotionBar; // Jauge -100 à +100

    [Header("Icônes état (optionnel)")]
    [SerializeField] private GameObject _transformIcon; // Icône transformation active

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
            _ilyaUnit.OnPAChanged -= UpdatePA;
        }
        if (_emotionSystem != null)
        {
            _emotionSystem.OnEmotionChanged -= UpdateEmotion;
            _emotionSystem.OnTransformationChanged -= UpdateTransformIcon;
        }

        _genericUnit = unit;
        _ilyaUnit = unit as IlyaUnit; // Cast si c'est Ilya
        _emotionSystem = unit.GetComponent<EmotionSystem>(); // Récupère EmotionSystem si présent

        if (_ilyaUnit != null)
        {
            // Abonnement à la nouvelle unité
            _ilyaUnit.OnPAChanged += UpdatePA;

            // Abonnement à EmotionSystem (si présent)
            if (_emotionSystem != null)
            {
                _emotionSystem.OnEmotionChanged += UpdateEmotion;
                _emotionSystem.OnTransformationChanged += UpdateTransformIcon;
            }

            // Ilya : affiche tout
            UpdateAll();

            // Active sections spécifiques Ilya
            if (_emotionText != null) _emotionText.gameObject.SetActive(true);
            if (_emotionBar != null) _emotionBar.gameObject.SetActive(true);
            if (_defensePText != null) _defensePText.gameObject.SetActive(true);
            if (_defenseMText != null) _defenseMText.gameObject.SetActive(true);
        }
        else
        {
            // Unité générique : affiche que HP/Mouvement/ATK
            UpdateBasicStats();

            // Désactive sections Ilya
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
            UpdatePM(_ilyaUnit.GetRemainingMovement(), _ilyaUnit.GetMovementRange());
            UpdateDefenseP();
            UpdateDefenseM();

            // Mise à jour des émotions si EmotionSystem présent
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

    /// <summary>
    /// Stats de base (pour Unit générique)
    /// </summary>
    private void UpdateBasicStats()
    {
        if (_genericUnit == null) return;

        UpdateName();
        UpdateHP();
        UpdatePM(_genericUnit.GetRemainingMovement(), _genericUnit.GetMovementRange());
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
            _defensePText.text = $"DEF: {_ilyaUnit.GetDefenseP()}";
        }
    }

    private void UpdateDefenseM()
    {
        if (_defenseMText != null && _ilyaUnit != null)
        {
            _defenseMText.text = $"DEF: {_ilyaUnit.GetDefenseM()}";
        }
    }

    /// <summary>
    /// Met à jour la jauge d'émotion (-100 à +100)
    /// </summary>
    private void UpdateEmotion(float currentEmotion, float maxEmotion)
    {
        // Affiche le nom de l'émotion actuelle
        if (_emotionText != null && _emotionSystem != null)
        {
            string emotionName = _emotionSystem.GetCurrentEmotionName();
            _emotionText.text = $"Émotion: {emotionName}";
        }

        // Met à jour la barre d'émotion (-100 à +100)
        if (_emotionBar != null)
        {
            _emotionBar.minValue = -maxEmotion;
            _emotionBar.maxValue = maxEmotion;
            _emotionBar.value = currentEmotion;
        }
    }

    /// <summary>
    /// Met à jour l'icône de transformation
    /// </summary>
    private void UpdateTransformIcon(EmotionSystem.TransformationState state)
    {
        if (_transformIcon != null)
        {
            // Affiche l'icône si transformé (positif ou négatif)
            _transformIcon.SetActive(state != EmotionSystem.TransformationState.Neutral);
        }
    }

}
