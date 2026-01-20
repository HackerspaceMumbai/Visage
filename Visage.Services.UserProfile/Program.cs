using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Visage.Services.UserProfile;
using Visage.Shared.Models;

Console.WriteLine("Starting Visage User Profile Service...");

var builder = WebApplication.CreateBuilder(args);

// Auth0 configuration values
Console.WriteLine("Auth0Domain: " + builder.Configuration["Auth0:Domain"]);
Console.WriteLine("Auth0Audience: " + builder.Configuration["Auth0:Audience"]);

// T024: Add Aspire service defaults (health checks, OpenTelemetry, service discovery)
builder.AddServiceDefaults();

// Add authentication (JWT Bearer for API)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        // Use the authority (issuer) provided by Auth0
        var domain = builder.Configuration["Auth0:Domain"];
        var audience = builder.Configuration["Auth0:Audience"];
        c.Authority = $"https://{domain}/";

        // Prefer setting the Audience property; ensure Issuer matches the authority URL
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = audience,
            ValidIssuer = c.Authority,
            // Ensure the 'sub' claim is exposed as the Name/NameIdentifier so downstream code can find it reliably
            NameClaimType = "sub",
            RoleClaimType = "roles"
        };

        // Helpful diagnostics during development: log token validation failures and token contents
        c.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("JwtEvents");
                logger?.LogInformation("JwtEvents: OnMessageReceived - Authorization header present: {HasAuth}", ctx.Request.Headers.ContainsKey("Authorization"));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("JwtEvents");
                logger?.LogError(ctx.Exception, "JwtEvents: Authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("JwtEvents");
                logger?.LogInformation("JwtEvents: Token validated for {Sub}", ctx.Principal?.FindFirst("sub")?.Value);
                
                // Only log detailed claims in development to prevent PII exposure
                var env = ctx.HttpContext.RequestServices.GetService<IHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    foreach (var claim in ctx.Principal?.Claims ?? Array.Empty<System.Security.Claims.Claim>())
                    {
                        logger?.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                }
                
                return Task.CompletedTask;
            }
        };
    });

// Add authorization services
// T037: Simplified - just require authenticated user with valid JWT
// Auth0 always includes "sub" claim which uniquely identifies the user
builder.Services.AddAuthorization();

// T024: Configure Aspire-managed SQL Server DbContext (replaces manual connection string)
// T027: Connection string is now managed by Aspire, no hardcoded values
builder.AddSqlServerDbContext<UserDB>("userprofiledb");

// T009: Register ProfileCompletionRepository
builder.Services.AddScoped<Visage.Services.UserProfile.Repositories.ProfileCompletionRepository>();

