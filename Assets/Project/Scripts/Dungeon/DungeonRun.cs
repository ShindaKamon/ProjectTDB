using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie en cours dans un donjon (statique, comme CombatParty : elle survit aux changements de
/// scène exploration ↔ combat). Garde la salle courante, la case du pion, les monstres vaincus et
/// le combat à lancer. Sans donjon actif, CombatScene garde ses monstres de scène.
/// </summary>
public static class DungeonRun
{
    public const string ExplorationSceneName = "ExplorationScene";
    // Le donjon jouable est cherché ici (Assets/Resources/Dungeons/Orphelinat.asset)
    private const string DungeonResourcePath = "Dungeons/Orphelinat";

    private static readonly HashSet<(int room, int spot)> _defeated = new HashSet<(int, int)>();
    private static (int room, int spot) _pending;
    private static bool _doorsJustOpened;

    public static DungeonData Dungeon { get; private set; }
    public static EncounterData CurrentEncounter { get; private set; }
    public static int CurrentRoom { get; private set; }
    public static Vector2Int PartyCell { get; private set; }

    public static bool IsActive => Dungeon != null;

    /// <summary>Bilan de l'expédition en cours (écran de fin de donjon).</summary>
    public static DungeonRunStats Stats { get; private set; } = new DungeonRunStats();

    /// <summary>Nouvelle partie : salle 0, pion sur sa case de départ, aucun monstre vaincu.</summary>
    public static void Begin(DungeonData dungeon)
    {
        Clear();
        Dungeon = dungeon;
        if (dungeon != null && dungeon.rooms.Count > 0) PartyCell = dungeon.rooms[0].start;
    }

    /// <summary>
    /// Scène à charger au lancement de la partie : l'exploration si le donjon existe, sinon le combat
    /// direct (<paramref name="combatScene"/>).
    /// </summary>
    public static string BeginFromResources(string combatScene)
    {
        DungeonData dungeon = Resources.Load<DungeonData>(DungeonResourcePath);
        if (dungeon == null || dungeon.rooms.Count == 0)
        {
            Clear();
            return combatScene;
        }
        Begin(dungeon);
        return ExplorationSceneName;
    }

    public static bool IsDefeated(int room, int spot) => _defeated.Contains((room, spot));

    /// <summary>Tous les groupes de monstres de la salle sont vaincus (ses portes s'ouvrent).</summary>
    public static bool IsRoomCleared(int room)
    {
        for (int s = 0; s < Dungeon.rooms[room].monsters.Count; s++)
            if (!IsDefeated(room, s)) return false;
        return true;
    }

    /// <summary>Le pion change de salle (porte franchie) et arrive sur la case donnée.</summary>
    public static void MoveToRoom(int room, Vector2Int arrivalCell)
    {
        CurrentRoom = room;
        PartyCell = arrivalCell;
    }

    /// <summary>Lance le combat du groupe de monstres <paramref name="spot"/> de la salle courante.</summary>
    public static void StartEncounter(int spot, Vector2Int partyCell)
    {
        CurrentEncounter = Dungeon.rooms[CurrentRoom].monsters[spot].encounter;
        _pending = (CurrentRoom, spot);
        PartyCell = partyCell;
    }

    /// <summary>Victoire : le groupe du combat qui vient de finir disparaît de la salle.</summary>
    public static void CompleteEncounter()
    {
        if (CurrentEncounter != null)
        {
            _defeated.Add(_pending);
            if (IsRoomCleared(_pending.room)) _doorsJustOpened = true;
        }
        CurrentEncounter = null;
    }

    /// <summary>
    /// Vrai une seule fois après le dernier combat d'une salle : l'exploration joue alors l'ouverture des portes
    /// au lieu de les afficher déjà ouvertes.
    /// </summary>
    public static bool ConsumeDoorsJustOpened()
    {
        bool value = _doorsJustOpened;
        _doorsJustOpened = false;
        return value;
    }

    /// <summary>Tous les monstres de toutes les salles sont vaincus.</summary>
    public static bool IsCompleted
    {
        get
        {
            if (Dungeon == null) return false;
            for (int r = 0; r < Dungeon.rooms.Count; r++)
                for (int s = 0; s < Dungeon.rooms[r].monsters.Count; s++)
                    if (!_defeated.Contains((r, s))) return false;
            return true;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Clear()
    {
        Dungeon = null;
        CurrentEncounter = null;
        CurrentRoom = 0;
        PartyCell = Vector2Int.zero;
        _defeated.Clear();
        _doorsJustOpened = false;
        Stats = new DungeonRunStats();
    }
}
