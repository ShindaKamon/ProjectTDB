using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }
    
    [Header("References")]
    public Canvas healthBarsCanvas;
    public GameObject healthBarPrefab;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Trouve le canvas si non assigné
        if (healthBarsCanvas == null)
        {
            healthBarsCanvas = FindAnyObjectByType<Canvas>();
        }
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