using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script de diagnostic pour vérifier que toutes les UI sont correctement configurées.
/// Attache ce script à n'importe quel GameObject dans la scène et lance le jeu pour voir le rapport.
/// </summary>
public class UIDebugChecker : MonoBehaviour
{
    [Header("Test au démarrage")]
    [SerializeField] private bool _checkOnStart = true;
    [SerializeField] private float _checkDelay = 2f; // Délai pour laisser tout s'initialiser

    void Start()
    {
        if (_checkOnStart)
        {
            Invoke(nameof(CheckAllUI), _checkDelay);
        }
    }

    void Update()
    {
        // Appuie sur F1 pour lancer le diagnostic
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            CheckAllUI();
        }
    }

    public void CheckAllUI()
    {
        Debug.Log("========================================");
        Debug.Log("=== DIAGNOSTIC UI BOSS & ENNEMI ===");
        Debug.Log("========================================");

        // 1. Vérifie BattleUIManager
        BattleUIManager battleUIManager = FindAnyObjectByType<BattleUIManager>();
        if (battleUIManager != null)
        {
            Debug.Log("✅ BattleUIManager trouvé: " + battleUIManager.gameObject.name);
        }
        else
        {
            Debug.LogError("❌ BattleUIManager INTROUVABLE! Crée un GameObject avec le script BattleUIManager.");
            return;
        }

        // 2. Vérifie BossHealthBarUI
        BossHealthBarUI bossUI = FindAnyObjectByType<BossHealthBarUI>();
        if (bossUI != null)
        {
            Debug.Log("✅ BossHealthBarUI trouvé: " + bossUI.gameObject.name);

            // Vérifie les références via réflexion
            var containerField = typeof(BossHealthBarUI).GetField("_container",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sliderField = typeof(BossHealthBarUI).GetField("_healthSlider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameTextField = typeof(BossHealthBarUI).GetField("_bossNameText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            GameObject container = containerField?.GetValue(bossUI) as GameObject;
            if (container != null)
            {
                Debug.Log("  ✅ Container assigné: " + container.name + " (Active: " + container.activeSelf + ")");
            }
            else
            {
                Debug.LogError("  ❌ Container NON ASSIGNÉ!");
            }

            if (sliderField?.GetValue(bossUI) != null)
            {
                Debug.Log("  ✅ Health Slider assigné");
            }
            else
            {
                Debug.LogWarning("  ⚠️ Health Slider non assigné");
            }

            if (nameTextField?.GetValue(bossUI) != null)
            {
                Debug.Log("  ✅ Boss Name Text assigné");
            }
            else
            {
                Debug.LogWarning("  ⚠️ Boss Name Text non assigné");
            }
        }
        else
        {
            Debug.LogError("❌ BossHealthBarUI INTROUVABLE! Ajoute le script à un Panel UI.");
        }

        // 3. Vérifie EnemyCardPreviewUI
        EnemyCardPreviewUI previewUI = FindAnyObjectByType<EnemyCardPreviewUI>();
        if (previewUI != null)
        {
            Debug.Log("✅ EnemyCardPreviewUI trouvé: " + previewUI.gameObject.name);

            var containerField = typeof(EnemyCardPreviewUI).GetField("_previewContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameTextField = typeof(EnemyCardPreviewUI).GetField("_cardNameText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            GameObject container = containerField?.GetValue(previewUI) as GameObject;
            if (container != null)
            {
                Debug.Log("  ✅ Container assigné: " + container.name + " (Active: " + container.activeSelf + ")");
            }
            else
            {
                Debug.LogError("  ❌ Container NON ASSIGNÉ!");
            }

            if (nameTextField?.GetValue(previewUI) != null)
            {
                Debug.Log("  ✅ Card Name Text assigné");
            }
            else
            {
                Debug.LogWarning("  ⚠️ Card Name Text non assigné");
            }
        }
        else
        {
            Debug.LogError("❌ EnemyCardPreviewUI INTROUVABLE! Ajoute le script à un Panel UI.");
        }

        // 4. Vérifie les ennemis
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"\n📊 Ennemis trouvés: {enemies.Length}");

        foreach (Enemy enemy in enemies)
        {
            Debug.Log($"  - {enemy.name}:");
            Debug.Log($"    • Est Boss: {enemy.IsBoss()}");
            Debug.Log($"    • HP: {enemy.GetHealth()}/{enemy.GetMaxHealth()}");
            Debug.Log($"    • PA: {enemy.GetCurrentPA()}/{enemy.GetMaxPA()}");

            CardData nextCard = enemy.GetNextCard();
            if (nextCard != null)
            {
                Debug.Log($"    • Prochaine carte: {nextCard.cardName}");
            }
            else
            {
                Debug.LogWarning($"    ⚠️ Pas de carte dans le deck!");
            }
        }

        // 5. Résumé
        Debug.Log("\n========================================");
        Debug.Log("=== RÉSUMÉ ===");
        if (battleUIManager != null && bossUI != null && previewUI != null)
        {
            Debug.Log("✅ Tous les scripts UI sont présents");

            if (enemies.Length == 0)
            {
                Debug.LogWarning("⚠️ Aucun ennemi dans la scène! Glisse un prefab Enemy pour tester.");
            }
            else
            {
                bool hasBoss = false;
                bool hasCards = false;
                foreach (Enemy enemy in enemies)
                {
                    if (enemy.IsBoss()) hasBoss = true;
                    if (enemy.GetNextCard() != null) hasCards = true;
                }

                if (!hasBoss)
                {
                    Debug.LogWarning("⚠️ Aucun boss trouvé. Active 'Is Boss' dans l'EnemyData pour tester la barre de vie boss.");
                }
                if (!hasCards)
                {
                    Debug.LogWarning("⚠️ Aucun ennemi n'a de cartes. Ajoute des cartes au Combat Deck dans l'EnemyData.");
                }
            }
        }
        else
        {
            Debug.LogError("❌ Configuration UI incomplète! Suis le guide QUICKSTART.md");
        }
        Debug.Log("========================================\n");
    }
}
