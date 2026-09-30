using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crux en Terreur — « Vol de mouvement » : chaque carte qui touche un ennemi lui retire des PM
/// (retrait normal de ResourceDebuffManager : le plus fort remplace le plus faible) et en donne à
/// Crux ce tour, dans la limite d'un plafond par tour.
/// </summary>
[CreateAssetMenu(fileName = "MovementStealFusion", menuName = "Champion/Fusion/Vol de mouvement")]
public class MovementStealFusion : FusionData
{
    [Tooltip("PM retirés à chaque ennemi touché, et PM gagnés par le champion pour chaque carte")]
    public int movementStolen = 1;

    [Tooltip("Plafond de PM gagnés par le champion au cours d'un tour")]
    public int gainCapPerTurn = 2;

    public override void OnEnemiesHit(Champion champion, CardData card, IReadOnlyList<Unit> enemies, bool firstOfCard)
    {
        foreach (Unit enemy in enemies)
            ResourceDebuffManager.ApplyDebuff(enemy, 0, movementStolen, champion);

        if (!firstOfCard) return;

        int gain = Mathf.Min(movementStolen, gainCapPerTurn - champion.FusionTurnCounter);
        if (gain <= 0) return;

        champion.FusionTurnCounter += gain;
        champion.GainMovement(gain);
        GameLog.Log($"[{formName}] {champion.name} vole {gain} PM ({champion.FusionTurnCounter}/{gainCapPerTurn} ce tour)");
    }
}
