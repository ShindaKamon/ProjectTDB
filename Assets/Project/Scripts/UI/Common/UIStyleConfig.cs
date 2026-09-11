using UnityEngine;

/// <summary>
/// Configuration centralisee des styles visuels de l'UI
/// Utiliser un seul asset pour garantir la coherence visuelle
/// </summary>
[CreateAssetMenu(fileName = "UIStyleConfig", menuName = "UI/Style Config")]
public class UIStyleConfig : ScriptableObject
{
    [Header("Couleurs principales")]
    public Color primaryColor = new Color(0.2f, 0.6f, 0.9f);      // Bleu principal
    public Color secondaryColor = new Color(0.9f, 0.4f, 0.2f);    // Orange accent
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f);  // Fond sombre
    public Color surfaceColor = new Color(0.15f, 0.15f, 0.2f);    // Surface des panneaux

    [Header("Couleurs de texte")]
    public Color textPrimary = Color.white;
    public Color textSecondary = new Color(0.7f, 0.7f, 0.7f);
    public Color textDisabled = new Color(0.4f, 0.4f, 0.4f);

    [Header("Couleurs de stats")]
    public Color healthColor = new Color(0.9f, 0.3f, 0.3f);       // Rouge pour HP
    public Color movementColor = new Color(0.4f, 0.7f, 1f);       // Bleu pour PM
    public Color actionPointsColor = new Color(1f, 0.8f, 0.2f);   // Jaune pour PA
    public Color attackColor = new Color(1f, 0.5f, 0.3f);         // Orange pour ATK
    public Color defenseColor = new Color(0.4f, 0.9f, 0.4f);      // Vert pour DEF

    [Header("Couleurs d'etat")]
    public Color selectedColor = new Color(1f, 0.84f, 0f);        // Or pour selection
    public Color hoverColor = new Color(0.6f, 0.6f, 0.6f);        // Gris pour hover
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);     // Gris fonce desactive
    public Color successColor = new Color(0.3f, 0.8f, 0.3f);      // Vert succes
    public Color errorColor = new Color(0.9f, 0.3f, 0.3f);        // Rouge erreur
    public Color warningColor = new Color(0.9f, 0.7f, 0.2f);      // Jaune avertissement

    [Header("Couleurs de cout PA")]
    public Color costLow = new Color(0.3f, 0.8f, 0.4f);           // Vert - 0-1 PA
    public Color costMedium = new Color(0.9f, 0.7f, 0.2f);        // Jaune - 2 PA
    public Color costHigh = new Color(0.9f, 0.4f, 0.2f);          // Orange - 3 PA
    public Color costVeryHigh = new Color(0.8f, 0.2f, 0.2f);      // Rouge - 4+ PA

    [Header("Bordures")]
    public Color borderNormal = new Color(0.3f, 0.3f, 0.35f);
    public Color borderHighlight = new Color(0.5f, 0.5f, 0.55f);
    public float borderWidth = 2f;
    public float borderRadius = 8f;

    [Header("Ombres")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
    public Vector2 shadowOffset = new Vector2(2f, -2f);
    public float shadowBlur = 4f;

    [Header("Animation")]
    public float hoverScale = 1.05f;
    public float clickScale = 0.95f;
    public float animationDuration = 0.15f;

    /// <summary>
    /// Retourne la couleur appropriee pour un cout PA
    /// </summary>
    public Color GetCostColor(int cost)
    {
        if (cost <= 1) return costLow;
        if (cost == 2) return costMedium;
        if (cost == 3) return costHigh;
        return costVeryHigh;
    }

    /// <summary>
    /// Retourne la couleur appropriee pour un type de stat
    /// </summary>
    public Color GetStatColor(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health: return healthColor;
            case StatType.Movement: return movementColor;
            case StatType.ActionPoints: return actionPointsColor;
            case StatType.Attack: return attackColor;
            case StatType.Defense: return defenseColor;
            default: return textPrimary;
        }
    }
}

/// <summary>
/// Types de statistiques pour le styling
/// </summary>
public enum StatType
{
    Health,
    Movement,
    ActionPoints,
    Attack,
    Defense
}
