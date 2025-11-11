using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Visage.Test.Aspire;

/// <summary>
/// Holds a single Aspire app instance for the entire test assembly.
/// Initialized in TestAssemblyHooks.
/// </summary>
public static class TestAppContext
{
    public static DistributedApplication App { get; internal set; } = default!;
    public static ResourceNotificationService ResourceNotificationService { get; internal set; } = default!;

    public static HttpClient CreateHttpClient(string serviceName) => App.CreateHttpClient(serviceName);
}