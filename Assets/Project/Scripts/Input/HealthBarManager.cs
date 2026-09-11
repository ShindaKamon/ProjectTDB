using UnityEngine;

public class HealthBarManager : MonoBehaviour, IHealthBarService
{
    [Header("References")]
    public Canvas healthBarsCanvas;
    public GameObject healthBarPrefab;

    void Awake()
    {
        if (ServiceLocator.Instance.IsRegistered<IHealthBarService>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Instance.Register<IHealthBarService>(this);

        // Trouve le canvas si non assigné
        if (healthBarsCanvas == null)
        {
            healthBarsCanvas = FindAnyObjectByType<Canvas>();
        }
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<IHealthBarService>();
    }

    public HealthBar CreateHealthBar(Transform target, Vector3 offset, Color color, int maxHP)
    {
        if (healthBarPrefab == null)
        {
            Debug.LogError("HealthBarManager: Le champ 'Health Bar Prefab' n'est pas assigné dans l'Inspector !");
            return null;
        }

        if (healthBarsCanvas == null)
        {
            Debug.LogError("HealthBarManager: Aucun Canvas trouvé pour afficher les barres de vie !");
            return null;
        }
        
        GameObject hbGO = Instantiate(healthBarPrefab, healthBarsCanvas.transform);
        HealthBar healthBar = hbGO.GetComponent<HealthBar>();
        
        healthBar.followTarget = target;
        healthBar.offset = offset;
        healthBar.SetColor(color);
        healthBar.UpdateHealth(maxHP, maxHP);
        
        return healthBar;
    }
}