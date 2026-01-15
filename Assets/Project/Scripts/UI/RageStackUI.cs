using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Affiche le stock de Rage sous forme d'icônes dans des slots pré-définis.
/// Supporte deux modes :
/// - Mode Slots : Utilise des emplacements pré-placés dans l'éditeur (recommandé)
/// - Mode Dynamique : Génère les icônes automatiquement
/// </summary>
public class RageStackUI : MonoBehaviour
{
    [Header("Mode d'affichage")]
    [Tooltip("Si true, utilise les slots pré-définis. Si false, génère les icônes dynamiquement.")]
    [SerializeField] private bool _usePreDefinedSlots = true;

    [Header("Slots pré-définis (Mode Slots)")]
    [Tooltip("Les 5 emplacements d'icônes de Rage placés dans l'éditeur")]
    [SerializeField] private List<Image> _rageSlots = new List<Image>();

    [Header("Apparence des slots")]
    [Tooltip("Couleur quand rempli (Rage active)")]
    [SerializeField] private Color _filledColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Rouge
    [Tooltip("Couleur quand vide")]
    [SerializeField] private Color _emptyColor = Color.white; // Blanc

    [Header("Configuration générale")]
    [SerializeField] private int _maxVisibleIcons = 5;
    [SerializeField] private float _popAnimationScale = 1.3f;
    [SerializeField] private float _animationDuration = 0.15f;

    [Header("Overflow (au-delà du max)")]
    [SerializeField] private TextMeshProUGUI _overflowText;
    [SerializeField] private GameObject _overflowContainer;

    [Header("Mode Dynamique (si pas de slots)")]
    [SerializeField] private Sprite _rageIconSprite;
    [SerializeField] private GameObject _iconPrefab;
    [SerializeField] private Transform _iconsContainer;
    [SerializeField] private float _iconSpacing = 35f;
    [SerializeField] private Color _iconColor = new Color(1f, 0.3f, 0f);

    private List<GameObject> _dynamicIcons = new List<GameObject>();
    private int _currentRageCount = 0;
    private IlyaUnit _trackedPlayer;

