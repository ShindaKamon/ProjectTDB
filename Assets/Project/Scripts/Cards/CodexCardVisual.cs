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
    Push,            // Déplacements : bond, recul, poussée, tirage (violet)
    Mute,
    Warn,            // Contreparties (orange)
    Defense,         // Armure et résistance magique (gris)
    ActionPoints,    // PA (bleu)
    MovementPoints   // PM (vert)
}

/// <summary>Pastille d'effet : icône (nom d'icône du codex), texte, famille de couleur.</summary>
public struct CardChip
{
    public string Icon;
    public string Text;
    public ChipKind Kind;
    public string Label; // Nom affiché avant l'icône (ex. « PV »), vide = aucun

    public CardChip(string icon, string text, ChipKind kind, string label = "")
    {
        Icon = icon;
        Text = text;
        Kind = kind;
        Label = label;
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
        EmotionType.Anger => Hex("#d64545"),
        EmotionType.Disgust => Hex("#9a4fbf"),
        EmotionType.Sadness => Hex("#5a6fd8"),
        EmotionType.Surprise => Hex("#4fa8e8"),
        EmotionType.Fear => Hex("#3f9d5c"),
        EmotionType.Trust => Hex("#5cc98a"),
        EmotionType.Joy => Hex("#d9a91f"),
        EmotionType.Anticipation => Hex("#e08a3a"),
        _ => Hex("#8b859e"),
    };

    /// <summary>Nom français de l'émotion (« Neutre » si aucune).</summary>
    public static string EmotionName(EmotionType emotion) => emotion switch
    {
        EmotionType.Anger => "Colère",
        EmotionType.Disgust => "Dégoût",
        EmotionType.Sadness => "Tristesse",
        EmotionType.Surprise => "Surprise",
        EmotionType.Fear => "Peur",
        EmotionType.Trust => "Confiance",
        EmotionType.Joy => "Joie",
        EmotionType.Anticipation => "Anticipation",
        _ => "Neutre",
    };

    /// <summary>Nom affiché en jeu d'une catégorie de carte.</summary>
    public static string CategoryName(CardCategory category) => category switch
    {
        CardCategory.Awakening => "Éveil",
        CardCategory.Signature => "Signature",
        _ => "Standard",
    };

