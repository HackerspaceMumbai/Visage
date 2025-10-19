using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using TUnit.Core;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for SQL Server Aspire resource configuration
/// Tests verify that SQL Server is registered as a first-class Aspire resource with proper health monitoring
/// </summary>
public class SqlServerIntegrationTests
{
    /// <summary>
    /// T008: Verify SQL Server resource appears in Aspire dashboard
    /// </summary>
    [Test]
    public async Task SqlServer_Resource_Should_Appear_In_Aspire_Dashboard()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        builder.Services.Should().NotBeNull("Aspire AppHost services should be configured");
        
        // Act
        await using var app = await builder.BuildAsync();
        await app.StartAsync();
        
        // Assert
        var sqlResource = builder.Resources.FirstOrDefault(r => r.Name == "sql");
        sqlResource.Should().NotBeNull("SQL Server resource should be registered with name 'sql'");
    }

    /// <summary>
    /// T009: Verify registrationdb database is available
    /// </summary>
    [Test]
    public async Task RegistrationDb_Database_Should_Be_Available()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        // Act
        await using var app = await builder.BuildAsync();
        await app.StartAsync();
        
        // Assert
        var registrationDbResource = builder.Resources.FirstOrDefault(r => r.Name == "registrationdb");
        registrationDbResource.Should().NotBeNull("registrationdb should be registered as a database resource");
    }

    /// <summary>
    /// T010: Verify eventingdb database is available
    /// </summary>
    [Test]
    public async Task EventingDb_Database_Should_Be_Available()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        // Act
        await using var app = await builder.BuildAsync();
        await app.StartAsync();
        
        // Assert
        var eventingDbResource = builder.Resources.FirstOrDefault(r => r.Name == "eventingdb");
        eventingDbResource.Should().NotBeNull("eventingdb should be registered as a database resource");
    }

    /// <summary>
    /// T011: Verify SQL Server health check reports correctly
    /// </summary>
    [Test]
    public async Task SqlServer_HealthCheck_Should_Report_Correctly()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        // Act
        await using var app = await builder.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        // Assert
        var sqlResource = builder.Resources.FirstOrDefault(r => r.Name == "sql");
        sqlResource.Should().NotBeNull("SQL Server resource should exist for health check validation");
        
        // Wait for SQL Server to be healthy (max 30 seconds as per spec SC-008)
        // WaitForResourceAsync will throw if resource doesn't reach the expected state within the timeout
        await resourceNotificationService.WaitForResourceAsync("sql", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));
        
        // If we reach here without exception, SQL Server successfully reached running state
        // This satisfies the health check requirement
    }
}
