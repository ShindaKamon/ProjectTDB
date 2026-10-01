using UnityEngine;

/// <summary>
/// Lit du combat contre le Monstre sous le lit : une case, de faction ennemie, sans tour ni déplacement.
/// Ses PV sont une part de ceux du boss (BedHiding). Tant que le boss est caché dessous, il résiste à tous
/// les dégâts et son ombre dépasse du lit.
/// </summary>
public class BedUnit : Unit
{
    [Tooltip("Ombre du monstre, affichée quand il est caché sous ce lit")]
    [SerializeField] private GameObject _shadow;
    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0f, 0.5f, 0f);

    private bool _occupied;

    /// <summary>True tant que le boss est caché sous ce lit.</summary>
    public bool Occupied
    {
        get => _occupied;
        set
        {
            _occupied = value;
            if (_shadow != null) _shadow.SetActive(value);
        }
    }

    public override UnitFaction GetFaction() => UnitFaction.Enemy;
    public override bool TakesTurns => false;
    public override string DisplayName => "Lit";
    protected override bool ResistsDamage => _occupied;

    /// <param name="wallDirection">Direction du mur contre lequel est posée la tête du lit.</param>
    public void InitializeBed(Vector2Int cell, Vector2Int wallDirection, int health)
    {
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
}
