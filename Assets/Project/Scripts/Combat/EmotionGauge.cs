using System.Collections.Generic;

/// <summary>
/// Jauges d'émotion d'un champion et état de sa fusion (Éveil) — classe pure, sans Unity.
/// Une jauge par émotion (2 points par palier, 3 paliers max). Jauge pleine, le joueur peut fusionner
/// avec cette émotion ; la fusion perd 1 palier au début de chaque tour du champion, se recharge en
/// jouant des cartes de l'émotion et prend fin à 0. Une seule fusion à la fois.
/// Règles : Docs/GDD/SYSTEME_EMOTIONS.md, section « Système d'Éveil ».
/// </summary>
public class EmotionGauge
{
    public const int PointsPerTier = 2;
    public const int MaxTiers = 3;
    public const int MaxPoints = PointsPerTier * MaxTiers;

    private readonly Dictionary<EmotionType, int> _points = new Dictionary<EmotionType, int>();

    /// <summary>Émotion de la fusion en cours (None si le champion n'est pas fusionné).</summary>
    public EmotionType ActiveFusion { get; private set; } = EmotionType.None;

    public bool IsFused => ActiveFusion != EmotionType.None;

    public int GetPoints(EmotionType emotion) =>
        _points.TryGetValue(emotion, out int points) ? points : 0;

    public int GetTiers(EmotionType emotion) => GetPoints(emotion) / PointsPerTier;

    public bool IsFull(EmotionType emotion) => GetPoints(emotion) >= MaxPoints;

    /// <summary>Ajoute des points à la jauge d'une émotion (plafonnée). Retourne true si elle a changé.</summary>
    public bool AddPoints(EmotionType emotion, int amount)
    {
        if (emotion == EmotionType.None || amount <= 0) return false;

        int before = GetPoints(emotion);
        int after = System.Math.Min(MaxPoints, before + amount);
        _points[emotion] = after;
        return after != before;
    }

    /// <summary>
    /// Retire des points à la jauge de la fusion en cours (carte d'une autre émotion jouée pendant la fusion).
    /// À 0 la fusion prend fin : retourne son émotion (None sinon).
    /// </summary>
    public EmotionType DrainActiveFusion(int amount)
    {
        if (!IsFused || amount <= 0) return EmotionType.None;
        int points = System.Math.Max(0, GetPoints(ActiveFusion) - amount);
        _points[ActiveFusion] = points;
        if (points > 0) return EmotionType.None;
        EmotionType ended = ActiveFusion;
        ActiveFusion = EmotionType.None;
        return ended;
    }

    /// <summary>Une fusion est possible : pas déjà fusionné et jauge de cette émotion pleine.</summary>
    public bool CanActivate(EmotionType emotion) =>
        emotion != EmotionType.None && !IsFused && IsFull(emotion);

    public bool TryActivate(EmotionType emotion)
    {
        if (!CanActivate(emotion)) return false;
        ActiveFusion = emotion;
        return true;
    }

    /// <summary>
    /// Début du tour du champion : la fusion en cours perd 1 palier ; à 0, elle prend fin.
    /// Retourne l'émotion de la fusion qui vient de se terminer (None sinon).
    /// </summary>
    public EmotionType OnTurnStart()
    {
        if (!IsFused) return EmotionType.None;

        int points = System.Math.Max(0, GetPoints(ActiveFusion) - PointsPerTier);
        _points[ActiveFusion] = points;
        if (points > 0) return EmotionType.None;

        EmotionType ended = ActiveFusion;
        ActiveFusion = EmotionType.None;
        return ended;
    }

    /// <summary>Texte stable de l'état (empreinte réseau) : émotion fusionnée puis jauges non vides.</summary>
    public string Describe()
    {
        var parts = new List<string>();
        for (EmotionType emotion = EmotionType.Anger; emotion <= EmotionType.Anticipation; emotion++)
        {
            int points = GetPoints(emotion);
            if (points > 0) parts.Add($"{emotion}{points}");
        }
        return $"fusion{ActiveFusion}[{string.Join(",", parts)}]";
    }
}
