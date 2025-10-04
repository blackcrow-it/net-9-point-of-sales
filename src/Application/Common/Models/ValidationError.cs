namespace Application.Common.Models;

/// <summary>
/// Represents a validation error
/// </summary>
public record ValidationError(string PropertyName, string ErrorMessage);
