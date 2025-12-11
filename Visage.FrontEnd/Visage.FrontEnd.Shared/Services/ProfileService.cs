using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// Client-side service for profile operations with 5-minute caching using IMemoryCache.
/// Reduces API calls for frequently accessed profile completion status.
/// </summary>
public class ProfileService : IProfileService
{
    private const string CacheKey = "ProfileCompletionStatus";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ProfileService> _logger;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

    public ProfileService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<ProfileService> logger, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Gets profile completion status with 5-minute client-side caching.
    /// Cache stored in IMemoryCache (in-memory cache).
    /// </summary>
    public async Task<ProfileCompletionStatusDto?> GetCompletionStatusAsync()
    {
        try
        {
            // Build a per-user cache key to avoid cross-user cache leakage in Blazor Server.
            // Only use the cache when we can determine an authenticated user's id. Do not use an "anonymous" key.
            string? userIdForCache = null;
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    userIdForCache = user.FindFirst("sub")?.Value
                        ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    _logger.LogDebug("ProfileService: Determined authenticated user id for cache: {UserId}", userIdForCache);
                }
                else
                {
                    _logger.LogDebug("ProfileService: No authenticated user found; skipping cache lookup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ProfileService: Failed to read AuthenticationState for cache key; skipping cache");
            }

            string? perUserCacheKey = null;
            if (!string.IsNullOrWhiteSpace(userIdForCache))
            {
                perUserCacheKey = CacheKey + ":" + userIdForCache;

                // Check if cache exists and is still valid
                if (_memoryCache.TryGetValue<ProfileCompletionStatusDto>(perUserCacheKey, out var cachedData))
                {
                    _logger.LogInformation("ProfileService: Cache hit for completion status (user={UserId})", userIdForCache);
                    return cachedData;
                }
            }


            _logger.LogInformation("ProfileService: Cache miss, calling API /api/profile/completion-status");
            _logger.LogInformation("ProfileService: HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

            // Call backend API directly - AuthenticationDelegatingHandler will attach the token
            var response = await _httpClient.GetAsync("/api/profile/completion-status");

            _logger.LogInformation("ProfileService: API response status code: {StatusCode}", response.StatusCode);
            
            // Log response details for debugging
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("ProfileService: Response content (first 200 chars): {Content}", 
                responseContent.Length > 200 ? responseContent.Substring(0, 200) : responseContent);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("ProfileService: 404 Not Found - treating as new user with incomplete profile");
                // 404 = New user with no profile record → treat as incomplete profile
                var dto = new ProfileCompletionStatusDto
                {
                    UserId = userIdForCache ?? "unknown",
                    IsProfileComplete = false,
                    IsAideProfileComplete = false,
                    CheckedAt = DateTime.UtcNow
                };

                // Cache per-user result briefly to avoid repeated 404 calls during the same session
                if (!string.IsNullOrWhiteSpace(perUserCacheKey))
                {
                    _memoryCache.Set(perUserCacheKey, dto, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });
                    _cacheKeys.TryAdd(perUserCacheKey, 0);
                }

                return dto;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ProfileService: Non-success status code {StatusCode} - returning null", response.StatusCode);
                // Return null for auth errors (401, 403) - let caller handle authentication
                return null;
            }

            var status = await response.Content.ReadFromJsonAsync<ProfileCompletionStatusDto>();

