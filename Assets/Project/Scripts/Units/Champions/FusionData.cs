using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Forme de fusion (Éveil) d'un champion avec une émotion : un asset par couple champion × émotion.
/// Ne porte aucun état (l'état vit sur le Champion) ; les sous-classes surchargent les hooks dont
/// elles ont besoin. Le code générique appelle ces hooks sans connaître la forme.
/// </summary>
public abstract class FusionData : ScriptableObject
{
    [Header("Identité")]
    [Tooltip("Émotion de la fusion : Colère = Rage, Joie = Extase, Peur = Terreur")]
    public EmotionType emotion = EmotionType.None;
    public string formName = "";
    [TextArea(2, 5)]
    public string description = "";

    /// <summary>Le champion vient de fusionner.</summary>
    public virtual void OnActivated(Champion champion) { }

    /// <summary>La fusion prend fin (jauge à 0).</summary>
    public virtual void OnEnded(Champion champion) { }

    /// <summary>Début du tour du champion fusionné, après la perte d'un palier (la fusion continue).</summary>
    public virtual void OnTurnStart(Champion champion) { }

    /// <summary>
    /// Une carte du champion vient d'infliger des dégâts à ces ennemis.
    /// firstOfCard : premier appel pour cette carte (une carte à cibles multiples appelle une fois par cible).
    /// </summary>
    public virtual void OnEnemiesHit(Champion champion, CardData card, IReadOnlyList<Unit> enemies, bool firstOfCard) { }

    /// <summary>Une carte du champion vient d'infliger une attaque à cet ennemi (dégâts avant défense).</summary>
    public virtual void OnEnemyAttacked(Champion champion, CardData card, Unit enemy, int attackDamage) { }

    /// <summary>Une carte vient de déplacer le champion (charge, bond) de ce nombre de cases, jusqu'à arrival.</summary>
    public virtual void OnDisplacement(Champion champion, CardData card, Vector2Int arrival, int cases) { }

    /// <summary>Le champion vient de former un motif de combo (voir IComboTracker), en dépensant paSpent PA.</summary>
    public virtual void OnComboPattern(Champion champion, ComboPattern pattern, int paSpent) { }

    /// <summary>
    /// L'invocation du champion va renvoyer un écho de cette attaque. Renvoyer true pour le remplacer
    /// (l'écho normal n'a alors pas lieu).
    /// </summary>
    public virtual bool ReplaceSummonEcho(Champion champion, SummonUnit summon, int attackDamage) => false;
}
