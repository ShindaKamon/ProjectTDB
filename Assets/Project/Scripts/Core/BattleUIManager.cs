using UnityEngine;

/// <summary>
/// Gère la connexion automatique des UI de combat (Boss Health Bar, Enemy Card Preview)
/// aux ennemis présents dans la scène.
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private BossHealthBarUI _bossHealthBar;
    [SerializeField] private EnemyCardPreviewUI _enemyCardPreview;

    private Enemy _currentTrackedEnemy;
    private Enemy _currentBoss;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("BattleUIManager: Instance créée");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Trouve automatiquement les UI si non assignées
        if (_bossHealthBar == null)
        {
            _bossHealthBar = FindAnyObjectByType<BossHealthBarUI>();
            if (_bossHealthBar != null)
            {
                Debug.Log("BattleUIManager: BossHealthBarUI trouvée automatiquement");
            }
            else
            {
                Debug.LogWarning("BattleUIManager: BossHealthBarUI introuvable dans la scène!");
            }
        }
        if (_enemyCardPreview == null)
        {
            _enemyCardPreview = FindAnyObjectByType<EnemyCardPreviewUI>();
            if (_enemyCardPreview != null)
            {
                Debug.Log("BattleUIManager: EnemyCardPreviewUI trouvée automatiquement");
            }
            else
            {
                Debug.LogWarning("BattleUIManager: EnemyCardPreviewUI introuvable dans la scène!");
            }
        }
    }

    /// <summary>
    /// Connecte un ennemi boss à la barre de vie de boss
    /// </summary>
    public void RegisterBoss(Enemy boss)
    {
        if (boss == null || !boss.IsBoss()) return;

        _currentBoss = boss;

        if (_bossHealthBar != null)
        {
            _bossHealthBar.SetBoss(boss);
            Debug.Log($"BattleUIManager: Boss {boss.name} connecté à la barre de vie");
        }
        else
        {
            Debug.LogWarning("BattleUIManager: BossHealthBarUI non trouvée!");
        }
    }

    /// <summary>
    /// Connecte un ennemi à la preview de carte
    /// (généralement le premier ennemi trouvé ou l'ennemi actif)
    /// </summary>
    public void TrackEnemyCards(Enemy enemy)
    {
        if (enemy == null) return;

        _currentTrackedEnemy = enemy;

        if (_enemyCardPreview != null)
        {
            _enemyCardPreview.SetTrackedEnemy(enemy);
            Debug.Log($"BattleUIManager: Preview de cartes trackant {enemy.name}");
        }
        else
        {
            Debug.LogWarning("BattleUIManager: EnemyCardPreviewUI non trouvée!");
        }
    }

    /// <summary>
    /// Change l'ennemi tracké pour la preview de carte
    /// (utile quand on veut voir les cartes d'un ennemi spécifique)
    /// </summary>
    public void SwitchTrackedEnemy(Enemy newEnemy)
    {
        TrackEnemyCards(newEnemy);
    }

    /// <summary>
    /// Appelé automatiquement par GridManager quand un ennemi est initialisé
    /// </summary>
    public void OnEnemySpawned(Enemy enemy)
    {
        Debug.Log($"BattleUIManager.OnEnemySpawned() appelé pour {enemy?.name}");

        if (enemy == null)
        {
            Debug.LogWarning("BattleUIManager.OnEnemySpawned: enemy est null!");
            return;
        }

        Debug.Log($"  - enemy.IsBoss(): {enemy.IsBoss()}");
        Debug.Log($"  - _bossHealthBar existe: {_bossHealthBar != null}");
        Debug.Log($"  - _enemyCardPreview existe: {_enemyCardPreview != null}");
        Debug.Log($"  - _currentTrackedEnemy: {_currentTrackedEnemy?.name ?? "null"}");

        // Si c'est un boss, connecte la barre de vie
        if (enemy.IsBoss())
        {
            Debug.Log($"  -> C'est un boss, appel de RegisterBoss()");
            RegisterBoss(enemy);
        }
        else
        {
            Debug.Log($"  -> Ce n'est PAS un boss");
        }

        // Si aucun ennemi n'est tracké pour la preview, track celui-ci
        if (_currentTrackedEnemy == null)
        {
            Debug.Log($"  -> Aucun ennemi tracké, appel de TrackEnemyCards()");
            TrackEnemyCards(enemy);
        }
        else
        {
            Debug.Log($"  -> Un ennemi est déjà tracké: {_currentTrackedEnemy.name}");
        }
    }

    /// <summary>
    /// Nettoie les références quand un ennemi meurt
    /// </summary>
    public void OnEnemyDied(Enemy enemy)
    {
        if (enemy == null) return;

        // Si c'était le boss, cache la barre
        if (enemy == _currentBoss && _bossHealthBar != null)
        {
            _bossHealthBar.HideBossBar();
            _currentBoss = null;
        }

        // Si c'était l'ennemi tracké, trouve un autre ennemi
        if (enemy == _currentTrackedEnemy)
        {
            _currentTrackedEnemy = null;

            // Cherche un autre ennemi vivant
            Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy remainingEnemy in remainingEnemies)
            {
                if (remainingEnemy != enemy && remainingEnemy.GetHealth() > 0)
                {
                    TrackEnemyCards(remainingEnemy);
                    break;
                }
            }

            // Si aucun ennemi restant, cache la preview
            if (_currentTrackedEnemy == null && _enemyCardPreview != null)
            {
                _enemyCardPreview.HidePreview();
            }
        }
    }
}
