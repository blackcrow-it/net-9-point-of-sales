namespace Application.Common.Interfaces;

/// <summary>
/// Service for accessing current user information
/// Used for audit trail and multi-tenancy
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's store ID for multi-tenancy
    /// </summary>
    Guid? StoreId { get; }

    /// <summary>
    /// Checks if the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
