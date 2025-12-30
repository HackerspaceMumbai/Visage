using System.Net.Http.Json;
using System.Security;
using System.Text.Json.Serialization;

namespace Visage.Test.Aspire;

/// <summary>
/// Helper class for Auth0 test authentication.
/// Uses Resource Owner Password Grant to obtain test tokens.
/// </summary>
public static class Auth0TestHelper
{
    private record Auth0TokenRequest(
        [property: JsonPropertyName("grant_type")] string GrantType,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("password")] string Password,
        [property: JsonPropertyName("client_id")] string ClientId,
        [property: JsonPropertyName("client_secret")] string ClientSecret,
        [property: JsonPropertyName("audience")] string Audience,
        [property: JsonPropertyName("scope")] string Scope
    );

    private record Auth0TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("id_token")] string IdToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );

    // Whitelist of emails allowed to use Password Grant (test accounts only)
    private static readonly HashSet<string> AllowedTestEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "test.playwright@hackmum.in",
        "e2e.test@hackmum.in",
        "ci.test@hackmum.in"
    };

    /// <summary>
    /// Checks if Auth0 test configuration is available.
    /// </summary>
    public static bool IsConfigured()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTH0_DOMAIN")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTH0_AUDIENCE")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_USER_EMAIL")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_USER_PASSWORD"));
    }

    /// <summary>
    /// Obtains an Auth0 access token for testing using Resource Owner Password Grant.
    /// Requires Auth0 tenant to have this grant enabled for the test application.
    /// SECURITY: Only whitelisted test emails are allowed to prevent production user credential abuse.
    /// </summary>
    public static async Task<string> GetTestAccessTokenAsync()
    {
        var domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? throw new InvalidOperationException("AUTH0_DOMAIN not set");
        var clientId = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID") ?? throw new InvalidOperationException("AUTH0_CLIENT_ID not set");
        var clientSecret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET") ?? throw new InvalidOperationException("AUTH0_CLIENT_SECRET not set");
        var audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE") ?? throw new InvalidOperationException("AUTH0_AUDIENCE not set");
        var username = Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? throw new InvalidOperationException("TEST_USER_EMAIL not set");
        var password = Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? throw new InvalidOperationException("TEST_USER_PASSWORD not set");

        // SECURITY: Validate that the email is in the allowed test user whitelist
        if (!AllowedTestEmails.Contains(username))
        {
            throw new SecurityException(
                $"Password Grant authentication is restricted to whitelisted test accounts only. " +
                $"Email '{username}' is not authorized. " +
                $"Allowed test emails: {string.Join(", ", AllowedTestEmails)}"
            );
        }

        // SECURITY: Prevent accidental use in production environments
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                         ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                         ?? "Production";
        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Password Grant authentication is disabled in Production environments. " +
                "Use interactive login or service principal authentication instead."
            );
        }

        using var httpClient = new HttpClient();
        var tokenRequest = new Auth0TokenRequest(
            GrantType: "password",
            Username: username,
            Password: password,
            ClientId: clientId,
            ClientSecret: clientSecret,
            Audience: audience,
            Scope: "openid profile email profile:read-write"
        );

        var response = await httpClient.PostAsJsonAsync($"https://{domain}/oauth/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<Auth0TokenResponse>();
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to obtain access token");
    }
}
