using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Système de visualisation de la vie avec des orbes/sphères.
/// Alternative aux barres de vie classiques.
/// Pattern: Observer Pattern
/// Responsabilités:
/// - Afficher la vie sous forme d'orbes rouges
/// - Modes: Radial (1 orbe qui se vide), Multiple (plusieurs petites orbes)
/// - Animation smooth des changements de vie
/// - Follow l'unité en world-to-screen
/// </summary>
public class HealthOrbUI : MonoBehaviour
{
    // ========== MODES ==========

    public enum DisplayMode
    {
        Radial,    // 1 orbe qui se remplit/vide (fill radial)
        Multiple   // Plusieurs petites orbes (1 orbe = X HP)
    }

    // ========== CONFIGURATION ==========

    [Header("Mode")]
    [SerializeField] private DisplayMode _displayMode = DisplayMode.Radial;

    [Header("Radial Mode Settings")]
    [SerializeField] private Image _orbFillImage;              // Image avec type "Filled" (Radial 360)
    [SerializeField] private Image _orbBackgroundImage;        // Image de fond
    [SerializeField] private Color _fullHealthColor = new Color(1f, 0.2f, 0.2f);    // Rouge vif
    [SerializeField] private Color _mediumHealthColor = new Color(1f, 0.6f, 0f);    // Orange
    [SerializeField] private Color _lowHealthColor = new Color(0.8f, 0.1f, 0.1f);   // Rouge foncé
    [SerializeField] private float _lowHealthThreshold = 0.3f; // < 30% = low health

    [Header("Multiple Orbs Mode Settings")]
    [SerializeField] private GameObject _smallOrbPrefab;       // Prefab d'une petite orbe
    [SerializeField] private Transform _orbsContainer;         // Container pour les orbes
    [SerializeField] private int _hpPerOrb = 10;               // 1 orbe = 10 HP
    [SerializeField] private float _orbSpacing = 0.2f;         // Espacement entre orbes
    [SerializeField] private int _maxOrbsDisplayed = 10;       // Max d'orbes affichées

    [Header("Follow Settings")]
    [SerializeField] private Transform _followTarget;          // Unité à suivre
    [SerializeField] private Vector3 _offset = new Vector3(0, 2.5f, 0);
    [SerializeField] private bool _smoothFollow = true;
    [SerializeField] private float _followSpeed = 10f;

    [Header("Animation")]
    [SerializeField] private float _fillSpeed = 5f;            // Vitesse de remplissage/vidage
    [SerializeField] private bool _animateColorChange = true;

    // ========== ÉTAT ==========

    private Unit _trackedUnit;
    private int _currentHealth;
    private int _maxHealth;
    private float _targetFillAmount;
    private float _currentFillAmount;
    private List<GameObject> _spawnedOrbs = new List<GameObject>();

    // ========== INITIALISATION ==========

    void Awake()
    {
        // Validation
        if (_displayMode == DisplayMode.Radial && _orbFillImage == null)
        {
            Debug.LogError("HealthOrbUI: _orbFillImage non assigné en mode Radial!");
        }

        if (_displayMode == DisplayMode.Multiple && _smallOrbPrefab == null)
        {
            Debug.LogError("HealthOrbUI: _smallOrbPrefab non assigné en mode Multiple!");
        }

        if (_displayMode == DisplayMode.Multiple && _orbsContainer == null)
        {
            // Crée un container automatiquement
            GameObject container = new GameObject("OrbsContainer");
            container.transform.SetParent(transform, false);
            _orbsContainer = container.transform;
        }
    }

    void Start()
    {
        // Initialise l'affichage
        if (_trackedUnit != null)
        {
            UpdateHealth(_trackedUnit.GetHealth(), _trackedUnit.GetMaxHealth());
        }
    }

    // ========== API PUBLIQUE ==========

    /// <summary>
    /// Assigne l'unité à suivre
    /// </summary>
    public void SetUnit(Unit unit)
    {
        // Désabonne de l'ancienne unité
        if (_trackedUnit != null)
        {
            _trackedUnit.OnHealthChanged -= OnHealthChangedHandler;
        }

        _trackedUnit = unit;
        _followTarget = unit != null ? unit.transform : null;

        if (_trackedUnit != null)
        {
            // S'abonne aux changements de vie
            _trackedUnit.OnHealthChanged += OnHealthChangedHandler;

            // Initialise l'affichage
            UpdateHealth(_trackedUnit.GetHealth(), _trackedUnit.GetMaxHealth());
        }
    }

    /// <summary>
    /// Change le mode d'affichage
    /// </summary>
    public void SetDisplayMode(DisplayMode mode)
    {
        if (_displayMode == mode) return;

        _displayMode = mode;

        // Réinitialise l'affichage
        if (_trackedUnit != null)
        {
            UpdateHealth(_trackedUnit.GetHealth(), _trackedUnit.GetMaxHealth());
        }
    }

    /// <summary>
    /// Change la couleur de l'orbe (radial mode)
    /// </summary>
    public void SetColor(Color color)
    {
        if (_orbFillImage != null)
        {
            _orbFillImage.color = color;
        }
    }

    // ========== EVENT HANDLERS ==========

    /// <summary>
    /// Handler pour l'événement OnHealthChanged de Unit (Action<int, int>)
    /// </summary>
    private void OnHealthChangedHandler(int currentHealth, int maxHealth)
    {
        UpdateHealth(currentHealth, maxHealth);
    }

