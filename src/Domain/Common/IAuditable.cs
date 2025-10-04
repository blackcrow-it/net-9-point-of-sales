namespace Domain.Common;

/// <summary>
/// Marker interface for entities that require audit trail tracking.
/// Entities implementing this interface will have their audit fields
/// automatically populated by the audit interceptor.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who created the entity
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated the entity
    /// </summary>
    string? UpdatedBy { get; set; }
}
