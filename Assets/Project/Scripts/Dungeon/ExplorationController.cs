using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Exploration d'une salle du donjon (façon Waven) : grille en vue isométrique (la même que le combat), un pion d'équipe qui
/// marche case par case au clic. Cliquer un groupe de monstres fait marcher le pion jusqu'à lui
/// puis lance le combat ; cliquer une porte change de salle. Tout est construit à l'exécution
/// depuis DungeonData (cases et personnages du combat, portes en formes simples).
/// </summary>
public class ExplorationController : MonoBehaviour
{
    [Tooltip("Même prefab de case que le combat (GridManager)")]
    [SerializeField] private GameObject _tilePrefab;
    [Tooltip("Pan de mur des salles (Quaternius Wall_Modular)")]
    [SerializeField] private GameObject _wallModel;
    [Tooltip("Pilier d'angle des salles (Quaternius Column)")]
    [SerializeField] private GameObject _cornerModel;
    [Tooltip("Fenêtre des murs (Quaternius Window_Small2)")]
    [SerializeField] private GameObject _windowModel;
    [Tooltip("Porte des murs (Quaternius Door3)")]
    [SerializeField] private GameObject _doorModel;
    [SerializeField] private string _combatSceneName = "CombatScene";
    [Tooltip("Vitesse du pion (cases par seconde)")]
    [SerializeField] private float _walkSpeed = 6f;

    private const float DoorOpenDuration = 0.9f;
    private static readonly Color MonsterColor = new Color(0.8f, 0.2f, 0.2f);
    private static readonly Color DoorColor = new Color(0.9f, 0.75f, 0.25f);
    private static readonly Color PartyColor = new Color(0.25f, 0.55f, 0.95f);

    private DungeonData.Room _room;
    private Transform _roomRoot;
    private Transform _party;
    private readonly List<Transform> _partyModels = new List<Transform>();
    private Vector2Int _partyCell;
    private bool _walking;
    private readonly Dictionary<Vector2Int, int> _monsterAt = new Dictionary<Vector2Int, int>();
    private readonly Dictionary<Vector2Int, DungeonData.Door> _doorAt = new Dictionary<Vector2Int, DungeonData.Door>();
    private Camera _camera;
    private MonsterPanel _monsterPanel;

    private void Start()
    {
        if (!DungeonRun.IsActive && DungeonRun.BeginFromResources("") == "")
        {
            Debug.LogError("Aucun donjon à explorer (Resources/Dungeons/Orphelinat manquant).");
            enabled = false;
            return;
        }

        _camera = Camera.main;
        _monsterPanel = gameObject.AddComponent<MonsterPanel>();
        _partyCell = DungeonRun.PartyCell;
        BuildRoom();
    }

