using UnityEngine;

/// <summary>
/// Données des émotions spécifiques à chaque famille
/// Définit les 3 états émotionnels : Positif (Tank) ↔ Neutre ↔ Négatif (DPS)
/// </summary>
[CreateAssetMenu(fileName = "NewFamilyEmotion", menuName = "Champion/Family Emotion Data")]
public class FamilyEmotionData : ScriptableObject
{
    [Header("Identité de la Famille")]
    [Tooltip("Type de famille concernée")]
    public CardFamillyType familyType = CardFamillyType.None;

    [Header("Noms des États Émotionnels")]
    [Tooltip("Nom de l'émotion positive (Gauche) - Ex: Contrariété pour Déchaînés")]
    public string positiveEmotionName = "Émotion Positive";

    [Tooltip("Nom de l'émotion neutre (Centre) - Ex: Colère pour Déchaînés")]
    public string neutralEmotionName = "Émotion Neutre";

    [Tooltip("Nom de l'émotion négative (Droite) - Ex: Rage pour Déchaînés")]
    public string negativeEmotionName = "Émotion Négative";

    /// <summary>
    /// Retourne le nom de l'émotion selon l'état de transformation
    /// </summary>
    public string GetEmotionName(EmotionSystem.TransformationState state)
    {
        switch (state)
        {
            case EmotionSystem.TransformationState.Positive:
                return positiveEmotionName;
            case EmotionSystem.TransformationState.Neutral:
                return neutralEmotionName;
            case EmotionSystem.TransformationState.Negative:
                return negativeEmotionName;
            default:
                return neutralEmotionName;
        }
    }

    /// <summary>
    /// Retourne le nom de l'émotion selon la valeur de la jauge
    /// </summary>
    public string GetEmotionNameFromValue(float emotionValue, float positiveThreshold, float negativeThreshold)
    {
        if (emotionValue >= positiveThreshold)
            return positiveEmotionName;
        else if (emotionValue <= negativeThreshold)
            return negativeEmotionName;
        else
            return neutralEmotionName;
    }
}