    // ========== AFFICHAGE ==========

    /// <summary>
    /// Met à jour l'affichage de la vie
    /// </summary>
    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;

        if (_displayMode == DisplayMode.Radial)
        {
            UpdateRadialDisplay();
        }
        else
        {
            UpdateMultipleOrbsDisplay();
        }
    }

    /// <summary>
    /// Mode Radial : 1 orbe qui se remplit/vide
    /// </summary>
    private void UpdateRadialDisplay()
    {
        if (_orbFillImage == null) return;

        // Calcule le fill amount cible (0.0 à 1.0)
        _targetFillAmount = _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;

        // Met à jour immédiatement si pas d'animation
        if (!_smoothFollow)
        {
            _currentFillAmount = _targetFillAmount;
            _orbFillImage.fillAmount = _currentFillAmount;
        }

        // Met à jour la couleur selon la vie restante
        if (_animateColorChange)
        {
            UpdateOrbColor();
        }
    }

    /// <summary>
    /// Mode Multiple : Plusieurs petites orbes
    /// </summary>
    private void UpdateMultipleOrbsDisplay()
    {
        if (_smallOrbPrefab == null || _orbsContainer == null) return;

        // Calcule le nombre d'orbes nécessaires
        int totalOrbs = Mathf.CeilToInt((float)_maxHealth / _hpPerOrb);
        int filledOrbs = Mathf.FloorToInt((float)_currentHealth / _hpPerOrb);
        float partialFill = ((float)_currentHealth % _hpPerOrb) / _hpPerOrb;

        // Limite le nombre d'orbes affichées
        totalOrbs = Mathf.Min(totalOrbs, _maxOrbsDisplayed);

        // Crée/détruit des orbes si nécessaire
        while (_spawnedOrbs.Count < totalOrbs)
        {
            GameObject orb = Instantiate(_smallOrbPrefab, _orbsContainer);
            _spawnedOrbs.Add(orb);

            // Position horizontale
            RectTransform rt = orb.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(_spawnedOrbs.Count * _orbSpacing * 100f, 0);
            }
        }

        while (_spawnedOrbs.Count > totalOrbs)
        {
            GameObject toRemove = _spawnedOrbs[_spawnedOrbs.Count - 1];
            _spawnedOrbs.RemoveAt(_spawnedOrbs.Count - 1);
            Destroy(toRemove);
        }

        // Met à jour chaque orbe
        for (int i = 0; i < _spawnedOrbs.Count; i++)
        {
            GameObject orb = _spawnedOrbs[i];
            Image orbImage = orb.GetComponent<Image>();

            if (orbImage != null)
            {
                if (i < filledOrbs)
                {
                    // Orbe complètement remplie
                    orbImage.fillAmount = 1f;
                    orbImage.color = _fullHealthColor;
                }
                else if (i == filledOrbs)
                {
                    // Orbe partiellement remplie
                    orbImage.fillAmount = partialFill;
                    orbImage.color = Color.Lerp(_lowHealthColor, _fullHealthColor, partialFill);
                }
                else
                {
                    // Orbe vide
                    orbImage.fillAmount = 0f;
                    orbImage.color = Color.gray;
                }
            }
        }
    }

    /// <summary>
    /// Met à jour la couleur de l'orbe selon la vie restante
    /// </summary>
    private void UpdateOrbColor()
    {
        if (_orbFillImage == null) return;

        float healthPercent = _targetFillAmount;

        if (healthPercent <= _lowHealthThreshold)
        {
            // Vie basse : rouge foncé
            _orbFillImage.color = _lowHealthColor;
        }
        else if (healthPercent <= 0.6f)
        {
            // Vie moyenne : orange
            _orbFillImage.color = _mediumHealthColor;
        }
        else
        {
            // Vie haute : rouge vif
            _orbFillImage.color = _fullHealthColor;
        }
    }

    // ========== UPDATE ==========

    void Update()
    {
        // Animation du fill amount (mode Radial)
        if (_displayMode == DisplayMode.Radial && _orbFillImage != null)
        {
            if (Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
            {
                _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _fillSpeed);
                _orbFillImage.fillAmount = _currentFillAmount;
            }
        }

        // Follow l'unité
        if (_followTarget != null)
        {
            UpdateFollowPosition();
        }
    }

    /// <summary>
    /// Met à jour la position pour suivre l'unité
    /// </summary>
    private void UpdateFollowPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 worldPos = _followTarget.position + _offset;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPos);

        // Vérifie si derrière la caméra
        if (screenPoint.z < 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Convertit en canvas space
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out canvasPosition
        );

        RectTransform rectTransform = GetComponent<RectTransform>();

        if (_smoothFollow)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                canvasPosition,
                Time.deltaTime * _followSpeed
            );
        }
        else
        {
            rectTransform.anchoredPosition = canvasPosition;
        }
    }

    // ========== CLEANUP ==========

    void OnDestroy()
    {
        if (_trackedUnit != null)
        {
            _trackedUnit.OnHealthChanged -= OnHealthChangedHandler;
        }

        // Nettoie les orbes spawnées
        foreach (GameObject orb in _spawnedOrbs)
        {
            if (orb != null)
            {
                Destroy(orb);
            }
        }
        _spawnedOrbs.Clear();
    }
}
