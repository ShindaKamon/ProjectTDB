/// <summary>
/// Énumération des types de marques disponibles dans le jeu.
/// Chaque champion peut avoir son propre type de marque avec des effets uniques.
/// Valeurs explicites : elles sont sérialisées dans les assets CardData, ne pas renuméroter
/// (1 était Stigmate, marque de Vylos retirée avec le personnage).
/// </summary>
public enum MarkType
{
    /// <summary>
    /// Aucune marque (valeur par défaut)
    /// </summary>
    None = 0,

    /// <summary>
    /// Poison - L'unité perd 10 PV à chaque début de son tour.
    /// Les dégâts du poison ignorent l'armure et les boucliers (dégâts bruts).
    /// </summary>
    Poison = 2,

    /// <summary>
    /// AllMarks - Utilisé uniquement pour la consommation.
    /// Consomme TOUTES les marques (de tous types) sur la/les cible(s) de la carte.
    /// Le ciblage dépend de targetType et areaEffect de la carte.
    /// Ne peut pas être utilisé pour appliquer des marques.
    /// </summary>
    AllMarks = 3
}
