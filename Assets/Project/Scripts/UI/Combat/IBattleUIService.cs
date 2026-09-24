using UnityEngine;

/// <summary>
/// Interface pour le service de connexion des UI de combat (Boss Health Bar, Enemy Card Preview, Orbe de vie).
/// Exposé via ServiceLocator pour éviter les appels directs à BattleUIManager.Instance
/// </summary>
public interface IBattleUIService
{
    /// <summary>
    /// Connecte un ennemi boss à la barre de vie de boss
    /// </summary>
    void RegisterBoss(Enemy boss);

    /// <summary>
    /// Connecte un ennemi à la preview de carte
    /// </summary>
    void TrackEnemyCards(Enemy enemy);

    /// <summary>
    /// Change l'ennemi tracké pour la preview de carte
    /// </summary>
    void SwitchTrackedEnemy(Enemy newEnemy);

    /// <summary>
    /// Appelé automatiquement par GridManager quand un ennemi est initialisé
    /// </summary>
    void OnEnemySpawned(Enemy enemy);

    /// <summary>
    /// Nettoie les références quand un ennemi meurt
    /// </summary>
    void OnEnemyDied(Enemy enemy);

    /// <summary>
    /// Enregistre le joueur pour mettre à jour l'Orbe de vie
    /// </summary>
    void RegisterPlayer(Champion player);

    /// <summary>
    /// Met à jour l'Orbe de vie du joueur
    /// </summary>
    void UpdatePlayerOrb(float currentHP, float maxHP, Color emotionColor);
}
