using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effets de combat différés en cours (écho de Lyse, charge, bond) : ils modifient encore l'état
/// après la fin de l'action qui les a lancés. Les commandes suivantes et l'empreinte réseau de
/// fin de tour attendent qu'il n'y en ait plus, pour que tous les PC voient le même état.
/// </summary>
public static class PendingEffects
{
    // Un effet interrompu sans passer par sa fin (unité détruite) ne bloque pas plus que ce délai
    private const float MaxDurationSeconds = 10f;

    private static readonly List<float> _startTimes = new List<float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => _startTimes.Clear();

    /// <summary>Au moins un effet différé est encore en cours.</summary>
    public static bool Any
    {
        get
        {
            _startTimes.RemoveAll(t => Time.time - t > MaxDurationSeconds);
            return _startTimes.Count > 0;
        }
    }

    /// <summary>À passer à StartCoroutine : compte l'effet tant qu'il n'est pas terminé.</summary>
    public static IEnumerator Track(IEnumerator effect)
    {
        float start = Time.time;
        _startTimes.Add(start);
        try
        {
            while (effect.MoveNext()) yield return effect.Current;
        }
        finally
        {
            _startTimes.Remove(start);
        }
    }
}
