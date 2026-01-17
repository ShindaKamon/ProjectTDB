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
    [SerializeField] private float _rotationSpeed = 10f; // Vitesse de rotation pour regarder vers la cible

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
    public event System.Action<int, int> OnMovementPointsChanged; // (currentPM, maxPM)
    public event System.Action OnStatsModified; // Déclenché quand ATK/DEF changent

    // ========== SYSTÈME DE BUFFS TEMPORAIRES ==========

    /// <summary>
    /// Structure représentant un buff de stat temporaire
    /// </summary>
    public struct StatBuff
    {
        public int atkModifier;
        public int defModifier;
        public int remainingTurns;

        public StatBuff(int atk, int def, int duration)
        {
            atkModifier = atk;
            defModifier = def;
            remainingTurns = duration;
        }
    }

    // Liste des buffs actifs sur l'unité
    protected List<StatBuff> _activeBuffs = new List<StatBuff>();

    // Nouvelles propriétés pour les statistiques de l'unité.
    protected int _maxHealth;
    protected int _health;
    protected int _attackDamage; // NOTE: Utilisé uniquement par IlyaUnit pour le système de transformation
    protected int _maxMovementPoints; // PM (Points de Mouvement) maximum

    // PM (Points de Mouvement) restants pour le tour actuel.
    private int _currentMovementPoints; // N'est pas SerializableField car géré en code.

    // Protection contre la double initialisation
    protected bool _isInitialized = false;

    // ===== STATE MACHINE (Phase 3.4) =====
    private UnitState _unitState;

    // NOTE: Le système PA a été déplacé vers les classes Champion et Enemy
    // Unit ne contient plus que la base commune (HP, Movement, Position)

    /// <summary>
    /// Initialise les aspects communs de l'unité (position, faction, état).
    /// Doit être appelée par les classes dérivées après l'initialisation des stats.
    /// </summary>
    public void Initialize(Vector2 initialGridPos)
    {
        // Protection contre la double initialisation
        if (_isInitialized)
        {
            return;
        }
        
        // Assigne les valeurs passées en paramètres
        _currentGridPos = initialGridPos;

        // Initialise la UnitState (Phase 3.4)
        _unitState = new UnitState(this);

        if (Services.Grid != null)
        {
            Tile tile = Services.Grid.GetTileAtPosition(_currentGridPos);
            if (tile != null)
            {
                transform.position = tile.gameObject.transform.position + new Vector3(0, 0.5f, 0);
                
                // Oriente l'unité selon sa faction au démarrage
                if (GetFaction() == UnitFaction.Enemy)
                {
                    transform.rotation = Quaternion.Euler(0, 180f, 0); // Face au joueur (Sud)
                }
                else
                {
                    transform.rotation = Quaternion.identity; // Face aux ennemis (Nord)
                }
                
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

    public bool IsInitialized() => _isInitialized;

    protected virtual void Start()
    {
        // Si l'unité n'a pas été initialisée (unités placées manuellement dans la scène)
        // Les classes dérivées (Enemy, Champion) sont responsables de leur propre initialisation dans leur Start().
        if (!_isInitialized)
        {
            Debug.LogWarning($"L'unité {gameObject.name} a été placée dans la scène mais n'a pas été initialisée par son script dérivé (ex: Enemy, Champion).");
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

    /// <summary>
    /// Initialise les stats de base de l'unité. Doit être appelée par les classes dérivées.
    /// </summary>
    protected virtual void InitUnitStats(int maxHealth, int movementRange, int attackDamage = 0)
    {
        _maxHealth = maxHealth;
        _health = _maxHealth;
        _maxMovementPoints = movementRange;
        _attackDamage = attackDamage;
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
            // Calcule la direction vers la cible
            Vector3 direction = _targetWorldPosition - transform.position;
            direction.y = 0; // On ignore la hauteur pour la rotation

            // Applique la rotation seulement si on a une direction horizontale significative
            // Cela évite que l'unité ne se mette de travers quand elle est très proche de la cible
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }

            // CORRECTION : Force l'unité à rester parfaitement droite (X et Z à 0)
            // Cela empêche l'unité de se pencher ou d'être "de travers" pendant le mouvement
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

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

                        // CORRECTION : Force l'unité à être droite à l'arrêt
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

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

    /// <summary>
    /// Fait tourner l'unité pour faire face à une position mondiale.
    /// </summary>
    public IEnumerator LookAtCoroutine(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // On ne veut tourner que sur l'axe Y

        if (direction.sqrMagnitude < 0.01f) yield break; // Déjà face à la cible ou trop proche

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Tourne jusqu'à ce que l'angle soit négligeable
        while (Quaternion.Angle(transform.rotation, targetRotation) > 1.0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotation; // Snap final pour la précision
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
            Die();
        }
    }

    /// <summary>
    /// Paie un coût en PV (ignore la défense, ne déclenche pas les effets de dégâts reçus comme la Rage)
    /// </summary>
    public void PayHealth(int amount)
    {
        if (amount <= 0) return;
        
        // Vérifie si l'unité est déjà morte
        if (_unitState != null && _unitState.IsDead()) return;

        _health = Mathf.Clamp(_health - amount, 0, _maxHealth);
        Debug.Log($"{name} paie {amount} PV (Coût). PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Feedback visuel (utilise le système de dégâts pour l'affichage, mais c'est un coût)
        EventBus.Publish(new UnitDamagedEvent(this, this, amount));

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth);
        }

        if (_health <= 0)
        {
            Die();
        }
    }

    // Méthode pour soigner l'unité.
    public void Heal(int amount)
    {
        // Ne soigne pas si déjà à max HP
        int missingHealth = _maxHealth - _health;
        int actualHealAmount = Mathf.Min(amount, missingHealth);

        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        
        if (actualHealAmount < amount)
        {
            Debug.Log($"{name} récupère {actualHealAmount} PV (Plafonné par MaxHP). Tentative de soin: {amount}. PV: {_health}/{_maxHealth}");
        }
        else
        {
            Debug.Log($"{name} récupère {amount} PV. PV actuels : {_health}/{_maxHealth}");
        }

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

    // Setters publics pour les stats (utilisés par les classes dérivées)
    public void SetMaxMovementPoints(int value)
    {
        _maxMovementPoints = value;
    }

    /// <summary>
    /// Modifie la santé maximum de l'unité.
    /// Ajuste aussi la santé actuelle proportionnellement pour éviter les incohérences.
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
        OnMovementPointsChanged?.Invoke(_currentMovementPoints, _maxMovementPoints);
    }

    // Méthode pour réinitialiser les PM au début du tour
    public void RefreshMovement()
    {
        _currentMovementPoints = _maxMovementPoints;
        Debug.Log($"{name}: PM réinitialisés à {_currentMovementPoints}.");
        OnMovementPointsChanged?.Invoke(_currentMovementPoints, _maxMovementPoints);
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
    /// Gère la mort de l'unité
    /// </summary>
    protected virtual void Die()
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
    public virtual UnitFaction GetFaction()
    {
        return UnitFaction.Player; // Par défaut, une Unit de base est considérée comme Player (ou neutre)
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

    /// <summary>
    /// Modifie les stats de l'unité (ATK, DEF).
    /// Si duration > 0, crée un buff temporaire qui sera retiré après X tours.
    /// Si duration == 0, le buff est permanent.
    /// </summary>
    public virtual void ModifyStats(int atk, int def, int duration)
    {
        // Applique immédiatement les modifications
        _attackDamage += atk;

        // Si duration > 0, enregistre le buff pour le retirer plus tard
        if (duration > 0 && (atk != 0 || def != 0))
        {
            _activeBuffs.Add(new StatBuff(atk, def, duration));
            Debug.Log($"{name}: Buff temporaire ajouté - ATK: {atk}, DEF: {def} pour {duration} tour(s)");
        }
        else if (atk != 0)
        {
            Debug.Log($"{name}: ATK modifiée de {atk} (Total: {_attackDamage}) - permanent");
        }

        OnStatsModified?.Invoke();
    }

    /// <summary>
    /// Appelé au début du tour de l'unité pour décrémenter et retirer les buffs expirés
    /// </summary>
    public virtual void ProcessBuffsOnTurnStart()
    {
        if (_activeBuffs.Count == 0) return;

        bool statsChanged = false;

        // Parcourt les buffs en sens inverse pour pouvoir supprimer pendant l'itération
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            StatBuff buff = _activeBuffs[i];
            buff.remainingTurns--;

            if (buff.remainingTurns <= 0)
            {
                // Retire les effets du buff
                _attackDamage -= buff.atkModifier;
                // Note: DEF est géré par les classes dérivées (IlyaUnit, Enemy)

                Debug.Log($"{name}: Buff expiré - ATK restaurée de {buff.atkModifier}");
                _activeBuffs.RemoveAt(i);
                statsChanged = true;
            }
            else
            {
                // Met à jour le buff avec la nouvelle durée
                _activeBuffs[i] = buff;
            }
        }

        if (statsChanged)
        {
            OnStatsModified?.Invoke();
        }
    }

    /// <summary>
    /// Retourne la liste des buffs actifs (pour les classes dérivées)
    /// </summary>
    protected List<StatBuff> GetActiveBuffs() => _activeBuffs;

    /// <summary>
    /// Permet aux classes dérivées de déclencher l'événement OnStatsModified
    /// </summary>
    protected void NotifyStatsModified()
    {
        OnStatsModified?.Invoke();
    }

    /// <summary>
    /// Retourne la valeur d'attaque de l'unité
    /// </summary>
    public int GetAttack()
    {
        return _attackDamage;
    }

    /// <summary>
    /// Applique un knockback à l'unité dans une direction donnée
    /// </summary>
    /// <param name="direction">Direction du knockback (normalisée)</param>
    /// <param name="distance">Nombre de cases à repousser</param>
    /// <returns>La position finale après le knockback</returns>
    public Vector2 ApplyKnockback(Vector2 direction, int distance)
    {
        if (distance <= 0) return GetCurrentGridPos();

        Vector2 currentPos = GetCurrentGridPos();
        Vector2 finalPos = currentPos;

        // Normalise la direction en mouvement de grille (1 case à la fois)
        Vector2 stepDirection = new Vector2(
            Mathf.RoundToInt(direction.x),
            Mathf.RoundToInt(direction.y)
        );

        // Si la direction est diagonale, on prend la composante la plus forte
        if (stepDirection.x != 0 && stepDirection.y != 0)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                stepDirection.y = 0;
            else
                stepDirection.x = 0;
        }

        // Repousse case par case
        for (int i = 0; i < distance; i++)
        {
            Vector2 nextPos = finalPos + stepDirection;

            // Vérifie si la case suivante est valide
            Tile nextTile = Services.Grid.GetTileAtPosition(nextPos);
            if (nextTile == null)
            {
                Debug.Log($"{name}: Knockback arrêté - bord de la grille à {nextPos}");
                break;
            }

            // Vérifie si la case est occupée
            Unit unitOnTile = Services.Grid.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null)
            {
                Debug.Log($"{name}: Knockback arrêté - unité {unitOnTile.name} à {nextPos}");
                break;
            }

            finalPos = nextPos;
        }

        // Si l'unité a bougé, on la déplace
        if (finalPos != currentPos)
        {
            // Crée un chemin simple pour le déplacement visuel
            List<Tile> knockbackPath = new List<Tile>();
            Vector2 pathPos = currentPos;
            while (pathPos != finalPos)
            {
                pathPos += stepDirection;
                Tile tile = Services.Grid.GetTileAtPosition(pathPos);
                if (tile != null) knockbackPath.Add(tile);
            }

            if (knockbackPath.Count > 0)
            {
                MoveToTile(knockbackPath);
                Debug.Log($"{name}: Knockback de {currentPos} vers {finalPos} ({knockbackPath.Count} cases)");
            }
        }

        return finalPos;
    }
} 