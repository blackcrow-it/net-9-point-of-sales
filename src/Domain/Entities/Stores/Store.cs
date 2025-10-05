using Domain.Common;

namespace Domain.Entities.Store;

/// <summary>
/// Physical store/warehouse locations
/// </summary>
public class Store : BaseEntity
{
    /// <summary>
    /// Store code (e.g., "STORE001", "WH001")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public StoreType Type { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }

    public string? Phone { get; set; }
    public string? Email { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// User ID of store manager
    /// </summary>
    public string? ManagerId { get; set; }

    /// <summary>
    /// Validates store constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new ArgumentException("Code is required", nameof(Code));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));
    }
}

public enum StoreType
{
    RetailStore = 0,
    Warehouse = 1,
    Both = 2
}
