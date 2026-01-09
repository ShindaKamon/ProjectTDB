/// <summary>
/// Résultat d'une validation qui peut être un succès ou un échec avec message d'erreur.
/// Permet de retourner des informations structurées sur les validations.
/// Pattern: Result Type pour gestion d'erreurs fonctionnelle
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Crée un résultat de validation réussi
    /// </summary>
    public static ValidationResult Success()
    {
        return new ValidationResult(true);
    }

    /// <summary>
    /// Crée un résultat de validation échoué avec message d'erreur
    /// </summary>
    public static ValidationResult Fail(string errorMessage)
    {
        return new ValidationResult(false, errorMessage);
    }

    /// <summary>
    /// Retourne le message d'erreur ou une chaîne vide si valide
    /// </summary>
    public override string ToString()
    {
        return IsValid ? "Validation réussie" : $"Erreur: {ErrorMessage}";
    }
}

/// <summary>
/// Résultat de validation générique avec une valeur de retour
/// </summary>
public class ValidationResult<T>
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; }
    public T Value { get; private set; }

    private ValidationResult(bool isValid, T value = default, string errorMessage = "")
    {
        IsValid = isValid;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Crée un résultat de validation réussi avec valeur
    /// </summary>
    public static ValidationResult<T> Success(T value)
    {
        return new ValidationResult<T>(true, value);
    }

    /// <summary>
    /// Crée un résultat de validation échoué avec message d'erreur
    /// </summary>
    public static ValidationResult<T> Fail(string errorMessage)
    {
        return new ValidationResult<T>(false, default, errorMessage);
    }
}
