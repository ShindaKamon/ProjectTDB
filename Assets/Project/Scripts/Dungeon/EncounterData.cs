using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un combat du donjon : quels monstres, sur quelles cases de la grille de combat. Lu par
/// GridManager à la place des monstres posés dans CombatScene.
/// </summary>
[CreateAssetMenu(fileName = "NewEncounter", menuName = "Dungeon/Encounter")]
public class EncounterData : ScriptableObject
{
    [Serializable]
    public struct Spawn
    {
        public EnemyData enemy;
        [Tooltip("Case de départ sur la grille de combat")]
        public Vector2Int cell;
    }

    public string encounterName = "Nouvelle rencontre";
    public List<Spawn> enemies = new List<Spawn>();
}
