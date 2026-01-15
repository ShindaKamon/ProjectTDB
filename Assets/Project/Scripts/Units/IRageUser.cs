using UnityEngine;

/// <summary>
/// Interface pour toute unité capable d'utiliser la mécanique de Rage.
/// Permet aux cartes d'interagir avec la Rage sans connaître la classe spécifique (IlyaUnit, Enemy, etc.)
/// </summary>
public interface IRageUser
{
    // Ajoute de la rage (généralement via des cartes rouges)
    void AddRageStock(int amount);

    // Tente de consommer de la rage. Renvoie true si succès.
    bool ConsumeRageStock(int amount);

    // Vérifie si le stock de Rage est plein
    bool IsRageStockFull();
}