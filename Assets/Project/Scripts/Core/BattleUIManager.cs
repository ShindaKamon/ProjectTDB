using System.Collections.Generic;
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
    [SerializeField] private HealthOrbController _playerHealthOrb; //TEST ORB

    private Enemy _currentTrackedEnemy;
    private Enemy _currentBoss;
    private IlyaUnit _currentPlayer;
    private EmotionSystem _playerEmotionSystem;

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

        if (_playerHealthOrb == null)
        {
            _playerHealthOrb = ComponentLocator.FindSingleObjectOfType<HealthOrbController>("BatleUIManager setup");            
        }

        // Trouve automatiquement les UI si non assignées
        // OPTIMISATION Phase 3.3: ComponentLocator (découragé mais nécessaire pour setup initial)
        if (_bossHealthBar == null)
        {
            _bossHealthBar = ComponentLocator.FindSingleObjectOfType<BossHealthBarUI>("BattleUIManager setup");
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
            // OPTIMISATION Phase 3.3: ComponentLocator (découragé mais nécessaire pour setup initial)
            _enemyCardPreview = ComponentLocator.FindSingleObjectOfType<EnemyCardPreviewUI>("BattleUIManager setup");
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
    public void RegisterPlayer(IlyaUnit player)
    {
        if (player == null) return;

        // Nettoyage si un joueur était déjà enregistré
        if (_currentPlayer != null)
        {
            _currentPlayer.OnHealthChanged -= OnPlayerHealthChanged;
            if (_playerEmotionSystem != null)
            {
                _playerEmotionSystem.OnEmotionChanged -= OnPlayerEmotionChanged;
                _playerEmotionSystem.OnTransformationChanged -= OnPlayerTransformationChanged;
            }
        }

        _currentPlayer = player;
        _currentPlayer.TryGetComponent(out _playerEmotionSystem);

        // Abonnements aux événements
        _currentPlayer.OnHealthChanged += OnPlayerHealthChanged;
        
        if (_playerEmotionSystem != null)
        {
            _playerEmotionSystem.OnEmotionChanged += OnPlayerEmotionChanged;
            _playerEmotionSystem.OnTransformationChanged += OnPlayerTransformationChanged;
        }

        // Mise à jour initiale
        UpdatePlayerOrbUI();
        Debug.Log($"BattleUIManager: Joueur {player.name} connecté à l'Orbe de vie");
    }

    private void OnPlayerHealthChanged(int current, int max) => UpdatePlayerOrbUI();
    private void OnPlayerEmotionChanged(float current, float max) => UpdatePlayerOrbUI();
    private void OnPlayerTransformationChanged(EmotionSystem.TransformationState state) => UpdatePlayerOrbUI();

    private void UpdatePlayerOrbUI()
    {
        if (_currentPlayer == null) return;

        Color orbColor = Color.white;
        if (_playerEmotionSystem != null && _playerEmotionSystem.IsTransformed())
        {
            var transData = _playerEmotionSystem.GetCurrentState() == EmotionSystem.TransformationState.Positive 
                ? _playerEmotionSystem.GetPositiveTransformation() 
                : _playerEmotionSystem.GetNegativeTransformation();
            
            if (transData != null) orbColor = transData.glowColor;
        }

        UpdatePlayerOrb(_currentPlayer.GetHealth(), _currentPlayer.GetMaxHealth(), orbColor);
    }

    public void UpdatePlayerOrb (float currentHP, float maxHP, Color emotionColor)
    {
        if (_playerHealthOrb != null)
        {
            _playerHealthOrb.UpdateHealth(currentHP, maxHP, emotionColor);
        }
    }
}
