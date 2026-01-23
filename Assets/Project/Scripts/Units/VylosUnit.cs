using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// VylosUnit hérite de Champion et représente le champion Vylos.
/// Spécificités :
/// - PA (Points d'Action) pour jouer des cartes (hérité de Champion)
/// - PM (Points de Mouvement) pour se déplacer (hérité de Unit)
/// - Passif 1 : 100% des dégâts subis par Vylos sont infligés à tous les ennemis marqués par un Stigmate (dégâts bruts)
/// - Passif 2 : La première fois que Vylos perd des PV lors de son prochain tour, il gagne 1 PA
/// </summary>
public class VylosUnit : Champion, IActionPointsUser
{
    // ========== VARIABLES POUR LES PASSIFS ==========
    
    /// <summary>
    /// Indique si Vylos a perdu des PV ce tour (pour le passif 2)
    /// </summary>
    private bool _hasLostHPThisTurn = false;
    
    /// <summary>
    /// Indique si Vylos doit gagner 1 PA au début du prochain tour (passif 2)
    /// </summary>
    private bool _shouldGainPAOnNextTurn = false;
    
    /// <summary>
    /// PV précédents pour détecter toute perte de PV (utilisé pour les deux passifs)
    /// </summary>
    private int _previousHealth;
    
    /// <summary>
    /// Flag pour éviter les boucles infinies lors du partage de dégâts vers Stigmates
    /// </summary>
    private bool _isSharingDamageToStigmates = false;

    // NOTE: PA system (GetCurrentPA, GetMaxPA, SpendPA, RefreshPA, OnActionPointsChanged)
    // est hérité de Champion

    // ========== INITIALISATION ==========

    /// <summary>
    /// Surcharge Initialize pour Vylos (pas de stats spéciales à initialiser pour l'instant)
    /// </summary>
    public new void Initialize(ChampionData data, Vector2 initialGridPos)
    {
        base.Initialize(data, initialGridPos);

        Debug.Log($"{name} (Vylos) initialisé - PA: {GetCurrentPA()}/{GetMaxPA()}, ATK: {GetAttack()}");
    }

    protected override void Start()
    {
        base.Start(); // Appelle le Start de Unit (gère l'initialisation automatique)

        // Désactivé : On utilise l'Orbe de vie (UI) au lieu de la barre flottante pour le joueur
        // CreateHealthBar();

        // Connecte l'unité à l'Orbe de vie via le BattleUIManager
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.RegisterPlayer(this);
        }
        
        // Initialise les PV précédents
        _previousHealth = GetHealth();
        
        // S'abonne à l'événement de changement de PV pour détecter TOUS les types de dégâts
        OnHealthChanged += HandleHealthChanged;
    }
    
    void OnDestroy()
    {
        // Se désabonne pour éviter les fuites mémoire
        OnHealthChanged -= HandleHealthChanged;
    }

    // ========== PASSIF 1 : PARTAGE DE DÉGÂTS VERS STIGMATES ==========
    
    /// <summary>
    /// Gère les changements de PV pour détecter TOUS les types de dégâts (ennemi, allié, self, carte, partage, etc.)
    /// Cette méthode est appelée via l'événement OnHealthChanged, donc elle capture tous les changements de PV
    /// </summary>
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        // Ignore si on est en train de partager des dégâts (évite les boucles infinies)
        if (_isSharingDamageToStigmates) return;
        
        // Calcule la perte de PV
        int healthLost = _previousHealth - currentHealth;
        
        // Met à jour les PV précédents
        _previousHealth = currentHealth;
        
        // Si Vylos a perdu des PV
        if (healthLost > 0)
        {
            // Passif 2 : Détecte la première perte de PV du tour
            if (!_hasLostHPThisTurn)
            {
                _hasLostHPThisTurn = true;
                _shouldGainPAOnNextTurn = true;
                Debug.Log($"[Vylos Passif 2] {name} a perdu des PV pour la première fois ce tour - gagnera 1 PA au prochain tour");
            }
            
            // Passif 1 : Partage les dégâts vers les ennemis avec Stigmate (dégâts bruts)
            // Note: On partage même si c'est un coût (PayHealth), car l'utilisateur a demandé que tous les types de dégâts agissent
            ShareDamageToStigmates(healthLost);
        }
    }

    /// <summary>
    /// Partage les dégâts subis par Vylos vers tous les ennemis marqués par un Stigmate (passif 1)
    /// </summary>
    /// <param name="damageAmount">Montant de dégâts à partager (dégâts bruts)</param>
    private void ShareDamageToStigmates(int damageAmount)
    {
        if (damageAmount <= 0) return;

        // Récupère tous les ennemis marqués par un Stigmate de Vylos
        List<Unit> markedEnemies = StigmateManager.GetUnitsMarkedBySource(this);

        if (markedEnemies.Count == 0)
        {
            return; // Aucun ennemi marqué, rien à faire
        }

        Debug.Log($"[Vylos Passif 1] {name} subit {damageAmount} dégâts → Partage vers {markedEnemies.Count} ennemi(s) avec Stigmate");

        // Active le flag pour éviter les boucles infinies
        _isSharingDamageToStigmates = true;

        // Inflige les dégâts bruts à chaque ennemi marqué
        foreach (Unit markedEnemy in markedEnemies)
        {
            if (markedEnemy != null && !markedEnemy.GetUnitState().IsDead())
            {
                markedEnemy.TakeRawDamage(damageAmount);
                Debug.Log($"[Vylos Passif 1] → {markedEnemy.name} prend {damageAmount} dégâts bruts (Stigmate)");
            }
        }
        
        // Désactive le flag après le partage
        _isSharingDamageToStigmates = false;
    }

    // ========== PASSIF 2 : GAIN DE PA AU DÉBUT DU TOUR ==========

    /// <summary>
    /// Surcharge ProcessBuffsOnTurnStart pour gérer le passif 2 (gain de PA)
    /// </summary>
    public override void ProcessBuffsOnTurnStart()
    {
        // Passif 2 : Si Vylos a perdu des PV le tour précédent, gagne 1 PA
        if (_shouldGainPAOnNextTurn)
        {
            // Utilise la méthode AddPA pour gagner 1 PA (ne peut pas dépasser le maximum)
            AddPA(1);
            Debug.Log($"[Vylos Passif 2] {name} gagne 1 PA (première perte de PV du tour précédent). PA: {GetCurrentPA()}/{GetMaxPA()}");
            
            _shouldGainPAOnNextTurn = false;
        }

        // Réinitialise le flag de perte de PV pour le nouveau tour
        _hasLostHPThisTurn = false;
        
        // Met à jour les PV précédents pour le nouveau tour
        _previousHealth = GetHealth();

        // Appelle la classe de base (gère les buffs temporaires)
        base.ProcessBuffsOnTurnStart();
    }
}
