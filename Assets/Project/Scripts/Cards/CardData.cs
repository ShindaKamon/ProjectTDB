using UnityEngine;
using System.Collections.Generic;

// Enum pour spécifier le type de cible valide
public enum CardTargetType
{
    None,           // Aucune
    Self,           // Soi-même
    Enemy,          // Un ou plusieurs ennemis
    Ally,           // Un ou plusieurs alliés (sauf soi-même)
    AllyOrSelf,     // Cible les alliés ET soi-même
    AllyorEnemy,    // Cible les alliés ET les ennemis
    AnyUnit,        // Cible n'importe quelle unité
    EmptyTile,      // Cible uniquement les tuiles vides
    AnyTile,        // Cible n'importe quelle tuile (vide ou occupée)
    EnemyOrTile     // Cible un ennemi OU une tuile
}

public enum CardAreaEffect
{
    None,           // Aucune zone
    OneTile,        // Une case (l'épicentre uniquement)
    Line,           // Ligne de aoeRadius cases depuis le lanceur, dans la direction de l'épicentre
    Cross,          // Croix de aoeRadius cases de rayon centrée sur l'épicentre (axes seulement)
    Circle,         // Cercle de aoeRadius cases de rayon centré sur l'épicentre
    Cone,           // Cône de aoeRadius cases de portée depuis le lanceur, ouverture 90°
    WholeTeam       // Toute l'équipe du lanceur, sans portée ni ligne de vue (ex: Communion joyeuse)
}

public enum CardAffectedTarget
{
    None,           // Aucune cible affectée
    Self,           // Soi-même
    Enemies,        // Que un ou plusieurs ennemies
    Ally,           // Que un ou plusieurs alliées
    AllyOrSelf,     // Que les alliés ET soi-même
    AllyorEnemy,    // Que les alliés ET les ennemis
    AnyUnit         // N'importe quelle unité
}

/// <summary>
/// Les 8 Émotions de base (Couleurs)
/// </summary>
public enum EmotionType
{
    None,
    Colere,         // Rouge #D64545
    Degout,         // Violet #9A4FBF
    Tristesse,      // Bleu #5A6FD8
    Surprise,       // Bleu clair #4FA8E8
    Peur,           // Vert #3F9D5C
    Confiance,      // Vert clair #5CC98A
    Joie,           // Jaune #D9A91F
    Anticipation    // Orange #E08A3A
}

public enum CardEffectType
{
    None,       // Aucun
    Riposte,    // Riposte
    Taunt,      // Taunt
    Knockback,  // Knockback
    Debuff,     // Debuff
    DamageShare // Partage de dégâts (le lanceur prend une partie des dégâts de la cible)
}

/// <summary>
/// Cibles pour la consommation de marques (indépendant du targetType de la carte)
/// </summary>
public enum MarkConsumeTarget
{
    CardTarget,     // Utilise le ciblage de la carte (targetType, AOE, etc.)
    AllEnemies,     // Tous les ennemis sur le terrain
    AllAllies,      // Tous les alliés sur le terrain
    AllUnits        // Toutes les unités sur le terrain
}

/// <summary>
/// Catégorie de slot dans un deck (structure 2 Signature + 6 Éveil + 16 Standard).
/// </summary>
public enum CardCategory
{
    Standard,   // Pioché dans le pool partagé, filtré par les émotions du deck
    Eveil,      // Nécessite un seuil d'émotion (système pas encore implémenté)
    Signature   // Fixe, liée à un champion précis (voir signatureOwner)
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              1. IDENTITÉ                                   ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ IDENTITÉ ═══")]
    [Tooltip("Nom affiché de la carte")]
    public string cardName = "Nom de la Carte";

    [TextArea(3, 5)]
    [Tooltip("Description de l'effet de la carte")]
    public string description = "Description de la carte.";

    [Tooltip("Illustration de la carte")]
    public Sprite artwork;

    [Space(5)]
    [Tooltip("Type d'émotion (Couleur)")]
    public EmotionType emotionType = EmotionType.None;

    [Tooltip("Catégorie de slot dans le deck (Standard/Éveil/Signature)")]
    public CardCategory category = CardCategory.Standard;

    [Tooltip("Champion propriétaire (uniquement pour category = Signature)")]
    public ChampionData signatureOwner;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              2. COÛTS                                      ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ COÛTS ═══")]
    [Tooltip("Coût en Points d'Action (payé avant l'effet)")]
    public int costPA = 0;

    [Tooltip("Coût en Points de Vie (payé avant l'effet)")]
    public int costHP = 0;

    [Tooltip("Coût en ressource spéciale (champion-spécifique)")]
    public int costOther = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                              3. CIBLAGE                                    ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ CIBLAGE ═══")]
    [Tooltip("Type de cible valide pour la carte")]
    public CardTargetType targetType = CardTargetType.None;

    [Tooltip("Portée maximale de la carte")]
    public int targetRange = 0;

    [Space(5)]
    [Tooltip("Forme de la zone d'effet")]
    public CardAreaEffect areaEffect = CardAreaEffect.None;

    [Tooltip("Rayon de la zone d'effet")]
    public int aoeRadius = 0;

    [Tooltip("Types d'unités affectées dans la zone")]
    public CardAffectedTarget affectedTarget = CardAffectedTarget.None;

    [Space(5)]
    [Tooltip("Si true, cette carte cible une autre carte de la main du lanceur (ex: Triche) au lieu d'une unité/tuile de la grille")]
    public bool targetsHandCard = false;

    [Space(5)]
    [Tooltip("Nombre de cibles distinctes à sélectionner manuellement avant exécution (1 = ciblage classique à une cible). Uniquement pour les cartes ciblant des unités.")]
    public int targetCount = 1;

