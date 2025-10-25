using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Visage.FrontEnd.Shared.Services;
using Visage.FrontEnd.Web.Components;
using Visage.FrontEnd.Web.Services;

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

//Add 



builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the Visage.FrontEnd.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// T015: Register IMemoryCache for event caching
builder.Services.AddMemoryCache();

// Add the delegating handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationDelegatingHandler>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

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
    client.BaseAddress = new Uri("https+http://registrations-api");
})
   .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IRegistrationService, RegistrationService>(client =>
{
    client.BaseAddress = new Uri("https+http://registrations-api");
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


app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Visage.FrontEnd.Shared._Imports).Assembly,
        typeof(Visage.FrontEnd.Web.Client._Imports).Assembly);

app.Run();
