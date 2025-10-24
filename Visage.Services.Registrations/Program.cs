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

// Add authentication (if using JWT Bearer for API)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
     {
         c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
         c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidAudience = builder.Configuration["Auth0:Audience"],
             ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
         };
     });

// Add authorization services
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("profile:read-write", policy => policy.RequireClaim("permissions", "profile:read-write"));
});

// T024: Configure Aspire-managed SQL Server DbContext (replaces manual connection string)
// T027: Connection string is now managed by Aspire, no hardcoded values
builder.AddSqlServerDbContext<RegistrantDB>("registrationdb");

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

app.MapPost("/register", async Task<Results<Created<Registrant>, BadRequest>> ([FromBody] Registrant inputRegistrant, RegistrantDB db) =>
{
    try
    {
        await db.Registrants.AddAsync(inputRegistrant);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/register/{inputRegistrant.Id}", inputRegistrant);
    }
    catch (Exception)
    {
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


