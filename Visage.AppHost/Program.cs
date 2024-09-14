using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;
using System.Globalization;


var builder = DistributedApplication.CreateBuilder(args);

#region Web

var webapp = builder.AddProject<Projects.Visage_FrontEnd_Web>("web")
                                                        .WithExternalHttpEndpoints();
                                                                    

#endregion


builder.Build().Run();