    // Propriétés dérivées pour compatibilité
    public bool targetsUnit => targetType == CardTargetType.Self || targetType == CardTargetType.Enemy || targetType == CardTargetType.Ally || targetType == CardTargetType.AllyOrSelf || targetType == CardTargetType.AllyorEnemy || targetType == CardTargetType.AnyUnit;
    public bool targetsTile => targetType == CardTargetType.EmptyTile || targetType == CardTargetType.AnyTile || targetType == CardTargetType.EnemyOrTile;

    /// <summary>
    /// True si la carte nécessite une sélection manuelle de plusieurs cibles distinctes
    /// (ex: Frappe rapide) au lieu du ciblage classique une-cible-un-clic.
    /// </summary>
    public bool isMultiTarget => targetCount > 1 && targetsUnit;
    public bool isAOE => areaEffect != CardAreaEffect.None && aoeRadius > 0;
    public bool affectsSelf => affectedTarget == CardAffectedTarget.Self || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsAllies => affectedTarget == CardAffectedTarget.Ally || affectedTarget == CardAffectedTarget.AllyOrSelf || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;
    public bool affectsEnemies => affectedTarget == CardAffectedTarget.Enemies || affectedTarget == CardAffectedTarget.AllyorEnemy || affectedTarget == CardAffectedTarget.AnyUnit;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         4. DÉGÂTS & SOIN                                   ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ DÉGÂTS & SOIN ═══")]
    [Tooltip("Dégâts infligés à la cible")]
    public int damageAmount = 0;

    [Tooltip("Dégâts infligés au lanceur après l'effet (contrecoup)")]
    public int damageSelf = 0;

    [Space(5)]
    [Tooltip("Points de vie restaurés à la cible")]
    public int healAmount = 0;

    [Tooltip("Points de vie volés si dégâts infligés")]
    public int lifestealFixedAmount = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         5. BUFFS & DÉBUFFS                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ BUFFS & DÉBUFFS ═══")]
    [Tooltip("Bonus d'attaque accordé")]
    public int atkIncreased = 0;

    [Tooltip("Points de défense accordés")]
    public int defenseAmount = 0;

    [Tooltip("Points de mouvement bonus accordés")]
    public int movementAmount = 0;

    [Tooltip("Durée des buffs/débuffs en tours")]
    public int effectDuration = 0;

    [Space(5)]
    [Tooltip("PA retirés à la cible")]
    public int paReduction = 0;

    [Tooltip("PM retirés à la cible")]
    public int pmReduction = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         6. EFFETS SPÉCIAUX                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ EFFETS SPÉCIAUX ═══")]
    [Tooltip("Type d'effet spécial")]
    public CardEffectType effectType = CardEffectType.None;

    [Space(5)]
    [Tooltip("Si true, le lanceur charge vers la cible")]
    public bool isChargeCard = false;

    [Tooltip("Distance de knockback/recul")]
    public int knockbackDistance = 0;

    [Tooltip("Si true, tire la cible VERS le lanceur au lieu de la repousser (ex: Corde de rappel)")]
    public bool pullsTowardCaster = false;

    [Space(5)]
    [Tooltip("Pourcentage de dégâts redirigés (si DamageShare)")]
    [Range(0, 100)]
    public int damageSharePercent = 50;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                            7. MARQUES                                      ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ MARQUES ═══")]
    [Tooltip("Type de marque à appliquer (None = pas de marque)")]
    public MarkType markToApply = MarkType.None;

    [Tooltip("Nombre de stacks de marque à appliquer")]
    public int markStacks = 1;

    [Tooltip("Durée de la marque (0 = permanent)")]
    public int markDuration = 0;

    [Tooltip("Valeur bonus stockée dans la marque (heal, dégâts...)")]
    public int markBonusValue = 0;

    [Space(5)]
    [Tooltip("Si true, consomme les marques au lieu d'en appliquer")]
    public bool consumeMarks = false;

    [Tooltip("Type de marque à consommer")]
    public MarkType markToConsume = MarkType.None;

    [Tooltip("Cibles pour la consommation (indépendant du targetType)")]
    public MarkConsumeTarget consumeMarkTarget = MarkConsumeTarget.CardTarget;

    [Tooltip("Dégâts par stack de marque consommée")]
    public int damagePerMarkStack = 0;

    [Tooltip("Soin sur le lanceur par marque présente sur la cible")]
    public int healSelfPerMarkOnTarget = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         8. GESTION DU DECK                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ GESTION DU DECK ═══")]
    [Tooltip("Nombre de cartes à piocher")]
    public int drawAmount = 0;

    [Space(5)]
    [Tooltip("Carte spécifique à chercher (Tutor)")]
    public CardData cardToFetch;

    [Tooltip("Nombre de copies à chercher")]
    public int fetchAmount = 0;

    [Space(5)]
    [Tooltip("Carte à ajouter au deck")]
    public CardData cardToAddToDeck;

    [Tooltip("Nombre de copies à ajouter")]
    public int cardsToAddCount = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                       10. DÉGÂTS CONDITIONNELS                             ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ DÉGÂTS CONDITIONNELS ═══")]
    [Tooltip("Dégâts supplémentaires par debuff sur la cible")]
    public int damagePerDebuff = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         11. INVOCATION                                     ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ INVOCATION ═══")]
    [Tooltip("Si true, cette carte invoque une unité (SummonUnit) sur la case ciblée")]
    public bool isSummonCard = false;

    [Tooltip("Prefab de l'invocation (doit porter un composant SummonUnit)")]
    public GameObject summonPrefab;

    [Space(5)]
    [Tooltip("Si true, cette carte repositionne l'invocation active du lanceur au lieu d'en créer une nouvelle")]
    public bool isRepositionSummonCard = false;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         12. COMBO (Ace)                                    ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ COMBO ═══")]
    [Tooltip("Si true, les dégâts de cette carte augmentent selon les PA déjà dépensés ce tour avant elle")]
    public bool scalesWithPASpentThisTurn = false;

