using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically populates audit fields on entities
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService, TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker.Entries<BaseEntity>();
        var userId = _currentUserService.UserId ?? "System";
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }
}
