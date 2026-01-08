using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;
    
    [Header("Health Bar Settings")]
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public Color healthBarColor = Color.green;
    
    private HealthBar healthBar;
    
    void Start()
    {
        currentHP = maxHP;
        CreateHealthBar();
    }
    
    void CreateHealthBar()
    {
        // Utilise le manager
        if (HealthBarManager.Instance == null)
        {
            Debug.LogError("HealthBarManager non trouvé dans la scène !");
            return;
        }
        
        healthBar = HealthBarManager.Instance.CreateHealthBar(
            transform,
            healthBarOffset,
            healthBarColor,
            maxHP
        );
    }
    
    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHP, maxHP);
        }
        
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHP, maxHP);
        }
    }
    
    void Die()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
        
        Debug.Log($"{gameObject.name} est mort !");
        // Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
    }
}