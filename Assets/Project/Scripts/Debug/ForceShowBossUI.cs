using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script temporaire pour forcer l'affichage des UI de boss.
/// Utilise ce script si les UI ne s'affichent pas automatiquement.
/// </summary>
public class ForceShowBossUI : MonoBehaviour
{
    [SerializeField] private float _delay = 1f; // Délai pour laisser tout s'initialiser

    void Start()
    {
        Invoke(nameof(ForceConnect), _delay);
    }

    void Update()
    {
        // Appuie sur F3 pour forcer l'affichage
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            ForceConnect();
        }
    }

    void ForceConnect()
    {
        Debug.Log("=== FORCE CONNEXION UI ===");

        // Trouve le BattleUIManager
        BattleUIManager manager = BattleUIManager.Instance;
        if (manager == null)
        {
            Debug.LogError("BattleUIManager introuvable!");
            return;
        }

        // Trouve tous les ennemis
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"Ennemis trouvés: {enemies.Length}");

        foreach (Enemy enemy in enemies)
        {
            Debug.Log($"Ennemi: {enemy.name}");
            Debug.Log($"  - Is Boss: {enemy.IsBoss()}");
            Debug.Log($"  - EnemyData: {enemy.GetEnemyData()?.enemyName}");
            Debug.Log($"  - HP: {enemy.GetHealth()}/{enemy.GetMaxHealth()}");

            if (enemy.GetEnemyData() != null)
            {
                Debug.Log($"  - Is Boss (Data): {enemy.GetEnemyData().isBoss}");
            }

            // Force la connexion
            Debug.Log($"Force la connexion de {enemy.name}...");
            manager.OnEnemySpawned(enemy);
        }

        Debug.Log("=== FIN FORCE CONNEXION ===");
    }
}
