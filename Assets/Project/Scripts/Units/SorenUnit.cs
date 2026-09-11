using UnityEngine;

/// <summary>
/// SorenUnit hérite de Champion et représente le champion Soren.
/// Passif : Miroir fraternel — géré côté carte (voir CardData.TryTriggerSummonEcho), qui
/// consulte l'invocation active de Soren via ISummonOwner.
/// </summary>
public class SorenUnit : Champion, IActionPointsUser, ISummonOwner
{
    private SummonUnit _activeSummon;
    public SummonUnit ActiveSummon => _activeSummon;

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

    // ========== ISummonOwner ==========

    public void RegisterSummon(SummonUnit summon)
    {
        if (_activeSummon != null)
        {
            _activeSummon.OnUnitDied -= HandleSummonDied;
        }

        _activeSummon = summon;
        summon.OnUnitDied += HandleSummonDied;
        GameLog.Log($"{name}: invocation active enregistrée -> {summon.name}");
    }

    public void RepositionSummon(Vector2Int newPos)
    {
        if (_activeSummon == null)
        {
            GameLog.LogWarning($"{name}: aucune invocation active à repositionner.");
            return;
        }

        if (Services.Grid.GetUnitAtGridPos(newPos) != null)
        {
            GameLog.LogWarning($"{name}: case {newPos} occupée, repositionnement annulé.");
            return;
        }

        _activeSummon.TeleportTo(newPos);
        GameLog.Log($"{name}: {_activeSummon.name} repositionnée à {newPos}");
    }

    private void HandleSummonDied(Unit diedUnit)
    {
        if ((Unit)_activeSummon == diedUnit)
        {
            _activeSummon = null;
        }
    }

    void OnDestroy()
    {
        if (_activeSummon != null)
        {
            _activeSummon.OnUnitDied -= HandleSummonDied;
        }
    }
}
