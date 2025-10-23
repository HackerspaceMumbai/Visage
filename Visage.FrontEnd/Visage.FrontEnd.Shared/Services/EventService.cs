using System.Collections.Generic;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Visage.FrontEnd.Shared.Services
{
    public class EventService(HttpClient httpClient, IMemoryCache cache) : IEventService
    {
        private const string UpcomingEventsCacheKey = "upcoming-events";
        private const string PastEventsCacheKey = "past-events";
        
        /// <summary>
        /// Asynchronously retrieves a list of upcoming events from the backend with 5min caching.
        /// </summary>
        /// <returns>A list of upcoming <see cref="Event"/> objects, or an empty list if none are found.</returns>
        public async Task<List<Event>> GetUpcomingEvents()
        {
            // T011: Try cache first (5min TTL)
            if (cache.TryGetValue(UpcomingEventsCacheKey, out List<Event>? cachedEvents))
            {
                return cachedEvents ?? new List<Event>();
            }

            // Fetch upcoming events from the backend
            var events = await httpClient.GetFromJsonAsync<List<Event>>("/events/upcoming")
                   ?? new List<Event>();
            
            // Cache for 5 minutes
            cache.Set(UpcomingEventsCacheKey, events, TimeSpan.FromMinutes(5));
            
            return events;
        }

        /// <summary>
        /// Asynchronously retrieves a list of past events from the backend with 1hr caching.
        /// </summary>
        /// <returns>A list of past events, or an empty list if none are found.</returns>
        public async Task<List<Event>> GetPastEvents()
        {
            // T012: Try cache first (1hr TTL)
            if (cache.TryGetValue(PastEventsCacheKey, out List<Event>? cachedEvents))
            {
                return cachedEvents ?? new List<Event>();
            }

            // Fetch past events from the backend
            var events = await httpClient.GetFromJsonAsync<List<Event>>("/events/past")
                   ?? new List<Event>();
            
            // Cache for 1 hour
            cache.Set(PastEventsCacheKey, events, TimeSpan.FromHours(1));
            
            return events;
        }
        
        /// <summary>
        /// Asynchronously retrieves a single event by ID with 10min caching (T013).
        /// </summary>
        public async Task<Event?> GetEventByIdAsync(Guid id)
        {
            var cacheKey = $"event-{id}";
            
            // Try cache first (10min TTL)
            if (cache.TryGetValue(cacheKey, out Event? cachedEvent))
            {
                return cachedEvent;
            }

            // Fetch event from backend
            var evt = await httpClient.GetFromJsonAsync<Event>($"/events/{id}");
            
            if (evt != null)
            {
                // Cache for 10 minutes
                cache.Set(cacheKey, evt, TimeSpan.FromMinutes(10));
            }
            
            return evt;
        }

        /// <summary>
        /// Sends a request to schedule a new event by posting its details to the backend.
        /// </summary>
        /// <param name="newEvent">The event to be scheduled.</param>
        public async Task ScheduleEvent(Event newEvent)
        {
            var response = await httpClient.PostAsJsonAsync("/events", newEvent);
            response.EnsureSuccessStatusCode();

            // Invalidate caches so newly created events appear immediately
            cache.Remove(UpcomingEventsCacheKey);
            cache.Remove(PastEventsCacheKey);
        }



        public async Task<string> UploadCoverPicture(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024)); // 5MB max size
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await httpClient.PostAsync("/upload", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseContent);
            var url = jsonResponse.RootElement.GetProperty("url").GetString();

            return url;
        }
    }
}