    [Tooltip("Bonus de dégâts par PA déjà dépensé ce tour avant cette carte")]
    public int comboDamagePerPASpent = 0;

    // Alias pour compatibilité (anciennes propriétés → effectDuration)
    [System.Obsolete("Utiliser effectDuration à la place")]
    public int statBoostDuration { get => effectDuration; set => effectDuration = value; }
    [System.Obsolete("Utiliser effectDuration à la place")]
    public int resourceDebuffDuration { get => effectDuration; set => effectDuration = value; }

    // Méthode pour vérifier si une unité est une cible valide
    public bool IsValidTarget(Unit source, Unit target)
    {
        if (source == null) return false;

        switch (targetType)
        {
            case CardTargetType.None:
                return true; // Pas besoin de cible

            case CardTargetType.Self:
                return target != null && target == source;

            case CardTargetType.Enemy:
                return target != null && target.GetFaction() != source.GetFaction();

            case CardTargetType.Ally:
                return target != null && target != source && target.GetFaction() == source.GetFaction();

            case CardTargetType.AllyOrSelf:
                return target != null && target.GetFaction() == source.GetFaction();

            case CardTargetType.AllyorEnemy:
                return target != null && target != source;

            case CardTargetType.AnyUnit:
                return target != null;

            case CardTargetType.EnemyOrTile:
                return target != null && target.GetFaction() != source.GetFaction();

            default:
                return false;
        }
    }

    // Méthode pour vérifier si une tuile est une cible valide
    public bool IsValidTileTarget(Tile tile)
    {
        switch (targetType)
        {
            case CardTargetType.None:
                return true;

            case CardTargetType.EmptyTile:
                // Vérifie qu'aucune unité n'occupe cette tuile
                return tile != null && !IsUnitOnTile(tile);

            case CardTargetType.AnyTile:
                return tile != null;

            case CardTargetType.EnemyOrTile:
                return tile != null;

            default:
                return false;
        }
    }

    /// <summary>
    /// Vérifie si une tuile est une cible valide pour une carte de charge (en ligne droite uniquement)
    /// Accepte les cases vides OU les cases avec un ennemi, mais seulement si aucune unité ne bloque le chemin
    /// </summary>
    public bool IsValidChargeTarget(Tile tile, Unit source)
    {
        if (tile == null || source == null) return false;
        if (!isChargeCard) return IsValidTileTarget(tile);

        Vector2Int sourcePos = source.GetCurrentGridPos();
        Vector2Int tilePos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);

