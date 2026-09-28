using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Écran de fin de combat (MVP) : « VICTOIRE ! » ou « DÉFAITE », puis Rejouer (même équipe, même
/// combat) ou Menu principal. Affiché sur BattleEndedEvent, construit à la première utilisation ;
/// son fond bloque les clics sur le plateau. Récompenses et retour de la couleur : plus tard.
/// </summary>
public class BattleEndUI : MonoBehaviour
{
    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
    [Tooltip("Délai avant l'écran, pour voir le dernier coup (secondes)")]
    [SerializeField] private float _delay = 0.8f;
    [SerializeField] private Color _victoryColor = new Color(0.95f, 0.8f, 0.3f);
    [SerializeField] private Color _defeatColor = new Color(0.85f, 0.3f, 0.3f);

    private GameObject _root;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _subtitle;
    private TextMeshProUGUI _alliesTable;
    private TextMeshProUGUI _enemiesTable;

    // Récapitulatif : dégâts et soins de chaque unité pendant le combat
    private readonly CombatStats _stats = new CombatStats();
    private bool _unitsRegistered;

    // Abonné du réveil à la destruction : l'écran est masqué jusqu'à la fin du combat
    void Awake()
    {
        EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Subscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitHealed);
        EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    }

    private void OnUnitDamaged(UnitDamagedEvent e) => _stats.RecordDamage(e.Source, e.Target, e.EffectiveDamage);
    private void OnUnitHealed(UnitHealedEvent e) => _stats.RecordHealing(e.Source, e.HealAmount);

    // Premier tour : toutes les unités du combat figurent au tableau, même celles qui ne feront rien
    private void OnTurnChanged(TurnChangedEvent e)
    {
        if (_unitsRegistered || !Services.IsGridServiceAvailable()) return;
        _unitsRegistered = true;
        foreach (Unit unit in Services.Grid.GetAllUnits()) _stats.Register(unit);
    }

    private void OnBattleEnded(BattleEndedEvent e) => StartCoroutine(ShowAfterDelay(e.Result));

    private System.Collections.IEnumerator ShowAfterDelay(BattleResult result)
    {
        yield return new WaitForSeconds(_delay);
        Show(result);
    }

    private void Show(BattleResult result)
    {
        if (_root == null) Build();

        bool victory = result == BattleResult.Victory;
        _title.text = victory ? "VICTOIRE !" : "DÉFAITE";
        _title.color = victory ? _victoryColor : _defeatColor;
        _subtitle.text = victory ? "Tous les ennemis sont vaincus." : "Tous les champions sont tombés.";
        _alliesTable.text = Table("Alliés", _stats.Allies);
        _enemiesTable.text = Table("Ennemis", _stats.Enemies);
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    // Colonnes Nom / Dégâts / Soins ; valeurs aux couleurs des pastilles (dégâts rouges, soins roses)
    private static string Table(string title, System.Collections.Generic.List<CombatStats.Entry> entries)
    {
        string dmg = ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(ChipKind.Damage));
        string heal = ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(ChipKind.Heal));
        string dim = ColorUtility.ToHtmlStringRGB(CodexCardVisual.InkDim);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>{title}</b>");
        sb.AppendLine($"<color=#{dim}><size=80%>Nom<pos=64%>Dégâts<pos=84%>Soins</size></color>");
        foreach (var e in entries)
            sb.AppendLine($"{e.Name}<pos=64%><color=#{dmg}>{e.Damage}</color><pos=84%><color=#{heal}>{e.Healing}</color>");
        if (entries.Count == 0) sb.AppendLine($"<color=#{dim}>—</color>");
        return sb.ToString();
    }

    private void Replay() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    private void MainMenu() => SceneManager.LoadScene(_mainMenuSceneName);

    private void Build()
    {
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;

        _root = new GameObject("BattleEndScreen", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        var rootRt = (RectTransform)_root.transform;
        rootRt.SetParent(canvas.transform, false);
        Stretch(rootRt);
        var ownCanvas = _root.GetComponent<Canvas>();
        ownCanvas.overrideSorting = true;
        ownCanvas.sortingOrder = 600; // au-dessus de tout le reste de l'interface
        _root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f); // bloque aussi les clics sur le plateau

        _title = NewText("Title", rootRt, 96f, FontStyles.Bold, new Vector2(0f, 290f), new Vector2(1000f, 130f));
        _subtitle = NewText("Subtitle", rootRt, 32f, FontStyles.Normal, new Vector2(0f, 210f), new Vector2(1000f, 50f));

        // Récapitulatif : alliés à gauche, ennemis à droite
        _alliesTable = NewTablePanel("AlliesTable", rootRt, new Vector2(-285f, 10f));
        _enemiesTable = NewTablePanel("EnemiesTable", rootRt, new Vector2(285f, 10f));

        NewButton("ReplayButton", rootRt, "Rejouer", new Vector2(-150f, -250f), Replay);
        NewButton("MainMenuButton", rootRt, "Menu principal", new Vector2(150f, -250f), MainMenu);
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

    private static TextMeshProUGUI NewTablePanel(string name, Transform parent, Vector2 pos)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)panel.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(540f, 300f);
        panel.GetComponent<Image>().color = CodexCardVisual.CardBackground;

        TextMeshProUGUI text = NewText("Text", rt, 26f, FontStyles.Normal, Vector2.zero, Vector2.zero);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(24f, 16f);
        text.rectTransform.offsetMax = new Vector2(-24f, -16f);
        text.alignment = TextAlignmentOptions.TopLeft;
        return text;
    }

    private static void NewButton(string name, Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(260f, 64f);
        go.GetComponent<Image>().color = CodexCardVisual.CardBorder;
        go.GetComponent<Button>().onClick.AddListener(onClick);

        TextMeshProUGUI text = NewText("Label", rt, 28f, FontStyles.Normal, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform);
        text.text = label;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
