namespace Domain.Common;

/// <summary>
/// Base entity class providing common fields for all entities in the system.
/// Includes audit trail support and soft delete pattern.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who created the entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the entity was last updated (nullable if never updated)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated the entity (nullable if never updated)
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag - if true, entity is logically deleted
    /// </summary>
    public bool IsDeleted { get; set; }
}
