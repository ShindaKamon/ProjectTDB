using System.Collections.Generic;

public enum BattleResult
{
    Ongoing,
    Victory,
    Defeat
}

/// <summary>
/// Issue d'un combat (Combat_System.md, « Fin du combat ») : victoire quand tous les ennemis sont
/// vaincus, défaite quand tous les champions le sont. Les invocations (Lyse) ne comptent pas.
/// Si les deux arrivent en même temps (ex. contrecoup mortel sur le dernier coup), c'est une
/// victoire (décision du 28/09/2026). C# pur : testable en EditMode.
/// </summary>
public static class BattleOutcome
{
    public static BattleResult Evaluate(IEnumerable<Unit> units)
    {
        bool championAlive = false, enemyAlive = false;

        if (units != null)
        {
            foreach (Unit unit in units)
            {
                if (unit == null || IsDead(unit)) continue;
                if (unit is Champion) championAlive = true;
                else if (unit is Enemy) enemyAlive = true;
            }
        }

        if (!enemyAlive) return BattleResult.Victory;
        if (!championAlive) return BattleResult.Defeat;
        return BattleResult.Ongoing;
    }

    private static bool IsDead(Unit unit)
    {
        UnitState state = unit.GetUnitState();
        return state != null ? state.IsDead() : unit.GetHealth() <= 0;
    }
}
