using UnityEngine;

/// <summary>
/// AceUnit hérite de Champion et représente le champion Ace.
/// Squelette Phase 2 : PA/PM/stats de base uniquement.
/// TODO Phase 3 : passif "Main gagnante" (détection de motif sur les coûts PA/Émotions des
/// cartes jouées ce tour — Paire/Suite/Bluff, nécessite un tracking des cartes jouées par
/// tour, rien de tel n'existe encore sur Unit/Champion) + carte signature "Il triche"
/// (modification de ±1 PA sur une carte en main, nécessite une couche de surcharge de coût
/// puisque CardData est un ScriptableObject partagé/immuable).
/// </summary>
public class AceUnit : Champion, IActionPointsUser
{
    public new void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        GameLog.Log($"{name} (Ace) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
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
