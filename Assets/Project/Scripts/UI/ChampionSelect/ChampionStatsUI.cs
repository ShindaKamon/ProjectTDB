using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche les statistiques d'un champion de maniere moderne et modulaire
/// Panel bas-gauche du systeme de selection
/// </summary>
public class ChampionStatsUI : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] private Image _championPortrait;
    [SerializeField] private Image _portraitFrame;

    [Header("Identite")]
    [SerializeField] private TextMeshProUGUI _championNameText;
    [SerializeField] private TextMeshProUGUI _championTitleText;
    [SerializeField] private TextMeshProUGUI _familyText;

    [Header("Stats - Ligne 1")]
    [SerializeField] private StatDisplayUI _healthStat;
    [SerializeField] private StatDisplayUI _movementStat;
    [SerializeField] private StatDisplayUI _actionPointsStat;

    [Header("Stats - Ligne 2")]
    [SerializeField] private StatDisplayUI _attackStat;
    [SerializeField] private StatDisplayUI _defenseStat;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Animation")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.2f;

    private ChampionData _currentChampion;

    /// <summary>
    /// Affiche les informations d'un champion
    /// </summary>
    public void ShowChampion(ChampionData champion)
    {
        if (champion == null)
        {
            Hide();
            return;
        }

        _currentChampion = champion;

        // Portrait
        if (_championPortrait != null && champion.portrait != null)
        {
            _championPortrait.sprite = champion.portrait;
            _championPortrait.color = Color.white;
        }
        else if (_championPortrait != null)
        {
            _championPortrait.color = new Color(0.3f, 0.3f, 0.3f);
        }

        // Identite
        if (_championNameText != null)
            _championNameText.text = champion.championName;

        if (_championTitleText != null)
            _championTitleText.text = champion.title ?? "";

        if (_familyText != null)
            _familyText.text = CodexCardVisual.EmotionName(champion.emotionType);

        // Stats - Ligne 1
        if (_healthStat != null)
            _healthStat.SetValue(champion.maxHealth, "PV");

        if (_movementStat != null)
            _movementStat.SetValue(champion.movementRange, "PM");

        if (_actionPointsStat != null)
            _actionPointsStat.SetValue(champion.maxActionPoints, "PA");

        // Stats - Ligne 2
        if (_attackStat != null)
            _attackStat.SetValue(champion.attackDamage, "ATQ");

        if (_defenseStat != null)
            _defenseStat.SetValue(champion.defense, "DEF");

        // Description
        if (_descriptionText != null)
            _descriptionText.text = champion.description ?? "";

        // Animation d'apparition
        if (_canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Cache le panneau des stats
    /// </summary>
    public void Hide()
    {
        if (_canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = elapsed / _fadeDuration;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}

