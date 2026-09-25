using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Barre de vie de boss affichée en haut de l'écran.
/// Contrairement aux barres normales qui suivent l'unité, celle-ci reste fixe en UI.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private TextMeshProUGUI _healthText; // Ex: "250/500"
    [SerializeField] private GameObject _container; // Container à masquer quand pas de boss

    [Header("Optional Visual Elements")]
    [SerializeField] private Image _bossPortrait; // Portrait du boss (optionnel)
    [SerializeField] private Image _fillImage; // Image de remplissage de la barre

    [Header("Stats du boss (PM, PA, attaque, armure, barrière, bouclier)")]
    [SerializeField] private Transform _statChipsContainer;
    [SerializeField] private Sprite _chipBackground;

    private Enemy _trackedBoss;

    void Start()
    {
        GameLog.Log("BossHealthBarUI: Start() appelé");

        // Configure le slider
        if (_healthSlider != null)
        {
            _healthSlider.minValue = 0;
            _healthSlider.direction = Slider.Direction.LeftToRight;
            _healthSlider.transition = Selectable.Transition.None;
            _healthSlider.interactable = false;
            GameLog.Log("BossHealthBarUI: Slider configuré");
        }
        else
        {
            GameLog.LogWarning("BossHealthBarUI: _healthSlider est null!");
        }

        // Cache la barre au démarrage SEULEMENT si aucun boss n'est déjà tracké
        if (_container != null)
        {
            if (_trackedBoss == null)
            {
                _container.SetActive(false);
                GameLog.Log("BossHealthBarUI: Container caché au démarrage (aucun boss tracké)");
            }
            else
            {
                GameLog.Log("BossHealthBarUI: Container laissé visible (boss déjà tracké: " + _trackedBoss.name + ")");
            }
        }
        else
        {
            GameLog.LogWarning("BossHealthBarUI: _container est null! Assigne-le dans l'Inspector.");
        }
    }

    void OnDestroy()
    {
        // Désabonne des événements
        if (_trackedBoss != null)
        {
            _trackedBoss.OnHealthChanged -= UpdateHealth;
            _trackedBoss.OnUnitDied -= OnBossDied;
            _trackedBoss.OnStatsModified -= RefreshStatChips;
            _trackedBoss.OnMovementPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnActionPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnShieldChanged -= OnBossShieldChanged;
        }
    }

    /// <summary>
    /// Assigne le boss à tracker
    /// </summary>
    public void SetBoss(Enemy boss)
    {
        // Désabonne de l'ancien boss si présent
        if (_trackedBoss != null)
        {
            _trackedBoss.OnHealthChanged -= UpdateHealth;
            _trackedBoss.OnUnitDied -= OnBossDied;
            _trackedBoss.OnStatsModified -= RefreshStatChips;
            _trackedBoss.OnMovementPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnActionPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnShieldChanged -= OnBossShieldChanged;
        }

        _trackedBoss = boss;

        if (_trackedBoss != null && _trackedBoss.IsBoss())
        {
            // S'abonne aux événements
            _trackedBoss.OnHealthChanged += UpdateHealth;
            _trackedBoss.OnUnitDied += OnBossDied;
            _trackedBoss.OnStatsModified += RefreshStatChips;
            _trackedBoss.OnMovementPointsChanged += OnBossResourcesChanged;
            _trackedBoss.OnActionPointsChanged += OnBossResourcesChanged;
            _trackedBoss.OnShieldChanged += OnBossShieldChanged;

            // Affiche le container
            if (_container != null)
            {
                _container.SetActive(true);
            }

            // Affiche le nom du boss
            if (_bossNameText != null)
            {
                _bossNameText.text = _trackedBoss.GetEnemyData().enemyName;
            }

            // Initialise la barre de vie
            UpdateHealth(_trackedBoss.GetHealth(), _trackedBoss.GetMaxHealth());
            RefreshStatChips();

            // TODO: Charger le portrait si disponible
            // if (_bossPortrait != null && boss.GetEnemyData().portrait != null)
            // {
            //     _bossPortrait.sprite = boss.GetEnemyData().portrait;
            // }

            GameLog.Log($"BossHealthBarUI: Tracking {_trackedBoss.name}");
        }
        else
        {
            // Pas de boss ou pas flaggé comme boss, cache la barre
            if (_container != null)
            {
                _container.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Met à jour la barre de vie
    /// </summary>
    private void UpdateHealth(int currentHP, int maxHP)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHP;
            _healthSlider.value = currentHP;
        }

        if (_healthText != null)
        {
            _healthText.text = $"{currentHP}/{maxHP}";
        }
    }

    /// <summary>
    /// Pastilles PM / PA / attaque / armure / barrière / bouclier sous la barre (valeurs courantes)
    /// </summary>
    private void RefreshStatChips()
    {
        CardChipsView.Build(_statChipsContainer, CodexCardVisual.UnitChips(_trackedBoss), _chipBackground,
            _bossNameText != null ? _bossNameText.font : null, 20f, 22f);
    }

    private void OnBossShieldChanged(int shield) => RefreshStatChips();
    private void OnBossResourcesChanged(int current, int max) => RefreshStatChips();

    // PM/PA affichés : restants pendant le tour du boss, sinon ceux de son prochain tour (retraits compris)
    void OnEnable()
    {
        EventBus.Subscribe<ResourceDebuffChangedEvent>(OnResourceDebuffChanged);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<ResourceDebuffChangedEvent>(OnResourceDebuffChanged);
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    }

    private void OnResourceDebuffChanged(ResourceDebuffChangedEvent e)
    {
        if (e.Target == _trackedBoss) RefreshStatChips();
    }

    private void OnTurnChanged(TurnChangedEvent e)
    {
        if (_trackedBoss != null) RefreshStatChips();
    }

    /// <summary>
    /// Appelé quand le boss meurt
    /// </summary>
    private void OnBossDied(Unit boss)
    {
        GameLog.Log($"BossHealthBarUI: {boss.name} a été vaincu !");

        // Cache la barre après un court délai (pour l'effet visuel)
        Invoke(nameof(HideBossBar), 2f);
    }

    /// <summary>
    /// Cache la barre de boss
    /// </summary>
    public void HideBossBar()
    {
        if (_container != null)
        {
            _container.SetActive(false);
        }

        // Désabonne
        if (_trackedBoss != null)
        {
            _trackedBoss.OnHealthChanged -= UpdateHealth;
            _trackedBoss.OnUnitDied -= OnBossDied;
            _trackedBoss.OnStatsModified -= RefreshStatChips;
            _trackedBoss.OnMovementPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnActionPointsChanged -= OnBossResourcesChanged;
            _trackedBoss.OnShieldChanged -= OnBossShieldChanged;
            _trackedBoss = null;
        }
    }

}
