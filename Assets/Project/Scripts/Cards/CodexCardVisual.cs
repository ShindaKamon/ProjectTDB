using System.Collections.Generic;
using UnityEngine;

/// <summary>Type d'une case du schéma de portée d'une carte (façon codex émotionnel).</summary>
public enum DiagramCell
{
    Empty,
    Range,      // case à portée
    Area,       // case touchée (ennemis)
    AreaAlly,   // case touchée (alliés)
    Caster      // le lanceur
}

/// <summary>Famille de couleur d'une pastille d'effet.</summary>
public enum ChipKind
{
    Damage,
    Heal,
    Shield,
    Move,
    Push,
    Mute,
    Warn
}

/// <summary>Pastille d'effet : icône (nom d'icône du codex), texte, famille de couleur.</summary>
public struct CardChip
{
    public string Icon;
    public string Text;
    public ChipKind Kind;

    public CardChip(string icon, string text, ChipKind kind)
    {
        Icon = icon;
        Text = text;
        Kind = kind;
    }
}

/// <summary>
/// Contenu visuel d'une carte selon le design du codex émotionnel (Docs/GDD/codex_emotionnel.html) :
/// schéma de portée 9×9, légende, pastilles d'effets, sous-titre et palette. Calculé uniquement à
/// partir de CardData (la donnée du jeu fait foi), en suivant les règles de geoOf/diagram/chipsFor
/// du codex. Les ajustements faits à la main dans le codex (géométries spéciales, valeurs Excel)
/// ne sont pas repris. C# pur : testable en EditMode.
/// </summary>
public static class CodexCardVisual
{
    public const int DiagramSize = 9;
    const int CenterY = 4;

    // ===== Palette du codex (thème sombre) =====
    public static readonly Color CardBackground = Hex("#1e1c29");
    public static readonly Color CardBorder = Hex("#322e44");
    public static readonly Color Accent = Hex("#9484f0");
    public static readonly Color Ink = Hex("#ece9f7");
    public static readonly Color InkDim = Hex("#a29cbd");
    public static readonly Color GridCell = Hex("#201d2e");
    public static readonly Color GridLine = Hex("#3a3552");
    public static readonly Color GridRange = Hex("#38305e");
    public static readonly Color Foe = Hex("#ef6b6b");
    public static readonly Color Ally = Hex("#5fcf8a");
    public static readonly Color WarnBackground = Hex("#3a2f16");

    public static Color EmotionColor(EmotionType emotion) => emotion switch
    {
        EmotionType.Colere => Hex("#d64545"),
        EmotionType.Degout => Hex("#9a4fbf"),
        EmotionType.Tristesse => Hex("#5a6fd8"),
        EmotionType.Surprise => Hex("#4fa8e8"),
        EmotionType.Peur => Hex("#3f9d5c"),
        EmotionType.Confiance => Hex("#5cc98a"),
        EmotionType.Joie => Hex("#d9a91f"),
        EmotionType.Anticipation => Hex("#e08a3a"),
        _ => Hex("#8b859e"),
    };

    /// <summary>
    /// Couleur du coût d'une carte : celle de son émotion (Colère rouge, Peur vert, Joie jaune),
    /// blanc pour une Signature.
    /// </summary>
    public static Color CostColor(CardData card) =>
        card != null && card.category == CardCategory.Signature ? Color.white : EmotionColor(card != null ? card.emotionType : EmotionType.None);

    /// <summary>Texte noir sur fond clair (Joie, Signature), blanc sur fond sombre.</summary>
    public static Color ReadableTextOn(Color background)
    {
        float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
        return luminance > 0.6f ? Hex("#1e1c29") : Color.white;
    }

    public static Color ChipColor(ChipKind kind) => kind switch
    {
        ChipKind.Damage => Hex("#f08383"),
        ChipKind.Heal => Hex("#6fd49a"),
        ChipKind.Shield => Hex("#7fb0f0"),
        ChipKind.Move => Hex("#e3b154"),
        ChipKind.Push => Hex("#b59cf5"),
        ChipKind.Warn => Hex("#e3b154"),
        _ => Hex("#9c97b5"),
    };

