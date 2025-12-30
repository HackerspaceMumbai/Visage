# Quick Fix: Replace Auth0 Social Linking with Direct OAuth

## Problem
The LinkedIn button still uses Auth0's `/Account/LinkSocial` endpoint instead of direct OAuth.

## Solution
Make these 3 small changes to `Visage.FrontEnd.Web/Program.cs`:

### Change 1: Add using statement (line 9)
After:
```csharp
using Visage.FrontEnd.Web.Services;
```

Add:
```csharp
using Visage.FrontEnd.Web.Configuration;
```

### Change 2: Add session and OAuth configuration (after line 30)
Replace:
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

// T087: Session support and OAuth configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.AddScoped<DirectOAuthService>();

builder.AddServiceDefaults();
```

### Change 3: Add session middleware (after line 143)
After:
```csharp
app.MapStaticAssets();
```

Add:
```csharp
app.UseSession();
```

### Change 4: Replace Auth0 social endpoint (around line 155)
**DELETE** this entire section:
```csharp
// T087: Endpoint to link social account (LinkedIn/GitHub) via Auth0
app.MapGet("/Account/LinkSocial", async (HttpContext httpContext, string connection, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .WithParameter("connection", connection) // Force Auth0 to use specific social provider
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});
```

**REPLACE** with:
```csharp
// T087: Direct OAuth endpoints (LinkedIn/GitHub)
app.MapGet("/oauth/linkedin/authorize", async (HttpContext ctx, DirectOAuthService oauth, string? returnUrl) =>
{
    var state = Guid.NewGuid().ToString("N");
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    ctx.Session.SetString($"oauth_state_{state}", returnUrl ?? "/registration/mandatory");
    ctx.Session.SetString($"oauth_provider_{state}", "linkedin");
    return Results.Redirect(oauth.GetLinkedInAuthUrl(baseUrl, state, returnUrl ?? "/registration/mandatory"));
}).RequireAuthorization();

app.MapGet("/oauth/github/authorize", async (HttpContext ctx, DirectOAuthService oauth, string? returnUrl) =>
{
    var state = Guid.NewGuid().ToString("N");
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    ctx.Session.SetString($"oauth_state_{state}", returnUrl ?? "/registration/mandatory");
    ctx.Session.SetString($"oauth_provider_{state}", "github");
    return Results.Redirect(oauth.GetGitHubAuthUrl(baseUrl, state, returnUrl ?? "/registration/mandatory"));
}).RequireAuthorization();

app.MapGet("/oauth/linkedin/callback", async (HttpContext ctx, DirectOAuthService oauth, string code, string state, ILogger<Program> log) =>
{
    var returnUrl = ctx.Session.GetString($"oauth_state_{state}");
    var provider = ctx.Session.GetString($"oauth_provider_{state}");
    if (string.IsNullOrEmpty(returnUrl) || provider != "linkedin") return Results.BadRequest("Invalid state");
    
    var (success, profileUrl, error) = await oauth.HandleLinkedInCallback(code, $"{ctx.Request.Scheme}://{ctx.Request.Host}");
    ctx.Session.Remove($"oauth_state_{state}");
    ctx.Session.Remove($"oauth_provider_{state}");
    
    var url = success ? $"{returnUrl}?provider=linkedin&profileUrl={Uri.EscapeDataString(profileUrl!)}&verified=true"
                      : $"{returnUrl}?provider=linkedin&verified=false&error={Uri.EscapeDataString(error ?? "Unknown")}";
    return Results.Redirect(url);
});

app.MapGet("/oauth/github/callback", async (HttpContext ctx, DirectOAuthService oauth, string code, string state, ILogger<Program> log) =>
{
    var returnUrl = ctx.Session.GetString($"oauth_state_{state}");
    var provider = ctx.Session.GetString($"oauth_provider_{state}");
    if (string.IsNullOrEmpty(returnUrl) || provider != "github") return Results.BadRequest("Invalid state");
    
    var (success, profileUrl, error) = await oauth.HandleGitHubCallback(code, $"{ctx.Request.Scheme}://{ctx.Request.Host}");
    ctx.Session.Remove($"oauth_state_{state}");
    ctx.Session.Remove($"oauth_provider_{state}");
    
    var url = success ? $"{returnUrl}?provider=github&profileUrl={Uri.EscapeDataString(profileUrl!)}&verified=true"
                      : $"{returnUrl}?provider=github&verified=false&error={Uri.EscapeDataString(error ?? "Unknown")}";
    return Results.Redirect(url);
});
```

## Test
1. Save Program.cs
2. Rebuild solution
3. Click LinkedIn button ? should go to YOUR LinkedIn app, not Auth0!