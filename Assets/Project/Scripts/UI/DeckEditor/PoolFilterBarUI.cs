using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau de filtres du pool de l'éditeur de deck (façon SpamDex) : recherche, pastilles
/// d'émotion et de catégorie (multi-sélection), pastilles de coût PA (une à la fois, 4+ inclus),
/// tri au clic et réinitialisation. Ne fait qu'écrire dans le CardPoolQuery fourni par
/// DeckEditorUI puis émettre Changed ; le filtrage lui-même reste dans CardPoolQuery.
/// </summary>
public class PoolFilterBarUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _searchInput;

    [Header("Tri")]
    [SerializeField] private Button _sortKeyButton;
    [SerializeField] private TextMeshProUGUI _sortKeyLabel;
    [SerializeField] private Button _sortDirectionButton;
    [SerializeField] private TextMeshProUGUI _sortDirectionLabel;

    [Header("Réinitialisation")]
    [SerializeField] private Button _resetButton; // visible seulement si un filtre est actif

    private readonly List<FilterChipUI> _chips = new List<FilterChipUI>();
    private CardPoolQuery _query;
    private bool _initialized;

    /// <summary>Émis à chaque modification d'un critère (filtre ou tri).</summary>
    public System.Action Changed;

    void Awake() => EnsureInitialized();

    // Bind peut précéder Awake si le panneau démarre désactivé
    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        GetComponentsInChildren(true, _chips);
        foreach (var chip in _chips)
        {
            var captured = chip;
            chip.GetComponent<Button>().onClick.AddListener(() => OnChipClicked(captured));
        }

        if (_searchInput != null)
            _searchInput.onValueChanged.AddListener(OnSearchChanged);

        if (_sortKeyButton != null)
            _sortKeyButton.onClick.AddListener(OnSortKeyClicked);

        if (_sortDirectionButton != null)
            _sortDirectionButton.onClick.AddListener(OnSortDirectionClicked);

        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetClicked);
    }

    /// <summary>
    /// Associe le panneau à la requête du pool pour le deck affiché : filtres remis à zéro, seules
    /// les couleurs du deck sont proposées (aucune si le deck n'en a qu'une : filtrer ne servirait
    /// à rien). Le tri choisi est conservé.
    /// </summary>
    public void Bind(CardPoolQuery query, ICollection<EmotionType> deckColors)
    {
        EnsureInitialized();
        _query = query;
        _query.ResetFilters();

        if (_searchInput != null)
            _searchInput.SetTextWithoutNotify("");

        bool severalColors = deckColors == null || deckColors.Count != 1;
        foreach (var chip in _chips)
        {
            if (chip.Kind != FilterChipKind.Emotion) continue;
            bool offered = deckColors == null || deckColors.Count == 0 || deckColors.Contains(chip.Emotion);
            chip.gameObject.SetActive(offered && severalColors);
        }

        RefreshVisuals();
    }

    private void OnChipClicked(FilterChipUI chip)
    {
        if (_query == null) return;

        switch (chip.Kind)
        {
            case FilterChipKind.Emotion:
                Toggle(_query.Emotions, chip.Emotion);
                break;

            case FilterChipKind.Category:
                Toggle(_query.Categories, chip.Category);
                break;

            case FilterChipKind.Cost:
                // Un seul coût à la fois ; recliquer sur le coût actif enlève le filtre
                if (IsCostSelected(chip))
                {
                    _query.MinCost = 0;
                    _query.MaxCost = int.MaxValue;
                }
                else
                {
                    _query.MinCost = chip.Cost;
                    _query.MaxCost = chip.OrMore ? int.MaxValue : chip.Cost;
                }
                break;
        }

        NotifyChanged();
    }

    private static void Toggle<T>(HashSet<T> set, T value)
    {
        if (!set.Remove(value))
            set.Add(value);
    }

    private bool IsCostSelected(FilterChipUI chip) =>
        _query.MinCost == chip.Cost && _query.MaxCost == (chip.OrMore ? int.MaxValue : chip.Cost);

    private void OnSearchChanged(string text)
    {
        if (_query == null) return;
        _query.Search = text;
        NotifyChanged();
    }

    private void OnSortKeyClicked()
    {
        if (_query == null) return;
        _query.SortKey = _query.SortKey switch
        {
            CardSortKey.Cost => CardSortKey.Name,
            CardSortKey.Name => CardSortKey.Emotion,
            _ => CardSortKey.Cost,
        };
        NotifyChanged();
    }

    private void OnSortDirectionClicked()
    {
        if (_query == null) return;
        _query.Descending = !_query.Descending;
        NotifyChanged();
    }

    private void OnResetClicked()
    {
        if (_query == null) return;
        _query.ResetFilters();
        if (_searchInput != null)
            _searchInput.SetTextWithoutNotify("");
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        RefreshVisuals();
        Changed?.Invoke();
    }

    private void RefreshVisuals()
    {
        if (_query == null) return;

        foreach (var chip in _chips)
        {
            bool active = chip.Kind switch
            {
                FilterChipKind.Emotion => _query.Emotions.Contains(chip.Emotion),
                FilterChipKind.Category => _query.Categories.Contains(chip.Category),
                _ => IsCostSelected(chip),
            };
            chip.SetActive(active);
        }

        if (_sortKeyLabel != null)
        {
            _sortKeyLabel.text = _query.SortKey switch
            {
                CardSortKey.Name => "Tri : Nom",
                CardSortKey.Emotion => "Tri : Émotion",
                _ => "Tri : Coût",
            };
        }

        if (_sortDirectionLabel != null)
            _sortDirectionLabel.text = _query.Descending ? "Décroissant" : "Croissant";

        if (_resetButton != null)
            _resetButton.gameObject.SetActive(_query.HasActiveFilters);
    }
}