    /// <summary>Nom affiché en jeu d'un type de dégâts.</summary>
    public static string DamageTypeName(DamageType type) => type == DamageType.Magical ? "Magique" : "Physique";

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
        ChipKind.Heal => Hex("#ff5c8a"),      // PV et soin : rose-rouge (comme l'orbe de vie)
        ChipKind.Shield => Hex("#5fd3e6"),    // bouclier : cyan (distinct du bleu des PA)
        ChipKind.Push => Hex("#b59cf5"),      // déplacements (bond, recul, poussée, tirage) : violet
        ChipKind.Warn => Hex("#e3b154"),      // contreparties uniquement : orange
        ChipKind.Defense => Hex("#b8b8c4"),
        ChipKind.ActionPoints => Hex("#5b8def"),
        ChipKind.MovementPoints => Hex("#9be15d"),
        _ => Hex("#9c97b5"),
    };

    // ===== Textes =====

    /// <summary>« Colère · Standard » ou « Neutre · Signature · Evan ».</summary>
    public static string Subtitle(CardData card)
    {
        string emotion = CodexCardVisual.EmotionName(card.emotionType);
        string kind = CodexCardVisual.CategoryName(card.category)
            + (card.category == CardCategory.Signature && card.signatureOwner != null ? " · " + card.signatureOwner.championName : "");
        string subtitle = emotion + " · " + kind;
        return card.costHP > 0 ? subtitle + " · " + card.costHP + " PV" : subtitle;
    }

    /// <summary>Légende du schéma, ex. « au contact · zone rayon 1 » ou « portée 1-3 ».</summary>
    public static string Caption(CardData card)
    {
        var bits = new List<string>();
        bool self = card.targetType == CardTargetType.Self;
        int radius = ZoneRadius(card);

        if (self) bits.Add(card.areaEffect == CardAreaEffect.None || card.areaEffect == CardAreaEffect.OneTile ? "sur soi" : "autour de soi");
        else if (card.isChargeCard || card.leapToTarget) bits.Add("bond " + Mathf.Max(1, card.targetRange) + " cases");
        else bits.Add(Range(card) <= 1 ? "au contact" : "portée 1-" + Range(card));

        string zone = card.areaEffect switch
        {
            CardAreaEffect.Line => "ligne",
            CardAreaEffect.Cone => "cône",
            CardAreaEffect.Cross => "croix",
            CardAreaEffect.WholeTeam => "équipe entière",
            CardAreaEffect.Circle => "zone rayon " + radius,
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
        // Zone « cercle » en 4 directions : losange de rayon n (distance de Manhattan, comme la grille)
        void Box(int x0, int y0, int n) { for (int a = -n; a <= n; a++) for (int b = -n; b <= n; b++) if (Mathf.Abs(a) + Mathf.Abs(b) <= n) Add(x0 + a, y0 + b); }
        void Team() { Add(2, 2); Add(6, 3); Add(3, 6); Add(7, 7); }

        if (card.isChargeCard || card.leapToTarget)
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

        // Portée : losange autour du lanceur (4 directions, distance de Manhattan, comme la grille)
        for (int x = 0; x < DiagramSize; x++)
            for (int y = 0; y < DiagramSize; y++)
            {
                int d = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                if (d >= 1 && d <= range) cells[x, y] = DiagramCell.Range;
            }

        foreach (var p in area) cells[p.x, p.y] = areaCell;
        cells[cx, cy] = DiagramCell.Caster;
        return cells;
    }

    // ===== Pastilles d'effets =====

    /// <summary>
    /// Pastilles des stats d'une unité, affichées sous la barre de vie du boss : ATQ, armure, résistance
    /// magique et bouclier s'ils sont non nuls, puis PA et PM (toujours, pour voir les retraits).
    /// PM/PA : ce qui reste pendant son tour ; hors de son tour, ce qu'elle aura à son prochain tour
    /// (maximum moins les retraits en attente).
    /// </summary>
    public static List<CardChip> UnitChips(Unit unit)
    {
        var chips = new List<CardChip>();
        if (unit == null) return chips;

        bool acting = Services.IsGridServiceAvailable() && Services.Grid.GetActiveUnit() == unit;
        var pending = ResourceDebuffManager.GetPending(unit);

        // Ordre : ATQ, armure (DEFP), résistance magique (DEFM), bouclier, PA, PM, Ténacité
        if (unit.GetAttack() != 0) chips.Add(new CardChip("dmg", unit.GetAttack().ToString(), ChipKind.Damage));
        if (unit.GetArmor() != 0) chips.Add(new CardChip("armor", unit.GetArmor().ToString(), ChipKind.Defense));
        if (unit.GetMagicResistance() != 0) chips.Add(new CardChip("magicresist", unit.GetMagicResistance().ToString(), ChipKind.Defense));
        if (unit.GetShield() > 0) chips.Add(new CardChip("shield", unit.GetShield().ToString(), ChipKind.Shield));

        if (unit is IActionPointsUser paUser)
        {
            int pa = acting ? paUser.GetCurrentPA() : Mathf.Max(0, paUser.GetMaxPA() - pending.pa);
            chips.Add(new CardChip("pa", pa.ToString(), ChipKind.ActionPoints));
        }
        int pm = acting ? unit.GetCurrentMovementPoints() : Mathf.Max(0, unit.GetMaxMovementPoints() - pending.pm);
        chips.Add(new CardChip("pm", pm.ToString(), ChipKind.MovementPoints));

        if (ResourceDebuffManager.IsPmImmune(unit))
        {
            chips.Add(new CardChip("lock", "tenace", ChipKind.Mute)); // Ténacité : retraits de PM ignorés à son prochain tour
        }
        return chips;
    }

    /// <summary>
    /// Pastilles de ressources de la fiche d'un champion (écran de sélection) : PV, PM, PA.
    /// </summary>
    public static List<CardChip> ChampionResourceChips(ChampionData champion)
    {
        var chips = new List<CardChip>();
        if (champion == null) return chips;

        chips.Add(new CardChip("heal", champion.maxHealth.ToString(), ChipKind.Heal, "PV"));
        chips.Add(new CardChip("pm", champion.movementRange.ToString(), ChipKind.MovementPoints, "PM"));
        chips.Add(new CardChip("pa", champion.maxActionPoints.ToString(), ChipKind.ActionPoints, "PA"));
        return chips;
    }

    /// <summary>
    /// Pastilles de combat de la fiche d'un champion (écran de sélection) : attaque, armure,
    /// résistance magique, valeurs nulles comprises.
    /// </summary>
    public static List<CardChip> ChampionChips(ChampionData champion)
    {
        var chips = new List<CardChip>();
        if (champion == null) return chips;

        chips.Add(new CardChip("dmg", champion.attackDamage.ToString(), ChipKind.Damage, "ATQ"));
        chips.Add(new CardChip("armor", champion.armor.ToString(), ChipKind.Defense, "ARM"));
        chips.Add(new CardChip("magicresist", champion.magicResistance.ToString(), ChipKind.Defense, "RM"));
        return chips;
    }

    // ===== Utilitaires =====

    internal static int Range(CardData card) => Mathf.Max(1, card.targetRange);

    static int ZoneRadius(CardData card) => card.areaEffect == CardAreaEffect.Circle ? Mathf.Max(1, card.aoeRadius) : 0;

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
