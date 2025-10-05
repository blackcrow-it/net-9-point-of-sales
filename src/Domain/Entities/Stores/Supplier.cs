using Domain.Common;

namespace Domain.Entities.Store;

/// <summary>
/// Product suppliers
/// </summary>
public class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    public string? TaxCode { get; set; }

    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Validates supplier constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new ArgumentException("Code is required", nameof(Code));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));
    }
}
