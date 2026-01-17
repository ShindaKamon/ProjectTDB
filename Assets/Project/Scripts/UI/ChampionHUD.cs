using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD affichant les stats du champion (Ilya) à l'écran.
/// Se met à jour automatiquement en s'abonnant aux événements de l'unité.
/// </summary>
public class ChampionHUD : MonoBehaviour
{
    [Header("Textes Stats")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _paText;
    [SerializeField] private TextMeshProUGUI _pmText;
    [SerializeField] private TextMeshProUGUI _atkText;
    [SerializeField] private TextMeshProUGUI _defText;

    [Header("Barres (optionnel)")]
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Image _hpBarFill;

    [Header("Couleurs HP")]
    [SerializeField] private Color _hpColorFull = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _hpColorMid = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private Color _hpColorLow = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private float _lowHpThreshold = 0.3f;
    [SerializeField] private float _midHpThreshold = 0.6f;

    private IlyaUnit _champion;
    private bool _isConnected = false;

    void Start()
    {
        // Essaie de se connecter immédiatement
        TryConnectToChampion();
    }

    void Update()
    {
        // Continue d'essayer de se connecter si pas encore fait
        if (!_isConnected)
        {
            TryConnectToChampion();
        }
    }

    private void TryConnectToChampion()
    {
        if (Services.Grid == null) return;

        Unit activeUnit = Services.Grid.GetActiveUnit();
        if (activeUnit is IlyaUnit ilya)
        {
            SetChampion(ilya);
        }
    }

    /// <summary>
    /// Connecte le HUD à un champion spécifique
    /// </summary>
    public void SetChampion(IlyaUnit champion)
    {
        // Désabonnement de l'ancien champion
        if (_champion != null)
        {
            _champion.OnHealthChanged -= OnHealthChanged;
            _champion.OnActionPointsChanged -= OnPAChanged;
            _champion.OnMovementPointsChanged -= OnPMChanged;
            _champion.OnStatsModified -= OnStatsModified;
        }

        _champion = champion;

        if (_champion != null)
        {
            // Abonnement aux événements
            _champion.OnHealthChanged += OnHealthChanged;
            _champion.OnActionPointsChanged += OnPAChanged;
            _champion.OnMovementPointsChanged += OnPMChanged;
            _champion.OnStatsModified += OnStatsModified;

            // Mise à jour initiale
            RefreshAllStats();
            _isConnected = true;

            Debug.Log($"ChampionHUD: Connecté à {_champion.name}");
        }
    }

    void OnDestroy()
    {
        // Désabonnement propre
        if (_champion != null)
        {
            _champion.OnHealthChanged -= OnHealthChanged;
            _champion.OnActionPointsChanged -= OnPAChanged;
            _champion.OnMovementPointsChanged -= OnPMChanged;
            _champion.OnStatsModified -= OnStatsModified;
        }
    }

    // ========== CALLBACKS ==========

    private void OnHealthChanged(int current, int max)
    {
        UpdateHP(current, max);
    }

    private void OnPAChanged(int current, int max)
    {
        UpdatePA(current, max);
    }

    private void OnPMChanged(int current, int max)
    {
        UpdatePM(current, max);
    }

    private void OnStatsModified()
    {
        UpdateDefense();
        UpdateAtk();
    }

    // ========== MISES À JOUR UI ==========

    /// <summary>
    /// Rafraîchit toutes les stats affichées
    /// </summary>
    public void RefreshAllStats()
    {
        if (_champion == null) return;

        UpdateHP(_champion.GetHealth(), _champion.GetMaxHealth());
        UpdatePA(_champion.GetCurrentPA(), _champion.GetMaxPA());
        UpdatePM(_champion.GetCurrentMovementPoints(), _champion.GetMaxMovementPoints());
        UpdateAtk();
        UpdateDefense();
    }

    private void UpdateHP(int current, int max)
    {
        if (_hpText != null)
        {
            _hpText.text = $"HP: {current}/{max}";
        }

        if (_hpBar != null)
        {
            _hpBar.maxValue = max;
            _hpBar.value = current;

            // Couleur dynamique selon le pourcentage de vie
            if (_hpBarFill != null)
            {
                float hpPercent = (float)current / max;
                if (hpPercent <= _lowHpThreshold)
                {
                    _hpBarFill.color = _hpColorLow;
                }
                else if (hpPercent <= _midHpThreshold)
                {
                    _hpBarFill.color = _hpColorMid;
                }
                else
                {
                    _hpBarFill.color = _hpColorFull;
                }
            }
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

    private void UpdateAtk()
    {
        if (_atkText != null && _champion != null)
        {
            _atkText.text = $"ATK: {_champion.GetAttack()}";
        }
    }

    private void UpdateDefense()
    {
        if (_defText != null && _champion != null)
        {
            _defText.text = $"DEF: {_champion.GetDefense()}";
        }
    }
}
