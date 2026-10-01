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

    [Tooltip("Lits du combat de boss (Monstre sous le lit) : le boss se cache dessous et ses PV sont répartis entre eux ; chaque lit a la tête contre le mur du fond le plus proche. Vide = pas de lits.")]
    public List<Vector2Int> bedCells = new List<Vector2Int>();

    [Tooltip("Prefab d'un lit (porte un BedUnit)")]
    public GameObject bedPrefab;
}
