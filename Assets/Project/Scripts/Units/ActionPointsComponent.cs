using UnityEngine;

/// <summary>
/// Component réutilisable qui gère le système de Points d'Action (PA).
/// Utilisé par Champion et Enemy pour éviter la duplication de code.
/// Pattern: Composition over Inheritance
/// </summary>
[System.Serializable]
public class ActionPointsComponent : IActionPointsUser
{
    // ========== VARIABLES PA ==========
    [SerializeField] private int _maxActionPoints;         // PA (Points d'Action) maximum par tour
    [SerializeField] private int _currentActionPoints;     // PA restants ce tour

    // Référence au nom de l'unité (pour les logs)
    private string _unitName;

    // ========== ÉVÉNEMENTS ==========
    public event System.Action<int, int> OnActionPointsChanged; // (current, max)

    // ========== CONSTRUCTEUR ==========

    /// <summary>
    /// Constructeur pour initialiser le système PA
    /// </summary>
    /// <param name="maxPA">PA maximum</param>
    /// <param name="unitName">Nom de l'unité (pour les logs)</param>
    public ActionPointsComponent(int maxPA, string unitName)
    {
        _maxActionPoints = maxPA;
        _currentActionPoints = maxPA;
        _unitName = unitName;
    }

    // ========== IMPLÉMENTATION INTERFACE ==========

    public int GetCurrentPA() => _currentActionPoints;

    public int GetMaxPA() => _maxActionPoints;

    public bool SpendPA(int amount)
    {
        if (_currentActionPoints >= amount)
        {
            _currentActionPoints -= amount;
            OnActionPointsChanged?.Invoke(_currentActionPoints, _maxActionPoints);
            Debug.Log($"{_unitName}: PA dépensés ({amount}). Restant: {_currentActionPoints}/{_maxActionPoints}");
            return true;
        }
        else
        {
            Debug.LogWarning($"{_unitName}: PA insuffisants ! Requis: {amount}, Disponible: {_currentActionPoints}");
            return false;
        }
    }

    public void RefreshPA()
    {
        _currentActionPoints = _maxActionPoints;
        OnActionPointsChanged?.Invoke(_currentActionPoints, _maxActionPoints);
        Debug.Log($"{_unitName}: PA rafraîchis à {_currentActionPoints}/{_maxActionPoints}");
    }

    public void SetMaxPA(int value)
    {
        _maxActionPoints = value;
        // S'assurer que les PA courants ne dépassent pas le nouveau max
        _currentActionPoints = Mathf.Clamp(_currentActionPoints, 0, _maxActionPoints);
        OnActionPointsChanged?.Invoke(_currentActionPoints, _maxActionPoints);
        Debug.Log($"{_unitName}: PA maximum changés à {_maxActionPoints}");
    }

    // ========== MÉTHODES UTILITAIRES ==========

    /// <summary>
    /// Met à jour le nom de l'unité (utile si le nom change après construction)
    /// </summary>
    public void SetUnitName(string unitName)
    {
        _unitName = unitName;
    }

    /// <summary>
    /// Initialise les PA avec de nouvelles valeurs
    /// </summary>
    public void Initialize(int maxPA, string unitName)
    {
        _maxActionPoints = maxPA;
        _currentActionPoints = maxPA;
        _unitName = unitName;
        OnActionPointsChanged?.Invoke(_currentActionPoints, _maxActionPoints);
    }
}
