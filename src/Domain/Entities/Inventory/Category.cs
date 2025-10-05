using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Product categorization with hierarchical structure
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    public string? ImageUrl { get; set; }

    // Navigation Properties
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Validates category constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));

        if (DisplayOrder < 0)
            throw new ArgumentException("Display order must be >= 0", nameof(DisplayOrder));

        // Prevent circular reference
        if (ParentId.HasValue && ParentId.Value == Id)
            throw new InvalidOperationException("Category cannot be its own parent");
    }
}
