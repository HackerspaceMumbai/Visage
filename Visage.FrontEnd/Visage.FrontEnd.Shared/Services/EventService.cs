using System.Collections.Generic;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;
using System.Net.Http.Json;

namespace Visage.FrontEnd.Shared.Services
{
    public class EventService(HttpClient httpClient) : IEventService
    {
        public async Task<List<Event>> GetUpcomingEvents()
        {
            // Fetch upcoming events from the backend
            // This is a placeholder implementation
            return await httpClient.GetFromJsonAsync<List<Event>>("/events") ?? [];

        }

        public async Task<List<Event>> GetPastEvents()
        {
            // Fetch past events from the backend
            // This is a placeholder implementation
            return await httpClient.GetFromJsonAsync<List<Event>>("/events") ?? [];

        }

        public async Task CreateEvent(Event newEvent)
        {
            // Send a POST request to the backend to create a new event
            await httpClient.PostAsJsonAsync("/events", newEvent);
        }
    }
}
