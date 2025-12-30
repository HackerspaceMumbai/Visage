using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Visage.Services.Registration;
using Visage.Shared.Models;

Console.WriteLine("Starting Visage Registration Service...");

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
                foreach (var claim in ctx.Principal?.Claims ?? Array.Empty<System.Security.Claims.Claim>())
                {
                    logger?.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
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
builder.AddSqlServerDbContext<RegistrantDB>("registrationdb");

// T009: Register ProfileCompletionRepository
builder.Services.AddScoped<Visage.Services.Registration.Repositories.ProfileCompletionRepository>();

// T028: Register UserPreferencesRepository
builder.Services.AddScoped<Visage.Services.Registration.Repositories.UserPreferencesRepository>();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestHeaders.Add("Authorization");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// T026: Run EF Core migrations automatically on service startup
using (var scope = app.Services.CreateScope())
{
    var registrantDb = scope.ServiceProvider.GetRequiredService<RegistrantDB>();
    Console.WriteLine("Running EF Core migrations...");
    try
    {
        await registrantDb.Database.MigrateAsync();
        Console.WriteLine("Migrations completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
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

app.MapPost("/register", async Task<Results<Created<Registrant>, Ok<Registrant>, BadRequest>> (
    [FromBody] Registrant inputRegistrant,
    RegistrantDB db,
    ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(inputRegistrant.Email))
    {
        logger.LogWarning("Registration upsert rejected: missing email address");
        return TypedResults.BadRequest();
    }

    try
    {
        inputRegistrant.Email = inputRegistrant.Email.Trim();

        Registrant? existing = null;

        if (inputRegistrant.Id != default)
        {
            existing = await db.Registrants.FirstOrDefaultAsync(r => r.Id == inputRegistrant.Id);
        }

        existing ??= await db.Registrants
            .OrderByDescending(r => r.ProfileCompletedAt)
            .FirstOrDefaultAsync(r => r.Email == inputRegistrant.Email);

        if (existing is null)
        {
            await db.Registrants.AddAsync(inputRegistrant);
            await db.SaveChangesAsync();
            logger.LogInformation("Created registrant for {Email}", inputRegistrant.Email);
            return TypedResults.Created($"/register/{inputRegistrant.Id}", inputRegistrant);
        }

        existing.FirstName = inputRegistrant.FirstName;
        existing.MiddleName = inputRegistrant.MiddleName;
        existing.LastName = inputRegistrant.LastName;
        existing.Email = inputRegistrant.Email;
        existing.MobileNumber = inputRegistrant.MobileNumber;
        existing.AddressLine1 = inputRegistrant.AddressLine1;
        existing.AddressLine2 = inputRegistrant.AddressLine2;
        existing.City = inputRegistrant.City;
        existing.State = inputRegistrant.State;
        existing.PostalCode = inputRegistrant.PostalCode;
        existing.GovtIdLast4Digits = inputRegistrant.GovtIdLast4Digits;
        existing.GovtIdType = inputRegistrant.GovtIdType;
        existing.OccupationStatus = inputRegistrant.OccupationStatus;
        existing.CompanyName = inputRegistrant.CompanyName;
        existing.EducationalInstituteName = inputRegistrant.EducationalInstituteName;

        // Social profiles (optional depending on occupation)
        existing.LinkedInProfile = inputRegistrant.LinkedInProfile;
        existing.LinkedInVanityName = inputRegistrant.LinkedInVanityName;
        existing.LinkedInSubject = inputRegistrant.LinkedInSubject;
        existing.LinkedInRawProfileJson = inputRegistrant.LinkedInRawProfileJson;
        existing.LinkedInRawEmailJson = inputRegistrant.LinkedInRawEmailJson;
        existing.LinkedInPayloadFetchedAt = inputRegistrant.LinkedInPayloadFetchedAt;

        existing.GitHubProfile = inputRegistrant.GitHubProfile;
        existing.IsLinkedInVerified = inputRegistrant.IsLinkedInVerified;
        existing.IsGitHubVerified = inputRegistrant.IsGitHubVerified;
        existing.LinkedInVerifiedAt = inputRegistrant.IsLinkedInVerified ? (inputRegistrant.LinkedInVerifiedAt ?? existing.LinkedInVerifiedAt ?? DateTime.UtcNow) : null;
        existing.GitHubVerifiedAt = inputRegistrant.IsGitHubVerified ? (inputRegistrant.GitHubVerifiedAt ?? existing.GitHubVerifiedAt ?? DateTime.UtcNow) : null;

        existing.IsProfileComplete = true;
        existing.ProfileCompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated registrant for {Email}", existing.Email);
        return TypedResults.Ok(existing);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Registration upsert failed for {Email}", inputRegistrant.Email);
        return TypedResults.BadRequest();
    }
});

app.MapGet("/register", async Task<IEnumerable<Registrant>> (RegistrantDB db) =>
{
    return await db.Registrants.ToListAsync();
});

ProfileApi.MapProfileEndpoints(app);

app.MapDefaultEndpoints();

app.Run();


