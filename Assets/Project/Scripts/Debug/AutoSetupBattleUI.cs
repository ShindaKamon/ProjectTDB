using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Script qui crée automatiquement toute l'UI de combat (Boss Health Bar + Enemy Card Preview).
/// Utilise ce script pour créer rapidement l'UI sans avoir à tout faire manuellement.
///
/// UTILISATION:
/// 1. Attache ce script à n'importe quel GameObject
/// 2. Clique sur le bouton "Setup Battle UI" dans l'Inspector
/// 3. OU lance le jeu et appuie sur F2
/// </summary>
public class AutoSetupBattleUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _setupOnStart = false;
    [SerializeField] private bool _createBattleUIManager = true;
    [SerializeField] private bool _createBossHealthBar = true;
    [SerializeField] private bool _createEnemyCardPreview = true;

    void Start()
    {
        if (_setupOnStart)
        {
            SetupAllBattleUI();
        }
    }

    void Update()
    {
        // Appuie sur F2 pour créer l'UI
        if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            SetupAllBattleUI();
        }
    }

    [ContextMenu("Setup Battle UI")]
    public void SetupAllBattleUI()
    {
        Debug.Log("=== AUTO SETUP BATTLE UI ===");

        // 1. Trouve ou crée le Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("Création du Canvas...");
            GameObject canvasGO = new GameObject("BattleUICanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("✅ Canvas créé");
        }

        // 2. Crée BattleUIManager si nécessaire
        if (_createBattleUIManager)
        {
            BattleUIManager existingManager = FindAnyObjectByType<BattleUIManager>();
            if (existingManager == null)
            {
                GameObject managerGO = new GameObject("BattleUIManager");
                managerGO.AddComponent<BattleUIManager>();
                Debug.Log("✅ BattleUIManager créé");
            }
            else
            {
                Debug.Log("BattleUIManager existe déjà");
            }
        }

        // 3. Crée Boss Health Bar UI
        if (_createBossHealthBar)
        {
            CreateBossHealthBarUI(canvas);
        }

        // 4. Crée Enemy Card Preview UI
        if (_createEnemyCardPreview)
        {
            CreateEnemyCardPreviewUI(canvas);
        }

        Debug.Log("=== SETUP TERMINÉ ===");
        Debug.Log("Lance le jeu pour voir les UI s'afficher automatiquement!");
    }

    private void CreateBossHealthBarUI(Canvas canvas)
    {
        // Vérifie si existe déjà
        BossHealthBarUI existing = FindAnyObjectByType<BossHealthBarUI>();
        if (existing != null)
        {
            Debug.Log("BossHealthBarUI existe déjà");
            return;
        }

        Debug.Log("Création de Boss Health Bar UI...");

        // Crée le panel principal
        GameObject panel = new GameObject("BossHealthBarPanel");
        panel.transform.SetParent(canvas.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Noir semi-transparent

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f); // Top Center
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -20);
        panelRect.sizeDelta = new Vector2(600, 80);

        // Nom du boss
        GameObject nameTextGO = new GameObject("BossNameText");
        nameTextGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
        nameText.text = "Boss Name";
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        RectTransform nameRect = nameTextGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -10);
        nameRect.sizeDelta = new Vector2(-20, 30);

        // Slider de vie
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(panel.transform, false);
        Slider slider = sliderGO.AddComponent<Slider>();

        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0);
        sliderRect.pivot = new Vector2(0.5f, 0);
        sliderRect.anchoredPosition = new Vector2(0, 10);
        sliderRect.sizeDelta = new Vector2(-40, 25);

        // Background du slider
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.5f, 0, 0); // Rouge foncé

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.red;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Configure le slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.interactable = false;

        // Texte HP
        GameObject hpTextGO = new GameObject("HealthText");
        hpTextGO.transform.SetParent(sliderGO.transform, false);
        TextMeshProUGUI hpText = hpTextGO.AddComponent<TextMeshProUGUI>();
        hpText.text = "100/100";
        hpText.fontSize = 18;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;

        RectTransform hpRect = hpTextGO.GetComponent<RectTransform>();
        hpRect.anchorMin = Vector2.zero;
        hpRect.anchorMax = Vector2.one;
        hpRect.sizeDelta = Vector2.zero;

        // Ajoute le script BossHealthBarUI
        BossHealthBarUI bossUI = panel.AddComponent<BossHealthBarUI>();

        // Assigne les références via réflexion
        var containerField = typeof(BossHealthBarUI).GetField("_container",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sliderField = typeof(BossHealthBarUI).GetField("_healthSlider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nameTextField = typeof(BossHealthBarUI).GetField("_bossNameText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hpTextField = typeof(BossHealthBarUI).GetField("_healthText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fillImageField = typeof(BossHealthBarUI).GetField("_fillImage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        containerField?.SetValue(bossUI, panel);
        sliderField?.SetValue(bossUI, slider);
        nameTextField?.SetValue(bossUI, nameText);
        hpTextField?.SetValue(bossUI, hpText);
        fillImageField?.SetValue(bossUI, fillImage);

        Debug.Log("✅ Boss Health Bar UI créée!");
    }

    private void CreateEnemyCardPreviewUI(Canvas canvas)
    {
        // Vérifie si existe déjà
        EnemyCardPreviewUI existing = FindAnyObjectByType<EnemyCardPreviewUI>();
        if (existing != null)
        {
            Debug.Log("EnemyCardPreviewUI existe déjà");
            return;
        }

        Debug.Log("Création de Enemy Card Preview UI...");

        // Crée le panel principal
        GameObject panel = new GameObject("EnemyCardPreviewPanel");
        panel.transform.SetParent(canvas.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f); // Middle Right
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20, 0);
        panelRect.sizeDelta = new Vector2(250, 350);

        // Titre
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Prochaine Carte";
        titleText.fontSize = 16;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.yellow;

        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 25);

        // Nom de la carte
        GameObject nameGO = new GameObject("CardNameText");
        nameGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = "Nom Carte";
        nameText.fontSize = 20;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -45);
        nameRect.sizeDelta = new Vector2(-20, 30);

        // Description
        GameObject descGO = new GameObject("CardDescriptionText");
        descGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI descText = descGO.AddComponent<TextMeshProUGUI>();
        descText.text = "Description de la carte";
        descText.fontSize = 14;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.textWrappingMode = TMPro.TextWrappingModes.Normal;
        descText.color = Color.white;

        RectTransform descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.pivot = new Vector2(0.5f, 1);
        descRect.anchoredPosition = new Vector2(0, -85);
        descRect.sizeDelta = new Vector2(-20, -130);

        // Coût
        GameObject costGO = new GameObject("CardCostText");
        costGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI costText = costGO.AddComponent<TextMeshProUGUI>();
        costText.text = "2 PA";
        costText.fontSize = 18;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = Color.yellow;

        RectTransform costRect = costGO.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0);
        costRect.anchorMax = new Vector2(1, 0);
        costRect.pivot = new Vector2(0.5f, 0);
        costRect.anchoredPosition = new Vector2(0, 10);
        costRect.sizeDelta = new Vector2(-20, 25);

        // Ajoute le script EnemyCardPreviewUI
        EnemyCardPreviewUI previewUI = panel.AddComponent<EnemyCardPreviewUI>();

        // Assigne les références via réflexion
        var containerField = typeof(EnemyCardPreviewUI).GetField("_previewContainer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nameField = typeof(EnemyCardPreviewUI).GetField("_cardNameText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descField = typeof(EnemyCardPreviewUI).GetField("_cardDescriptionText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var costField = typeof(EnemyCardPreviewUI).GetField("_cardCostText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        containerField?.SetValue(previewUI, panel);
        nameField?.SetValue(previewUI, nameText);
        descField?.SetValue(previewUI, descText);
        costField?.SetValue(previewUI, costText);

        Debug.Log("✅ Enemy Card Preview UI créée!");
    }
}