    private void Update()
    {
        if (_walking || DungeonRun.IsCompleted) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance))
            OnCellClicked(WorldToCell(ray.GetPoint(distance)));
    }

    // ========== CLIC ==========

    private void OnCellClicked(Vector2Int cell)
    {
        if (cell.x < 0 || cell.y < 0 || cell.x >= _room.size.x || cell.y >= _room.size.y) return;

        if (_monsterAt.TryGetValue(cell, out int spot))
        {
            // Le pion s'arrête à côté du groupe, puis le combat démarre
            WalkThen(c => GridGeometry.AreAdjacent(c, cell), () => StartEncounter(spot));
        }
        else if (_doorAt.TryGetValue(cell, out DungeonData.Door door))
        {
            WalkThen(c => c == cell, () => EnterRoom(door));
        }
        else
        {
            WalkThen(c => c == cell, null);
        }
    }

    private void WalkThen(System.Func<Vector2Int, bool> isGoal, System.Action onArrived)
    {
        List<Vector2Int> path = ExplorationPathfinder.FindPath(_partyCell, _room.size,
            c => _monsterAt.ContainsKey(c), isGoal);
        if (path == null) return;
        StartCoroutine(Walk(path, onArrived));
    }

    private IEnumerator Walk(List<Vector2Int> path, System.Action onArrived)
    {
        _walking = true;
        SetPartyWalking(true);
        foreach (Vector2Int next in path)
        {
            Vector3 from = _party.position, to = CellToWorld(next) + Vector3.up * _party.position.y;
            Face(to - from);
            for (float t = 0f; t < 1f; t += Time.deltaTime * _walkSpeed)
            {
                _party.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
            _party.position = to;
            _partyCell = next;
        }
        _walking = false;
        SetPartyWalking(false);
        onArrived?.Invoke();
    }

    private void StartEncounter(int spot)
    {
        DungeonRun.StartEncounter(spot, _partyCell);
        SceneManager.LoadScene(_combatSceneName);
    }

    private void EnterRoom(DungeonData.Door door)
    {
        DungeonRun.MoveToRoom(door.targetRoom, door.arrivalCell);
        _partyCell = door.arrivalCell;
        BuildRoom();
    }

    // ========== CONSTRUCTION DE LA SALLE ==========

    private void BuildRoom()
    {
        if (_roomRoot != null) Destroy(_roomRoot.gameObject);
        _roomRoot = new GameObject("Room").transform;
        _monsterAt.Clear();
        _doorAt.Clear();

        _room = DungeonRun.Dungeon.rooms[DungeonRun.CurrentRoom];
        FrameCamera();
        for (int x = 0; x < _room.size.x; x++)
            for (int y = 0; y < _room.size.y; y++)
                MakeTile(new Vector2Int(x, y));

        // Les portes restent fermées (battant plein) tant que tous les groupes de la salle ne sont pas vaincus ;
        // elles s'ouvrent en s'animant juste après le dernier combat, sinon directement ouvertes
        var doorCells = new List<Vector2Int>();
        foreach (DungeonData.Door door in _room.doors) doorCells.Add(door.cell);
        Dictionary<Vector2Int, Transform> leaves = RoomDecor.Build(_roomRoot, _room.size, CellToWorld, doorCells, _wallModel, _cornerModel, _windowModel, _doorModel);

        if (DungeonRun.IsRoomCleared(DungeonRun.CurrentRoom))
        {
            bool animate = DungeonRun.ConsumeDoorsJustOpened();
            foreach (DungeonData.Door door in _room.doors)
            {
                leaves.TryGetValue(door.cell, out Transform leaf);
                if (animate && leaf != null) StartCoroutine(OpenDoor(door, leaf));
                else
                {
                    if (leaf != null) leaf.rotation = RoomDecor.OpenRotation(leaf);
                    ActivateDoor(door);
                }
            }
        }

        for (int i = 0; i < _room.monsters.Count; i++)
        {
            if (DungeonRun.IsDefeated(DungeonRun.CurrentRoom, i)) continue;
            DungeonData.MonsterSpot spot = _room.monsters[i];
            _monsterAt[spot.cell] = i;
            GameObject monsterPrefab = spot.encounter.enemies.Count > 0 ? spot.encounter.enemies[0].enemy.prefab : null;
            if (MakeModel(monsterPrefab, _roomRoot, CellToWorld(spot.cell), 1f, 180f) == null)
                Make(PrimitiveType.Capsule, CellToWorld(spot.cell) + Vector3.up * 0.9f, new Vector3(0.8f, 0.9f, 0.8f), MonsterColor);
        }
        _monsterPanel.Show(RemainingGroups());

        BuildParty();
    }

    // Le battant pivote vers l'extérieur, puis la porte devient cliquable
    private IEnumerator OpenDoor(DungeonData.Door door, Transform leaf)
    {
        Quaternion from = leaf.rotation, to = RoomDecor.OpenRotation(leaf);
        for (float t = 0f; t < 1f; t += Time.deltaTime / DoorOpenDuration)
        {
            leaf.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        leaf.rotation = to;
        ActivateDoor(door);
    }

    private void ActivateDoor(DungeonData.Door door)
    {
        _doorAt[door.cell] = door;
        Make(PrimitiveType.Cube, CellToWorld(door.cell) + Vector3.up * 0.05f, new Vector3(0.9f, 0.12f, 0.9f), DoorColor);
        Label("Porte : " + DungeonRun.Dungeon.rooms[door.targetRoom].roomName, CellToWorld(door.cell) + Vector3.up * 2.3f);
    }

    // Le pion d'équipe : le modèle de chaque champion du groupe, côte à côte (une capsule si le groupe est vide)
    private void BuildParty()
    {
        _party = new GameObject("Party").transform;
        _party.SetParent(_roomRoot, false);
        _party.position = CellToWorld(_partyCell);
        _partyModels.Clear();

        int count = CombatParty.Count;
        float scale = count > 1 ? 0.6f : 1f;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Vector3.right * (i - (count - 1) / 2f) * 0.4f;
            Transform model = MakeModel(CombatParty.Members[i].Champion.prefab, _party, offset, scale, 0f);
            if (model != null) _partyModels.Add(model);
        }
        if (_partyModels.Count == 0)
            Make(PrimitiveType.Capsule, _party.position + Vector3.up * 0.9f, new Vector3(0.7f, 0.9f, 0.7f), PartyColor).transform.SetParent(_party);
    }

    // Clone le visuel (enfant « Model ») d'un prefab de combat, sans ses scripts d'unité ; null s'il n'en a pas
    private static Transform MakeModel(GameObject prefab, Transform parent, Vector3 position, float scale, float yaw)
    {
        Transform model = prefab != null ? prefab.transform.Find("Model") : null;
        if (model == null) return null;

        Transform copy = Instantiate(model, parent);
        float rootScale = prefab.transform.localScale.x;
        copy.localScale = Vector3.one * rootScale * scale;
        // Même hauteur qu'en combat : la racine de l'unité flotte à 0,5 au-dessus de la case, le modèle est décalé sous elle
        copy.localPosition = position + Vector3.up * (0.5f + model.localPosition.y * rootScale);
        copy.rotation = Quaternion.Euler(0f, yaw, 0f);
        return copy;
    }

    private void SetPartyWalking(bool walking)
    {
        foreach (Transform model in _partyModels)
            if (model.TryGetComponent(out UnitAnimator animator)) animator.SetWalking(walking);
    }

    private void Face(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(direction);
        foreach (Transform model in _partyModels) model.rotation = look;
    }

    private void FrameCamera()
    {
        _camera.orthographic = true;
        // Même vue isométrique que CombatScene
        _camera.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
        _camera.transform.position = -_camera.transform.forward * 32f;
        _camera.orthographicSize = 6f;
    }

    private void MakeTile(Vector2Int cell)
    {
        GameObject tile = Instantiate(_tilePrefab, CellToWorld(cell), Quaternion.identity, _roomRoot);
        tile.GetComponent<Tile>().Init((cell.x + cell.y) % 2 == 1);
    }

    private GameObject Make(PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(_roomRoot, false);
        go.transform.position = position;
        go.transform.localScale = scale;

        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        go.GetComponent<Renderer>().SetPropertyBlock(block);
        return go;
    }

    private void Label(string text, Vector3 position)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(_roomRoot, false);
        go.transform.position = position;
        go.transform.rotation = _camera.transform.rotation;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1f);
    }

    // ========== CASES ↔ MONDE (salle centrée sur l'origine) ==========

    private Vector3 CellToWorld(Vector2Int cell) =>
        new Vector3(cell.x - (_room.size.x - 1) / 2f, 0f, cell.y - (_room.size.y - 1) / 2f);

    private Vector2Int WorldToCell(Vector3 world) =>
        new Vector2Int(Mathf.RoundToInt(world.x + (_room.size.x - 1) / 2f), Mathf.RoundToInt(world.z + (_room.size.y - 1) / 2f));

    private IEnumerable<EncounterData> RemainingGroups()
    {
        for (int i = 0; i < _room.monsters.Count; i++)
            if (!DungeonRun.IsDefeated(DungeonRun.CurrentRoom, i)) yield return _room.monsters[i].encounter;
    }

    // ========== INTERFACE ==========

    private void OnGUI()
    {
        if (_room == null) return;
        var big = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
        GUI.Label(new Rect(16, 12, 700, 40), $"{DungeonRun.Dungeon.dungeonName} — {_room.roomName}", big);
        // Fin du donjon : écran affiché dans le combat, après le dernier combat (BattleEndUI)
    }
}
