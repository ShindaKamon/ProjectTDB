using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la connexion automatique des UI de combat (Boss Health Bar, Enemy Card Preview)
/// aux ennemis présents dans la scène.
/// </summary>
public class BattleUIManager : MonoBehaviour, IBattleUIService
{
    [Header("UI References")]
    [SerializeField] private BossHealthBarUI _bossHealthBar;
    [SerializeField] private EnemyCardPreviewUI _enemyCardPreview;
    [SerializeField] private HealthOrbController _playerHealthOrb;

    [Header("Settings")]
    [SerializeField] private Color _defaultOrbColor = Color.red;

    private Enemy _currentTrackedEnemy;
    private Enemy _currentBoss;
    private Champion _currentPlayer;

    void Awake()
    {
        if (ServiceLocator.Instance.IsRegistered<IBattleUIService>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Instance.Register<IBattleUIService>(this);
        GameLog.Log("BattleUIManager: Enregistré dans ServiceLocator comme IBattleUIService");

        if (_playerHealthOrb == null)
        {
            _playerHealthOrb = ComponentLocator.FindSingleObjectOfType<HealthOrbController>("BatleUIManager setup");            
        }

        // Récupère la couleur initiale de l'orbe pour ne pas l'écraser avec du blanc/rouge par défaut
        if (_playerHealthOrb != null)
        {
            // La couleur est celle de la jauge (l'image « Filled » de l'orbe)
            Image orbImage = null;
            foreach (var img in _playerHealthOrb.GetComponentsInChildren<Image>(true))
            {
                if (img.type == Image.Type.Filled)
                {
                    orbImage = img;
                    break;
                }
            }

            if (orbImage != null)
            {
                _defaultOrbColor = orbImage.color;
                GameLog.Log($"BattleUIManager: Couleur initiale de l'orbe détectée sur '{orbImage.name}': {_defaultOrbColor}");
            }
        }

        // Trouve automatiquement les UI si non assignées
        // OPTIMISATION Phase 3.3: ComponentLocator (découragé mais nécessaire pour setup initial)
        if (_bossHealthBar == null)
        {
            _bossHealthBar = ComponentLocator.FindSingleObjectOfType<BossHealthBarUI>("BattleUIManager setup");
            if (_bossHealthBar != null)
            {
                GameLog.Log("BattleUIManager: BossHealthBarUI trouvée automatiquement");
            }
            else
            {
                GameLog.LogWarning("BattleUIManager: BossHealthBarUI introuvable dans la scène!");
            }
        }
        if (_enemyCardPreview == null)
        {
            // OPTIMISATION Phase 3.3: ComponentLocator (découragé mais nécessaire pour setup initial)
            _enemyCardPreview = ComponentLocator.FindSingleObjectOfType<EnemyCardPreviewUI>("BattleUIManager setup");
            if (_enemyCardPreview != null)
            {
                GameLog.Log("BattleUIManager: EnemyCardPreviewUI trouvée automatiquement");
            }
            else
            {
                GameLog.LogWarning("BattleUIManager: EnemyCardPreviewUI introuvable dans la scène!");
            }
        }
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<IBattleUIService>();
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
            GameLog.Log($"BattleUIManager: Boss {boss.name} connecté à la barre de vie");
        }
        else
        {
            GameLog.LogWarning("BattleUIManager: BossHealthBarUI non trouvée!");
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
            GameLog.Log($"BattleUIManager: Preview de cartes trackant {enemy.name}");
        }
        else
        {
            GameLog.LogWarning("BattleUIManager: EnemyCardPreviewUI non trouvée!");
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
        GameLog.Log($"BattleUIManager.OnEnemySpawned() appelé pour {enemy?.name}");

        if (enemy == null)
        {
            GameLog.LogWarning("BattleUIManager.OnEnemySpawned: enemy est null!");
            return;
        }

        GameLog.Log($"  - enemy.IsBoss(): {enemy.IsBoss()}");
        GameLog.Log($"  - _bossHealthBar existe: {_bossHealthBar != null}");
        GameLog.Log($"  - _enemyCardPreview existe: {_enemyCardPreview != null}");
        GameLog.Log($"  - _currentTrackedEnemy: {_currentTrackedEnemy?.name ?? "null"}");

        // Si c'est un boss, connecte la barre de vie
        if (enemy.IsBoss())
        {
            GameLog.Log($"  -> C'est un boss, appel de RegisterBoss()");
            RegisterBoss(enemy);
        }
        else
        {
            GameLog.Log($"  -> Ce n'est PAS un boss");
        }

        // Si aucun ennemi n'est tracké pour la preview, track celui-ci
        if (_currentTrackedEnemy == null)
        {
            GameLog.Log($"  -> Aucun ennemi tracké, appel de TrackEnemyCards()");
            TrackEnemyCards(enemy);
        }
        else
        {
            GameLog.Log($"  -> Un ennemi est déjà tracké: {_currentTrackedEnemy.name}");
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

            // Cherche un autre ennemi vivant via GridRepository (OPTIMISATION: pas de FindObjectsByType)
            // Vérifie que le service est disponible (évite erreurs lors de la destruction de scène)
            if (Services.IsGridServiceAvailable())
            {
                List<Unit> allEnemies = Services.Grid.GetAllEnemyUnits();
                foreach (Unit enemyUnit in allEnemies)
                {
                    Enemy remainingEnemy = enemyUnit as Enemy;
                    if (remainingEnemy != null && remainingEnemy != enemy && remainingEnemy.GetHealth() > 0)
                    {
                        TrackEnemyCards(remainingEnemy);
                        break;
                    }
                }
            }

            // Si aucun ennemi restant, cache la preview
            if (_currentTrackedEnemy == null && _enemyCardPreview != null)
            {
                _enemyCardPreview.HidePreview();
            }
        }
    }

    /// <summary>
    /// Enregistre le joueur pour mettre à jour l'Orbe de vie
    /// </summary>
    public void RegisterPlayer(Champion player)
    {
        if (player == null) return;

        // Nettoyage si un joueur était déjà enregistré
        if (_currentPlayer != null)
        {
            _currentPlayer.OnHealthChanged -= OnPlayerHealthChanged;
        }

        _currentPlayer = player;

        // Abonnements aux événements
        _currentPlayer.OnHealthChanged += OnPlayerHealthChanged;

        // Mise à jour initiale
        UpdatePlayerOrbUI();
        GameLog.Log($"BattleUIManager: Joueur {player.name} connecté à l'Orbe de vie");
    }

    private void OnPlayerHealthChanged(int current, int max) => UpdatePlayerOrbUI();

    private void UpdatePlayerOrbUI()
    {
        if (_currentPlayer == null) return;

        UpdatePlayerOrb(_currentPlayer.GetHealth(), _currentPlayer.GetMaxHealth(), _defaultOrbColor);
    }

    public void UpdatePlayerOrb (float currentHP, float maxHP, Color emotionColor)
    {
        if (_playerHealthOrb != null)
        {
            _playerHealthOrb.UpdateHealth(currentHP, maxHP, emotionColor);
        }
    }
}
