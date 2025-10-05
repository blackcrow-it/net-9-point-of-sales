using Domain.Common;

namespace Domain.Entities.Employees;

/// <summary>
/// System users (employees)
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>
    /// Primary store assignment
    /// </summary>
    public Guid? StoreId { get; set; }

    public Guid RoleId { get; set; }

    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation Properties
    public Store.Store? Store { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    /// <summary>
    /// Updates the refresh token
    /// </summary>
    public void UpdateRefreshToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration must be in the future", nameof(expiresAt));

        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Marks the last login time
    /// </summary>
    public void MarkLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates user constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("Username is required", nameof(Username));

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new ArgumentException("Password hash is required", nameof(PasswordHash));

        // Vietnamese phone validation: 10 digits
        if (!string.IsNullOrWhiteSpace(Phone) && !System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^\d{10}$"))
            throw new ArgumentException("Phone must be 10 digits (Vietnamese format)", nameof(Phone));
    }
}
