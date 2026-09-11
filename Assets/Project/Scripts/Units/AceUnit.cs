using UnityEngine;

/// <summary>
/// AceUnit hérite de Champion et représente le champion Ace.
/// Passif : Main gagnante — analyse la carte en cours par rapport à la précédente jouée ce
/// tour et déclenche un bonus selon le motif reconnu par cette paire :
/// - Bluff (émotions différentes) → -10% dégâts subis jusqu'au prochain tour.
/// - Suite (coûts N puis N+1) → +1 PA immédiat.
/// - Paire (même coût) → cette carte ignore les réductions de dégâts en % de la cible.
/// Le plus exigeant l'emporte si une paire remplit plusieurs critères à la fois (Bluff >
/// Suite > Paire). Version provisoire (choix de cible/déclenchement automatique plutôt qu'un
/// choix du joueur) — cf. notes de design du classeur source, à retravailler en playtest.
/// </summary>
public class AceUnit : Champion, IActionPointsUser, IComboTracker
{
    [Header("=== Main gagnante ===")]
    [Tooltip("Réduction des dégâts subis quand le bouclier Bluff est actif (0.10 = -10%)")]
    [SerializeField] private float _bluffDamageReduction = 0.10f;

    private CardData _lastCardPlayedThisTurn;
    private int _paSpentThisTurn = 0;
    private bool _hasBluffShield = false;
    private bool _ignoreReductionThisCard = false;

    public int PASpentThisTurn => _paSpentThisTurn;
    public bool ShouldIgnoreDamageReduction => _ignoreReductionThisCard;

    public new void Initialize(ChampionData data, Vector2Int initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        GameLog.Log($"{name} (Ace) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
    }

    protected override void Start()
    {
        base.Start();

        if (Services.IsBattleUIServiceAvailable())
        {
            Services.BattleUI.RegisterPlayer(this);
        }
    }

    // ========== IComboTracker ==========

    public void OnCardAboutToExecute(CardData card)
    {
        _ignoreReductionThisCard = false;

        if (_lastCardPlayedThisTurn != null)
        {
            bool differentEmotion = card.emotionType != _lastCardPlayedThisTurn.emotionType
                && card.emotionType != EmotionType.None
                && _lastCardPlayedThisTurn.emotionType != EmotionType.None;
            bool isSuite = card.costPA == _lastCardPlayedThisTurn.costPA + 1;
            bool isPaire = card.costPA == _lastCardPlayedThisTurn.costPA;

            if (differentEmotion)
            {
                _hasBluffShield = true;
                GameLog.Log($"[Main gagnante] Bluff ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {name} réduit les prochains dégâts subis de {_bluffDamageReduction:P0} jusqu'à son prochain tour.");
            }
            else if (isSuite)
            {
                AddPA(1); // hérité de Champion
                GameLog.Log($"[Main gagnante] Suite ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {name} gagne 1 PA immédiat.");
            }
            else if (isPaire)
            {
                _ignoreReductionThisCard = true;
                GameLog.Log($"[Main gagnante] Paire ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {card.cardName} ignore les réductions de dégâts en % de la cible.");
            }
        }
    }

    public void OnCardResolved(CardData card)
    {
        _paSpentThisTurn += card.costPA;
        _lastCardPlayedThisTurn = card;
    }

    // ========== BOUCLIER BLUFF (dégâts subis) ==========

    public override void TakeDamage(int rawDamage)
    {
        if (_hasBluffShield && rawDamage > 0)
        {
            int reduced = Mathf.Max(1, Mathf.RoundToInt(rawDamage * (1f - _bluffDamageReduction)));
            GameLog.Log($"[Main gagnante] {name} réduit {rawDamage} -> {reduced} dégâts (bouclier Bluff actif)");
            base.TakeDamage(reduced);
            return;
        }

        base.TakeDamage(rawDamage);
    }

    /// <summary>
    /// Réinitialise l'historique de combo et le bouclier Bluff au début du tour d'Ace
    /// (protège tout le tour adverse, comme le Réflexe du grimpeur de L'Alpiniste).
    /// </summary>
    public override void ProcessBuffsOnTurnStart()
    {
        if (_hasBluffShield)
        {
            _hasBluffShield = false;
            GameLog.Log($"[Main gagnante] Bouclier Bluff de {name} expiré (nouveau tour)");
        }

        _lastCardPlayedThisTurn = null;
        _paSpentThisTurn = 0;

        base.ProcessBuffsOnTurnStart();
    }
}
