using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire centralisé des HealthOrbUI.
/// Responsabilités:
/// - Créer automatiquement des orbes de vie pour chaque unité
/// - Gérer la durée de vie des orbes
/// - Permettre de basculer entre barres et orbes
/// - Nettoyer les orbes des unités mortes
/// </summary>
public class HealthOrbManager : MonoBehaviour
{
    // ========== SINGLETON ==========

    private static HealthOrbManager _instance;
    public static HealthOrbManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<HealthOrbManager>();
                if (_instance == null)
                {
                    Debug.LogWarning("HealthOrbManager: Instance non trouvée dans la scène!");
                }
            }
            return _instance;
        }
    }

    // ========== CONFIGURATION ==========

    [Header("Prefab")]
    [SerializeField] private GameObject _healthOrbPrefab;

    [Header("Canvas")]
    [SerializeField] private Canvas _healthOrbCanvas;

    [Header("Display Mode")]
    [SerializeField] private HealthOrbUI.DisplayMode _defaultDisplayMode = HealthOrbUI.DisplayMode.Radial;

    [Header("Toggle")]
    [SerializeField] private bool _useOrbsInsteadOfBars = true;  // Toggle entre orbes et barres

    [Header("Auto Setup")]
    [SerializeField] private bool _autoCreateForAllUnits = true;  // Crée automatiquement pour toutes les unités

    // ========== ÉTAT ==========

    private Dictionary<Unit, HealthOrbUI> _activeOrbs = new Dictionary<Unit, HealthOrbUI>();
    private Transform _orbsContainer;

    // ========== INITIALISATION ==========

    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Trouve le canvas automatiquement si non assigné
        if (_healthOrbCanvas == null)
        {
            _healthOrbCanvas = ComponentLocator.FindSingleObjectOfType<Canvas>("HealthOrbManager: Recherche Canvas");
            if (_healthOrbCanvas != null)
            {
                Debug.Log("HealthOrbManager: Canvas trouvé automatiquement");
            }
            else
            {
                Debug.LogError("HealthOrbManager: Aucun Canvas trouvé dans la scène!");
            }
        }

        // Crée un container pour les orbes
        if (_healthOrbCanvas != null)
        {
            GameObject containerObj = new GameObject("HealthOrbsContainer");
            containerObj.transform.SetParent(_healthOrbCanvas.transform, false);
            _orbsContainer = containerObj.transform;
        }
    }

    void Start()
    {
        // Crée automatiquement des orbes pour toutes les unités existantes
        if (_autoCreateForAllUnits)
        {
            CreateOrbsForAllUnits();
        }
    }

    void OnEnable()
    {
        // Écoute les événements d'unités
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    // ========== AUTO SETUP ==========

    /// <summary>
    /// Crée des orbes pour toutes les unités présentes dans la scène
    /// </summary>
    private void CreateOrbsForAllUnits()
    {
        if (!_useOrbsInsteadOfBars) return;

        // Utilise GridRepository pour obtenir toutes les unités (optimisation Phase 3.1)
        if (!Services.IsGridServiceAvailable()) return;

        List<Unit> allUnits = Services.Grid.GetAllUnits();

        foreach (Unit unit in allUnits)
        {
            CreateOrbForUnit(unit);
        }

        Debug.Log($"HealthOrbManager: {_activeOrbs.Count} orbes créées pour les unités");
    }

    // ========== API PUBLIQUE ==========

    /// <summary>
    /// Crée une orbe de vie pour une unité
    /// </summary>
    public HealthOrbUI CreateOrbForUnit(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("HealthOrbManager: Unité null passée à CreateOrbForUnit");
            return null;
        }

        // Si l'orbe existe déjà, la retourne
        if (_activeOrbs.ContainsKey(unit))
        {
            return _activeOrbs[unit];
        }

        if (_healthOrbPrefab == null)
        {
            Debug.LogError("HealthOrbManager: _healthOrbPrefab n'est pas assigné!");
            return null;
        }

        if (_orbsContainer == null)
        {
            Debug.LogError("HealthOrbManager: _orbsContainer est null!");
            return null;
        }

        // Instancie l'orbe
        GameObject orbObj = Instantiate(_healthOrbPrefab, _orbsContainer);
        orbObj.name = $"HealthOrb_{unit.name}";

        HealthOrbUI orbUI = orbObj.GetComponent<HealthOrbUI>();
        if (orbUI == null)
        {
            Debug.LogError("HealthOrbManager: Le prefab n'a pas de composant HealthOrbUI!");
            Destroy(orbObj);
            return null;
        }

        // Configure l'orbe
        orbUI.SetUnit(unit);
        orbUI.SetDisplayMode(_defaultDisplayMode);

        // Enregistre l'orbe
        _activeOrbs[unit] = orbUI;

        return orbUI;
    }

    /// <summary>
    /// Supprime l'orbe d'une unité
    /// </summary>
    public void RemoveOrbForUnit(Unit unit)
    {
        if (unit == null || !_activeOrbs.ContainsKey(unit)) return;

        HealthOrbUI orb = _activeOrbs[unit];
        _activeOrbs.Remove(unit);

        if (orb != null)
        {
            Destroy(orb.gameObject);
        }
    }

    /// <summary>
    /// Récupère l'orbe associée à une unité
    /// </summary>
    public HealthOrbUI GetOrbForUnit(Unit unit)
    {
        if (unit == null || !_activeOrbs.ContainsKey(unit)) return null;
        return _activeOrbs[unit];
    }

    /// <summary>
    /// Change le mode d'affichage pour toutes les orbes
    /// </summary>
    public void SetDisplayModeForAll(HealthOrbUI.DisplayMode mode)
    {
        _defaultDisplayMode = mode;

        foreach (HealthOrbUI orb in _activeOrbs.Values)
        {
            if (orb != null)
            {
                orb.SetDisplayMode(mode);
            }
        }
    }

    /// <summary>
    /// Toggle entre orbes et barres de vie
    /// </summary>
    public void ToggleOrbsVsBars()
    {
        _useOrbsInsteadOfBars = !_useOrbsInsteadOfBars;

        if (_useOrbsInsteadOfBars)
        {
            // Active les orbes
            CreateOrbsForAllUnits();

            // Désactive les barres de vie (si HealthBar.cs est utilisé)
            DisableAllHealthBars();
        }
        else
        {
            // Désactive les orbes
            RemoveAllOrbs();

            // Réactive les barres de vie
            EnableAllHealthBars();
        }

        Debug.Log($"HealthOrbManager: Mode changé → {(_useOrbsInsteadOfBars ? "Orbes" : "Barres")}");
    }

    /// <summary>
    /// Supprime toutes les orbes
    /// </summary>
    public void RemoveAllOrbs()
    {
        foreach (HealthOrbUI orb in _activeOrbs.Values)
        {
            if (orb != null)
            {
                Destroy(orb.gameObject);
            }
        }

        _activeOrbs.Clear();
    }

    // ========== EVENT HANDLERS ==========

    private void OnUnitDied(UnitDiedEvent evt)
    {
        // Supprime l'orbe de l'unité morte
        if (evt.DeadUnit != null)
        {
            RemoveOrbForUnit(evt.DeadUnit);
        }
    }

    // ========== HEALTH BAR INTEGRATION ==========

    /// <summary>
    /// Désactive toutes les barres de vie (HealthBar.cs)
    /// </summary>
    private void DisableAllHealthBars()
    {
        HealthBar[] healthBars = FindObjectsByType<HealthBar>(FindObjectsSortMode.None);
        foreach (HealthBar bar in healthBars)
        {
            bar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Réactive toutes les barres de vie
    /// </summary>
    private void EnableAllHealthBars()
    {
        HealthBar[] healthBars = FindObjectsByType<HealthBar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (HealthBar bar in healthBars)
        {
            bar.gameObject.SetActive(true);
        }
    }

    // ========== CLEANUP ==========

    void OnDestroy()
    {
        RemoveAllOrbs();
    }

    // ========== API STATIQUE ==========

    /// <summary>
    /// Crée rapidement une orbe pour une unité (raccourci)
    /// </summary>
    public static HealthOrbUI CreateOrb(Unit unit)
    {
        if (Instance == null) return null;
        return Instance.CreateOrbForUnit(unit);
    }

    /// <summary>
    /// Supprime rapidement l'orbe d'une unité (raccourci)
    /// </summary>
    public static void RemoveOrb(Unit unit)
    {
        if (Instance == null) return;
        Instance.RemoveOrbForUnit(unit);
    }
}
