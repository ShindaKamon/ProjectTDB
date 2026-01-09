using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Affiche un nombre de dégâts/soins qui flotte au-dessus d'une unité.
/// Pattern: Object Pooling (à implémenter plus tard)
/// Responsabilités:
/// - Afficher le nombre avec couleur appropriée
/// - Animation de montée + fade out
/// - Auto-destruction après animation
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class DamageNumberPopup : MonoBehaviour
{
    // ========== CONFIGURATION ==========

    [Header("Animation Settings")]
    [SerializeField] private float _moveSpeed = 2f;          // Vitesse de montée (unités/sec)
    [SerializeField] private float _lifetime = 1.5f;         // Durée avant disparition
    [SerializeField] private float _fadeStartDelay = 0.5f;   // Délai avant fade
    [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Settings")]
    [SerializeField] private float _startScale = 1.5f;       // Échelle initiale (pop effect)
    [SerializeField] private float _endScale = 1f;           // Échelle finale
    [SerializeField] private float _scaleDuration = 0.3f;    // Durée du scale down

    [Header("Colors")]
    [SerializeField] private Color _damageColor = new Color(1f, 0.2f, 0.2f);      // Rouge pour dégâts
    [SerializeField] private Color _healColor = new Color(0.2f, 1f, 0.2f);        // Vert pour soins
    [SerializeField] private Color _criticalColor = new Color(1f, 0.5f, 0f);      // Orange pour critiques
    [SerializeField] private Color _immuneColor = new Color(0.7f, 0.7f, 0.7f);    // Gris pour immunité

    // ========== COMPOSANTS ==========

    private TextMeshProUGUI _text;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    // ========== ÉTAT ==========

    private Vector3 _startPosition;
    private float _elapsedTime;
    private bool _isAnimating;

    // ========== TYPES ==========

    public enum PopupType
    {
        Damage,
        Heal,
        Critical,
        Immune
    }

    // ========== INITIALISATION ==========

    void Awake()
    {
        // Récupère les composants
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();

        // Ajoute CanvasGroup si absent (pour fade)
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // ========== API PUBLIQUE ==========

    /// <summary>
    /// Configure et affiche le popup de dégâts/soins
    /// </summary>
    /// <param name="value">Valeur à afficher (nombre)</param>
    /// <param name="type">Type de popup (Damage, Heal, Critical, Immune)</param>
    /// <param name="worldPosition">Position monde où afficher le popup</param>
    public void Show(int value, PopupType type, Vector3 worldPosition)
    {
        // Configure le texte
        switch (type)
        {
            case PopupType.Damage:
                _text.text = $"-{value}";
                _text.color = _damageColor;
                break;

            case PopupType.Heal:
                _text.text = $"+{value}";
                _text.color = _healColor;
                break;

            case PopupType.Critical:
                _text.text = $"-{value}!";
                _text.color = _criticalColor;
                _text.fontSize = _text.fontSize * 1.2f; // Plus gros pour critiques
                break;

            case PopupType.Immune:
                _text.text = "IMMUNE";
                _text.color = _immuneColor;
                break;
        }

        // Configure la position initiale (world to screen)
        _startPosition = worldPosition;
        UpdateScreenPosition();

        // État initial
        _elapsedTime = 0f;
        _isAnimating = true;
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one * _startScale;

        // Lance l'animation
        StartCoroutine(AnimatePopup());
    }

    /// <summary>
    /// Affiche un texte personnalisé
    /// </summary>
    public void ShowCustomText(string text, Color color, Vector3 worldPosition)
    {
        _text.text = text;
        _text.color = color;

        _startPosition = worldPosition;
        UpdateScreenPosition();

        _elapsedTime = 0f;
        _isAnimating = true;
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one * _startScale;

        StartCoroutine(AnimatePopup());
    }

    // ========== ANIMATION ==========

    private IEnumerator AnimatePopup()
    {
        float scaleElapsed = 0f;

        while (_elapsedTime < _lifetime)
        {
            _elapsedTime += Time.deltaTime;

            // 1. SCALE DOWN (effet "pop")
            if (scaleElapsed < _scaleDuration)
            {
                scaleElapsed += Time.deltaTime;
                float scaleProgress = scaleElapsed / _scaleDuration;
                float currentScale = Mathf.Lerp(_startScale, _endScale, scaleProgress);
                transform.localScale = Vector3.one * currentScale;
            }

            // 2. MOUVEMENT VERS LE HAUT
            float moveProgress = _elapsedTime / _lifetime;
            float curvedProgress = _moveCurve.Evaluate(moveProgress);
            float yOffset = curvedProgress * _moveSpeed * _lifetime;

            Vector3 currentWorldPos = _startPosition + Vector3.up * yOffset;
            UpdateScreenPosition(currentWorldPos);

            // 3. FADE OUT (après délai)
            if (_elapsedTime > _fadeStartDelay)
            {
                float fadeProgress = (_elapsedTime - _fadeStartDelay) / (_lifetime - _fadeStartDelay);
                _canvasGroup.alpha = 1f - fadeProgress;
            }

            yield return null;
        }

        // Animation terminée
        _isAnimating = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// Convertit une position monde en position écran pour le RectTransform
    /// </summary>
    private void UpdateScreenPosition(Vector3? worldPosition = null)
    {
        Vector3 worldPos = worldPosition ?? _startPosition;

        // Convertit world → screen space
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("DamageNumberPopup: Camera.main est null");
            return;
        }

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPos);

        // Vérifie si la position est devant la caméra
        if (screenPoint.z < 0)
        {
            // Derrière la caméra, cache le popup
            _canvasGroup.alpha = 0f;
            return;
        }

        // Convertit screen → canvas space
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("DamageNumberPopup: Pas de Canvas parent trouvé");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Convertit en coordonnées locales du canvas
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out canvasPosition
        );

        _rectTransform.anchoredPosition = canvasPosition;
    }

    // ========== UPDATE ==========

    void Update()
    {
        // Si on anime et que la caméra bouge, met à jour la position
        if (_isAnimating)
        {
            // La position est déjà mise à jour dans la coroutine
            // Mais on peut ajouter ici un ajustement si nécessaire
        }
    }

    // ========== UTILITAIRES ==========

    /// <summary>
    /// Crée un popup de dégâts à une position donnée
    /// </summary>
    public static DamageNumberPopup Create(GameObject prefab, int value, PopupType type, Vector3 worldPosition, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("DamageNumberPopup.Create: Prefab est null!");
            return null;
        }

        GameObject popupObj = Instantiate(prefab, parent);
        DamageNumberPopup popup = popupObj.GetComponent<DamageNumberPopup>();

        if (popup == null)
        {
            Debug.LogError("DamageNumberPopup.Create: Le prefab n'a pas de composant DamageNumberPopup!");
            Destroy(popupObj);
            return null;
        }

        popup.Show(value, type, worldPosition);
        return popup;
    }
}