        return ChargeHelper.IsValidChargeTarget(sourcePos, tilePos, source);
    }

    // Méthode helper pour vérifier si une unité occupe une tuile
    private bool IsUnitOnTile(Tile tile)
    {
        // OPTIMISATION: Utilise GridRepository au lieu de FindObjectsByType
        Vector2Int tilePos = Services.Grid.GetGridPosFromWorldPos(tile.transform.position);
        Unit unitOnTile = Services.Grid.GetUnitAtGridPos(tilePos);
        return unitOnTile != null;
    }

    // Méthode pour obtenir toutes les unités affectées par l'AOE
    public List<Unit> GetAOEAffectedUnits(Unit source, Vector2Int epicenter)
    {
        List<Unit> affectedUnits = new List<Unit>();

        if (!isAOE || aoeRadius <= 0)
        {
            return affectedUnits;
        }

        // OPTIMISATION: Utilise GridRepository au lieu de FindObjectsByType
        List<Unit> allUnits = Services.Grid.GetAllUnits();

        foreach (Unit unit in allUnits)
        {
            if (!IsInAOEShape(source, epicenter, unit.GetCurrentGridPos()))
                continue;

            bool shouldAffect;

            // Vérifie si c'est le lanceur
            if (unit == source)
            {
                shouldAffect = affectsSelf;
            }
            // Vérifie si c'est un allié
            else if (unit.GetFaction() == source.GetFaction())
            {
                shouldAffect = affectsAllies;
            }
            // C'est un ennemi
            else
            {
                shouldAffect = affectsEnemies;
            }

            if (shouldAffect)
            {
                affectedUnits.Add(unit);
            }
        }

        return affectedUnits;
    }

    /// <summary>
    /// Détermine si une case donnée est couverte par la forme de zone de la carte
    /// (Circle/OneTile centrés sur l'épicentre ; Line/Cone tracés depuis le lanceur en
    /// direction de l'épicentre, sur 4 directions ; WholeTeam ignore position/épicentre).
    /// </summary>
    private bool IsInAOEShape(Unit source, Vector2Int epicenter, Vector2Int tilePos)
    {
        switch (areaEffect)
        {
            case CardAreaEffect.OneTile:
                return tilePos == epicenter;

            case CardAreaEffect.Circle:
                // 4 directions : un « cercle » de rayon r est un losange (rayon 1 = 5 cases, rayon 2 = 13)
                return GridGeometry.Distance(epicenter, tilePos) <= aoeRadius;

            case CardAreaEffect.Cross:
            {
                Vector2Int diff = tilePos - epicenter;
                bool onAxis = diff.x == 0 || diff.y == 0;
                int dist = Mathf.Abs(diff.x) + Mathf.Abs(diff.y);
                return onAxis && dist <= aoeRadius;
            }

            case CardAreaEffect.Line:
            {
                Vector2Int lineSourcePos = source.GetCurrentGridPos();
                // La case du lanceur (origine de la ligne) est considérée "dans la forme" ;
                // c'est affectsSelf (géré par l'appelant) qui décide si elle est réellement affectée.
                if (tilePos == lineSourcePos) return true;

                Vector2Int dir = GetSnappedDirection(lineSourcePos, epicenter);
                if (dir == Vector2Int.zero) return tilePos == epicenter;

                Vector2Int cur = lineSourcePos;
                for (int i = 1; i <= aoeRadius; i++)
                {
                    cur += dir;
                    if (cur == tilePos) return true;
                }
                return false;
            }

            case CardAreaEffect.Cone:
            {
                Vector2Int sourcePos = source.GetCurrentGridPos();
                // La case du lanceur (origine du cône) est considérée "dans la forme" ;
                // c'est affectsSelf (géré par l'appelant) qui décide si elle est réellement affectée.
                if (tilePos == sourcePos) return true;

                Vector2Int dir = GetSnappedDirection(sourcePos, epicenter);
                if (dir == Vector2Int.zero) return tilePos == epicenter;

                Vector2Int toTile = tilePos - sourcePos;
                if (GridGeometry.Distance(sourcePos, tilePos) > aoeRadius) return false;

                float angle = Vector2.Angle(dir, toTile);
                return angle <= 45f; // ouverture totale de 90°
            }

            case CardAreaEffect.WholeTeam:
                return true; // aucune contrainte de position (sans portée ni ligne de vue)

            default:
                return false;
        }
    }

    /// <summary>
    /// Direction (parmi les 4) la plus proche entre deux positions : l'axe dominant.
    /// </summary>
    private static Vector2Int GetSnappedDirection(Vector2Int from, Vector2Int to) =>
        GridGeometry.SnapDirection(from, to);

    /// <summary>
    /// Passif "Miroir fraternel" (Soren) : si le lanceur a une invocation active avec un ennemi à
    /// portée, elle inflige un écho à 40% des dégâts réellement infligés (après réduction/boucliers).
    /// Règles (décision du 24/09/2026) :
    /// - portée = celle de la carte jouée, mesurée depuis l'invocation, en 4 directions comme toute
    ///   la grille : on place Lyse selon la carte qu'on veut jouer ;
    /// - cible : l'ennemi visé par le lanceur s'il est à portée de l'invocation (et encore en vie),
    ///   sinon l'ennemi le plus proche de l'invocation ;
    /// - déclenchement automatique (le choix manuel de la cible est prévu pour la V2).
    /// </summary>
    private void TryTriggerSummonEcho(Unit source, int appliedDamage, Unit sourceTarget)
    {
        if (appliedDamage <= 0) return;
        if (!(source is ISummonOwner summonOwner)) return;

        SummonUnit summon = summonOwner.ActiveSummon;
        if (summon == null || IsDead(summon)) return;

        Vector2Int summonPos = summon.GetCurrentGridPos();
        bool IsValidEchoTarget(Unit unit) =>
            unit != null && unit != source && unit != summon && !IsDead(unit)
            && unit.GetFaction() != summon.GetFaction() // uniquement les ennemis de l'invocation
            && GridGeometry.Distance(summonPos, unit.GetCurrentGridPos()) <= targetRange;

        Unit echoTarget = IsValidEchoTarget(sourceTarget) ? sourceTarget : null;

        if (echoTarget == null)
        {
            int bestDist = int.MaxValue;
            foreach (Unit unit in Services.Grid.GetAllUnits())
            {
                if (!IsValidEchoTarget(unit)) continue;

                int dist = GridGeometry.Distance(summonPos, unit.GetCurrentGridPos());
                if (dist < bestDist)
                {
                    bestDist = dist;
                    echoTarget = unit;
                }
            }
        }

        if (echoTarget == null) return;

        int echoDamage = Mathf.Max(1, Mathf.RoundToInt(appliedDamage * 0.4f));
        echoTarget.TakeDamageFrom(echoDamage, summon);
        GameLog.Log($"[Miroir fraternel] {summon.name} renvoie un écho de {echoDamage} dégâts (40% de {appliedDamage}) sur {echoTarget.name}");
    }

    private static bool IsDead(Unit unit)
    {
        UnitState state = unit.GetUnitState();
        return state != null && state.IsDead();
    }

    // Méthode pour exécuter l'effet de la carte
    /// <param name="isAdditionalMultiTargetHit">
    /// True quand cet appel correspond à une cible supplémentaire d'une MÊME carte à cibles
    /// multiples déjà résolue pour une cible précédente (voir HandUIController.ExecutePendingMultiTargetCard,
    /// qui appelle ExecuteEffect une fois par cible). Dans ce cas, les effets qui doivent se
    /// produire une seule fois par carte jouée (et non une fois par cible) sont sautés :
    /// combo tracker (Main gagnante d'Ace), invocation/repositionnement, dégâts sur soi, pioche,
    /// fetch, ajout de cartes au deck, écho de Miroir fraternel.
    /// </param>
    public virtual void ExecuteEffect(Unit source, Unit targetUnit = null, Vector2Int targetTile = default, bool isAdditionalMultiTargetHit = false)
    {
        GameLog.Log($"Exécution de l'effet de la carte {cardName} par {source.name}.");

        // Passif "Main gagnante" (Ace) : détecte un motif avec la carte précédente AVANT de
        // résoudre les effets, pour que le bonus s'applique à CETTE carte (ex: Paire).
        // Ne s'exécute qu'une fois par carte jouée, pas une fois par cible (sinon la carte se
        // comparerait à elle-même sur la 2e cible d'une carte à cibles multiples).
        IComboTracker comboTracker = source as IComboTracker;
        if (!isAdditionalMultiTargetHit)
        {
            comboTracker?.OnCardAboutToExecute(this);
        }

        // Pour EnemyOrTile, si aucune unité n'est ciblée explicitement, on regarde sur la case cible
        if (targetType == CardTargetType.EnemyOrTile && targetUnit == null)
        {
            targetUnit = Services.Grid.GetUnitAtGridPos(targetTile);
            // On s'assure que c'est bien un ennemi
            if (targetUnit != null && targetUnit.GetFaction() == source.GetFaction())
                targetUnit = null;
        }

        // Variables locales pour les valeurs finales (modifiables par boost)
        int finalDamage = damageAmount;
        int finalHeal = healAmount;
        int finalDefense = defenseAmount;
        int finalDraw = drawAmount;
        int finalFetch = fetchAmount;
        int finalAtk = atkIncreased;
        int finalLifesteal = lifestealFixedAmount;

        // --- SCALING PAR PA DÉPENSÉS CE TOUR (ex: Tapis d'Ace) ---
        if (scalesWithPASpentThisTurn && comboDamagePerPASpent > 0 && comboTracker != null)
        {
            int bonus = comboDamagePerPASpent * comboTracker.PASpentThisTurn;
            if (bonus > 0)
            {
                finalDamage += bonus;
                GameLog.Log($"[CardData] {cardName}: +{bonus} dégâts (combo, {comboTracker.PASpentThisTurn} PA déjà dépensés ce tour)");
            }
        }

        // --- MODIFICATEUR DE DÉGÂTS SORTANTS GÉNÉRIQUE (ex: Réflexe du grimpeur) ---
        // Ne consomme le bonus que si la carte inflige réellement des dégâts, pour qu'il
        // reste disponible si le joueur joue d'abord une carte de soin/buff.
        if (finalDamage > 0 && source is IOutgoingDamageModifier dmgMod)
        {
            float multiplier = dmgMod.GetDamageMultiplier();
            if (multiplier != 1f)
            {
                int before = finalDamage;
                finalDamage = Mathf.RoundToInt(finalDamage * multiplier);
                dmgMod.ConsumeDamageModifier();
                GameLog.Log($"[CardData] {cardName}: dégâts modifiés par {source.name} : {before} -> {finalDamage} (x{multiplier:F2})");
            }
        }

        // --- INVOCATION / REPOSITIONNEMENT (ex: Invocation de Lyse, Écho évanescent) ---
        // Une seule fois par carte jouée (pas une fois par cible d'une carte à cibles multiples).
        if (!isAdditionalMultiTargetHit)
        {
            if (isSummonCard && summonPrefab != null)
            {
                // Si l'invocation du lanceur est déjà active et vivante (ex: Lyse pour Soren),
                // on ne réinvoque pas une deuxième copie : on la soigne à la place (décision produit).
                SummonUnit existingSummon = (source is ISummonOwner existingSummonOwner) ? existingSummonOwner.ActiveSummon : null;
                UnitState existingSummonState = existingSummon != null ? existingSummon.GetUnitState() : null;

                if (existingSummon != null && (existingSummonState == null || !existingSummonState.IsDead()))
                {
                    existingSummon.Heal(healAmount);
                    GameLog.Log($"{cardName} : {existingSummon.name} est déjà invoquée, elle est soignée de {healAmount} PV au lieu d'être réinvoquée.");
                }
                else
                {
                    Vector2Int spawnPos = targetsTile ? targetTile : source.GetCurrentGridPos();
                    var summon = Services.Grid.SpawnSummon(summonPrefab, spawnPos, source, source.GetHealth() / 2);
                    if (summon != null && source is ISummonOwner summonOwner)
                    {
                        summonOwner.RegisterSummon(summon);
                    }
                }
            }
            else if (isRepositionSummonCard && targetsTile && source is ISummonOwner repositionOwner)
            {
                // Invocation choisie à la 1re étape du ciblage (targetUnit) ; à défaut (IA, appel
                // direct), l'invocation active du lanceur.
                SummonUnit summonToMove = targetUnit as SummonUnit;
                if (summonToMove == null) summonToMove = repositionOwner.ActiveSummon;
                repositionOwner.RepositionSummon(summonToMove, targetTile);
            }
        }

        // Détermine l'épicentre de l'effet
        Vector2Int effectEpicenter;
        if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
        {
            effectEpicenter = targetUnit.GetCurrentGridPos();
        }
        else if (targetsTile)
        {
            effectEpicenter = targetTile;
        }
        else
        {
            effectEpicenter = source.GetCurrentGridPos();
        }

        // Cumul des dégâts réellement appliqués (après réduction/boucliers) sur les cibles,
        // utilisé par le passif "Miroir fraternel" (cf. TryTriggerSummonEcho) pour ne se
        // déclencher que si - et en fonction de ce que - la carte a réellement infligé.
        int actualDamageDealt = 0;

        // Applique l'effet AOE si activé
        if (isAOE && aoeRadius > 0)
        {
            List<Unit> affectedUnits = GetAOEAffectedUnits(source, effectEpicenter);
            GameLog.Log($"🔥 AOE {cardName} : {affectedUnits.Count} unités affectées dans un rayon de {aoeRadius}");

            foreach (Unit unit in affectedUnits)
            {
                int totalUnitDamage = finalDamage;
                if (damagePerDebuff > 0)
                {
                    totalUnitDamage += damagePerDebuff * unit.GetDebuffCount();
                }

                bool damageDealt = false;
                if (totalUnitDamage > 0)
                {
                    int hpBefore = unit.GetHealth();
                    if (comboTracker != null && comboTracker.ShouldIgnoreDamageReduction)
                        unit.TakeRawDamage(totalUnitDamage); // Paire (Ace) : ignore les boucliers en %
                    else
                        unit.TakeDamage(totalUnitDamage);
                    int hpAfter = unit.GetHealth();
                    if (hpAfter < hpBefore) damageDealt = true;
                    actualDamageDealt += hpBefore - hpAfter;
                    GameLog.Log($"  → {unit.name} prend {totalUnitDamage} dégâts AOE");
                }
                if (finalHeal > 0)
                {
                    unit.Heal(finalHeal);
                    GameLog.Log($"  → {unit.name} récupère {finalHeal} PV AOE");
                }
                if (healSelfPerMarkOnTarget > 0)
                {
                    int markCount = unit.GetTotalMarkCount();
                    int healSelf = markCount * healSelfPerMarkOnTarget;
                    if (healSelf > 0)
                    {
                        source.Heal(healSelf);
                        GameLog.Log($"  → {source.name} récupère {healSelf} PV (AOE sur {unit.name})");
                    }
                }
                if (finalLifesteal > 0 && damageDealt)
                {
                    source.Heal(finalLifesteal);
                    GameLog.Log($"  → {source.name} vole {finalLifesteal} PV à {unit.name} (AOE)");
                }
            }
        }
        // Sinon, applique l'effet sur la cible unique
        else
        {
            if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                int totalTargetDamage = finalDamage;
                if (damagePerDebuff > 0)
                {
                    totalTargetDamage += damagePerDebuff * targetUnit.GetDebuffCount();
                }

                bool damageDealt = false;
                if (totalTargetDamage > 0)
                {
                    int hpBefore = targetUnit.GetHealth();
                    if (comboTracker != null && comboTracker.ShouldIgnoreDamageReduction)
                        targetUnit.TakeRawDamage(totalTargetDamage); // Paire (Ace) : ignore les boucliers en %
                    else
                        targetUnit.TakeDamage(totalTargetDamage);
                    int hpAfter = targetUnit.GetHealth();
                    if (hpAfter < hpBefore) damageDealt = true;
                    actualDamageDealt += hpBefore - hpAfter;
                    GameLog.Log($"{source.name} inflige {totalTargetDamage} dégâts à {targetUnit.name} avec {cardName}.");
                }
                if (finalHeal > 0)
                {
                    targetUnit.Heal(finalHeal);
                    GameLog.Log($"{source.name} soigne {targetUnit.name} de {finalHeal} PV avec {cardName}.");
                }
                if (healSelfPerMarkOnTarget > 0)
                {
                    int markCount = targetUnit.GetTotalMarkCount();
                    int healSelf = markCount * healSelfPerMarkOnTarget;
                    if (healSelf > 0)
                    {
                        source.Heal(healSelf);
                        GameLog.Log($"{source.name} récupère {healSelf} PV grâce aux marques sur {targetUnit.name} ({markCount} marques).");
                    }
                }
                if (finalLifesteal > 0 && damageDealt)
                {
                    source.Heal(finalLifesteal);
                    GameLog.Log($"{source.name} vole {finalLifesteal} PV à {targetUnit.name}.");
                }
            }
            else if (targetType == CardTargetType.Self && finalHeal > 0)
            {
                source.Heal(finalHeal);
                GameLog.Log($"{source.name} se soigne de {finalHeal} PV avec {cardName}.");
            }
        }

        // Passif "Miroir fraternel" (Soren) : écho à 40% de puissance sur une cible à portée
        // de l'invocation active, si la carte jouée inflige réellement des dégâts.
        // Basé sur les dégâts réellement appliqués (après réduction/boucliers), pas sur
        // finalDamage (théorique, avant résolution) - sinon l'écho se déclenchait même quand
        // la cible était entièrement protégée.
        // Une seule fois par carte jouée (pas une fois par cible).
        if (!isAdditionalMultiTargetHit)
        {
            TryTriggerSummonEcho(source, actualDamageDealt, targetUnit);
        }

        // Dégâts sur soi-même (ex: cartes puissantes mais risquées)
        // Une seule fois par carte jouée (pas une fois par cible).
        if (damageSelf > 0 && !isAdditionalMultiTargetHit)
        {
            source.TakeDamage(damageSelf);
            GameLog.Log($"{source.name} subit {damageSelf} dégâts de contrecoup avec {cardName}.");
        }

        if (movementAmount > 0)
        {
            // La logique de mouvement sera gérée par l'InputManager ou une autre entité
            // pour l'instant, nous pouvons juste loguer l'intention.
            GameLog.Log($"{source.name} gagne {movementAmount} points de mouvement supplémentaires avec {cardName}.");
        }

        // --- NOUVELLES CAPACITÉS ---

        // 1. Pioche de cartes (une seule fois par carte jouée, pas une fois par cible)
        if (finalDraw > 0 && !isAdditionalMultiTargetHit)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                deckManager.DrawCards(finalDraw);
            }
        }

        // 2. Aller chercher une carte spécifique (Fetch) - idem, une seule fois par carte jouée
        if (cardToFetch != null && finalFetch > 0 && !isAdditionalMultiTargetHit)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                deckManager.FetchCards(c => c == cardToFetch, finalFetch);
            }
        }

        // 4. Gain de Stats (Force / Défense)
        if (finalAtk != 0 || finalDefense != 0)
        {
            // Si la cible est définie, on l'utilise, sinon si c'est Self/None, c'est le lanceur
            Unit statTarget = ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null) ? targetUnit : source;

            // Applique les stats (nécessite la méthode ModifyStats sur Unit)
            statTarget.ModifyStats(finalAtk, finalDefense, effectDuration);
        }

        // 5. Ajout de cartes au deck (cartes générées)
        // Une seule fois par carte jouée (pas une fois par cible).
        if (cardToAddToDeck != null && cardsToAddCount > 0 && !isAdditionalMultiTargetHit)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                for (int i = 0; i < cardsToAddCount; i++)
                {
                    deckManager.AddCardToDeck(cardToAddToDeck);
                }

                deckManager.ShuffleDeck();

                GameLog.Log($"{source.name} ajoute {cardsToAddCount}x {cardToAddToDeck.cardName} à son deck.");
            }
        }

        // 6. Knockback simple (sur la cible) — pousse loin du lanceur, ou tire vers lui si pullsTowardCaster
        if (effectType == CardEffectType.Knockback && !isChargeCard && targetUnit != null && knockbackDistance > 0)
        {
            Vector2Int sourcePos = source.GetCurrentGridPos();
            Vector2Int targetPos = targetUnit.GetCurrentGridPos();
            Vector2 knockbackDir = pullsTowardCaster
                ? ((Vector2)sourcePos - (Vector2)targetPos).normalized
                : ((Vector2)targetPos - (Vector2)sourcePos).normalized;
            targetUnit.ApplyKnockback(knockbackDir, knockbackDistance);
        }

        // 7. Réduction de PA/PM sur la cible
        if (paReduction > 0 || pmReduction > 0)
        {
            // Détermine les cibles pour la réduction
            List<Unit> debuffTargets = new List<Unit>();

            if (isAOE && aoeRadius > 0)
            {
                debuffTargets = GetAOEAffectedUnits(source, effectEpicenter);
            }
            else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                debuffTargets.Add(targetUnit);
            }

            foreach (Unit debuffTarget in debuffTargets)
            {
                // Utilise le ResourceDebuffManager pour gérer la durée
                ResourceDebuffManager.ApplyDebuff(debuffTarget, paReduction, pmReduction, effectDuration, source);
            }
        }

        // 8. Système de Marques
        // 8a. Consommation de marques (doit être fait AVANT l'application pour éviter de consommer ce qu'on vient d'appliquer)
        if (consumeMarks && markToConsume != MarkType.None)
        {
            List<Unit> markTargets = new List<Unit>();

            // Détermine les cibles en fonction de consumeMarkTarget
            switch (consumeMarkTarget)
            {
                case MarkConsumeTarget.AllEnemies:
                    markTargets = Services.Grid?.GetAllEnemyUnits() ?? new List<Unit>();
                    break;

                case MarkConsumeTarget.AllAllies:
                    markTargets = Services.Grid?.GetAllPlayerUnits() ?? new List<Unit>();
                    break;

                case MarkConsumeTarget.AllUnits:
                    markTargets = Services.Grid?.GetAllUnits() ?? new List<Unit>();
                    break;

                case MarkConsumeTarget.CardTarget:
                default:
                    // Utilise le ciblage standard de la carte
                    if (isAOE && aoeRadius > 0)
                    {
                        markTargets = GetAOEAffectedUnits(source, effectEpicenter);
                    }
                    else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
                    {
                        markTargets.Add(targetUnit);
                    }
                    break;
            }

            int totalHeal = 0;

            foreach (Unit markTarget in markTargets)
            {
                // AllMarks : consomme TOUTES les marques de tous types sur cette cible
                if (markToConsume == MarkType.AllMarks)
                {
                    List<UnitMark> consumedMarks = markTarget.ConsumeAllMarksFromSource(source);

                    foreach (UnitMark consumedMark in consumedMarks)
                    {
                        // Dégâts par stack
                        int bonusDamage = consumedMark.stacks * damagePerMarkStack;
                        if (bonusDamage > 0)
                        {
                            markTarget.TakeDamage(bonusDamage);
                            GameLog.Log($"🎯 MARQUE CONSOMMÉE ! {source.name} inflige {bonusDamage} dégâts bonus à {markTarget.name} ({consumedMark.stacks} stacks de {consumedMark.markType})");
                        }

                        // Bonus de la marque
                        if (consumedMark.bonusValue > 0)
                        {
                            int markBonus = consumedMark.bonusValue * consumedMark.stacks;
                            markTarget.TakeDamage(markBonus);
                            GameLog.Log($"🎯 BONUS DE MARQUE ! {markBonus} dégâts supplémentaires");
                        }

                        // Heal par marque consommée
                        if (healAmount > 0)
                        {
                            totalHeal += healAmount;
                        }
                    }
                }
                // Type de marque spécifique
                else if (markTarget.HasMark(markToConsume))
                {
                    // Consomme uniquement les marques appliquées par ce champion
                    UnitMark consumedMark = markTarget.ConsumeMarkFromSource(markToConsume, source);

                    if (consumedMark.markType != MarkType.None)
                    {
                        // Calcule les dégâts bonus basés sur les stacks
                        int bonusDamage = consumedMark.stacks * damagePerMarkStack;

                        if (bonusDamage > 0)
                        {
                            markTarget.TakeDamage(bonusDamage);
                            GameLog.Log($"🎯 MARQUE CONSOMMÉE ! {source.name} inflige {bonusDamage} dégâts bonus à {markTarget.name} ({consumedMark.stacks} stacks de {markToConsume})");
                        }

                        // Bonus supplémentaire de la marque
                        if (consumedMark.bonusValue > 0)
                        {
                            markTarget.TakeDamage(consumedMark.bonusValue * consumedMark.stacks);
                            GameLog.Log($"🎯 BONUS DE MARQUE ! {consumedMark.bonusValue * consumedMark.stacks} dégâts supplémentaires");
                        }

                        // Heal par marque consommée
                        if (healAmount > 0)
                        {
                            totalHeal += healAmount;
                        }
                    }
                }
            }

            // Applique le heal total au lanceur
            if (totalHeal > 0)
            {
                source.Heal(totalHeal);
                GameLog.Log($"🎯 {source.name} récupère {totalHeal} PV (marques consommées)");
            }
        }

        // 8b. Application de nouvelles marques
        if (markToApply != MarkType.None && markStacks > 0)
        {
            // Détermine les cibles pour l'application
            List<Unit> markTargets = new List<Unit>();

            if (isAOE && aoeRadius > 0)
            {
                markTargets = GetAOEAffectedUnits(source, effectEpicenter);
            }
            else if ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null)
            {
                markTargets.Add(targetUnit);
            }

            foreach (Unit markTarget in markTargets)
            {
                markTarget.ApplyMark(markToApply, source, markStacks, markDuration, markBonusValue);
                GameLog.Log($"🎯 MARQUE APPLIQUÉE ! {source.name} marque {markTarget.name} avec {markToApply} ({markStacks} stack(s), durée: {(markDuration == 0 ? "permanent" : markDuration + " tours")})");
            }
        }

        // 9. Partage de Dégâts (Damage Share)
        if (effectType == CardEffectType.DamageShare && targetUnit != null)
        {
            // Applique le lien de partage de dégâts sur la cible vers le lanceur
            // Ratio basé sur damageSharePercent, durée basée sur statBoostDuration (défaut 1 tour si 0)
            float ratio = Mathf.Clamp01(damageSharePercent / 100f);
            targetUnit.SetDamageShare(source, ratio, effectDuration > 0 ? effectDuration : 1);
        }

        // Met à jour l'historique de combo (Main gagnante) pour la prochaine carte jouée
        // Une seule fois par carte jouée (pas une fois par cible, sinon la carte se comparerait
        // à elle-même et gonflerait PASpentThisTurn d'un coût supplémentaire par cible).
        if (!isAdditionalMultiTargetHit)
        {
            comboTracker?.OnCardResolved(this);
        }
    }

    /// <summary>
    /// Exécute l'effet de charge : le lanceur se déplace vers la cible, s'arrête si un ennemi bloque le chemin et le repousse
    /// </summary>
    /// <param name="source">L'unité qui charge</param>
    /// <param name="targetTilePos">La position cible de la charge</param>
    /// <param name="onComplete">Callback appelé quand la charge est terminée</param>
    public void ExecuteChargeEffect(Unit source, Vector2Int targetTilePos, System.Action onComplete = null)
    {
        if (!isChargeCard)
        {
            GameLog.LogWarning($"{cardName} n'est pas une carte de charge!");
            onComplete?.Invoke();
            return;
        }

        // Lance la coroutine via le MonoBehaviour source
        source.StartCoroutine(ExecuteChargeEffectCoroutine(source, targetTilePos, onComplete));
    }

    /// <summary>
    /// Coroutine qui exécute l'effet de charge avec attente du mouvement
    /// </summary>
    private System.Collections.IEnumerator ExecuteChargeEffectCoroutine(Unit source, Vector2Int targetTilePos, System.Action onComplete)
    {
        // Passif "Main gagnante" (Ace) / système de combo : mêmes hooks que ExecuteEffect,
        // pour que les cartes de charge (ex: L'Alpiniste) participent au combo.
        IComboTracker comboTracker = source as IComboTracker;
        comboTracker?.OnCardAboutToExecute(this);

        Vector2Int sourcePos = source.GetCurrentGridPos();

        // Utilise le helper pour calculer le chemin de charge
        ChargePathInfo pathInfo = ChargeHelper.CalculateChargePath(sourcePos, targetTilePos, source);

        if (!pathInfo.IsValid)
        {
            GameLog.LogWarning($"Charge invalide : la cible n'est pas en ligne droite!");
            comboTracker?.OnCardResolved(this);
            onComplete?.Invoke();
            yield break;
        }

        GameLog.Log($"🏃 CHARGE ! {source.name} de {sourcePos} vers {targetTilePos} (distance: {pathInfo.Distance}, direction: {pathInfo.StepDirection})");

        // Déplace le lanceur
        if (pathInfo.Path.Count > 0)
        {
            source.MoveToTile(pathInfo.Path);
            GameLog.Log($"🏃 CHARGE ! {source.name} se déplace ({pathInfo.Path.Count} cases)");

            // Attend que le mouvement soit terminé
            while (source.IsMoving())
            {
                yield return null;
            }
        }

        // Notifie l'unité de son atterrissage (ex: Réflexe du grimpeur de L'Alpiniste)
        if (source is IChargeLandingReactor landingReactor)
        {
            landingReactor.OnChargeLanded();
        }

        // --- MODIFICATEUR DE DÉGÂTS SORTANTS GÉNÉRIQUE (ex: Réflexe du grimpeur) ---
        // Même traitement que dans ExecuteEffect, pour que les cartes de charge bénéficient
        // aussi des modificateurs de dégâts sortants (et les consomment).
        int finalChargeDamage = damageAmount;
        if (finalChargeDamage > 0 && source is IOutgoingDamageModifier dmgMod)
        {
            float multiplier = dmgMod.GetDamageMultiplier();
            if (multiplier != 1f)
            {
                int before = finalChargeDamage;
                finalChargeDamage = Mathf.RoundToInt(finalChargeDamage * multiplier);
                dmgMod.ConsumeDamageModifier();
                GameLog.Log($"[CardData] {cardName}: dégâts de charge modifiés par {source.name} : {before} -> {finalChargeDamage} (x{multiplier:F2})");
            }
        }

        // Si un ennemi a été touché, applique le knockback et les dégâts
        if (pathInfo.EnemyHit != null)
        {
            GameLog.Log($"🏃 CHARGE ! Ennemi touché: {pathInfo.EnemyHit.name}, knockback: {knockbackDistance}, dégâts: {finalChargeDamage}");

            // Applique les dégâts de la charge
            if (finalChargeDamage > 0)
            {
                pathInfo.EnemyHit.TakeDamage(finalChargeDamage);
                GameLog.Log($"🏃 CHARGE ! {source.name} inflige {finalChargeDamage} dégâts à {pathInfo.EnemyHit.name}");
            }

            // Applique le knockback APRÈS les dégâts et APRÈS le mouvement
            if (knockbackDistance > 0)
            {
                GameLog.Log($"🏃 KNOCKBACK ! Direction: {pathInfo.StepDirection}, Distance: {knockbackDistance}");
                pathInfo.EnemyHit.ApplyKnockback(pathInfo.StepDirection, knockbackDistance);

                // Attend que le knockback soit terminé
                while (pathInfo.EnemyHit.IsMoving())
                {
                    yield return null;
                }
            }
        }
        else
        {
            GameLog.Log($"🏃 CHARGE ! Aucun ennemi touché");
        }

        // Rafraîchit l'affichage de la portée de mouvement après la charge et le knockback
        EventBus.Publish(new ShowMovementRangeEvent(source));

        // Met à jour l'historique de combo (Main gagnante) pour la prochaine carte jouée
        comboTracker?.OnCardResolved(this);

        onComplete?.Invoke();
    }
}
