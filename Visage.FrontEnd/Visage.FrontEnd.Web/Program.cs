using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Visage.FrontEnd.Shared.Services;
using Visage.FrontEnd.Web.Components;
using Visage.FrontEnd.Web.Configuration;
using Visage.FrontEnd.Web.Services;
using Visage.Shared.Models; // Add this using directive

var builder = WebApplication.CreateBuilder(args);


//Log all the auth0 configuration values
Console.WriteLine("Auth0Domain " + builder.Configuration["Auth0:Domain"]);
Console.WriteLine("Auth0ClientId " + builder.Configuration["Auth0:ClientId"]);
Console.WriteLine("Auth0ClientSecret " + builder.Configuration["Auth0:ClientSecret"]);
Console.WriteLine("Auth0Audience " + builder.Configuration["Auth0:Audience"]);

builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        options.Scope = "openid profile email offline_access profile:read-write";  

    })
    .WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.UseRefreshTokens = true; // Enable refresh tokens
    });

// Add authorization services 
builder.Services.AddAuthorization();

// Direct OAuth (LinkedIn/GitHub) configuration + services (US1)
builder.Services.AddOptions<OAuthOptions>()
    .Bind(builder.Configuration.GetSection(OAuthOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<DirectOAuthService>();

// Session storage for OAuth state (CSRF protection) + returnUrl
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});



builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the Visage.FrontEnd.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// T015: Register IMemoryCache for event caching
builder.Services.AddMemoryCache();

// T087: Register RegistrationDraftService for persisting form state across redirects
builder.Services.AddScoped<IRegistrationDraftService, ServerRegistrationDraftService>();

// Add the delegating handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CircuitAccessTokenProvider>(); // Token cache for Blazor Server circuit
builder.Services.AddScoped<AuthenticationDelegatingHandler>();

// Image CDN transformation options and service
builder.Services.Configure<ImageCdnOptions>(builder.Configuration.GetSection("ImageCdn"));
builder.Services.AddScoped<IImageUrlTransformer, ConfigImageUrlTransformer>();



// T014: Register typed HttpClients for backend services via Aspire Service Discovery.
// With ServiceDefaults configured, setting BaseAddress to the resource name enables
// automatic resolution to the correct endpoint in all environments.
// Use the special "https+http" scheme to prefer HTTPS and fall back to HTTP in dev.

builder.Services.AddHttpClient<IEventService, EventService>(client =>
{
    client.BaseAddress = new Uri("https+http://eventing");
});

builder.Services.AddHttpClient<ICloudinaryImageSigningService, CloudinaryImageSigningService>(client =>
{
    client.BaseAddress = new Uri("https+http://cloudinary-image-signing");
});

