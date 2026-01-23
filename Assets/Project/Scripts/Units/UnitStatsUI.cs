using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'affichage UI des stats d'une unité
/// Compatible avec Unit de base ET Champion (Ilya, Vylos, etc.)
/// </summary>
public class UnitStatsUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _paText;
    [SerializeField] private TextMeshProUGUI _pmText;
    [SerializeField] private TextMeshProUGUI _defenseText;

    [Header("Barres (optionnel)")]
    [SerializeField] private Slider _hpBar;

    private Champion _champion;
    private Unit _genericUnit;

    /// <summary>
    /// Initialise l'UI pour une unité donnée
    /// </summary>
    public void SetUnit(Unit unit)
    {
        // Désabonnement de l'ancienne unité (si présente)
        if (_champion != null)
        {
            _champion.OnActionPointsChanged -= UpdatePA;
        }

        _genericUnit = unit;
        _champion = unit as Champion;

        if (_champion != null)
        {
            _champion.OnActionPointsChanged += UpdatePA;
            UpdateAll();
            // Affiche DEF pour tous les champions (depuis ChampionData)
            if (_defenseText != null) _defenseText.gameObject.SetActive(true);
        }
        else
        {
            UpdateBasicStats();
            if (_defenseText != null) _defenseText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Met à jour tous les éléments UI
    /// </summary>
    public void UpdateAll()
    {
        if (_champion != null)
        {
            UpdateName();
            UpdateHP();
            UpdatePA(_champion.GetCurrentPA(), _champion.GetMaxPA());
            UpdatePM(_champion.GetCurrentMovementPoints(), _champion.GetMaxMovementPoints());
            UpdateDefense();
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

    private void UpdateDefense()
    {
        if (_defenseText != null && _champion != null)
        {
            // Vérifie si le champion a une défense active (IlyaUnit a GetDefense() qui peut changer)
            if (_champion is IlyaUnit ilyaUnit)
            {
                // Utilise la défense active (peut être modifiée par des buffs)
                _defenseText.text = $"DEF: {ilyaUnit.GetDefense()}";
            }
            else
            {
                // Affiche la défense de base depuis ChampionData (pour l'affichage statique)
                int baseDefense = _champion.GetBaseDefense();
                _defenseText.text = $"DEF: {baseDefense}";
            }
        }
    }
}
