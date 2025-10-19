using Microsoft.EntityFrameworkCore;
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

// T026: Run EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegistrantDB>();
    await db.Database.MigrateAsync();
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
