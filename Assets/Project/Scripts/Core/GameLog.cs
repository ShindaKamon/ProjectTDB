using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Wrapper autour de Debug.Log / Debug.LogWarning qui disparaît complètement
/// (appel ET évaluation des arguments, y compris les chaînes interpolées)
/// des builds release grâce à [Conditional]. Complète GameLogConfig, qui ne
/// coupe que l'écriture du log au runtime mais pas son coût de construction.
///
/// Debug.LogError n'est pas wrappé ici : les erreurs restent actives en
/// release (voir GameLogConfig) pour le diagnostic des crashes joueurs.
/// </summary>
public static class GameLog
{
    private const string EditorSymbol = "UNITY_EDITOR";
    private const string DevBuildSymbol = "DEVELOPMENT_BUILD";

    [Conditional(EditorSymbol), Conditional(DevBuildSymbol)]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional(EditorSymbol), Conditional(DevBuildSymbol)]
    public static void Log(object message, Object context)
    {
        Debug.Log(message, context);
    }

    [Conditional(EditorSymbol), Conditional(DevBuildSymbol)]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [Conditional(EditorSymbol), Conditional(DevBuildSymbol)]
    public static void LogWarning(object message, Object context)
    {
        Debug.LogWarning(message, context);
    }
}
