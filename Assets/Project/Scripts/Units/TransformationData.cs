using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Données pour les transformations des champions (ex: Rage, Contrariété pour Ilya)
/// </summary>
[CreateAssetMenu(fileName = "NewTransformation", menuName = "Champion/Transformation Data")]
public class TransformationData : ScriptableObject
{
    [Header("Identité de la Transformation")]
    public string transformationName = "Nouvelle Transformation";
    [TextArea(2, 4)]
    public string description = "Description de la transformation.";
    public Sprite icon; // Icône pour l'UI

    [Header("Modificateurs de Stats")]
    [Tooltip("Modificateur de PV max (additif)")]
    public int maxHealthModifier = 0;
    [Tooltip("Modificateur de PA max (additif)")]
    public int maxPAModifier = 0;
    [Tooltip("Modificateur de défense physique (additif)")]
    public int defensePModifier = 0;
    [Tooltip("Modificateur de défense magique (additif)")]
    public int defenseMModifier = 0;
    [Tooltip("Modificateur de mouvement (additif)")]
    public int movementModifier = 0;

    [Header("Effets Passifs")]
    [Tooltip("Dégâts bonus sur les attaques (additif)")]
    public int bonusDamage = 0;
    [Tooltip("Pourcentage de vol de vie (0.0 à 1.0)")]
    [Range(0f, 1f)]
    public float lifesteal = 0f;
    [Tooltip("Régénération de PV par tour")]
    public int hpRegenPerTurn = 0;

    [Header("Visuel")]
    public Color glowColor = Color.white;
    [Tooltip("Préfab d'effet visuel à spawner (optionnel)")]
    public GameObject visualEffectPrefab;
}
