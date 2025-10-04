using Application.Common.Interfaces;
using System.Security.Claims;

namespace WebApi.Services;

/// <summary>
/// Service for accessing current authenticated user information
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid? StoreId
    {
        get
        {
            var storeIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("StoreId");
            return storeIdClaim != null && Guid.TryParse(storeIdClaim, out var storeId)
                ? storeId
                : null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
