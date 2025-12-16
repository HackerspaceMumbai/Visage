using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// T087: Service for managing OAuth-based social profile connections
/// </summary>
public interface ISocialAuthService
{
    /// <summary>
    /// Initiates OAuth flow for linking LinkedIn account
    /// </summary>
    Task<string> GetLinkedInAuthUrlAsync();

    /// <summary>
    /// Initiates OAuth flow for linking GitHub account
    /// </summary>
    Task<string> GetGitHubAuthUrlAsync();

    /// <summary>
    /// Stores OAuth-verified social profile after successful authentication
    /// </summary>
    Task<bool> LinkSocialProfileAsync(SocialProfileLinkDto linkDto);

    /// <summary>
    /// Disconnects a social provider (server-side)
    /// </summary>
    Task<bool> DisconnectAsync(string provider);

    /// <summary>
    /// Retrieves current social connection status
    /// </summary>
    Task<SocialConnectionStatusDto?> GetSocialStatusAsync();
}

public class SocialAuthService : ISocialAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SocialAuthService> _logger;
    private readonly NavigationManager _navigation;

    public SocialAuthService(HttpClient httpClient, ILogger<SocialAuthService> logger, NavigationManager navigation)
    {
        _httpClient = httpClient;
        _logger = logger;
        _navigation = navigation;
    }

    public Task<string> GetLinkedInAuthUrlAsync()
    {
        // T050: Initiate direct OAuth start endpoint in the web host (bypasses Auth0 social connection)
        var returnUrl = "/registration/mandatory";
        var authUrl = $"/oauth/linkedin/start?returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Task.FromResult(authUrl);
    }

    public Task<string> GetGitHubAuthUrlAsync()
    {
        // T050: Initiate direct OAuth start endpoint in the web host (bypasses Auth0 social connection)
        var returnUrl = "/registration/mandatory";
        var authUrl = $"/oauth/github/start?returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Task.FromResult(authUrl);
    }

    public async Task<bool> LinkSocialProfileAsync(SocialProfileLinkDto linkDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/profile/social/link-callback", linkDto);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully linked {Provider} profile", linkDto.Provider);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to link {Provider} profile: {Error}", linkDto.Provider, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking {Provider} profile", linkDto.Provider);
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(string provider)
    {
        try
        {
            var dto = new SocialDisconnectDto { Provider = provider };
            var response = await _httpClient.PostAsJsonAsync("/api/profile/social/disconnect", dto);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully disconnected {Provider}", provider);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to disconnect {Provider}: {Error}", provider, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting {Provider}", provider);
            return false;
        }
    }
    public async Task<SocialConnectionStatusDto?> GetSocialStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/profile/social/status");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SocialConnectionStatusDto>();
            }

            _logger.LogWarning("Failed to retrieve social status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving social status");
            return null;
        }
    }
}
