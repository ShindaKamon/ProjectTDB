using UnityEngine;
using System.Collections;

/// <summary>
/// Gestionnaire centralisé des feedbacks visuels de combat.
/// Pattern: Singleton + Observer Pattern
/// Responsabilités:
/// - Écouter les événements de combat (UnitDamagedEvent, UnitHealedEvent)
/// - Afficher les nombres de dégâts/soins flottants
/// - Déclencher effets visuels (shake, flash, etc.)
/// - Gérer le prefab de DamageNumberPopup
/// </summary>
public class CombatFeedbackManager : MonoBehaviour
{
    // ========== SINGLETON ==========

    private static CombatFeedbackManager _instance;
    public static CombatFeedbackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CombatFeedbackManager>();
                if (_instance == null)
                {
                    Debug.LogWarning("CombatFeedbackManager: Instance non trouvée dans la scène!");
                }
            }
            return _instance;
        }
    }

    // ========== CONFIGURATION ==========

    [Header("Prefabs")]
    [SerializeField] private GameObject _damageNumberPrefab;

    [Header("Canvas")]
    [SerializeField] private Canvas _damageNumberCanvas;

    [Header("Shake Settings")]
    [SerializeField] private float _damageShakeDuration = 0.2f;
    [SerializeField] private float _damageShakeIntensity = 0.15f;
    [SerializeField] private float _criticalShakeIntensity = 0.3f;

    [Header("Flash Settings")]
    [SerializeField] private float _damageFlashDuration = 0.15f;
    [SerializeField] private Color _damageFlashColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _healFlashColor = new Color(0.3f, 1f, 0.3f, 0.5f);

    [Header("Offset")]
    [SerializeField] private Vector3 _damageNumberOffset = new Vector3(0, 2f, 0); // Offset au-dessus de l'unité

    // ========== ÉTAT ==========

    private Transform _damageNumberParent;

    // ========== INITIALISATION ==========

    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Trouve le canvas automatiquement si non assigné
        if (_damageNumberCanvas == null)
        {
            _damageNumberCanvas = ComponentLocator.FindSingleObjectOfType<Canvas>("CombatFeedbackManager: Recherche Canvas");
            if (_damageNumberCanvas != null)
            {
                Debug.Log("CombatFeedbackManager: Canvas trouvé automatiquement");
            }
            else
            {
                Debug.LogError("CombatFeedbackManager: Aucun Canvas trouvé dans la scène!");
            }
        }

        // Crée un parent pour les popups si nécessaire
        if (_damageNumberCanvas != null)
        {
            GameObject parentObj = new GameObject("DamageNumbersContainer");
            parentObj.transform.SetParent(_damageNumberCanvas.transform, false);
            _damageNumberParent = parentObj.transform;
        }
    }

    void OnEnable()
    {
        // S'abonne aux événements de combat
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Subscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
    }

    void OnDisable()
    {
        // Se désabonne
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
    }

    // ========== EVENT HANDLERS ==========

    /// <summary>
    /// Appelé quand une unité prend des dégâts
    /// </summary>
    private void OnUnitDamaged(UnitDamagedEvent evt)
    {
        if (evt.Target == null) return;

        // Détermine si c'est un coup critique (pour l'instant, détection simple)
        bool isCritical = evt.Damage > 15; // TODO: Implémenter système de critiques

        // 1. Affiche le nombre de dégâts
        ShowDamageNumber(
            evt.Damage,
            isCritical ? DamageNumberPopup.PopupType.Critical : DamageNumberPopup.PopupType.Damage,
            evt.Target.transform.position
        );

        // 2. Shake de l'unité
        float shakeIntensity = isCritical ? _criticalShakeIntensity : _damageShakeIntensity;
        StartCoroutine(ShakeUnit(evt.Target.transform, _damageShakeDuration, shakeIntensity));

        // 3. Flash rouge (optionnel - nécessite SpriteRenderer ou Material)
        StartCoroutine(FlashUnit(evt.Target.transform, _damageFlashColor, _damageFlashDuration));
    }

    /// <summary>
    /// Appelé quand une unité est soignée
    /// </summary>
    private void OnUnitHealed(UnitHealedEvent evt)
    {
        if (evt.Target == null) return;

        // 1. Affiche le nombre de soins
        ShowDamageNumber(
            evt.HealAmount,
            DamageNumberPopup.PopupType.Heal,
            evt.Target.transform.position
        );

        // 2. Flash vert (optionnel)
        StartCoroutine(FlashUnit(evt.Target.transform, _healFlashColor, _damageFlashDuration));
    }

    /// <summary>
    /// Appelé quand une unité meurt
    /// </summary>
    private void OnUnitDied(UnitDiedEvent evt)
    {
        if (evt.DeadUnit == null) return;

        // Affiche "MORT" ou autre texte
        ShowCustomText("MORT", new Color(0.5f, 0.5f, 0.5f), evt.DeadUnit.transform.position);

        // TODO: Ajouter animation de mort (fade out, fall down, etc.)
    }

    // ========== AFFICHAGE DAMAGE NUMBERS ==========

    /// <summary>
    /// Affiche un nombre de dégâts/soins flottant
    /// </summary>
    private void ShowDamageNumber(int value, DamageNumberPopup.PopupType type, Vector3 worldPosition)
    {
        if (_damageNumberPrefab == null)
        {
            Debug.LogWarning("CombatFeedbackManager: _damageNumberPrefab n'est pas assigné!");
            return;
        }

        if (_damageNumberParent == null)
        {
            Debug.LogWarning("CombatFeedbackManager: _damageNumberParent est null!");
            return;
        }

        // Position avec offset
        Vector3 spawnPosition = worldPosition + _damageNumberOffset;

        // Crée le popup
        DamageNumberPopup.Create(_damageNumberPrefab, value, type, spawnPosition, _damageNumberParent);
    }

    /// <summary>
    /// Affiche un texte personnalisé
    /// </summary>
    private void ShowCustomText(string text, Color color, Vector3 worldPosition)
    {
        if (_damageNumberPrefab == null || _damageNumberParent == null) return;

        Vector3 spawnPosition = worldPosition + _damageNumberOffset;

        GameObject popupObj = Instantiate(_damageNumberPrefab, _damageNumberParent);
        DamageNumberPopup popup = popupObj.GetComponent<DamageNumberPopup>();

        if (popup != null)
        {
            popup.ShowCustomText(text, color, spawnPosition);
        }
    }

    // ========== EFFETS VISUELS ==========

    /// <summary>
    /// Fait trembler une unité (shake effect)
    /// </summary>
    private IEnumerator ShakeUnit(Transform target, float duration, float intensity)
    {
        if (target == null) yield break;

        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Décroissance de l'intensité
            float currentIntensity = intensity * (1f - elapsed / duration);

            // Position aléatoire autour de l'origine
            float offsetX = Random.Range(-currentIntensity, currentIntensity);
            float offsetZ = Random.Range(-currentIntensity, currentIntensity);

            target.localPosition = originalPosition + new Vector3(offsetX, 0, offsetZ);

            yield return null;
        }

        // Remet à la position originale
        target.localPosition = originalPosition;
    }

    /// <summary>
    /// Flash de couleur sur une unité (nécessite SpriteRenderer ou Material)
    /// </summary>
    private IEnumerator FlashUnit(Transform target, Color flashColor, float duration)
    {
        if (target == null) yield break;

        // Cherche un SpriteRenderer (pour 2D) ou un Renderer (pour 3D)
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        Renderer renderer = target.GetComponentInChildren<Renderer>();

        if (spriteRenderer != null)
        {
            // Effet flash avec SpriteRenderer
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = flashColor;

            yield return new WaitForSeconds(duration);

            spriteRenderer.color = originalColor;
        }
        else if (renderer != null)
        {
            // Effet flash avec Material (3D)
            Material material = renderer.material;
            Color originalColor = material.color;

            material.color = flashColor;

            yield return new WaitForSeconds(duration);

            material.color = originalColor;
        }
        else
        {
            // Pas de renderer trouvé, skip le flash
            yield break;
        }
    }

    // ========== API PUBLIQUE ==========

    /// <summary>
    /// Affiche manuellement un nombre de dégâts
    /// </summary>
    public void ShowDamage(int damage, Vector3 worldPosition)
    {
        ShowDamageNumber(damage, DamageNumberPopup.PopupType.Damage, worldPosition);
    }

    /// <summary>
    /// Affiche manuellement un nombre de soins
    /// </summary>
    public void ShowHeal(int healAmount, Vector3 worldPosition)
    {
        ShowDamageNumber(healAmount, DamageNumberPopup.PopupType.Heal, worldPosition);
    }

    /// <summary>
    /// Affiche "IMMUNE" pour un coup bloqué
    /// </summary>
    public void ShowImmune(Vector3 worldPosition)
    {
        if (_damageNumberPrefab == null || _damageNumberParent == null) return;

        Vector3 spawnPosition = worldPosition + _damageNumberOffset;

        GameObject popupObj = Instantiate(_damageNumberPrefab, _damageNumberParent);
        DamageNumberPopup popup = popupObj.GetComponent<DamageNumberPopup>();

        if (popup != null)
        {
            popup.Show(0, DamageNumberPopup.PopupType.Immune, spawnPosition);
        }
    }

    /// <summary>
    /// Shake manuel d'une unité
    /// </summary>
    public void ShakeTransform(Transform target, float duration = -1f, float intensity = -1f)
    {
        if (target == null) return;

        float shakeDuration = duration > 0 ? duration : _damageShakeDuration;
        float shakeIntensity = intensity > 0 ? intensity : _damageShakeIntensity;

        StartCoroutine(ShakeUnit(target, shakeDuration, shakeIntensity));
    }
}