    // ===== Textes =====

    /// <summary>« Colère · Standard » ou « Neutre · Signature · Evan ».</summary>
    public static string Subtitle(CardData card)
    {
        string emotion = CardVisualHelper.GetEmotionName(card.emotionType);
        string kind = card.category switch
        {
            CardCategory.Signature => "Signature" + (card.signatureOwner != null ? " · " + card.signatureOwner.championName : ""),
            CardCategory.Eveil => "Éveil",
            _ => "Standard",
        };
        string subtitle = emotion + " · " + kind;
        return card.costHP > 0 ? subtitle + " · " + card.costHP + " PV" : subtitle;
    }

    /// <summary>Légende du schéma, ex. « au contact · zone 3×3 » ou « portée 1-3 ».</summary>
    public static string Caption(CardData card)
    {
        var bits = new List<string>();
        bool self = card.targetType == CardTargetType.Self;
        int radius = ZoneRadius(card);

        if (self) bits.Add(card.areaEffect == CardAreaEffect.None || card.areaEffect == CardAreaEffect.OneTile ? "sur soi" : "autour de soi");
        else if (card.isChargeCard) bits.Add("bond " + Mathf.Max(1, card.targetRange) + " cases");
        else bits.Add(Range(card) <= 1 ? "au contact" : "portée 1-" + Range(card));

        string zone = card.areaEffect switch
        {
            CardAreaEffect.Line => "ligne",
            CardAreaEffect.Cone => "cône",
            CardAreaEffect.Cross => "croix",
            CardAreaEffect.WholeTeam => "équipe entière",
            CardAreaEffect.Circle => "zone " + (2 * radius + 1) + "×" + (2 * radius + 1),
            _ => "",
        };
        if (zone != "") bits.Add(zone);

        return string.Join(" · ", bits);
    }

    // ===== Schéma de portée =====

    /// <summary>
    /// Schéma 9×9 [x, y] : le lanceur à gauche (ou au centre pour une carte sur soi), sa portée
    /// (carré, comme le codex), la zone touchée autour de la cible, en rouge (ennemis) ou vert (alliés).
    /// </summary>
    public static DiagramCell[,] BuildDiagram(CardData card)
    {
        var cells = new DiagramCell[DiagramSize, DiagramSize];
        bool self = card.targetType == CardTargetType.Self;
        bool ally = card.targetType == CardTargetType.Ally || card.targetType == CardTargetType.AllyOrSelf
                    || card.affectedTarget == CardAffectedTarget.Ally || card.affectedTarget == CardAffectedTarget.AllyOrSelf;
        DiagramCell areaCell = ally ? DiagramCell.AreaAlly : DiagramCell.Area;

        int range = self ? 0 : Range(card);
        int radius = ZoneRadius(card);
        int cx = self ? CenterY : (range >= 5 ? 0 : 1);
        int cy = CenterY;

        var area = new List<Vector2Int>();
        void Add(int x, int y) { if (x >= 0 && x < DiagramSize && y >= 0 && y < DiagramSize) area.Add(new Vector2Int(x, y)); }
        void Box(int x0, int y0, int n) { for (int a = -n; a <= n; a++) for (int b = -n; b <= n; b++) Add(x0 + a, y0 + b); }
        void Team() { Add(2, 2); Add(6, 3); Add(3, 6); Add(7, 7); }

        if (card.isChargeCard)
        {
            // Bond : zone autour du point d'arrivée, pas de portée affichée
            int landing = Mathf.Min(cx + 3, DiagramSize - 1);
            if (card.areaEffect == CardAreaEffect.Circle) Box(landing, cy, radius); else Add(landing, cy);
            range = 0;
        }
        else if (self)
        {
            if (card.areaEffect == CardAreaEffect.WholeTeam) Team();
            else if (card.areaEffect == CardAreaEffect.Circle) Box(cx, cy, radius);
            else Add(cx, cy);
        }
        else
        {
            int tx = Mathf.Min(cx + Mathf.Max(range, 1), DiagramSize - 1);
            int ty = cy;
            switch (card.areaEffect)
            {
                case CardAreaEffect.Circle: Box(tx, ty, radius); break;
                case CardAreaEffect.Line: for (int k = 0; k < 3; k++) Add(tx + k, ty); break;
                case CardAreaEffect.Cone: Add(tx, ty); Add(tx + 1, ty - 1); Add(tx + 1, ty + 1); Add(tx + 1, ty); break;
                case CardAreaEffect.Cross: Add(tx, ty); Add(tx - 1, ty); Add(tx + 1, ty); Add(tx, ty - 1); Add(tx, ty + 1); break;
                case CardAreaEffect.WholeTeam: Team(); break;
                default:
                    Add(tx, ty);
                    if (card.targetCount >= 2) Add(tx, ty - 2); // plusieurs cibles : une 2e case touchée
                    break;
            }
        }

        // Portée : carré autour du lanceur (distance de Tchebychev, comme le schéma du codex)
        for (int x = 0; x < DiagramSize; x++)
            for (int y = 0; y < DiagramSize; y++)
            {
                int d = Mathf.Max(Mathf.Abs(x - cx), Mathf.Abs(y - cy));
                if (d >= 1 && d <= range) cells[x, y] = DiagramCell.Range;
            }

        foreach (var p in area) cells[p.x, p.y] = areaCell;
        cells[cx, cy] = DiagramCell.Caster;
        return cells;
    }

