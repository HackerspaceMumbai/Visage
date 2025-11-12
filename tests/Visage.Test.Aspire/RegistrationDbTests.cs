using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for Registration service database migration to Aspire-managed SQL Server.
/// Uses a single Aspire app started once per test assembly (see TestAssemblyHooks).
/// </summary>
public class RegistrationDbTests
{
    /// <summary>
    /// T018: Verify Registration service connects to Aspire-managed database
    /// </summary>
    [Test]
    public async Task Registration_Service_Should_Connect_To_Aspire_Managed_Database()
    {
        // Arrange - Use shared app (already started in assembly hook)
        var resourceNotificationService = TestAppContext.ResourceNotificationService;
        
        // Wait for registrations-api to be ready
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Assert - Verify database connectivity via health endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
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
        // Arrange - Use shared app
        var resourceNotificationService = TestAppContext.ResourceNotificationService;
        
        // Wait for Registration service to be ready
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        
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
        // Arrange - Use shared app
        var resourceNotificationService = TestAppContext.ResourceNotificationService;
        
        // Wait for services to be ready
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Act - Query registrants from the /register endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var getResponse = await httpClient.GetAsync("/register");
        
        // Assert - Verify successful query
        getResponse.Should().NotBeNull("GET response should not be null");
        getResponse.IsSuccessStatusCode.Should().BeTrue($"GET should succeed but got {getResponse.StatusCode}");
        
        // Deserialize and verify registrants collection
        var registrants = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Registrant>>();
        registrants.Should().NotBeNull("Registrants collection should not be null");
    }

    /// <summary>
    /// T037: Posting the same email should update the existing registrant record instead of creating duplicates.
    /// </summary>
    [Test]
    public async Task RegisterEndpoint_WhenSameEmailPosted_ShouldUpdateExistingRecord()
    {
        // Arrange - Use shared app
        var resourceNotificationService = TestAppContext.ResourceNotificationService;

        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var email = "duplicate-update@example.com";

        var firstRegistrant = new Registrant
        {
            FirstName = "First",
            LastName = "Person",
            Email = email,
            MobileNumber = "+919999999990",
            AddressLine1 = "Line 1",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "1234",
            GovtIdType = "Aadhaar",
            OccupationStatus = "Employed",
            CompanyName = "Initial Co"
        };

        var secondRegistrant = new Registrant
        {
            FirstName = "First",
            LastName = "Person",
            Email = email,
            MobileNumber = "+919999999990",
            AddressLine1 = "Line 1",
            City = "Pune",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "1234",
            GovtIdType = "Aadhaar",
            OccupationStatus = "Employed",
            CompanyName = "Updated Co"
        };

        var firstResponse = await httpClient.PostAsJsonAsync("/register", firstRegistrant);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var secondResponse = await httpClient.PostAsJsonAsync("/register", secondRegistrant);
        secondResponse.IsSuccessStatusCode.Should().BeTrue();

        var registrantsResponse = await httpClient.GetAsync("/register");
        registrantsResponse.IsSuccessStatusCode.Should().BeTrue();

        var registrants = await registrantsResponse.Content.ReadFromJsonAsync<IEnumerable<Registrant>>();
        registrants.Should().NotBeNull();

        var matching = registrants!
            .Where(r => string.Equals(r.Email, email, StringComparison.OrdinalIgnoreCase))
            .ToList();

        matching.Count.Should().Be(1, "upserts should avoid duplicate rows for the same email");
        matching[0].City.Should().Be("Pune", "latest submission should update the city");
        matching[0].CompanyName.Should().Be("Updated Co", "latest submission should update professional info");
        matching[0].IsProfileComplete.Should().BeTrue("successful upsert should mark profile as complete");
    }

    /// <summary>
    /// T021: Verify EF Core migrations run automatically on service startup
    /// </summary>
    [Test]
    public async Task EF_Core_Migrations_Should_Run_Automatically_On_Startup()
    {
        // Arrange - Use shared app (migrations already ran during assembly initialization)
        var resourceNotificationService = TestAppContext.ResourceNotificationService;
        
        // Wait for service to be ready
        await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Assert - If service is running, migrations succeeded (checked during fixture initialization)
        // Verify we can query the service health endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue("Service should be healthy after migrations");
    }
}
