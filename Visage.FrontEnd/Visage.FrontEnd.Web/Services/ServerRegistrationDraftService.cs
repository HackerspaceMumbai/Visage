using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Visage.FrontEnd.Shared.Services;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// T087: Server-side implementation of IRegistrationDraftService using IMemoryCache.
/// Uses the user's 'sub' claim as the cache key to persist data across OAuth redirects.
/// </summary>
public class ServerRegistrationDraftService : IRegistrationDraftService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServerRegistrationDraftService> _logger;

    public ServerRegistrationDraftService(
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServerRegistrationDraftService> logger)
    {
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string? GetUserKey()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Use 'sub' claim as the unique identifier for the draft
        return user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public Task SaveDraftAsync(User user)
    {
        var key = GetUserKey();
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Cannot save registration draft: User not authenticated");
            return Task.CompletedTask;
        }

        var cacheKey = $"registration_draft_{key}";
        _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
        _logger.LogInformation("Saved registration draft for user {UserId}", key);
        return Task.CompletedTask;
    }

    public Task<User?> GetDraftAsync()
    {
        var key = GetUserKey();
        if (string.IsNullOrEmpty(key))
        {
            return Task.FromResult<User?>(null);
        }

        var cacheKey = $"registration_draft_{key}";
        if (_cache.TryGetValue(cacheKey, out User? user))
        {
            _logger.LogInformation("Restored registration draft for user {UserId}", key);
            return Task.FromResult(user);
        }

        return Task.FromResult<User?>(null);
    }

    public Task ClearDraftAsync()
    {
        var key = GetUserKey();
        if (!string.IsNullOrEmpty(key))
        {
            var cacheKey = $"registration_draft_{key}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cleared registration draft for user {UserId}", key);
        }
        return Task.CompletedTask;
    }
}
