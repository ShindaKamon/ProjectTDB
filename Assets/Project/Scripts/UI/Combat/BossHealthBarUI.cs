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

    [Header("Stats du boss (PM, attaque, armure, résistance magique, bouclier)")]
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
            _trackedBoss.OnShieldChanged += OnBossShieldChanged;

            // Affiche le container
            if (_container != null)
            {
                _container.SetActive(true);
            }

            // Affiche le nom du boss
            UpdateBossName();

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

        // Bouclier : toute la barre en bleu clair et son montant après les PV (comme les autres barres)
        int shield = _trackedBoss != null ? _trackedBoss.GetShield() : 0;
        if (_fillImage != null)
        {
            if (_fillBaseColor == null) _fillBaseColor = _fillImage.color;
            _fillImage.color = shield > 0 ? CodexCardVisual.ChipColor(ChipKind.Shield) : _fillBaseColor.Value;
        }

        if (_healthText != null)
        {
            _healthText.text = shield > 0 ? $"{currentHP}/{maxHP} +{shield}" : $"{currentHP}/{maxHP}";
        }
    }

    private Color? _fillBaseColor; // couleur de la barre sans bouclier (celle de la scène)

    /// <summary>
    /// Pastilles PM / attaque / armure / résistance magique / bouclier sous la barre (valeurs courantes)
    /// </summary>
    private void RefreshStatChips()
    {
        CardChipsView.Build(_statChipsContainer, CodexCardVisual.UnitChips(_trackedBoss), _chipBackground,
            _bossNameText != null ? _bossNameText.font : null, 20f, 22f);
    }

    private void OnBossShieldChanged(int shield)
    {
        RefreshStatChips();
        if (_trackedBoss != null) UpdateHealth(_trackedBoss.GetHealth(), _trackedBoss.GetMaxHealth());
    }
    private void OnBossResourcesChanged(int current, int max) => RefreshStatChips();

    // PM affichés : restants pendant le tour du boss, sinon ceux de son prochain tour (retraits compris)
    void OnEnable()
    {
        EventBus.Subscribe<ResourceDebuffChangedEvent>(OnResourceDebuffChanged);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<ResourceDebuffChangedEvent>(OnResourceDebuffChanged);
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
    }

    private void OnBossPhaseChanged(BossPhaseChangedEvent e)
    {
        if (e.Boss != _trackedBoss) return;
        UpdateBossName();

        // Bandeau au centre de l'écran : la barre repart pleine, le joueur doit comprendre pourquoi et quoi faire
        EnemyData.BossPhase phase = _trackedBoss.GetEnemyData().nextPhases[e.Phase - 1];
        ShowBanner(e.Phase, e.PhaseCount, phase.title, phase.objective);
    }

    private bool _startBannerShown;

    // Début du combat : bandeau de la première phase et son objectif (ex. les lits du Monstre sous le lit)
    private void ShowStartBanner()
    {
        if (_startBannerShown || _trackedBoss == null) return;
        _startBannerShown = true;
        EnemyData data = _trackedBoss.GetEnemyData();
        if (data != null && !string.IsNullOrEmpty(data.firstPhaseTitle))
            ShowBanner(0, _trackedBoss.PhaseCount, data.firstPhaseTitle, data.firstPhaseObjective);
    }

    private void ShowBanner(int phase, int phaseCount, string title, string objective)
    {
        string text = phaseCount > 1 ? $"Phase {phase + 1}/{phaseCount}" : "";
        if (!string.IsNullOrEmpty(title)) text += (text.Length > 0 ? "\n" : "") + title;
        if (!string.IsNullOrEmpty(objective)) text += $"\n<size=45%>{objective}</size>";
        if (_phaseBanner != null) StopCoroutine(_phaseBanner);
        _phaseBanner = StartCoroutine(ShowPhaseBanner(text, string.IsNullOrEmpty(objective) ? 2f : 4.5f));
    }

    private Coroutine _phaseBanner;
    private TextMeshProUGUI _phaseBannerText;
    private CanvasGroup _phaseBannerGroup; // bande sombre du bandeau, pour le fondu

    private System.Collections.IEnumerator ShowPhaseBanner(string text, float visibleSeconds)
    {
        if (_phaseBannerText == null)
        {
            // Créé à la demande sur le canvas de la barre, au centre de l'écran : une bande sombre sur toute la largeur
            // (lisible par-dessus le plateau), le texte dessus
            var band = new GameObject("PhaseBanner", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            band.transform.SetParent(GetComponentInParent<Canvas>().rootCanvas.transform, false);
            var bandRect = (RectTransform)band.transform;
            bandRect.anchorMin = new Vector2(0f, 0.6f);
            bandRect.anchorMax = new Vector2(1f, 0.6f);
            bandRect.pivot = new Vector2(0.5f, 0.5f);
            bandRect.sizeDelta = new Vector2(0f, 300f);
            var bandImage = band.GetComponent<Image>();
            bandImage.color = new Color(0f, 0f, 0f, 0.65f);
            bandImage.raycastTarget = false;
            _phaseBannerGroup = band.GetComponent<CanvasGroup>();
            _phaseBannerGroup.blocksRaycasts = false;

            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(bandRect, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1300f, 300f);
            _phaseBannerText = go.AddComponent<TextMeshProUGUI>();
            _phaseBannerText.alignment = TextAlignmentOptions.Center;
            _phaseBannerText.fontSize = 64f;
            _phaseBannerText.raycastTarget = false;
            if (_bossNameText != null) _phaseBannerText.font = _bossNameText.font;
        }

        _phaseBannerText.text = text;
        _phaseBannerGroup.gameObject.SetActive(true);
        for (float t = 0f; t < visibleSeconds + 0.5f; t += Time.deltaTime)
        {
            _phaseBannerGroup.alpha = t < visibleSeconds ? 1f : 1f - (t - visibleSeconds) / 0.5f; // visible, puis fondu
            yield return null;
        }
        _phaseBannerGroup.gameObject.SetActive(false);
        _phaseBanner = null;
    }

    // Nom du boss, suivi de sa phase s'il en a plusieurs (« Phase 2/3 ») : sa barre repart pleine à chaque phase
    private void UpdateBossName()
    {
        if (_bossNameText == null || _trackedBoss == null) return;
        string name = _trackedBoss.GetEnemyData().enemyName;
        _bossNameText.text = _trackedBoss.PhaseCount > 1
            ? $"{name}  —  Phase {_trackedBoss.Phase + 1}/{_trackedBoss.PhaseCount}"
            : name;
    }

    private void OnResourceDebuffChanged(ResourceDebuffChangedEvent e)
    {
        if (e.Target == _trackedBoss) RefreshStatChips();
    }

    private void OnTurnChanged(TurnChangedEvent e)
    {
        ShowStartBanner();
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
            _trackedBoss.OnShieldChanged -= OnBossShieldChanged;
            _trackedBoss = null;
        }
    }

}
