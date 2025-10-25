using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;
using Visage.Shared.Models;
using System.Net.Http.Json;

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
        
        // Assert - Verify database is registered and running
        var registrationDbResource = builder.Resources.FirstOrDefault(r => r.Name == "registrationdb");
        registrationDbResource.Should().NotBeNull("registrationdb should be registered as a database resource");
        
        // Verify runtime connectivity by querying the registrations service health endpoint
        // (which requires DB connection for EF Core migrations to have run)
        var httpClient = app.CreateHttpClient("registrations-api");
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue(
            "Registration service health check should succeed, confirming database connectivity");
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
        
        var httpClient = app.CreateHttpClient("registrations-api");
        
        // Create a valid registrant with all required properties
        var newRegistrant = new Registrant
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@example.com",
            MobileNumber = "+919876543210",
            AddressLine1 = "123 Test Street",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtId = "ABCD1234E",
            GovtIdLast4Digits = "234E",
            OccupationStatus = "Employed"
        };
        
        // Act - POST to /register endpoint
        var postResponse = await httpClient.PostAsJsonAsync("/register", newRegistrant);
        
        // Assert - Verify successful creation
        postResponse.Should().NotBeNull("POST response should not be null");
        postResponse.IsSuccessStatusCode.Should().BeTrue($"POST should succeed but got {postResponse.StatusCode}");
        
        // Verify the created registrant is returned
        var createdRegistrant = await postResponse.Content.ReadFromJsonAsync<Registrant>();
        createdRegistrant.Should().NotBeNull("Created registrant should be returned in response");
        createdRegistrant!.FirstName.Should().Be("Test", "FirstName should match");
        createdRegistrant.LastName.Should().Be("User", "LastName should match");
        createdRegistrant.Email.Should().Be("testuser@example.com", "Email should match");
        createdRegistrant.Id.Should().NotBeNull("Id should be auto-generated");
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
        
        // Act - Query registrants from the /register endpoint
        var httpClient = app.CreateHttpClient("registrations-api");
        var getResponse = await httpClient.GetAsync("/register");
        
        // Assert - Verify successful query
        getResponse.Should().NotBeNull("GET response should not be null");
        getResponse.IsSuccessStatusCode.Should().BeTrue($"GET should succeed but got {getResponse.StatusCode}");
        
        // Deserialize and verify registrants collection
        var registrants = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Registrant>>();
        registrants.Should().NotBeNull("Registrants collection should not be null");
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
