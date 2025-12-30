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

        var effectiveBase = string.IsNullOrWhiteSpace(_options.BaseUrl) ? baseUrl : _options.BaseUrl;
        var redirectUri = $"{effectiveBase}{linkedIn.CallbackPath}";

        var authUrl = $"{linkedIn.AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(linkedIn.ClientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={Uri.EscapeDataString(state)}&scope={Uri.EscapeDataString(linkedIn.Scope)}";

        // Log redirect URI and scope used; full auth URL at debug level
        _logger.LogInformation("LinkedIn auth URL generated; redirect_uri={RedirectUri}; usingConfiguredBase={UsingConfigured}; scope={Scope}", redirectUri, !string.IsNullOrWhiteSpace(_options.BaseUrl), linkedIn.Scope);
        _logger.LogDebug("LinkedIn auth URL: {AuthUrl}", authUrl);

        return authUrl;
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

    public async Task<(bool Success, string? LinkedInSubject, string? RawProfileJson, string? RawEmailJson, string? Email, string? ErrorMessage)> HandleLinkedInCallbackAsync(string code, string baseUrl)
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
                return (false, null, null, null, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.TryGetProperty("access_token", out var at) ? at.GetString() : null;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, null, null, null, null, "Token exchange returned no access token");
            }

            // Fetch minimal profile
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // LinkedIn recommends the Restli protocol header for v2 APIs
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Restli-Protocol-Version", "2.0.0");

            var profileResponse = await httpClient.GetAsync(linkedIn.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                // If the configured endpoint used the legacy people projection and returned a 400 syntax error,
                // retry using the standard /v2/me endpoint which is more broadly supported.
                var snippet = profileContent is null ? string.Empty : profileContent.Length > 400 ? profileContent.Substring(0, 400) + "..." : profileContent;
                _logger.LogError("LinkedIn profile fetch failed: {StatusCode}; body={Body}", profileResponse.StatusCode, snippet);

                if (profileResponse.StatusCode == System.Net.HttpStatusCode.BadRequest && linkedIn.UserInfoEndpoint.Contains("/people/"))
                {
                    var fallback = "https://api.linkedin.com/v2/me";
                    _logger.LogInformation("Retrying LinkedIn profile fetch with fallback endpoint: {Fallback}", fallback);
                    var fallbackResponse = await httpClient.GetAsync(fallback);
                    var fallbackContent = await fallbackResponse.Content.ReadAsStringAsync();

                    if (fallbackResponse.IsSuccessStatusCode)
                    {
                        profileResponse = fallbackResponse;
                        profileContent = fallbackContent;
                    }
                    else
                    {
                        var fsnippet = fallbackContent is null ? string.Empty : fallbackContent.Length > 400 ? fallbackContent.Substring(0, 400) + "..." : fallbackContent;
                        _logger.LogError("LinkedIn fallback profile fetch failed: {StatusCode}; body={Body}", fallbackResponse.StatusCode, fsnippet);
                        var err = $"Profile fetch failed: {fallbackResponse.StatusCode}";
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(fallbackContent))
                            {
                                using var doc = JsonDocument.Parse(fallbackContent);
                                if (doc.RootElement.TryGetProperty("message", out var m)) err += $" - {m.GetString()}";
                            }
                        }
                        catch (JsonException jex)
                        {
                            _logger.LogDebug(jex, "Failed to parse LinkedIn fallback error response as JSON");
                        }
                        return (false, null, null, null, null, err);
                    }
                }

                // If we didn't return from fallback, surface original error
                var err2 = $"Profile fetch failed: {profileResponse.StatusCode}";
                try
                {
                    if (!string.IsNullOrWhiteSpace(profileContent))
                    {
                        using var doc = JsonDocument.Parse(profileContent);
                        if (doc.RootElement.TryGetProperty("message", out var m)) err2 += $" - {m.GetString()}";
                    }
                }
                catch (JsonException jex)
                {
                    _logger.LogDebug(jex, "Failed to parse LinkedIn error response as JSON");
                }
                return (false, null, null, null, null, err2);
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            _logger.LogDebug("LinkedIn profile content: {ProfileContent}", profileContent);

            string? linkedInId = null;
            string? email = null;
            if (profileData.ValueKind != JsonValueKind.Undefined)
            {
                if (profileData.TryGetProperty("id", out var id)) linkedInId = id.GetString();
                else if (profileData.TryGetProperty("sub", out var sub)) linkedInId = sub.GetString();

                if (profileData.TryGetProperty("email", out var emailProp)) email = emailProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(linkedInId))
            {
                return (false, null, profileContent, null, null, "Unable to resolve LinkedIn profile id");
            }

            _logger.LogInformation("Verified LinkedIn profile id returned (not used to construct public URL)");
            return (true, linkedInId, profileContent, null, email, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkedIn OAuth callback failed");
            return (false, null, null, null, null, "LinkedIn OAuth callback failed");
        }
    }

    public async Task<(bool Success, string? ProfileUrl, string? Email, string? ErrorMessage)> HandleGitHubCallbackAsync(string code, string baseUrl)
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
                return (false, null, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.TryGetProperty("access_token", out var at) ? at.GetString() : null;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, null, null, "Token exchange returned no access token");
            }

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Visage", "1.0"));

            var profileResponse = await httpClient.GetAsync(github.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub profile fetch failed: {StatusCode}", profileResponse.StatusCode);
                return (false, null, null, "Profile fetch failed");
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            var profileUrl = profileData.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : null;
            var email = profileData.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                return (false, null, null, "Unable to resolve GitHub profile url");
            }

            _logger.LogInformation("Verified GitHub profile url derived");
            return (true, profileUrl, email, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub OAuth callback failed");
            return (false, null, null, "GitHub OAuth callback failed");
        }
    }
}
