using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Evan en Peur — « Appât » : à l'activation, Lyse attire tous les ennemis contre elle (les plus proches
/// d'abord) et laisse sur sa case une zone qui retire des PM aux ennemis qui y commencent leur tour, tant
/// que dure la fusion ; Lyse rejoint ensuite Evan. Sans Lyse, la zone se pose sur Evan.
/// </summary>
[CreateAssetMenu(fileName = "LureFusion", menuName = "Champion/Fusion/Appât")]
public class LureFusion : FusionData
{
    [Tooltip("Rayon de la zone (en cases autour de la case de Lyse)")]
    public int zoneRadius = 1;

    [Tooltip("PM retirés au début du tour des ennemis qui sont dans la zone")]
    public int movementLoss = 1;

    public override void OnActivated(Champion champion)
    {
        if (champion is EvanUnit evan && Services.IsGridServiceAvailable())
            champion.StartCoroutine(PendingEffects.Track(Lure(evan)));
    }

    public override void OnEnded(Champion champion) => FusionZones.Clear(champion);

    private IEnumerator Lure(EvanUnit evan)
    {
        SummonUnit lyse = evan.ActiveSummon;
        Unit center = lyse != null ? (Unit)lyse : evan;
        Vector2Int target = center.GetCurrentGridPos();

        var enemies = new List<Unit>(Services.Grid.GetAllEnemyUnits());
        enemies.Sort((a, b) =>
        {
            Vector2Int pa = a.GetCurrentGridPos(), pb = b.GetCurrentGridPos();
            int byDistance = GridGeometry.Distance(target, pa).CompareTo(GridGeometry.Distance(target, pb));
            if (byDistance != 0) return byDistance;
            return pa.x != pb.x ? pa.x.CompareTo(pb.x) : pa.y.CompareTo(pb.y);
        });

        foreach (Unit enemy in enemies)
        {
            // Deux tirs au plus : l'axe dominant d'abord, puis l'autre ; le tirage s'arrête au contact
            for (int step = 0; step < 2 && enemy != null && enemy.GetHealth() > 0; step++)
            {
                Vector2Int delta = target - enemy.GetCurrentGridPos();
                if (GridGeometry.Distance(target, enemy.GetCurrentGridPos()) <= 1) break;

                enemy.ApplyKnockback(delta, Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)));
                while (enemy != null && enemy.IsMoving()) yield return null;
            }
        }

        FusionZones.Add(target, zoneRadius, movementLoss, evan);
        GameLog.Log($"[{formName}] zone posée en {target} (rayon {zoneRadius}, -{movementLoss} PM)");

        if (lyse != null) evan.PlaceSummonNearSelf();
    }
}
