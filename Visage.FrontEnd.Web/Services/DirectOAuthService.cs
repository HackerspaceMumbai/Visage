using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using Visage.FrontEnd.Web.Configuration;

namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// T087: Direct OAuth service for LinkedIn and GitHub profile verification
/// This bypasses Auth0 and goes directly to the providers to verify profile ownership
/// </summary>
public class DirectOAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DirectOAuthService> _logger;

    public DirectOAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DirectOAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetLinkedInAuthUrl(string baseUrl, string state, string returnUrl)
    {
        var oauthOptions = _configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>();
        var linkedIn = oauthOptions?.LinkedIn;

        if (linkedIn == null || string.IsNullOrEmpty(linkedIn.ClientId))
        {
            throw new InvalidOperationException("LinkedIn OAuth not configured");
        }

        var redirectUri = $"{baseUrl}{linkedIn.CallbackPath}";
        return $"{linkedIn.AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(linkedIn.ClientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&scope={Uri.EscapeDataString(linkedIn.Scope)}";
    }

    public string GetGitHubAuthUrl(string baseUrl, string state, string returnUrl)
    {
        var oauthOptions = _configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>();
        var github = oauthOptions?.GitHub;

        if (github == null || string.IsNullOrEmpty(github.ClientId))
        {
            throw new InvalidOperationException("GitHub OAuth not configured");
        }

        var redirectUri = $"{baseUrl}{github.CallbackPath}";
        return $"{github.AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(github.ClientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&scope={Uri.EscapeDataString(github.Scope)}";
    }

    public async Task<(bool Success, string? ProfileUrl, string? ErrorMessage)> HandleLinkedInCallback(string code, string baseUrl)
    {
        var oauthOptions = _configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>();
        var linkedIn = oauthOptions?.LinkedIn;

        if (linkedIn == null)
        {
            return (false, null, "LinkedIn OAuth not configured");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var redirectUri = $"{baseUrl}{linkedIn.CallbackPath}";

            // Exchange code for access token
            var tokenRequest = new FormUrlEncodedContent(new[]
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
                _logger.LogError("LinkedIn token exchange failed: {Error}", tokenContent);
                return (false, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            // Get user profile
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var profileResponse = await httpClient.GetAsync(linkedIn.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogError("LinkedIn profile fetch failed: {Error}", profileContent);
                return (false, null, "Profile fetch failed");
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            var linkedInId = profileData.GetProperty("id").GetString();

            // Construct LinkedIn profile URL
            var profileUrl = $"https://www.linkedin.com/in/{linkedInId}";

            _logger.LogInformation("Successfully verified LinkedIn profile: {ProfileUrl}", profileUrl);
            return (true, profileUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkedIn OAuth callback failed");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ProfileUrl, string? ErrorMessage)> HandleGitHubCallback(string code, string baseUrl)
    {
        var oauthOptions = _configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>();
        var github = oauthOptions?.GitHub;

        if (github == null)
        {
            return (false, null, "GitHub OAuth not configured");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var redirectUri = $"{baseUrl}{github.CallbackPath}";

            // Exchange code for access token
            var tokenRequest = new FormUrlEncodedContent(new[]
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
                _logger.LogError("GitHub token exchange failed: {Error}", tokenContent);
                return (false, null, "Token exchange failed");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            // Get user profile
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Visage", "1.0"));

            var profileResponse = await httpClient.GetAsync(github.UserInfoEndpoint);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub profile fetch failed: {Error}", profileContent);
                return (false, null, "Profile fetch failed");
            }

            var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);
            var profileUrl = profileData.GetProperty("html_url").GetString();

            _logger.LogInformation("Successfully verified GitHub profile: {ProfileUrl}", profileUrl);
            return (true, profileUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub OAuth callback failed");
            return (false, null, ex.Message);
        }
    }
}