using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe Enemy hérite de Unit et ajoute le système de cartes pour les ennemis.
/// Les ennemis piochent séquentiellement leur deck (pas de mélange) comme pattern de combat.
/// </summary>
public class Enemy : Unit
{
    // ========== ENEMY DATA ==========

    [Header("=== Enemy Specific ==")]
    [SerializeField] private EnemyData _enemyData;

    [SerializeField] private int _maxPA = 2;           // PA max par tour
    [SerializeField] private int _currentPA = 2;       // PA restants ce tour

    // Deck system pour pattern de combat
    private List<CardData> _combatDeck = new List<CardData>();
    private int _currentCardIndex = 0; // Index de la prochaine carte à jouer

    // ========== EVENTS ==========
    public event System.Action<int, int> OnPAChanged;        // (current, max)
    public event System.Action<CardData> OnNextCardChanged;   // Prochaine carte visible

    // ========== GETTERS PUBLICS ==========

    public int GetCurrentPA() => _currentPA;
    public int GetMaxPA() => _maxPA;
    public EnemyData GetEnemyData() => _enemyData;
    public bool IsBoss() => _enemyData != null && _enemyData.isBoss;

    /// <summary>
    /// Retourne la prochaine carte qui sera jouée (visible par le joueur)
    /// </summary>
    public CardData GetNextCard()
    {
        if (_combatDeck == null || _combatDeck.Count == 0) return null;

        // Boucle automatique : si on a atteint la fin du deck, recommence au début
        if (_currentCardIndex >= _combatDeck.Count)
        {
            _currentCardIndex = 0;
            Debug.Log($"{name} (Enemy): Fin du cycle de cartes, retour au début du pattern");
        }

        return _combatDeck[_currentCardIndex];
    }

    // ========== INITIALISATION ==========

    /// <summary>
    /// Initialise l'ennemi avec EnemyData (au lieu de ChampionData)
    /// </summary>
    public void InitializeEnemy(EnemyData data, Vector2 initialGridPos)
    {
        _enemyData = data;

        if (_enemyData == null)
        {
            Debug.LogError($"EnemyData n'est pas assigné à l'ennemi {gameObject.name} !");
            enabled = false;
            return;
        }

        // Initialise les stats de base depuis EnemyData
        _maxHealth = data.maxHealth;
        _health = _maxHealth;
        _movementRange = data.movementRange;
        _maxPA = data.maxPA;
        _currentPA = _maxPA;

        // Les ennemis n'ont pas d'attaque de base, tout passe par les cartes
        _attackDamage = 0;

        // Initialise position et faction
        _currentGridPos = initialGridPos;
        SetFaction(UnitFaction.Enemy);

        // Copie le deck de combat (pattern)
        _combatDeck.Clear();
        if (data.combatDeck != null)
        {
            _combatDeck.AddRange(data.combatDeck);
        }
        _currentCardIndex = 0;

        // Définit le nom
        gameObject.name = data.enemyName;

        // Positionne l'ennemi dans le monde
        if (GridManager.Instance != null)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(_currentGridPos);
            if (tile != null)
            {
                transform.position = tile.gameObject.transform.position + new Vector3(0, 0.5f, 0);
                Debug.Log($"{name} (Enemy) initialisé et positionné à {_currentGridPos}");
            }
        }

        // Notifie la prochaine carte
        OnNextCardChanged?.Invoke(GetNextCard());

        Debug.Log($"{name} (Enemy) initialisé - HP: {_health}/{_maxHealth}, PA: {_currentPA}/{_maxPA}, Deck: {_combatDeck.Count} cartes");
    }

    protected override void Start()
    {
        // Si l'ennemi n'a pas été initialisé via InitializeEnemy (placé manuellement)
        if (_enemyData != null && _health == 0)
        {
            Vector2 currentWorldGridPos = GridManager.Instance.GetGridPosFromWorldPos(transform.position);
            InitializeEnemy(_enemyData, currentWorldGridPos);
        }
        else if (_enemyData == null)
        {
            Debug.LogError($"L'ennemi {gameObject.name} n'a pas de EnemyData assigné !");
            enabled = false;
            return;
        }

        // Crée la barre de vie (sauf si c'est un boss - sera géré différemment)
        if (!IsBoss())
        {
            CreateHealthBar();
        }
    }

    // ========== SYSTÈME PA ==========

    /// <summary>
    /// Dépense des PA pour jouer une carte
    /// </summary>
    public bool SpendPA(int amount)
    {
        if (_currentPA >= amount)
        {
            _currentPA -= amount;
            OnPAChanged?.Invoke(_currentPA, _maxPA);
            Debug.Log($"{name} (Enemy): PA dépensés ({amount}). Restant: {_currentPA}/{_maxPA}");
            return true;
        }
        else
        {
            Debug.LogWarning($"{name} (Enemy): PA insuffisants ! Requis: {amount}, Disponible: {_currentPA}");
            return false;
        }
    }

    /// <summary>
    /// Rafraîchit les PA en début de tour
    /// </summary>
    public void RefreshPA()
    {
        _currentPA = _maxPA;
        OnPAChanged?.Invoke(_currentPA, _maxPA);
        Debug.Log($"{name} (Enemy): PA rafraîchis à {_currentPA}/{_maxPA}");
    }

    // ========== SYSTÈME DE DECK SÉQUENTIEL ==========

    /// <summary>
    /// Pioche et joue la prochaine carte du deck (séquentiel, pas de mélange)
    /// Retourne null si aucune carte disponible ou pas assez de PA
    /// </summary>
    public CardData DrawAndPlayNextCard()
    {
        // Vérifie s'il reste des cartes
        if (_combatDeck == null || _combatDeck.Count == 0)
        {
            Debug.LogWarning($"{name} (Enemy): Deck vide !");
            return null;
        }

        if (_currentCardIndex >= _combatDeck.Count)
        {
            // Fin du deck, recommence au début (boucle)
            Debug.Log($"{name} (Enemy): Fin du deck, retour au début");
            _currentCardIndex = 0;
        }

        CardData nextCard = _combatDeck[_currentCardIndex];

        // Vérifie si on a assez de PA
        if (nextCard.costPA > _currentPA)
        {
            Debug.LogWarning($"{name} (Enemy): Pas assez de PA pour jouer {nextCard.cardName} (coût: {nextCard.costPA}, dispo: {_currentPA})");
            return null;
        }

        // Joue la carte
        Debug.Log($"{name} (Enemy) joue la carte: {nextCard.cardName}");
        _currentCardIndex++;

        // Notifie la prochaine carte (pour la preview)
        OnNextCardChanged?.Invoke(GetNextCard());

        return nextCard;
    }

    /// <summary>
    /// Réinitialise le deck au début (utilisé si on veut forcer un reset)
    /// </summary>
    public void ResetDeckIndex()
    {
        _currentCardIndex = 0;
        OnNextCardChanged?.Invoke(GetNextCard());
        Debug.Log($"{name} (Enemy): Index du deck réinitialisé");
    }

    /// <summary>
    /// Retourne le nombre de cartes restantes avant la boucle
    /// </summary>
    public int GetRemainingCardsInCycle()
    {
        if (_combatDeck == null || _combatDeck.Count == 0) return 0;
        return _combatDeck.Count - _currentCardIndex;
    }

    /// <summary>
    /// Appelé quand l'ennemi est détruit (mort ou autre raison)
    /// </summary>
    void OnDestroy()
    {
        // Notifie le BattleUIManager pour nettoyer les UI
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.OnEnemyDied(this);
        }
    }
}
