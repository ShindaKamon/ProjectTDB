/// <summary>
/// Interface pour le service de connexion des UI de combat (Boss Health Bar, Enemy Card Preview, Orbe de vie).
/// Exposé via ServiceLocator pour éviter les appels directs à BattleUIManager.Instance.
/// L'orbe de vie suit seule le champion actif (TurnChangedEvent) : seuls les ennemis sont signalés ici.
/// </summary>
public interface IBattleUIService
{
    /// <summary>
    /// Appelé automatiquement par GridManager quand un ennemi est initialisé
    /// </summary>
    void OnEnemySpawned(Enemy enemy);

    /// <summary>
    /// Nettoie les références quand un ennemi meurt
    /// </summary>
    void OnEnemyDied(Enemy enemy);
}
