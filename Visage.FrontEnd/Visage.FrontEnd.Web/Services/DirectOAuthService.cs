using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Visage.FrontEnd.Web.Configuration;

namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// Direct OAuth service for LinkedIn and GitHub profile verification.
/// This bypasses Auth0 for social verification, while Auth0 remains the primary auth provider.
/// </summary>
public sealed class DirectOAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OAuthOptions _options;
    private readonly ILogger<DirectOAuthService> _logger;

    public DirectOAuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<OAuthOptions> options,
        ILogger<DirectOAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public string GetLinkedInAuthUrl(string baseUrl, string state)
    {
        var linkedIn = _options.LinkedIn;

        if (string.IsNullOrWhiteSpace(linkedIn.ClientId))
        {
            throw new InvalidOperationException("LinkedIn OAuth not configured");
        }

        var redirectUri = $"{baseUrl}{linkedIn.CallbackPath}";

        return $"{linkedIn.AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(linkedIn.ClientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={Uri.EscapeDataString(state)}&scope={Uri.EscapeDataString(linkedIn.Scope)}";
    }

    public string GetGitHubAuthUrl(string baseUrl, string state)
    {
        var github = _options.GitHub;

        if (string.IsNullOrWhiteSpace(github.ClientId))
        {
            throw new InvalidOperationException("GitHub OAuth not configured");
        }

        var redirectUri = $"{baseUrl}{github.CallbackPath}";

        return $"{github.AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(github.ClientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={Uri.EscapeDataString(state)}&scope={Uri.EscapeDataString(github.Scope)}";
    }

    public async Task<(bool Success, string? ProfileUrl, string? ErrorMessage)> HandleLinkedInCallbackAsync(string code, string baseUrl)
    {
        var linkedIn = _options.LinkedIn;

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var redirectUri = $"{baseUrl}{linkedIn.CallbackPath}";

            // Exchange code for access token
            using var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", linkedIn.ClientId),
                new KeyValuePair<string, string>("client_secret", linkedIn.ClientSecret)
            });

            var tokenResponse = await httpClient.PostAsync(linkedIn.TokenEndpoint, tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn token exchange failed: {StatusCode}", tokenResponse.StatusCode);
                return (false, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.TryGetProperty("access_token", out var at) ? at.GetString() : null;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, null, "Token exchange returned no access token");
            }

            // Fetch minimal profile
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var profileResponse = await httpClient.GetAsync(linkedIn.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn profile fetch failed: {StatusCode}", profileResponse.StatusCode);
                return (false, null, "Profile fetch failed");
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            var linkedInId = profileData.TryGetProperty("id", out var id) ? id.GetString() : null;

            if (string.IsNullOrWhiteSpace(linkedInId))
            {
                return (false, null, "Unable to resolve LinkedIn profile id");
            }

            // Best-effort canonical profile URL.
            // NOTE: LinkedIn public URLs may not map 1:1 from API ids in all configurations.
            var profileUrl = $"https://www.linkedin.com/in/{linkedInId}";

            _logger.LogInformation("Verified LinkedIn profile url derived");
            return (true, profileUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkedIn OAuth callback failed");
            return (false, null, "LinkedIn OAuth callback failed");
        }
    }

    public async Task<(bool Success, string? ProfileUrl, string? ErrorMessage)> HandleGitHubCallbackAsync(string code, string baseUrl)
    {
        var github = _options.GitHub;

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var redirectUri = $"{baseUrl}{github.CallbackPath}";

            using var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", github.ClientId),
                new KeyValuePair<string, string>("client_secret", github.ClientSecret)
            });

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var tokenResponse = await httpClient.PostAsync(github.TokenEndpoint, tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub token exchange failed: {StatusCode}", tokenResponse.StatusCode);
                return (false, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.TryGetProperty("access_token", out var at) ? at.GetString() : null;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, null, "Token exchange returned no access token");
            }

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Visage", "1.0"));

            var profileResponse = await httpClient.GetAsync(github.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub profile fetch failed: {StatusCode}", profileResponse.StatusCode);
                return (false, null, "Profile fetch failed");
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            var profileUrl = profileData.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : null;

            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                return (false, null, "Unable to resolve GitHub profile url");
            }

            _logger.LogInformation("Verified GitHub profile url derived");
            return (true, profileUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub OAuth callback failed");
            return (false, null, "GitHub OAuth callback failed");
        }
    }
}
