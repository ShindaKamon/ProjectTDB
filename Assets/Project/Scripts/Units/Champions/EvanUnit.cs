using UnityEngine;

/// <summary>
/// EvanUnit hérite de Champion et représente le champion Evan.
/// Passif : Miroir fraternel — géré côté carte (voir CardData.TryTriggerSummonEcho), qui
/// consulte l'invocation active d'Evan via ISummonOwner.
/// </summary>
public class EvanUnit : Champion, ISummonOwner
{
    private SummonUnit _activeSummon;
    public SummonUnit ActiveSummon => _activeSummon;

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

    public void RepositionSummon(SummonUnit summon, Vector2Int newPos)
    {
        if (summon == null || summon.Owner != this)
        {
            GameLog.LogWarning($"{name}: aucune de ses invocations à repositionner.");
            return;
        }

        if (Services.Grid.GetUnitAtGridPos(newPos) != null)
        {
            GameLog.LogWarning($"{name}: case {newPos} occupée, repositionnement annulé.");
            return;
        }

        summon.TeleportTo(newPos);
        GameLog.Log($"{name}: {summon.name} repositionnée à {newPos}");
    }

    /// <summary>Retire l'invocation active du combat sans la tuer (Deux en un). False s'il n'y en a pas.</summary>
    public bool AbsorbSummon()
    {
        if (_activeSummon == null) return false;

        GameLog.Log($"{name}: absorbe {_activeSummon.name}");
        _absorbedSummonHealth = _activeSummon.GetHealth();
        _activeSummon.Despawn();
        return true;
    }

    // PV de l'invocation au moment où elle a été absorbée
    private int _absorbedSummonHealth;

    /// <summary>
    /// Fait reparaître l'invocation sur la case libre la plus proche d'Evan, avec la moitié des PV qu'elle avait
    /// à son absorption (au moins 1) ; son maximum reste la moitié des PV d'Evan.
    /// </summary>
    public void ReleaseSummon(GameObject prefab)
    {
        if (prefab == null || _activeSummon != null || !Services.IsGridServiceAvailable()) return;
        if (!TryFindFreeTileNear(GetCurrentGridPos(), out Vector2Int pos)) return;

        SummonUnit summon = Services.Grid.SpawnSummon(prefab, pos, this, GetHealth() / 2);
        if (summon == null) return;

        RegisterSummon(summon);
        if (_absorbedSummonHealth > 0) summon.SetCurrentHealth(_absorbedSummonHealth / 2);
    }

    /// <summary>Téléporte l'invocation active sur la case libre la plus proche d'Evan.</summary>
    public void PlaceSummonNearSelf()
    {
        if (_activeSummon != null && TryFindFreeTileNear(GetCurrentGridPos(), out Vector2Int pos))
            RepositionSummon(_activeSummon, pos);
    }

    // Case libre la plus proche de origin (anneaux successifs, ordre fixe pour rester identique sur tous les PC)
    private static bool TryFindFreeTileNear(Vector2Int origin, out Vector2Int found)
    {
        for (int radius = 1; radius <= 4; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int dy = radius - Mathf.Abs(dx);
                for (int side = 0; side < (dy == 0 ? 1 : 2); side++)
                {
                    Vector2Int pos = origin + new Vector2Int(dx, side == 0 ? dy : -dy);
                    if (Services.Grid.GetTileAtPosition(pos) == null || Services.Grid.GetUnitAtGridPos(pos) != null) continue;
                    found = pos;
                    return true;
                }
            }
        }
        found = origin;
        return false;
    }

    private void HandleSummonDied(Unit diedUnit)
    {
        if ((Unit)_activeSummon == diedUnit)
        {
            _activeSummon = null;
        }
    }

    /// <summary>
    /// Si Evan meurt, son invocation active (Lyse) doit mourir immédiatement avec lui plutôt
    /// que de rester orpheline sur le terrain (décision produit). On capture la référence avant
    /// base.Die() (qui ne touche pas _activeSummon) puis on tue la summon via son propre Die(),
    /// pour que le nettoyage habituel (GridManager.HandleUnitDied, EventBus, UI) s'applique
    /// aussi à elle. Pas de risque de boucle : la mort de la summon ne redéclenche pas celle
    /// d'Evan (HandleSummonDied se contente de nettoyer la référence).
    /// </summary>
    protected override void Die()
    {
        SummonUnit summonToKill = _activeSummon;

        base.Die();

        if (summonToKill != null)
        {
            GameLog.Log($"{name}: mort de l'invocateur -> {summonToKill.name} meurt aussi.");
            summonToKill.Kill();
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
