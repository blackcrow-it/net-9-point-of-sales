using Domain.Common;

namespace Domain.Entities.Employees;

/// <summary>
/// Granular access control
/// </summary>
public class Permission : BaseEntity
{
    public Guid RoleId { get; set; }

    /// <summary>
    /// Resource name (e.g., "Orders", "Products", "Reports")
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Action name (e.g., "View", "Create", "Update", "Delete")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    // Navigation Properties
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Validates permission constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Resource))
            throw new ArgumentException("Resource is required", nameof(Resource));

        if (string.IsNullOrWhiteSpace(Action))
            throw new ArgumentException("Action is required", nameof(Action));
    }
}
