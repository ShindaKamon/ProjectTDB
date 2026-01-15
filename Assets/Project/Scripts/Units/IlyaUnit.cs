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
public class IlyaUnit : Champion, IActionPointsUser, IRageUser
{
    // ========== STATS SPÉCIFIQUES ILYA ==========

    [Header("=== Ilya Defense Stats ===")]
    [SerializeField] private int _physicalDefense = 10;        // Défense physique (réduit dégâts physiques)
    [SerializeField] private int _magicalDefense = 10;        // Défense magique (réduit dégâts magiques)

    [Header("=== Rage Mechanic ===")]
    [Tooltip("Carte Rage ajoutée à la main quand Ilya subit des dégâts")]
    [SerializeField] private CardData _rageCard;
    [Tooltip("Dégâts nécessaires pour générer une carte Rage")]
    [SerializeField] private int _damageThresholdForRage = 10;

    [Header("=== Rage Stock ===")]
    [SerializeField] private int _rageStock = 0;
    [SerializeField] private int _maxRageStock = 5;
    public event System.Action<int> OnRageStockChanged;

    private int _accumulatedDamage = 0;

    // NOTE: PA system (GetCurrentPA, GetMaxPA, SpendPA, RefreshPA, OnActionPointsChanged)
    // est maintenant hérité de Champion

    // ========== GETTERS PUBLICS ==========

    public int GetPhysicalDefense() => _physicalDefense;
    public int GetMagicalDefense() => _magicalDefense;
    public int GetRageStock() => _rageStock;
    public int GetMaxRageStock() => _maxRageStock;
    public bool IsRageStockFull() => _rageStock >= _maxRageStock;
    
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

        // Mécanique de Rage : Génération de carte sur dégâts
        // On ne génère de la rage que si l'unité est encore en vie
        if (_health > 0 && _rageCard != null && _damageThresholdForRage > 0)
        {
            _accumulatedDamage += actualDamage;
            
            if (_accumulatedDamage >= _damageThresholdForRage)
            {
                int cardsToGain = _accumulatedDamage / _damageThresholdForRage;
                _accumulatedDamage %= _damageThresholdForRage;
                
                AddRageCards(cardsToGain);
            }
        }
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

    private void AddRageCards(int count)
    {
        if (this.TryGetComponentSafe(out DeckManager deckManager))
        {
            for (int i = 0; i < count; i++)
            {
                // Ajoute la carte directement à la main (sans limite de taille)
                deckManager.AddCardToHand(_rageCard);
            }
            
            Debug.Log($"😡 RAGE ! {name} gagne {count} carte(s) {_rageCard.cardName} ajoutées à la main suite aux dégâts subis.");
        }
    }

    /// <summary>
    /// Surcharge pour gérer les défenses spécifiques d'Ilya
    /// </summary>
    public override void ModifyStats(int atk, int defP, int defM, int duration)
    {
        base.ModifyStats(atk, defP, defM, duration);
        
        if (defP != 0)
        {
            _physicalDefense += defP;
            Debug.Log($"{name}: DEF P modifiée de {defP} (Total: {_physicalDefense})");
        }
        
        if (defM != 0)
        {
            _magicalDefense += defM;
            Debug.Log($"{name}: DEF M modifiée de {defM} (Total: {_magicalDefense})");
        }
    }

    /// <summary>
    /// Ajoute de la Rage au stock (appelé quand une carte Rage est jouée)
    /// </summary>
    public void AddRageStock(int amount)
    {
        _rageStock += amount;
        OnRageStockChanged?.Invoke(_rageStock);
        Debug.Log($"{name} stocke {amount} Rage. Total: {_rageStock}");
    }

    /// <summary>
    /// Tente de consommer de la Rage du stock
    /// </summary>
    public bool ConsumeRageStock(int amount)
    {
        if (_rageStock < amount) return false;
        
        _rageStock -= amount;
        OnRageStockChanged?.Invoke(_rageStock);
        Debug.Log($"{name} consomme {amount} Rage. Restant: {_rageStock}");
        return true;
    }
}
