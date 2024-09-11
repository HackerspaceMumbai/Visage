using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Visage.FrontEnd.Shared.Services;
using Visage.FrontEnd.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Visage.FrontEnd.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
