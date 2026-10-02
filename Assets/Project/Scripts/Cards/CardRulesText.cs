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
    /// paSpentThisTurn : PA déjà dépensés ce tour par le lanceur (main en combat) ; une carte dont
    /// les dégâts en dépendent (ex: Tapis) affiche alors ses dégâts actuels, « Inflige 40 (56) ».
    /// </summary>
    public static string Build(CardData card, int paSpentThisTurn = 0)
    {
        var lines = new List<string>();
        void Effect(string icon, ChipKind kind, string text) => lines.Add(Icon(icon, kind) + " " + ColorValue(text, kind));
        string Turns() => card.effectDuration > 0 ? $" ({card.effectDuration} tour{(card.effectDuration > 1 ? "s" : "")})" : "";

        if (card.damageAmount > 0)
            Effect(card.damageType == DamageType.Magical ? "magic" : "dmg", ChipKind.Damage,
                "Inflige " + card.damageAmount + (card.damageType == DamageType.Magical ? " (magique)" : "")
                + (card.targetType == CardTargetType.AllyorEnemy ? " à un ennemi" : "")
                + (card.scalesWithPASpentThisTurn && paSpentThisTurn > 0 ? $" ({card.damageAmount + card.comboDamagePerPASpent * paSpentThisTurn})" : ""));
        if (card.scalesWithPASpentThisTurn && card.comboDamagePerPASpent > 0)
            Effect("dmg", ChipKind.Damage, $"+{card.comboDamagePerPASpent} dégâts par PA déjà dépensé ce tour");
        // Carte d'invocation : le soin ne sert que si l'invocation est déjà sur le terrain (voir ExecuteEffect)
        if (card.healAmount > 0 && !card.isSummonCard) Effect("heal", ChipKind.Heal, "Soigne " + card.healAmount);
        if (card.lifestealFixedAmount > 0) Effect("drain", ChipKind.Heal, "Vol de vie " + card.lifestealFixedAmount + (card.isAOE ? " par ennemi touché" : ""));
        if (card.damageAroundTarget > 0)
            Effect(card.damageType == DamageType.Magical ? "magic" : "dmg", ChipKind.Damage, $"Inflige {card.damageAroundTarget} aux ennemis au contact de la cible");
        if (card.shieldAmount > 0)
            Effect("shield", ChipKind.Shield, "Bouclier " + card.shieldAmount + (card.reactiveShield ? " au premier coup ennemi reçu" : ""));
        if (card.nextAttackBonus > 0) Effect("buff", ChipKind.Damage, $"+{card.nextAttackBonus} dégâts sur la prochaine carte offensive");
        if (card.armorAmount != 0) Effect("armor", ChipKind.Defense, "Armure " + card.armorAmount.ToString("+#;−#") + Turns());
        if (card.magicResistanceAmount != 0) Effect("magicresist", ChipKind.Defense, "Résistance magique " + card.magicResistanceAmount.ToString("+#;−#") + Turns());
        // Malus de la cible, formulés comme les autres malus : « Perd 1 PM pendant 1 tour »
        if (card.removeAllMovement) Effect("lock", ChipKind.MovementPoints, "Perd tous ses PM pendant " + DebuffTurns(card));
        else if (card.pmReduction > 0) Effect("pm", ChipKind.MovementPoints, $"Perd {card.pmReduction} PM pendant " + DebuffTurns(card));
        if (card.paReduction > 0) Effect("pa", ChipKind.ActionPoints, $"Perd {card.paReduction} PA pendant " + DebuffTurns(card)
            + (card.paBecomesPmInShadow ? $" ({card.paReduction} PM dans l'ombre)" : ""));
        if (card.nextTurnActionGain > 0) Effect("pa", ChipKind.ActionPoints, $"+{card.nextTurnActionGain} PA au prochain tour");
        if (card.cancelsEnemyNextCard) Effect("lock", ChipKind.Mute, "Le monstre ne joue pas sa prochaine carte");
        if (card.spawnedEnemy != null) Effect("summon", ChipKind.Mute, "Fait apparaître : " + card.spawnedEnemy.enemyName);
        if (card.throwsDebris) Effect("summon", ChipKind.Mute, "Ramasse les débris : une zone de plus par tas");
        if (card.isAmbush) Effect("bond", ChipKind.Push, "Au prochain tour : surgit au contact du champion le plus faible et frappe");
        if (card.darkensTerrain) Effect("lock", ChipKind.Mute, "Assombrit le terrain jusqu'à son prochain tour");
        if (card.changesHidingSpot) Effect("summon", ChipKind.Mute, "Il change de cachette");
        if (card.hindersEnemyNextCard) Effect("lock", ChipKind.Mute, "Entrave : le monstre fait son attaque de base au lieu de sa prochaine carte");
        if (card.knockbackDistance > 0)
            Effect(card.pullsTowardCaster ? "pull" : "push", ChipKind.Push,
                (card.pullsTowardCaster ? "Tire de " : "Repousse de ") + Cases(card.knockbackDistance));
        if (card.isChargeCard)
            Effect("bond", ChipKind.Push, card.targetsUnit
                ? "Te hisse jusqu'à la cible (" + Cases(CodexCardVisual.Range(card)) + " max)" // ex: Grappin
                : "Bond jusqu'à " + Cases(CodexCardVisual.Range(card)));
        if (card.leapToTarget) Effect("bond", ChipKind.Push, "Bondis sur la case visée");
        if (card.isSummonCard)
        {
            Effect("summon", ChipKind.Mute, "Invoque " + (card.summonPrefab != null ? SummonName(card.summonPrefab.name) : "une invocation"));
            if (card.healAmount > 0) Effect("heal", ChipKind.Heal, "Si déjà présente : soigne " + card.healAmount);
        }
        if (card.isRepositionSummonCard) Effect("summon", ChipKind.Mute, "Déplace ton invocation");
        if (card.targetsHandCard) Effect("hand", ChipKind.Mute, "Cible une carte de ta main");
        if (card.drawAmount > 0) Effect("hand", ChipKind.Mute, "Pioche " + card.drawAmount);
        if (card.discardHandAttackBonusPerCard > 0)
            Effect("buff", ChipKind.Damage, $"Défausse ta main : +{card.discardHandAttackBonusPerCard} dégâts sur ta prochaine carte offensive par carte défaussée");
        if (card.casterMovementGain > 0) Effect("pm", ChipKind.MovementPoints, $"+{card.casterMovementGain} PM ce tour");
        if (card.casterActionGain > 0) Effect("pa", ChipKind.ActionPoints, $"+{card.casterActionGain} PA ce tour");
        if (card.casterArmorAmount > 0) Effect("armor", ChipKind.Defense, $"Ton armure +{card.casterArmorAmount}" + CasterTurns(card));
        if (card.casterRetreat > 0) Effect("push", ChipKind.Push, "Tu recules de " + Cases(card.casterRetreat));

        // Carte sur soi avec une zone : « Zone : autour de toi, … » suffit
        bool selfZone = card.targetType == CardTargetType.Self && ZoneText(card) != "";
        // Lancer annoncé (boss) : les zones remplacent la cible
        string target = card.telegraphedZoneCount > 0
            ? $"{card.telegraphedZoneCount} zones annoncées au sol, une sur chaque champion ; elles tombent à son prochain tour"
            : selfZone ? "" : TargetText(card);
        if (target != "") lines.Add("<b>Cible :</b> " + target);

        string zone = ZoneText(card);
        if (zone != "") lines.Add("<b>Zone :</b> " + (selfZone && card.areaEffect != CardAreaEffect.WholeTeam ? "autour de toi, " : "") + zone);

        // Contreparties, en couleur d'avertissement
        void Warn(string icon, string text) =>
            lines.Add($"{Icon(icon, ChipKind.Warn)} <color=#{ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(ChipKind.Warn))}>{text}</color>");

        if (card.damageSelf > 0) Warn("self", $"Contrecoup : tu subis {card.damageSelf}");
        if (card.casterArmorAmount < 0) Warn("armor", $"Contrecoup : ton armure {card.casterArmorAmount.ToString("+#;−#")}" + CasterTurns(card));
        if (card.casterMovementLoss > 0) Warn("pm", $"Contrecoup : tu perds {card.casterMovementLoss} PM pendant 1 tour");

        if (!string.IsNullOrWhiteSpace(card.specialText))
            lines.Add("<i>Spécial :</i> " + card.specialText.Trim());

        return string.Join("\n", lines);
    }

    public static string Icon(string name, ChipKind kind) =>
        $"<sprite name=\"{name}\" color=#{ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(kind))}>";

    static string Cases(int n) => n + (n > 1 ? " cases" : " case");

    // Valeur d'un effet (1er nombre de la ligne, signe compris) dans la couleur de sa stat, comme
    // l'icône : « Inflige <rouge>27</rouge> », « Perd <vert>1</vert> PM pendant 1 tour ». Les
    // effets sans stat (pioche, invocation…) gardent la couleur du texte.
    static readonly System.Text.RegularExpressions.Regex FirstValue =
        new System.Text.RegularExpressions.Regex(@"[+−-]?\d+%?");

    public static string ColorValue(string text, ChipKind kind)
    {
        if (kind == ChipKind.Mute) return text;
        string hex = ColorUtility.ToHtmlStringRGB(CodexCardVisual.ChipColor(kind));
        return FirstValue.Replace(text, m => $"<color=#{hex}>{m.Value}</color>", 1);
    }

    // Durée d'un retrait de PA/PM (au moins 1 tour), ex. « 1 tour »
    static string DebuffTurns(CardData card)
    {
        int turns = Mathf.Max(1, card.effectDuration);
        return $"{turns} tour{(turns > 1 ? "s" : "")}";
    }

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
        return who + (card.targetInStraightLine ? " en ligne droite" : "") + " · "
            + (CodexCardVisual.Range(card) <= 1 ? "au contact" : "portée 1-" + CodexCardVisual.Range(card));
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
