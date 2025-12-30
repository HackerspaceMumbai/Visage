using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Visage.Test.Aspire;

/// <summary>
/// Holds a single Aspire app instance for the entire test assembly.
/// Initialized in TestAssemblyHooks.
/// </summary>
public static class TestAppContext
{
    public static DistributedApplication App { get; internal set; } = default!;
    public static ResourceNotificationService ResourceNotificationService { get; internal set; } = default!;

    // Experimental: tests can point at an externally running Aspire application instead
    // of starting a suite-managed distributed app by setting the environment variable
    // TEST_USE_EXTERNAL_SERVICES=true and providing optional per-service URLs
    // via TEST_SERVICE_<UPPER_SERVICE_NAME>_URL. When external services are enabled
    // we recreate an HttpClient with the base URL.
    public static bool UseExternalServices { get; internal set; }
    private static readonly Dictionary<string, string> _externalServiceBaseUrls = new(StringComparer.OrdinalIgnoreCase);

    public static void AddExternalServiceUrl(string serviceName, string baseUrl)
    {
        _externalServiceBaseUrls[serviceName] = baseUrl;
    }

    public static HttpClient CreateHttpClient(string serviceName)
    {
        if (!UseExternalServices)
            return App.CreateHttpClient(serviceName);

        // If explicit base url configured, use it. Otherwise, assume https://{servicename}
        var baseUrl = _externalServiceBaseUrls.TryGetValue(serviceName, out var url)
            ? url
            : $"https://{serviceName}";

        var client = new HttpClient() { BaseAddress = new Uri(baseUrl) };
        // Copy any default resilience headers or timeouts here if needed
        return client;
    }

    // Shared cached Auth token for test runs (reduces repeated ROPG calls)
    private static string? _cachedAuthToken;
    private static DateTime _cachedAuthTokenExpiry = DateTime.MinValue;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    /// <summary>
    /// Checks if Auth0 test authentication is configured.
    /// </summary>
    public static bool IsAuthConfigured() => Auth0TestHelper.IsConfigured();

    public static async Task<string> GetAuthTokenAsync()
    {
        if (!Auth0TestHelper.IsConfigured())
        {
            throw new InvalidOperationException(
                "Auth0 test configuration not found. Tests requiring authentication need the following environment variables:\n" +
                "- AUTH0_DOMAIN\n" +
                "- AUTH0_CLIENT_ID\n" +
                "- AUTH0_CLIENT_SECRET\n" +
                "- AUTH0_AUDIENCE\n" +
                "- TEST_USER_EMAIL\n" +
                "- TEST_USER_PASSWORD\n" +
                "\nSee tests/Visage.Test.Aspire/README.md for setup instructions.");
        }

        if (!string.IsNullOrEmpty(_cachedAuthToken) && DateTime.UtcNow < _cachedAuthTokenExpiry)
        {
            return _cachedAuthToken;
        }

        await _tokenLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_cachedAuthToken) && DateTime.UtcNow < _cachedAuthTokenExpiry)
            {
                return _cachedAuthToken;
            }

            // Fetch new token via Auth0TestHelper and cache for 50 minutes
            var token = await Auth0TestHelper.GetTestAccessTokenAsync();
            _cachedAuthToken = token;
            _cachedAuthTokenExpiry = DateTime.UtcNow.AddMinutes(50);
            return token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public static async Task SetDefaultAuthHeader(HttpClient client)
    {
        var token = await GetAuthTokenAsync();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Waits for the named resource to be ready. In local test mode, this proxies the
    /// ResourceNotificationService from the distributed app; when running in external
    /// mode this will poll the external service health endpoint.
    /// </summary>
    public static async Task WaitForResourceAsync(string resourceName, string desiredState, TimeSpan timeout)
    {
        if (!UseExternalServices)
        {
            if (ResourceNotificationService == null)
                throw new InvalidOperationException("ResourceNotificationService is not initialized. Ensure TestAssemblyHooks started the distributed app or enabled external mode.");

            // desiredState should be a constant from KnownResourceStates (e.g. "Running", "Healthy")
            await ResourceNotificationService.WaitForResourceAsync(resourceName, desiredState).WaitAsync(timeout);
            return;
        }

        // External mode: simply poll the service health endpoint at /health until it returns success
        var deadline = DateTime.UtcNow + timeout;

        using var httpClient = CreateHttpClient(resourceName);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await httpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // ignore and retry
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException($"Timed out waiting for external resource '{resourceName}' to be ready.");
    }}