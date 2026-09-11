using UnityEngine;

/// <summary>
/// SorenUnit hérite de Champion et représente le champion Soren.
/// Squelette Phase 2 : PA/PM/stats de base uniquement.
/// TODO Phase 3 : invocation de Lyse (Vector2Int, PV = moitié des PV actuels de Soren,
/// recalculé en continu) + passif "Miroir fraternel" (écho à 40% de puissance sur une carte
/// offensive si une invocation a une cible valide à sa propre portée).
/// </summary>
public class SorenUnit : Champion, IActionPointsUser
{
    public new void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        GameLog.Log($"{name} (Soren) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
    }

    protected override void Start()
    {
        base.Start();

        if (Services.IsBattleUIServiceAvailable())
        {
            Services.BattleUI.RegisterPlayer(this);
        }
    }
}
