using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Construit les pastilles d'effets (icône du codex + valeur) dans un conteneur : cartes du pool
/// de l'éditeur, cartes en main, aperçu de la carte ennemie, stats du boss.
/// Les icônes sont les sprites de l'atlas Resources/CodexIcons/CodexIcons (un sprite par nom : « dmg », « heal »…),
/// le même atlas que le Sprite Asset TextMeshPro du texte des cartes.
/// Le conteneur porte sa propre mise en page (VerticalLayoutGroup, HorizontalLayoutGroup…).
/// </summary>
public static class CardChipsView
{
    private const string IconsAtlas = "CodexIcons/CodexIcons";
    private static Dictionary<string, Sprite> _iconsByName;

    /// <summary>Remplace les pastilles du conteneur par celles données.</summary>
    public static void Build(Transform container, IEnumerable<CardChip> chips, Sprite background,
        TMP_FontAsset font = null, float fontSize = 12f, float iconSize = 14f)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            child.SetActive(false); // Destroy est différé : on masque tout de suite l'ancienne pastille
            Object.Destroy(child);
        }

        foreach (CardChip chip in chips)
            CreateChip(container, chip, background, font, fontSize, iconSize);
    }

    public static bool TryGetIcon(string name, out Sprite sprite)
    {
        if (_iconsByName == null)
        {
            _iconsByName = new Dictionary<string, Sprite>();
            foreach (Sprite icon in Resources.LoadAll<Sprite>(IconsAtlas))
                _iconsByName[icon.name] = icon;
        }
        return _iconsByName.TryGetValue(name, out sprite);
    }

    /// <summary>Pastille : fond arrondi, icône colorée selon l'effet, valeur (ex. « ↗ 33 »).</summary>
    private static void CreateChip(Transform container, CardChip chip, Sprite background,
        TMP_FontAsset font, float fontSize, float iconSize)
    {
        bool warn = chip.Kind == ChipKind.Warn;
        Color accent = CodexCardVisual.ChipColor(chip.Kind);

        var go = new GameObject("Chip_" + chip.Icon, typeof(RectTransform));
        go.transform.SetParent(container, false);

        var bg = go.AddComponent<Image>();
        bg.sprite = background;
        bg.type = Image.Type.Sliced;
        bg.color = warn ? CodexCardVisual.WarnBackground : CodexCardVisual.CardBorder;
        bg.raycastTarget = false;

        var layout = go.AddComponent<HorizontalLayoutGroup>();
        // Marges proportionnelles à l'icône (icône 14 → 5, 7, 2, 2, le réglage d'origine du pool)
        int padY = Mathf.RoundToInt(iconSize * 0.15f);
        layout.padding = new RectOffset(Mathf.RoundToInt(iconSize * 0.35f), Mathf.RoundToInt(iconSize * 0.5f), padY, padY);
        layout.spacing = Mathf.Max(4, Mathf.RoundToInt(iconSize * 0.3f));
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Nom de la stat avant l'icône (ex. « PV »), en encre atténuée
        if (!string.IsNullOrEmpty(chip.Label))
        {
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = chip.Label;
            label.fontSize = fontSize * 0.75f;
            label.color = CodexCardVisual.InkDim;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
        }

        if (TryGetIcon(chip.Icon, out Sprite sprite))
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(go.transform, false);
            var icon = iconGO.AddComponent<Image>();
            icon.sprite = sprite;
            icon.color = accent;
            icon.raycastTarget = false;
            var iconLayout = iconGO.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.minWidth = iconSize;
            iconLayout.preferredHeight = iconLayout.minHeight = iconSize;
        }

        if (!string.IsNullOrEmpty(chip.Text))
        {
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = chip.Text;
            text.fontSize = fontSize;
            text.color = warn ? accent : CodexCardVisual.Ink;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
        }
    }
}
