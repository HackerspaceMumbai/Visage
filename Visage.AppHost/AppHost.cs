
using Microsoft.Extensions.Hosting;
using Visage.AppHost;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

#region Auth0Configuration

var iamDomain = builder.AddParameter("auth0-domain");
var iamClientId = builder.AddParameter("auth0-clientid");
var iamClientSecret = builder.AddParameter("auth0-clientsecret", secret: true);
var iamAudience = builder.AddParameter("auth0-audience"); //For API access

#endregion

#region database

// T012: Register SQL Server as a first-class Aspire resource and pin image tag to avoid 2025-latest pulls
var sqlServer = builder.AddSqlServer("sql")
                                                 .WithLifetime(ContainerLifetime.Persistent);


// T013: Register registrationdb as a database on the SQL Server instance
var registrationDb = sqlServer.AddDatabase("registrationdb");

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


#region RegistrationAPI

// T022-T023: Wire Registration service to Aspire-managed registrationdb
var registrationAPI = builder.AddProject<Projects.Visage_Services_Registrations>("registrations-api")
    .WithEnvironment("Auth0__Domain", iamDomain)
    .WithEnvironment("Auth0__Audience", iamAudience)
    .WithReference(registrationDb)  // Aspire-managed database connection
    .WaitFor(registrationDb);  // Ensure database is ready before service starts

#endregion


#region ScalarApiReference

// Add Scalar API Reference for all services
var scalar = builder.AddScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.BluePlanet);
    //options.WithSidebar(false);
    // You can add more options here (title, sidebar, etc.)
})
.WithLifetime(ContainerLifetime.Persistent);

// Register your APIs with Scalar
scalar
    .WithApiReference(registrationAPI, options =>
    {
        options.WithOpenApiRoutePattern("scalar/v1");
        });
    // .WithApiReference(registrationAPI, options =>
    // {
    //     options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    // });

#endregion

#region CloudinaryImageSigning

var cloudinaryCloudName = builder.AddParameter("cloudinary-cloudname", secret: false);
var cloudinaryApiKey = builder.AddParameter("cloudinary-apikey", secret: true);
var cloudinaryApiSecret = builder.AddParameter("cloudinary-apisecret", secret: true);

var cloudinaryImageSigning = builder.AddNpmApp("cloudinary-image-signing", "../services/CloudinaryImageSigning", "watch")
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
    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
    .WithEnvironment("Clarity__ProjectId", clarityProjectId)
    .WithReference(eventAPI)
    .WaitFor(eventAPI)
    .WithReference(registrationAPI)
    .WaitFor(registrationAPI)
    .WithReference(cloudinaryImageSigning)
    .WaitFor(cloudinaryImageSigning)
    .WithExternalHttpEndpoints();

#endregion


builder.Build().Run();
