using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Scalar.AspNetCore;
using Visage.Services.Registration;
using Visage.Shared.Models;

Console.WriteLine("Starting Visage Registration Service...");

// Auth0 configuration values

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Auth0 configuration values
Console.WriteLine("Auth0Domain: " + builder.Configuration["Auth0:Domain"]);
Console.WriteLine("Auth0Audience: " + builder.Configuration["Auth0:Audience"]);

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


var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__VisageSQL");
Console.WriteLine("Using connection string: " + connectionString);



// Replace the in-memory database with local SQL Server
builder.Services.AddDbContext<RegistrantDB>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestHeaders.Add("Authorization");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.UseHttpsRedirection();
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

app.Run();


