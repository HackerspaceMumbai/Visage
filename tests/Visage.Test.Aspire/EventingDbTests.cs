using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for Eventing service database migration to Aspire-managed SQL Server.
/// Uses a single Aspire app started once per test assembly (see TestAssemblyHooks).
/// </summary>
public class EventingDbTests
{
    /// <summary>
    /// T032: Verify Eventing service connects to Aspire-managed database
    /// </summary>
    [Test]
    public async Task Eventing_Service_Should_Connect_To_Aspire_Managed_Database()
    {
        // Arrange - Use shared app (already started in assembly hook)
        
        // Wait for eventing service to be ready
        await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Act - Query health endpoint
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        var healthResponse = await httpClient.GetAsync("/health");
        
        // Assert - Health check should succeed, confirming database connectivity
        healthResponse.Should().NotBeNull("Eventing service health check should succeed, confirming database connectivity");
        healthResponse.IsSuccessStatusCode.Should().BeTrue("Eventing service health check should succeed, confirming database connectivity");
    }

    /// <summary>
    /// T033: Verify creating event records works with Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Create_New_Event_Record_In_Aspire_Database()
    {
        // Arrange - Use shared app
        
        // Wait for Eventing service to be ready
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        
        // Create a valid event with all required properties
        var newEvent = new Event
        {
            Title = "Test Integration Event",
            Description = "Integration test event for Aspire SQL Server migration",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            StartTime = new TimeOnly(14, 0),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            EndTime = new TimeOnly(17, 0),
            Location = "Test Location",
            Type = "Workshop",
            Theme = "Testing",
            Hashtag = "IntegrationTest",
            CoverPicture = "https://example.com/test.jpg"
        };
        
        // Act - POST to /events endpoint
        var postResponse = await httpClient.PostAsJsonAsync("/events", newEvent);
        
        // Assert - Verify successful creation
        postResponse.Should().NotBeNull("POST response should not be null");
        postResponse.IsSuccessStatusCode.Should().BeTrue($"POST should succeed but got {postResponse.StatusCode}");
        
        // Verify the created event is returned
        var createdEvent = await postResponse.Content.ReadFromJsonAsync<Event>();
        createdEvent.Should().NotBeNull("Created event should be returned in response");
        createdEvent!.Title.Should().Be("Test Integration Event", "Title should match");
        createdEvent.Description.Should().Be("Integration test event for Aspire SQL Server migration", "Description should match");
        createdEvent.Location.Should().Be("Test Location", "Location should match");
        createdEvent.Id.Should().NotBeNull("Id should be auto-generated");
    }

    /// <summary>
    /// T034: Verify querying events from Aspire-managed database
    /// </summary>
    [Test]
    public async Task Should_Query_Events_From_Aspire_Managed_Database()
    {
        // Arrange - Use shared app
        
        // Wait for services to be ready
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Act - Query events from the /events endpoint
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        var getResponse = await httpClient.GetAsync("/events");
        
        // Assert - Verify successful query
        getResponse.Should().NotBeNull("GET response should not be null");
        getResponse.IsSuccessStatusCode.Should().BeTrue($"GET should succeed but got {getResponse.StatusCode}");
        
        // Deserialize and verify events collection
        var events = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Event>>();
        events.Should().NotBeNull("Events collection should not be null");
        
        // Should have seeded events or at least be queryable
        events.Should().NotBeNull("Should be able to query events from Aspire-managed database");
    }

    /// <summary>
    /// T035: Verify EF Core migrations run automatically on service startup
    /// </summary>
    [Test]
    public async Task EF_Core_Migrations_Should_Run_Automatically_On_Startup()
    {
        // Arrange - Use shared app (migrations already ran during assembly initialization)
        
        // Wait for service to be ready
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        // Assert - If service is running, migrations succeeded (checked during fixture initialization)
        // Verify we can query the service health endpoint
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue("Service should be healthy after migrations");
    }

    /// <summary>
    /// Bonus test: Verify querying upcoming events works correctly
    /// </summary>
    [Test]
    public async Task Should_Query_Upcoming_Events_From_Aspire_Database()
    {
        // Arrange
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        
        // Act - Query upcoming events
        var response = await httpClient.GetAsync("/events/upcoming");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Upcoming events endpoint should work");
        var upcomingEvents = await response.Content.ReadFromJsonAsync<IEnumerable<Event>>();
        upcomingEvents.Should().NotBeNull("Should return upcoming events collection");
    }

    /// <summary>
    /// Bonus test: Verify querying past events works correctly
    /// </summary>
    [Test]
    public async Task Should_Query_Past_Events_From_Aspire_Database()
    {
        // Arrange
            await TestAppContext.WaitForResourceAsync("eventing", KnownResourceStates.Running, TimeSpan.FromSeconds(90));
        
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        
        // Act - Query past events
        var response = await httpClient.GetAsync("/events/past");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Past events endpoint should work");
        var pastEvents = await response.Content.ReadFromJsonAsync<IEnumerable<Event>>();
        pastEvents.Should().NotBeNull("Should return past events collection");
    }
}
