using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;


var builder = DistributedApplication.CreateBuilder(args);

#region web

string iam_domain = builder.Configuration["Auth0:Domain"] ?? throw new Exception("Auth0 Domain required");
string iam_clientid = builder.Configuration["Auth0:ClientId"] ?? throw new Exception("Auth0 ClientId required");


var webapp = builder.AddProject<Projects.Visage_FrontEnd_Web>("frontendweb")
                                                        .WithEnvironment("Auth0__Domain", iam_domain)
                                                        .WithEnvironment("Auth0__ClientId", iam_clientid)
                                                        .WithExternalHttpEndpoints();
                                                                    

#endregion


builder.Build().Run();
