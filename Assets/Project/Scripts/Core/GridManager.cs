using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GridManager adapté pour Émotions Tactics
/// Gère la grille carrée, les unités, le système de tours, et l'UI
/// Compatible avec Unit de base ET IlyaUnit
/// Phase 3.5: Implémente IGridService pour injection de dépendances
/// </summary>
public class GridManager : MonoBehaviour, IGridService
{
    // NOTE: Instance gardé pour compatibilité (sera progressivement éliminé)
    public static GridManager Instance { get; private set; }

    [Header("=== Configuration Grille ===")]
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Vector2 _playerSpawnGridPos = new Vector2(0,0); // Position de la grille où le joueur apparaîtra
    
    [Header("=== Couleurs Portées ===")]
    [SerializeField] private Color _moveColor = Color.blue;
    [SerializeField] private Color _cardTargetColor = Color.yellow; // Couleur pour les cibles de carte
    [SerializeField] private Color _aoeColor = new Color(1f, 0.5f, 0f, 0.7f); // Couleur pour la zone AOE (orange transparent)
    
    
    [Header("=== Managers ===")]
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Button _endTurnButton; // Référence au bouton Fin de Tour
    
    // ===== DONNÉES INTERNES =====
    private Dictionary<Vector2, Tile> _tiles;
    private List<Unit> _units;
    private Unit _activeUnit;

    // ===== REPOSITORY PATTERN =====
    // GridRepository centralise toutes les requêtes de grille pour améliorer la testabilité
    private GridRepository _gridRepository;

    // ===== STATE MACHINE (Phase 3.4) =====
    // TurnStateMachine gère les états explicites du système de tours
    private TurnStateMachine _turnStateMachine;

    // ===== UI CACHE =====
    // Cache pour éviter FindFirstObjectByType à chaque tour
    private UnitStatsUI _cachedStatsUI;

    // ===== AWAKE & INIT =====

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        _tiles = new Dictionary<Vector2, Tile>();
        _units = new List<Unit>();
        GenerateGrid();

        // Initialise le GridRepository après la génération de la grille
        _gridRepository = new GridRepository(_tiles, _units, _width, _height);

        // Initialise la TurnStateMachine (Phase 3.4)
        _turnStateMachine = new TurnStateMachine();

        // Enregistre ce GridManager comme IGridService dans le ServiceLocator (Phase 3.5)
        ServiceLocator.Instance.Register<IGridService>(this);
        Debug.Log("GridManager: Enregistré dans ServiceLocator comme IGridService");

