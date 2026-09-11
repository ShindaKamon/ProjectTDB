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

        CreateCardGridItemPrefab();
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

        if (GUILayout.Button("CardGridItem (grille de deck)"))
            CreateCardGridItemPrefab();

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
    /// Cree le prefab CardGridItem pour la grille de deck (mode visualisation)
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/CardGridItem")]
    public static void CreateCardGridItemPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root
        GameObject root = new GameObject("CardGridItem");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(100, 140);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(0.15f, 0.15f, 0.2f);

        DeckGridCardUI cardUI = root.AddComponent<DeckGridCardUI>();

        // Card Frame (fond)
        GameObject frame = CreateChild(root, "CardFrame");
        Image frameImg = frame.AddComponent<Image>();
        frameImg.color = new Color(0.1f, 0.1f, 0.15f);
        SetAnchors(frame, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));

        // Family Border (bordure coloree)
        GameObject border = CreateChild(root, "FamilyBorder");
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = Color.red;
        SetAnchors(border, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Outline borderOutline = border.AddComponent<Outline>();
        borderOutline.effectColor = Color.red;
        borderOutline.effectDistance = new Vector2(2, -2);

        // Card Image (artwork)
        GameObject cardImage = CreateChild(root, "CardImage");
        Image cardImg = cardImage.AddComponent<Image>();
        cardImg.color = new Color(0.3f, 0.3f, 0.3f);
        SetAnchors(cardImage, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);

        // Name Background
        GameObject nameBg = CreateChild(root, "NameBackground");
        Image nameBgImg = nameBg.AddComponent<Image>();
        nameBgImg.color = new Color(0, 0, 0, 0.7f);
        SetAnchors(nameBg, new Vector2(0, 0), new Vector2(1, 0.25f), Vector2.zero, Vector2.zero);

        // Card Name Text
        GameObject nameText = CreateChild(nameBg, "CardNameText");
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nom Carte";
        nameTMP.fontSize = 12;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        SetAnchors(nameText, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));

        // Cost Badge (coin haut gauche)
        GameObject costBadge = CreateChild(root, "CostBadge");
        Image costBadgeImg = costBadge.AddComponent<Image>();
        costBadgeImg.color = new Color(0.9f, 0.7f, 0.2f);
        RectTransform costBadgeRT = costBadge.GetComponent<RectTransform>();
        costBadgeRT.anchorMin = new Vector2(0, 1);
        costBadgeRT.anchorMax = new Vector2(0, 1);
        costBadgeRT.pivot = new Vector2(0, 1);
        costBadgeRT.anchoredPosition = new Vector2(5, -5);
        costBadgeRT.sizeDelta = new Vector2(24, 24);

        // Cost Text
        GameObject costText = CreateChild(costBadge, "CostText");
        TextMeshProUGUI costTMP = costText.AddComponent<TextMeshProUGUI>();
        costTMP.text = "2";
        costTMP.fontSize = 14;
        costTMP.fontStyle = FontStyles.Bold;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.color = Color.white;
        SetAnchors(costText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Count Badge (coin bas droite)
        GameObject countBadge = CreateChild(root, "CountBadge");
        Image countBadgeImg = countBadge.AddComponent<Image>();
        countBadgeImg.color = new Color(0.2f, 0.2f, 0.8f);
        RectTransform countBadgeRT = countBadge.GetComponent<RectTransform>();
        countBadgeRT.anchorMin = new Vector2(1, 0);
        countBadgeRT.anchorMax = new Vector2(1, 0);
        countBadgeRT.pivot = new Vector2(1, 0);
        countBadgeRT.anchoredPosition = new Vector2(-5, 5);
        countBadgeRT.sizeDelta = new Vector2(28, 20);

        // Count Text
        GameObject countText = CreateChild(countBadge, "CountText");
        TextMeshProUGUI countTMP = countText.AddComponent<TextMeshProUGUI>();
        countTMP.text = "x2";
        countTMP.fontSize = 12;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.alignment = TextAlignmentOptions.Center;
        countTMP.color = Color.white;
        SetAnchors(countText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Assigner les references au script
        SerializedObject so = new SerializedObject(cardUI);
        so.FindProperty("_cardFrame").objectReferenceValue = frameImg;
        so.FindProperty("_cardImage").objectReferenceValue = cardImg;
        so.FindProperty("_familyBorder").objectReferenceValue = borderImg;
        so.FindProperty("_costBadge").objectReferenceValue = costBadgeImg;
        so.FindProperty("_costText").objectReferenceValue = costTMP;
        so.FindProperty("_countBadge").objectReferenceValue = countBadgeImg;
        so.FindProperty("_countText").objectReferenceValue = countTMP;
        so.FindProperty("_cardNameText").objectReferenceValue = nameTMP;
        so.FindProperty("_nameBackground").objectReferenceValue = nameBgImg;
        so.ApplyModifiedProperties();

        // Sauvegarder le prefab
        string path = $"{PrefabPath}/CardGridItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
    }

    /// <summary>
    /// Cree le prefab DeckSlot pour la liste des decks
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/DeckSlot")]
    public static void CreateDeckSlotPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root
        GameObject root = new GameObject("DeckSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(120, 80);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(0.8f, 0.2f, 0.2f);

        DeckSlotUI slotUI = root.AddComponent<DeckSlotUI>();

        // Background 2 (gradient/seconde couleur)
        GameObject bg2 = CreateChild(root, "Background2");
        Image bg2Img = bg2.AddComponent<Image>();
        bg2Img.color = new Color(0.6f, 0.15f, 0.15f);
        SetAnchors(bg2, new Vector2(0.5f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        // Selection Border
        GameObject border = CreateChild(root, "SelectionBorder");
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.3f, 0.3f, 0.3f);
        borderImg.type = Image.Type.Sliced;
        SetAnchors(border, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Outline borderOutline = border.AddComponent<Outline>();
        borderOutline.effectColor = new Color(0.3f, 0.3f, 0.3f);
        borderOutline.effectDistance = new Vector2(3, -3);

        // Default Icon (pour le deck de base)
        GameObject icon = CreateChild(root, "DefaultIcon");
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = new Color(1f, 0.84f, 0f); // Or
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(1, 1);
        iconRT.anchorMax = new Vector2(1, 1);
        iconRT.pivot = new Vector2(1, 1);
        iconRT.anchoredPosition = new Vector2(-5, -5);
        iconRT.sizeDelta = new Vector2(16, 16);
        icon.SetActive(false);

        // Deck Name Text
        GameObject nameText = CreateChild(root, "DeckNameText");
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nom du Deck";
        nameTMP.fontSize = 14;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        SetAnchors(nameText, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero);

        // Card Count Text
        GameObject countText = CreateChild(root, "CardCountText");
        TextMeshProUGUI countTMP = countText.AddComponent<TextMeshProUGUI>();
        countTMP.text = "10 cartes";
        countTMP.fontSize = 11;
        countTMP.alignment = TextAlignmentOptions.Center;
        countTMP.color = new Color(0.8f, 0.8f, 0.8f);
        SetAnchors(countText, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.4f), Vector2.zero, Vector2.zero);

        // Assigner les references
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("_backgroundImage").objectReferenceValue = rootImage;
        so.FindProperty("_backgroundImage2").objectReferenceValue = bg2Img;
        so.FindProperty("_selectionBorder").objectReferenceValue = borderImg;
        so.FindProperty("_defaultIcon").objectReferenceValue = iconImg;
        so.FindProperty("_deckNameText").objectReferenceValue = nameTMP;
        so.FindProperty("_cardCountText").objectReferenceValue = countTMP;
        so.ApplyModifiedProperties();

        // Sauvegarder
        string path = $"{PrefabPath}/DeckSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
    }

    /// <summary>
    /// Cree le prefab CardPoolItem pour le pool d'edition
    /// </summary>
    [MenuItem("Tools/UI/Prefabs/CardPoolItem")]
    public static void CreateCardPoolItemPrefab()
    {
        EnsureFolderExists(PrefabPath);

        // Root
        GameObject root = new GameObject("CardPoolItem");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(200, 60);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(0.15f, 0.15f, 0.2f);

        CardPoolItemUI poolUI = root.AddComponent<CardPoolItemUI>();

        // Family Border (barre coloree a gauche)
        GameObject border = CreateChild(root, "FamilyBorder");
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = Color.red;
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = new Vector2(0, 1);
        borderRT.pivot = new Vector2(0, 0.5f);
        borderRT.anchoredPosition = Vector2.zero;
        borderRT.sizeDelta = new Vector2(4, 0);

        // Card Icon
        GameObject icon = CreateChild(root, "CardIcon");
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.1f);
        iconRT.anchorMax = new Vector2(0, 0.9f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(10, 0);
        iconRT.sizeDelta = new Vector2(45, 0);

        // Cost Badge
        GameObject costBadge = CreateChild(root, "CostBadge");
        Image costBadgeImg = costBadge.AddComponent<Image>();
        costBadgeImg.color = new Color(0.9f, 0.7f, 0.2f);
        RectTransform costBadgeRT = costBadge.GetComponent<RectTransform>();
        costBadgeRT.anchorMin = new Vector2(0, 0.5f);
        costBadgeRT.anchorMax = new Vector2(0, 0.5f);
        costBadgeRT.pivot = new Vector2(0, 0.5f);
        costBadgeRT.anchoredPosition = new Vector2(60, 0);
        costBadgeRT.sizeDelta = new Vector2(24, 24);

        // Cost Text
        GameObject costText = CreateChild(costBadge, "CostText");
        TextMeshProUGUI costTMP = costText.AddComponent<TextMeshProUGUI>();
        costTMP.text = "2";
        costTMP.fontSize = 14;
        costTMP.fontStyle = FontStyles.Bold;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.color = Color.white;
        SetAnchors(costText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Card Name
        GameObject nameText = CreateChild(root, "CardNameText");
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nom de la Carte";
        nameTMP.fontSize = 14;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Left;
        nameTMP.color = Color.white;
        RectTransform nameRT = nameText.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.5f);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.pivot = new Vector2(0, 0.5f);
        nameRT.anchoredPosition = new Vector2(90, 0);
        nameRT.sizeDelta = new Vector2(-100, 0);

        // Description
        GameObject descText = CreateChild(root, "CardDescriptionText");
        TextMeshProUGUI descTMP = descText.AddComponent<TextMeshProUGUI>();
        descTMP.text = "Description de la carte...";
        descTMP.fontSize = 10;
        descTMP.alignment = TextAlignmentOptions.Left;
        descTMP.color = new Color(0.7f, 0.7f, 0.7f);
        descTMP.enableWordWrapping = true;
        descTMP.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform descRT = descText.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0);
        descRT.anchorMax = new Vector2(1, 0.5f);
        descRT.pivot = new Vector2(0, 0.5f);
        descRT.anchoredPosition = new Vector2(90, 0);
        descRT.sizeDelta = new Vector2(-100, 0);

        // Assigner les references
        SerializedObject so = new SerializedObject(poolUI);
        so.FindProperty("_backgroundImage").objectReferenceValue = rootImage;
        so.FindProperty("_familyBorder").objectReferenceValue = borderImg;
        so.FindProperty("_cardIcon").objectReferenceValue = iconImg;
        so.FindProperty("_costBadge").objectReferenceValue = costBadgeImg;
        so.FindProperty("_cardCostText").objectReferenceValue = costTMP;
        so.FindProperty("_cardNameText").objectReferenceValue = nameTMP;
        so.FindProperty("_cardDescriptionText").objectReferenceValue = descTMP;
        so.ApplyModifiedProperties();

        // Sauvegarder
        string path = $"{PrefabPath}/CardPoolItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log($"Prefab cree: {path}");
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
        // Trouver DeckEditorUI
        DeckEditorUI deckEditor = FindFirstObjectByType<DeckEditorUI>();
        if (deckEditor != null)
        {
            SerializedObject so = new SerializedObject(deckEditor);

            // Charger les prefabs
            GameObject cardGridItem = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/CardGridItem.prefab");
            GameObject cardPoolItem = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/CardPoolItem.prefab");
            GameObject deckCardSlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/DeckCardSlot.prefab");

            if (cardGridItem != null)
                so.FindProperty("_cardGridItemPrefab").objectReferenceValue = cardGridItem;
            if (cardPoolItem != null)
                so.FindProperty("_cardPoolItemPrefab").objectReferenceValue = cardPoolItem;
            if (deckCardSlot != null)
                so.FindProperty("_deckCardSlotPrefab").objectReferenceValue = deckCardSlot;

            so.ApplyModifiedProperties();
            Debug.Log("DeckEditorUI configure!");
        }

        // Trouver DeckListUI
        DeckListUI deckList = FindFirstObjectByType<DeckListUI>();
        if (deckList != null)
        {
            SerializedObject so = new SerializedObject(deckList);

            GameObject deckSlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/DeckSlot.prefab");
            if (deckSlot != null)
                so.FindProperty("_deckSlotPrefab").objectReferenceValue = deckSlot;

            so.ApplyModifiedProperties();
            Debug.Log("DeckListUI configure!");
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