builder.Services.AddHttpClient<IUserProfileService, UserProfileService>(client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
})
   .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IRegistrationService, RegistrationService>(client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

// T034: Register HttpClient for ProfileService calling backend API directly
// In Blazor Server, ProfileService runs server-side and can use AuthenticationDelegatingHandler
builder.Services.AddHttpClient<IProfileService, ProfileService>(client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

// T087: Register SocialAuthService for OAuth-based social profile linking
builder.Services.AddHttpClient<ISocialAuthService, SocialAuthService>(client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

// BFF endpoint for profile completion status
builder.Services.AddHttpClient("userprofile-api-bff", client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
});

// Named client for persisting direct OAuth results to UserProfile API.
builder.Services.AddHttpClient("userprofile-api-direct", client =>
{
    client.BaseAddress = new Uri("https+http://userprofile-api");
});




// Register the IUserService and UserService in the dependency injection container
//builder.Services.AddHttpClient<IUserService, UserService>(client =>
//    client.BaseAddress = new Uri("https+http://auth0"));

// Read Clarity Project ID from environment/configuration
var clarityProjectId = builder.Configuration["Clarity:ProjectId"] ?? builder.Configuration["Clarity__ProjectId"];

// Register as singleton for DI
builder.Services.AddSingleton(new ClarityConfig { ProjectId = clarityProjectId });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();

app.UseSession();


app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

// T087: Endpoint to link social account (LinkedIn/GitHub) via Auth0
app.MapGet("/Account/LinkSocial", async (HttpContext httpContext, string connection, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .WithParameter("connection", connection) // Force Auth0 to use specific social provider
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

// US1: Direct OAuth start endpoints (bypass Auth0 social connections)
app.MapGet("/oauth/linkedin/start", (HttpContext httpContext, DirectOAuthService oAuth, string? returnUrl) =>
{
    var safeReturnUrl = GetSafeReturnUrl(returnUrl) ?? "/registration/mandatory";
    var state = CreateOpaqueState();

    httpContext.Session.SetString("oauth:linkedin:state", state);
    httpContext.Session.SetString("oauth:linkedin:returnUrl", safeReturnUrl);
    httpContext.Session.SetString("oauth:linkedin:createdAtUtc", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
    var authUrl = oAuth.GetLinkedInAuthUrl(baseUrl, state);
    return Results.Redirect(authUrl);
}).RequireAuthorization();

app.MapGet("/oauth/github/start", (HttpContext httpContext, DirectOAuthService oAuth, string? returnUrl) =>
{
    var safeReturnUrl = GetSafeReturnUrl(returnUrl) ?? "/registration/mandatory";
    var state = CreateOpaqueState();

    httpContext.Session.SetString("oauth:github:state", state);
    httpContext.Session.SetString("oauth:github:returnUrl", safeReturnUrl);
    httpContext.Session.SetString("oauth:github:createdAtUtc", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
    var authUrl = oAuth.GetGitHubAuthUrl(baseUrl, state);
    return Results.Redirect(authUrl);
}).RequireAuthorization();

// US1: Direct OAuth callback endpoints
app.MapGet("/oauth/linkedin/callback", async (
    HttpContext httpContext,
    DirectOAuthService oAuth,
    IHttpClientFactory httpClientFactory,
    IRegistrationDraftService registrationDraftService,
    ILogger<Program> logger,
    Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
    string? code,
    string? state,
    string? error,
    string? error_description) =>
{
    var returnUrl = httpContext.Session.GetString("oauth:linkedin:returnUrl") ?? "/registration/mandatory";
    returnUrl = GetSafeReturnUrl(returnUrl) ?? "/registration/mandatory";

    var expectedState = httpContext.Session.GetString("oauth:linkedin:state");
    var createdAtRaw = httpContext.Session.GetString("oauth:linkedin:createdAtUtc");

    // Single-use state: clear immediately
    httpContext.Session.Remove("oauth:linkedin:state");
    httpContext.Session.Remove("oauth:linkedin:createdAtUtc");
    httpContext.Session.Remove("oauth:linkedin:returnUrl");

    if (!string.IsNullOrWhiteSpace(error))
    {
        logger.LogInformation("LinkedIn OAuth returned error: {Error}", error);
        var qs = new Dictionary<string, string?>
        {
            ["social"] = "linkedin",
            ["result"] = "error"
        };
        if (env.IsDevelopment()) qs["reason"] = error;
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, qs));
    }

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(expectedState))
    {
        logger.LogWarning("LinkedIn callback missing parameters: code={HasCode}, state={HasState}, expectedState={HasExpectedState}", 
            !string.IsNullOrWhiteSpace(code), !string.IsNullOrWhiteSpace(state), !string.IsNullOrWhiteSpace(expectedState));

        var qs = new Dictionary<string, string?>
        {
            ["social"] = "linkedin",
            ["result"] = "state_invalid"
        };
        if (env.IsDevelopment()) qs["reason"] = "missing_code_or_state";
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, qs));
    }

    if (!CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(state), System.Text.Encoding.UTF8.GetBytes(expectedState)))
    {
        var qs = new Dictionary<string, string?>
        {
            ["social"] = "linkedin",
            ["result"] = "state_invalid"
        };
        if (env.IsDevelopment()) qs["reason"] = "state_mismatch";
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, qs));
    }

    if (!long.TryParse(createdAtRaw, out var createdAtSeconds) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() - createdAtSeconds > 600)
    {
        var qs = new Dictionary<string, string?>
        {
            ["social"] = "linkedin",
            ["result"] = "state_expired"
        };
        if (env.IsDevelopment()) qs["reason"] = "state_expired";
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, qs));
    }

    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
    logger.LogInformation("LinkedIn callback invoked; baseUrl={BaseUrl}", baseUrl);
    var (success, linkedInSubject, rawProfileJson, rawEmailJson, email, errorMessage) = await oAuth.HandleLinkedInCallbackAsync(code, baseUrl);
    if (!success)
    {
        logger.LogWarning("LinkedIn OAuth verification failed: {Message}", errorMessage);
        var qs = new Dictionary<string, string?>
        {
            ["social"] = "linkedin",
            ["result"] = "error"
        };
        if (env.IsDevelopment() && !string.IsNullOrWhiteSpace(errorMessage)) qs["reason"] = errorMessage;
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, qs));
    }

    if (!string.IsNullOrWhiteSpace(linkedInSubject))
    {
        httpContext.Session.SetString("oauth:linkedin:subject", linkedInSubject);
    }

    if (!string.IsNullOrWhiteSpace(rawProfileJson))
    {
        httpContext.Session.SetString("oauth:linkedin:rawProfileJson", rawProfileJson);
    }

    if (!string.IsNullOrWhiteSpace(rawEmailJson))
    {
        httpContext.Session.SetString("oauth:linkedin:rawEmailJson", rawEmailJson);
    }

    if (!string.IsNullOrWhiteSpace(email))
    {
        httpContext.Session.SetString("oauth:linkedin:pendingEmail", email);
    }

    try
    {
        var draft = await registrationDraftService.GetDraftAsync();
        if (draft == null)
        {
            draft = new Visage.Shared.Models.User();
        }

        draft.IsLinkedInVerified = true;
        draft.LinkedInSubject = linkedInSubject;
        draft.LinkedInRawProfileJson = rawProfileJson;
        draft.LinkedInRawEmailJson = rawEmailJson;
        draft.LinkedInPayloadFetchedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(draft.Email) && !string.IsNullOrWhiteSpace(email))
        {
            draft.Email = email;
        }

        await registrationDraftService.SaveDraftAsync(draft);
        logger.LogInformation("Persisted LinkedIn pending verification into registration draft cache");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to persist LinkedIn pending verification into registration draft cache");
    }

    return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
    {
        ["social"] = "linkedin",
        ["result"] = "success"
    }));
}).RequireAuthorization();

