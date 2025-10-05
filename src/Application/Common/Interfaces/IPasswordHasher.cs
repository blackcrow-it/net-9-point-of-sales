namespace Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string passwordHash);
}
