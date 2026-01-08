using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor; // Couleurs de base et décalée pour l'alternance.
    [SerializeField] private Renderer _renderer; // Référence au Renderer de la tuile.

    // Méthode pour initialiser la couleur de la tuile.
    public void Init(bool isOffset)
    {
        // Définit la couleur de la tuile en fonction de son positionnement (offset ou non).
        _renderer.material.color = isOffset ? _offsetColor : _baseColor;
    }

    // Méthode pour changer la couleur de la tuile (par exemple, pour indiquer une zone de déplacement).
    public void SetColor(Color color)
    {
        _renderer.material.color = color;
    }

    // Méthode pour réinitialiser la couleur de la tuile à sa couleur de base.
    public void ResetColor(bool isOffset)
    {
        _renderer.material.color = isOffset ? _offsetColor : _baseColor;
    }
}
