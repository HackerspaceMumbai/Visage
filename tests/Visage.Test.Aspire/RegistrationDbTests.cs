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
/// Integration tests for UserProfile service database migration to Aspire-managed SQL Server.
/// Uses a single Aspire app started once per test assembly (see TestAssemblyHooks).
/// </summary>
public class RegistrationDbTests
{
    /// <summary>
    /// T018: Verify UserProfile service connects to Aspire-managed database
    /// </summary>
    [Test]
    public async Task Registration_Service_Should_Connect_To_Aspire_Managed_Database()
    {
        // Arrange - Use shared app (already started in assembly hook)
        // Using shared TestAppContext for startup synchronization
        
        // Wait for registrations-api to be ready
            await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Assert - Verify database connectivity via health endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue(
            "Registration service health check should succeed, confirming database connectivity");
    }

    /// <summary>
    /// T019: Verify creating user records works with Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Create_New_User_Record_In_Aspire_Database()
    {
        // Arrange - Use shared app
        // Using shared TestAppContext for startup synchronization
        
        // Wait for Registration service to be ready
            await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        
        // Create a valid user with all required properties
        var newUser = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"testuser-{Guid.NewGuid():N}@example.com",
            MobileNumber = "+919876543210",
            AddressLine1 = "123 Test Street",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "1234",
            GovtIdType = "Aadhaar",
            OccupationStatus = "Employed"
        };
        
        // Act - POST to /api/users endpoint
        var postResponse = await httpClient.PostAsJsonAsync("/api/users", newUser);
        
        // Assert - Verify successful creation
        postResponse.Should().NotBeNull("POST response should not be null");
        postResponse.IsSuccessStatusCode.Should().BeTrue($"POST should succeed but got {postResponse.StatusCode}");
        
        // Verify the created user is returned
        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull("Created user should be returned in response");
        createdUser!.FirstName.Should().Be("Test", "FirstName should match");
        createdUser.LastName.Should().Be("User", "LastName should match");
        createdUser.Email.Should().Be(newUser.Email, "Email should match");
    }

    /// <summary>
    /// T020: Verify querying users from Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Query_Users_From_Aspire_Managed_Database()
    {
        // Arrange - Use shared app
        // Using shared TestAppContext for startup synchronization
        
        // Wait for services to be ready
            await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Act - Query users from the /api/users endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var getResponse = await httpClient.GetAsync("/api/users");
        
        // Assert - Verify successful query
        getResponse.Should().NotBeNull("GET response should not be null");
        getResponse.IsSuccessStatusCode.Should().BeTrue($"GET should succeed but got {getResponse.StatusCode}");
        
        // Deserialize and verify users collection
        var users = await getResponse.Content.ReadFromJsonAsync<IEnumerable<User>>();
        users.Should().NotBeNull("Users collection should not be null");
    }

    /// <summary>
    /// T037: Posting the same email should update the existing user record instead of creating duplicates.
    /// </summary>
    [Test]
    public async Task RegisterEndpoint_WhenSameEmailPosted_ShouldUpdateExistingRecord()
    {
        // Arrange - Use shared app
        // Using shared TestAppContext for startup synchronization

        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var email = $"duplicate-update-{Guid.NewGuid():N}@example.com";

        var firstUser = new User
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

        var secondUser = new User
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

        var firstResponse = await httpClient.PostAsJsonAsync("/api/users", firstUser);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var secondResponse = await httpClient.PostAsJsonAsync("/api/users", secondUser);
        secondResponse.IsSuccessStatusCode.Should().BeTrue();

        var usersResponse = await httpClient.GetAsync("/api/users");
        usersResponse.IsSuccessStatusCode.Should().BeTrue();

        var users = await usersResponse.Content.ReadFromJsonAsync<IEnumerable<User>>();
        users.Should().NotBeNull();

        var matching = users!
            .Where(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))
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
        
        // Wait for service to be ready
            await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Assert - If service is running, migrations succeeded (checked during fixture initialization)
        // Verify we can query the service health endpoint
        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue("Service should be healthy after migrations");
    }
}
