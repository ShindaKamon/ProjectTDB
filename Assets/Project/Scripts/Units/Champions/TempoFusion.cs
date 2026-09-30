using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raze en Peur — « Pioche et tempo » : la première fois qu'une carte touche un ennemi chaque tour, Raze
/// pioche une carte et gagne un PA ; une carte qui forme une Suite retire en plus un PA aux ennemis touchés
/// (retrait normal de ResourceDebuffManager, à leur prochain tour).
/// </summary>
[CreateAssetMenu(fileName = "TempoFusion", menuName = "Champion/Fusion/Pioche et tempo")]
public class TempoFusion : FusionData
{
    [Tooltip("Cartes piochées par tour, à la première carte qui touche un ennemi")]
    public int cardsDrawn = 1;

    [Tooltip("PA gagnés par tour, à la première carte qui touche un ennemi")]
    public int actionPointsGained = 1;

    [Tooltip("PA retirés (prochain tour) aux ennemis touchés par une carte qui forme une Suite")]
    public int suiteActionPointLoss = 1;

    public override void OnEnemiesHit(Champion champion, CardData card, IReadOnlyList<Unit> enemies, bool firstOfCard)
    {
        if (champion is IComboTracker combo && combo.CurrentPattern == ComboPattern.Suite)
        {
            foreach (Unit enemy in enemies)
                ResourceDebuffManager.ApplyDebuff(enemy, suiteActionPointLoss, 0, champion);
        }

        if (champion.FusionTurnCounter > 0) return;
        champion.FusionTurnCounter = 1;

        if (cardsDrawn > 0 && champion.TryGetComponentSafe(out DeckManager deck)) deck.DrawCards(cardsDrawn);
        if (actionPointsGained > 0) champion.AddPA(actionPointsGained);
        GameLog.Log($"[{formName}] {champion.name} pioche {cardsDrawn} et gagne {actionPointsGained} PA");
    }
}
