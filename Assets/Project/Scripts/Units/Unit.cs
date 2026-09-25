using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Nouvelle énumération pour les factions (Joueur ou Ennemi).
    public enum UnitFaction { Player, Enemy }

    // La position actuelle de l'unité sur la grille (coordonnées X, Y).
    [SerializeField] protected Vector2Int _currentGridPos;
    // La position de grille initiale de l'unité, configurable dans l'Inspector.
    [SerializeField] protected Vector2Int _initialGridPos = new Vector2Int(0, 0); // Par défaut à (0,0).
    // Vitesse de déplacement de l'unité.
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f; // Vitesse de rotation pour regarder vers la cible
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
        public int armorModifier;
        public int barrierModifier;
        public int remainingTurns;

        public StatBuff(int atk, int armor, int barrier, int duration)
        {
            atkModifier = atk;
            armorModifier = armor;
            barrierModifier = barrier;
            remainingTurns = duration;
        }
    }

    // Liste des buffs actifs sur l'unité
    protected List<StatBuff> _activeBuffs = new List<StatBuff>();

    // Nouvelles propriétés pour les statistiques de l'unité.
    protected int _maxHealth;
    protected int _health;
    protected int _attackDamage;
    protected int _armor;   // Réduit les dégâts physiques reçus (valeur fixe, buffs compris)
    protected int _barrier; // Réduit les dégâts magiques reçus (valeur fixe, buffs compris)
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
    public void Initialize(Vector2Int initialGridPos)
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
                
                GameLog.Log($"{name} initialisé et positionné à la tuile {_currentGridPos}");
            }
            else
            {
                GameLog.LogWarning($"Impossible de trouver la tuile à la position de grille : {_currentGridPos} pour {name}.");
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
            GameLog.LogWarning($"L'unité {gameObject.name} a été placée dans la scène mais n'a pas été initialisée par son script dérivé (ex: Enemy, Champion).");
            enabled = false;
        }
    }

    protected void CreateHealthBar(Vector3 offset, Color color)
    {
        // Si une barre existe déjà, détruit l'ancienne avant d'en créer une nouvelle
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

        if (!Services.IsHealthBarServiceAvailable())
        {
            return;
        }

        // Ne crée pas la barre si maxHealth est 0 (pas encore initialisé)
        if (_maxHealth <= 0)
        {
            return;
        }

        healthBar = Services.HealthBar.CreateHealthBar(
            transform,
            offset,
            color,
            _maxHealth
        );

        // Initialise la barre avec la santé actuelle
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }
    }

    /// <summary>
    /// Initialise les stats de base de l'unité. Doit être appelée par les classes dérivées.
    /// </summary>
    protected virtual void InitUnitStats(int maxHealth, int movementRange, int attackDamage = 0, int armor = 0, int barrier = 0)
    {
        _maxHealth = maxHealth;
        _health = _maxHealth;
        _maxMovementPoints = movementRange;
        _attackDamage = attackDamage;
        _armor = armor;
        _barrier = barrier;
    }

    // Méthode pour déplacer l'unité vers une tuile spécifique de la grille.
    // Maintenant accepte un chemin (liste de tuiles) pour le déplacement case par case.
    public void MoveToTile(List<Tile> path)
    {
        MoveToTile(path, false);
    }

    /// <summary>
    /// Déplace l'unité le long d'un chemin de tuiles
    /// </summary>
    /// <param name="path">Le chemin à suivre</param>
    /// <param name="forceMove">Si true, ignore la vérification d'état (utilisé pour knockback)</param>
    public void MoveToTile(List<Tile> path, bool forceMove)
    {
        if (path == null || path.Count == 0)
        {
            GameLog.LogWarning($"{name}: Chemin de déplacement vide ou nul.");
            _isMoving = false;
            return;
        }

        // Phase 3.4: Vérifie l'état avant de bouger (sauf si forceMove)
        if (!forceMove && _unitState != null && !_unitState.CanMove())
        {
            GameLog.LogWarning($"{name}: Cannot move - state is {_unitState.GetCurrentState()}");
            return;
        }

        // Phase 3.4: Marque comme "en mouvement" (sauf si forceMove car l'unité n'est pas Active)
        if (!forceMove)
        {
            _unitState?.BeginMoving();
        }

        _path = path; // Stocke le chemin.
        _isMoving = true; // Active le mouvement.
        _targetWorldPosition = _path[0].gameObject.transform.position + new Vector3(0, 0.5f, 0); // La première tuile du chemin est la première cible.
        GameLog.Log($"Déplacement de {name} le long d'un chemin de {path.Count} tuiles.");
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

            // Applique la rotation seulement si on a une direction horizontale significative.
            // Snap immédiat sur la direction cardinale dominante (Nord/Sud/Est/Ouest) : pas de Slerp, donc pas
            // de passage transitoire par un angle en diagonale pendant les virages.
            if (direction.sqrMagnitude > 0.001f)
            {
                Vector2Int snapped = GridGeometry.SnapDirection(new Vector2(direction.x, direction.z));
                transform.rotation = Quaternion.LookRotation(new Vector3(snapped.x, 0, snapped.y));
            }

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

                        GameLog.Log($"{name} a atteint sa destination finale.");
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

    // Origine des dégâts en cours d'application (voir TakeDamageFrom), relayée dans UnitDamagedEvent
    private Unit _incomingDamageSource;

    /// <summary>
    /// Inflige des dégâts en précisant leur origine (ex: écho de Lyse), pour que les retours
    /// visuels puissent les distinguer. Passe par TakeDamage, surcharges comprises (boucliers).
    /// </summary>
    public void TakeDamageFrom(int damage, Unit source)
    {
        _incomingDamageSource = source;
        try
        {
            TakeDamage(damage);
        }
        finally
        {
            _incomingDamageSource = null;
        }
    }

    // Méthode pour infliger des dégâts à cette unité.
    public virtual void TakeDamage(int damage)
    {
        // Phase 3.4: Vérifie si l'unité peut recevoir des dégâts
        if (_unitState != null && !_unitState.CanTakeDamage())
        {
            GameLog.LogWarning($"{name}: Cannot take damage - already dead");
            return;
        }

        int damageToSelf = AbsorbWithShield(damage);
        _health = Mathf.Clamp(_health - damageToSelf, 0, _maxHealth);
        GameLog.Log($"{name} a pris {damageToSelf} dégâts (Total initial: {damage}). PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Phase 4.1: Publie l'événement de dégâts pour le système de combat visuals
        // Source connue seulement via TakeDamageFrom (null sinon)
        EventBus.Publish(new UnitDamagedEvent(this, _incomingDamageSource, damage));

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }

        if (_health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Inflige des dégâts qui ignorent le bouclier et les réductions en % (Paire de Raze).
    /// L'armure et la barrière sont déjà déduites par l'appelant (voir ReduceByDefense).
    /// </summary>
    public void TakeRawDamage(int damage)
    {
        // Phase 3.4: Vérifie si l'unité peut recevoir des dégâts
        if (_unitState != null && !_unitState.CanTakeDamage())
        {
            GameLog.LogWarning($"{name}: Cannot take raw damage - already dead");
            return;
        }

        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
        GameLog.Log($"{name} a pris {damage} dégâts bruts (ignore bouclier et réductions en %). PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Phase 4.1: Publie l'événement de dégâts pour le système de combat visuals
        EventBus.Publish(new UnitDamagedEvent(this, _incomingDamageSource, damage));

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }

        if (_health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Force la mort immédiate de l'unité sans passer par des dégâts (ex: une invocation dont
    /// l'invocateur vient de mourir). Passe par le même Die() que TakeDamage/TakeRawDamage pour
    /// que tout le nettoyage habituel (UnitState, EventBus, GridManager, UI) se déclenche.
    /// </summary>
    public void Kill()
    {
        // Phase 3.4: Évite une double mort
        if (_unitState != null && !_unitState.CanTakeDamage())
        {
            return;
        }

        _health = 0;
        OnHealthChanged?.Invoke(_health, _maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }

        Die();
    }

    /// <summary>
    /// Paie un coût en PV (ignore la défense, ne déclenche pas les effets de dégâts reçus)
    /// </summary>
    public void PayHealth(int amount)
    {
        if (amount <= 0) return;
        
        // Vérifie si l'unité est déjà morte
        if (_unitState != null && _unitState.IsDead()) return;

        _health = Mathf.Clamp(_health - amount, 0, _maxHealth);
        GameLog.Log($"{name} paie {amount} PV (Coût). PV restants : {_health}/{_maxHealth}");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Feedback visuel (utilise le système de dégâts pour l'affichage, mais c'est un coût)
        EventBus.Publish(new UnitDamagedEvent(this, this, amount));

        // Met à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }

        if (_health <= 0)
        {
            Die();
        }
    }

    // ========== ARMURE / BARRIÈRE ==========

    public int GetArmor() => _armor;
    public int GetBarrier() => _barrier;

    /// <summary>
    /// Dégâts restants après l'armure (physique) ou la barrière (magique) : soustraction fixe,
    /// minimum 1 si le coup fait des dégâts. Une valeur négative (débuff) augmente les dégâts.
    /// </summary>
    public int ReduceByDefense(int damage, DamageType type)
    {
        if (damage <= 0) return damage;

        int defense = type == DamageType.Magique ? _barrier : _armor;
        return Mathf.Max(1, damage - defense);
    }

    // ========== BOUCLIER (PV temporaires) ==========

    // Absorbe les dégâts avant les PV (pas les dégâts bruts, ex: Paire de Raze).
    // Dure jusqu'au début du prochain tour de celui qui l'a donné ; les boucliers se cumulent.
    private int _shield;
    private Unit _shieldSource;

    public event System.Action<int> OnShieldChanged;
    public int GetShield() => _shield;

    public void AddShield(int amount, Unit source)
    {
        if (amount <= 0) return;

        _shield += amount;
        _shieldSource = source;
        GameLog.Log($"{name} gagne un bouclier de {amount} (total {_shield}), jusqu'au prochain tour de {source?.name}");
        NotifyShieldChanged();
    }

    /// <summary>
    /// Appelé au début du tour de chaque unité : le bouclier expire au début du prochain tour
    /// de celui qui l'a donné (ou tout de suite si celui-ci est mort).
    /// </summary>
    public void ExpireShieldOnTurnStartOf(Unit turnUnit)
    {
        if (_shield <= 0) return;

        bool sourceGone = _shieldSource == null || (_shieldSource.GetUnitState()?.IsDead() ?? false);
        if (_shieldSource != turnUnit && !sourceGone) return;

        GameLog.Log($"{name}: bouclier de {_shield} expiré");
        _shield = 0;
        _shieldSource = null;
        NotifyShieldChanged();
    }

    /// <summary>
    /// Retire du bouclier ce qu'il peut absorber et renvoie les dégâts restants pour les PV.
    /// </summary>
    private int AbsorbWithShield(int damage)
    {
        if (_shield <= 0 || damage <= 0) return damage;

        int absorbed = Mathf.Min(_shield, damage);
        _shield -= absorbed;
        GameLog.Log($"{name}: le bouclier absorbe {absorbed} dégâts (reste {_shield})");
        OnShieldChanged?.Invoke(_shield);
        return damage - absorbed;
    }

    private void NotifyShieldChanged()
    {
        OnShieldChanged?.Invoke(_shield);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
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
            GameLog.Log($"{name} récupère {actualHealAmount} PV (Plafonné par MaxHP). Tentative de soin: {amount}. PV: {_health}/{_maxHealth}");
        }
        else
        {
            GameLog.Log($"{name} récupère {amount} PV. PV actuels : {_health}/{_maxHealth}");
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
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
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

        GameLog.Log($"{name}: Max Health changé à {_maxHealth}, HP actuels ajustés à {_health}");

        // Notifier le changement
        OnHealthChanged?.Invoke(_health, _maxHealth);

        // Mettre à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_health, _maxHealth, _shield);
        }
    }

    /// <summary>
    /// Retourne la UnitState (Phase 3.4)
    /// </summary>
    public UnitState GetUnitState()
    {
        return _unitState;
    }

    // Méthode pour dépenser des PM (Points de Mouvement)
    public void SpendMovement(int amount)
    {
        _currentMovementPoints -= amount;
        if (_currentMovementPoints < 0) _currentMovementPoints = 0;
        GameLog.Log($"{name} a dépensé {amount} PM. Restant : {_currentMovementPoints}");
        OnMovementPointsChanged?.Invoke(_currentMovementPoints, _maxMovementPoints);
    }

    // Méthode pour réinitialiser les PM au début du tour
    public void RefreshMovement()
    {
        _currentMovementPoints = _maxMovementPoints;
        GameLog.Log($"{name}: PM réinitialisés à {_currentMovementPoints}.");
        OnMovementPointsChanged?.Invoke(_currentMovementPoints, _maxMovementPoints);
    }

    // Getters pour les propriétés de l'unité (nécessaires pour l'affichage UI).
    public int GetHealth()
    {
        return _health;
    }

    /// <summary>
    /// Gère la mort de l'unité
    /// </summary>
    protected virtual void Die()
    {
        GameLog.Log($"{name} a été vaincu !");

        // Phase 3.4: Marque comme mort
        _unitState?.SetDead();

        OnUnitDied?.Invoke(this);

        // Phase 4.1: Publie l'événement de mort pour le système de combat visuals
        EventBus.Publish(new UnitDiedEvent(this));

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

    /// <summary>
    /// False pour une unité sans tour propre (invocation) : la rotation des tours la saute.
    /// </summary>
    public virtual bool TakesTurns => true;

    // Getter pour la position de grille actuelle de l'unité.
    public Vector2Int GetCurrentGridPos()
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
    /// Modifie les stats de l'unité (ATK, armure, barrière).
    /// Si duration > 0, crée un buff temporaire qui sera retiré après X tours.
    /// Si duration == 0, le buff est permanent.
    /// </summary>
    public virtual void ModifyStats(int atk, int armor, int barrier, int duration)
    {
        // Applique immédiatement les modifications
        _attackDamage += atk;
        _armor += armor;
        _barrier += barrier;

        // Si duration > 0, enregistre le buff pour le retirer plus tard
        if (duration > 0 && (atk != 0 || armor != 0 || barrier != 0))
        {
            _activeBuffs.Add(new StatBuff(atk, armor, barrier, duration));
            GameLog.Log($"{name}: Buff temporaire ajouté - ATK: {atk}, armure: {armor}, barrière: {barrier} pour {duration} tour(s)");
        }
        else if (atk != 0 || armor != 0 || barrier != 0)
        {
            GameLog.Log($"{name}: stats modifiées de façon permanente - ATK {_attackDamage}, armure {_armor}, barrière {_barrier}");
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
                _armor -= buff.armorModifier;
                _barrier -= buff.barrierModifier;

                GameLog.Log($"{name}: Buff expiré - ATK {-buff.atkModifier:+#;-#;0}, armure {-buff.armorModifier:+#;-#;0}, barrière {-buff.barrierModifier:+#;-#;0}");
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
    public Vector2Int ApplyKnockback(Vector2 direction, int distance)
    {
        if (distance <= 0) return GetCurrentGridPos();

        Vector2Int currentPos = GetCurrentGridPos();
        Vector2Int finalPos = currentPos;

        // Direction cardinale dominante (4 directions), 1 case à la fois
        Vector2Int stepDirection = GridGeometry.SnapDirection(direction);
        if (stepDirection == Vector2Int.zero) return currentPos;

        // Repousse case par case
        for (int i = 0; i < distance; i++)
        {
            Vector2Int nextPos = finalPos + stepDirection;

            // Vérifie si la case suivante est valide
            Tile nextTile = Services.Grid.GetTileAtPosition(nextPos);
            if (nextTile == null)
            {
                GameLog.Log($"{name}: Knockback arrêté - bord de la grille à {nextPos}");
                break;
            }

            // Vérifie si la case est occupée
            Unit unitOnTile = Services.Grid.GetUnitAtGridPos(nextPos);
            if (unitOnTile != null)
            {
                GameLog.Log($"{name}: Knockback arrêté - unité {unitOnTile.name} à {nextPos}");
                break;
            }

            finalPos = nextPos;
        }

        // Si l'unité a bougé, on la déplace
        if (finalPos != currentPos)
        {
            // Crée un chemin simple pour le déplacement visuel
            List<Tile> knockbackPath = new List<Tile>();
            Vector2Int pathPos = currentPos;
            while (pathPos != finalPos)
            {
                pathPos += stepDirection;
                Tile tile = Services.Grid.GetTileAtPosition(pathPos);
                if (tile != null) knockbackPath.Add(tile);
            }

            if (knockbackPath.Count > 0)
            {
                // forceMove = true car le knockback doit fonctionner même si l'unité est en état Idle
                MoveToTile(knockbackPath, true);
                GameLog.Log($"{name}: Knockback de {currentPos} vers {finalPos} ({knockbackPath.Count} cases)");
            }
        }

        return finalPos;
    }
} 