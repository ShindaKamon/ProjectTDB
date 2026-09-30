using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pastille de portrait d'une unité, construite en code : cadre coloré (allié / ennemi / tour en cours),
/// portrait du champion s'il existe (sinon l'initiale du nom) et, en option, une mini-jauge de PV dessous.
/// Partagée par la frise des tours et le panneau d'équipe.
/// </summary>
public class UnitPortraitView
{
    public static readonly Color AllyFrame = new Color(0.55f, 0.85f, 1f);
    public static readonly Color EnemyFrame = new Color(1f, 0.55f, 0.5f);
    public static readonly Color ActiveFrame = new Color(1f, 0.85f, 0.3f);
    private static readonly Color BackColor = new Color(0.118f, 0.110f, 0.161f);
    private static readonly Color HpBack = new Color(0.05f, 0.05f, 0.07f);
    private static readonly Color HpColor = new Color(0.86f, 0.30f, 0.30f);

    private const float Border = 3f;
    private const float HpBarHeight = 6f;

    public RectTransform Root { get; private set; }

    private Image _frame;
    private Image _face;
    private TextMeshProUGUI _initial;
    private RectTransform _hpFill;
    private Image _hpFillImage;

    /// <summary>Hauteur totale d'une pastille de cette taille (jauge de PV comprise).</summary>
    public static float TotalHeight(float size, bool withHpBar) => withHpBar ? size + 3f + HpBarHeight : size;

    public static UnitPortraitView Create(Transform parent, float size, Sprite roundedSprite, bool withHpBar)
    {
        var view = new UnitPortraitView();
        var root = new GameObject("Portrait", typeof(RectTransform));
        view.Root = (RectTransform)root.transform;
        view.Root.SetParent(parent, false);
        view.Root.sizeDelta = new Vector2(size, TotalHeight(size, withHpBar));

        view._frame = NewImage("Frame", view.Root, roundedSprite);
        Anchor(view._frame.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -size), Vector2.zero);

        Image back = NewImage("Back", view._frame.transform, roundedSprite);
        back.color = BackColor;
        Stretch(back.rectTransform, Border);

        view._face = NewImage("Face", back.transform, null);
        view._face.preserveAspect = true;
        Stretch(view._face.rectTransform, 0f);

        var initialGo = new GameObject("Initial", typeof(RectTransform));
        initialGo.transform.SetParent(back.transform, false);
        view._initial = initialGo.AddComponent<TextMeshProUGUI>();
        view._initial.alignment = TextAlignmentOptions.Center;
        view._initial.fontStyle = FontStyles.Bold;
        view._initial.fontSize = size * 0.55f;
        view._initial.raycastTarget = false;
        Stretch(view._initial.rectTransform, 0f);

        if (withHpBar)
        {
            Image hpBack = NewImage("HpBack", view.Root, null);
            hpBack.color = HpBack;
            Anchor(hpBack.rectTransform, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, HpBarHeight));

            view._hpFillImage = NewImage("HpFill", hpBack.transform, null);
            view._hpFillImage.color = HpColor;
            view._hpFill = view._hpFillImage.rectTransform;
            Stretch(view._hpFill, 1f);
        }
        return view;
    }

    /// <summary>Affiche l'unité : portrait ou initiale, couleur du cadre (or si c'est son tour), PV.</summary>
    public void Set(Unit unit, bool active)
    {
        bool ally = unit.GetFaction() == Unit.UnitFaction.Player;
        Color side = ally ? AllyFrame : EnemyFrame;
        _frame.color = active ? ActiveFrame : side;

        Sprite portrait = unit is Champion champion && champion.championData != null ? champion.championData.portrait : null;
        _face.sprite = portrait;
        _face.enabled = portrait != null;
        _initial.enabled = portrait == null;
        _initial.text = string.IsNullOrEmpty(unit.DisplayName) ? "?" : unit.DisplayName.Substring(0, 1).ToUpperInvariant();
        _initial.color = side;

        if (_hpFill != null)
        {
            float ratio = unit.GetMaxHealth() > 0 ? Mathf.Clamp01((float)unit.GetHealth() / unit.GetMaxHealth()) : 0f;
            _hpFill.anchorMax = new Vector2(ratio, 1f);
            _hpFillImage.color = unit.GetShield() > 0 ? CodexCardVisual.ChipColor(ChipKind.Shield) : HpColor;
        }
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform r, float inset)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(inset, inset);
        r.offsetMax = new Vector2(-inset, -inset);
    }

    private static void Anchor(RectTransform r, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        r.anchorMin = min;
        r.anchorMax = max;
        r.offsetMin = offsetMin;
        r.offsetMax = offsetMax;
    }
}
