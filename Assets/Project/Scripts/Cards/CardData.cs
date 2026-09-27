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
    Anger,          // Colère — rouge #D64545
    Disgust,        // Dégoût — violet #9A4FBF
    Sadness,        // Tristesse — bleu #5A6FD8
    Surprise,       // Surprise — bleu clair #4FA8E8
    Fear,           // Peur — vert #3F9D5C
    Trust,          // Confiance — vert clair #5CC98A
    Joy,            // Joie — jaune #D9A91F
    Anticipation    // Anticipation — orange #E08A3A
}

/// <summary>
/// Type de dégâts d'une carte : l'armure réduit les dégâts physiques, la résistance magique les magiques.
/// </summary>
public enum DamageType
{
    Physical,   // Physique : réduit par l'armure
    Magical     // Magique : réduit par la résistance magique
}

/// <summary>
/// Catégorie de slot dans un deck (structure 2 Signature + 6 Éveil + 16 Standard).
/// </summary>
public enum CardCategory
{
    Standard,   // Pioché dans le pool partagé, filtré par les émotions du deck
    Awakening,  // Éveil : nécessite un seuil d'émotion (système pas encore implémenté)
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
    [Tooltip("Texte d'origine du codex. Plus affiché en jeu (le texte est généré depuis les champs, voir CardRulesText.Build) ; sert à la recherche de l'éditeur de deck.")]
    public string description = "Description de la carte.";

    [TextArea(2, 4)]
    [Tooltip("Règle que les champs ne décrivent pas (ex. condition, cas particulier). Affichée en « Spécial : … » sous le texte généré. Vide = aucune.")]
    public string specialText = "";

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

    [Tooltip("Physique : réduit par l'armure de la cible. Magique : réduit par sa résistance magique.")]
    public DamageType damageType = DamageType.Physical;

    [Tooltip("Dégâts infligés au lanceur après l'effet (contrecoup)")]
    public int damageSelf = 0;

    [Space(5)]
    [Tooltip("Points de vie restaurés à la cible")]
    public int healAmount = 0;

    [Tooltip("Points de vie volés si dégâts infligés")]
    public int lifestealFixedAmount = 0;

    [Tooltip("Dégâts infligés à chaque ennemi au contact de la cible, du type de la carte (ex: Éclat de joie)")]
    public int damageAroundTarget = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         5. BUFFS & DÉBUFFS                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ BUFFS & DÉBUFFS ═══")]
    [Tooltip("Dégâts ajoutés à la prochaine carte offensive de la cible (ex: Montée d'adrénaline), consommés par la première carte qui inflige des dégâts ; cumulable")]
    [UnityEngine.Serialization.FormerlySerializedAs("atkIncreased")]
    public int nextAttackBonus = 0;

    [Tooltip("Bouclier : réserve de PV qui absorbe les dégâts avant les PV (une seule fois, tous types), jusqu'au début du prochain tour du lanceur. ≠ armure, qui réduit chaque coup physique")]
    [UnityEngine.Serialization.FormerlySerializedAs("defenseAmount")]
    public int shieldAmount = 0;

    [Tooltip("Le bouclier ne se déclenche qu'au premier coup ennemi reçu avant le prochain tour du lanceur, et absorbe ce coup (ex: Réflexe de survie)")]
    public bool reactiveShield = false;

    [Tooltip("Armure : retire N à CHAQUE coup physique reçu, pendant effectDuration tours du lanceur (négatif = armure retirée). ≠ bouclier, qui absorbe une seule fois")]
    public int armorAmount = 0;

    [Tooltip("Résistance magique ajoutée à la cible (négatif = résistance magique retirée) pendant effectDuration tours")]
    [UnityEngine.Serialization.FormerlySerializedAs("barrierAmount")]
    public int magicResistanceAmount = 0;

    [Tooltip("Durée des effets à durée, en tours du lanceur (1 = jusqu'au début de son prochain tour) : armure, résistance magique, vulnérabilité. Renseignée aussi sur les retraits de PA/PM, en vue de retraits sur plusieurs tours (aujourd'hui un retrait vaut toujours pour le prochain tour de la cible)")]
    public int effectDuration = 0;

