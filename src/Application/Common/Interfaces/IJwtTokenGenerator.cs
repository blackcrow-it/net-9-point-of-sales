using Domain.Entities.Employees;

namespace Application.Common.Interfaces;

/// <summary>
/// Service for generating JWT tokens
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates an access token for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(User user, List<string> roles);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateRefreshToken(string token);
}
