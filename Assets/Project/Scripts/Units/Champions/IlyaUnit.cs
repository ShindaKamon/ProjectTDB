using UnityEngine;

/// <summary>
/// IlyaUnit hérite de Champion et représente Ilya, « le Dévoué enchaîné » (champion de test, hors
/// roster MVP, sans Éveil : ses cartes sont toutes des Signatures neutres).
/// Passif : Colère enchaînée — tous les PointsPerRageCard PV perdus (coups, PV payés, contrecoups),
/// une carte RAGE arrive en main ; jouée, elle remplit le stock de Rage (RageGauge). Stock plein, la
/// carte Chaînes brisées arrive en main : Ilya passe en forme Déchaînée pour quelques tours (+PA,
/// +PM, vol de vie, perte de PV à chaque tour, plus de Rage). Règles : Docs/GDD/archive/ilya_deck_simple.md.
/// </summary>
public class IlyaUnit : Champion, IRageUser
{
    [Header("=== Colère enchaînée ===")]
    [Tooltip("Carte ajoutée à la main à chaque palier de PV perdus")]
    [SerializeField] private CardData _rageCard;
    [Tooltip("Carte ajoutée à la main quand le stock de Rage est plein")]
    [SerializeField] private CardData _breakChainsCard;
    [Tooltip("PV perdus pour une carte RAGE")]
    [SerializeField] private int _healthPerRageCard = 10;

    [Header("=== Forme Déchaînée ===")]
    [Tooltip("Durée en tours d'Ilya, celui où il brise ses chaînes compris")]
    [SerializeField] private int _unchainedTurns = 3;
    [Tooltip("PA et PM maximum en plus")]
    [SerializeField] private int _unchainedBonus = 1;
    [Tooltip("PV soignés en brisant ses chaînes")]
    [SerializeField] private int _unchainedHeal = 15;
    [Tooltip("Part des dégâts infligés par Ilya qu'il récupère en PV (%)")]
    [SerializeField] private int _unchainedLifestealPercent = 25;
    [Tooltip("PV perdus au début de chaque tour (Ilya ne meurt jamais de cette perte)")]
    [SerializeField] private int _unchainedHealthLossPerTurn = 5;

    private RageGauge _rage;
    private int _lastHealth = -1;

    private RageGauge Rage => _rage ??= new RageGauge(_healthPerRageCard);

    // ========== IRageUser ==========

    public int RageStock => Rage.Stock;
    public bool IsUnchained => UnchainedTurnsLeft > 0;
    public int UnchainedTurnsLeft { get; private set; }
    public bool CanBreakChains => Rage.IsFull && !IsUnchained;

    public void GainRage(int amount)
    {
        if (IsUnchained || Rage.Gain(amount) == 0) return;
        GameLog.Log($"😡 {name} : Rage {Rage.Stock}/{RageGauge.MaxStock}");
        RefreshRageCards();
    }

    public int ConsumeAllRage()
    {
        int consumed = Rage.ConsumeAll();
        RefreshRageCards();
        return consumed;
    }

    public void BreakChains()
    {
        if (!CanBreakChains) return;

        Rage.ConsumeAll();
        UnchainedTurnsLeft = _unchainedTurns;
        SetMaxPA(GetMaxPA() + _unchainedBonus);
        SetMaxMovementPoints(GetMaxMovementPoints() + _unchainedBonus);
        AddPA(_unchainedBonus);       // le bonus vaut dès ce tour
        GainMovement(_unchainedBonus);
        HealFrom(_unchainedHeal, this);
        RefreshRageCards();
        GameLog.Log($"⛓️ {name} brise ses chaînes : forme Déchaînée pour {_unchainedTurns} tours");
    }

    // ========== RAGE ==========

    protected override void Start()
    {
        base.Start();
        OnHealthChanged += HandleHealthChanged;
        _lastHealth = GetHealth();
    }

    protected override void OnDestroy()
    {
        OnHealthChanged -= HandleHealthChanged;
        base.OnDestroy();
    }

    void OnEnable() => EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
    void OnDisable() => EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);

    // PV perdus (coup, PV payés, contrecoup) : une carte RAGE par palier, hors forme Déchaînée
    private void HandleHealthChanged(int health, int maxHealth)
    {
        int lost = _lastHealth - health;
        _lastHealth = health;
        if (lost <= 0 || health <= 0 || IsUnchained || _rageCard == null) return;
        if (!this.TryGetComponentSafe(out DeckManager deck)) return;

        int cards = Rage.OnHealthLost(lost, deck.GetHand().FindAll(c => c == _rageCard).Count);
        for (int i = 0; i < cards; i++) deck.AddCardToHand(_rageCard);
        if (cards > 0) GameLog.Log($"😡 {name} perd {lost} PV : {cards} carte(s) {_rageCard.cardName} en main");
    }

    // Forme Déchaînée : vol de vie sur les dégâts qu'Ilya inflige aux ennemis
    private void OnUnitDamaged(UnitDamagedEvent e)
    {
        if (!IsUnchained || e.Source != this || e.Target == null || e.Target.GetFaction() == GetFaction()) return;
        int healed = e.EffectiveDamage * _unchainedLifestealPercent / 100;
        if (healed > 0) HealFrom(healed, this);
    }

    // Chaînes brisées en main seulement quand elle est jouable ; plus de RAGE en forme Déchaînée
    private void RefreshRageCards()
    {
        if (this.TryGetComponentSafe(out DeckManager deck))
        {
            if (IsUnchained && _rageCard != null) deck.RemoveAllFromHand(_rageCard);
            if (_breakChainsCard != null)
            {
                bool inHand = deck.GetHand().Contains(_breakChainsCard);
                if (CanBreakChains && !inHand) deck.AddCardToHand(_breakChainsCard);
                else if (!CanBreakChains && inHand) deck.RemoveAllFromHand(_breakChainsCard);
            }
        }
        NotifyStatsModified(); // pastilles de Rage
    }

    public override void OnOwnTurnStart()
    {
        base.OnOwnTurnStart();
        if (!IsUnchained) return;

        UnchainedTurnsLeft--;
        if (UnchainedTurnsLeft > 0)
        {
            PayHealth(Mathf.Min(_unchainedHealthLossPerTurn, GetHealth() - 1));
        }
        else
        {
            // Retour à la forme Enchaînée : PA et PM déjà remis à niveau pour ce tour, on retire le bonus
            SetMaxPA(GetMaxPA() - _unchainedBonus);
            SetMaxMovementPoints(GetMaxMovementPoints() - _unchainedBonus);
            SpendMovement(_unchainedBonus);
            GameLog.Log($"⛓️ {name} reprend ses chaînes");
        }
        RefreshRageCards();
    }
}
