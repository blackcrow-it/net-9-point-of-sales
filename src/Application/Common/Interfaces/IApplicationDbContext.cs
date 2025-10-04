namespace Application.Common.Interfaces;

/// <summary>
/// Database context interface for the application
/// Provides access to DbSets and database operations
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Saves all changes made in this context to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
