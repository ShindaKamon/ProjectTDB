using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GridManager adapté pour Émotions Tactics
/// Gère la grille carrée, les unités, le système de tours, et l'UI
/// Compatible avec Unit de base ET Champion
/// Phase 3.5: Implémente IGridService pour injection de dépendances
/// </summary>
public class GridManager : MonoBehaviour, IGridService
{
    [Header("=== Configuration Grille ===")]
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private GameObject _tilePrefab;
    [Tooltip("Murs de la salle de combat, comme en exploration (Quaternius Wall_Modular, Column, Window_Small2)")]
    [SerializeField] private GameObject _wallModel;
    [SerializeField] private GameObject _cornerModel;
    [SerializeField] private GameObject _windowModel;
    [Tooltip("Cases de départ des champions (en rouge pendant le placement) ; chaque joueur apparaît sur la case de son rang, puis peut changer de case")]
    [SerializeField] private Vector2Int[] _startCells =
    {
        new Vector2Int(3, 1), new Vector2Int(5, 1), new Vector2Int(7, 1),
        new Vector2Int(2, 0), new Vector2Int(4, 0), new Vector2Int(6, 0)
    };
    [Tooltip("Phase de placement avant le premier tour (vide = le combat démarre directement)")]
    [SerializeField] private PlacementPhase _placementPhase;
    private CombatCommandExecutor _commands;

    [Header("=== Couleurs Portées ===")]
    [SerializeField] private Color _moveColor = Color.blue;
    [SerializeField] private Color _cardTargetColor = Color.yellow; // Couleur pour les cibles de carte
    [SerializeField] private Color _aoeColor = new Color(1f, 0.5f, 0f, 0.7f); // Couleur pour la zone AOE (orange transparent)
    
    
    [Header("=== Managers ===")]
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Button _endTurnButton; // Référence au bouton Fin de Tour
    
    // ===== DONNÉES INTERNES =====
    private Dictionary<Vector2Int, Tile> _tiles;
    private List<Unit> _units;
    private readonly List<BedUnit> _encounterBeds = new List<BedUnit>(); // lits du combat de boss (BedHiding)
    private readonly HashSet<Unit> _unitsWhoPlayed = new HashSet<Unit>(); // unités ayant déjà eu un tour (pioche dès le 2e)
    private Unit _activeUnit;

    // ===== REPOSITORY PATTERN =====
    // GridRepository centralise toutes les requêtes de grille pour améliorer la testabilité
    private GridRepository _gridRepository;

    // ===== STATE MACHINE (Phase 3.4) =====
    // TurnStateMachine gère les états explicites du système de tours
    private TurnStateMachine _turnStateMachine;

    // ===== AWAKE & INIT =====