        // S'abonne aux événements de l'EventBus
        EventBus.Subscribe<TurnEndRequestedEvent>(OnTurnEndRequested);
        EventBus.Subscribe<ShowMovementRangeEvent>(OnShowMovementRange);
        EventBus.Subscribe<ShowCardTargetsEvent>(OnShowCardTargets);
        EventBus.Subscribe<ShowAOEZoneEvent>(OnShowAOEZone);
        EventBus.Subscribe<ResetTileColorsEvent>(OnResetTileColors);
    }

    private void Start()
    {
        // Initialise les unités dans Start() pour que le BattleUIManager ait le temps de s'initialiser dans Awake()
        InitUnits();
    }

    private void OnDestroy()
    {
        // Se désabonne des événements pour éviter les fuites mémoire
        EventBus.Unsubscribe<TurnEndRequestedEvent>(OnTurnEndRequested);
        EventBus.Unsubscribe<ShowMovementRangeEvent>(OnShowMovementRange);
        EventBus.Unsubscribe<ShowCardTargetsEvent>(OnShowCardTargets);
        EventBus.Unsubscribe<ShowAOEZoneEvent>(OnShowAOEZone);
        EventBus.Unsubscribe<ResetTileColorsEvent>(OnResetTileColors);

        // Désenregistre du ServiceLocator (Phase 3.5)
        ServiceLocator.Instance.Unregister<IGridService>();
    }

    /// <summary>
    /// Appelé quand une unité demande la fin du tour via l'EventBus
    /// </summary>
    private void OnTurnEndRequested(TurnEndRequestedEvent e)
    {
        Debug.Log($"[EventBus] Fin de tour demandée par {e.RequestingUnit?.name ?? "Inconnu"}");
        OnEndTurnButtonClick();
    }

    /// <summary>
    /// Appelé quand un composant demande l'affichage de la portée de mouvement
    /// </summary>
    private void OnShowMovementRange(ShowMovementRangeEvent e)
    {
        if (e.Unit != null)
        {
            ShowMovementRange(e.Unit);
        }
    }

    /// <summary>
    /// Appelé quand un composant demande l'affichage des cibles de carte
    /// </summary>
    private void OnShowCardTargets(ShowCardTargetsEvent e)
    {
        if (e.Card != null && e.Source != null)
        {
            ShowCardTargets(e.Card, e.Source);
        }
    }

    /// <summary>
    /// Appelé quand un composant demande l'affichage d'une zone AOE
    /// </summary>
    private void OnShowAOEZone(ShowAOEZoneEvent e)
    {
        if (e.Card != null && e.Source != null)
        {
            ShowAOEZone(e.Epicenter, e.Radius, e.Card, e.Source);
        }
    }

    /// <summary>
    /// Appelé quand un composant demande la réinitialisation des couleurs de tuiles
    /// </summary>
    private void OnResetTileColors(ResetTileColorsEvent e)
    {
        ResetAllTileColors();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 tileWorldPosition = new Vector3(x - _width / 2, 0, y - _height / 2);
                var spawnedTile = Instantiate(_tilePrefab, tileWorldPosition, Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.transform.SetParent(transform);

                // OPTIMISATION Phase 3.3: ComponentLocator
                var tile = spawnedTile.GetRequiredComponent<Tile>("Tile instantiée par GridManager");
                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                tile.Init(isOffset);

                _tiles[new Vector2(x, y)] = tile;
            }
        }
        
        transform.position = Vector3.zero;
        Debug.Log($"Grille générée : {_width}x{_height} = {_tiles.Count} tuiles");
    }

    // ===== GESTION UNITÉS =====
    
    private void InitUnits()
    {
        // _units est déjà initialisé dans Awake() et partagé avec GridRepository
        Unit instantiatedPlayerUnit = null;

        // 1. Instancie le champion sélectionné (si disponible)
        if (ChampionSelectManager.SelectedChampion != null)
        {
            GameObject playerUnitGO = Instantiate(ChampionSelectManager.SelectedChampion.prefab);
            
            // On récupère le composant spécifique du champion (ex: IlyaUnit) pour appeler son initialisation.
            // NOTE: Ceci suppose que le prefab du champion a un script dérivé de Unit (comme IlyaUnit) qui gère son initialisation.
            instantiatedPlayerUnit = playerUnitGO.GetRequiredComponent<IlyaUnit>("Champion sélectionné");

            if (instantiatedPlayerUnit != null)
            {
                // Initialise le champion du joueur avec ses données et la position de départ
                (instantiatedPlayerUnit as IlyaUnit)?.Initialize(ChampionSelectManager.SelectedChampion, _playerSpawnGridPos);

                // Initialise le DeckManager de l'unité joueur (OPTIMISATION Phase 3.3: ComponentLocator)
                DeckManager playerDeckManager = instantiatedPlayerUnit.GetRequiredComponent<DeckManager>("DeckManager du champion");
                if (playerDeckManager != null && ChampionSelectManager.SelectedChampion != null)
                {
                    if (ChampionSelectManager.SelectedChampion.startingDeck != null && ChampionSelectManager.SelectedChampion.startingDeck.Count > 0)
                    {
                        playerDeckManager.InitializeDeck(ChampionSelectManager.SelectedChampion.startingDeck);
                        Debug.Log($"Deck de {instantiatedPlayerUnit.name} initialisé avec {ChampionSelectManager.SelectedChampion.startingDeck.Count} cartes.");
                    }
                    else
                    {
                        Debug.LogWarning($"Le champion {instantiatedPlayerUnit.name} n'a pas de cartes de départ dans son ChampionData.");
                    }
                }
                else
                {
                    Debug.LogWarning($"L'unité {instantiatedPlayerUnit.name} n'a pas de composant DeckManager.");
                }
                _units.Add(instantiatedPlayerUnit);
                _activeUnit = instantiatedPlayerUnit;
                Debug.Log($"Champion sélectionné instancié : {instantiatedPlayerUnit.name} à {_playerSpawnGridPos}");
            }
        }
        else
        {
            Debug.LogWarning("Aucun champion sélectionné. Le jeu commencera sans unité joueur initialement.");
        }

        // 2. Trouve toutes les autres unités (ennemis) déjà présentes dans la scène
        // et les ajoute à la liste, en s'assurant de les initialiser si elles ne l'ont pas été.
        Unit[] existingUnitsInScene = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in existingUnitsInScene)
        {
            if (!_units.Contains(unit)) // Évite d'ajouter le joueur si déjà instancié
            {
                // Si un champion a été instancié, on désactive les unités "placeholder" (Unit de base)
                // pour éviter d'avoir un cube qui traîne (le cube "Player" de la scène).
                if (instantiatedPlayerUnit != null && unit.GetType() == typeof(Unit))
                {
                    Debug.Log($"Unité placeholder ignorée: {unit.name}");
                    unit.gameObject.SetActive(false);
                    continue;
                }

                // Vérifie si c'est un Enemy avec EnemyData
                Enemy enemy = unit as Enemy;
                if (enemy != null)
                {
                    // Initialise l'ennemi avec EnemyData si pas encore fait
                    if (!enemy.IsInitialized() && enemy.GetEnemyData() != null)
                    {
                        enemy.InitializeEnemy(enemy.GetEnemyData(), GetGridPosFromWorldPos(enemy.transform.position));
                        Debug.Log($"Ennemi initialisé: {enemy.name}");
                    }

                    // Notifie le BattleUIManager pour connecter les UI
                    Debug.Log($"GridManager: Tentative de connexion UI pour {enemy.name}...");
                    Debug.Log($"  - BattleUIManager.Instance existe: {BattleUIManager.Instance != null}");
                    Debug.Log($"  - enemy.IsBoss(): {enemy.IsBoss()}");
                    Debug.Log($"  - enemy.GetEnemyData(): {enemy.GetEnemyData()?.enemyName}");

                    if (BattleUIManager.Instance != null)
                    {
                        Debug.Log($"GridManager: Appel de OnEnemySpawned pour {enemy.name}");
                        BattleUIManager.Instance.OnEnemySpawned(enemy);
                    }
                    else
                    {
                        Debug.LogError("GridManager: BattleUIManager.Instance est NULL!");
                    }
                }
                // Sinon, c'est une autre unité (un champion placé dans la scène)
                else if (!unit.IsInitialized())
                {
                    // Tente d'initialiser comme un champion (ex: IlyaUnit)
                    IlyaUnit championInScene = unit as IlyaUnit;
                    if (championInScene != null && championInScene.championData != null)
                    {
                        championInScene.Initialize(championInScene.championData, GetGridPosFromWorldPos(unit.transform.position));
                        Debug.Log($"Champion de la scène initialisé: {championInScene.name}");
                    }
                }
                _units.Add(unit);
            }
        }
        
        if (_units.Count > 0)
        {
            // Si aucun champion n'a été sélectionné ET qu'il n'y a pas d'unité active, prend la première unité trouvée comme active
            if (_activeUnit == null)
            {
                _activeUnit = _units[0];
            }
            
            Debug.Log($"Unité active initiale : {_activeUnit.name}");

            // Événements
            _activeUnit.OnMovementStepCompleted += HandleUnitMovementStep;

            foreach (Unit unit in _units)
            {
                unit.OnUnitDied += HandleUnitDied;
            }

            // Initialisation de l'unité active
            RefreshActiveUnitTurn();

            // Affichage initial
            UpdateUnitUI();
            DisplayMovementRange(_activeUnit);

            // Démarre la battle avec la TurnStateMachine (Phase 3.4)
            _turnStateMachine.StartBattle(_activeUnit);

            // Phase 3.4: Met la première unité en état Active
            UnitState initialState = _activeUnit.GetUnitState();
            if (initialState != null)
            {
                initialState.SetActive();
            }
            else
            {
                Debug.LogWarning($"GridManager.InitUnits: {_activeUnit.name} n'a pas de UnitState!");
            }

            // Gère le premier tour
            HandleTurnStart(_activeUnit);
        }
        else
        {
            Debug.LogWarning("Aucune unité (joueur ou ennemi) trouvée dans la scène.");
        }
    }
    
    /// <summary>
    /// Rafraîchit le tour de l'unité active (PA, Mouvement, et Pioche)
    /// </summary>
    private void RefreshActiveUnitTurn()
    {
        if (_activeUnit == null) return;

        // Rafraîchit les PM pour toutes les unités
        _activeUnit.RefreshMovement();
        Debug.Log($"{_activeUnit.name} : PM rafraîchis ({_activeUnit.GetCurrentMovementPoints()}/{_activeUnit.GetMaxMovementPoints()})");

        // Si c'est IlyaUnit, rafraîchit aussi les PA
        IlyaUnit ilyaUnit = _activeUnit as IlyaUnit;
        if (ilyaUnit != null)
        {
            ilyaUnit.RefreshPA();
            Debug.Log($"{ilyaUnit.name} : PA rafraîchis ({ilyaUnit.GetCurrentPA()}/{ilyaUnit.GetMaxPA()})");
        }

        // Pioche une carte si l'unité a un DeckManager (unités joueur uniquement)
        // OPTIMISATION Phase 3.3: ComponentLocator (optionnel car les ennemis n'ont pas de DeckManager)
        if (_activeUnit.TryGetComponentSafe(out DeckManager deckManager))
        {
            deckManager.DrawCard();
            Debug.Log($"{_activeUnit.name} : Pioche une carte au début du tour");
        }
    }

    // ===== SYSTÈME DE TOURS =====
    
    public void NextTurn()
    {
        // Phase 3.4: Commence la transition entre tours
        _turnStateMachine.BeginTurnTransition();

        int currentIndex = _units.IndexOf(_activeUnit);
        int nextIndex = (currentIndex + 1) % _units.Count;

        Unit previousUnit = _activeUnit;

        // Désabonne l'ancienne unité
        if (_activeUnit != null)
        {
            _activeUnit.OnMovementStepCompleted -= HandleUnitMovementStep;
        }

        _activeUnit = _units[nextIndex];
        Debug.Log($"=== Tour de : {_activeUnit.name} ===");

        // Phase 3.4: Met l'ancienne unité en état Idle
        if (previousUnit != null)
        {
            UnitState prevState = previousUnit.GetUnitState();
            if (prevState != null)
            {
                prevState.SetIdle();
            }
        }

        // Phase 3.4: Met la nouvelle unité active en état Active
        UnitState activeState = _activeUnit.GetUnitState();
        if (activeState != null)
        {
            activeState.SetActive();
        }
        else
        {
            Debug.LogWarning($"GridManager.NextTurn: {_activeUnit.name} n'a pas de UnitState!");
        }

        // Invalide tous les caches (OPTIMISATION: nouvel état de jeu)
        _gridRepository.InvalidateAllCaches();

        // Publie l'événement de changement de tour
        EventBus.Publish(new TurnChangedEvent(_activeUnit, previousUnit));

        // Rafraîchit la nouvelle unité
        RefreshActiveUnitTurn();

        // Réabonne aux événements
        _activeUnit.OnMovementStepCompleted += HandleUnitMovementStep;

        // Met à jour l'affichage
        UpdateUnitUI();
        DisplayMovementRange(_activeUnit);

        // Phase 3.4: Transition vers le nouvel état (PlayerTurn ou EnemyTurn)
        if (_activeUnit.GetFaction() == Unit.UnitFaction.Player)
        {
            _turnStateMachine.BeginPlayerTurn(_activeUnit);
        }
        else
        {
            _turnStateMachine.BeginEnemyTurn(_activeUnit);
        }

        // Gère le début de tour (joueur ou IA)
        HandleTurnStart(_activeUnit);
    }
    
    public void OnEndTurnButtonClick()
    {
        Debug.Log("=== Fin de tour ===");
        ResetAllTileColors();
        NextTurn();
    }
    
    // ===== GESTION INPUT/IA =====
    
    private void HandleTurnStart(Unit unit)
    {
        // Appliquer les effets de début de tour (transformation)
        // OPTIMISATION Phase 3.3: ComponentLocator (optionnel car toutes les unités n'ont pas EmotionSystem)
        if (unit.TryGetComponentSafe(out EmotionSystem emotionSystem))
        {
            emotionSystem.ApplyTurnEffects();
        }

        if (unit.GetFaction() == Unit.UnitFaction.Player)
        {
            _inputManager.enabled = true;
            if (_endTurnButton != null) _endTurnButton.interactable = true;
            Debug.Log($"Tour du joueur : {unit.name}");
        }
        else // Ennemi
        {
            _inputManager.enabled = false;
            if (_endTurnButton != null) _endTurnButton.interactable = false;
            // OPTIMISATION Phase 3.3: ComponentLocator
            if (unit.TryGetComponentSafe(out EnemyAI enemyAI))
            {
                StartCoroutine(ExecuteEnemyTurn(enemyAI, 1.0f));
            }
            else
            {
                Debug.LogError($"{unit.name} n'a pas de composant EnemyAI !");
                OnEndTurnButtonClick();
            }
        }
    }
    
    private IEnumerator ExecuteEnemyTurn(EnemyAI enemyAI, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemyAI.TakeTurn();
    }
    
    // ===== ÉVÉNEMENTS UNITÉS =====
    
    private void HandleUnitDied(Unit diedUnit)
    {
        Debug.Log($"{diedUnit.name} est mort.");
        
        diedUnit.OnMovementStepCompleted -= HandleUnitMovementStep;
        diedUnit.OnUnitDied -= HandleUnitDied;
        
        _units.Remove(diedUnit);
        
        if (_activeUnit == diedUnit)
        {
            Debug.Log("L'unité active est morte. Passage au tour suivant.");
            OnEndTurnButtonClick();
        }
        else
        {
            UpdateUnitUI();
            DisplayMovementRange(GetActiveUnit());
        }
    }
    
    private void HandleUnitMovementStep()
    {
        ResetAllTileColors();
        UpdateUnitUI();
        DisplayMovementRange(GetActiveUnit());
    }
    
    // ===== UI =====
    
    public void UpdateUnitUI()
    {
        Unit activeUnit = GetActiveUnit();
        if (activeUnit == null) return;

        // Utilise le cache pour éviter FindFirstObjectByType à chaque tour (OPTIMISATION)
        if (_cachedStatsUI == null)
        {
            _cachedStatsUI = FindFirstObjectByType<UnitStatsUI>();
            if (_cachedStatsUI == null)
            {
                Debug.LogWarning("UnitStatsUI introuvable ! Ajoute le script sur le Canvas.");
                return;
            }
        }

        _cachedStatsUI.SetUnit(activeUnit);
    }
    
    // ===== AFFICHAGE PORTÉES =====
    
    public void DisplayMovementRange(Unit unit)
    {
        if (unit == null) return;
        if (unit.GetFaction() != Unit.UnitFaction.Player) return;

        // Récupère la portée de mouvement (PM pour toutes les unités)
        int range = unit.GetCurrentMovementPoints();

        Dictionary<Tile, int> movementTilesWithCost = GetMovementTiles(
            unit.GetCurrentGridPos(),
            range,
            unit
        );

        // Retire la tuile actuelle
        Tile currentTile = GetTileAtPosition(unit.GetCurrentGridPos());
        if (currentTile != null && movementTilesWithCost.ContainsKey(currentTile))
        {
            movementTilesWithCost.Remove(currentTile);
        }

        if (movementTilesWithCost.Count == 0) return;

        // Colorie uniquement avec la couleur de mouvement
        foreach (var entry in movementTilesWithCost)
        {
            Tile tile = entry.Key;
            tile.SetColor(_moveColor); // Mouvement seul
        }
    }
    
    // Méthode supprimée - l'affichage de la portée d'attaque n'est plus utilisé
    // Toutes les attaques se font via les cartes qui ont leur propre système d'affichage de portée
    
    public void ShowMovementRange(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("ShowMovementRange: unit est null");
            return;
        }

        Debug.Log($"ShowMovementRange appelé pour {unit.name}");
        ResetAllTileColors();
        DisplayMovementRange(unit);

        Debug.Log($"Portée de mouvement affichée pour {unit.name} : {unit.GetCurrentMovementPoints()}/{unit.GetMaxMovementPoints()} PM");
    }
    
    // ===== PATHFINDING & PORTÉES =====
    // Toutes les méthodes de pathfinding délèguent maintenant au GridRepository

    public Dictionary<Tile, int> GetMovementTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
        => _gridRepository.GetMovementTiles(startPos, range, ignoreUnit);

    public List<Tile> GetAttackTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
        => _gridRepository.GetAttackTiles(startPos, range, ignoreUnit);

    public List<Tile> GetPathToTile(Vector2 startPos, Vector2 targetPos, int maxRange, Unit ignoreUnit = null)
        => _gridRepository.GetPathToTile(startPos, targetPos, maxRange, ignoreUnit);
    
    public void ResetAllTileColors()
    {
        foreach (var entry in _tiles)
        {
            Vector2 pos = entry.Key;
            Tile tile = entry.Value;
            bool isOffset = (pos.x % 2 == 0 && pos.y % 2 != 0) || (pos.x % 2 != 0 && pos.y % 2 == 0);
            tile.ResetColor(isOffset);
        }
    }
    
    // ===== GETTERS PUBLICS =====
    // Toutes les méthodes de requête délèguent maintenant au GridRepository
    // pour améliorer la testabilité et réduire le couplage

    public Unit GetActiveUnit() => _activeUnit;

    /// <summary>
    /// Retourne le GridRepository pour injection de dépendances (Phase 2)
    /// </summary>
    public GridRepository GetGridRepository() => _gridRepository;

    /// <summary>
    /// Invalide le cache d'attaque (appelé quand une carte est sélectionnée/désélectionnée)
    /// </summary>
    public void InvalidateAttackTilesCache() => _gridRepository.InvalidateAttackTilesCache();

    public Tile GetTileAtPosition(Vector2 pos) => _gridRepository.GetTileAtPosition(pos);

    public Vector2 GetGridPosFromWorldPos(Vector3 worldPos) => _gridRepository.GetGridPosFromWorldPos(worldPos);

    public List<Unit> GetAllPlayerUnits() => _gridRepository.GetAllPlayerUnits();

    public List<Unit> GetAllEnemyUnits() => _gridRepository.GetAllEnemyUnits();

    public Unit GetUnitAtGridPos(Vector2 gridPos) => _gridRepository.GetUnitAtGridPos(gridPos);

    /// <summary>
    /// Retourne la TurnStateMachine (Phase 3.4)
    /// </summary>
    public TurnStateMachine GetTurnStateMachine() => _turnStateMachine;

    // ===== SYSTÈME DE CIBLAGE DE CARTES =====

    /// <summary>
    /// Affiche les cibles valides pour une carte donnée
    /// </summary>
    public void ShowCardTargets(CardData card, Unit source)
    {
        if (card == null || source == null) return;

        ResetAllTileColors();

        Vector2 sourcePos = source.GetCurrentGridPos();
        int range = card.targetRange;

        // Obtient toutes les tuiles dans la portée de la carte
        List<Tile> tilesInRange = GetAttackTiles(sourcePos, range, source);

        // Colorie TOUTES les tuiles dans la portée en jaune
        foreach (Tile tile in tilesInRange)
        {
            tile.SetColor(_cardTargetColor);
        }

        Debug.Log($"Portée affichée pour {card.cardName} (portée: {range})");
    }

    /// <summary>
    /// Affiche la zone AOE autour d'une position donnée
    /// </summary>
    public void ShowAOEZone(Vector2 epicenter, int radius, CardData card, Unit source)
    {
        if (radius <= 0) return;

        // Obtient toutes les tuiles dans le rayon AOE
        List<Tile> aoeArea = GetAttackTiles(epicenter, radius, null);

        foreach (Tile tile in aoeArea)
        {
            Vector2 tilePos = GetGridPosFromWorldPos(tile.transform.position);
            Unit unitOnTile = GetUnitAtGridPos(tilePos);

            // Colore différemment selon si une unité sera affectée
            bool willBeAffected = false;
            if (unitOnTile != null)
            {
                if (unitOnTile == source)
                {
                    willBeAffected = card.affectsSelf;
                }
                else if (unitOnTile.GetFaction() == source.GetFaction())
                {
                    willBeAffected = card.affectsAllies;
                }
                else
                {
                    willBeAffected = card.affectsEnemies;
                }
            }

            // Utilise une couleur différente si une unité sera affectée
            if (willBeAffected)
            {
                tile.SetColor(Color.red); // Rouge pour les unités qui seront touchées
            }
            else
            {
                tile.SetColor(_aoeColor); // Orange transparent pour la zone
            }
        }
    }

    /// <summary>
    /// Surligne une tuile spécifique (pour hover)
    /// </summary>
    public void HighlightTile(Vector2 tilePos, Color color)
    {
        Tile tile = GetTileAtPosition(tilePos);
        if (tile != null)
        {
            tile.SetColor(color);
        }
    }

    /// <summary>
    /// Obtient la liste de toutes les unités
    /// </summary>
    public List<Unit> GetAllUnits() => _gridRepository.GetAllUnits();
}