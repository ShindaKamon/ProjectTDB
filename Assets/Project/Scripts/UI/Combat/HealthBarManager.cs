using UnityEngine;

public class HealthBarManager : MonoBehaviour, IHealthBarService
{
    [Header("References")]
    [SerializeField] private Canvas _healthBarsCanvas;
    [SerializeField] private GameObject _healthBarPrefab;

    void Awake()
    {
        if (ServiceLocator.Instance.IsRegistered<IHealthBarService>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Instance.Register<IHealthBarService>(this);

        // Trouve le canvas si non assigné
        if (_healthBarsCanvas == null)
        {
            _healthBarsCanvas = FindAnyObjectByType<Canvas>();
        }
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<IHealthBarService>();
    }

    public HealthBar CreateHealthBar(Transform target, Vector3 offset, Color color, int maxHP)
    {
        if (_healthBarPrefab == null)
        {
            Debug.LogError("HealthBarManager: Le champ 'Health Bar Prefab' n'est pas assigné dans l'Inspector !");
            return null;
        }

        if (_healthBarsCanvas == null)
        {
            Debug.LogError("HealthBarManager: Aucun Canvas trouvé pour afficher les barres de vie !");
            return null;
        }

        GameObject hbGO = Instantiate(_healthBarPrefab, _healthBarsCanvas.transform);
        HealthBar healthBar = hbGO.GetComponent<HealthBar>();

        healthBar.SetFollowTarget(target, offset);
        healthBar.SetColor(color);
        healthBar.UpdateHealth(maxHP, maxHP);

        return healthBar;
    }
}