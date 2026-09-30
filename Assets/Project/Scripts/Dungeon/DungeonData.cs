using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un donjon : des salles à explorer, chacune avec des groupes de monstres (un clic lance leur
/// combat) et des portes vers les autres salles. Les cases sont celles de la grille d'exploration.
/// </summary>
[CreateAssetMenu(fileName = "NewDungeon", menuName = "Dungeon/Dungeon")]
public class DungeonData : ScriptableObject
{
    [Serializable]
    public class MonsterSpot
    {
        public Vector2Int cell;
        public EncounterData encounter;
    }

    [Serializable]
    public class Door
    {
        public Vector2Int cell;
        public int targetRoom;
        [Tooltip("Case où le pion arrive dans la salle de destination")]
        public Vector2Int arrivalCell;
    }

    [Serializable]
    public class Room
    {
        public string roomName = "Salle";
        public Vector2Int size = new Vector2Int(9, 7);
        [Tooltip("Case de départ du pion au début du donjon (salle 0 seulement)")]
        public Vector2Int start;
        public List<MonsterSpot> monsters = new List<MonsterSpot>();
        public List<Door> doors = new List<Door>();
    }

    public string dungeonName = "Nouveau donjon";
    public List<Room> rooms = new List<Room>();
}
