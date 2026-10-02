using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lit du combat contre le Monstre sous le lit : 2 cases (tête contre le mur, pied vers la salle), de faction ennemie, sans tour ni déplacement.
/// Cachette du boss (BedHiding) : seul le lit sous lequel il se cache subit des dégâts (un lit vide ne craint
/// rien), et ces dégâts passent au boss. L'ombre du monstre ne dépasse que quand il a été révélé (touché).
/// </summary>
public class BedUnit : Unit
{
    [Tooltip("Ombre du monstre, affichée quand il est révélé sous ce lit")]
    [SerializeField] private GameObject _shadow;
    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0f, 0.5f, 0f);

    private bool _occupied;

    /// <summary>True tant que le boss est caché sous ce lit (il redevient caché : ombre masquée).</summary>
    public bool Occupied
    {
        get => _occupied;
        set
        {
            _occupied = value;
            Revealed = false;
        }
    }

    /// <summary>Ombre du monstre visible sous ce lit (il a été touché, ou il a fusionné avec le lit).</summary>
    public bool Revealed
    {
        get => _shadow != null && _shadow.activeSelf;
        set { if (_shadow != null) _shadow.SetActive(value); }
    }

    public override UnitFaction GetFaction() => UnitFaction.Enemy;
    public override bool TakesTurns => false;
    public override bool ShownInCombatSummary => false; // c'est UnderBed qu'on combat, pas ses lits
    public override string DisplayName => "Lit";
    protected override bool ResistsDamage => !_occupied;

    private Vector2Int _footCell; // le lit prend 2 cases : la tête contre le mur, le pied vers la salle

    public override IEnumerable<Vector2Int> OccupiedCells
    {
        get
        {
            yield return GetCurrentGridPos();
            yield return _footCell;
        }
    }

    /// <param name="cell">Case de la tête du lit ; le pied occupe la case voisine, côté salle.</param>
    /// <param name="wallDirection">Direction du mur contre lequel est posée la tête du lit.</param>
    public void InitializeBed(Vector2Int cell, Vector2Int wallDirection, int health)
    {
        _footCell = cell - wallDirection;
        InitUnitStats(health, 0);
        Initialize(cell);
        transform.rotation = Quaternion.LookRotation(new Vector3(wallDirection.x, 0f, wallDirection.y));
        Occupied = false;
        CreateHealthBar(_healthBarOffset, Color.red);
    }

    // Initialisé par InitializeBed (GridManager), pas par Unit.Start
    protected override void Start() { }

    /// <summary>Le lit cède (le boss en sort) : il disparaît comme une unité vaincue.</summary>
    public void Collapse()
    {
        Occupied = false;
        Die();
    }

    /// <summary>Fixe les PV du lit, barre pleine (ex. le Lit de la phase 2 reçoit les PV de la phase).</summary>
    public void SetFullHealth(int health)
    {
        SetMaxHealth(health);
        SetCurrentHealth(health);
    }
}
