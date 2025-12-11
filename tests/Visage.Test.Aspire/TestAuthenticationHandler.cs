using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Visage.Test.Aspire;

/// <summary>
/// Test authentication handler that bypasses Auth0 for integration tests
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string DefaultUserId = "test-user-123";
    public const string DefaultEmail = "testuser@example.com";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test claims matching Auth0 structure
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, DefaultUserId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, DefaultEmail),
            new Claim("scope", "profile:read-write"),
            // Auth0 custom claim for user ID
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", DefaultUserId),
            // Auth0 email claim
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", DefaultEmail)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
