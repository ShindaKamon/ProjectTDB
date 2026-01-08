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

    void Start()
    {
        Debug.Log("EnemyCardPreviewUI: Start() appelé");

        // Cache le preview au démarrage
        if (_previewContainer != null)
        {
            _previewContainer.SetActive(false);
            Debug.Log("EnemyCardPreviewUI: Container caché au démarrage");
        }
        else
        {
            Debug.LogWarning("EnemyCardPreviewUI: _previewContainer est null! Assigne-le dans l'Inspector.");
        }
    }

    void OnDestroy()
    {
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
        if (nextCard == null)
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
            _cardDescriptionText.text = nextCard.description;
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

        Debug.Log($"EnemyCardPreviewUI: Affiche prochaine carte - {nextCard.cardName}");
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
