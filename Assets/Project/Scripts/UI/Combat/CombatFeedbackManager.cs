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
    // ========== CONFIGURATION ==========

    [Header("Prefabs")]
    [SerializeField] private GameObject _damageNumberPrefab;

    [Header("Canvas")]
    [SerializeField] private Canvas _damageNumberCanvas;

    [Header("Shake Settings")]
    [SerializeField] private float _damageShakeDuration = 0.2f;
    [SerializeField] private float _damageShakeIntensity = 0.15f;

    [Header("Flash Settings")]
    [SerializeField] private float _damageFlashDuration = 0.15f;
    [SerializeField] private Color _damageFlashColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _healFlashColor = new Color(0.3f, 1f, 0.3f, 0.5f);

    [Header("Offset")]
    [SerializeField] private Vector3 _damageNumberOffset = new Vector3(0, 2f, 0); // Offset au-dessus de l'unité


    [Header("Écho d'invocation (Miroir fraternel)")]
    [SerializeField] private Vector3 _echoNumberExtraOffset = new Vector3(0f, 0.5f, 0f); // au-dessus du chiffre du lanceur, pas vers une case voisine
    [Header("Bonus / malus")]
    [Tooltip("Écart vertical entre les textes de bonus/malus apparus en même temps sur une unité (au-dessus du chiffre de dégâts)")]
    [SerializeField] private float _effectTextSpacing = 0.5f;

    // ========== ÉTAT ==========

    private Transform _damageNumberParent;

    // Textes de bonus/malus déjà affichés ce frame, par unité (pour les empiler)
    private readonly System.Collections.Generic.Dictionary<Unit, int> _effectTextsThisFrame = new System.Collections.Generic.Dictionary<Unit, int>();
    private int _effectTextsFrame = -1;

    // ========== INITIALISATION ==========

    void Awake()
    {
        // Trouve le canvas automatiquement si non assigné
        if (_damageNumberCanvas == null)
        {
            _damageNumberCanvas = ComponentLocator.FindSingleObjectOfType<Canvas>("CombatFeedbackManager: Recherche Canvas");
            if (_damageNumberCanvas != null)
            {
                GameLog.Log("CombatFeedbackManager: Canvas trouvé automatiquement");
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
        EventBus.Subscribe<UnitEffectAppliedEvent>(OnUnitEffectApplied);
    }

    void OnDisable()
    {
        // Se désabonne
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
        EventBus.Unsubscribe<UnitEffectAppliedEvent>(OnUnitEffectApplied);
    }


    // ========== EVENT HANDLERS ==========

    /// <summary>
    /// Appelé quand un bonus ou un malus est appliqué : texte flottant, empilé au-dessus du
    /// chiffre de dégâts si plusieurs effets tombent en même temps sur la même unité
    /// </summary>
    private void OnUnitEffectApplied(UnitEffectAppliedEvent evt)
    {
        if (evt.Target == null) return;

        if (_effectTextsFrame != Time.frameCount)
        {
            _effectTextsFrame = Time.frameCount;
            _effectTextsThisFrame.Clear();
        }
        _effectTextsThisFrame.TryGetValue(evt.Target, out int index);
        _effectTextsThisFrame[evt.Target] = index + 1;

        var (text, kind) = DescribeEffect(evt.Effect, evt.Amount);
        Vector3 offset = Vector3.up * _effectTextSpacing * (index + 1);
        ShowCustomText(text, CodexCardVisual.ChipColor(kind), evt.Target.transform.position + offset);
    }

    /// <summary>
    /// Texte et famille de couleur d'un bonus/malus, ex. « +5 bouclier », « -1 PA »
    /// </summary>
    public static (string text, ChipKind kind) DescribeEffect(UnitEffect effect, int amount)
    {
        string signed = amount >= 0 ? "+" + amount : amount.ToString();
        return effect switch
        {
            UnitEffect.Shield => (signed + " bouclier", ChipKind.Shield),
            UnitEffect.ReactiveShield => (signed + " bouclier réactif", ChipKind.Shield),
            UnitEffect.NextAttackBonus => (signed + " prochaine attaque", ChipKind.Damage),
            UnitEffect.Attack => (signed + " ATQ", ChipKind.Damage),
            UnitEffect.Armor => (signed + " armure", ChipKind.Defense),
            UnitEffect.MagicResistance => (signed + " résistance magique", ChipKind.Defense),
            UnitEffect.ActionPoints => (signed + " PA", ChipKind.ActionPoints),
            UnitEffect.MovementPoints => (amount == -int.MaxValue ? "-tous les PM" : signed + " PM", ChipKind.MovementPoints),
            UnitEffect.DamageTakenPercent => (signed + "% dégâts subis", ChipKind.Shield),
            UnitEffect.NextAttackPercent => (signed + "% prochaine attaque", ChipKind.Damage),
            UnitEffect.PmImmune => ("Tenace", ChipKind.Mute),
            UnitEffect.CardCancelled => ("Carte annulée", ChipKind.Mute),
            _ => (signed, ChipKind.Mute)
        };
    }

    /// <summary>
    /// Appelé quand une unité prend des dégâts
    /// </summary>
    private void OnUnitDamaged(UnitDamagedEvent evt)
    {
        if (evt.Target == null) return;

        // Écho d'une invocation (Miroir fraternel, infligé un peu après le coup du lanceur) :
        // l'invocation se tourne vers la cible, le chiffre apparaît au-dessus de celui du lanceur
        Vector3 numberPosition = evt.Target.transform.position;
        if (evt.Source is SummonUnit summon)
        {
            FaceTarget(summon, evt.Target);
            numberPosition += _echoNumberExtraOffset;
        }

        // 1. Affiche le nombre de dégâts
        ShowDamageNumber(evt.Damage, DamageNumberPopup.PopupType.Damage, numberPosition);

        // 2. Shake de l'unité
        StartCoroutine(ShakeUnit(evt.Target.transform, _damageShakeDuration, _damageShakeIntensity));

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
            GameLog.LogWarning("CombatFeedbackManager: _damageNumberPrefab n'est pas assigné!");
            return;
        }

        if (_damageNumberParent == null)
        {
            GameLog.LogWarning("CombatFeedbackManager: _damageNumberParent est null!");
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
    /// <summary>
    /// Écho d'une invocation : elle se tourne vers l'ennemi qu'elle frappe (4 directions, sans bouger).
    /// Purement visuel : les dégâts sont déjà appliqués et affichés sur la cible.
    /// </summary>
    private static void FaceTarget(Unit summon, Unit target)
    {
        Vector2Int dir = GridGeometry.SnapDirection(summon.GetCurrentGridPos(), target.GetCurrentGridPos());
        if (dir != Vector2Int.zero)
            summon.transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));
    }

    private IEnumerator ShakeUnit(Transform target, float duration, float intensity)
    {
        if (target == null) yield break;

        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break; // unité détruite pendant le shake (coup fatal)

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
        if (target != null) target.localPosition = originalPosition;
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

            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
        else if (renderer != null)
        {
            // Effet flash avec Material (3D)
            Material material = renderer.material;
            Color originalColor = material.color;

            material.color = flashColor;

            yield return new WaitForSeconds(duration);

            if (material != null) material.color = originalColor;
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
