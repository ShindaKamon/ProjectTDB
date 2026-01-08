using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// GridManager adapté pour Émotions Tactics
/// Gère la grille carrée, les unités, le système de tours, et l'UI
/// Compatible avec Unit de base ET IlyaUnit
/// </summary>
public class GridManager : MonoBehaviour
{
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
    
    // ===== DONNÉES INTERNES =====
    private Dictionary<Vector2, Tile> _tiles;
    private List<Unit> _units;
    private Unit _activeUnit;
    
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
        GenerateGrid();
    }

    private void Start()
    {
        // Initialise les unités dans Start() pour que le BattleUIManager ait le temps de s'initialiser dans Awake()
        InitUnits();
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

                var tile = spawnedTile.GetComponent<Tile>();
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
        _units = new List<Unit>();
        Unit instantiatedPlayerUnit = null;

        // 1. Instancie le champion sélectionné (si disponible)
        if (ChampionSelectManager.SelectedChampion != null)
        {
            GameObject playerUnitGO = Instantiate(ChampionSelectManager.SelectedChampion.prefab);
            instantiatedPlayerUnit = playerUnitGO.GetComponent<Unit>();
            if (instantiatedPlayerUnit != null)
            {
                // Initialise le champion du joueur avec ses données et la position de départ
                instantiatedPlayerUnit.Initialize(ChampionSelectManager.SelectedChampion, _playerSpawnGridPos, Unit.UnitFaction.Player);

                // Initialise le DeckManager de l'unité joueur
                DeckManager playerDeckManager = instantiatedPlayerUnit.GetComponent<DeckManager>();
                if (playerDeckManager != null)
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
                // Vérifie si c'est un Enemy avec EnemyData
                Enemy enemy = unit as Enemy;
                if (enemy != null)
                {
                    // Initialise l'ennemi avec EnemyData si pas encore fait
                    if (enemy.GetEnemyData() != null && enemy.GetHealth() == 0)
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
                // Sinon, c'est une Unit de base (pour compatibilité avec anciennes unités)
                else if (unit.championData != null && unit.GetHealth() == 0)
                {
                    unit.Initialize(unit.championData, GetGridPosFromWorldPos(unit.transform.position), unit.GetFaction());
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
        Debug.Log($"{_activeUnit.name} : PM rafraîchis ({_activeUnit.GetRemainingMovement()}/{_activeUnit.GetMovementRange()})");

        // Si c'est IlyaUnit, rafraîchit aussi les PA
        IlyaUnit ilyaUnit = _activeUnit as IlyaUnit;
        if (ilyaUnit != null)
        {
            ilyaUnit.RefreshPA();
            Debug.Log($"{ilyaUnit.name} : PA rafraîchis ({ilyaUnit.GetCurrentPA()}/{ilyaUnit.GetMaxPA()})");
        }

        // Pioche une carte si l'unité a un DeckManager (unités joueur uniquement)
        DeckManager deckManager = _activeUnit.GetComponent<DeckManager>();
        if (deckManager != null)
        {
            deckManager.DrawCard();
            Debug.Log($"{_activeUnit.name} : Pioche une carte au début du tour");
        }
    }

    // ===== SYSTÈME DE TOURS =====
    
    public void NextTurn()
    {
        int currentIndex = _units.IndexOf(_activeUnit);
        int nextIndex = (currentIndex + 1) % _units.Count;
        
        // Désabonne l'ancienne unité
        if (_activeUnit != null)
        {
            _activeUnit.OnMovementStepCompleted -= HandleUnitMovementStep;
        }
        
        _activeUnit = _units[nextIndex];
        Debug.Log($"=== Tour de : {_activeUnit.name} ===");
        
        // Rafraîchit la nouvelle unité
        RefreshActiveUnitTurn();
        
        // Réabonne aux événements
        _activeUnit.OnMovementStepCompleted += HandleUnitMovementStep;
        
        // Met à jour l'affichage
        UpdateUnitUI();
        DisplayMovementRange(_activeUnit);

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
        EmotionSystem emotionSystem = unit.GetComponent<EmotionSystem>();
        if (emotionSystem != null)
        {
            emotionSystem.ApplyTurnEffects();
        }

        if (unit.GetFaction() == Unit.UnitFaction.Player)
        {
            _inputManager.enabled = true;
            Debug.Log($"Tour du joueur : {unit.name}");
        }
        else // Ennemi
        {
            _inputManager.enabled = false;
            EnemyAI enemyAI = unit.GetComponent<EnemyAI>();
            if (enemyAI != null)
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
        
        // Utilise le nouveau système UI
        UnitStatsUI statsUI = FindFirstObjectByType<UnitStatsUI>();
        if (statsUI != null)
        {
            statsUI.SetUnit(activeUnit);
        }
        else
        {
            Debug.LogWarning("UnitStatsUI introuvable ! Ajoute le script sur le Canvas.");
        }
    }
    
    // ===== AFFICHAGE PORTÉES =====
    
    public void DisplayMovementRange(Unit unit)
    {
        if (unit == null) return;
        if (unit.GetFaction() != Unit.UnitFaction.Player) return;

        // Récupère la portée de mouvement (PM pour toutes les unités)
        int range = unit.GetRemainingMovement();

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

        Debug.Log($"Portée de mouvement affichée pour {unit.name} : {unit.GetRemainingMovement()}/{unit.GetMovementRange()} PM");
    }
    
    // ===== PATHFINDING & PORTÉES =====
    
    public Dictionary<Tile, int> GetMovementTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
    {
        Dictionary<Tile, int> reachableTilesWithCost = new Dictionary<Tile, int>();
        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, int> visited = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        visited[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2 currentPos = queue.Dequeue();
            int currentCost = visited[currentPos];
            Tile currentTile = GetTileAtPosition(currentPos);

            if (currentTile != null && !reachableTilesWithCost.ContainsKey(currentTile))
            {
                reachableTilesWithCost.Add(currentTile, currentCost);
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                Unit unitAtNeighbor = GetUnitAtGridPos(neighborPos);
                bool isOccupied = (unitAtNeighbor != null && unitAtNeighbor != ignoreUnit);

                if (_tiles.ContainsKey(neighborPos) && 
                    !visited.ContainsKey(neighborPos) && 
                    currentCost + 1 <= range && 
                    !isOccupied)
                {
                    visited[neighborPos] = currentCost + 1;
                    queue.Enqueue(neighborPos);
                }
            }
        }

        return reachableTilesWithCost;
    }
    
    public List<Tile> GetAttackTiles(Vector2 startPos, int range, Unit ignoreUnit = null)
    {
        List<Tile> reachableTiles = new List<Tile>();
        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, int> visited = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        visited[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2 currentPos = queue.Dequeue();
            int currentCost = visited[currentPos];
            Tile currentTile = GetTileAtPosition(currentPos);
            
            if (currentTile != null)
            {
                reachableTiles.Add(currentTile);
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                if (_tiles.ContainsKey(neighborPos) && 
                    !visited.ContainsKey(neighborPos) && 
                    currentCost + 1 <= range)
                {
                    visited[neighborPos] = currentCost + 1;
                    queue.Enqueue(neighborPos);
                }
            }
        }
        
        if (range > 0 && reachableTiles.Contains(GetTileAtPosition(startPos)))
        {
            reachableTiles.Remove(GetTileAtPosition(startPos));
        }
        
        return reachableTiles;
    }
    
    public List<Tile> GetPathToTile(Vector2 startPos, Vector2 targetPos, int maxRange, Unit ignoreUnit = null)
    {
        if (startPos == targetPos)
        {
            return new List<Tile>();
        }

        Queue<Vector2> queue = new Queue<Vector2>();
        Dictionary<Vector2, Vector2> parentMap = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, int> costMap = new Dictionary<Vector2, int>();

        queue.Enqueue(startPos);
        costMap[startPos] = 0;

        Vector2 currentPos;
        while (queue.Count > 0)
        {
            currentPos = queue.Dequeue();
            int currentCost = costMap[currentPos];

            if (currentPos == targetPos)
            {
                break;
            }

            Vector2[] neighbors = new Vector2[]
            {
                currentPos + new Vector2(0, 1),
                currentPos + new Vector2(0, -1),
                currentPos + new Vector2(1, 0),
                currentPos + new Vector2(-1, 0)
            };

            foreach (Vector2 neighborPos in neighbors)
            {
                Unit unitAtNeighbor = GetUnitAtGridPos(neighborPos);
                bool isOccupied = (unitAtNeighbor != null && unitAtNeighbor != ignoreUnit);

                if (_tiles.ContainsKey(neighborPos) && !isOccupied)
                {
                    int newCost = currentCost + 1;
                    
                    if (newCost <= maxRange && 
                        (!costMap.ContainsKey(neighborPos) || newCost < costMap[neighborPos]))
                    {
                        costMap[neighborPos] = newCost;
                        parentMap[neighborPos] = currentPos;
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }

        List<Tile> path = new List<Tile>();
        if (parentMap.ContainsKey(targetPos))
        {
            currentPos = targetPos;
            while (currentPos != startPos)
            {
                path.Add(GetTileAtPosition(currentPos));
                currentPos = parentMap[currentPos];
            }
            path.Reverse();
        }

        return path;
    }
    
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
    
    public Unit GetActiveUnit() => _activeUnit;
    
    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }
    
    public Vector2 GetGridPosFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x + _width / 2.0f);
        int y = Mathf.RoundToInt(worldPos.z + _height / 2.0f);
        return new Vector2(x, y);
    }
    
    public List<Unit> GetAllPlayerUnits()
    {
        List<Unit> playerUnits = new List<Unit>();
        foreach (Unit unit in _units)
        {
            if (unit.GetFaction() == Unit.UnitFaction.Player)
            {
                playerUnits.Add(unit);
            }
        }
        return playerUnits;
    }
    
    public Unit GetUnitAtGridPos(Vector2 gridPos)
    {
        foreach (Unit unit in _units)
        {
            if (unit.GetCurrentGridPos() == gridPos)
            {
                return unit;
            }
        }
        return null;
    }

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
    public List<Unit> GetAllUnits()
    {
        return _units;
    }
}