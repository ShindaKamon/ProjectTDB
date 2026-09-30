using UnityEngine;

/// <summary>
/// CruxUnit hérite de Champion et représente le champion Crux.
/// Passif : Réflexe du grimpeur — quand une de ses cartes le met au contact d'une unité (il se
/// déplace jusqu'à elle : Grappin, Bond percutant ; ou il la tire contre lui : Corde de
/// rappel) : au contact d'un allié il gagne un bouclier (comme celui des cartes : absorbe les
/// dégâts jusqu'à épuisement) ; au contact d'un ennemi, un bonus de dégâts sur la prochaine
/// carte de dégâts jouée. Le joueur choisit tank ou assassin selon qui il met au contact.
/// </summary>
public class CruxUnit : Champion, IContactReactor, IOutgoingDamageModifier
{
    [Header("=== Réflexe du grimpeur ===")]
    [Tooltip("Bouclier gagné en atterrissant près d'un allié (PV absorbés, sans durée)")]
    [SerializeField] private int _landingShield = 15;

    [Tooltip("Bonus de dégâts sur la prochaine carte de dégâts jouée (0.15 = +15%)")]
    [SerializeField] private float _nextCardDamageBonus = 0.15f;

    // Bonus de dégâts à usage unique sur la prochaine carte de dégâts (atterrissage près d'un ennemi)
    private bool _hasNextCardBonus = false;

    // ========== IContactReactor ==========

    /// <summary>
    /// Appelé par CardData quand une carte de Crux crée un contact. Unité précisée (tirage) :
    /// son camp décide ; sinon (atterrissage) on regarde les 4 cases voisines, l'allié en priorité.
    /// </summary>
    public void OnContactCreated(Unit contact)
    {
        bool adjacentAlly = false;
        bool adjacentEnemy = false;

        if (contact != null)
        {
            adjacentAlly = contact.GetFaction() == GetFaction();
            adjacentEnemy = !adjacentAlly;
        }
        else
        {
            if (!Services.IsGridServiceAvailable()) return;
            Vector2Int pos = GetCurrentGridPos();

            foreach (var offset in GridGeometry.Directions4)
            {
                Unit unit = Services.Grid.GetUnitAtGridPos(pos + offset);
                if (unit == null || unit == this) continue;

                if (unit.GetFaction() == GetFaction())
                    adjacentAlly = true;
                else
                    adjacentEnemy = true;
            }
        }

        if (adjacentAlly)
        {
            GameLog.Log($"[Réflexe du grimpeur] {name} atterrit près d'un allié -> bouclier de {_landingShield}");
            AddShield(_landingShield, this);
        }
        else if (adjacentEnemy)
        {
            _hasNextCardBonus = true;
            NotifyStatsModified();
            GameLog.Log($"[Réflexe du grimpeur] {name} atterrit près d'un ennemi -> +{_nextCardDamageBonus:P0} dégâts sur la prochaine carte");
            EventBus.Publish(new UnitEffectAppliedEvent(this, UnitEffect.NextAttackPercent, Mathf.RoundToInt(_nextCardDamageBonus * 100)));
        }
    }

    // ========== IOutgoingDamageModifier (bonus prochaine carte) ==========

    // Avalanche (fusion) : le bonus est permanent tant que dure la fusion
    public float GetDamageMultiplier() => _hasNextCardBonus || ActiveFusion is AvalancheFusion ? 1f + _nextCardDamageBonus : 1f;

    public void ConsumeDamageModifier()
    {
        _hasNextCardBonus = false;
        NotifyStatsModified();
    }
}
