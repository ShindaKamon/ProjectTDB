using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class HealthOrbController : MonoBehaviour
{
    [Header("UI Elements")]
    [FormerlySerializedAs("liquidMaskImage")] [SerializeField] private Image _liquidMaskImage; // L'image avec le Fill Method "Vertical"
    [FormerlySerializedAs("liquidTextureImage")] [SerializeField] private Image _liquidTextureImage; // L'image de la texture de lave/eau

    [Header("Settings")]
    [FormerlySerializedAs("scrollSpeedX")] [SerializeField] private float _scrollSpeedX = 0.1f;
    [FormerlySerializedAs("scrollSpeedY")] [SerializeField] private float _scrollSpeedY = 0.05f;

    private Material _liquidMaterial;

    void Start()
    {
        // On crée une instance unique du matériau pour ne pas modifier l'original
        _liquidMaterial = Instantiate(_liquidTextureImage.material);
        _liquidTextureImage.material = _liquidMaterial;
    }

    void Update()
    {
        // 1. Animation de la texture (défilement pour l'effet liquide)
        Vector2 offset = _liquidMaterial.mainTextureOffset;
        offset.x += _scrollSpeedX * Time.deltaTime;
        offset.y += _scrollSpeedY * Time.deltaTime;
        _liquidMaterial.mainTextureOffset = offset;
    }

    // Appelle cette fonction pour mettre à jour l'orbite
    public void UpdateHealth(float currentHealth, float maxHealth, Color emotionColor)
    {
        // 2. Mise à jour du remplissage (0 à 1)
        float fillAmount = currentHealth / maxHealth;
        _liquidMaskImage.fillAmount = fillAmount;

        // 3. Changement de couleur selon l'émotion
        _liquidTextureImage.color = emotionColor;
    }
}