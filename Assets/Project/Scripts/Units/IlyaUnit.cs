using UnityEngine;

/// <summary>
/// IlyaUnit hérite de Unit et ajoute les mécaniques spécifiques à Émotions Tactics :
/// - PA (Points d'Action) pour jouer des cartes
/// - PM (Points de Mouvement) pour se déplacer (comme Unit de base)
/// - DEFP (Défense physique)
/// - DEFM (Défense magique)
/// - Système d'émotions géré par EmotionSystem (Contrariété ↔ Colère ↔ Rage)
/// </summary>
public class IlyaUnit : Unit
{
    // ========== STATS ÉMOTIONS TACTICS ==========

    [Header("=== Émotions Tactics Stats ===")]

    [SerializeField] private int _maxPA = 3;            // PA max par tour
    [SerializeField] private int _currentPA = 3;        // PA restants ce tour
    [SerializeField] private int _defenseP = 10;        // Défense (réduit dégâts)
    [SerializeField] private int _defenseM = 10;        // Défense magique (réduit dégats)

    // ========== EVENTS ==========
    public event System.Action<int, int> OnPAChanged;        // (current, max)

    // ========== GETTERS PUBLICS ==========

    public int GetCurrentPA() => _currentPA;
    public int GetMaxPA() => _maxPA;
    public int GetDefenseP() => _defenseP;
    public int GetDefenseM() => _defenseM;

    // ========== SETTERS PUBLICS ==========

    public void SetMaxPA(int value)
    {
        _maxPA = value;
        // S'assurer que les PA courants ne dépassent pas le nouveau max
        _currentPA = Mathf.Clamp(_currentPA, 0, _maxPA);
        OnPAChanged?.Invoke(_currentPA, _maxPA);
    }
    
    // ========== INITIALISATION ==========

    protected override void Start()
    {
        base.Start(); // Appelle le Start de Unit (gère l'initialisation automatique)

        // Recrée la barre de vie avec les bonnes stats (au cas où elle aurait été créée avant l'initialisation)
        CreateHealthBar();

        Debug.Log($"{name} (Ilya) initialisé - PA: {_currentPA}/{_maxPA}, DEFP: {_defenseP}, DEFM: {_defenseM}");
    }
    
    // ========== SYSTÈME PA ==========
    
    /// <summary>
    /// Dépense des PA pour une action
    /// </summary>
    public bool SpendPA(int amount)
    {
        if (_currentPA >= amount)
        {
            _currentPA -= amount;
            OnPAChanged?.Invoke(_currentPA, _maxPA);
            Debug.Log($"{name}: PA dépensés ({amount}). Restant: {_currentPA}/{_maxPA}");
            return true;
        }
        else
        {
            Debug.LogWarning($"{name}: PA insuffisants ! Requis: {amount}, Disponible: {_currentPA}");
            return false;
        }
    }
    
    /// <summary>
    /// Rafraîchit les PA en début de tour
    /// </summary>
    public void RefreshPA()
    {
        _currentPA = _maxPA;
        OnPAChanged?.Invoke(_currentPA, _maxPA);
        Debug.Log($"{name}: PA rafraîchis à {_currentPA}/{_maxPA}");
    }

    // ========== SYSTÈME DE DÉFENSE ==========

    /// <summary>
    /// Surcharge TakeDamage pour appliquer la défense
    /// </summary>
    public override void TakeDamage(int rawDamage)
    {
        // Applique la défense (minimum 1 dégât)
        int actualDamage = Mathf.Max(1, rawDamage - _defenseP);

        // Applique les dégâts (appelle la méthode de base Unit)
        base.TakeDamage(actualDamage);

        Debug.Log($"{name} prend {actualDamage} dégâts (brut: {rawDamage}, DEF: {_defenseP})");
    }

    /// <summary>
    /// Modifie la défense physique (utilisé par EmotionSystem pour les transformations)
    /// </summary>
    public void SetDefenseP(int value)
    {
        _defenseP = value;
        Debug.Log($"{name}: Défense physique changée à {_defenseP}");
    }

    /// <summary>
    /// Modifie la défense magique (utilisé par EmotionSystem pour les transformations)
    /// </summary>
    public void SetDefenseM(int value)
    {
        _defenseM = value;
        Debug.Log($"{name}: Défense magique changée à {_defenseM}");
    }
}
