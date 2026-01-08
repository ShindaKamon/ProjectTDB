using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système de gestion des émotions et transformations des champions
/// Gère la jauge émotionnelle : Contrariété (positif/gauche/Tank) ↔ Colère (neutre) ↔ Rage (négatif/droite/DPS)
/// </summary>
public class EmotionSystem : MonoBehaviour
{
    [Header("Configuration des Émotions")]
    [Tooltip("Données des émotions de la famille (définit les noms des 3 états)")]
    [SerializeField] private FamilyEmotionData familyEmotionData;

    [Header("Configuration des Transformations")]
    [Tooltip("Transformation positive (Tank/Gauche)")]
    [SerializeField] private TransformationData positiveTransformation;
    [Tooltip("Transformation négative (DPS/Droite)")]
    [SerializeField] private TransformationData negativeTransformation;

    [Header("Seuils de Transformation")]
    [Tooltip("Seuil pour déclencher la transformation positive (ex: 100)")]
    [SerializeField] private float positiveThreshold = 100f;
    [Tooltip("Seuil pour déclencher la transformation négative (ex: -100)")]
    [SerializeField] private float negativeThreshold = -100f;

    [Header("État Actuel")]
    [Tooltip("Valeur actuelle de l'émotion (-100 à +100)")]
    [SerializeField] private float currentEmotion = 0f;
    [Tooltip("État de transformation actuel")]
    [SerializeField] private TransformationState currentState = TransformationState.Neutral;

    // État de transformation
    public enum TransformationState
    {
        Neutral,        // État neutre (Colère pour Ilya)
        Positive,       // Transformation positive (Contrariété/Tank)
        Negative        // Transformation négative (Rage/DPS)
    }

    // Références aux composants
    private Unit unit;
    private IlyaUnit ilyaUnit;

    // Modificateurs de stats appliqués
    private int appliedMaxHealthMod = 0;
    private int appliedMaxPAMod = 0;
    private int appliedDefensePMod = 0;
    private int appliedDefenseMMod = 0;
    private int appliedMovementMod = 0;

    // Événements
    public System.Action<float, float> OnEmotionChanged; // (current, max)
    public System.Action<TransformationState> OnTransformationChanged;

    void Awake()
    {
        unit = GetComponent<Unit>();
        ilyaUnit = GetComponent<IlyaUnit>();
    }

    /// <summary>
    /// Modifie la valeur de l'émotion
    /// </summary>
    public void ModifyEmotion(float amount)
    {
        float oldEmotion = currentEmotion;
        currentEmotion = Mathf.Clamp(currentEmotion + amount, negativeThreshold, positiveThreshold);

        string emotionName = GetCurrentEmotionName();
        Debug.Log($"{unit.name} émotion modifiée de {oldEmotion:F1} à {currentEmotion:F1} ({amount:+F1;-F1;0}) - État: {emotionName}");

        OnEmotionChanged?.Invoke(currentEmotion, positiveThreshold);

        // Vérifier si on doit déclencher une transformation
        CheckForTransformation();
    }

    /// <summary>
    /// Vérifie si une transformation doit être déclenchée
    /// </summary>
    private void CheckForTransformation()
    {
        // Si on est déjà transformé, on ne peut pas se transformer à nouveau
        if (currentState != TransformationState.Neutral)
        {
            return;
        }

        // Transformation positive (Contrariété/Tank)
        if (currentEmotion >= positiveThreshold && positiveTransformation != null)
        {
            ApplyTransformation(positiveTransformation, TransformationState.Positive);
        }
        // Transformation négative (Rage/DPS)
        else if (currentEmotion <= negativeThreshold && negativeTransformation != null)
        {
            ApplyTransformation(negativeTransformation, TransformationState.Negative);
        }
    }

    /// <summary>
    /// Applique une transformation
    /// </summary>
    private void ApplyTransformation(TransformationData transformation, TransformationState newState)
    {
        if (transformation == null) return;

        Debug.Log($"🔥 {unit.name} se transforme en {transformation.transformationName}!");

        currentState = newState;
        OnTransformationChanged?.Invoke(currentState);

        // Appliquer les modificateurs de stats
        ApplyStatModifiers(transformation);

        // Appliquer l'effet visuel
        ApplyVisualEffect(transformation);
    }

    /// <summary>
    /// Applique les modificateurs de stats d'une transformation
    /// </summary>
    private void ApplyStatModifiers(TransformationData transformation)
    {
        // Appliquer les modificateurs de santé
        if (transformation.maxHealthModifier != 0)
        {
            appliedMaxHealthMod = transformation.maxHealthModifier;
            // TODO: Appliquer à Unit.maxHealth
            Debug.Log($"  → Max Health: {transformation.maxHealthModifier:+0;-0;0}");
        }

        // Appliquer les modificateurs de PA (pour IlyaUnit)
        if (transformation.maxPAModifier != 0 && ilyaUnit != null)
        {
            appliedMaxPAMod = transformation.maxPAModifier;
            ilyaUnit.SetMaxPA(ilyaUnit.GetMaxPA() + appliedMaxPAMod);
            Debug.Log($"  → Max PA: {transformation.maxPAModifier:+0;-0;0}");
        }

        // Appliquer les modificateurs de défense physique (pour IlyaUnit)
        if (transformation.defensePModifier != 0 && ilyaUnit != null)
        {
            appliedDefensePMod = transformation.defensePModifier;
            ilyaUnit.SetDefenseP(ilyaUnit.GetDefenseP() + appliedDefensePMod);
            Debug.Log($"  → Défense P: {transformation.defensePModifier:+0;-0;0}");
        }

        // Appliquer les modificateurs de défense magique (pour IlyaUnit)
        if (transformation.defenseMModifier != 0 && ilyaUnit != null)
        {
            appliedDefenseMMod = transformation.defenseMModifier;
            ilyaUnit.SetDefenseM(ilyaUnit.GetDefenseM() + appliedDefenseMMod);
            Debug.Log($"  → Défense M: {transformation.defenseMModifier:+0;-0;0}");
        }

        // Appliquer les modificateurs de mouvement
        if (transformation.movementModifier != 0)
        {
            appliedMovementMod = transformation.movementModifier;
            unit.SetMovementRange(unit.GetMovementRange() + appliedMovementMod);
            Debug.Log($"  → Mouvement: {transformation.movementModifier:+0;-0;0}");
        }

        // Afficher les effets passifs (conservés pour référence)
        if (transformation.bonusDamage != 0)
        {
            Debug.Log($"  → Dégâts bonus: +{transformation.bonusDamage}");
        }
        if (transformation.lifesteal > 0)
        {
            Debug.Log($"  → Vol de vie: {transformation.lifesteal * 100}%");
        }
        if (transformation.hpRegenPerTurn > 0)
        {
            Debug.Log($"  → Régénération par tour: +{transformation.hpRegenPerTurn}");
        }
    }


