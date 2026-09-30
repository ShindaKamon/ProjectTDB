using UnityEngine;

/// <summary>
/// RazeUnit hérite de Champion et représente le champion Raze.
/// Passif : Main gagnante — analyse la carte en cours par rapport à la précédente jouée ce
/// tour et déclenche un bonus selon le motif reconnu par cette paire :
/// - Bluff (émotions différentes) → bouclier (comme celui des cartes, sans durée).
/// - Suite (coûts N puis N+1) → +1 PA immédiat.
/// - Paire (même coût) → cette carte ignore les réductions de dégâts en % de la cible.
/// Le plus exigeant l'emporte si une paire remplit plusieurs critères à la fois (Bluff >
/// Suite > Paire). Version provisoire (choix de cible/déclenchement automatique plutôt qu'un
/// choix du joueur) — cf. notes de design du classeur source, à retravailler en playtest.
/// </summary>
public class RazeUnit : Champion, IComboTracker
{
    [Header("=== Main gagnante ===")]
    [Tooltip("Bouclier gagné à chaque Bluff (PV absorbés, sans durée)")]
    [SerializeField] private int _bluffShield = 8;

    private CardData _lastCardPlayedThisTurn;
    private int _lastCardCost; // coût réellement payé (Triche comprise), noté avant que la carte quitte la main
    private int _paSpentThisTurn = 0;
    private bool _ignoreReductionThisCard = false;

    public int PASpentThisTurn => _paSpentThisTurn;
    public bool ShouldIgnoreDamageReduction => _ignoreReductionThisCard;
    public ComboPattern CurrentPattern { get; private set; }

    // ========== IComboTracker ==========

    public void OnCardAboutToExecute(CardData card)
    {
        _ignoreReductionThisCard = false;
        CurrentPattern = ComboPattern.None;

        // All-in (fusion) : toutes les combinaisons se cumulent, et chaque carte compte comme une Suite
        AllInFusion allIn = ActiveFusion as AllInFusion;
        int multiplier = allIn != null ? allIn.patternMultiplier : 1;

        int cost = EffectiveCost(card);
        bool hasPrevious = _lastCardPlayedThisTurn != null;
        bool isBluff = hasPrevious && card.emotionType != _lastCardPlayedThisTurn.emotionType
            && card.emotionType != EmotionType.None
            && _lastCardPlayedThisTurn.emotionType != EmotionType.None;
        bool isSuite = hasPrevious && cost == _lastCardCost + 1;
        bool isPair = hasPrevious && cost == _lastCardCost;
        if (allIn != null)
        {
            isSuite = true;
        }
        else
        {
            isSuite &= !isBluff;
            isPair &= !isBluff && !isSuite;
        }

        int paSpent = cost + (hasPrevious ? _lastCardCost : 0);
        string previousName = hasPrevious ? _lastCardPlayedThisTurn.cardName : "—";

        if (isBluff)
        {
            GameLog.Log($"[Main gagnante] Bluff ({previousName} -> {card.cardName}) : {name} gagne un bouclier de {_bluffShield * multiplier}.");
            AddShield(_bluffShield * multiplier, this);
            FormPattern(ComboPattern.Bluff, paSpent);
        }
        if (isSuite)
        {
            AddPA(multiplier); // hérité de Champion
            GameLog.Log($"[Main gagnante] Suite ({previousName} -> {card.cardName}) : {name} gagne {multiplier} PA immédiat.");
            FormPattern(ComboPattern.Suite, paSpent);
        }
        if (isPair)
        {
            _ignoreReductionThisCard = true;
            GameLog.Log($"[Main gagnante] Paire ({previousName} -> {card.cardName}) : {card.cardName} ignore les réductions de dégâts en % de la cible.");
            FormPattern(ComboPattern.Pair, paSpent);
        }
    }

    // Motif reconnu : retenu si c'est le plus exigeant de la carte (Bluff > Suite > Paire), et signalé à la fusion
    private void FormPattern(ComboPattern pattern, int paSpent)
    {
        if (CurrentPattern == ComboPattern.None || PatternRank(pattern) > PatternRank(CurrentPattern)) CurrentPattern = pattern;
        ActiveFusion?.OnComboPattern(this, pattern, paSpent);
    }

    private static int PatternRank(ComboPattern pattern) => pattern == ComboPattern.Bluff ? 3 : pattern == ComboPattern.Suite ? 2 : 1;

    public void OnCardResolved(CardData card)
    {
        _lastCardCost = EffectiveCost(card);
        _paSpentThisTurn += _lastCardCost;

        // All-in : contrecoup en PV pour chaque PA dépensé (sans jamais tuer Raze)
        if (ActiveFusion is AllInFusion allIn && allIn.recoilPerPA > 0)
            PayHealth(Mathf.Min(allIn.recoilPerPA * _lastCardCost, GetHealth() - 1));
        NotifyStatsModified(); // la main réaffiche les dégâts de Tapis
        _lastCardPlayedThisTurn = card;
    }

    // Coût réel d'une carte en main, modification de Triche comprise
    private int EffectiveCost(CardData card) =>
        this.TryGetComponentSafe(out DeckManager deck) ? deck.GetEffectiveCost(card) : card.costPA;

    /// <summary>
    /// Réinitialise l'historique de combo au début du tour de Raze
    /// </summary>
    public override void OnOwnTurnStart()
    {
        _lastCardPlayedThisTurn = null;
        _paSpentThisTurn = 0;
        NotifyStatsModified();

        base.OnOwnTurnStart();
    }
}
