using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu Échap, dans toutes les scènes (décision du 02/10/2026) : Reprendre, Menu principal (sauf depuis le menu
/// principal) et Quitter le jeu. Créé au lancement et gardé d'une scène à l'autre ; met le jeu en pause en solo
/// (pas en réseau, la partie continue pour les autres). Échap le ferme ; un panneau qui se ferme lui-même avec Échap
/// (ex. CardPileViewerUI) appelle ConsumeEscape pour ne pas l'ouvrir dans la foulée.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private const string MainMenuScene = "MainMenuScene";
    private static int _escapeConsumedFrame = -1;

    private GameObject _root;
    private GameObject _mainMenuButton;
    private float _timeScaleBeforePause = 1f;

    public static bool IsOpen { get; private set; }

    /// <summary>Échap de cette image a déjà servi (ex. fermer la pile de cartes affichée).</summary>
    public static void ConsumeEscape() => _escapeConsumedFrame = Time.frameCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("PauseMenu");
        DontDestroyOnLoad(go);
        go.AddComponent<PauseMenuUI>();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Close();

    // Après les Update des panneaux (ex. pile de cartes), qui ont pu consommer Échap
    private void LateUpdate()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (_escapeConsumedFrame == Time.frameCount) return;
        if (IsOpen) Close();
        else Open();
    }

    private void Open()
    {
        if (_root == null) Build();
        EnsureEventSystem();
        _mainMenuButton.SetActive(SceneManager.GetActiveScene().name != MainMenuScene);
        _root.SetActive(true);
        IsOpen = true;
        if (!NetworkSession.IsActive)
        {
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    // Les boutons ont besoin d'un EventSystem ; certaines scènes n'en ont pas (ex. ExplorationScene) : on en crée un
    // dans la scène active, détruit avec elle
    private static void EnsureEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null) return;
        var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
    }

    private void Close()
    {
        if (!IsOpen) return;
        if (_root != null) _root.SetActive(false);
        IsOpen = false;
        if (!NetworkSession.IsActive) Time.timeScale = _timeScaleBeforePause;
    }

    private void GoToMainMenu()
    {
        Close();
        DungeonRun.Clear();
        if (NetworkSession.IsActive) NetworkSession.LeaveToMenu();
        else SceneManager.LoadScene(MainMenuScene);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Build()
    {
        _root = new GameObject("PauseMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _root.transform.SetParent(transform, false);
        var canvas = _root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // au-dessus de tout, écran de fin de combat compris
        var scaler = _root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Fond qui bloque les clics sur le jeu
        var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        var dimRt = (RectTransform)dim.transform;
        dimRt.SetParent(_root.transform, false);
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var panelRt = (RectTransform)panel.transform;
        panelRt.SetParent(_root.transform, false);
        panelRt.sizeDelta = new Vector2(420f, 0f);
        panel.GetComponent<Image>().color = CodexCardVisual.CardBackground;
        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 32, 36);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI title = NewText("Title", panelRt, "Pause", 48f, FontStyles.Bold);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;
        NewButton(panelRt, "Reprendre", Close);
        _mainMenuButton = NewButton(panelRt, "Menu principal", GoToMainMenu);
        NewButton(panelRt, "Quitter le jeu", Quit);
        _root.SetActive(false);
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = CodexCardVisual.Ink;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static GameObject NewButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = CodexCardVisual.CardBorder;
        go.GetComponent<Button>().onClick.AddListener(onClick);
        go.GetComponent<LayoutElement>().preferredHeight = 64f;
        TextMeshProUGUI label = NewText("Label", go.transform, text, 28f, FontStyles.Normal);
        var rt = label.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }
}