    /// <summary>
    /// Applique l'effet visuel d'une transformation
    /// </summary>
    private void ApplyVisualEffect(TransformationData transformation)
    {
        // TODO: Implémenter les effets visuels
        // - Changer la couleur de l'aura du champion (glowColor)
        // - Instancier le préfab d'effet visuel si disponible
        if (transformation.visualEffectPrefab != null)
        {
            // Instantiate(transformation.visualEffectPrefab, transform);
            Debug.Log($"  → Effet visuel: {transformation.visualEffectPrefab.name}");
        }
    }

    /// <summary>
    /// Retire la transformation actuelle et retourne à l'état neutre
    /// </summary>
    public void RemoveTransformation()
    {
        if (currentState == TransformationState.Neutral)
        {
            return;
        }

        Debug.Log($"✨ {unit.name} retourne à l'état neutre");

        // Retirer les modificateurs de stats
        RemoveStatModifiers();

        // Retourner à l'état neutre
        currentState = TransformationState.Neutral;
        currentEmotion = 0f;

        OnTransformationChanged?.Invoke(currentState);
        OnEmotionChanged?.Invoke(currentEmotion, positiveThreshold);
    }

    /// <summary>
    /// Retire les modificateurs de stats appliqués
    /// </summary>
    private void RemoveStatModifiers()
    {
        // Retirer les modificateurs de PA
        if (appliedMaxPAMod != 0 && ilyaUnit != null)
        {
            ilyaUnit.SetMaxPA(ilyaUnit.GetMaxPA() - appliedMaxPAMod);
            appliedMaxPAMod = 0;
        }

        // Retirer les modificateurs de défense physique
        if (appliedDefensePMod != 0 && ilyaUnit != null)
        {
            ilyaUnit.SetDefenseP(ilyaUnit.GetDefenseP() - appliedDefensePMod);
            appliedDefensePMod = 0;
        }

        // Retirer les modificateurs de défense magique
        if (appliedDefenseMMod != 0 && ilyaUnit != null)
        {
            ilyaUnit.SetDefenseM(ilyaUnit.GetDefenseM() - appliedDefenseMMod);
            appliedDefenseMMod = 0;
        }

        // Retirer les modificateurs de mouvement
        if (appliedMovementMod != 0)
        {
            unit.SetMovementRange(unit.GetMovementRange() - appliedMovementMod);
            appliedMovementMod = 0;
        }

        // Réinitialiser les autres modificateurs
        appliedMaxHealthMod = 0;
    }


    /// <summary>
    /// Applique les effets de début de tour
    /// Note: Pour l'instant, aucun effet de tour n'est appliqué (hpLossPerTurn/hpRegenPerTurn retirés)
    /// </summary>
    public void ApplyTurnEffects()
    {
        // Méthode conservée pour compatibilité avec GridManager
        // Peut être utilisée plus tard pour d'autres effets de tour
    }

    // Getters
    public float GetCurrentEmotion() => currentEmotion;
    public TransformationState GetCurrentState() => currentState;
    public TransformationData GetPositiveTransformation() => positiveTransformation;
    public TransformationData GetNegativeTransformation() => negativeTransformation;
    public bool IsTransformed() => currentState != TransformationState.Neutral;
    public float GetPositiveThreshold() => positiveThreshold;
    public float GetNegativeThreshold() => negativeThreshold;

    // Setters pour configuration via ChampionData
    public void SetPositiveTransformation(TransformationData data) => positiveTransformation = data;
    public void SetNegativeTransformation(TransformationData data) => negativeTransformation = data;
    public void SetFamilyEmotionData(FamilyEmotionData data) => familyEmotionData = data;
    public void SetThresholds(float positive, float negative)
    {
        positiveThreshold = positive;
        negativeThreshold = negative;
    }

    /// <summary>
    /// Retourne le nom de l'émotion actuelle selon la famille
    /// </summary>
    public string GetCurrentEmotionName()
    {
        if (familyEmotionData != null)
        {
            if (currentState != TransformationState.Neutral)
            {
                return familyEmotionData.GetEmotionName(currentState);
            }
            else
            {
                return familyEmotionData.GetEmotionNameFromValue(currentEmotion, positiveThreshold, negativeThreshold);
            }
        }
        return currentState.ToString();
    }

}