            if (status != null)
            {
                _logger.LogInformation("ProfileService: API returned status - IsComplete: {IsComplete}, IsAideComplete: {IsAide}",
                    status.IsProfileComplete, status.IsAideProfileComplete);
                
                // Store in cache with 5-minute absolute expiration (per-user key)
                if (!string.IsNullOrWhiteSpace(perUserCacheKey))
                {
                    _memoryCache.Set(perUserCacheKey, status, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = CacheDuration
                    });
                    _cacheKeys.TryAdd(perUserCacheKey, 0);
                }
            }
            else
            {
                _logger.LogWarning("ProfileService: API returned 200 OK but deserialization failed");
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProfileService: Exception while getting completion status - Type: {ExceptionType}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            // On error, return null - caller should handle gracefully
            return null;
        }
    }

    /// <summary>
    /// Invalidates cached profile completion status.
    /// Call after profile updates to ensure next read fetches fresh data.
    /// </summary>
    public async Task InvalidateCacheAsync()
    {
        string? userId = null;

        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                userId = user.FindFirst("sub")?.Value
                    ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ProfileService: Failed to resolve user for cache invalidation; will clear tracked keys");
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var perUserKey = CacheKey + ":" + userId;
            _memoryCache.Remove(perUserKey);
            _cacheKeys.TryRemove(perUserKey, out _);
            _logger.LogInformation("ProfileService: Invalidated cache for user {UserId}", userId);
            return;
        }

        foreach (var key in _cacheKeys.Keys)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }

        _memoryCache.Remove(CacheKey);
        _logger.LogInformation("ProfileService: Cleared all cached profile completion entries (fallback)");
    }

    /// <summary>
    /// Gets the full profile for the current authenticated user using their email.
    /// </summary>
    public async Task<Registrant?> GetProfileAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("GetProfileAsync: User not authenticated");
                return null;
            }

            // Get email from claims (Auth0 uses namespaced claim)
            var email = user.FindFirst("https://hackmum.in/claims/email")?.Value
                ?? user.FindFirst("email")?.Value
                ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GetProfileAsync: No email claim found");
                return null;
            }

            _logger.LogInformation("GetProfileAsync: Fetching profile for {Email}", email);

            var response = await _httpClient.GetAsync($"/api/profile/{email}");
            
            if (response.IsSuccessStatusCode)
            {
                var registrant = await response.Content.ReadFromJsonAsync<Registrant>();
                _logger.LogInformation("GetProfileAsync: Profile loaded successfully");
                return registrant;
            }
            else
            {
                _logger.LogWarning("GetProfileAsync: API returned {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GetProfileAsync: Network error fetching profile");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "GetProfileAsync: Request timeout fetching profile");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileAsync: Error fetching profile");
            return null;
        }
    }

    /// <summary>
    /// Updates the profile for the current authenticated user.
    /// </summary>
    public async Task<bool> UpdateProfileAsync(Registrant registrant)
    {
        try
        {
            _logger.LogInformation("UpdateProfileAsync: Updating profile {Id}", registrant.Id);

            var response = await _httpClient.PutAsJsonAsync($"/api/profile/{registrant.Id}", registrant);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("UpdateProfileAsync: Profile updated successfully");
                
                // Invalidate cache so next GetCompletionStatusAsync call fetches fresh data
                await InvalidateCacheAsync();
                
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UpdateProfileAsync: API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync: Network error updating profile");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync: Request timeout updating profile");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync: Error updating profile");
            return false;
        }
    }

    /// <summary>
    /// Saves a draft of the profile section for the current authenticated user.
    /// Drafts auto-expire after 30 days per FR-008.
    /// </summary>
    public async Task<bool> SaveDraftAsync(string section, string draftData)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("SaveDraftAsync: User not authenticated");
                return false;
            }

            // Get userId from claims
            var userId = user.FindFirst("sub")?.Value
                ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SaveDraftAsync: No userId claim found");
                return false;
            }

            var draftDto = new RegistrationDraftDto
            {
                UserId = userId,
                Section = section,
                DraftData = draftData,
                SavedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _logger.LogInformation("SaveDraftAsync: Saving draft for section {Section}", section);

            var response = await _httpClient.PostAsJsonAsync("/api/profile/draft", draftDto);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SaveDraftAsync: Draft saved successfully for section {Section}", section);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("SaveDraftAsync: API returned {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveDraftAsync: Error saving draft for section {Section}", section);
            return false;
        }
    }

    /// <summary>
    /// Retrieves a saved draft for the specified section.
    /// Returns null if no draft exists or draft is expired.
    /// </summary>
    public async Task<string?> GetDraftAsync(string section)
    {
        try
        {
            _logger.LogInformation("GetDraftAsync: Retrieving draft for section {Section}", section);

            var response = await _httpClient.GetAsync($"/api/profile/draft/{section}");

            if (response.IsSuccessStatusCode)
            {
                var draftDto = await response.Content.ReadFromJsonAsync<RegistrationDraftDto>();
                if (draftDto != null)
                {
                    _logger.LogInformation("GetDraftAsync: Draft retrieved successfully for section {Section}", section);
                    return draftDto.DraftData;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("GetDraftAsync: No draft found for section {Section}", section);
                return null;
            }
            else
            {
                _logger.LogWarning("GetDraftAsync: API returned {StatusCode}", response.StatusCode);
                return null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDraftAsync: Error retrieving draft for section {Section}", section);
            return null;
        }
    }

    /// <summary>
    /// Deletes the draft for the specified section.
    /// Called after successful profile submission to clean up draft data.
    /// </summary>
    public async Task<bool> DeleteDraftAsync(string section)
    {
        try
        {
            _logger.LogInformation("DeleteDraftAsync: Deleting draft for section {Section}", section);

            var response = await _httpClient.DeleteAsync($"/api/profile/draft/{section}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("DeleteDraftAsync: Draft deleted successfully for section {Section}", section);
                return true;
            }
            else
            {
                _logger.LogWarning("DeleteDraftAsync: API returned {StatusCode}", response.StatusCode);
                // Return true even on 404 - draft already doesn't exist
                return response.StatusCode == System.Net.HttpStatusCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDraftAsync: Error deleting draft for section {Section}", section);
            return false;
        }
    }
}
