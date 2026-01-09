using UnityEngine;

/// <summary>
/// Service standardisé pour l'accès sécurisé aux composants Unity.
/// Pattern: Utility Class + Safe Component Access
/// Responsabilités:
/// - Fournir des méthodes d'accès sécurisées aux composants (avec null check)
/// - Standardiser GetComponent, TryGetComponent, FindObjectOfType patterns
/// - Logger les erreurs de composants manquants de manière cohérente
/// - Éviter les références nulles et les erreurs runtime
/// </summary>
public static class ComponentLocator
{
    // ========== GET COMPONENT (avec log d'erreur si null) ==========

    /// <summary>
    /// Récupère un composant de manière sécurisée avec log d'erreur si absent.
    /// Utiliser quand le composant est OBLIGATOIRE pour le fonctionnement.
    /// </summary>
    public static T GetRequiredComponent<T>(this GameObject gameObject, string contextMessage = null) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            string errorMessage = $"ComponentLocator: Composant requis {typeof(T).Name} introuvable sur {gameObject.name}";
            if (!string.IsNullOrEmpty(contextMessage))
            {
                errorMessage += $" - {contextMessage}";
            }
            Debug.LogError(errorMessage, gameObject);
        }
        return component;
    }

    /// <summary>
    /// Récupère un composant de manière sécurisée avec log d'erreur si absent.
    /// Utiliser quand le composant est OBLIGATOIRE pour le fonctionnement.
    /// </summary>
    public static T GetRequiredComponent<T>(this Component component, string contextMessage = null) where T : Component
    {
        return component.gameObject.GetRequiredComponent<T>(contextMessage);
    }

    // ========== TRY GET COMPONENT (sans log d'erreur) ==========

    /// <summary>
    /// Tente de récupérer un composant sans logger d'erreur si absent.
    /// Utiliser quand le composant est OPTIONNEL.
    /// Retourne true si le composant existe, false sinon.
    /// </summary>
    public static bool TryGetComponentSafe<T>(this GameObject gameObject, out T component) where T : Component
    {
        component = gameObject.GetComponent<T>();
        return component != null;
    }

    /// <summary>
    /// Tente de récupérer un composant sans logger d'erreur si absent.
    /// Utiliser quand le composant est OPTIONNEL.
    /// Retourne true si le composant existe, false sinon.
    /// </summary>
    public static bool TryGetComponentSafe<T>(this Component sourceComponent, out T component) where T : Component
    {
        return sourceComponent.gameObject.TryGetComponentSafe(out component);
    }

    // ========== GET COMPONENT IN PARENT (avec log d'erreur si null) ==========

    /// <summary>
    /// Récupère un composant dans les parents de manière sécurisée.
    /// Utiliser quand le composant parent est OBLIGATOIRE.
    /// </summary>
    public static T GetRequiredComponentInParent<T>(this GameObject gameObject, string contextMessage = null) where T : Component
    {
        T component = gameObject.GetComponentInParent<T>();
        if (component == null)
        {
            string errorMessage = $"ComponentLocator: Composant parent requis {typeof(T).Name} introuvable sur {gameObject.name}";
            if (!string.IsNullOrEmpty(contextMessage))
            {
                errorMessage += $" - {contextMessage}";
            }
            Debug.LogError(errorMessage, gameObject);
        }
        return component;
    }

    /// <summary>
    /// Récupère un composant dans les parents de manière sécurisée.
    /// Utiliser quand le composant parent est OBLIGATOIRE.
    /// </summary>
    public static T GetRequiredComponentInParent<T>(this Component component, string contextMessage = null) where T : Component
    {
        return component.gameObject.GetRequiredComponentInParent<T>(contextMessage);
    }

    /// <summary>
    /// Tente de récupérer un composant dans les parents sans logger d'erreur.
    /// Utiliser quand le composant parent est OPTIONNEL.
    /// </summary>
    public static bool TryGetComponentInParentSafe<T>(this GameObject gameObject, out T component) where T : Component
    {
        component = gameObject.GetComponentInParent<T>();
        return component != null;
    }

    /// <summary>
    /// Tente de récupérer un composant dans les parents sans logger d'erreur.
    /// Utiliser quand le composant parent est OPTIONNEL.
    /// </summary>
    public static bool TryGetComponentInParentSafe<T>(this Component sourceComponent, out T component) where T : Component
    {
        return sourceComponent.gameObject.TryGetComponentInParentSafe(out component);
    }

    // ========== GET COMPONENT IN CHILDREN (avec log d'erreur si null) ==========

    /// <summary>
    /// Récupère un composant dans les enfants de manière sécurisée.
    /// Utiliser quand le composant enfant est OBLIGATOIRE.
    /// </summary>
    public static T GetRequiredComponentInChildren<T>(this GameObject gameObject, string contextMessage = null, bool includeInactive = false) where T : Component
    {
        T component = gameObject.GetComponentInChildren<T>(includeInactive);
        if (component == null)
        {
            string errorMessage = $"ComponentLocator: Composant enfant requis {typeof(T).Name} introuvable sur {gameObject.name}";
            if (!string.IsNullOrEmpty(contextMessage))
            {
                errorMessage += $" - {contextMessage}";
            }
            Debug.LogError(errorMessage, gameObject);
        }
        return component;
    }

    /// <summary>
    /// Récupère un composant dans les enfants de manière sécurisée.
    /// Utiliser quand le composant enfant est OBLIGATOIRE.
    /// </summary>
    public static T GetRequiredComponentInChildren<T>(this Component component, string contextMessage = null, bool includeInactive = false) where T : Component
    {
        return component.gameObject.GetRequiredComponentInChildren<T>(contextMessage, includeInactive);
    }

    /// <summary>
    /// Tente de récupérer un composant dans les enfants sans logger d'erreur.
    /// Utiliser quand le composant enfant est OPTIONNEL.
    /// </summary>
    public static bool TryGetComponentInChildrenSafe<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
    {
        component = gameObject.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }

    /// <summary>
    /// Tente de récupérer un composant dans les enfants sans logger d'erreur.
    /// Utiliser quand le composant enfant est OPTIONNEL.
    /// </summary>
    public static bool TryGetComponentInChildrenSafe<T>(this Component sourceComponent, out T component, bool includeInactive = false) where T : Component
    {
        return sourceComponent.gameObject.TryGetComponentInChildrenSafe(out component, includeInactive);
    }

    // ========== FIND OBJECT OF TYPE (découragé - utiliser avec prudence) ==========

    /// <summary>
    /// ⚠️ DÉPRÉCIÉ: FindObjectOfType est lent et devrait être évité.
    /// Utiliser uniquement pour setup initial ou debug.
    /// Préférer l'injection de dépendances ou les singletons.
    /// </summary>
    public static T FindSingleObjectOfType<T>(string contextMessage = null) where T : Object
    {
        T obj = Object.FindAnyObjectByType<T>();
        if (obj == null && !string.IsNullOrEmpty(contextMessage))
        {
            Debug.LogWarning($"ComponentLocator: {typeof(T).Name} introuvable dans la scène - {contextMessage}");
        }
        return obj;
    }

    // ========== VALIDATION ==========

    /// <summary>
    /// Vérifie qu'un composant n'est pas null et log une erreur si c'est le cas.
    /// Retourne true si le composant est valide.
    /// </summary>
    public static bool ValidateComponent<T>(T component, GameObject context, string componentName = null) where T : Component
    {
        if (component == null)
        {
            string name = componentName ?? typeof(T).Name;
            Debug.LogError($"ComponentLocator: Composant {name} est null sur {context.name}", context);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Vérifie qu'un GameObject n'est pas null et log une erreur si c'est le cas.
    /// Retourne true si le GameObject est valide.
    /// </summary>
    public static bool ValidateGameObject(GameObject obj, string objectName, Object context = null)
    {
        if (obj == null)
        {
            if (context != null)
            {
                Debug.LogError($"ComponentLocator: GameObject {objectName} est null", context);
            }
            else
            {
                Debug.LogError($"ComponentLocator: GameObject {objectName} est null");
            }
            return false;
        }
        return true;
    }
}
