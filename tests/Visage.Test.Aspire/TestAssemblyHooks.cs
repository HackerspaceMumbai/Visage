using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Visage.Test.Aspire;

/// <summary>
/// Assembly-level hooks to start and stop the Aspire app exactly once.
/// </summary>
public static class TestAssemblyHooks
{
    [Before(HookType.Assembly)]
    public static async Task StartAspireOnceAsync()
    {
        // Optional dev toggle: if set, tests will use externally-provisioned Aspire services
        var useExternalStr = Environment.GetEnvironmentVariable("TEST_USE_EXTERNAL_SERVICES");
        var useExternal = !string.IsNullOrEmpty(useExternalStr) && useExternalStr.Equals("true", StringComparison.OrdinalIgnoreCase);

        if (useExternal)
        {
            TestAppContext.UseExternalServices = true;

            // Read explicit per-service URLs from env vars TEST_SERVICE_<SERVICE>_URL
            void AddIfPresent(string svc)
            {
                var envName = $"TEST_SERVICE_{svc.ToUpperInvariant().Replace('-', '_')}_URL";
                var val = Environment.GetEnvironmentVariable(envName);
                if (!string.IsNullOrEmpty(val))
                {
                    TestAppContext.AddExternalServiceUrl(svc, val);
                }
            }

            AddIfPresent("registrations-api");
            AddIfPresent("eventing");
            AddIfPresent("frontendweb");
            AddIfPresent("cloudinary-image-signing");

            // Verify that external endpoints are reachable quickly (fail early)
            var checkTimeout = TimeSpan.FromSeconds(45);
            await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, checkTimeout);
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, checkTimeout);
            await TestAppContext.WaitForResourceAsync("frontendweb", KnownResourceStates.Running, checkTimeout);
            return;
        }

        // Ensure Docker or Podman is running before we attempt to start Aspire
        if (!await IsContainerRuntimeAvailableAsync())
        {
            throw new InvalidOperationException("Docker or Podman is not running. Start Docker/Podman before running tests to avoid resource startup failures.");
        }
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();

        var app = await builder.BuildAsync();
        var rns = app.Services.GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Wait for core resources
        await rns.WaitForResourceAsync("sql", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        await rns.WaitForResourceAsync("registrationdb", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        await rns.WaitForResourceAsync("eventingdb", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        await rns.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));

        TestAppContext.App = app;
        TestAppContext.ResourceNotificationService = rns;
    }

    [After(HookType.Assembly)]
    public static async Task StopAspireOnceAsync()
    {
        if (TestAppContext.App is not null)
        {
            await TestAppContext.App.DisposeAsync();
        }
    }
    private static async Task<bool> IsContainerRuntimeAvailableAsync()
    {
        // Allow overriding the container runtime check when explicitly set by developer for debugging.
        var disableCheck = Environment.GetEnvironmentVariable("TEST_DISABLE_CONTAINER_CHECK");
        if (!string.IsNullOrEmpty(disableCheck) && disableCheck.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        // Try docker first, then podman. Runs a quick `info` to validate runtime availability.
        static async Task<bool> TryExec(string command, string args)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var p = System.Diagnostics.Process.Start(psi);
                if (p == null)
                    return false;

                var completed = p.WaitForExit(5000);
                if (!completed)
                {
                    try { p.Kill(); } catch { }
                    return false;
                }

                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        if (await TryExec("docker", "info"))
            return true;

        if (await TryExec("podman", "info"))
            return true;

        return false;
    }
}
