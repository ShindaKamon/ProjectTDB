using UnityEngine;

/// <summary>
/// UnderBed est un ennemi simple qui hérite de Enemy.
/// Utilise le système de deck séquentiel et PA des ennemis.
/// Peut avoir des comportements ou propriétés spécifiques ajoutés ici.
/// </summary>
public class UnderBed : Enemy
{
    // Vous pouvez ajouter des comportements ou des propriétés spécifiques à UnderBed ici.
    // Par exemple, des pouvoirs spéciaux, des animations uniques, etc.

    protected override void Start()
    {
        base.Start(); // Important : Appelle la méthode Start de la classe parente Enemy.
                      // Cela initialise les statistiques à partir de EnemyData assigné.
        Debug.Log($"{name} (UnderBed) est prêt ! HP: {GetHealth()}/{GetMaxHealth()}, PA: {GetCurrentPA()}/{GetMaxPA()}");
    }

    // Vous pouvez surcharger d'autres méthodes de Enemy ici si UnderBed a un comportement différent.
    // Par exemple :
    // - public override void TakeDamage(int damage) { ... }
    // - Ajouter des mécaniques spéciales (poison, vol de vie, etc.)
    // - Modifier le pattern d'attaque
}