    private void Awake()
    {
        // Empêche les doublons : un seul GridManager peut être enregistré comme IGridService
        if (ServiceLocator.Instance.IsRegistered<IGridService>())
        {
            Destroy(gameObject);
            return;
        }

        _tiles = new Dictionary<Vector2Int, Tile>();
        _units = new List<Unit>();
        TerrainDarkness.Clear(); // nouveau combat : terrain normal
        GenerateGrid();

        // Initialise le GridRepository après la génération de la grille
        _gridRepository = new GridRepository(_tiles, _units, _width, _height);

        // Initialise la TurnStateMachine (Phase 3.4)
        _turnStateMachine = new TurnStateMachine();

        // Enregistre ce GridManager comme IGridService dans le ServiceLocator (Phase 3.5)
        ServiceLocator.Instance.Register<IGridService>(this);
        GameLog.Log("GridManager: Enregistré dans ServiceLocator comme IGridService");

        // Actions des joueurs (déplacement, cartes, fin de tour, placement) : voir CombatCommandExecutor
        _commands = gameObject.AddComponent<CombatCommandExecutor>();
        _commands.Init(this, _placementPhase);

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
        GameLog.Log($"[EventBus] Fin de tour demandée par {e.RequestingUnit?.name ?? "Inconnu"}");
        EndActiveTurn();
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

                _tiles[new Vector2Int(x, y)] = tile;
            }
        }
        
        transform.position = Vector3.zero;
        RoomDecor.Build(transform, new Vector2Int(_width, _height), cell => _tiles[cell].transform.position,
            System.Array.Empty<Vector2Int>(), _wallModel, _cornerModel, _windowModel, null);
        GameLog.Log($"Grille générée : {_width}x{_height} = {_tiles.Count} tuiles");
    }

    // ===== GESTION UNITÉS =====
    
    private void InitUnits()
    {
        // _units est déjà initialisé dans Awake() et partagé avec GridRepository

        // 1. Instancie les champions de l'équipe sur les cases de départ, dans l'ordre des joueurs
        // (= ordre des tours dans _units)
        int playerCount = Mathf.Min(CombatParty.Count, _startCells.Length);
        var champions = new List<Champion>();
        for (int i = 0; i < playerCount; i++)
        {
            CombatParty.Member member = CombatParty.Members[i];
            // Utiliser le deck personnalisé s'il existe, sinon le startingDeck
            var deckToUse = member.Deck != null && member.Deck.Count > 0 ? member.Deck : member.Champion.startingDeck;
            Champion champion = SpawnChampion(member.Champion, deckToUse, _startCells[i]);
            if (champion != null) champions.Add(champion);
        }

        if (playerCount == 0)
        {
            GameLog.LogWarning("Aucun champion sélectionné. Le jeu commencera sans unité joueur initialement.");
        }

        // Donjon : les monstres de la rencontre remplacent ceux posés dans la scène
        if (DungeonRun.CurrentEncounter != null) SpawnEncounter(DungeonRun.CurrentEncounter);

        // 2. Trouve toutes les autres unités (ennemis) déjà présentes dans la scène
        // et les ajoute à la liste, en s'assurant de les initialiser si elles ne l'ont pas été.
        Unit[] existingUnitsInScene = FindObjectsByType<Unit>();
        // Ordre fixe (par position dans la scène) : c'est l'ordre des tours des monstres, qui doit
        // être le même sur tous les PC en réseau
        System.Array.Sort(existingUnitsInScene, (a, b) =>
        {
            Vector3 pa = a.transform.position, pb = b.transform.position;
            int byX = pa.x.CompareTo(pb.x);
            return byX != 0 ? byX : pa.z.CompareTo(pb.z);
        });
        foreach (Unit unit in existingUnitsInScene)
        {
            if (!_units.Contains(unit)) // Évite d'ajouter le joueur si déjà instancié
            {

                // Vérifie si c'est un Enemy avec EnemyData
                Enemy enemy = unit as Enemy;
                if (enemy != null)
                {
                    // Initialise l'ennemi avec EnemyData si pas encore fait
                    if (!enemy.IsInitialized() && enemy.GetEnemyData() != null)
                    {
                        enemy.InitializeEnemy(enemy.GetEnemyData(), GetGridPosFromWorldPos(enemy.transform.position));
                        GameLog.Log($"Ennemi initialisé: {enemy.name}");
                    }

                    // PV et dégâts selon le nombre de joueurs (1 en solo : barème inchangé)
                    enemy.ScaleForPlayers(playerCount);

                    // Réseau : tirages du monstre (lancers annoncés) communs à tous les PC, une graine par monstre
                    // (le nombre d'unités déjà ajoutées est le même partout, l'ordre des unités étant fixe)
                    if (CombatParty.Seed != 0) enemy.SetRandomSeed(CombatParty.Seed + 104729 * _units.Count);

                    // Notifie le BattleUIManager pour connecter les UI
                    GameLog.Log($"GridManager: Tentative de connexion UI pour {enemy.name}...");
                    GameLog.Log($"  - IBattleUIService disponible: {Services.IsBattleUIServiceAvailable()}");
                    GameLog.Log($"  - enemy.IsBoss(): {enemy.IsBoss()}");
                    GameLog.Log($"  - enemy.GetEnemyData(): {enemy.GetEnemyData()?.enemyName}");

                    if (Services.IsBattleUIServiceAvailable())
                    {
                        GameLog.Log($"GridManager: Appel de OnEnemySpawned pour {enemy.name}");
                        Services.BattleUI.OnEnemySpawned(enemy);
                    }
                    else
                    {
                        Debug.LogError("GridManager: IBattleUIService non enregistré!");
                    }
                }
                // Sinon, c'est une autre unité (un champion placé dans la scène)
                else if (!unit.IsInitialized())
                {
                    // Tente d'initialiser comme un champion
                    Champion championInScene = unit as Champion;
                    if (championInScene != null && championInScene.championData != null)
                    {
                        championInScene.Initialize(championInScene.championData, GetGridPosFromWorldPos(unit.transform.position));
                        GameLog.Log($"Champion de la scène initialisé: {championInScene.name}");
                    }
                }
                _units.Add(unit);
            }
        }

        // Combat de boss avec lits : le boss se cache dessous, ses PV (déjà adaptés au nombre de joueurs) répartis entre eux
        if (_encounterBeds.Count > 0)
        {
            Enemy boss = _units.Find(u => u is Enemy e && e.IsBoss()) as Enemy;
            if (boss != null) boss.gameObject.AddComponent<BedHiding>().Begin(boss, _encounterBeds);
        }

        // 3. Placement des champions (façon Dofus) avant le premier tour ; le boss garde sa case.
        // Pas d'unité active pendant le placement : InputManager ne réagit pas.
        if (_placementPhase != null && champions.Count > 0)
            _placementPhase.Begin(champions, _startCells, () => StartBattle(champions[0]));
        else
            StartBattle(champions.Count > 0 ? champions[0] : null);
    }

    /// <summary>
    /// Remplace les monstres posés dans la scène par ceux d'une rencontre de donjon.
    /// </summary>
    private void SpawnEncounter(EncounterData encounter)
    {
        foreach (Enemy sceneEnemy in FindObjectsByType<Enemy>())
        {
            sceneEnemy.gameObject.SetActive(false); // Destroy n'agit qu'en fin de frame
            Destroy(sceneEnemy.gameObject);
        }

        foreach (EncounterData.Spawn spawn in encounter.enemies)
        {
            if (spawn.enemy == null || spawn.enemy.prefab == null)
            {
                Debug.LogError($"Rencontre « {encounter.encounterName} » : monstre ou prefab manquant.");
                continue;
            }

            Enemy enemy = Instantiate(spawn.enemy.prefab).GetRequiredComponent<Enemy>("Monstre de la rencontre");
            if (enemy != null) enemy.InitializeEnemy(spawn.enemy, spawn.cell);
        }

        // Lits, tête vers le mur du fond le plus proche (nord ou est), pied vers la salle ; PV fixés ensuite par BedHiding
        _encounterBeds.Clear();
        if (encounter.bedPrefab == null) return;
        foreach (Vector2Int cell in encounter.bedCells)
        {
            BedUnit bed = Instantiate(encounter.bedPrefab).GetRequiredComponent<BedUnit>("Lit de la rencontre");
            if (bed == null) continue;
            Vector2Int wall = _height - 1 - cell.y <= _width - 1 - cell.x ? Vector2Int.up : Vector2Int.right;
            bed.InitializeBed(cell, wall, 1);
            _encounterBeds.Add(bed);
        }
    }

    /// <summary>
    /// Démarre le premier tour (après la phase de placement s'il y en a une).
    /// </summary>
    private void StartBattle(Unit firstUnit)
    {
        _activeUnit = firstUnit;

        if (_units.Count > 0)
        {
            // Si aucun champion n'a été sélectionné ET qu'il n'y a pas d'unité active, prend la première unité trouvée comme active
            if (_activeUnit == null)
            {
                _activeUnit = _units[0];
            }
            
            GameLog.Log($"Unité active initiale : {_activeUnit.name}");

            // Événements
            _activeUnit.OnMovementStepCompleted += HandleUnitMovementStep;

            foreach (Unit unit in _units)
            {
                unit.OnUnitDied += HandleUnitDied;
            }

            // Initialisation de l'unité active
            RefreshActiveUnitTurn();

            // Affichage initial
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
                GameLog.LogWarning($"GridManager.InitUnits: {_activeUnit.name} n'a pas de UnitState!");
            }

            // Les UI qui suivent le champion actif (main, HUD, orbe…) se branchent sur le premier tour
            EventBus.Publish(new TurnChangedEvent(_activeUnit, null));

            // Gère le premier tour
            HandleTurnStart(_activeUnit);
        }
        else
        {
            GameLog.LogWarning("Aucune unité (joueur ou ennemi) trouvée dans la scène.");
        }
    }
    
    /// <summary>
    /// Instancie un champion joueur, l'initialise avec son deck et l'ajoute à _units.
    /// </summary>
    private Champion SpawnChampion(ChampionData data, List<CardData> deck, Vector2Int gridPos)
    {
        GameObject playerUnitGO = Instantiate(data.prefab);

        // On récupère le composant Champion pour appeler son initialisation.
        // NOTE: Le prefab du champion doit avoir un script dérivé de Champion (EvanUnit, CruxUnit, RazeUnit)
        Champion champion = playerUnitGO.GetRequiredComponent<Champion>("Champion sélectionné");
        if (champion == null) return null;

        // Initialise le champion du joueur avec ses données et la position de départ
        champion.Initialize(data, gridPos);

        // Initialise le DeckManager de l'unité joueur (OPTIMISATION Phase 3.3: ComponentLocator)
        DeckManager playerDeckManager = champion.GetRequiredComponent<DeckManager>("DeckManager du champion");
        if (playerDeckManager != null)
        {
            if (deck != null && deck.Count > 0)
            {
                // Réseau : mélange commun à tous les PC (graine de l'hôte, une par joueur)
                if (CombatParty.Seed != 0)
                    playerDeckManager.SetShuffleSeed(CombatParty.Seed + 7919 * CombatParty.IndexOf(data));
                playerDeckManager.InitializeDeck(deck);
                GameLog.Log($"Deck de {champion.name} initialisé avec {deck.Count} cartes.");
            }
            else
            {
                GameLog.LogWarning($"Le champion {champion.name} n'a pas de deck valide.");
            }
        }
        else
        {
            GameLog.LogWarning($"L'unité {champion.name} n'a pas de composant DeckManager.");
        }

        _units.Add(champion);
        GameLog.Log($"Champion instancié : {champion.name} à {gridPos}");
        return champion;
    }

    /// <summary>
    /// Rafraîchit le tour de l'unité active (PA, Mouvement, et Pioche)
    /// </summary>
    private void RefreshActiveUnitTurn()
    {
        if (_activeUnit == null) return;

        // Rafraîchit les PM pour toutes les unités
        _activeUnit.RefreshMovement();
        GameLog.Log($"{_activeUnit.name} : PM rafraîchis ({_activeUnit.GetCurrentMovementPoints()}/{_activeUnit.GetMaxMovementPoints()})");

        // Rafraîchit les PA de toute unité qui en a (champions et monstres), avant les retraits de
        // PA/PM appliqués ensuite dans HandleTurnStart
        if (_activeUnit is IActionPointsUser paUser)
        {
            paUser.RefreshPA();
            GameLog.Log($"{_activeUnit.name} : PA rafraîchis ({paUser.GetCurrentPA()}/{paUser.GetMaxPA()})");
        }

        // Pioche une carte si l'unité a un DeckManager (unités joueur uniquement)
        // OPTIMISATION Phase 3.3: ComponentLocator (optionnel car les ennemis n'ont pas de DeckManager)
        // Pas de pioche au premier tour de chaque champion : sa main de départ (5) suffit
        bool firstTurn = _unitsWhoPlayed.Add(_activeUnit);
        if (_activeUnit.TryGetComponentSafe(out DeckManager deckManager))
        {
            // Coûts modifiés (ex: Triche) non joués : retour au coût normal
            deckManager.ClearAllCostOverrides();

            if (!firstTurn)
            {
                deckManager.DrawCard();
                GameLog.Log($"{_activeUnit.name} : Pioche une carte au début du tour");
            }
        }
    }

    // ===== SYSTÈME DE TOURS =====
    
    public void NextTurn()
    {
        if (_units.Count == 0)
        {
            GameLog.LogWarning("GridManager.NextTurn: plus aucune unité sur le terrain, arrêt de la rotation des tours.");
            return;
        }

        // Phase 3.4: Commence la transition entre tours
        _turnStateMachine.BeginTurnTransition();

        int currentIndex = _units.IndexOf(_activeUnit);
        int nextIndex = (currentIndex + 1) % _units.Count;

        // Les unités sans tour propre (invocations comme Lyse : PA/PM = 0,
        // aucun DeckManager) : elles sont enregistrées dans _units pour les requêtes de grille
        // mais doivent être sautées dans la rotation des tours, sous peine de bloquer le joueur
        // sur un tour vide qu'il ne peut que passer.
        int skipGuard = 0;
        while (!_units[nextIndex].TakesTurns && skipGuard < _units.Count)
        {
            nextIndex = (nextIndex + 1) % _units.Count;
            skipGuard++;
        }

        Unit previousUnit = _activeUnit;

        // Désabonne l'ancienne unité
        if (_activeUnit != null)
        {
            _activeUnit.OnMovementStepCompleted -= HandleUnitMovementStep;
        }

        _activeUnit = _units[nextIndex];
        GameLog.Log($"=== Tour de : {_activeUnit.name} ===");

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
            GameLog.LogWarning($"GridManager.NextTurn: {_activeUnit.name} n'a pas de UnitState!");
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
    
    /// <summary>
    /// Arrête le combat : plus aucun tour ne démarre, l'écran de fin (BattleEndUI) s'affiche.
    /// </summary>
    private void EndBattle(BattleResult result)
    {
        if (_turnStateMachine.IsBattleOver()) return;

        GameLog.Log($"=== Fin du combat : {(result == BattleResult.Victory ? "VICTOIRE" : "DÉFAITE")} ===");
        _turnStateMachine.EndBattle();
        ResetAllTileColors();
        EventBus.Publish(new BattleEndedEvent(result));
    }

    /// <summary>
    /// Bouton « Fin de tour » : action du joueur, qui passe par les commandes (voir CombatCommandExecutor)
    /// </summary>
    public void OnEndTurnButtonClick()
    {
        int actor = _commands != null ? _commands.ActiveActor : -1;
        if (actor >= 0 && _activeUnit != null) _commands.Submit(CombatCommand.EndTurn(actor));
    }

    /// <summary>
    /// Termine le tour de l'unité active : commande Fin de tour, fin du tour d'un monstre, mort de l'unité active
    /// </summary>
    public void EndActiveTurn()
    {
        // Phase de placement : pas encore d'unité active, rien à terminer ; combat fini : plus de tour
        if (_activeUnit == null || _turnStateMachine.IsBattleOver()) return;

        // Main au-delà du maximum : le joueur défausse d'abord l'excédent (au choix), puis le tour
        // se termine (l'UI de la main redemande la fin de tour). Pas pour une unité morte.
        if (_units.Contains(_activeUnit) && _activeUnit.TryGetComponentSafe(out DeckManager hand) && hand.ExcessCards > 0)
        {
            EventBus.Publish(new HandDiscardRequiredEvent(_activeUnit, hand.ExcessCards));
            return;
        }

        GameLog.Log("=== Fin de tour ===");
        ResetAllTileColors();
        NextTurn();
    }
    
    // ===== GESTION INPUT/IA =====
    
    private void HandleTurnStart(Unit unit)
    {
        // Passifs de l'unité liés au début de son tour (ex: combo de Raze, bouclier de Crux)
        unit.OnOwnTurnStart();

        // Effets posés par cette unité (buffs, malus, boucliers) : leur durée est comptée
        // en tours du lanceur, ils avancent donc au début de son tour
        foreach (Unit u in _units)
        {
            u.TickEffectsOnTurnStartOf(unit);
        }

        // Zones de fusion (ex. Appât d'Evan) : perte de PM pour qui commence son tour dedans
        FusionZones.ApplyOnTurnStart(unit);

        // Retraits de PA/PM programmés contre cette unité (après la remise à niveau de ses PA/PM)
        ResourceDebuffManager.ProcessDebuffsOnTurnStart(unit);

        if (unit.GetFaction() == Unit.UnitFaction.Player)
        {
            // Réseau : seul le PC du joueur dont c'est le tour peut agir ; les autres regardent
            bool local = _commands != null && _commands.IsLocalTurn;
            _inputManager.enabled = local;
            if (_endTurnButton != null) _endTurnButton.interactable = local;
            GameLog.Log($"Tour du joueur : {unit.name}{(local ? "" : " (autre PC)")}");
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
                EndActiveTurn();
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
        GameLog.Log($"{diedUnit.name} est mort.");
        
        diedUnit.OnMovementStepCompleted -= HandleUnitMovementStep;
        diedUnit.OnUnitDied -= HandleUnitDied;
        
        _units.Remove(diedUnit);

        // Fin du combat : plus d'ennemi (victoire) ou plus de champion (défaite)
        BattleResult result = BattleOutcome.Evaluate(_units);
        if (result != BattleResult.Ongoing)
        {
            EndBattle(result);
            return;
        }

        if (_activeUnit == diedUnit)
        {
            GameLog.Log("L'unité active est morte. Passage au tour suivant.");
            EndActiveTurn();
        }
        else
        {
            DisplayMovementRange(GetActiveUnit());
        }
    }
    
    private void HandleUnitMovementStep()
    {
        ResetAllTileColors();
        DisplayMovementRange(GetActiveUnit());
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
            GameLog.LogWarning("ShowMovementRange: unit est null");
            return;
        }

        GameLog.Log($"ShowMovementRange appelé pour {unit.name}");
        ResetAllTileColors();
        DisplayMovementRange(unit);

        GameLog.Log($"Portée de mouvement affichée pour {unit.name} : {unit.GetCurrentMovementPoints()}/{unit.GetMaxMovementPoints()} PM");
    }
    
    // ===== PATHFINDING & PORTÉES =====
    // Toutes les méthodes de pathfinding délèguent maintenant au GridRepository

    public Dictionary<Tile, int> GetMovementTiles(Vector2Int startPos, int range, Unit ignoreUnit = null)
        => _gridRepository.GetMovementTiles(startPos, range, ignoreUnit);

    public List<Tile> GetAttackTiles(Vector2Int startPos, int range, Unit ignoreUnit = null)
        => _gridRepository.GetAttackTiles(startPos, range, ignoreUnit);

    public List<Tile> GetPathToTile(Vector2Int startPos, Vector2Int targetPos, int maxRange, Unit ignoreUnit = null)
        => _gridRepository.GetPathToTile(startPos, targetPos, maxRange, ignoreUnit);

    public void ResetAllTileColors()
    {
        foreach (var entry in _tiles)
        {
            Vector2Int pos = entry.Key;
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

    /// <summary>
    /// Invalide le cache d'attaque (appelé quand une carte est sélectionnée/désélectionnée)
    /// </summary>
    public void InvalidateAttackTilesCache() => _gridRepository.InvalidateAttackTilesCache();

    public Tile GetTileAtPosition(Vector2Int pos) => _gridRepository.GetTileAtPosition(pos);

    public List<Vector2Int> GetAllCells()
    {
        var cells = new List<Vector2Int>(_tiles.Keys);
        cells.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        return cells;
    }

    public Vector2Int GetGridPosFromWorldPos(Vector3 worldPos) => _gridRepository.GetGridPosFromWorldPos(worldPos);

    public List<Unit> GetAllPlayerUnits() => _gridRepository.GetAllPlayerUnits();

    public List<Unit> GetAllEnemyUnits() => _gridRepository.GetAllEnemyUnits();

    public Unit GetUnitAtGridPos(Vector2Int gridPos) => _gridRepository.GetUnitAtGridPos(gridPos);

    /// <summary>
    /// Instancie et enregistre une invocation sur la grille (voir IGridService.SpawnSummon).
    /// </summary>
    public SummonUnit SpawnSummon(GameObject prefab, Vector2Int gridPos, Unit owner, int maxHealth)
    {
        if (prefab == null)
        {
            GameLog.LogWarning("SpawnSummon: prefab null.");
            return null;
        }

        if (!_tiles.ContainsKey(gridPos))
        {
            GameLog.LogWarning($"SpawnSummon: case {gridPos} hors grille.");
            return null;
        }

        if (GetUnitAtGridPos(gridPos) != null)
        {
            GameLog.LogWarning($"SpawnSummon: case {gridPos} déjà occupée.");
            return null;
        }

        GameObject summonGO = Instantiate(prefab);
        SummonUnit summon = summonGO.GetComponent<SummonUnit>();
        if (summon == null)
        {
            GameLog.LogWarning($"SpawnSummon: le prefab '{prefab.name}' n'a pas de composant SummonUnit.");
            Destroy(summonGO);
            return null;
        }

        summon.InitializeSummon(owner, gridPos, maxHealth);
        _units.Add(summon);
        _gridRepository.AddUnit(summon);
        summon.OnUnitDied += HandleUnitDied; // même nettoyage que les unités initiales (voir InitUnits)

        GameLog.Log($"Invocation créée : {summon.name} par {(owner != null ? owner.name : "inconnu")} à {gridPos}");
        return summon;
    }

    public Enemy SpawnEnemy(EnemyData data, Vector2Int gridPos)
    {
        if (data == null || data.prefab == null || !_tiles.ContainsKey(gridPos) || GetUnitAtGridPos(gridPos) != null)
        {
            GameLog.LogWarning($"SpawnEnemy : impossible de faire apparaître {data?.enemyName} en {gridPos}.");
            return null;
        }

        Enemy enemy = Instantiate(data.prefab).GetRequiredComponent<Enemy>("Monstre invoqué");
        if (enemy == null) return null;
        enemy.InitializeEnemy(data, gridPos);

        // Mêmes réglages que les monstres du départ (voir InitUnits) : nombre de joueurs, graine commune en réseau
        enemy.ScaleForPlayers(Mathf.Min(CombatParty.Count, _startCells.Length));
        if (CombatParty.Seed != 0) enemy.SetRandomSeed(CombatParty.Seed + 104729 * _units.Count);

        _units.Add(enemy);
        _gridRepository.AddUnit(enemy);
        enemy.OnUnitDied += HandleUnitDied;
        if (Services.IsBattleUIServiceAvailable()) Services.BattleUI.OnEnemySpawned(enemy);

        GameLog.Log($"Monstre invoqué : {enemy.name} en {gridPos}");
        return enemy;
    }

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

        // Carte qui cible une carte de la main (ex: Triche) : aucune case à montrer
        if (card.targetsHandCard) return;

        // Carte de déplacement d'invocation (ex: Écho évanescent), ciblage en 2 étapes :
        // source = lanceur -> étape 1, on montre ses invocations ; source = l'invocation choisie
        // -> étape 2, on montre les cases d'arrivée autour d'elle.
        if (card.isRepositionSummonCard)
        {
            if (source is SummonUnit chosenSummon)
                ShowSummonMoveTargets(card, chosenSummon);
            else
                ShowSummonsToMove(card, source);
            return;
        }

        Vector2Int sourcePos = source.GetCurrentGridPos();
        int range = card.targetRange;

        // Carte d'invocation alors que l'invocation est déjà là : seule sa case est ciblable (soin)
        if (GameActionValidator.HealsActiveSummon(card, source))
        {
            Tile summonTile = GetTileAtPosition(((ISummonOwner)source).ActiveSummon.GetCurrentGridPos());
            if (summonTile != null) summonTile.SetColor(_cardTargetColor);
            return;
        }

        // Pour les cartes de charge, affiche uniquement les cases en ligne droite
        if (card.isChargeCard)
        {
            ShowChargeTargets(card, sourcePos, range, source);
            return;
        }

        // Obtient toutes les tuiles dans la portée de la carte
        List<Tile> tilesInRange = GetAttackTiles(sourcePos, range, source);

        // Colorie TOUTES les tuiles dans la portée en jaune (seulement les lignes droites si la carte l'exige)
        foreach (Tile tile in tilesInRange)
        {
            if (card.targetInStraightLine && !GridGeometry.TryGetLine(sourcePos, GetGridPosFromWorldPos(tile.transform.position), out _, out _))
                continue;
            tile.SetColor(_cardTargetColor);
        }

        // Sa propre case n'est pas dans la portée (retirée par GetAttackTiles), mais une carte
        // « allié ou soi-même » (ex: Souffle apaisant) peut viser le lanceur
        if (range > 0 && card.targetsUnit && card.IsValidTarget(source, source))
        {
            Tile ownTile = GetTileAtPosition(sourcePos);
            if (ownTile != null) ownTile.SetColor(_cardTargetColor);
        }

        GameLog.Log($"Portée affichée pour {card.cardName} (portée: {range})");
    }

    /// <summary>Étape 1 d'une carte de déplacement d'invocation : surligne les invocations du lanceur.</summary>
    private void ShowSummonsToMove(CardData card, Unit caster)
    {
        foreach (Unit unit in _units)
        {
            if (!GameActionValidator.CanSelectSummonToMove(card, caster, unit).IsValid) continue;

            Tile tile = GetTileAtPosition(unit.GetCurrentGridPos());
            if (tile != null) tile.SetColor(_cardTargetColor);
        }
    }

    /// <summary>Étape 2 : surligne les cases d'arrivée valides autour de l'invocation choisie.</summary>
    private void ShowSummonMoveTargets(CardData card, SummonUnit summon)
    {
        foreach (var pair in _tiles)
        {
            bool isFree = GetUnitAtGridPos(pair.Key) == null;
            if (GameActionValidator.CanMoveSummonTo(card, summon, pair.Key, isFree).IsValid)
                pair.Value.SetColor(_cardTargetColor);
        }
    }

    /// <summary>
    /// Affiche les cibles valides pour une carte de charge (lignes droites uniquement)
    /// Affiche les cases vides ET les cases avec ennemis comme cibles valides (jaune)
    /// L'ennemi sera surligné en rouge uniquement au hover (géré par InputManager)
    /// </summary>
    private void ShowChargeTargets(CardData card, Vector2Int sourcePos, int range, Unit source)
    {
        // 4 directions : haut, bas, gauche, droite
        foreach (Vector2Int dir in GridGeometry.Directions4)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int targetPos = sourcePos + dir * i;
                Tile tile = GetTileAtPosition(targetPos);

                if (tile == null) break; // Bord de la grille

                Unit unitOnTile = GetUnitAtGridPos(targetPos);
                if (unitOnTile != null)
                {
                    // Il y a une unité
                    if (card.targetsUnit ? card.IsValidTarget(source, unitOnTile) : unitOnTile.GetFaction() != source.GetFaction())
                    {
                        // Cible valide (ennemi, ou allié si la carte cible une unité) : jaune comme
                        // les autres cases, la tuile rouge n'apparaîtra qu'au hover
                        tile.SetColor(_cardTargetColor);
                    }
                    // On s'arrête ici (on ne peut pas cibler au-delà d'une unité)
                    break;
                }

                // Case vide valide, on la colorie en jaune (pas pour une charge qui cible une unité)
                if (!card.targetsUnit) tile.SetColor(_cardTargetColor);
            }
        }

        GameLog.Log($"Portée de charge affichée (portée: {range}, lignes droites uniquement)");
    }

    /// <summary>
    /// Affiche la zone AOE de la carte (ligne, cercle, cône…) pour un épicentre donné
    /// </summary>
    public void ShowAOEZone(Vector2Int epicenter, int radius, CardData card, Unit source)
    {
        if (radius <= 0) return;

        // Même forme que celle utilisée pour appliquer l'effet (CardData.IsInAOEShape)
        foreach (var entry in _tiles)
        {
            Vector2Int tilePos = entry.Key;
            if (!card.IsInAOEShape(source, epicenter, tilePos)) continue;

            Tile tile = entry.Value;
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
    public void HighlightTile(Vector2Int tilePos, Color color)
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