// US1: Endpoint to retrieve pending social profiles from session (used by MandatoryRegistration)
app.MapGet("/api/profile/social/pending", (HttpContext httpContext) =>
{
    var linkedin = httpContext.Session.GetString("oauth:linkedin:pendingProfileUrl");
    var linkedinEmail = httpContext.Session.GetString("oauth:linkedin:pendingEmail");
    var linkedinSubject = httpContext.Session.GetString("oauth:linkedin:subject");
    var linkedinRawProfileJson = httpContext.Session.GetString("oauth:linkedin:rawProfileJson");
    var linkedinRawEmailJson = httpContext.Session.GetString("oauth:linkedin:rawEmailJson");

    var github = httpContext.Session.GetString("oauth:github:pendingProfileUrl");
    var githubEmail = httpContext.Session.GetString("oauth:github:pendingEmail");

    if (string.IsNullOrEmpty(linkedin) && string.IsNullOrEmpty(github) && string.IsNullOrEmpty(linkedinSubject) && string.IsNullOrEmpty(linkedinRawProfileJson))
    {
        return Results.NoContent();
    }

    return Results.Ok(new Visage.Shared.Models.PendingSocialProfilesDto
    {
        LinkedInProfile = linkedin,
        LinkedInEmail = linkedinEmail,
        LinkedInSubject = linkedinSubject,
        LinkedInRawProfileJson = linkedinRawProfileJson,
        LinkedInRawEmailJson = linkedinRawEmailJson,
        GitHubProfile = github,
        GitHubEmail = githubEmail
    });
}).RequireAuthorization();

