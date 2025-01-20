using TUnit;
using FluentAssertions;
using Visage.FrontEnd.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace Visage.Tests.Integration
{
    public class EventServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EventServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnAllEvents()
        {
            // Arrange
            var response = await _client.GetAsync("/events");
            response.EnsureSuccessStatusCode();

            // Act
            var events = await response.Content.ReadFromJsonAsync<List<Event>>();

            // Assert
            events.Should().NotBeNull();
            events.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task ScheduleEvent_ShouldAddEvent()
        {
            // Arrange
            var newEvent = new Event { Name = "Integration Test Event" };
            var response = await _client.PostAsJsonAsync("/events", newEvent);
            response.EnsureSuccessStatusCode();

            // Act
            var createdEvent = await response.Content.ReadFromJsonAsync<Event>();

            // Assert
            createdEvent.Should().NotBeNull();
            createdEvent.Name.Should().Be(newEvent.Name);
        }
    }
}
