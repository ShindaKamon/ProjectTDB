using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Réseau : messages d'incident en combat.
/// - Joueur déconnecté (NetworkPlayerLeftEvent) : bandeau, la partie continue.
/// - Désynchronisation (NetworkDesyncEvent) : écran « Partie interrompue » avec retour au menu
///   (les PC ne jouent plus la même partie, CombatCommandExecutor n'exécute plus rien).
/// </summary>
public class DesyncWarningUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private GameObject _interruption;

    void Awake()
    {
        EventBus.Subscribe<NetworkDesyncEvent>(OnDesync);
        EventBus.Subscribe<NetworkPlayerLeftEvent>(OnPlayerLeft);
        if (_text != null) _text.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<NetworkDesyncEvent>(OnDesync);
        EventBus.Unsubscribe<NetworkPlayerLeftEvent>(OnPlayerLeft);
    }

    private void OnPlayerLeft(NetworkPlayerLeftEvent e)
    {
        GameLog.LogWarning($"Réseau : le joueur {e.Actor + 1} s'est déconnecté, l'hôte passe ses tours.");
        if (_text == null) return;
        _text.text = $"Joueur {e.Actor + 1} déconnecté : ses tours sont passés";
        _text.gameObject.SetActive(true);
    }

    private void OnDesync(NetworkDesyncEvent e)
    {
        GameLog.LogWarning($"Réseau : désynchronisation signalée au tour {e.Turn}");
        if (_interruption == null) _interruption = Build(e.Turn);
        _interruption.SetActive(true);
        _interruption.transform.SetAsLastSibling();
    }

    private GameObject Build(int turn)
    {
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;

        var root = new GameObject("InterruptionScreen", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        var rootRt = (RectTransform)root.transform;
        rootRt.SetParent(canvas.transform, false);
        Stretch(rootRt);
        var ownCanvas = root.GetComponent<Canvas>();
        ownCanvas.overrideSorting = true;
        ownCanvas.sortingOrder = 700; // au-dessus de l'écran de fin de combat
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        TextMeshProUGUI title = NewText("Title", rootRt, 72f, FontStyles.Bold, new Vector2(0f, 120f), new Vector2(1400f, 110f));
        title.text = "Partie interrompue";
        TextMeshProUGUI detail = NewText("Detail", rootRt, 32f, FontStyles.Normal, new Vector2(0f, 20f), new Vector2(1200f, 120f));
        detail.text = $"Désynchronisation au tour {turn} : les PC ne voient plus la même partie.";

        var button = new GameObject("MainMenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
        var buttonRt = (RectTransform)button.transform;
        buttonRt.SetParent(rootRt, false);
        buttonRt.anchoredPosition = new Vector2(0f, -140f);
        buttonRt.sizeDelta = new Vector2(320f, 70f);
        button.GetComponent<Image>().color = CodexCardVisual.CardBorder;
        button.GetComponent<Button>().onClick.AddListener(NetworkSession.LeaveToMenu);
        TextMeshProUGUI label = NewText("Label", buttonRt, 30f, FontStyles.Normal, Vector2.zero, Vector2.zero);
        Stretch(label.rectTransform);
        label.text = "Retour au menu";

        return root;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, float size, FontStyles style, Vector2 pos, Vector2 box)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = CodexCardVisual.Ink;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