// US1: Endpoint to clear pending social profiles (session + draft). Used when user clicks Disconnect
// before completing registration (no registrant row exists yet).
app.MapPost("/api/profile/social/pending/clear", async (
    HttpContext httpContext,
    IRegistrationDraftService registrationDraftService,
    string provider) =>
{
    if (string.IsNullOrWhiteSpace(provider))
    {
        return Results.BadRequest(new { error = "provider is required" });
    }

    var normalized = provider.Trim().ToLowerInvariant();
    switch (normalized)
    {
        case "linkedin":
            httpContext.Session.Remove("oauth:linkedin:pendingProfileUrl");
            httpContext.Session.Remove("oauth:linkedin:pendingEmail");
            httpContext.Session.Remove("oauth:linkedin:subject");
            httpContext.Session.Remove("oauth:linkedin:rawProfileJson");
            httpContext.Session.Remove("oauth:linkedin:rawEmailJson");
            break;
        case "github":
            httpContext.Session.Remove("oauth:github:pendingProfileUrl");
            httpContext.Session.Remove("oauth:github:pendingEmail");
            break;
        default:
            return Results.BadRequest(new { error = $"Invalid provider: {provider}" });
    }

    var draft = await registrationDraftService.GetDraftAsync();
    if (draft is not null)
    {
        if (normalized == "linkedin")
        {
            draft.LinkedInProfile = null;
            draft.IsLinkedInVerified = false;
        }
        else
        {
            draft.GitHubProfile = null;
            draft.IsGitHubVerified = false;
        }

        await registrationDraftService.SaveDraftAsync(draft);
    }

    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/oauth/github/callback", async (
    HttpContext httpContext,
    DirectOAuthService oAuth,
    IHttpClientFactory httpClientFactory,
    IRegistrationDraftService registrationDraftService,
    ILogger<Program> logger,
    string? code,
    string? state,
    string? error,
    string? error_description) =>
{
    var returnUrl = httpContext.Session.GetString("oauth:github:returnUrl") ?? "/registration/mandatory";
    returnUrl = GetSafeReturnUrl(returnUrl) ?? "/registration/mandatory";

    var expectedState = httpContext.Session.GetString("oauth:github:state");
    var createdAtRaw = httpContext.Session.GetString("oauth:github:createdAtUtc");

    // Single-use state: clear immediately
    httpContext.Session.Remove("oauth:github:state");
    httpContext.Session.Remove("oauth:github:createdAtUtc");
    httpContext.Session.Remove("oauth:github:returnUrl");

    if (!string.IsNullOrWhiteSpace(error))
    {
        logger.LogInformation("GitHub OAuth returned error: {Error}", error);
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
        {
            ["social"] = "github",
            ["result"] = "error"
        }));
    }

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(expectedState))
    {
        logger.LogWarning("GitHub callback missing parameters: code={HasCode}, state={HasState}, expectedState={HasExpectedState}", 
            !string.IsNullOrWhiteSpace(code), !string.IsNullOrWhiteSpace(state), !string.IsNullOrWhiteSpace(expectedState));

        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
        {
            ["social"] = "github",
            ["result"] = "state_invalid"
        }));
    }

    if (!CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(state), System.Text.Encoding.UTF8.GetBytes(expectedState)))
    {
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
        {
            ["social"] = "github",
            ["result"] = "state_invalid"
        }));
    }

    if (!long.TryParse(createdAtRaw, out var createdAtSeconds) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() - createdAtSeconds > 600)
    {
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
        {
            ["social"] = "github",
            ["result"] = "state_expired"
        }));
    }

    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
    var (success, profileUrl, email, errorMessage) = await oAuth.HandleGitHubCallbackAsync(code, baseUrl);
    if (!success || string.IsNullOrWhiteSpace(profileUrl))
    {
        logger.LogWarning("GitHub OAuth verification failed: {Message}", errorMessage);
        return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
        {
            ["social"] = "github",
            ["result"] = "error"
        }));
    }

    // T087: Store in session
    httpContext.Session.SetString("oauth:github:pendingProfileUrl", profileUrl);
    if (!string.IsNullOrWhiteSpace(email))
    {
        httpContext.Session.SetString("oauth:github:pendingEmail", email);
    }

    // T087: Also persist into the server-side registration draft cache (keyed by Auth0 sub)
    // to survive Blazor Server circuit teardown caused by external redirects.
    try
    {
        var draft = await registrationDraftService.GetDraftAsync();
        if (draft == null)
        {
            draft = new Visage.Shared.Models.User();
        }
        draft.GitHubProfile = profileUrl;
        draft.IsGitHubVerified = true;

        if (string.IsNullOrWhiteSpace(draft.Email) && !string.IsNullOrWhiteSpace(email))
        {
            draft.Email = email;
        }

        await registrationDraftService.SaveDraftAsync(draft);
        logger.LogInformation("Persisted GitHub pending profile into registration draft cache");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to persist GitHub pending profile into registration draft cache");
    }

    return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
    {
        ["social"] = "github",
        ["result"] = "success"
    }));
}).RequireAuthorization();

