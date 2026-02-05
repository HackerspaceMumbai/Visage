using Microsoft.Extensions.Hosting;
using Aspire.Hosting;
using Visage.AppHost;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);


#region Infra
// Add the Azure App Container environment
builder.AddAzureContainerAppEnvironment("dev");

#endregion


#region Auth0Configuration

var iamDomain = builder.AddParameter("auth0-domain");
var iamClientId = builder.AddParameter("auth0-clientid");
var iamClientSecret = builder.AddParameter("auth0-clientsecret", secret: true);
var iamAudience = builder.AddParameter("auth0-audience"); //For API access

#endregion

#region OAuthConfiguration

// T087: OAuth parameters for direct social profile verification
// These are stored in user secrets for development and environment variables for production
var oauthLinkedInClientId = builder.AddParameter("oauth-linkedin-clientid", secret: true);
var oauthLinkedInClientSecret = builder.AddParameter("oauth-linkedin-clientsecret", secret: true);
var oauthGitHubClientId = builder.AddParameter("oauth-github-clientid", secret: true);
var oauthGitHubClientSecret = builder.AddParameter("oauth-github-clientsecret", secret: true);
// Optional: explicitly set OAuth base URL used to build the provider redirect_uri
// Example for local dev: OAuth__BaseUrl = "https://localhost:7400"
var oauthBaseUrl = builder.AddParameter("oauth-baseurl");

#endregion

#region database

// T012: Register SQL Server as a first-class Aspire resource and pin image tag to avoid 2025-latest pulls
var sqlServer = builder.AddSqlServer("sql")
                                                 .WithLifetime(ContainerLifetime.Persistent);


//// T013: Register registrationdb as a database on the SQL Server instance
//var registrationDb = sqlServer.AddDatabase("registrationdb");

// Dedicated database for the User Profile service (separates user/profile schema from registrations)
var userProfileDb = sqlServer.AddDatabase("userprofiledb");

// T014: Register eventingdb as a database on the SQL Server instance
var eventingDb = sqlServer.AddDatabase("eventingdb");

// Legacy connection string - to be removed after full migration
var visageSQL = builder.AddConnectionString("visagesql");
#endregion

#region Clarity

var clarityProjectId = builder.AddParameter("clarity-projectid", secret: false);

#endregion

#region EventAPI

// Register the Eventing service under the canonical name "eventing" so
// Aspire service discovery exposes the hostname `https://eventing` that
// the frontend and documentation expect.
var eventAPI = builder.AddProject<Projects.Visage_Services_Eventing>("eventing")
    .WithReference(eventingDb)
    .WaitFor(eventingDb)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Event API Scalar OpenAPI";
        url.Url += "/scalar/v1";
    });

#endregion


#region UserProfileAPI

// T022-T023: Wire UserProfile service to Aspire-managed userprofiledb
var userProfileApi = builder.AddProject<Projects.Visage_Services_UserProfile>("userprofile-api")
    .WithEnvironment("Auth0__Domain", iamDomain)
    .WithEnvironment("Auth0__Audience", iamAudience)
    .WithReference(userProfileDb)
    .WaitFor(userProfileDb);  // Ensure database is ready before service starts

#endregion


#region ScalarApiReference

if (builder.Environment.IsDevelopment())
{
    // Add Scalar API Reference for all services (development only)
    var scalar = builder.AddScalarApiReference(options =>
    {
        options
            .PreferHttpsEndpoint()
            .AllowSelfSignedCertificates()
            .WithTheme(ScalarTheme.BluePlanet);
    })
    .WithLifetime(ContainerLifetime.Persistent);

    // Register your APIs with Scalar
    scalar
        .WithApiReference(eventAPI)
        .WithApiReference(userProfileApi);
}

#endregion

#region CloudinaryImageSigning

var cloudinaryCloudName = builder.AddParameter("cloudinary-cloudname", secret: false);
var cloudinaryApiKey = builder.AddParameter("cloudinary-apikey", secret: true);
var cloudinaryApiSecret = builder.AddParameter("cloudinary-apisecret", secret: true);

var cloudinaryImageSigning = builder.AddNodeApp("cloudinary-image-signing", "../services/CloudinaryImageSigning", "app.js")
    .WithNpm()
    .WithRunScript("watch")
    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
    .WithEnvironment("Cloudinary__ApiSecret", cloudinaryApiSecret)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

var launchProfile = builder.Configuration["DOTNET_LAUNCH_PROFILE"] ??
                    builder.Configuration["AppHost:DefaultLaunchProfileName"];

if (builder.Environment.IsDevelopment() && launchProfile == "https")
{
    cloudinaryImageSigning.RunWithHttpsDevCertificate("HTTPS_CERT_FILE", "HTTPS_CERT_KEY_FILE");
}

#endregion

#region web

var webapp = builder.AddProject<Projects.Visage_FrontEnd_Web>("frontendweb")
    .WithEnvironment("Auth0__Domain", iamDomain)
    .WithEnvironment("Auth0__ClientId", iamClientId)
    .WithEnvironment("Auth0__ClientSecret", iamClientSecret)
    .WithEnvironment("Auth0__Audience", iamAudience)
    .WithEnvironment("OAuth__LinkedIn__ClientId", oauthLinkedInClientId)
    .WithEnvironment("OAuth__LinkedIn__ClientSecret", oauthLinkedInClientSecret)
    .WithEnvironment("OAuth__GitHub__ClientId", oauthGitHubClientId)
    .WithEnvironment("OAuth__GitHub__ClientSecret", oauthGitHubClientSecret)
// Optional override for the OAuth redirect host/port used to build redirect_uri
    .WithEnvironment("OAuth__BaseUrl", oauthBaseUrl)
    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
    .WithEnvironment("Clarity__ProjectId", clarityProjectId)
    .WithReference(eventAPI)
    .WaitFor(eventAPI)
    .WithReference(userProfileApi)
    .WaitFor(userProfileApi)
    .WithReference(cloudinaryImageSigning)
    .WaitFor(cloudinaryImageSigning)
    .WithExternalHttpEndpoints();
#endregion

builder.Build().Run();

// ENC0118 is a hot reload warning indicating that changes to top-level statements (like those in Program.cs or AppHost.cs) will not take effect until the application is restarted.
// This is not a code error, but a limitation of .NET Hot Reload for top-level statements.
// To resolve this warning, you must stop and restart the application after making changes to this file.
// No code changes are required to fix ENC0118 itself, but you should always restart the app after editing this file to ensure changes are applied.
