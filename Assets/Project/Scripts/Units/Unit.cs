using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Nouvelle énumération pour les factions (Joueur ou Ennemi).
    public enum UnitFaction { Player, Enemy }

    // La position actuelle de l'unité sur la grille (coordonnées X, Y).
    [SerializeField] protected Vector2 _currentGridPos;
    // La position de grille initiale de l'unité, configurable dans l'Inspector.
    [SerializeField] protected Vector2 _initialGridPos = new Vector2(0, 0); // Par défaut à (0,0).
    // Vitesse de déplacement de l'unité.
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Color healthBarColor = Color.green;

    private HealthBar healthBar;

    // Variables pour le déplacement fluide.
    private Vector3 _targetWorldPosition; // La position mondiale cible de l'unité.
    private bool _isMoving = false; // Indique si l'unité est en cours de déplacement.
    private List<Tile> _path; // Le chemin que l'unité doit suivre.

    // Événement déclenché à chaque fois que l'unité termine une étape de son mouvement.
    public event System.Action OnMovementStepCompleted; // Nouvel événement
    // Événement déclenché lorsque l'unité meurt.
    public event System.Action<Unit> OnUnitDied; // Nouveau: passe l'unité qui est morte.
    public event System.Action<int, int> OnHealthChanged; // Nouveau: (currentHealth, maxHealth)

    // Nouvelles propriétés pour les statistiques de l'unité.
    protected int _maxHealth;
    protected int _health;
    protected int _attackDamage; // NOTE: Utilisé uniquement par IlyaUnit pour le système de transformation
    protected int _movementRange;

    public ChampionData championData; // Référence aux données du champion.

    // Nouvelle propriété pour la faction de l'unité.
    [SerializeField] private UnitFaction _faction = UnitFaction.Player; // Par défaut, c'est une unité du joueur.

    // Points de mouvement restants pour le tour actuel.
    private int _remainingMovement; // N'est pas SerializableField car géré en code.

    public void Initialize(ChampionData data, Vector2 initialGridPos, UnitFaction faction)
    {
        championData = data;
        _currentGridPos = initialGridPos;
        _faction = faction;

        if (championData == null)
        {
            Debug.LogError($"ChampionData n'est pas assigné à l'unité {gameObject.name} lors de l'initialisation !");
            enabled = false;
            return;
        }

        InitUnitStats(championData); // Appelle la méthode d'initialisation des stats de base

        // Initialise le système d'émotion si disponible
        InitEmotionSystem(championData);

        // Définit le nom du GameObject de l'unité avec le nom du champion
        gameObject.name = championData.championName;

        // Positionne l'unité instantanément à sa position de grille initiale.
        if (GridManager.Instance != null)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(_currentGridPos);
            if (tile != null)
            {
                transform.position = tile.gameObject.transform.position + new Vector3(0, 0.5f, 0);
                Debug.Log($"{name} initialisé et positionné à la tuile {_currentGridPos}");
            }
            else
            {
                Debug.LogWarning($"Impossible de trouver la tuile à la position de grille : {_currentGridPos} pour {name}.");
            }
        }
        else
        {
            Debug.LogError("GridManager.Instance n'est pas disponible lors de l'initialisation de l'unité.");
        }
    }

    protected virtual void Start()
    {
        // Si l'unité n'a pas été initialisée via la méthode Initialize() (par exemple, ennemis placés manuellement)
        if (championData != null && _health == 0) // Vérifie si les stats n'ont pas été définies
        {
            Vector2 currentWorldGridPos = GridManager.Instance.GetGridPosFromWorldPos(transform.position); // Récupère la position actuelle de la scène
            Initialize(championData, currentWorldGridPos, _faction);
        }
        else if (championData == null)
        {
            Debug.LogError($"L'unité {gameObject.name} n'a pas de ChampionData assigné et n'a pas été initialisée. Elle ne fonctionnera pas correctement.");
            enabled = false;
        }

        // Ne crée la barre de vie QUE si c'est une Unit de base (pas une classe dérivée comme IlyaUnit)
        // Les classes dérivées appelleront CreateHealthBar() elles-mêmes après leur propre initialisation
        if (GetType() == typeof(Unit))
        {
            CreateHealthBar();
        }
    }

    protected void CreateHealthBar()
    {
        // Si une barre existe déjà, détruit l'ancienne avant d'en créer une nouvelle
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

        if (HealthBarManager.Instance == null)
        {
            return;
        }

        // Ne crée pas la barre si maxHealth est 0 (pas encore initialisé)
        if (_maxHealth <= 0)
        {
            return;
        }

        healthBar = HealthBarManager.Instance.CreateHealthBar(
            transform,
            healthBarOffset,
            healthBarColor,
            _maxHealth
        );

        // Initialise la barre avec la santé actuelle
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }
    }

    // Méthode pour initialiser les stats de base de l'unité (peut être surchargée)
    protected virtual void InitUnitStats(ChampionData data)
    {
        _maxHealth = data.maxHealth;
        _health = _maxHealth;
        _movementRange = data.movementRange;

        // Plus d'attaque de base, tout passe par les cartes
        _attackDamage = 0;
    }

    /// <summary>
    /// Initialise le système d'émotion avec les données du champion
    /// </summary>
    protected virtual void InitEmotionSystem(ChampionData data)
    {
        EmotionSystem emotionSystem = GetComponent<EmotionSystem>();
        if (emotionSystem != null)
        {
            // Configure les données émotionnelles de la famille
            emotionSystem.SetFamilyEmotionData(data.familyEmotionData);

            // Configure les transformations et seuils depuis ChampionData
            emotionSystem.SetPositiveTransformation(data.positiveTransformation);
            emotionSystem.SetNegativeTransformation(data.negativeTransformation);
            emotionSystem.SetThresholds(data.positiveThreshold, data.negativeThreshold);

            string emotionNames = data.familyEmotionData != null
                ? $"{data.familyEmotionData.positiveEmotionName}/{data.familyEmotionData.neutralEmotionName}/{data.familyEmotionData.negativeEmotionName}"
                : "Non défini";
            Debug.Log($"EmotionSystem configuré pour {data.championName} - Émotions: {emotionNames}");
        }
    }

    // Méthode pour déplacer l'unité vers une tuile spécifique de la grille.
    // Maintenant accepte un chemin (liste de tuiles) pour le déplacement case par case.
    public void MoveToTile(List<Tile> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"{name}: Chemin de déplacement vide ou nul.");
            _isMoving = false;
            return;
        }

        _path = path; // Stocke le chemin.
        _isMoving = true; // Active le mouvement.
        _targetWorldPosition = _path[0].gameObject.transform.position + new Vector3(0, 0.5f, 0); // La première tuile du chemin est la première cible.
        Debug.Log($"Déplacement de {name} le long d'un chemin de {path.Count} tuiles.");
    }

    // Update is called once per frame
    void Update()
    {
        // Si l'unité est en mouvement, la déplace progressivement vers la cible.
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetWorldPosition, _moveSpeed * Time.deltaTime);

            // Vérifie si l'unité a atteint sa position cible actuelle.
            if (transform.position == _targetWorldPosition)
            {
                // Si l'unité a atteint une tuile du chemin.
                if (_path.Count > 0)
                {
                    _currentGridPos = GridManager.Instance.GetGridPosFromWorldPos(_path[0].gameObject.transform.position); // Met à jour la position de grille actuelle
                    _path.RemoveAt(0); // Retire la tuile atteinte du chemin.

                    OnMovementStepCompleted?.Invoke(); // Déclenche l'événement après chaque étape de mouvement.

                    if (_path.Count > 0)
                    {
                        // Définit la prochaine tuile comme cible.
                        _targetWorldPosition = _path[0].gameObject.transform.position + new Vector3(0, 0.5f, 0);
                    }
                    else
                    {
                        // Le chemin est vide, l'unité a atteint sa destination finale.
                        _isMoving = false; // Arrête le mouvement.
                        Debug.Log($"{name} a atteint sa destination finale.");
                    }
                }
                else
                {
                    // Cas où _isMoving est true mais _path est vide (ne devrait pas arriver avec la logique ci-dessus).
                    _isMoving = false;
                }
            }
        }
    }

    // Méthode pour infliger des dégâts à cette unité.
    public virtual void TakeDamage(int damage)
    {
        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
        Debug.Log($"{name} a pris {damage} dégâts. PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }

        if (_health <= 0)
        {
            Debug.Log($"{name} a été vaincu !");
            OnUnitDied?.Invoke(this);

            // Détruit la barre de vie
            if (healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }

            Destroy(gameObject);
        }
    }

    // Méthode pour soigner l'unité.
    public void Heal(int amount)
    {
        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        Debug.Log($"{name} récupère {amount} PV. PV actuels : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth); // Déclenche l'événement de changement de PV

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }
    }

    // Setters publics pour les stats (utilisés par EmotionSystem et classes dérivées).
    public void SetMovementRange(int value)
    {
        _movementRange = value;
    }

    // NOTE: Utilisé uniquement par IlyaUnit pour modifier l'ATK lors des transformations
    protected void SetAttackDamage(int value)
    {
        _attackDamage = value;
    }

    // Setter pour la faction de l'unité (utilisé lors de l'instanciation du champion sélectionné)
    public void SetFaction(UnitFaction newFaction)
    {
        _faction = newFaction;
    }

    // LEGACY: Méthode d'attaque de base (obsolète, utilisée uniquement par IlyaUnit pour lifesteal)
    // Les attaques se font maintenant via les cartes, cette méthode n'inflige plus de dégâts directs
    public virtual void Attack(Unit target)
    {
        Debug.Log($"{name} attaque {target.name} et inflige {_attackDamage} dégâts.");
        target.TakeDamage(_attackDamage);
    }

    // Méthode pour dépenser des points de mouvement.
    public void SpendMovement(int amount)
    {
        _remainingMovement -= amount;
        if (_remainingMovement < 0) _remainingMovement = 0;
        Debug.Log($"{name} a dépensé {amount} points de mouvement. Restant : {_remainingMovement}");
    }

    // Méthode pour réinitialiser les points de mouvement au début du tour.
    public void RefreshMovement()
    {
        _remainingMovement = _movementRange;
        Debug.Log($"{name}: Points de mouvement réinitialisés à {_remainingMovement}.");
    }

    // Getters pour les propriétés de l'unité (nécessaires pour l'affichage UI).
    public int GetHealth()
    {
        return _health;
    }

    // NOTE: Utilisé uniquement par IlyaUnit pour le système de lifesteal en forme Déchaînée
    public int GetAttackDamage()
    {
        return _attackDamage;
    }

    public int GetMovementRange()
    {
        return _movementRange;
    }

    // Getter pour la faction de l'unité.
    public UnitFaction GetFaction()
    {
        return _faction;
    }

    // Getter pour la position de grille actuelle de l'unité.
    public Vector2 GetCurrentGridPos()
    {
        return _currentGridPos;
    }

    // Getter pour les points de mouvement restants.
    public int GetRemainingMovement()
    {
        return _remainingMovement;
    }

    // Getter pour vérifier si l'unité est en mouvement.
    public bool IsMoving()
    {
        return _isMoving;
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
    }
} 