using UnityEngine;

/// <summary>
/// Coupe les logs Debug.Log / Debug.LogWarning dans les builds release
/// (hors éditeur et hors Development Build). Les erreurs et exceptions
/// restent tracées pour le diagnostic des crashes joueurs.
///
/// Ce filtre runtime reste utile en garde-fou (Development Build, appels
/// Debug.Log restants), mais la plupart des sites d'appel passent par
/// GameLog (voir GameLog.cs), qui élimine aussi le coût de construction
/// des chaînes interpolées grâce à [Conditional].
/// </summary>
public static class GameLogConfig
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
        {
            Debug.unityLogger.filterLogType = LogType.Error;
        }
    }
}
