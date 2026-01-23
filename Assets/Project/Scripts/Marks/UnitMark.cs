using UnityEngine;

/// <summary>
/// Structure représentant une marque appliquée sur une unité.
/// Les marques peuvent avoir des effets variés selon leur type et peuvent être empilées.
/// </summary>
[System.Serializable]
public struct UnitMark
{
    /// <summary>
    /// Type de la marque (détermine l'effet lors de la consommation)
    /// </summary>
    public MarkType markType;

    /// <summary>
    /// Nombre de stacks de cette marque (certains effets se cumulent)
    /// </summary>
    public int stacks;

    /// <summary>
    /// Nombre de tours restants avant expiration (0 = permanent jusqu'à consommation)
    /// </summary>
    public int remainingTurns;

    /// <summary>
    /// L'unité qui a appliqué cette marque (utile pour les effets de retour)
    /// </summary>
    public Unit appliedBy;

    /// <summary>
    /// Valeur bonus associée à la marque (ex: dégâts bonus, % de réduction, etc.)
    /// </summary>
    public int bonusValue;

    /// <summary>
    /// Constructeur pour créer une nouvelle marque
    /// </summary>
    /// <param name="type">Type de la marque</param>
    /// <param name="source">Unité qui applique la marque</param>
    /// <param name="initialStacks">Nombre de stacks initiaux (défaut: 1)</param>
    /// <param name="duration">Durée en tours (0 = permanent)</param>
    /// <param name="bonus">Valeur bonus de la marque</param>
    public UnitMark(MarkType type, Unit source, int initialStacks = 1, int duration = 0, int bonus = 0)
    {
        markType = type;
        appliedBy = source;
        stacks = Mathf.Max(1, initialStacks);
        remainingTurns = duration;
        bonusValue = bonus;
    }

    /// <summary>
    /// Ajoute des stacks à cette marque
    /// </summary>
    /// <param name="amount">Nombre de stacks à ajouter</param>
    /// <returns>La marque modifiée</returns>
    public UnitMark AddStacks(int amount)
    {
        stacks += amount;
        return this;
    }

    /// <summary>
    /// Retire des stacks de cette marque
    /// </summary>
    /// <param name="amount">Nombre de stacks à retirer</param>
    /// <returns>La marque modifiée</returns>
    public UnitMark RemoveStacks(int amount)
    {
        stacks = Mathf.Max(0, stacks - amount);
        return this;
    }

    /// <summary>
    /// Décrémente la durée de la marque d'un tour
    /// </summary>
    /// <returns>True si la marque a expiré, false sinon</returns>
    public bool DecrementTurn()
    {
        if (remainingTurns > 0)
        {
            remainingTurns--;
            return remainingTurns <= 0;
        }
        return false; // Les marques permanentes (duration 0) n'expirent pas par le temps
    }

    /// <summary>
    /// Vérifie si la marque est expirée (plus de stacks ou durée écoulée)
    /// </summary>
    public bool IsExpired => stacks <= 0 || (remainingTurns < 0);

    /// <summary>
    /// Vérifie si la marque est permanente (ne disparaît que par consommation)
    /// </summary>
    public bool IsPermanent => remainingTurns == 0;

    public override string ToString()
    {
        string durationStr = IsPermanent ? "permanent" : $"{remainingTurns} tour(s)";
        return $"[{markType}] x{stacks} ({durationStr}) - Bonus: {bonusValue}";
    }
}
