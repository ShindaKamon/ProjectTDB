using UnityEngine;

/// <summary>
/// Coupe les logs Debug.Log / Debug.LogWarning dans les builds release
/// (hors éditeur et hors Development Build). Les erreurs et exceptions
/// restent tracées pour le diagnostic des crashes joueurs.
///
/// NOTE : ceci supprime l'écriture des logs, pas le coût de construction
/// des chaînes interpolées passées à Debug.Log. Un wrapper GameLog avec
/// [Conditional] sur les ~400 sites d'appel reste à faire pour éliminer
/// aussi ces allocations.
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
