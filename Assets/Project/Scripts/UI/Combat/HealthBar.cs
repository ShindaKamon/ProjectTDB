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
    /// Met à jour la barre de vie (0-1)
    /// </summary>
    public void UpdateHealth(float currentHP, float maxHP)
    {
        if (_healthSlider != null)
        {
            _healthSlider.value = Mathf.Clamp01(currentHP / maxHP);
        }

        // Met à jour le texte (optionnel)
        if (_healthText != null)
        {
            _healthText.text = $"{(int)currentHP}/{(int)maxHP}";
        }
    }

    /// <summary>
    /// Change la couleur de la barre
    /// </summary>
    public void SetColor(Color color)
    {
        if (_healthSlider != null)
        {
            Image fillImage = _healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }
    }
}