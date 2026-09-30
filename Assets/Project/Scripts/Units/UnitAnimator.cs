using UnityEngine;

/// <summary>
/// Anime le modèle 3D (enfant « Model ») d'une unité : repos, marche, coup reçu, mort. Suit l'unité parente
/// (déplacement, événements de dégâts et de mort) ; sans unité parente (exploration, aperçu de sélection),
/// la marche se pilote à la main avec SetWalking. Les contrôleurs sont générés par Tools > Animations.
/// </summary>
[RequireComponent(typeof(Animator))]
public class UnitAnimator : MonoBehaviour
{
    private static readonly int WalkingParam = Animator.StringToHash("Walking");
    private static readonly int HitParam = Animator.StringToHash("Hit");
    private static readonly int DeathParam = Animator.StringToHash("Death");

    [Tooltip("Durée (s) pendant laquelle le corps reste visible après la mort de l'unité")]
    [SerializeField] private float _deathLingerSeconds = 2f;

    private Animator _animator;
    private Unit _unit;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _unit = GetComponentInParent<Unit>();
    }

    private void OnEnable()
    {
        if (_unit == null) return;
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    private void Update()
    {
        if (_unit != null) SetWalking(_unit.IsMoving());
    }

    public void SetWalking(bool walking) => _animator.SetBool(WalkingParam, walking);

    private void OnUnitDamaged(UnitDamagedEvent e)
    {
        // Un coût payé en PV (source = cible) n'est pas un coup reçu
        if (e.Target != _unit || e.Source == e.Target || e.EffectiveDamage <= 0) return;
        _animator.SetTrigger(HitParam);
    }

    private void OnUnitDied(UnitDiedEvent e)
    {
        if (e.DeadUnit != _unit) return;

        // L'unité est détruite juste après l'événement : le modèle s'en détache pour jouer sa mort
        OnDisable();
        _unit = null;
        transform.SetParent(null, true);
        _animator.ResetTrigger(HitParam);
        _animator.SetBool(WalkingParam, false);
        _animator.SetTrigger(DeathParam);
        Destroy(gameObject, _deathLingerSeconds);
    }
}
