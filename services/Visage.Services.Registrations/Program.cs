using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Visage.Services.Registration;

var builder = WebApplication.CreateBuilder(args);

// T024: Add Aspire service defaults (health checks, OpenTelemetry, service discovery)
builder.AddServiceDefaults();

// T024: Configure Aspire-managed SQL Server DbContext
builder.AddSqlServerDbContext<RegistrantDB>("registrationdb");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Auth0 authorization
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("profile:read-write", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("scope", "profile:read-write"));
});

var app = builder.Build();

// T026: Run EF Core migrations on startup (development only)
// PRODUCTION: Migrations should be applied via deployment pipeline (e.g., dotnet ef database update in CI/CD)
// This auto-migration is gated to development environment to prevent production database changes at runtime
if (app.Environment.IsDevelopment() || 
    bool.TryParse(app.Configuration["MIGRATE_ON_STARTUP"], out var migrateOnStartup) && migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RegistrantDB>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations. Service will continue but may have issues if schema is outdated.");
        // Non-failing: service continues to start even if migrations fail
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map Profile API endpoints
app.MapProfileEndpoints();

app.MapDefaultEndpoints();

app.Run();