// T028: Register UserPreferencesRepository
builder.Services.AddScoped<Visage.Services.UserProfile.Repositories.UserPreferencesRepository>();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields =
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders |
        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
    // Do not log Authorization header to prevent bearer tokens from being captured
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// T026: Run EF Core migrations automatically on service startup
// NOTE: When running under Aspire, the connection string is injected via service discovery.
// For non-Aspire runs, set ConnectionStrings__registrationdb or use `dotnet ef database update`.
if (app.Environment.IsDevelopment() ||
    bool.TryParse(app.Configuration["MIGRATE_ON_STARTUP"], out var migrateOnStartup) && migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var userDb = scope.ServiceProvider.GetRequiredService<UserDB>();
    Console.WriteLine("Ensuring EF Core database exists...");
    try
    {
        await userDb.Database.MigrateAsync();
        Console.WriteLine("Database is ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Visage Registration API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpLogging();

// User profile creation/update endpoint
app.MapPost("/api/users", async Task<Results<Created<User>, Ok<User>, BadRequest>> (
    [FromBody] User inputUser,
    UserDB db,
    HttpContext httpContext,
    ILogger<Program> logger) =>
{
    var auth0Subject =
        httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value;

    if (string.IsNullOrWhiteSpace(auth0Subject))
    {
        logger.LogWarning("User upsert rejected: missing Auth0 subject claim");
        return TypedResults.BadRequest();
    }

    inputUser.Auth0Subject = auth0Subject;

    if (string.IsNullOrWhiteSpace(inputUser.Email))
    {
        logger.LogWarning("User upsert rejected: missing email address");
        return TypedResults.BadRequest();
    }

    try
    {
        inputUser.Email = inputUser.Email.Trim();

        // Find user by Auth0Subject to ensure authenticated ownership
        var existing = await db.Users
            .FirstOrDefaultAsync(u => u.Auth0Subject == auth0Subject);

        if (existing is null)
        {
            inputUser.CreatedAt = DateTime.UtcNow;
            await db.Users.AddAsync(inputUser);
            await db.SaveChangesAsync();
            logger.LogInformation("Created user for {Email}", inputUser.Email);
            return TypedResults.Created($"/api/users/{inputUser.Id}", inputUser);
        }

        existing.Auth0Subject = auth0Subject;

        existing.FirstName = inputUser.FirstName;
        existing.MiddleName = inputUser.MiddleName;
        existing.LastName = inputUser.LastName;
        existing.Email = inputUser.Email;
        existing.MobileNumber = inputUser.MobileNumber;
        existing.AddressLine1 = inputUser.AddressLine1;
        existing.AddressLine2 = inputUser.AddressLine2;
        existing.City = inputUser.City;
        existing.State = inputUser.State;
        existing.PostalCode = inputUser.PostalCode;
        existing.GovtIdLast4Digits = inputUser.GovtIdLast4Digits;
        existing.GovtIdType = inputUser.GovtIdType;
        existing.OccupationStatus = inputUser.OccupationStatus;
        existing.CompanyName = inputUser.CompanyName;
        existing.EducationalInstituteName = inputUser.EducationalInstituteName;

        // Social profiles (optional depending on occupation)
        existing.LinkedInProfile = inputUser.LinkedInProfile;
        existing.LinkedInVanityName = inputUser.LinkedInVanityName;
        existing.LinkedInSubject = inputUser.LinkedInSubject;
        existing.LinkedInRawProfileJson = inputUser.LinkedInRawProfileJson;
        existing.LinkedInRawEmailJson = inputUser.LinkedInRawEmailJson;
        existing.LinkedInPayloadFetchedAt = inputUser.LinkedInPayloadFetchedAt;

        existing.GitHubProfile = inputUser.GitHubProfile;
        existing.IsLinkedInVerified = inputUser.IsLinkedInVerified;
        existing.IsGitHubVerified = inputUser.IsGitHubVerified;
        existing.LinkedInVerifiedAt = inputUser.IsLinkedInVerified ? (inputUser.LinkedInVerifiedAt ?? existing.LinkedInVerifiedAt ?? DateTime.UtcNow) : null;
        existing.GitHubVerifiedAt = inputUser.IsGitHubVerified ? (inputUser.GitHubVerifiedAt ?? existing.GitHubVerifiedAt ?? DateTime.UtcNow) : null;

        existing.IsProfileComplete = true;
        existing.ProfileCompletedAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated user for {Email}", existing.Email);
        return TypedResults.Ok(existing);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "User upsert failed for {Email}", inputUser.Email);
        return TypedResults.BadRequest();
    }
}).RequireAuthorization();

// Get all users endpoint
app.MapGet("/api/users", async Task<IEnumerable<User>> (UserDB db) =>
{
    return await db.Users.ToListAsync();
}).RequireAuthorization();

// Event registration endpoint
app.MapPost("/api/registrations", async Task<Results<Created<EventRegistration>, BadRequest<string>>> (
    [FromBody] EventRegistration registration,
    UserDB db,
    HttpContext httpContext,
    ILogger<Program> logger) =>
{
    var auth0Subject =
        httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value;

    if (string.IsNullOrWhiteSpace(auth0Subject))
    {
        logger.LogWarning("Event registration rejected: missing Auth0 subject claim");
        return TypedResults.BadRequest("Authentication required");
    }

    if (registration.EventId == default)
    {
        logger.LogWarning("Event registration rejected: missing EventId");
        return TypedResults.BadRequest("EventId is required");
    }

    try
    {
        // Resolve user by Auth0Subject to ensure authenticated ownership
        var user = await db.Users.FirstOrDefaultAsync(u => u.Auth0Subject == auth0Subject);
        if (user is null)
        {
            logger.LogWarning("Event registration rejected: User profile not found for Auth0Subject");
            return TypedResults.BadRequest("User profile not found");
        }

        // Set UserId and Auth0Subject from authenticated user
        registration.UserId = user.Id;
        registration.Auth0Subject = auth0Subject;

        // Check if already registered for this event
        var existingRegistration = await db.EventRegistrations
            .FirstOrDefaultAsync(r => r.UserId == registration.UserId && r.EventId == registration.EventId);

        if (existingRegistration is not null)
        {
            logger.LogWarning("Event registration rejected: User {UserId} already registered for event {EventId}",
                registration.UserId, registration.EventId);
            return TypedResults.BadRequest("User is already registered for this event");
        }

        registration.RegisteredAt = DateTime.UtcNow;
        registration.Status = RegistrationStatus.Pending;

        await db.EventRegistrations.AddAsync(registration);
        await db.SaveChangesAsync();

        logger.LogInformation("User {UserId} registered for event {EventId}", registration.UserId, registration.EventId);
        return TypedResults.Created($"/api/registrations/{registration.Id}", registration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Event registration failed for User {UserId}, Event {EventId}",
            registration.UserId, registration.EventId);
        return TypedResults.BadRequest("Registration failed");
    }
}).RequireAuthorization();

// Get user's event registrations
app.MapGet("/api/users/{userId}/registrations", async Task<IEnumerable<EventRegistration>> (
    string userId,
    UserDB db,
    ILogger<Program> logger) =>
{
    if (!StrictId.Id<User>.TryParse(userId, out var parsedUserId))
    {
        logger.LogWarning("Invalid userId format: {UserId}", userId);
        return [];
    }

    return await db.EventRegistrations
        .Where(r => r.UserId == parsedUserId)
        .ToListAsync();
}).RequireAuthorization();

// Legacy endpoint for backward compatibility
app.MapPost("/register", async Task<Results<Created<User>, Ok<User>, BadRequest>> (
    [FromBody] User inputUser,
    UserDB db,
    HttpContext httpContext,
    ILogger<Program> logger) =>
{
    var auth0Subject =
        httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value;

    if (string.IsNullOrWhiteSpace(auth0Subject))
    {
        logger.LogWarning("Registration upsert rejected: missing Auth0 subject claim");
        return TypedResults.BadRequest();
    }

    inputUser.Auth0Subject = auth0Subject;

    if (string.IsNullOrWhiteSpace(inputUser.Email))
    {
        logger.LogWarning("Registration upsert rejected: missing email address");
        return TypedResults.BadRequest();
    }

    try
    {
        inputUser.Email = inputUser.Email.Trim();

        User? existing = null;

        if (inputUser.Id != default)
        {
            existing = await db.Users.FirstOrDefaultAsync(u => u.Id == inputUser.Id);
        }

        existing ??= await db.Users
            .OrderByDescending(u => u.ProfileCompletedAt)
            .FirstOrDefaultAsync(u => u.Email == inputUser.Email);

        if (existing is null)
        {
            inputUser.CreatedAt = DateTime.UtcNow;
            await db.Users.AddAsync(inputUser);
            await db.SaveChangesAsync();
            logger.LogInformation("Created user for {Email}", inputUser.Email);
            return TypedResults.Created($"/register/{inputUser.Id}", inputUser);
        }

        existing.Auth0Subject = auth0Subject;

        existing.FirstName = inputUser.FirstName;
        existing.MiddleName = inputUser.MiddleName;
        existing.LastName = inputUser.LastName;
        existing.Email = inputUser.Email;
        existing.MobileNumber = inputUser.MobileNumber;
        existing.AddressLine1 = inputUser.AddressLine1;
        existing.AddressLine2 = inputUser.AddressLine2;
        existing.City = inputUser.City;
        existing.State = inputUser.State;
        existing.PostalCode = inputUser.PostalCode;
        existing.GovtIdLast4Digits = inputUser.GovtIdLast4Digits;
        existing.GovtIdType = inputUser.GovtIdType;
        existing.OccupationStatus = inputUser.OccupationStatus;
        existing.CompanyName = inputUser.CompanyName;
        existing.EducationalInstituteName = inputUser.EducationalInstituteName;

        // Social profiles
        existing.LinkedInProfile = inputUser.LinkedInProfile;
        existing.LinkedInVanityName = inputUser.LinkedInVanityName;
        existing.LinkedInSubject = inputUser.LinkedInSubject;
        existing.LinkedInRawProfileJson = inputUser.LinkedInRawProfileJson;
        existing.LinkedInRawEmailJson = inputUser.LinkedInRawEmailJson;
        existing.LinkedInPayloadFetchedAt = inputUser.LinkedInPayloadFetchedAt;

        existing.GitHubProfile = inputUser.GitHubProfile;
        existing.IsLinkedInVerified = inputUser.IsLinkedInVerified;
        existing.IsGitHubVerified = inputUser.IsGitHubVerified;
        existing.LinkedInVerifiedAt = inputUser.IsLinkedInVerified ? (inputUser.LinkedInVerifiedAt ?? existing.LinkedInVerifiedAt ?? DateTime.UtcNow) : null;
        existing.GitHubVerifiedAt = inputUser.IsGitHubVerified ? (inputUser.GitHubVerifiedAt ?? existing.GitHubVerifiedAt ?? DateTime.UtcNow) : null;

        existing.IsProfileComplete = true;
        existing.ProfileCompletedAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated user for {Email}", existing.Email);
        return TypedResults.Ok(existing);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Registration upsert failed for {Email}", inputUser.Email);
        return TypedResults.BadRequest();
    }
});

app.MapGet("/register", async Task<IEnumerable<User>> (UserDB db) =>
{
    return await db.Users.ToListAsync();
}).RequireAuthorization();

ProfileApi.MapProfileEndpoints(app);

app.MapDefaultEndpoints();

app.Run();


