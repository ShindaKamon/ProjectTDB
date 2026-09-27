using UnityEngine;
using TMPro;

/// <summary>
/// Affiche un indicateur "Votre tour" / "Tour ennemi" selon l'état de la TurnStateMachine.
/// S'abonne à TurnStateChangedEvent (EventBus) plutôt qu'à la state machine directement,
/// pour rester découplé (même pattern que CombatFeedbackManager).
/// </summary>
public class TurnIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _container;
    [SerializeField] private TextMeshProUGUI _turnText;

    [Header("Textes")]
    [SerializeField] private string _playerTurnLabel = "Votre tour";
    [SerializeField] private string _enemyTurnLabel = "Tour ennemi";

    void OnEnable()
    {
        EventBus.Subscribe<TurnStateChangedEvent>(OnTurnStateChanged);

        // Affiche l'état courant tout de suite, au cas où le combat aurait déjà commencé
        // avant que ce composant ne s'active (évite un indicateur vide/faux au premier tour).
        RefreshFromCurrentState();
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<TurnStateChangedEvent>(OnTurnStateChanged);
    }

    private void RefreshFromCurrentState()
    {
        // Au chargement de la scène, GridManager peut ne pas être encore enregistré (ordre des Awake/OnEnable)
        if (!Services.IsGridServiceAvailable()) return;

        TurnStateMachine turnStateMachine = Services.Grid.GetTurnStateMachine();
        if (turnStateMachine == null) return;

        ApplyState(turnStateMachine.GetCurrentState());
    }

    private void OnTurnStateChanged(TurnStateChangedEvent evt)
    {
        ApplyState(evt.NewState);
    }

    /// <summary>
    /// Met à jour l'affichage selon l'état de tour.
    /// Cas limites gérés explicitement :
    /// - TransitioningTurn : ne touche à rien, pour éviter un clignotement pendant l'animation
    ///   de transition (le texte du tour précédent reste affiché jusqu'au prochain état stable).
    /// - BattleEnd (et Initializing) : masque proprement l'indicateur.
    /// </summary>
    private void ApplyState(TurnState state)
    {
        switch (state)
        {
            case TurnState.PlayerTurn:
                SetVisible(true, _playerTurnLabel);
                break;

            case TurnState.EnemyTurn:
                SetVisible(true, _enemyTurnLabel);
                break;

            case TurnState.TransitioningTurn:
                // Ne rien faire : pas de clignotement pendant la transition entre tours.
                break;

            case TurnState.BattleEnd:
            case TurnState.Initializing:
            default:
                SetVisible(false, string.Empty);
                break;
        }
    }

    private void SetVisible(bool visible, string text)
    {
        if (_turnText != null)
        {
            _turnText.text = text;
        }

        if (_container != null)
        {
            _container.SetActive(visible);
        }
        else if (_turnText != null)
        {
            _turnText.gameObject.SetActive(visible);
        }
    }
}
