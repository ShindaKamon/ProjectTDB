/// <summary>
/// Énumération des types de marques disponibles dans le jeu.
/// Chaque champion peut avoir son propre type de marque avec des effets uniques.
/// </summary>
public enum MarkType
{
    /// <summary>
    /// Aucune marque (valeur par défaut)
    /// </summary>
    None = 0,

    /// <summary>
    /// Stigmate - Marque permanente (max 3 ennemis).
    /// Consommation : Heal le champion par ennemi marqué, l'ennemi perd 1 PA au tour suivant.
    /// Disparaît si : consommée, ennemi meurt, ou champion meurt.
    /// </summary>
    Stigmate,

    /// <summary>
    /// Poison - L'unité perd 10 PV à chaque début de son tour.
    /// Les dégâts du poison ignorent l'armure et les boucliers (dégâts bruts).
    /// </summary>
    Poison,

    /// <summary>
    /// AllMarks - Utilisé uniquement pour la consommation.
    /// Consomme TOUTES les marques (de tous types) sur la/les cible(s) de la carte.
    /// Le ciblage dépend de targetType et areaEffect de la carte.
    /// Ne peut pas être utilisé pour appliquer des marques.
    /// </summary>
    AllMarks
}
