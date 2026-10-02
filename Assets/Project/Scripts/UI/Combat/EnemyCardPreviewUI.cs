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
    [SerializeField] private Image _cardIllustrationImage; // Optionnel
    [SerializeField] private GameObject _previewContainer; // Container à masquer quand pas de carte

    [Tooltip("Durée du retournement de la carte quand le monstre passe à sa carte suivante (secondes)")]
    [SerializeField] private float _flipDuration = 0.35f;


    private Enemy _trackedEnemy;

    // Pas d'aperçu avant le premier tour (pendant la phase de placement, le combat n'a pas commencé)
    private bool _battleStarted;

    // Abonné du réveil à la destruction (pas OnEnable/OnDisable) : l'aperçu se cache lui-même
    // (_previewContainer est souvent ce GameObject) et doit quand même recevoir le premier tour
    void Awake()
    {
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
        if (_previewContainer != null) _baseScale = _previewContainer.transform.localScale;
    }

    // Échelle d'origine de la carte (les aperçus des mobs sont réduits) : le retournement la respecte
    private Vector3 _baseScale = Vector3.one;

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
    /// <summary>Aperçu créé en cours de combat (monstre invoqué) : le combat a déjà commencé, il s'affiche tout de suite.</summary>
    public void MarkBattleStarted() => _battleStarted = true;

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
    /// Met à jour l'affichage avec la prochaine carte : retournement de carte si l'aperçu est déjà
    /// visible (le monstre vient de jouer, même si la carte suivante est identique), sinon direct
    /// </summary>
    private void UpdatePreview(CardData nextCard)
    {
        bool visible = _previewContainer != null && _previewContainer.activeInHierarchy && isActiveAndEnabled;
        if (visible && nextCard != null && _battleStarted)
        {
            if (_flip != null) StopCoroutine(_flip);
            _flip = StartCoroutine(FlipTo(nextCard));
            return;
        }
        ShowCard(nextCard);
    }

    private Coroutine _flip;

    // Désactivé en plein retournement (coroutine interrompue) : la carte reprend sa taille
    void OnDisable()
    {
        _flip = null;
        if (_previewContainer != null) _previewContainer.transform.localScale = _baseScale;
    }

    // Demi-tour (la carte se referme), changement de contenu, puis demi-tour inverse
    private System.Collections.IEnumerator FlipTo(CardData nextCard)
    {
        Transform card = _previewContainer.transform;
        float half = _flipDuration / 2f;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            card.localScale = new Vector3(_baseScale.x * (1f - t / half), _baseScale.y, _baseScale.z);
            yield return null;
        }
        ShowCard(nextCard);
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            card.localScale = new Vector3(_baseScale.x * t / half, _baseScale.y, _baseScale.z);
            yield return null;
        }
        card.localScale = _baseScale;
        _flip = null;
    }

    private void ShowCard(CardData nextCard)
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
            // Carte annulée (ex: Sidération) : nom barré, le monstre ne la jouera pas
            // Carte entravée (ex: Aura de terreur) : nom barré, attaque de base à la place
            bool cancelled = _trackedEnemy != null && _trackedEnemy.IsNextCardCancelled;
            bool hindered = _trackedEnemy != null && _trackedEnemy.IsNextCardHindered;
            _cardNameText.text = cancelled ? $"<s>{nextCard.cardName}</s> (annulée)"
                : hindered ? $"<s>{nextCard.cardName}</s> (entravée)"
                : nextCard.cardName;
        }

        if (_cardDescriptionText != null)
        {
            CardTextView.Apply(_cardDescriptionText, nextCard); // texte généré depuis les champs
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
        // Un retournement en cours réafficherait la carte : on l'arrête
        if (_flip != null) { StopCoroutine(_flip); _flip = null; }
        if (_previewContainer != null) _previewContainer.transform.localScale = _baseScale;

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
