using UnityEngine;

/// <summary>
/// Lyse, l'invocation signature de Soren. Ses PV ne sont jamais figés à l'invocation : ils
/// valent en permanence la moitié des PV ACTUELS de Soren, recalculés à chaque changement de
/// PV de Soren. Elle peut donc mourir sans être ciblée directement, si Soren perd trop de PV.
/// </summary>
public class LyseUnit : SummonUnit
{
    public override void InitializeSummon(Unit owner, Vector2Int gridPos, int maxHealth)
    {
        // Le paramètre maxHealth est ignoré : Lyse calcule toujours sa propre valeur.
        int hp = owner != null ? Mathf.Max(1, owner.GetHealth() / 2) : Mathf.Max(1, maxHealth);
        base.InitializeSummon(owner, gridPos, hp);

        if (owner != null)
        {
            owner.OnHealthChanged += HandleOwnerHealthChanged;
        }
    }

    private void HandleOwnerHealthChanged(int currentHealth, int maxHealth)
    {
        // Recalcule en continu : la moitié des PV ACTUELS de Soren (SetMaxHealth ajuste
        // aussi les PV courants de Lyse proportionnellement, cf. Unit.SetMaxHealth).
        SetMaxHealth(Mathf.Max(1, currentHealth / 2));
    }

    void OnDestroy()
    {
        if (_owner != null)
        {
            _owner.OnHealthChanged -= HandleOwnerHealthChanged;
        }
    }
}
