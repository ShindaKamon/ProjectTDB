using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Affiche la prochaine carte qu'un ennemi va jouer.
/// Cette UI donne au joueur l'information stratégique pour anticiper les actions ennemies.
/// </summary>
public class EnemyCardPreviewUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;
    [SerializeField] private TextMeshProUGUI _cardCostText;
    [SerializeField] private Image _cardIllustrationImage; // Optionnel
    [SerializeField] private GameObject _previewContainer; // Container à masquer quand pas de carte


    private Enemy _trackedEnemy;

    // Pas d'aperçu avant le premier tour (pendant la phase de placement, le combat n'a pas commencé)
    private bool _battleStarted;

    // Abonné du réveil à la destruction (pas OnEnable/OnDisable) : l'aperçu se cache lui-même
    // (_previewContainer est souvent ce GameObject) et doit quand même recevoir le premier tour
    void Awake() => EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);

    private void OnTurnChanged(TurnChangedEvent e)
    {
        if (_battleStarted) return;

        _battleStarted = true;
        if (_trackedEnemy != null) UpdatePreview(_trackedEnemy.GetNextCard());
    }

    void Start()
    {
        GameLog.Log("EnemyCardPreviewUI: Start() appelé");

        // Cache le preview au démarrage. Start peut n'arriver qu'au lancement du combat (aperçu
        // masqué avant son premier Start) : il ne doit pas alors recacher la carte qui vient de s'afficher
        if (_previewContainer != null && _battleStarted)
        {
            // rien à faire : l'aperçu est déjà à jour
        }
        else if (_previewContainer != null)
        {
            _previewContainer.SetActive(false);
            GameLog.Log("EnemyCardPreviewUI: Container caché au démarrage");
        }
        else
        {
            GameLog.LogWarning("EnemyCardPreviewUI: _previewContainer est null! Assigne-le dans l'Inspector.");
        }
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);

        // Désabonne des événements
        if (_trackedEnemy != null)
        {
            _trackedEnemy.OnNextCardChanged -= UpdatePreview;
        }
    }

    /// <summary>
    /// Assigne l'ennemi à tracker pour afficher sa prochaine carte
    /// </summary>
    public void SetTrackedEnemy(Enemy enemy)
    {
        // Désabonne de l'ancien ennemi si présent
        if (_trackedEnemy != null)
        {
            _trackedEnemy.OnNextCardChanged -= UpdatePreview;
        }

        _trackedEnemy = enemy;

        if (_trackedEnemy != null)
        {
            // S'abonne aux changements de carte
            _trackedEnemy.OnNextCardChanged += UpdatePreview;

            // Affiche la carte actuelle
            UpdatePreview(_trackedEnemy.GetNextCard());
        }
        else
        {
            // Pas d'ennemi, cache le preview
            if (_previewContainer != null)
            {
                _previewContainer.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Met à jour l'affichage avec la prochaine carte
    /// </summary>
    private void UpdatePreview(CardData nextCard)
    {
        if (nextCard == null || !_battleStarted)
        {
            // Pas de carte à afficher
            if (_previewContainer != null)
            {
                _previewContainer.SetActive(false);
            }
            return;
        }

        // Active le container
        if (_previewContainer != null)
        {
            _previewContainer.SetActive(true);
        }

        // Met à jour les textes
        if (_cardNameText != null)
        {
            _cardNameText.text = nextCard.cardName;
        }

        if (_cardDescriptionText != null)
        {
            CardTextView.Apply(_cardDescriptionText, nextCard); // texte généré depuis les champs
        }

        if (_cardCostText != null)
        {
            _cardCostText.text = nextCard.costPA.ToString();
        }


        // Met à jour l'illustration si disponible
        if (_cardIllustrationImage != null && nextCard.artwork != null)
        {
            _cardIllustrationImage.sprite = nextCard.artwork;
            _cardIllustrationImage.enabled = true;
        }
        else if (_cardIllustrationImage != null)
        {
            _cardIllustrationImage.enabled = false;
        }

        GameLog.Log($"EnemyCardPreviewUI: Affiche prochaine carte - {nextCard.cardName}");
    }

    /// <summary>
    /// Cache le preview manuellement (utilisé quand l'ennemi meurt, etc.)
    /// </summary>
    public void HidePreview()
    {
        if (_previewContainer != null)
        {
            _previewContainer.SetActive(false);
        }

        // Désabonne de l'ennemi
        if (_trackedEnemy != null)
        {
            _trackedEnemy.OnNextCardChanged -= UpdatePreview;
            _trackedEnemy = null;
        }
    }
}
