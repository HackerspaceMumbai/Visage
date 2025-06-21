using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;
using System.Reflection;
using Visage.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

#region Auth0Configuration

var iamDomain = builder.AddParameter("auth0-domain", secret: false);
var iamClientId = builder.AddParameter("auth0-clientid", secret: false);
var iamClientSecret = builder.AddParameter("auth0-clientsecret", secret: true);
var iamAudience = builder.AddParameter("auth0-audience", secret: false);

#endregion

#region database
var VisageSQL = builder.AddConnectionString("visagesql");
#endregion

#region Clarity

var clarityProjectId = builder.AddParameter("clarity-projectid", secret: false);

// Validate the parameter value at runtime
var clarityProjectIdValue = builder.Configuration["clarity-projectid"];
if (string.IsNullOrWhiteSpace(clarityProjectIdValue))
{
    throw new Exception("Clarity Project ID required");
}

#endregion

#region EventAPI

var EventAPI = builder.AddProject<Projects.Visage_Services_Eventing>("event-api")
                 .WithExternalHttpEndpoints();

#endregion

#region RegistrationAPI

var registrationAPI = builder.AddProject<Projects.Visage_Services_Registrations>("registrations-api")
    .WithEnvironment("Auth0__Domain", iamDomain)
var cloudinaryCloudName = builder.AddParameter("cloudinary-cloudname", secret: false);
var cloudinaryApiKey    = builder.AddParameter("cloudinary-apikey",    secret: true);
var cloudinaryApiSecret = builder.AddParameter("cloudinary-apisecret", secret: true);

// Note: Parameter validation should be handled by the AddParameter method or during app startup#endregion

#region CloudinaryImageSigning

var cloudinaryCloudName = builder.AddParameter("cloudinary-cloudname", secret: false) ?? throw new Exception("Cloudinary Cloud Name required");
var cloudinaryApiKey = builder.AddParameter("cloudinary-apikey", secret: true) ?? throw new Exception("Cloudinary API Key required");
var cloudinaryApiSecret = builder.AddParameter("cloudinary-apisecret", secret: true) ?? throw new Exception("Cloudinary API Secret required");

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
    .WithReference(EventAPI)
    .WaitFor(EventAPI)
    .WithReference(registrationAPI)
    .WaitFor(registrationAPI)
    .WithReference(cloudinaryImageSigning)
    .WaitFor(cloudinaryImageSigning)
    .WithExternalHttpEndpoints();

#endregion

builder.Build().Run();
