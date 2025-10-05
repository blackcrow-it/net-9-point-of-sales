using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Product brands/manufacturers
/// </summary>
public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // Navigation Properties
    public ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Validates brand constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));

        if (!string.IsNullOrWhiteSpace(Website) && !Uri.TryCreate(Website, UriKind.Absolute, out _))
            throw new ArgumentException("Website must be a valid URL", nameof(Website));
    }
}
