using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Provides extension methods for configuring Scalar API documentation in Aspire applications.
/// </summary>
public static class ScalarExtensions
{
    /// <summary>
    /// Adds Scalar API documentation support to the service defaults.
    /// This configures OpenAPI services and provides default Scalar configuration.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddScalarDefaults(this IHostApplicationBuilder builder)
    {
        // Add OpenAPI services that Scalar depends on
        builder.Services.AddOpenApi();
        
        return builder;
    }

    /// <summary>
    /// Maps Scalar API reference endpoints with default configuration for Aspire applications.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="title">The title for the API documentation. If null, uses the application name.</param>
    /// <param name="path">The path where Scalar UI will be served. Defaults to "/scalar/v1".</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapScalarDefaults(this WebApplication app, string? title = null, string? path = null)
    {
        if (app.Environment.IsDevelopment())
        {
            // Map OpenAPI endpoint
            app.MapOpenApi();
            
            // Configure and map Scalar API reference
            app.MapScalarApiReference(options =>
            {
                var apiTitle = title ?? app.Environment.ApplicationName ?? "API";
                options.WithTitle(apiTitle)
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        return app;
    }

    /// <summary>
    /// Maps Scalar API reference endpoints with custom configuration.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configure">Action to configure Scalar options.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapScalarDefaults(this WebApplication app, Action<ScalarOptions> configure)
    {
        if (app.Environment.IsDevelopment())
        {
            // Map OpenAPI endpoint
            app.MapOpenApi();
            
            // Configure and map Scalar API reference with custom options
            app.MapScalarApiReference(configure);
        }

        return app;
    }
}