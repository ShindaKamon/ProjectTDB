using UnityEngine;

/// <summary>
/// Raze en Colère — « All-in » : chaque carte compte comme une Suite (+PA) en plus de ses vraies combinaisons
/// (Bluff, Paire) qui se cumulent, et les bonus chiffrés sont multipliés ; en contrepartie Raze subit un
/// contrecoup par PA dépensé. La logique est dans RazeUnit (Main gagnante), qui lit ces paramètres.
/// Forme à tester en jeu : l'équilibre dépend du multiplicateur et du contrecoup.
/// </summary>
[CreateAssetMenu(fileName = "AllInFusion", menuName = "Champion/Fusion/All-in")]
public class AllInFusion : FusionData
{
    [Tooltip("Multiplicateur des bonus chiffrés des combinaisons (bouclier du Bluff, PA de la Suite)")]
    public int patternMultiplier = 2;

    [Tooltip("PV perdus par PA dépensé sur une carte (Raze ne meurt jamais de ce contrecoup)")]
    public int recoilPerPA = 1;
}