app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});


app.UseAuthentication(); // This should come before UseAuthorization
app.UseAuthorization();  // This requires AddAuthorization() to be called

// BFF endpoint for profile completion status - must be after UseAuthentication/UseAuthorization
app.MapGet("/bff/profile/completion-status", async (HttpContext httpContext, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    logger.LogInformation("[BFF] /bff/profile/completion-status called");
    
    if (!httpContext.User.Identity?.IsAuthenticated == true)
    {
        logger.LogWarning("[BFF] User not authenticated");
        return Results.Unauthorized();
    }

    var userId = httpContext.User.FindFirst("sub")?.Value ?? httpContext.User.Identity.Name;
    logger.LogInformation("[BFF] Authenticated user: {UserId}", userId);

    var accessToken = await httpContext.GetTokenAsync("access_token");
    if (string.IsNullOrWhiteSpace(accessToken))
    {
        logger.LogWarning("[BFF] No access token found");
        return Results.Unauthorized();
    }

    logger.LogInformation("[BFF] Access token retrieved (length: {Length})", accessToken.Length);

    var client = httpClientFactory.CreateClient("userprofile-api-bff");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    
    logger.LogInformation("[BFF] Calling backend API: {BaseAddress}/api/profile/completion-status", client.BaseAddress);
    var response = await client.GetAsync("/api/profile/completion-status");
    
    logger.LogInformation("[BFF] Backend API response: {StatusCode}", response.StatusCode);
    
    // Return the response content with proper status code
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    var content = await response.Content.ReadAsStringAsync();
    
    logger.LogInformation("[BFF] Returning response to client (status: {StatusCode}, content length: {Length})", 
        (int)response.StatusCode, content.Length);
    
    return Results.Content(content, contentType, statusCode: (int)response.StatusCode);
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Visage.FrontEnd.Shared._Imports).Assembly,
        typeof(Visage.FrontEnd.Web.Client._Imports).Assembly);

app.Run();

static string CreateOpaqueState()
{
    Span<byte> bytes = stackalloc byte[32];
    RandomNumberGenerator.Fill(bytes);
    return WebEncoders.Base64UrlEncode(bytes);
}

static string? GetSafeReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return null;

    // Allow only local relative paths to avoid open redirects.
    if (!returnUrl.StartsWith("/", StringComparison.Ordinal))
        return null;

    if (returnUrl.StartsWith("//", StringComparison.Ordinal))
        return null;

    return returnUrl;
}
