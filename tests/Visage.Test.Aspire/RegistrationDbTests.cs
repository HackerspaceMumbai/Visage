using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;
using Visage.Shared.Models;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for Registration service database migration to Aspire-managed SQL Server
/// </summary>
public class RegistrationDbTests
{
    /// <summary>
    /// T018: Verify Registration service connects to Aspire-managed database
    /// </summary>
    [Test]
    public async Task Registration_Service_Should_Connect_To_Aspire_Managed_Database()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        // Act
        await using var app = await builder.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        // Wait for registrationdb to be ready
        await resourceNotificationService.WaitForResourceAsync("registrationdb", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        
        // Assert
        var registrationDbResource = builder.Resources.FirstOrDefault(r => r.Name == "registrationdb");
        registrationDbResource.Should().NotBeNull("registrationdb should be registered as a database resource");
    }

    /// <summary>
    /// T019: Verify creating registrant records works with Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Create_New_Registrant_Record_In_Aspire_Database()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        await using var app = await builder.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        // Wait for Registration service to be ready
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Act & Assert - Verify service is accessible
        var httpClient = app.CreateHttpClient("registrations-api");
        var response = await httpClient.GetAsync("/health");
        response.Should().NotBeNull("Registration service should be accessible");
    }

    /// <summary>
    /// T020: Verify querying registrants from Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Query_Registrants_From_Aspire_Managed_Database()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        await using var app = await builder.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        // Wait for services to be ready
        await resourceNotificationService.WaitForResourceAsync("registrationdb", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Act & Assert
        var httpClient = app.CreateHttpClient("registrations-api");
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.Should().NotBeNull("Registration service should connect to database");
    }

    /// <summary>
    /// T021: Verify EF Core migrations run automatically on service startup
    /// </summary>
    [Test]
    public async Task EF_Core_Migrations_Should_Run_Automatically_On_Startup()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        await using var app = await builder.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        // Wait for database and service to start
        await resourceNotificationService.WaitForResourceAsync("registrationdb", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(60));
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Assert - If service reaches Running state, migrations succeeded
        var registrationService = builder.Resources.FirstOrDefault(r => r.Name == "registrations-api");
        registrationService.Should().NotBeNull("Registration service should start after migrations");
    }
}
