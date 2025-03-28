using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Visage.FrontEnd.Shared.Services;
using Visage.FrontEnd.Web;
using Visage.FrontEnd.Web.Client;
using Visage.FrontEnd.Web.Components;
using Visage.FrontEnd.Web.Services;



var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the Visage.FrontEnd.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();


// Register the IEventService and EventService in the dependency injection container
builder.Services.AddHttpClient<IEventService, EventService>( client =>
                client.BaseAddress = new Uri("https+http://event-api"));

// Register the ICloudinaryImageSigningService and CloudinaryImageSigningService in the dependency injection container
builder.Services.AddHttpClient<ICloudinaryImageSigningService, CloudinaryImageSigningService>(client =>
    client.BaseAddress = new Uri("https+http://cloudinary-image-signing"));

// Register the IRegistrationService and RegistrationService in the dependency injection container
builder.Services.AddHttpClient<IRegistrationService, RegistrationService>(client =>
    client.BaseAddress = new Uri("https+http://registrations-api"));

var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

builder.Services.AddAuthentication(oidcScheme)
                .AddKeycloakOpenIdConnect("keycloak", realm: "Visage", oidcScheme, options =>
                {
                    options.ClientId = "VisageWeb";
                    options.ResponseType = OpenIdConnectResponseType.Code;
                   // options.Scope.Add("weather:all");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    options.SaveTokens = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddCascadingAuthenticationState();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    IdentityModelEventSource.ShowPII = true;

}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

//app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
//{
//    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
//            .WithRedirectUri(returnUrl)
//            .Build();

//    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
//});

//app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
//{
//    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
//            .WithRedirectUri("/")
//            .Build();

//    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
//    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Visage.FrontEnd.Shared._Imports).Assembly,
        typeof(Visage.FrontEnd.Web.Client._Imports).Assembly);

app.MapLoginAndLogout();

app.Run();

