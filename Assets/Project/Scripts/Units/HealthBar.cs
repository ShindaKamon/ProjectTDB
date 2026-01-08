using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Transform followTarget;  // Le personnage à suivre

    [Header("Optional Text")]
    public TextMeshProUGUI healthText;  // Support pour TextMeshPro
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);  // Offset au-dessus de la tête
    public bool smoothFollow = true;
    public float followSpeed = 10f;
    
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
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 1;
            healthSlider.direction = Slider.Direction.LeftToRight;
            healthSlider.transition = Selectable.Transition.None;
            healthSlider.interactable = false;
        }
    }
    
    void LateUpdate()
    {
        if (followTarget == null || mainCamera == null) return;

        // Position world du target + offset
        Vector3 worldPos = followTarget.position + offset;

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
            else if (smoothFollow)
            {
                rectTransform.position = Vector3.Lerp(
                    rectTransform.position,
                    screenPos,
                    Time.deltaTime * followSpeed
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
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp01(currentHP / maxHP);
        }

        // Met à jour le texte (optionnel)
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHP}/{(int)maxHP}";
        }
    }
    
    /// <summary>
    /// Change la couleur de la barre
    /// </summary>
    public void SetColor(Color color)
    {
        if (healthSlider != null)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }
    }
}