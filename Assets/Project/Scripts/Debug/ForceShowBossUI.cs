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
        GameLog.Log("=== FORCE CONNEXION UI ===");

        // Trouve le BattleUIManager
        if (!Services.IsBattleUIServiceAvailable())
        {
            Debug.LogError("IBattleUIService non enregistré!");
            return;
        }
        IBattleUIService manager = Services.BattleUI;

        // Trouve tous les ennemis
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        GameLog.Log($"Ennemis trouvés: {enemies.Length}");

        foreach (Enemy enemy in enemies)
        {
            GameLog.Log($"Ennemi: {enemy.name}");
            GameLog.Log($"  - Is Boss: {enemy.IsBoss()}");
            GameLog.Log($"  - EnemyData: {enemy.GetEnemyData()?.enemyName}");
            GameLog.Log($"  - HP: {enemy.GetHealth()}/{enemy.GetMaxHealth()}");

            if (enemy.GetEnemyData() != null)
            {
                GameLog.Log($"  - Is Boss (Data): {enemy.GetEnemyData().isBoss}");
            }

            // Force la connexion
            GameLog.Log($"Force la connexion de {enemy.name}...");
            manager.OnEnemySpawned(enemy);
        }

        GameLog.Log("=== FIN FORCE CONNEXION ===");
    }
}