    // ===== Pastilles d'effets =====

    public static List<CardChip> Chips(CardData card)
    {
        var chips = new List<CardChip>();
        if (card.damageAmount > 0) chips.Add(new CardChip("dmg", card.damageAmount.ToString(), ChipKind.Damage));
        if (card.healAmount > 0) chips.Add(new CardChip("heal", card.healAmount.ToString(), ChipKind.Heal));
        if (card.defenseAmount > 0) chips.Add(new CardChip("shield", card.defenseAmount.ToString(), ChipKind.Shield));
        if (card.atkIncreased > 0) chips.Add(new CardChip("buff", "+" + card.atkIncreased, ChipKind.Shield));
        if (card.lifestealFixedAmount > 0) chips.Add(new CardChip("drain", "vol de vie", ChipKind.Heal));

        if (card.pmReduction >= 5) chips.Add(new CardChip("lock", "tous les PM", ChipKind.Move));
        else if (card.pmReduction > 0) chips.Add(new CardChip("pm", "−" + card.pmReduction + " PM", ChipKind.Move));
        if (card.paReduction > 0) chips.Add(new CardChip("pa", "−" + card.paReduction + " PA", ChipKind.Move));

        if (card.knockbackDistance > 0)
            chips.Add(card.pullsTowardCaster
                ? new CardChip("pull", card.knockbackDistance.ToString(), ChipKind.Push)
                : new CardChip("push", card.knockbackDistance.ToString(), ChipKind.Push));

        if (card.isChargeCard) chips.Add(new CardChip("bond", "bond", ChipKind.Move));
        if (card.scalesWithPASpentThisTurn && card.comboDamagePerPASpent > 0)
            chips.Add(new CardChip("pa", "+" + card.comboDamagePerPASpent + "/PA", ChipKind.Move));
        if (card.isSummonCard) chips.Add(new CardChip("summon", "invoque", ChipKind.Mute));
        if (card.isRepositionSummonCard) chips.Add(new CardChip("summon", "déplace", ChipKind.Mute));
        if (card.targetsHandCard) chips.Add(new CardChip("hand", "carte en main", ChipKind.Mute));
        if (card.damageSelf > 0) chips.Add(new CardChip("self", "−" + card.damageSelf, ChipKind.Warn));
        return chips;
    }

    // ===== Utilitaires =====

    static int Range(CardData card) => Mathf.Max(1, card.targetRange);

    static int ZoneRadius(CardData card) => card.areaEffect == CardAreaEffect.Circle ? Mathf.Max(1, card.aoeRadius) : 0;

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
