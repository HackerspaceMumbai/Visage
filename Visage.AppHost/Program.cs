using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;

var builder = DistributedApplication.CreateBuilder(args);

#region EventAPI

var EventAPI = builder.AddProject<Projects.Visage_Services_Eventing>("event-api")
                 .WithExternalHttpEndpoints();

#endregion

#region CloudinaryImageSigning

string cloudinaryCloudName = builder.Configuration["Cloudinary:CloudName"] ?? throw new Exception("Cloudinary CloudName required");
string cloudinaryApiKey = builder.Configuration["Cloudinary:ApiKey"] ?? throw new Exception("Cloudinary ApiKey required");
string cloudinaryApiSecret = builder.Configuration["Cloudinary:ApiSecret"] ?? throw new Exception("Cloudinary ApiSecret required");

var cloudinaryImageSigning = builder.AddProject<Projects.CloudinaryImageSigning>("cloudinary-image-signing")
                                    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
                                    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
                                    .WithEnvironment("Cloudinary__ApiSecret", cloudinaryApiSecret)
                                    .WithExternalHttpEndpoints();

#endregion

#region web

string iam_domain = builder.Configuration["Auth0:Domain"] ?? throw new Exception("Auth0 Domain required");
string iam_clientid = builder.Configuration["Auth0:ClientId"] ?? throw new Exception("Auth0 ClientId required");

var webapp = builder.AddProject<Projects.Visage_FrontEnd_Web>("frontendweb")
                                                        .WithEnvironment("Auth0__Domain", iam_domain)
                                                        .WithEnvironment("Auth0__ClientId", iam_clientid)
                                                        .WithReference(EventAPI)
                                                        .WithReference(cloudinaryImageSigning)
                                                        .WithExternalHttpEndpoints();

#endregion

builder.Build().Run();
