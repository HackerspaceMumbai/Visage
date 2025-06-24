using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Scalar API documentation to Aspire applications.
/// </summary>
public static class ScalarResourceExtensions
{
    /// <summary>
    /// Configures a project resource to expose Scalar API documentation.
    /// This adds the necessary URL endpoint configuration for Scalar UI.
    /// </summary>
    /// <param name="projectResource">The project resource to configure.</param>
    /// <param name="title">The title for the API documentation. If null, uses the resource name.</param>
    /// <param name="scalarPath">The path where Scalar UI will be served. Defaults to "/scalar/v1".</param>
    /// <returns>The project resource for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithScalarApiDocumentation(
        this IResourceBuilder<ProjectResource> projectResource,
        string? title = null,
        string scalarPath = "/scalar/v1")
    {
        var apiTitle = title ?? $"{projectResource.Resource.Name} API";
        
        return projectResource
            .WithUrlForEndpoint("http", url => 
                url.DisplayLocation = UrlDisplayLocation.DetailsOnly) // Hide the plain-HTTP link from the Resources grid
            .WithUrlForEndpoint("https", url =>
            {
                url.DisplayText = $"{apiTitle} Scalar OpenAPI";
                url.Url += scalarPath;
            });
    }

    /// <summary>
    /// Configures a project resource to expose Scalar API documentation with custom endpoint configuration.
    /// </summary>
    /// <param name="projectResource">The project resource to configure.</param>
    /// <param name="configure">Action to configure the Scalar endpoint.</param>
    /// <returns>The project resource for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithScalarApiDocumentation(
        this IResourceBuilder<ProjectResource> projectResource,
        Action<ResourceUrlAnnotation> configure)
    {
        return projectResource
            .WithUrlForEndpoint("http", url => 
                url.DisplayLocation = UrlDisplayLocation.DetailsOnly) // Hide the plain-HTTP link from the Resources grid
            .WithUrlForEndpoint("https", configure);
    }
}