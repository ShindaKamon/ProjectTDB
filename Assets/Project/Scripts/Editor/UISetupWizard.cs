#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Wizard pour creer et configurer automatiquement tous les prefabs UI du systeme de selection de champion
/// </summary>
public class UISetupWizard : EditorWindow
{
    private static string PrefabPath = "Assets/Project/Prefabs/UI/ChampionSelect";

    [MenuItem("Tools/UI/Setup Wizard - Champion Select")]
    public static void ShowWindow()
    {
        GetWindow<UISetupWizard>("UI Setup Wizard");
    }

    [MenuItem("Tools/UI/Generer tous les prefabs")]
    public static void GenerateAllPrefabs()
    {
        EnsureFolderExists(PrefabPath);

        CreateDeckListRowPrefab();
        CreateDeckSlotPrefab();
        CreateCardPoolItemPrefab();
        CreateDeckCardSlotPrefab();

        AssetDatabase.Refresh();
        Debug.Log("Tous les prefabs UI ont ete generes avec succes!");
    }

    private void OnGUI()
    {
        GUILayout.Label("Champion Select UI Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generer TOUS les prefabs", GUILayout.Height(40)))
        {
            GenerateAllPrefabs();
        }

        GUILayout.Space(10);
        GUILayout.Label("Ou generez individuellement:", EditorStyles.label);

        if (GUILayout.Button("DeckListRow (liste du deck)"))
            CreateDeckListRowPrefab();

        if (GUILayout.Button("DeckSlot (slot de deck)"))
            CreateDeckSlotPrefab();

        if (GUILayout.Button("CardPoolItem (pool d'edition)"))
            CreateCardPoolItemPrefab();

        if (GUILayout.Button("DeckCardSlot (slot d'edition)"))
            CreateDeckCardSlotPrefab();

        GUILayout.Space(20);

