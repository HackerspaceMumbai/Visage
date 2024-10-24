using System.Collections.Generic;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;

namespace Visage.FrontEnd.Shared.Services
{
    public class EventService : IEventService
    {
        public async Task<List<Event>> GetUpcomingEvents()
        {
            // Fetch upcoming events from the backend
            // This is a placeholder implementation
            await Task.Delay(1000);
            return new List<Event>
            {
                new Event
                {
                    Title = "Upcoming Event 1",
                    CoverPicture = "cover1.jpg",
                    Date = "2023-09-01",
                    Venue = "Venue 1",
                    AttendeesPercentage = 0
                },
                new Event
                {
                    Title = "Upcoming Event 2",
                    CoverPicture = "cover2.jpg",
                    Date = "2023-09-15",
                    Venue = "Venue 2",
                    AttendeesPercentage = 0
                }
            };
        }

        public async Task<List<Event>> GetPastEvents()
        {
            // Fetch past events from the backend
            // This is a placeholder implementation
            await Task.Delay(1000);
            return new List<Event>
            {
                new Event
                {
                    Title = "Past Event 1",
                    CoverPicture = "cover3.jpg",
                    Date = "2023-08-01",
                    Venue = "Venue 3",
                    AttendeesPercentage = 75
                },
                new Event
                {
                    Title = "Past Event 2",
                    CoverPicture = "cover4.jpg",
                    Date = "2023-08-15",
                    Venue = "Venue 4",
                    AttendeesPercentage = 60
                }
            };
        }
    }
}
