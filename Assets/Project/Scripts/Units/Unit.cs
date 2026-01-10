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
    protected int _maxMovementPoints; // PM (Points de Mouvement) maximum

    public ChampionData championData; // Référence aux données du champion.

    // Nouvelle propriété pour la faction de l'unité.
    [SerializeField] private UnitFaction _faction = UnitFaction.Player; // Par défaut, c'est une unité du joueur.

    // PM (Points de Mouvement) restants pour le tour actuel.
    private int _currentMovementPoints; // N'est pas SerializableField car géré en code.

    // Protection contre la double initialisation
    protected bool _isInitialized = false;

    // ===== STATE MACHINE (Phase 3.4) =====
    private UnitState _unitState;

    // NOTE: Le système PA a été déplacé vers les classes Champion et Enemy
    // Unit ne contient plus que la base commune (HP, Movement, Position)

    public void Initialize(ChampionData data, Vector2 initialGridPos, UnitFaction faction)
    {
        // Protection contre la double initialisation
        if (_isInitialized)
        {
            Debug.LogWarning($"{gameObject.name} est déjà initialisé. Initialisation ignorée.");
            return;
        }

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

        // Initialise la UnitState (Phase 3.4)
        _unitState = new UnitState(this);

        // Positionne l'unité instantanément à sa position de grille initiale.
        if (Services.Grid != null)
        {
            Tile tile = Services.Grid.GetTileAtPosition(_currentGridPos);
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
            Debug.LogError("Services.Grid n'est pas disponible lors de l'initialisation de l'unité.");
        }

        // Marque l'unité comme initialisée
        _isInitialized = true;
    }

    protected virtual void Start()
    {
        // Si l'unité n'a pas été initialisée (unités placées manuellement dans la scène)
        if (!_isInitialized)
        {
            if (championData != null)
            {
                Vector2 currentWorldGridPos = Services.Grid.GetGridPosFromWorldPos(transform.position);
                Initialize(championData, currentWorldGridPos, _faction);
            }
            else
            {
                Debug.LogError($"L'unité {gameObject.name} n'a pas de ChampionData assigné et ne peut pas être initialisée.");
                enabled = false;
                return;
            }
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
        // Validation centralisée des données
        ValidationResult validation = GameActionValidator.ValidateChampionData(data);
        if (!validation.IsValid)
        {
            Debug.LogError($"❌ Échec initialisation Unit : {validation.ErrorMessage}");
            enabled = false;
            return;
        }

        _maxHealth = data.maxHealth;
        _health = _maxHealth;
        _maxMovementPoints = data.movementRange;

        // NOTE: Les PA sont maintenant gérés par les classes dérivées (Champion, Enemy)
        // Plus d'attaque de base, tout passe par les cartes
        _attackDamage = 0;
    }

    /// <summary>
    /// Initialise le système d'émotion avec les données du champion
    /// </summary>
    protected virtual void InitEmotionSystem(ChampionData data)
    {
        // OPTIMISATION Phase 3.3: ComponentLocator (optionnel car toutes les unités n'ont pas EmotionSystem)
        if (this.TryGetComponentSafe(out EmotionSystem emotionSystem))
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

        // Phase 3.4: Vérifie l'état avant de bouger
        if (_unitState != null && !_unitState.CanMove())
        {
            Debug.LogWarning($"{name}: Cannot move - state is {_unitState.GetCurrentState()}");
            return;
        }

        // Phase 3.4: Marque comme "en mouvement"
        _unitState?.BeginMoving();

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
                    _currentGridPos = Services.Grid.GetGridPosFromWorldPos(_path[0].gameObject.transform.position); // Met à jour la position de grille actuelle
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

                        // Phase 3.4: Termine le mouvement
                        _unitState?.EndMoving();

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
        // Phase 3.4: Vérifie si l'unité peut recevoir des dégâts
        if (_unitState != null && !_unitState.CanTakeDamage())
        {
            Debug.LogWarning($"{name}: Cannot take damage - already dead");
            return;
        }

        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
        Debug.Log($"{name} a pris {damage} dégâts. PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Phase 4.1: Publie l'événement de dégâts pour le système de combat visuals
        // Note: On ne connaît pas forcément la source des dégâts ici, donc on passe null
        EventBus.Publish(new UnitDamagedEvent(this, null, damage));

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }

        if (_health <= 0)
        {
            Debug.Log($"{name} a été vaincu !");

            // Phase 3.4: Marque comme mort
            _unitState?.SetDead();

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
        // Ne soigne pas si déjà à max HP
        int actualHealAmount = Mathf.Min(amount, _maxHealth - _health);

        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        Debug.Log($"{name} récupère {amount} PV. PV actuels : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth); // Déclenche l'événement de changement de PV

        // Phase 4.1: Publie l'événement de soins pour le système de combat visuals
        if (actualHealAmount > 0)
        {
            EventBus.Publish(new UnitHealedEvent(this, actualHealAmount));
        }

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }
    }

    // Setters publics pour les stats (utilisés par EmotionSystem et classes dérivées).
    public void SetMaxMovementPoints(int value)
    {
        _maxMovementPoints = value;
    }

    /// <summary>
    /// Modifie la santé maximum de l'unité (utilisé par EmotionSystem pour les transformations)
    /// Ajuste aussi la santé actuelle proportionnellement pour éviter les incohérences
    /// </summary>
    public void SetMaxHealth(int value)
    {
        if (value <= 0)
        {
            Debug.LogError($"{name}: Tentative de définir maxHealth à {value}, valeur invalide!");
            return;
        }

        // Calculer le pourcentage de santé actuel
        float healthPercentage = (_maxHealth > 0) ? ((float)_health / _maxHealth) : 1f;

        // Appliquer la nouvelle santé maximum
        _maxHealth = value;

        // Ajuster la santé actuelle pour maintenir le même pourcentage
        _health = Mathf.RoundToInt(_maxHealth * healthPercentage);
        _health = Mathf.Clamp(_health, 1, _maxHealth); // Au minimum 1 HP

        Debug.Log($"{name}: Max Health changé à {_maxHealth}, HP actuels ajustés à {_health}");

        // Notifier le changement
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Mettre à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }
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

    /// <summary>
    /// Retourne la UnitState (Phase 3.4)
    /// </summary>
    public UnitState GetUnitState()
    {
        return _unitState;
    }

    // LEGACY: Méthode d'attaque de base (obsolète, utilisée uniquement par IlyaUnit pour lifesteal)
    // Les attaques se font maintenant via les cartes, cette méthode n'inflige plus de dégâts directs
    public virtual void Attack(Unit target)
    {
        Debug.Log($"{name} attaque {target.name} et inflige {_attackDamage} dégâts.");
        target.TakeDamage(_attackDamage);
    }

    // Méthode pour dépenser des PM (Points de Mouvement)
    public void SpendMovement(int amount)
    {
        _currentMovementPoints -= amount;
        if (_currentMovementPoints < 0) _currentMovementPoints = 0;
        Debug.Log($"{name} a dépensé {amount} PM. Restant : {_currentMovementPoints}");
    }

    // Méthode pour réinitialiser les PM au début du tour
    public void RefreshMovement()
    {
        _currentMovementPoints = _maxMovementPoints;
        Debug.Log($"{name}: PM réinitialisés à {_currentMovementPoints}.");
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

    /// <summary>
    /// Retourne les PM (Points de Mouvement) maximum
    /// </summary>
    public int GetMaxMovementPoints()
    {
        return _maxMovementPoints;
    }

    /// <summary>
    /// Retourne les PM (Points de Mouvement) restants ce tour
    /// </summary>
    public int GetCurrentMovementPoints()
    {
        return _currentMovementPoints;
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

    // Getter pour vérifier si l'unité est en mouvement.
    public bool IsMoving()
    {
        return _isMoving;
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
    }

    // NOTE: Les méthodes PA (GetCurrentPA, GetMaxPA, SetMaxPA, SpendPA, RefreshPA)
    // ont été déplacées vers les classes Champion et Enemy
} 