    void Start()
    {
        // Auto-détecte le mode si pas de slots assignés
        if (_rageSlots.Count == 0)
        {
            _usePreDefinedSlots = false;
        }

        // Mode slots : initialise tous les slots comme vides
        if (_usePreDefinedSlots)
        {
            InitializeSlots();
        }
        else
        {
            // Mode dynamique : prépare le prefab
            if (_iconPrefab == null)
            {
                CreateDefaultIconPrefab();
            }
            if (_iconsContainer == null)
            {
                _iconsContainer = transform;
            }
        }

        // Cache l'overflow au départ
        if (_overflowContainer != null)
        {
            _overflowContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Initialise tous les slots en état vide
    /// </summary>
    private void InitializeSlots()
    {
        foreach (var slot in _rageSlots)
        {
            if (slot != null)
            {
                SetSlotEmpty(slot);
            }
        }
    }

    /// <summary>
    /// Connecte le système à un joueur pour écouter les changements de Rage
    /// </summary>
    public void SetPlayer(IlyaUnit player)
    {
        // Déconnexion de l'ancien joueur
        if (_trackedPlayer != null)
        {
            _trackedPlayer.OnRageStockChanged -= OnRageChanged;
        }

        _trackedPlayer = player;

        if (_trackedPlayer != null)
        {
            _trackedPlayer.OnRageStockChanged += OnRageChanged;
            // Mise à jour initiale
            UpdateDisplay(_trackedPlayer.GetRageStock());
        }
        else
        {
            UpdateDisplay(0);
        }
    }

    private void OnRageChanged(int newAmount)
    {
        int previousAmount = _currentRageCount;
        UpdateDisplay(newAmount);

        // Animation si on gagne de la rage
        if (newAmount > previousAmount)
        {
            AnimateNewRage(previousAmount, newAmount);
        }
    }

    /// <summary>
    /// Met à jour l'affichage selon le mode choisi
    /// </summary>
    public void UpdateDisplay(int rageCount)
    {
        _currentRageCount = rageCount;

        if (_usePreDefinedSlots)
        {
            UpdateSlotsDisplay(rageCount);
        }
        else
        {
            UpdateDynamicDisplay(rageCount);
        }

        // Gère l'overflow (+X) si au-delà du max
        UpdateOverflow(rageCount);
    }

    /// <summary>
    /// Mode Slots : Met à jour l'état de chaque slot
    /// </summary>
    private void UpdateSlotsDisplay(int rageCount)
    {
        for (int i = 0; i < _rageSlots.Count; i++)
        {
            if (_rageSlots[i] == null) continue;

            if (i < rageCount)
            {
                SetSlotFilled(_rageSlots[i]);
            }
            else
            {
                SetSlotEmpty(_rageSlots[i]);
            }
        }
    }

    private void SetSlotFilled(Image slot)
    {
        slot.color = _filledColor;
    }

    private void SetSlotEmpty(Image slot)
    {
        slot.color = _emptyColor;
    }

    /// <summary>
    /// Mode Dynamique : Génère les icônes au runtime
    /// </summary>
    private void UpdateDynamicDisplay(int rageCount)
    {
        int iconsToShow = Mathf.Min(rageCount, _maxVisibleIcons);

        // Ajoute des icônes si nécessaire
        while (_dynamicIcons.Count < iconsToShow)
        {
            CreateDynamicIcon();
        }

        // Active/désactive les icônes
        for (int i = 0; i < _dynamicIcons.Count; i++)
        {
            _dynamicIcons[i].SetActive(i < iconsToShow);
        }

        RepositionDynamicIcons();
    }

    private void CreateDynamicIcon()
    {
        GameObject icon;

        if (_iconPrefab != null)
        {
            icon = Instantiate(_iconPrefab, _iconsContainer);
            icon.SetActive(true);
        }
        else
        {
            icon = new GameObject($"RageIcon_{_dynamicIcons.Count}");
            icon.transform.SetParent(_iconsContainer, false);

            var image = icon.AddComponent<Image>();
            if (_rageIconSprite != null)
            {
                image.sprite = _rageIconSprite;
            }
            image.color = _iconColor;

            var rect = icon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30, 30);
        }

        _dynamicIcons.Add(icon);
    }

    private void RepositionDynamicIcons()
    {
        int activeCount = 0;
        foreach (var icon in _dynamicIcons)
        {
            if (icon.activeSelf) activeCount++;
        }

        float totalWidth = (activeCount - 1) * _iconSpacing;
        float startX = -totalWidth / 2f;

        int index = 0;
        foreach (var icon in _dynamicIcons)
        {
            if (!icon.activeSelf) continue;

            var rect = icon.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(startX + (index * _iconSpacing), 0);
            index++;
        }
    }

    private void UpdateOverflow(int rageCount)
    {
        int overflow = rageCount - _maxVisibleIcons;

        if (_overflowContainer != null)
        {
            _overflowContainer.SetActive(overflow > 0);
            if (_overflowText != null && overflow > 0)
            {
                _overflowText.text = $"+{overflow}";
            }
        }
    }

    private void AnimateNewRage(int previousCount, int newCount)
    {
        if (_usePreDefinedSlots)
        {
            // Anime les slots qui viennent d'être remplis
            for (int i = previousCount; i < newCount && i < _rageSlots.Count; i++)
            {
                if (_rageSlots[i] != null)
                {
                    StartCoroutine(PopAnimation(_rageSlots[i].transform));
                }
            }
        }
        else
        {
            // Anime les icônes dynamiques
            int startIndex = Mathf.Max(0, Mathf.Min(newCount, _maxVisibleIcons) - (newCount - previousCount));
            for (int i = startIndex; i < _dynamicIcons.Count && i < _maxVisibleIcons; i++)
            {
                if (_dynamicIcons[i].activeSelf)
                {
                    StartCoroutine(PopAnimation(_dynamicIcons[i].transform));
                }
            }
        }
    }

    private System.Collections.IEnumerator PopAnimation(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 popScale = originalScale * _popAnimationScale;

        // Scale up
        float elapsed = 0f;
        while (elapsed < _animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (_animationDuration / 2f);
            target.localScale = Vector3.Lerp(originalScale, popScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < _animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (_animationDuration / 2f);
            target.localScale = Vector3.Lerp(popScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private void CreateDefaultIconPrefab()
    {
        _iconPrefab = new GameObject("RageIconTemplate");
        _iconPrefab.SetActive(false);

        var rect = _iconPrefab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30, 30);

        var image = _iconPrefab.AddComponent<Image>();
        image.color = _iconColor;

        if (_rageIconSprite != null)
        {
            image.sprite = _rageIconSprite;
        }

        _iconPrefab.transform.SetParent(transform, false);
    }

    void OnDestroy()
    {
        if (_trackedPlayer != null)
        {
            _trackedPlayer.OnRageStockChanged -= OnRageChanged;
        }
    }
}
