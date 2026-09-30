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

    [Header("Consultation")]
    [Tooltip("Fenêtre ouverte en cliquant la pioche (triée par coût) ou la défausse (dernière carte jouée en premier)")]
    [SerializeField] private CardPileViewerUI _pileViewer;

    [Header("Défausse remélangée")]
    [Tooltip("Nombre maximum de cartes animées de la défausse vers la pioche")]
    [SerializeField] private int _reshuffleMaxCards = 8;
    [SerializeField] private float _reshuffleCardDuration = 0.35f;
    [SerializeField] private float _reshuffleInterval = 0.06f;
    [SerializeField] private float _reshuffleArcHeight = 70f;

    private DeckManager _currentDeckManager;
    private bool _isInitialized = false;

    void Awake()
    {
        MakeClickable(_deckVisual, ShowDeck);
        MakeClickable(_discardVisual, ShowDiscard);
    }

    private static void MakeClickable(GameObject visual, UnityEngine.Events.UnityAction onClick)
    {
        if (visual == null) return;
        if (!visual.TryGetComponent(out Button button)) button = visual.AddComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    private void ShowDeck()
    {
        if (_pileViewer == null || _currentDeckManager == null) return;
        var cards = CardPileOrder.ForDeckView(_currentDeckManager.GetDeckCards());
        _pileViewer.Show($"Pioche ({cards.Count}) · triée par coût", cards);
    }

    private void ShowDiscard()
    {
        if (_pileViewer == null || _currentDeckManager == null) return;
        var cards = CardPileOrder.ForDiscardView(_currentDeckManager.GetDiscardCards());
        _pileViewer.Show($"Défausse ({cards.Count}) · dernière jouée en premier", cards);
    }

    void OnEnable() => EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    void OnDisable() => EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);

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
            Champion shown = activeUnit != null ? LocalView.ChampionToShow(activeUnit) : null;
            if (shown != null)
            {
                BindToUnit(shown);
            }
        }
    }

    // Coop sur un PC : la pioche et la défausse suivent le champion dont c'est le tour ; réseau :
    // toujours celles du champion de ce PC (LocalView)
    private void OnTurnChanged(TurnChangedEvent e)
    {
        Champion shown = LocalView.ChampionToShow(e.NewActiveUnit);
        if (shown == null || (_currentDeckManager != null && _currentDeckManager.gameObject == shown.gameObject)) return;

        Unbind();
        BindToUnit(shown);
    }

    private void BindToUnit(Unit unit)
    {
        // On cherche le DeckManager sur l'unité active
        if (unit.TryGetComponentSafe(out _currentDeckManager))
        {
            // Abonnement aux événements
            _currentDeckManager.OnDeckChanged += UpdateDeckUI;
            _currentDeckManager.OnDiscardChanged += UpdateDiscardUI;
            _currentDeckManager.OnDiscardReshuffled += PlayReshuffle;

            // Mise à jour initiale
            UpdateDeckUI(_currentDeckManager.GetDeckCount());
            UpdateDiscardUI(_currentDeckManager.GetDiscardCount());

            _isInitialized = true;
            GameLog.Log($"DeckDiscardUI: connecté à {unit.name}.");
        }
    }

    private void Unbind()
    {
        if (_currentDeckManager != null)
        {
            _currentDeckManager.OnDeckChanged -= UpdateDeckUI;
            _currentDeckManager.OnDiscardChanged -= UpdateDiscardUI;
            _currentDeckManager.OnDiscardReshuffled -= PlayReshuffle;
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

    // La défausse repart dans la pioche : des dos de cartes volent d'un tas à l'autre, en arc, une par une
    private void PlayReshuffle(int cardCount)
    {
        if (_deckVisual == null || _discardVisual == null || !isActiveAndEnabled) return;

        int shown = Mathf.Clamp(cardCount, 1, _reshuffleMaxCards);
        for (int i = 0; i < shown; i++)
            StartCoroutine(FlyCardBack(i * _reshuffleInterval));
    }

    private System.Collections.IEnumerator FlyCardBack(float delay)
    {
        var from = (RectTransform)_discardVisual.transform;
        var to = (RectTransform)_deckVisual.transform;
        var parent = (RectTransform)from.parent;

        var card = new GameObject("ReshuffledCard", typeof(RectTransform), typeof(Image), typeof(Outline));
        var rt = (RectTransform)card.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = from.rect.size * 0.8f;
        card.GetComponent<Image>().color = CodexCardVisual.CardBorder;
        card.GetComponent<Image>().raycastTarget = false;
        card.GetComponent<Outline>().effectColor = CodexCardVisual.InkDim;
        rt.localScale = Vector3.zero;

        Vector3 start = parent.InverseTransformPoint(from.position);
        Vector3 end = parent.InverseTransformPoint(to.position);

        yield return new WaitForSeconds(delay);

        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.01f, _reshuffleCardDuration))
        {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            rt.localPosition = Vector3.Lerp(start, end, eased) + Vector3.up * (Mathf.Sin(eased * Mathf.PI) * _reshuffleArcHeight);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(eased * Mathf.PI) * 20f);
            rt.localScale = Vector3.one;
            yield return null;
        }

        Destroy(card);
    }

    void OnDestroy()
    {
        Unbind();
    }
}
