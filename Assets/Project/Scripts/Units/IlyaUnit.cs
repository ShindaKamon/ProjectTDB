using UnityEngine;

/// <summary>
/// IlyaUnit hérite de Champion et représente le personnage jouable principal.
/// Spécificités :
/// - PA (Points d'Action) pour jouer des cartes (hérité de Champion)
/// - PM (Points de Mouvement) pour se déplacer (hérité de Unit)
/// - DEF (Défense) - réduit les dégâts subis
/// - Système de Rage (génération de cartes Rage sur dégâts + stock)
/// </summary>
public class IlyaUnit : Champion, IActionPointsUser, IRageUser
{
    // ========== STATS SPÉCIFIQUES ILYA ==========

    // Défense - initialisée depuis ChampionData.defense
    private int _defense;

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

    public int GetDefense() => _defense;
    public int GetRageStock() => _rageStock;
    public int GetMaxRageStock() => _maxRageStock;
    public bool IsRageStockFull() => _rageStock >= _maxRageStock;

    // ========== INITIALISATION ==========

    /// <summary>
    /// Surcharge Initialize pour initialiser la défense depuis ChampionData
    /// </summary>
    public new void Initialize(ChampionData data, Vector2 initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        // Initialise la défense depuis ChampionData
        _defense = data.defense;

        Debug.Log($"{name} (Ilya) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, DEF: {_defense}, ATK: {GetAttack()}");
    }

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
    }

    // NOTE: SpendPA() et RefreshPA() sont maintenant hérités de Champion

    // ========== SYSTÈME DE DÉFENSE ==========

    /// <summary>
    /// Surcharge TakeDamage pour appliquer la défense
    /// </summary>
    public override void TakeDamage(int rawDamage)
    {
        // Applique la défense (minimum 1 dégât)
        int actualDamage = Mathf.Max(1, rawDamage - _defense);

        // Capture les PV avant dégâts pour calculer la perte réelle (après partage éventuel)
        int hpBefore = _health;

        // Applique les dégâts (appelle la méthode de base Unit)
        base.TakeDamage(actualDamage);

        int damageTaken = hpBefore - _health;
        Debug.Log($"{name} prend {damageTaken} dégâts réels (Calculé: {actualDamage}, Brut: {rawDamage}, DEF: {_defense})");

        // Mécanique de Rage : Génération de carte sur dégâts
        TryGenerateRage(damageTaken);
    }

    /// <summary>
    /// Surcharge PayHealth pour générer de la Rage quand Ilya paie des PV (coût de cartes)
    /// </summary>
    public new void PayHealth(int amount)
    {
        if (amount <= 0) return;

        // Appelle la méthode de base pour le paiement des PV
        base.PayHealth(amount);

        // Mécanique de Rage : Génération de carte sur perte de PV
        TryGenerateRage(amount);
    }

    /// <summary>
    /// Tente de générer des cartes Rage basé sur les dégâts/PV perdus accumulés
    /// </summary>
    private void TryGenerateRage(int damageAmount)
    {
        // Ne génère de la rage que si l'unité est encore en vie et configurée pour
        if (_health <= 0 || _rageCard == null || _damageThresholdForRage <= 0) return;

        _accumulatedDamage += damageAmount;

        if (_accumulatedDamage >= _damageThresholdForRage)
        {
            int cardsToGain = _accumulatedDamage / _damageThresholdForRage;
            _accumulatedDamage %= _damageThresholdForRage;
            AddRageCards(cardsToGain);
        }
    }

    /// <summary>
    /// Modifie la défense
    /// </summary>
    public void SetDefense(int value)
    {
        _defense = value;
        NotifyStatsModified();
        Debug.Log($"{name}: Défense changée à {_defense}");
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
    /// Surcharge pour gérer la défense d'Ilya
    /// </summary>
    public override void ModifyStats(int atk, int def, int duration)
    {
        // Modifie la défense AVANT d'appeler base (qui déclenche OnStatsModified)
        if (def != 0)
        {
            _defense += def;
            Debug.Log($"{name}: DEF modifiée de {def} (Total: {_defense})");
        }

        // Appelle la classe de base (gère ATK, buffs temporaires et déclenche OnStatsModified)
        base.ModifyStats(atk, def, duration);
    }

    /// <summary>
    /// Surcharge pour retirer les buffs DEF expirés
    /// </summary>
    public override void ProcessBuffsOnTurnStart()
    {
        // Parcourt les buffs pour retirer la DEF des buffs expirés
        var buffs = GetActiveBuffs();
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            StatBuff buff = buffs[i];
            if (buff.remainingTurns <= 1 && buff.defModifier != 0)
            {
                // Le buff va expirer, retire la DEF
                _defense -= buff.defModifier;
                Debug.Log($"{name}: Buff DEF expiré - DEF restaurée de {buff.defModifier} (Total: {_defense})");
            }
        }

        // Appelle la classe de base (gère ATK et la liste des buffs)
        base.ProcessBuffsOnTurnStart();
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
