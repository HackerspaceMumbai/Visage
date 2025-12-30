The `/Account/LinkSocial` endpoint in Program.cs is still pointing to Auth0. We need to replace it with direct OAuth endpoints.

Here's what you need to do to fix the LinkedIn button issue:

## 1. Update Program.cs

Add these using statements at the top:
```csharp
using Visage.FrontEnd.Web.Configuration;
```

Replace this section (around line 30):
```csharp
// Add authorization services 
builder.Services.AddAuthorization();

//Add 



builder.AddServiceDefaults();
```

With:
```csharp
// Add authorization services 
builder.Services.AddAuthorization();

// T087: Add session support for OAuth state management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// T087: Configure OAuth options and register DirectOAuthService
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.AddScoped<DirectOAuthService>();

builder.AddServiceDefaults();
```

After `app.MapStaticAssets();` add:
```csharp
// T087: Enable session middleware for OAuth state management
app.UseSession();
```

**REPLACE the entire `/Account/LinkSocial` endpoint** (around line 155) with these new OAuth endpoints:

```csharp
// T087: Direct OAuth endpoints for LinkedIn and GitHub profile verification
// LinkedIn OAuth authorization initiation
app.MapGet("/oauth/linkedin/authorize", async (HttpContext httpContext, DirectOAuthService oauthService, string? returnUrl) =>
{
    try
    {
        var state = Guid.NewGuid().ToString("N");
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        // Validate returnUrl: must be relative and match whitelist
        string ValidateReturnUrl(string? url)
        {
            var allowedPrefixes = new[] { "/registration/mandatory", "/account/profile" };
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/")) return "/registration/mandatory";
            foreach (var prefix in allowedPrefixes)
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return url;
            return "/registration/mandatory";
        }
        var safeReturnUrl = ValidateReturnUrl(returnUrl);
        // Store state and return URL in session
        httpContext.Session.SetString($"oauth_state_{state}", safeReturnUrl);
        httpContext.Session.SetString($"oauth_provider_{state}", "linkedin");
        var authUrl = oauthService.GetLinkedInAuthUrl(baseUrl, state);
        return Results.Redirect(authUrl);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"OAuth initialization failed: {ex.Message}");
    }
}).RequireAuthorization();

// GitHub OAuth authorization initiation
app.MapGet("/oauth/github/authorize", async (HttpContext httpContext, DirectOAuthService oauthService, string? returnUrl) =>
{
    try
    {
        var state = Guid.NewGuid().ToString("N");
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        // Validate returnUrl: must be relative and match whitelist
        string ValidateReturnUrl(string? url)
        {
            var allowedPrefixes = new[] { "/registration/mandatory", "/account/profile" };
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/")) return "/registration/mandatory";
            foreach (var prefix in allowedPrefixes)
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return url;
            return "/registration/mandatory";
        }
        var safeReturnUrl = ValidateReturnUrl(returnUrl);
        // Store state and return URL in session
        httpContext.Session.SetString($"oauth_state_{state}", safeReturnUrl);
        httpContext.Session.SetString($"oauth_provider_{state}", "github");
        var authUrl = oauthService.GetGitHubAuthUrl(baseUrl, state);
        return Results.Redirect(authUrl);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"OAuth initialization failed: {ex.Message}");
    }
}).RequireAuthorization();

// LinkedIn OAuth callback handler
app.MapGet("/oauth/linkedin/callback", async (HttpContext httpContext, DirectOAuthService oauthService, string code, string state, ILogger<Program> logger) =>
{
    try
    {
        // Validate state
        var returnUrl = httpContext.Session.GetString($"oauth_state_{state}");
        var provider = httpContext.Session.GetString($"oauth_provider_{state}");
        // Re-validate returnUrl in callback for defense-in-depth
        string ValidateReturnUrl(string? url)
        {
            var allowedPrefixes = new[] { "/registration/mandatory", "/account/profile" };
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/")) return "/registration/mandatory";
            foreach (var prefix in allowedPrefixes)
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return url;
            return "/registration/mandatory";
        }
        var safeReturnUrl = ValidateReturnUrl(returnUrl);
        if (string.IsNullOrEmpty(safeReturnUrl) || provider != "linkedin")
        {
            logger.LogWarning("Invalid OAuth state for LinkedIn callback");
            return Results.BadRequest("Invalid OAuth state");
        }

        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var (success, linkedInSubject, rawProfileJson, rawEmailJson, email, errorMessage) = await oauthService.HandleLinkedInCallback(code, baseUrl);

        // Clean up session
        httpContext.Session.Remove($"oauth_state_{state}");
        httpContext.Session.Remove($"oauth_provider_{state}");

        if (success && !string.IsNullOrEmpty(linkedInSubject))
        {
            var profileUrl = $"https://www.linkedin.com/in/{linkedInSubject}";
            var callbackUrl = $"{safeReturnUrl}?provider=linkedin&profileUrl={Uri.EscapeDataString(profileUrl)}&verified=true";
            return Results.Redirect(callbackUrl);
        }
        else
        {
            logger.LogError("LinkedIn OAuth callback failed: {Error}", errorMessage);
            var errorUrl = $"{safeReturnUrl}?provider=linkedin&verified=false&error={Uri.EscapeDataString(errorMessage ?? "Unknown error")}";
            return Results.Redirect(errorUrl);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "LinkedIn OAuth callback exception");
        return Results.BadRequest($"OAuth callback failed: {ex.Message}");
    }
});

// GitHub OAuth callback handler
app.MapGet("/oauth/github/callback", async (HttpContext httpContext, DirectOAuthService oauthService, string code, string state, ILogger<Program> logger) =>
{
    try
    {
        // Validate state
        var returnUrl = httpContext.Session.GetString($"oauth_state_{state}");
        var provider = httpContext.Session.GetString($"oauth_provider_{state}");
        // Re-validate returnUrl in callback for defense-in-depth
        string ValidateReturnUrl(string? url)
        {
            var allowedPrefixes = new[] { "/registration/mandatory", "/account/profile" };
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/")) return "/registration/mandatory";
            foreach (var prefix in allowedPrefixes)
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return url;
            return "/registration/mandatory";
        }
        var safeReturnUrl = ValidateReturnUrl(returnUrl);
        if (string.IsNullOrEmpty(safeReturnUrl) || provider != "github")
        {
            logger.LogWarning("Invalid OAuth state for GitHub callback");
            return Results.BadRequest("Invalid OAuth state");
        }

        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var (success, profileUrl, email, errorMessage) = await oauthService.HandleGitHubCallback(code, baseUrl);

        // Clean up session
        httpContext.Session.Remove($"oauth_state_{state}");
        httpContext.Session.Remove($"oauth_provider_{state}");

        if (success && !string.IsNullOrEmpty(profileUrl))
        {
            var callbackUrl = $"{safeReturnUrl}?provider=github&profileUrl={Uri.EscapeDataString(profileUrl)}&verified=true";
            return Results.Redirect(callbackUrl);
        }
        else
        {
            logger.LogError("GitHub OAuth callback failed: {Error}", errorMessage);
            var errorUrl = $"{safeReturnUrl}?provider=github&verified=false&error={Uri.EscapeDataString(errorMessage ?? "Unknown error")}";
            return Results.Redirect(errorUrl);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "GitHub OAuth callback exception");
        return Results.BadRequest($"OAuth callback failed: {ex.Message}");
    }
});
```

## 2. Update SocialAuthService.cs

The GetLinkedInAuthUrlAsync and GetGitHubAuthUrlAsync methods should return:
```csharp
public Task<string> GetLinkedInAuthUrlAsync()
{
    return Task.FromResult("/oauth/linkedin/authorize?returnUrl=/registration/mandatory");
}

public Task<string> GetGitHubAuthUrlAsync()
{
    return Task.FromResult("/oauth/github/authorize?returnUrl=/registration/mandatory");
}
```

## 3. Update MandatoryRegistration.razor

The ConnectLinkedIn and ConnectGitHub methods should use `forceLoad: true`:
```csharp
private async Task ConnectLinkedIn()
{
    var authUrl = await SocialAuthService.GetLinkedInAuthUrlAsync();
    Navigation.NavigateTo(authUrl, forceLoad: true);
}

private async Task ConnectGitHub()
{
    var authUrl = await SocialAuthService.GetGitHubAuthUrlAsync();
    Navigation.NavigateTo(authUrl, forceLoad: true);
}
```

After making these changes:
1. Rebuild the solution
2. Restart the app
3. The LinkedIn/GitHub buttons will now go to YOUR OAuth apps, not Auth0!