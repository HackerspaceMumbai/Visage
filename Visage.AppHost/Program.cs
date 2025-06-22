using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;
using System.Reflection;
using Visage.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

#region Auth0Configuration

var iamDomain = builder.AddParameter("auth0-domain");
var iamClientId = builder.AddParameter("auth0-clientid");
var iamClientSecret = builder.AddParameter("auth0-clientsecret", secret: true);
var iamAudience = builder.AddParameter("auth0-audience"); //For API access

#endregion

#region database
var VisageSQL = builder.AddConnectionString("visagesql");
#endregion

#region Clarity

var clarityProjectId = builder.AddParameter("clarity-projectid", secret: false);

#endregion

#region EventAPI

var EventAPI = builder.AddProject<Projects.Visage_Services_Eventing>("event-api")
                                            .WithUrlForEndpoint("http",
                               url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly) // Hide the plain-HTTP link from the Resources grid
                                            .WithUrlForEndpoint("https", url =>
                                            {
                                                url.DisplayText = "Event API Scalar OpenAPI";
                                                url.Url += "/scalar/v1";
                                            }); 
               
                 

#endregion

#region RegistrationAPI

var registrationAPI = builder.AddProject<Projects.Visage_Services_Registrations>("registrations-api")
    .WithEnvironment("Auth0__Domain", iamDomain)
    .WithEnvironment("Auth0__Audience", iamAudience)
    .WithReference(VisageSQL);

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
    .WithReference(EventAPI)
    .WaitFor(EventAPI)
    .WithReference(registrationAPI)
    .WaitFor(registrationAPI)
    .WithReference(cloudinaryImageSigning)
    .WaitFor(cloudinaryImageSigning)
    .WithExternalHttpEndpoints();

#endregion

builder.Build().Run();
