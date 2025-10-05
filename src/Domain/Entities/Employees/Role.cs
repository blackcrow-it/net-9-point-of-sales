using Domain.Common;

namespace Domain.Entities.Employees;

/// <summary>
/// User roles with permissions
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    /// <summary>
    /// Validates role constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));
    }
}
