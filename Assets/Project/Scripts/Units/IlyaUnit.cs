using UnityEngine;

/// <summary>
/// IlyaUnit hérite de Champion et représente le personnage jouable principal.
/// Spécificités :
/// - PA (Points d'Action) pour jouer des cartes (hérité de Champion)
/// - PM (Points de Mouvement) pour se déplacer (hérité de Unit)
/// - DEFP (Défense physique) - réduit les dégâts physiques
/// - DEFM (Défense magique) - réduit les dégâts magiques
/// - Système d'émotions géré par EmotionSystem (Contrariété ↔ Colère ↔ Rage)
/// </summary>
public class IlyaUnit : Champion
{
    // ========== STATS SPÉCIFIQUES ILYA ==========

    [Header("=== Ilya Defense Stats ===")]
    [SerializeField] private int _physicalDefense = 10;        // Défense physique (réduit dégâts physiques)
    [SerializeField] private int _magicalDefense = 10;        // Défense magique (réduit dégâts magiques)

    // NOTE: PA system (GetCurrentPA, GetMaxPA, SpendPA, RefreshPA, OnActionPointsChanged)
    // est maintenant hérité de Champion

    // ========== GETTERS PUBLICS ==========

    public int GetPhysicalDefense() => _physicalDefense;
    public int GetMagicalDefense() => _magicalDefense;
    
    // ========== INITIALISATION ==========

    protected override void Start()
    {
        base.Start(); // Appelle le Start de Unit (gère l'initialisation automatique)

        // Désactivé : On utilise l'Orbe de vie (UI) au lieu de la barre flottante pour le joueur
        // CreateHealthBar();

        // Connecte l'unité à l'Orbe de vie via le BattleUIManager
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.RegisterPlayer(this);
        }

        Debug.Log($"{name} (Ilya) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, DEFP: {_physicalDefense}, DEFM: {_magicalDefense}");
    }

    // NOTE: SpendPA() et RefreshPA() sont maintenant hérités de Champion

    // ========== SYSTÈME DE DÉFENSE ==========

    /// <summary>
    /// Surcharge TakeDamage pour appliquer la défense
    /// </summary>
    public override void TakeDamage(int rawDamage)
    {
        // Applique la défense (minimum 1 dégât)
        int actualDamage = Mathf.Max(1, rawDamage - _physicalDefense);

        // Applique les dégâts (appelle la méthode de base Unit)
        base.TakeDamage(actualDamage);

        Debug.Log($"{name} prend {actualDamage} dégâts (brut: {rawDamage}, DEF: {_physicalDefense})");
    }

    /// <summary>
    /// Modifie la défense physique (utilisé par EmotionSystem pour les transformations)
    /// </summary>
    public void SetPhysicalDefense(int value)
    {
        _physicalDefense = value;
        Debug.Log($"{name}: Défense physique changée à {_physicalDefense}");
    }

    /// <summary>
    /// Modifie la défense magique (utilisé par EmotionSystem pour les transformations)
    /// </summary>
    public void SetMagicalDefense(int value)
    {
        _magicalDefense = value;
        Debug.Log($"{name}: Défense magique changée à {_magicalDefense}");
    }
}
