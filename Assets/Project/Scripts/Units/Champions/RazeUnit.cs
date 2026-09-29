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

    // ========== IComboTracker ==========

    public void OnCardAboutToExecute(CardData card)
    {
        _ignoreReductionThisCard = false;

        if (_lastCardPlayedThisTurn != null)
        {
            bool differentEmotion = card.emotionType != _lastCardPlayedThisTurn.emotionType
                && card.emotionType != EmotionType.None
                && _lastCardPlayedThisTurn.emotionType != EmotionType.None;
            int cost = EffectiveCost(card);
            bool isSuite = cost == _lastCardCost + 1;
            bool isPair = cost == _lastCardCost;

            if (differentEmotion)
            {
                GameLog.Log($"[Main gagnante] Bluff ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {name} gagne un bouclier de {_bluffShield}.");
                AddShield(_bluffShield, this);
            }
            else if (isSuite)
            {
                AddPA(1); // hérité de Champion
                GameLog.Log($"[Main gagnante] Suite ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {name} gagne 1 PA immédiat.");
            }
            else if (isPair)
            {
                _ignoreReductionThisCard = true;
                GameLog.Log($"[Main gagnante] Paire ({_lastCardPlayedThisTurn.cardName} -> {card.cardName}) : {card.cardName} ignore les réductions de dégâts en % de la cible.");
            }
        }
    }

    public void OnCardResolved(CardData card)
    {
        _lastCardCost = EffectiveCost(card);
        _paSpentThisTurn += _lastCardCost;
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
