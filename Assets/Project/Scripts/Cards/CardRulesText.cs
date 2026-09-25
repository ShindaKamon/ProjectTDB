using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Texte des cartes généré depuis leurs champs (TextMeshPro, icônes en &lt;sprite&gt;) : une ligne
/// par effet, puis la cible, la zone, la contrepartie et la règle spéciale. Affiché par CardTextView.
/// </summary>
public static class CardRulesText
{
    /// <summary>Sprite Asset TextMeshPro des icônes du codex (Resources/CodexIcons).</summary>
    public const string IconSpriteAsset = "CodexIcons/CodexIcons";

    /// <summary>
    /// Texte complet d'une carte, une ligne par élément.
    /// Ex. « ↗ Inflige 33 / Cible : 1 ennemi · au contact / Zone : cercle de 1 (ennemis) ».
    /// </summary>
    public static string Build(CardData card)
    {
        var lines = new List<string>();
        void Effect(string icon, ChipKind kind, string text) => lines.Add(Icon(icon, kind) + " " + text);
        string Turns() => card.effectDuration > 0 ? $" ({card.effectDuration} tour{(card.effectDuration > 1 ? "s" : "")})" : "";

        if (card.damageAmount > 0)
            Effect(card.damageType == DamageType.Magical ? "magic" : "dmg", ChipKind.Damage,
                "Inflige " + card.damageAmount + (card.damageType == DamageType.Magical ? " (magique)" : ""));
        if (card.scalesWithPASpentThisTurn && card.comboDamagePerPASpent > 0)
            Effect("dmg", ChipKind.Damage, $"+{card.comboDamagePerPASpent} dégâts par PA déjà dépensé ce tour");
        // Carte d'invocation : le soin ne sert que si l'invocation est déjà sur le terrain (voir ExecuteEffect)
        if (card.healAmount > 0 && !card.isSummonCard) Effect("heal", ChipKind.Heal, "Soigne " + card.healAmount);
        if (card.lifestealFixedAmount > 0) Effect("drain", ChipKind.Heal, "Vol de vie " + card.lifestealFixedAmount);
        if (card.damageAroundTarget > 0)
            Effect(card.damageType == DamageType.Magical ? "magic" : "dmg", ChipKind.Damage, $"Inflige {card.damageAroundTarget} aux ennemis au contact de la cible");
        if (card.shieldAmount > 0)
            Effect("shield", ChipKind.Shield, "Bouclier " + card.shieldAmount + (card.reactiveShield ? " au premier coup ennemi reçu" : ""));
        if (card.nextAttackBonus > 0) Effect("buff", ChipKind.Damage, $"+{card.nextAttackBonus} dégâts sur la prochaine carte offensive");
        if (card.armorAmount != 0) Effect("armor", ChipKind.Defense, "Armure " + card.armorAmount.ToString("+#;−#") + Turns());
        if (card.magicResistanceAmount != 0) Effect("magicresist", ChipKind.Defense, "Résistance magique " + card.magicResistanceAmount.ToString("+#;−#") + Turns());
        if (card.removeAllMovement) Effect("lock", ChipKind.MovementPoints, "Retire tous les PM au prochain tour");
        else if (card.pmReduction > 0) Effect("pm", ChipKind.MovementPoints, $"Retire {card.pmReduction} PM au prochain tour");
        if (card.paReduction > 0) Effect("pa", ChipKind.ActionPoints, $"Retire {card.paReduction} PA au prochain tour");
        if (card.knockbackDistance > 0)
            Effect(card.pullsTowardCaster ? "pull" : "push", ChipKind.Push,
                (card.pullsTowardCaster ? "Tire de " : "Repousse de ") + Cases(card.knockbackDistance));
        if (card.isChargeCard) Effect("bond", ChipKind.Push, "Bond jusqu'à " + Cases(CodexCardVisual.Range(card)));
        if (card.isSummonCard)
        {
            Effect("summon", ChipKind.Mute, "Invoque " + (card.summonPrefab != null ? SummonName(card.summonPrefab.name) : "une invocation"));
            if (card.healAmount > 0) Effect("heal", ChipKind.Heal, "Si déjà présente : soigne " + card.healAmount);
        }
        if (card.isRepositionSummonCard) Effect("summon", ChipKind.Mute, "Déplace ton invocation");
        if (card.targetsHandCard) Effect("hand", ChipKind.Mute, "Cible une carte de ta main");
        if (card.drawAmount > 0) Effect("hand", ChipKind.Mute, "Pioche " + card.drawAmount);
        if (card.casterMovementGain > 0) Effect("pm", ChipKind.MovementPoints, $"+{card.casterMovementGain} PM ce tour");
        if (card.casterActionGain > 0) Effect("pa", ChipKind.ActionPoints, $"+{card.casterActionGain} PA ce tour");
        if (card.casterArmorAmount > 0) Effect("armor", ChipKind.Defense, $"Ton armure +{card.casterArmorAmount}" + CasterTurns(card));
        if (card.casterRetreat > 0) Effect("push", ChipKind.Push, "Tu recules de " + Cases(card.casterRetreat));

        // Carte sur soi avec une zone : « Zone : autour de toi, … » suffit
        bool selfZone = card.targetType == CardTargetType.Self && ZoneText(card) != "";
        string target = selfZone ? "" : TargetText(card);
        if (target != "") lines.Add("<b>Cible :</b> " + target);

        string zone = ZoneText(card);
        if (zone != "") lines.Add("<b>Zone :</b> " + (selfZone && card.areaEffect != CardAreaEffect.WholeTeam ? "autour de toi, " : "") + zone);

        // Contreparties, en couleur d'avertissement
        void Warn(string icon, string text) =>
            lines.Add($"{Icon(icon, ChipKind.Warn)} <color=#{ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(ChipKind.Warn))}>{text}</color>");

        if (card.damageSelf > 0) Warn("self", $"Contrecoup : tu subis {card.damageSelf}");
        if (card.casterArmorAmount < 0) Warn("armor", $"Contrecoup : ton armure {card.casterArmorAmount.ToString("+#;−#")}" + CasterTurns(card));
        if (card.casterMovementLoss > 0) Warn("pm", $"Contrecoup : tu perds {card.casterMovementLoss} PM au prochain tour");

        if (!string.IsNullOrWhiteSpace(card.specialText))
            lines.Add("<i>Spécial :</i> " + card.specialText.Trim());

        return string.Join("\n", lines);
    }

    static string Icon(string name, ChipKind kind) =>
        $"<sprite name=\"{name}\" color=#{ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(kind))}>";

    static string Cases(int n) => n + (n > 1 ? " cases" : " case");

    // Durée d'un effet sur le lanceur (au moins 1 tour : jusqu'à son prochain tour)
    static string CasterTurns(CardData card)
    {
        int turns = Mathf.Max(1, card.effectDuration);
        return $" ({turns} tour{(turns > 1 ? "s" : "")})";
    }

    // « Lyse_Summon » → « Lyse »
    static string SummonName(string prefabName) => prefabName.Split('_')[0];

    static string TargetText(CardData card)
    {
        if (card.targetsHandCard) return "";

        int n = Mathf.Max(1, card.targetCount);
        string who = card.targetType switch
        {
            CardTargetType.Self => "soi",
            CardTargetType.Enemy => n > 1 ? $"{n} ennemis distincts" : "1 ennemi",
            CardTargetType.Ally => n > 1 ? $"{n} alliés distincts" : "1 allié",
            CardTargetType.AllyOrSelf => n > 1 ? $"{n} alliés distincts (toi compris)" : "toi ou 1 allié",
            CardTargetType.AllyorEnemy => "1 autre unité",
            CardTargetType.AnyUnit => "1 unité",
            CardTargetType.EmptyTile => "1 case vide",
            CardTargetType.AnyTile => "1 case",
            CardTargetType.EnemyOrTile => "1 ennemi ou 1 case",
            _ => "",
        };
        if (who == "" || card.targetType == CardTargetType.Self) return who;
        if (card.isChargeCard) return who + " en ligne droite";
        return who + " · " + (CodexCardVisual.Range(card) <= 1 ? "au contact" : "portée 1-" + CodexCardVisual.Range(card));
    }

    static string ZoneText(CardData card)
    {
        int r = Mathf.Max(1, card.aoeRadius);
        string shape = card.areaEffect switch
        {
            CardAreaEffect.Circle => "cercle de " + r,
            CardAreaEffect.Cross => "croix de " + r,
            CardAreaEffect.Line => "ligne de " + Cases(r),
            CardAreaEffect.Cone => "cône de " + r,
            CardAreaEffect.WholeTeam => "toute l'équipe",
            _ => "",
        };
        if (shape == "") return "";

        string affected = card.affectedTarget switch
        {
            CardAffectedTarget.Enemies => "ennemis",
            CardAffectedTarget.Ally => "alliés",
            CardAffectedTarget.AllyOrSelf => "toi et tes alliés",
            CardAffectedTarget.AllyorEnemy => "alliés et ennemis",
            CardAffectedTarget.AnyUnit => "tout le monde",
            CardAffectedTarget.Self => "toi",
            _ => "",
        };
        return affected == "" ? shape : $"{shape} ({affected})";
    }
}