    [Space(5)]
    [Tooltip("PA retirés à la cible au début de son prochain tour (le plus fort retrait l'emporte, pas de cumul)")]
    public int paReduction = 0;

    [Tooltip("PM retirés à la cible au début de son prochain tour (le plus fort retrait l'emporte, pas de cumul)")]
    public int pmReduction = 0;

    [Tooltip("Retire tous les PM de la cible au début de son prochain tour (ex: Terreur paralysante)")]
    public bool removeAllMovement = false;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         6. EFFETS SPÉCIAUX                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ EFFETS SPÉCIAUX ═══")]
    [Tooltip("Si true, le lanceur charge vers la cible")]
    public bool isChargeCard = false;

    [Tooltip("Si true, le lanceur bondit sur la case visée (par-dessus les unités, pas forcément en ligne droite), puis l'effet se déclenche depuis son point d'arrivée (ex: Bond percutant). À utiliser avec une cible « case vide »")]
    public bool leapToTarget = false;

    [Tooltip("Distance de poussée/tirage ; une carte à zone pousse toutes les unités touchées")]
    public int knockbackDistance = 0;

    [Tooltip("Si true, tire la cible VERS le lanceur au lieu de la repousser (ex: Corde de rappel)")]
    public bool pullsTowardCaster = false;

    [Space(5)]
    [Tooltip("Le lanceur recule de N cases à l'opposé de sa cible après l'effet (contrepartie « Repli automatique », ex: Fuite panique)")]
    public int casterRetreat = 0;

    [Tooltip("PM gagnés par le lanceur pour ce tour (« Élan tactique », ex: Piège et recul)")]
    public int casterMovementGain = 0;

    [Tooltip("PA gagnés par le lanceur pour ce tour (sans dépasser son maximum)")]
    public int casterActionGain = 0;

    [Tooltip("Armure du lanceur pendant effectDuration tours (négatif = vulnérabilité, ex: Communion joyeuse)")]
    public int casterArmorAmount = 0;

    [Tooltip("PM perdus par le lanceur au début de son prochain tour (contrepartie « Perd 1 PM », ex: Bouclier de la terreur)")]
    public int casterMovementLoss = 0;

    // ╔════════════════════════════════════════════════════════════════════════════╗
    // ║                         8. GESTION DU DECK                                 ║
    // ╚════════════════════════════════════════════════════════════════════════════╝

    [Header("═══ GESTION DU DECK ═══")]
    [Tooltip("Nombre de cartes à piocher")]
    public int drawAmount = 0;

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
        echoTarget.TakeDamageFrom(echoTarget.ReduceByDefense(echoDamage, damageType), summon);
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
    /// écho de Miroir fraternel.
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
        int finalShield = shieldAmount;
        int finalDraw = drawAmount;
        int finalNextAttackBonus = nextAttackBonus;
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

