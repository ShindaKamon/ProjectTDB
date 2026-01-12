using UnityEngine;
using UnityEngine.UI;

public class HealthOrbController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image liquidMaskImage; // L'image avec le Fill Method "Vertical"
    public Image liquidTextureImage; // L'image de la texture de lave/eau

    [Header("Settings")]
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0.05f;

    private Material liquidMaterial;

    void Start()
    {
        // On crée une instance unique du matériau pour ne pas modifier l'original
        liquidMaterial = Instantiate(liquidTextureImage.material);
        liquidTextureImage.material = liquidMaterial;
    }

    void Update()
    {
        // 1. Animation de la texture (défilement pour l'effet liquide)
        Vector2 offset = liquidMaterial.mainTextureOffset;
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;
        liquidMaterial.mainTextureOffset = offset;
    }

    // Appelle cette fonction pour mettre à jour l'orbite
    public void UpdateHealth(float currentHealth, float maxHealth, Color emotionColor)
    {
        // 2. Mise à jour du remplissage (0 à 1)
        float fillAmount = currentHealth / maxHealth;
        liquidMaskImage.fillAmount = fillAmount;

        // 3. Changement de couleur selon l'émotion
        liquidTextureImage.color = emotionColor;
    }
}