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
    [Tooltip("Aperçus (plus petits, sous celui du boss) de la prochaine carte des monstres ordinaires, un par monstre")]
    [SerializeField] private EnemyCardPreviewUI[] _minionCardPreviews;
    [SerializeField] private HealthOrbController _playerHealthOrb;

    [Header("Settings")]
    [SerializeField] private Color _defaultOrbColor = Color.red;

    private Enemy _currentTrackedEnemy;
    private Enemy _currentBoss;
    private readonly Dictionary<Enemy, EnemyCardPreviewUI> _minionPreviews = new Dictionary<Enemy, EnemyCardPreviewUI>();
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
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<IBattleUIService>();
    }

    /// <summary>
    /// Connecte un ennemi boss à la barre de vie de boss
    /// </summary>
    private void RegisterBoss(Enemy boss)
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
    private void TrackEnemyCards(Enemy enemy)
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
    private void SwitchTrackedEnemy(Enemy newEnemy)
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

        // Le boss a le grand aperçu ; chaque monstre ordinaire prend un petit aperçu libre
        if (enemy.IsBoss())
        {
            TrackEnemyCards(enemy);
            return;
        }
        if (TryTrackMinionCards(enemy)) return;

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

    // Petits aperçus des monstres ordinaires : empilés sous la carte du boss (ou à sa place sans boss), dans l'ordre
    // d'arrivée, sans trou ; un aperçu de plus est créé si les emplacements de la scène sont tous pris
    private readonly List<EnemyCardPreviewUI> _minionPool = new List<EnemyCardPreviewUI>();
    private readonly List<Enemy> _minionOrder = new List<Enemy>();
    private float _minionTop, _minionStep, _bossPreviewTop;

    private bool TryTrackMinionCards(Enemy enemy)
    {
        if (_minionCardPreviews == null || _minionCardPreviews.Length == 0) return false;
        if (_minionPool.Count == 0) InitMinionLayout();

        EnemyCardPreviewUI preview = _minionPool.Find(p => p != null && !_minionPreviews.ContainsValue(p));
        if (preview == null)
        {
            preview = Instantiate(_minionPool[0], _minionPool[0].transform.parent);
            preview.MarkBattleStarted(); // le combat a déjà commencé : pas d'attente du prochain tour
            _minionPool.Add(preview);
        }

        _minionPreviews[enemy] = preview;
        _minionOrder.Add(enemy);
        preview.SetTrackedEnemy(enemy);
        LayoutMinionPreviews();
        return true;
    }

    // Position des emplacements de la scène : le plus haut et l'écart entre deux, pour empiler les suivants
    private void InitMinionLayout()
    {
        var ys = new List<float>();
        foreach (EnemyCardPreviewUI preview in _minionCardPreviews)
        {
            if (preview == null) continue;
            _minionPool.Add(preview);
            ys.Add(((RectTransform)preview.transform).anchoredPosition.y);
        }
        ys.Sort((a, b) => b.CompareTo(a));
        _minionTop = ys[0];
        _minionStep = ys.Count > 1 ? ys[0] - ys[1] : 90f;
        _bossPreviewTop = _enemyCardPreview != null ? ((RectTransform)_enemyCardPreview.transform).anchoredPosition.y : _minionTop;
    }

    private void LayoutMinionPreviews()
    {
        float top = _currentBoss != null ? _minionTop : _bossPreviewTop; // sans boss, la place de sa carte est libre
        for (int i = 0; i < _minionOrder.Count; i++)
        {
            var rect = (RectTransform)_minionPreviews[_minionOrder[i]].transform;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, top - i * _minionStep);
        }
    }

    /// <summary>
    /// Nettoie les références quand un ennemi meurt
    /// </summary>
    public void OnEnemyDied(Enemy enemy)
    {
        if (enemy == null) return;

        // Monstre ordinaire : son petit aperçu disparaît
        if (_minionPreviews.TryGetValue(enemy, out EnemyCardPreviewUI minionPreview))
        {
            minionPreview.HidePreview();
            _minionPreviews.Remove(enemy);
            _minionOrder.Remove(enemy);
            LayoutMinionPreviews(); // les suivants remontent
            return;
        }

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

    void OnEnable() => EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    void OnDisable() => EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);

    // L'orbe de vie montre le champion dont c'est le tour (coop sur un PC : elle change à chaque tour ;
    // réseau : toujours le champion de ce PC, voir LocalView)
    private void OnTurnChanged(TurnChangedEvent e)
    {
        Champion champion = LocalView.ChampionToShow(e.NewActiveUnit);
        if (champion != null && champion != _currentPlayer) RegisterPlayer(champion);
    }

    /// <summary>
    /// Enregistre le joueur pour mettre à jour l'Orbe de vie
    /// </summary>
    private void RegisterPlayer(Champion player)
    {
        if (player == null) return;

        // Nettoyage si un joueur était déjà enregistré
        if (_currentPlayer != null)
        {
            _currentPlayer.OnHealthChanged -= OnPlayerHealthChanged;
            _currentPlayer.OnShieldChanged -= OnPlayerShieldChanged;
        }

        _currentPlayer = player;

        // Abonnements aux événements
        _currentPlayer.OnHealthChanged += OnPlayerHealthChanged;
        _currentPlayer.OnShieldChanged += OnPlayerShieldChanged;

        // Mise à jour initiale
        UpdatePlayerOrbUI();
        GameLog.Log($"BattleUIManager: Joueur {player.name} connecté à l'Orbe de vie");
    }

    private void OnPlayerHealthChanged(int current, int max) => UpdatePlayerOrbUI();

    private void OnPlayerShieldChanged(int shield) => UpdatePlayerOrbUI();

    private void UpdatePlayerOrbUI()
    {
        if (_currentPlayer == null) return;

        UpdatePlayerOrb(_currentPlayer.GetHealth(), _currentPlayer.GetMaxHealth(), _defaultOrbColor, _currentPlayer.GetShield());
    }

    private void UpdatePlayerOrb(float currentHP, float maxHP, Color emotionColor, float shield)
    {
        if (_playerHealthOrb != null)
        {
            _playerHealthOrb.UpdateHealth(currentHP, maxHP, emotionColor, shield);
        }
    }
}