        // --- BONUS DE PROCHAINE ATTAQUE (ex: Montée d'adrénaline) ---
        // Consommé par la première carte qui inflige des dégâts (une fois par carte : sur une
        // carte à zone il touche toute la zone, sur une carte à cibles multiples la 1re cible)
        if (finalDamage > 0 && !isAdditionalMultiTargetHit)
        {
            int attackBonus = source.ConsumeNextAttackBonus();
            if (attackBonus > 0)
            {
                finalDamage += attackBonus;
                GameLog.Log($"[CardData] {cardName}: +{attackBonus} dégâts (bonus de prochaine attaque)");
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

                bool damageDealt = false;
                if (totalUnitDamage > 0)
                {
                    int hpBefore = unit.GetHealth();
                    if (comboTracker != null && comboTracker.ShouldIgnoreDamageReduction)
                        unit.TakeRawDamage(unit.ReduceByDefense(totalUnitDamage, damageType)); // Paire (Ace) : ignore bouclier et réductions en %, pas l'armure/résistance magique
                    else
                        unit.TakeDamage(unit.ReduceByDefense(totalUnitDamage, damageType));
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

                bool damageDealt = false;
                if (totalTargetDamage > 0)
                {
                    int hpBefore = targetUnit.GetHealth();
                    if (comboTracker != null && comboTracker.ShouldIgnoreDamageReduction)
                        targetUnit.TakeRawDamage(targetUnit.ReduceByDefense(totalTargetDamage, damageType)); // Paire (Ace) : ignore bouclier et réductions en %, pas l'armure/résistance magique
                    else
                        targetUnit.TakeDamage(targetUnit.ReduceByDefense(totalTargetDamage, damageType));
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

        // --- NOUVELLES CAPACITÉS ---

        // 1. Pioche de cartes (une seule fois par carte jouée, pas une fois par cible)
        if (finalDraw > 0 && !isAdditionalMultiTargetHit)
        {
            if (source.TryGetComponentSafe(out DeckManager deckManager))
            {
                deckManager.DrawCards(finalDraw);
            }
        }

        // 4. Bonus de prochaine attaque, armure, résistance magique, bouclier
        if (finalNextAttackBonus > 0 || finalShield != 0 || armorAmount != 0 || magicResistanceAmount != 0)
        {
            // Zone : toutes les unités affectées (ex: Rugissement destructeur) ; sinon la cible
            // définie, ou le lanceur pour Self/None
            List<Unit> statTargets = isAOE
                ? GetAOEAffectedUnits(source, effectEpicenter)
                : new List<Unit> { ((targetsUnit || targetType == CardTargetType.EnemyOrTile) && targetUnit != null) ? targetUnit : source };

            foreach (Unit statTarget in statTargets)
            {
                // Bonus de dégâts sur la prochaine carte offensive de la cible (ex: Chant d'encouragement)
                if (finalNextAttackBonus > 0) statTarget.AddNextAttackBonus(finalNextAttackBonus);

                // Armure et résistance magique pendant effectDuration tours du lanceur
                if (armorAmount != 0 || magicResistanceAmount != 0)
                    statTarget.ModifyStats(0, armorAmount, magicResistanceAmount, effectDuration, source);

                // Bouclier en PV (jusqu'au début du prochain tour du lanceur), ou bouclier
                // réactif qui ne se déclenche qu'au premier coup ennemi (ex: Réflexe de survie)
                if (finalShield > 0)
                {
                    if (reactiveShield) statTarget.ArmReactiveShield(finalShield, source);
                    else statTarget.AddShield(finalShield, source);
                }
            }
        }

        // 5. Dégâts aux ennemis au contact de la cible (ex: Éclat de joie), une fois par carte
        if (damageAroundTarget > 0 && targetUnit != null && !isAdditionalMultiTargetHit)
        {
            Vector2Int around = targetUnit.GetCurrentGridPos();
            foreach (Unit unit in Services.Grid.GetAllUnits())
            {
                if (unit == targetUnit || unit.GetFaction() == source.GetFaction() || IsDead(unit)) continue;
                if (GridGeometry.Distance(around, unit.GetCurrentGridPos()) != 1) continue;

                unit.TakeDamage(unit.ReduceByDefense(damageAroundTarget, damageType));
                GameLog.Log($"{cardName} : {unit.name} (au contact de {targetUnit.name}) subit {damageAroundTarget} dégâts");
            }
        }

        // 6. Poussée / tirage — sur toute la zone pour une carte à zone (ex: Onde de terreur)
        if (!isChargeCard && knockbackDistance > 0)
        {
            Vector2Int sourcePos = source.GetCurrentGridPos();
            List<Unit> movedUnits = isAOE
                ? GetAOEAffectedUnits(source, effectEpicenter)
                : (targetUnit != null ? new List<Unit> { targetUnit } : new List<Unit>());
            movedUnits.Remove(source);

            // Poussée : les plus éloignés d'abord, pour qu'une unité ne bloque pas celle qui la
            // suit ; tirage : les plus proches d'abord
            movedUnits.Sort((a, b) => GridGeometry.Distance(sourcePos, a.GetCurrentGridPos())
                .CompareTo(GridGeometry.Distance(sourcePos, b.GetCurrentGridPos())));
            if (!pullsTowardCaster) movedUnits.Reverse();

            foreach (Unit moved in movedUnits)
            {
                Vector2Int movedPos = moved.GetCurrentGridPos();
                Vector2 direction = pullsTowardCaster
                    ? (Vector2)(sourcePos - movedPos)
                    : (Vector2)(movedPos - sourcePos);
                moved.ApplyKnockback(direction, knockbackDistance);
            }
        }

        // 7. Réduction de PA/PM sur la cible
        if (paReduction > 0 || pmReduction > 0 || removeAllMovement)
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
                // Retrait appliqué au début du prochain tour de la cible (voir ResourceDebuffManager)
                ResourceDebuffManager.ApplyDebuff(debuffTarget, paReduction, removeAllMovement ? int.MaxValue : pmReduction, source);
            }
        }

        // 8. Effets sur le lanceur, une fois par carte : gains de PM/PA, armure (vulnérabilité si
        // négative), perte de PM au prochain tour,
        // puis recul à l'opposé de la cible
        if (!isAdditionalMultiTargetHit)
        {
            if (casterMovementGain > 0) source.GainMovement(casterMovementGain);
            if (casterActionGain > 0 && source is IActionPointsUser paUser) paUser.AddPA(casterActionGain);
            if (casterArmorAmount != 0) source.ModifyStats(0, casterArmorAmount, 0, Mathf.Max(1, effectDuration), source);
            if (casterMovementLoss > 0) ResourceDebuffManager.ApplyDebuff(source, 0, casterMovementLoss, source);

            if (casterRetreat > 0)
            {
                Vector2Int from = targetUnit != null ? targetUnit.GetCurrentGridPos() : effectEpicenter;
                Vector2Int away = source.GetCurrentGridPos() - from;
                if (away != Vector2Int.zero) source.ApplyKnockback(away, casterRetreat);
            }
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

        // --- BONUS DE PROCHAINE ATTAQUE puis MODIFICATEUR DE DÉGÂTS SORTANTS ---
        // Même traitement que dans ExecuteEffect, pour que les cartes de charge bénéficient
        // aussi de ces bonus (et les consomment).
        int finalChargeDamage = damageAmount;
        if (finalChargeDamage > 0)
        {
            int attackBonus = source.ConsumeNextAttackBonus();
            if (attackBonus > 0)
            {
                finalChargeDamage += attackBonus;
                GameLog.Log($"[CardData] {cardName}: +{attackBonus} dégâts de charge (bonus de prochaine attaque)");
            }
        }

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
                pathInfo.EnemyHit.TakeDamage(pathInfo.EnemyHit.ReduceByDefense(finalChargeDamage, damageType));
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

    /// <summary>
    /// Exécute un bond (ex: Bond percutant) : le lanceur saute sur la case visée, puis l'effet
    /// de la carte se résout normalement, centré sur son point d'arrivée.
    /// </summary>
    public void ExecuteLeapEffect(Unit source, Vector2Int targetTilePos)
    {
        source.StartCoroutine(ExecuteLeapEffectCoroutine(source, targetTilePos));
    }

    private System.Collections.IEnumerator ExecuteLeapEffectCoroutine(Unit source, Vector2Int targetTilePos)
    {
        // Chemin d'une seule case : le lanceur va droit sur la case, par-dessus ce qui se trouve entre
        Tile landing = Services.Grid.GetTileAtPosition(targetTilePos);
        if (landing != null && Services.Grid.GetUnitAtGridPos(targetTilePos) == null)
        {
            GameLog.Log($"🦘 BOND ! {source.name} de {source.GetCurrentGridPos()} vers {targetTilePos}");
            source.MoveToTile(new List<Tile> { landing });
            while (source.IsMoving())
                yield return null;

            // Un bond est un déplacement rapide : il déclenche le Réflexe du grimpeur, comme la charge
            if (source is IChargeLandingReactor landingReactor)
                landingReactor.OnChargeLanded();
        }
        else
        {
            GameLog.LogWarning($"{cardName} : case d'arrivée {targetTilePos} invalide ou occupée, l'effet part de la position actuelle");
            targetTilePos = source.GetCurrentGridPos();
        }

        // Case cible = case d'arrivée : la zone se centre sur le lanceur
        ExecuteEffect(source, null, targetTilePos);

        EventBus.Publish(new ShowMovementRangeEvent(source));
    }
}