        if (GUILayout.Button("Configurer la scene actuelle", GUILayout.Height(30)))
        {
            SetupCurrentScene();
        }
    }

    private static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }

    /// <summary>
    /// Cree le prefab DeckListRow : une ligne de la liste du deck (facon MTG Arena) avec cout PA
    /// dans un rond a la couleur de l'emotion (blanc pour une Signature), nom et quantite.
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/DeckListRow")]
    public static void CreateDeckListRowPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root : fond de la ligne, hauteur fixe pour le VerticalLayoutGroup de la liste
        GameObject root = new GameObject("DeckListRow");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(300, 32);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = CodexCardVisual.CardBackground;

        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.minHeight = 32;
        layout.preferredHeight = 32;

        CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
        DeckListRowUI rowUI = root.AddComponent<DeckListRowUI>();

        // Rond de cout PA (couleur de l'emotion, appliquee par DeckListRowUI)
        GameObject costBadge = CreateChild(root, "CostBadge");
        Image costBadgeImg = costBadge.AddComponent<Image>();
        costBadgeImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        costBadgeImg.color = new Color(0.9f, 0.7f, 0.2f);
        costBadgeImg.raycastTarget = false;
        RectTransform costRT = costBadge.GetComponent<RectTransform>();
        costRT.anchorMin = new Vector2(0, 0.5f);
        costRT.anchorMax = new Vector2(0, 0.5f);
        costRT.pivot = new Vector2(0, 0.5f);
        costRT.anchoredPosition = new Vector2(8, 0);
        costRT.sizeDelta = new Vector2(24, 24);

        GameObject costText = CreateChild(costBadge, "CostText");
        TextMeshProUGUI costTMP = costText.AddComponent<TextMeshProUGUI>();
        costTMP.text = "2";
        costTMP.fontSize = 15;
        costTMP.fontStyle = FontStyles.Bold;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.color = Color.white;
        costTMP.raycastTarget = false;
        SetAnchors(costText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Nom de la carte (entre le cout et la quantite, tronque si trop long)
        GameObject nameText = CreateChild(root, "NameText");
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nom de la carte";
        nameTMP.fontSize = 16;
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameTMP.color = Color.white;
        nameTMP.textWrappingMode = TextWrappingModes.NoWrap;
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        nameTMP.raycastTarget = false;
        SetAnchors(nameText, Vector2.zero, Vector2.one, new Vector2(40, 0), new Vector2(-44, 0));

        // Quantite (bord droit)
        GameObject countText = CreateChild(root, "CountText");
        TextMeshProUGUI countTMP = countText.AddComponent<TextMeshProUGUI>();
        countTMP.text = "×2";
        countTMP.fontSize = 16;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.alignment = TextAlignmentOptions.MidlineRight;
        countTMP.color = new Color(1f, 0.85f, 0.5f);
        countTMP.raycastTarget = false;
        SetAnchors(countText, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-42, 0), new Vector2(-8, 0));

        // Assigner les references au script
        SerializedObject so = new SerializedObject(rowUI);
        so.FindProperty("_background").objectReferenceValue = rootImage;
        so.FindProperty("_costBadge").objectReferenceValue = costBadgeImg;
        so.FindProperty("_costText").objectReferenceValue = costTMP;
        so.FindProperty("_nameText").objectReferenceValue = nameTMP;
        so.FindProperty("_countText").objectReferenceValue = countTMP;
        so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        so.ApplyModifiedProperties();

        // Sauvegarder le prefab
        string path = $"{PrefabPath}/DeckListRow.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
    }

    /// <summary>
    /// Cree le prefab DeckSlot : tuile d'un deck sur la page Choix du deck (palette du codex) avec
    /// bande aux couleurs du deck, nom, couleurs ecrites dans leur teinte et nombre de cartes.
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/DeckSlot")]
    public static void CreateDeckSlotPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root : fond et cadre (couleur du cadre geree par DeckSlotUI : normal / survol / selection)
        GameObject root = new GameObject("DeckSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(260, 120);

        Image bg = root.AddComponent<Image>();
        bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        bg.type = Image.Type.Sliced;
        bg.color = CodexCardVisual.CardBackground;

        Outline border = root.AddComponent<Outline>();
        border.effectColor = CodexCardVisual.CardBorder;
        border.effectDistance = new Vector2(1.5f, -1.5f);

        DeckSlotUI slotUI = root.AddComponent<DeckSlotUI>();

        // Bande du haut : un segment par couleur du deck (crees par DeckSlotUI)
        GameObject strip = CreateChild(root, "ColorStrip");
        SetAnchors(strip, new Vector2(0, 1), new Vector2(1, 1), new Vector2(4, -12), new Vector2(-4, -4));
        HorizontalLayoutGroup stripLayout = strip.AddComponent<HorizontalLayoutGroup>();
        stripLayout.spacing = 2;
        stripLayout.childControlWidth = stripLayout.childControlHeight = true;
        stripLayout.childForceExpandWidth = stripLayout.childForceExpandHeight = true;

        TextMeshProUGUI nameTMP = AddText(root, "DeckNameText", "Nom du deck", 22, FontStyles.Bold, TextAlignmentOptions.Left, CodexCardVisual.Ink);
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        nameTMP.textWrappingMode = TextWrappingModes.NoWrap;
        SetAnchors(nameTMP.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -50), new Vector2(-16, -18));

        TextMeshProUGUI colorsTMP = AddText(root, "ColorsText", "Colère  ·  Joie", 16, FontStyles.Bold, TextAlignmentOptions.Left, CodexCardVisual.Ink);
        colorsTMP.textWrappingMode = TextWrappingModes.NoWrap;
        SetAnchors(colorsTMP.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -78), new Vector2(-16, -52));

        TextMeshProUGUI countTMP = AddText(root, "CardCountText", "18 cartes", 14, FontStyles.Normal, TextAlignmentOptions.Left, CodexCardVisual.InkDim);
        SetAnchors(countTMP.gameObject, new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 10), new Vector2(-16, 34));

        // Assigner les references au script
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("_background").objectReferenceValue = bg;
        so.FindProperty("_border").objectReferenceValue = border;
        so.FindProperty("_colorStrip").objectReferenceValue = strip.transform;
        so.FindProperty("_deckNameText").objectReferenceValue = nameTMP;
        so.FindProperty("_colorsText").objectReferenceValue = colorsTMP;
        so.FindProperty("_cardCountText").objectReferenceValue = countTMP;
        so.ApplyModifiedProperties();

        // Sauvegarder le prefab (meme chemin : les references de la scene sont conservees)
        string path = $"{PrefabPath}/DeckSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
    }

    /// <summary>
    /// Cree le prefab CardPoolItem (carte du pool d'edition) au design du codex emotionnel :
    /// en-tete (rond de cout, nom, sous-titre), schema de portee 9x9 + legende, pastilles
    /// d'effets (icones du codex dans Textures/UI/CodexIcons), description.
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/CardPoolItem")]
    public static void CreateCardPoolItemPrefab()
    {
        EnsureFolderExists(PrefabPath);

        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Root : cadre de la carte
        GameObject root = new GameObject("CardPoolItem");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(250, 250);

        Image bg = root.AddComponent<Image>();
        bg.sprite = rounded;
        bg.type = Image.Type.Sliced;
        bg.color = CodexCardVisual.CardBackground;

        Outline border = root.AddComponent<Outline>();
        border.effectColor = CodexCardVisual.CardBorder;
        border.effectDistance = new Vector2(1.5f, -1.5f);

        CardPoolItemUI ui = root.AddComponent<CardPoolItemUI>();

        // --- En-tete ---
        GameObject costCircle = CreateChild(root, "CostCircle");
        Image costImg = costCircle.AddComponent<Image>();
        costImg.sprite = circle;
        costImg.color = CodexCardVisual.EmotionColor(EmotionType.Colere);
        costImg.raycastTarget = false;
        RectTransform costRT = costCircle.GetComponent<RectTransform>();
        costRT.anchorMin = costRT.anchorMax = new Vector2(0, 1);
        costRT.pivot = new Vector2(0, 1);
        costRT.anchoredPosition = new Vector2(12, -12);
        costRT.sizeDelta = new Vector2(32, 32);

        TextMeshProUGUI costTMP = AddText(costCircle, "CostText", "2", 19, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        SetAnchors(costTMP.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        TextMeshProUGUI nameTMP = AddText(root, "NameText", "Nom de la carte", 17, FontStyles.Bold, TextAlignmentOptions.TopLeft, CodexCardVisual.Ink);
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        nameTMP.textWrappingMode = TextWrappingModes.NoWrap;
        SetAnchors(nameTMP.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(52, -32), new Vector2(-10, -11));

        TextMeshProUGUI subTMP = AddText(root, "SubtitleText", "Colère · Standard", 11.5f, FontStyles.Normal, TextAlignmentOptions.TopLeft, CodexCardVisual.InkDim);
        subTMP.overflowMode = TextOverflowModes.Ellipsis;
        subTMP.textWrappingMode = TextWrappingModes.NoWrap;
        SetAnchors(subTMP.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(52, -48), new Vector2(-10, -32));

        // --- Schema de portee 9x9 (cases de 9 px, 1 px de trait) ---
        GameObject diagram = CreateChild(root, "Diagram");
        Image diagramFrame = diagram.AddComponent<Image>();
        diagramFrame.color = CodexCardVisual.GridLine; // le fond visible entre les cases fait office de quadrillage
        diagramFrame.raycastTarget = false;
        RectTransform diagRT = diagram.GetComponent<RectTransform>();
        diagRT.anchorMin = diagRT.anchorMax = new Vector2(0, 1);
        diagRT.pivot = new Vector2(0, 1);
        diagRT.anchoredPosition = new Vector2(12, -58);
        diagRT.sizeDelta = new Vector2(91, 91);

        GridLayoutGroup grid = diagram.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(1, 1, 1, 1);
        grid.cellSize = new Vector2(9, 9);
        grid.spacing = new Vector2(1, 1);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = CodexCardVisual.DiagramSize;
        for (int i = 0; i < CodexCardVisual.DiagramSize * CodexCardVisual.DiagramSize; i++)
        {
            GameObject cell = CreateChild(diagram, "Cell");
            Image cellImg = cell.AddComponent<Image>();
            cellImg.color = CodexCardVisual.GridCell;
            cellImg.raycastTarget = false;
        }

        TextMeshProUGUI captionTMP = AddText(root, "CaptionText", "au contact", 10.5f, FontStyles.Normal, TextAlignmentOptions.TopLeft, CodexCardVisual.InkDim);
        SetAnchors(captionTMP.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -178), new Vector2(112, -152));

        // --- Pastilles d'effets (a droite du schema, une par ligne) ---
        GameObject chips = CreateChild(root, "Chips");
        SetAnchors(chips, new Vector2(0, 1), new Vector2(1, 1), new Vector2(116, -150), new Vector2(-10, -58));
        VerticalLayoutGroup chipsLayout = chips.AddComponent<VerticalLayoutGroup>();
        chipsLayout.spacing = 5;
        chipsLayout.childAlignment = TextAnchor.UpperLeft;
        chipsLayout.childControlWidth = false;
        chipsLayout.childControlHeight = false;
        chipsLayout.childForceExpandWidth = false;
        chipsLayout.childForceExpandHeight = false;

        // --- Description ---
        TextMeshProUGUI descTMP = AddText(root, "DescriptionText", "Description de la carte.", 12.5f, FontStyles.Normal, TextAlignmentOptions.TopLeft, CodexCardVisual.Ink);
        descTMP.enableAutoSizing = true;
        descTMP.fontSizeMin = 10;
        descTMP.fontSizeMax = 12.5f;
        descTMP.overflowMode = TextOverflowModes.Ellipsis;
        SetAnchors(descTMP.gameObject, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 10), new Vector2(-12, -182));

        // Icones du codex (generees depuis ses SVG)
        var icons = new System.Collections.Generic.List<Sprite>();
        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Project/Textures/UI/CodexIcons" }))
            icons.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));

        // Assigner les references au script
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("_background").objectReferenceValue = bg;
        so.FindProperty("_border").objectReferenceValue = border;
        so.FindProperty("_costCircle").objectReferenceValue = costImg;
        so.FindProperty("_costText").objectReferenceValue = costTMP;
        so.FindProperty("_nameText").objectReferenceValue = nameTMP;
        so.FindProperty("_subtitleText").objectReferenceValue = subTMP;
        so.FindProperty("_diagramGrid").objectReferenceValue = diagram.transform;
        so.FindProperty("_captionText").objectReferenceValue = captionTMP;
        so.FindProperty("_chipsContainer").objectReferenceValue = chips.transform;
        so.FindProperty("_chipBackground").objectReferenceValue = rounded;
        so.FindProperty("_descriptionText").objectReferenceValue = descTMP;
        SerializedProperty iconsProp = so.FindProperty("_chipIcons");
        iconsProp.arraySize = icons.Count;
        for (int i = 0; i < icons.Count; i++)
            iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
        so.ApplyModifiedProperties();

        // Sauvegarder le prefab
        string path = $"{PrefabPath}/CardPoolItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path} ({icons.Count} icones)");
    }

    private static TextMeshProUGUI AddText(GameObject parent, string name, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
    {
        GameObject go = CreateChild(parent, name);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>
    /// Cree le prefab DeckCardSlot pour l'edition de deck
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/DeckCardSlot")]
    public static void CreateDeckCardSlotPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root
        GameObject root = new GameObject("DeckCardSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(80, 100);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(0.2f, 0.2f, 0.25f);

        DeckCardSlotUI slotUI = root.AddComponent<DeckCardSlotUI>();

        // Empty State Background
        GameObject emptyBg = CreateChild(root, "EmptyBackground");
        Image emptyBgImg = emptyBg.AddComponent<Image>();
        emptyBgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);
        SetAnchors(emptyBg, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);

        // Card Image
        GameObject cardImage = CreateChild(root, "CardImage");
        Image cardImg = cardImage.AddComponent<Image>();
        cardImg.color = new Color(0.3f, 0.3f, 0.3f);
        SetAnchors(cardImage, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);

        // Card Name
        GameObject nameText = CreateChild(root, "CardNameText");
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "";
        nameTMP.fontSize = 9;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        nameTMP.enableWordWrapping = true;
        SetAnchors(nameText, new Vector2(0, 0), new Vector2(1, 0.2f), Vector2.zero, Vector2.zero);

        // Cost Text
        GameObject costText = CreateChild(root, "CostText");
        TextMeshProUGUI costTMP = costText.AddComponent<TextMeshProUGUI>();
        costTMP.text = "";
        costTMP.fontSize = 12;
        costTMP.fontStyle = FontStyles.Bold;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.color = new Color(1f, 0.8f, 0.2f);
        RectTransform costRT = costText.GetComponent<RectTransform>();
        costRT.anchorMin = new Vector2(0, 1);
        costRT.anchorMax = new Vector2(0, 1);
        costRT.pivot = new Vector2(0, 1);
        costRT.anchoredPosition = new Vector2(5, -5);
        costRT.sizeDelta = new Vector2(20, 20);

        // Remove Button
        GameObject removeBtn = CreateChild(root, "RemoveButton");
        Image removeBtnImg = removeBtn.AddComponent<Image>();
        removeBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        Button removeBtnComp = removeBtn.AddComponent<Button>();
        removeBtnComp.targetGraphic = removeBtnImg;
        RectTransform removeBtnRT = removeBtn.GetComponent<RectTransform>();
        removeBtnRT.anchorMin = new Vector2(1, 1);
        removeBtnRT.anchorMax = new Vector2(1, 1);
        removeBtnRT.pivot = new Vector2(1, 1);
        removeBtnRT.anchoredPosition = new Vector2(-2, -2);
        removeBtnRT.sizeDelta = new Vector2(20, 20);

        // Remove Button Text (X)
        GameObject removeText = CreateChild(removeBtn, "Text");
        TextMeshProUGUI removeTMP = removeText.AddComponent<TextMeshProUGUI>();
        removeTMP.text = "X";
        removeTMP.fontSize = 12;
        removeTMP.fontStyle = FontStyles.Bold;
        removeTMP.alignment = TextAlignmentOptions.Center;
        removeTMP.color = Color.white;
        SetAnchors(removeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Assigner les references
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("_backgroundImage").objectReferenceValue = rootImage;
        so.FindProperty("_cardImage").objectReferenceValue = cardImg;
        so.FindProperty("_cardNameText").objectReferenceValue = nameTMP;
        so.FindProperty("_costText").objectReferenceValue = costTMP;
        so.FindProperty("_removeButton").objectReferenceValue = removeBtnComp;
        so.FindProperty("_emptyStateImage").objectReferenceValue = emptyBgImg;
        so.ApplyModifiedProperties();

        // Sauvegarder
        string path = $"{PrefabPath}/DeckCardSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
    }

    /// <summary>
    /// Configure automatiquement la scene actuelle avec les references aux prefabs
    /// </summary>
    public static void SetupCurrentScene()
    {
        // Trouver DeckEditorUI (zone Pool + Deck + Courbe PA de l'écran unifié)
        DeckEditorUI deckEditor = FindFirstObjectByType<DeckEditorUI>();
        if (deckEditor != null)
        {
            SerializedObject so = new SerializedObject(deckEditor);

            // Charger les prefabs
            GameObject deckListRow = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/DeckListRow.prefab");
            GameObject cardPoolItem = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/CardPoolItem.prefab");

            if (deckListRow != null)
                so.FindProperty("_deckListRowPrefab").objectReferenceValue = deckListRow;
            if (cardPoolItem != null)
                so.FindProperty("_cardPoolItemPrefab").objectReferenceValue = cardPoolItem;

            so.ApplyModifiedProperties();
            Debug.Log("DeckEditorUI configure!");
        }

        // Trouver LoadoutTabsUI (barre d'onglets de loadout, ex-DeckListUI)
        LoadoutTabsUI loadoutTabs = FindFirstObjectByType<LoadoutTabsUI>();
        if (loadoutTabs != null)
        {
            SerializedObject so = new SerializedObject(loadoutTabs);

            GameObject deckSlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/DeckSlot.prefab");
            if (deckSlot != null)
                so.FindProperty("_tabPrefab").objectReferenceValue = deckSlot;

            so.ApplyModifiedProperties();
            Debug.Log("LoadoutTabsUI configure!");
        }

        Debug.Log("Configuration de la scene terminee!");
    }

    // Helpers
    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.AddComponent<RectTransform>();
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    private static void SetAnchors(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
#endif
