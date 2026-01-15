using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeckDiscardUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI _deckCountText;
    [SerializeField] private TextMeshProUGUI _discardCountText;
    [SerializeField] private GameObject _deckVisual; // Le tas de cartes (Deck)
    [SerializeField] private GameObject _discardVisual; // Le tas de cartes (Défausse)

    private DeckManager _currentDeckManager;
    private bool _isInitialized = false;

    void Update()
    {
        if (!_isInitialized)
        {
            TryInitialize();
        }
    }

    private void TryInitialize()
    {
        // On attend que le GridService et l'unité active soient prêts
        if (Services.IsGridServiceAvailable())
        {
            Unit activeUnit = Services.Grid.GetActiveUnit();
            if (activeUnit != null)
            {
                // On cherche le DeckManager sur l'unité active
                if (activeUnit.TryGetComponentSafe(out _currentDeckManager))
                {
                    // Abonnement aux événements
                    _currentDeckManager.OnDeckChanged += UpdateDeckUI;
                    _currentDeckManager.OnDiscardChanged += UpdateDiscardUI;
                    
                    // Mise à jour initiale
                    UpdateDeckUI(_currentDeckManager.GetDeckCount());
                    UpdateDiscardUI(_currentDeckManager.GetDiscardCount());
                    
                    _isInitialized = true;
                    Debug.Log("DeckDiscardUI: Initialisé avec succès.");
                }
            }
        }
    }

    private void UpdateDeckUI(int count)
    {
        if (_deckCountText != null)
        {
            _deckCountText.text = count.ToString();
        }

        if (_deckVisual != null)
        {
            // On peut cacher le visuel si le deck est vide
            _deckVisual.SetActive(count > 0);
        }
    }

    private void UpdateDiscardUI(int count)
    {
        if (_discardCountText != null)
        {
            _discardCountText.text = count.ToString();
        }

        if (_discardVisual != null)
        {
            // On cache la défausse si elle est vide
            _discardVisual.SetActive(count > 0);
        }
    }

    void OnDestroy()
    {
        if (_currentDeckManager != null)
        {
            _currentDeckManager.OnDeckChanged -= UpdateDeckUI;
            _currentDeckManager.OnDiscardChanged -= UpdateDiscardUI;
        }
    }
}
