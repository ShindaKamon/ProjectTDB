using UnityEngine;

/// <summary>
/// AlpinisteUnit hérite de Champion et représente le champion L'Alpiniste.
/// Squelette Phase 2 : PA/PM/stats de base uniquement.
/// TODO Phase 3 : passif "Réflexe du grimpeur" — après un déplacement rapide vers une unité,
/// bouclier (-15% dégâts subis jusqu'au prochain tour) si atterrissage adjacent à un allié,
/// ou +15% dégâts sur la prochaine carte si atterrissage adjacent à un ennemi. À accrocher
/// sur la fin de CardData.ExecuteChargeEffectCoroutine (déclenché par Piolet d'ascension).
/// </summary>
public class AlpinisteUnit : Champion, IActionPointsUser
{
    public new void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        GameLog.Log($"{name} (L'Alpiniste) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
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
