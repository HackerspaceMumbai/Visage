using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace Visage.Test.Aspire;

/// <summary>
/// Assembly-level hooks to start and stop the Aspire app exactly once.
/// </summary>
public static class TestAssemblyHooks
{
    [Before(HookType.Assembly)]
    public static async Task StartAspireOnceAsync()
    {
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
}
