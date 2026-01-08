using UnityEngine;

public class UnderBed : Unit
{
    // Vous pouvez ajouter des comportements ou des propriétés spécifiques à UnderBed ici.
    // Par exemple, des pouvoirs spéciaux, des animations uniques, etc.
    
    protected override void Start()
    {
        base.Start(); // Important : Appelle la méthode Start de la classe parente Unit.
                      // Cela initialise les statistiques à partir du ChampionData assigné.
        Debug.Log($"{name} (UnderBed) est prêt ! HP: {GetHealth()}/{GetMaxHealth()}");
    }

    // Vous pouvez surcharger d'autres méthodes de Unit ici si UnderBed a un comportement différent.
    // Par exemple, public override void TakeDamage(int damage) { ... }
    // public override void Attack(Unit target) { ... }
}
