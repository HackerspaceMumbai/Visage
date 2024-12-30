using TUnit;
using FluentAssertions;
using Visage.FrontEnd.Shared.Services;
using Visage.FrontEnd.Shared.Models;
using Moq;

namespace Visage.Tests.Unit
{
    public class EventServiceTests
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _eventServiceMock = new Mock<IEventService>();
            _eventService = new EventService(_eventServiceMock.Object);
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnAllEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Name = "Event 1" },
                new Event { Id = 2, Name = "Event 2" }
            };
            _eventServiceMock.Setup(service => service.GetAllEvents()).ReturnsAsync(events);

            // Act
            var result = await _eventService.GetAllEvents();

            // Assert
            result.Should().BeEquivalentTo(events);
        }

        [Fact]
        public async Task ScheduleEvent_ShouldAddEvent()
        {
            // Arrange
            var newEvent = new Event { Id = 3, Name = "Event 3" };
            _eventServiceMock.Setup(service => service.ScheduleEvent(newEvent)).ReturnsAsync(newEvent);

            // Act
            var result = await _eventService.ScheduleEvent(newEvent);

            // Assert
            result.Should().BeEquivalentTo(newEvent);
        }
    }
}
