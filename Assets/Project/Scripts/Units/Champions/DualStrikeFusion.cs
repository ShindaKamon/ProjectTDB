using System.Collections;
using UnityEngine;

/// <summary>
/// Evan en Colère — « Deux en un » : Evan absorbe Lyse (plus d'écho) ; chaque attaque d'Evan est rejouée
/// une seconde fois, à pleine puissance. À la fin de la fusion, Lyse reparaît près d'Evan.
/// </summary>
[CreateAssetMenu(fileName = "DualStrikeFusion", menuName = "Champion/Fusion/Deux en un")]
public class DualStrikeFusion : FusionData
{
    [Tooltip("Prefab de l'invocation qui reparaît à la fin de la fusion (Lyse)")]
    public GameObject summonPrefab;

    [Tooltip("Délai avant la seconde frappe (secondes), pour la distinguer de la première")]
    public float replayDelay = 0.5f;

    public override void OnActivated(Champion champion)
    {
        (champion as EvanUnit)?.AbsorbSummon();
    }

    public override void OnEnded(Champion champion)
    {
        (champion as EvanUnit)?.ReleaseSummon(summonPrefab);
    }

    public override void OnEnemyAttacked(Champion champion, CardData card, Unit enemy, int attackDamage)
    {
        if (attackDamage <= 0) return;
        champion.StartCoroutine(PendingEffects.Track(Replay(champion, enemy, attackDamage, card.damageType)));
    }

    private IEnumerator Replay(Champion champion, Unit enemy, int attackDamage, DamageType type)
    {
        if (replayDelay > 0f) yield return new WaitForSeconds(replayDelay);
        if (enemy == null || enemy.GetHealth() <= 0) yield break;

        enemy.TakeDamageFrom(enemy.ReduceByDefense(attackDamage, type), champion);
        GameLog.Log($"[{formName}] seconde frappe de {champion.name} sur {enemy.name} : {attackDamage} dégâts avant défense");
    }
}
