using System.Collections.Generic;

/// <summary>
/// Interface pour les unités qui peuvent recevoir des marques.
/// Implémentée par défaut dans Unit.cs
/// </summary>
public interface IMarkable
{
    /// <summary>
    /// Applique une marque sur l'unité
    /// </summary>
    /// <param name="mark">La marque à appliquer</param>
    void ApplyMark(UnitMark mark);

    /// <summary>
    /// Applique une marque avec les paramètres spécifiés
    /// </summary>
    /// <param name="type">Type de la marque</param>
    /// <param name="source">Unité source</param>
    /// <param name="stacks">Nombre de stacks</param>
    /// <param name="duration">Durée en tours (0 = permanent)</param>
    /// <param name="bonusValue">Valeur bonus</param>
    void ApplyMark(MarkType type, Unit source, int stacks = 1, int duration = 0, int bonusValue = 0);

    /// <summary>
    /// Consomme une marque spécifique et retourne ses données
    /// </summary>
    /// <param name="type">Type de marque à consommer</param>
    /// <param name="consumeAllStacks">Si true, consomme tous les stacks. Sinon, un seul.</param>
    /// <returns>La marque consommée (ou une marque None si absente)</returns>
    UnitMark ConsumeMark(MarkType type, bool consumeAllStacks = true);

    /// <summary>
    /// Consomme toutes les marques d'un type donné appliquées par une source spécifique
    /// </summary>
    /// <param name="type">Type de marque à consommer</param>
    /// <param name="source">Unité source qui a appliqué les marques</param>
    /// <returns>La marque consommée (ou une marque None si absente)</returns>
    UnitMark ConsumeMarkFromSource(MarkType type, Unit source);

    /// <summary>
    /// Retire une marque sans déclencher d'effet de consommation
    /// </summary>
    /// <param name="type">Type de marque à retirer</param>
    void RemoveMark(MarkType type);

    /// <summary>
    /// Vérifie si l'unité possède une marque d'un type donné
    /// </summary>
    /// <param name="type">Type de marque à vérifier</param>
    /// <returns>True si l'unité a cette marque</returns>
    bool HasMark(MarkType type);

    /// <summary>
    /// Vérifie si l'unité possède une marque d'un type donné appliquée par une source spécifique
    /// </summary>
    /// <param name="type">Type de marque à vérifier</param>
    /// <param name="source">Unité source</param>
    /// <returns>True si l'unité a cette marque de cette source</returns>
    bool HasMarkFromSource(MarkType type, Unit source);

    /// <summary>
    /// Retourne le nombre de stacks d'un type de marque
    /// </summary>
    /// <param name="type">Type de marque</param>
    /// <returns>Nombre de stacks (0 si pas de marque)</returns>
    int GetMarkStacks(MarkType type);

    /// <summary>
    /// Retourne toutes les marques actives sur l'unité
    /// </summary>
    /// <returns>Liste des marques actives</returns>
    List<UnitMark> GetAllMarks();

    /// <summary>
    /// Retourne une marque spécifique si elle existe
    /// </summary>
    /// <param name="type">Type de marque</param>
    /// <returns>La marque ou null si absente</returns>
    UnitMark? GetMark(MarkType type);

    /// <summary>
    /// Traite les marques au début du tour (décrémente durées, retire les expirées)
    /// </summary>
    void ProcessMarksOnTurnStart();
}
