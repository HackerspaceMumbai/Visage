using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;
using System.Reflection;
using Visage.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

#region EventAPI

var EventAPI = builder.AddProject<Projects.Visage_Services_Eventing>("event-api")
                 .WithExternalHttpEndpoints();

#endregion

#region CloudinaryImageSigning

string cloudinaryCloudName = builder.Configuration["Cloudinary:CloudName"] ?? throw new Exception("Cloudinary CloudName required");
string cloudinaryApiKey = builder.Configuration["Cloudinary:ApiKey"] ?? throw new Exception("Cloudinary ApiKey required");
string cloudinaryApiSecret = builder.Configuration["Cloudinary:ApiSecret"] ?? throw new Exception("Cloudinary ApiSecret required");

var cloudinaryImageSigning = builder.AddNpmApp("cloudinary-image-signing", "../services/CloudinaryImageSigning", "watch")
                                    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
                                    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
                                    .WithEnvironment("Cloudinary__ApiSecret", cloudinaryApiSecret)
                                    .WithHttpEndpoint(env: "PORT")
                                    .PublishAsDockerFile();


var launchProfile = builder.Configuration["DOTNET_LAUNCH_PROFILE"] ??
                    builder.Configuration["AppHost:DefaultLaunchProfileName"]; // work around https://github.com/dotnet/aspire/issues/5093

if (builder.Environment.IsDevelopment() && launchProfile == "https")
{
    cloudinaryImageSigning.RunWithHttpsDevCertificate("HTTPS_CERT_FILE", "HTTPS_CERT_KEY_FILE");
}


#endregion

#region web

string iam_domain = builder.Configuration["Auth0:Domain"] ?? throw new Exception("Auth0 Domain required");
string iam_clientid = builder.Configuration["Auth0:ClientId"] ?? throw new Exception("Auth0 ClientId required");

var webapp = builder.AddProject<Projects.Visage_FrontEnd_Web>("frontendweb")
                                                        .WithEnvironment("Auth0__Domain", iam_domain)
                                                        .WithEnvironment("Auth0__ClientId", iam_clientid)
                                                        .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
                                                        .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
                                                        .WithReference(EventAPI)
                                                        .WaitFor(EventAPI)
                                                        .WithReference(cloudinaryImageSigning)
                                                        .WaitFor(cloudinaryImageSigning)
                                                        .WithExternalHttpEndpoints();

#endregion

builder.Build().Run();
