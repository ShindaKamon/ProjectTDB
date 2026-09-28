using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [FormerlySerializedAs("healthSlider")] [SerializeField] private Slider _healthSlider;
    [FormerlySerializedAs("followTarget")] [SerializeField] private Transform _followTarget;  // Le personnage à suivre

    [Header("Optional Text")]
    [FormerlySerializedAs("healthText")] [SerializeField] private TextMeshProUGUI _healthText;  // Support pour TextMeshPro

    [Header("Settings")]
    [FormerlySerializedAs("offset")] [SerializeField] private Vector3 _offset = new Vector3(0, 2f, 0);  // Offset au-dessus de la tête
    [FormerlySerializedAs("smoothFollow")] [SerializeField] private bool _smoothFollow = true;
    [FormerlySerializedAs("followSpeed")] [SerializeField] private float _followSpeed = 10f;

    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform rectTransform;
    private bool _hasInitializedPosition = false;

    void Awake()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // Configure le slider
        if (_healthSlider != null)
        {
            _healthSlider.minValue = 0;
            _healthSlider.maxValue = 1;
            _healthSlider.direction = Slider.Direction.LeftToRight;
            _healthSlider.transition = Selectable.Transition.None;
            _healthSlider.interactable = false;
        }
    }

    /// <summary>
    /// Assigne la cible à suivre et son offset (appelé par HealthBarManager après instanciation).
    /// </summary>
    public void SetFollowTarget(Transform target, Vector3 offset)
    {
        _followTarget = target;
        _offset = offset;
    }

    void LateUpdate()
    {
        if (_followTarget == null || mainCamera == null) return;

        // Position world du target + offset
        Vector3 worldPos = _followTarget.position + _offset;

        // Convertit en screen space
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // Si derrière la caméra, cache cette barre uniquement
        if (screenPos.z < 0)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }

        // Applique la position
        if (rectTransform != null)
        {
            // Au premier frame, positionne directement sans Lerp pour éviter le "vol" des barres
            if (!_hasInitializedPosition)
            {
                rectTransform.position = screenPos;
                _hasInitializedPosition = true;
            }
            else if (_smoothFollow)
            {
                rectTransform.position = Vector3.Lerp(
                    rectTransform.position,
                    screenPos,
                    Time.deltaTime * _followSpeed
                );
            }
            else
            {
                rectTransform.position = screenPos;
            }
        }
    }
    
    /// <summary>
    /// Met à jour la barre de vie (0-1). Tant que l'unité a du bouclier, toute la barre passe en
    /// bleu clair (comme l'orbe de vie des champions) et le montant s'affiche après les PV.
    /// </summary>
    public void UpdateHealth(float currentHP, float maxHP, float shield = 0f)
    {
        if (_healthSlider != null)
        {
            _healthSlider.value = maxHP > 0 ? Mathf.Clamp01(currentHP / maxHP) : 0f;
            ApplyFillColor(shield > 0f ? ShieldColor : _baseColor);
        }

        // Met à jour le texte (optionnel)
        if (_healthText != null)
        {
            _healthText.text = $"{(int)currentHP}/{(int)maxHP}";
        }

        UpdateShieldText(shield);
    }

    // Montant du bouclier sous la barre (le texte des PV est posé sur la barre : illisible sur le bleu)
    private TextMeshProUGUI _shieldText;

    private void UpdateShieldText(float shield)
    {
        if (_shieldText == null)
        {
            if (shield <= 0f || _healthSlider == null) return;

            var go = new GameObject("ShieldText", typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_healthSlider.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -2f);
            rt.sizeDelta = new Vector2(160f, 24f);

            _shieldText = go.GetComponent<TextMeshProUGUI>();
            if (_healthText != null)
            {
                _shieldText.font = _healthText.font;
                _shieldText.fontSize = _healthText.fontSize;
            }
            _shieldText.color = ShieldColor;
            _shieldText.fontStyle = FontStyles.Bold;
            _shieldText.alignment = TextAlignmentOptions.Top;
            _shieldText.textWrappingMode = TextWrappingModes.NoWrap;
            _shieldText.raycastTarget = false;
        }

        _shieldText.gameObject.SetActive(shield > 0f);
        _shieldText.text = $"Bouclier {(int)shield}";
    }

    // Couleur du bouclier : celle de la palette unique des icônes (CodexCardVisual.ChipColor)
    private static Color ShieldColor => CodexCardVisual.ChipColor(ChipKind.Shield);

    // Couleur de la barre sans bouclier (SetColor : rouge pour les monstres, celle de l'invocation…)
    private Color _baseColor = Color.red;

    /// <summary>
    /// Change la couleur de la barre (sans bouclier)
    /// </summary>
    public void SetColor(Color color)
    {
        _baseColor = color;
        ApplyFillColor(color);
    }

    private void ApplyFillColor(Color color)
    {
        if (_healthSlider == null || _healthSlider.fillRect == null) return;

        Image fillImage = _healthSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
}