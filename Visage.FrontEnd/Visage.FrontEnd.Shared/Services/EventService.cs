using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System;
using StrictId;
using Microsoft.Extensions.Logging;
using SharedEvent = Visage.Shared.Models.Event;
using SharedEventRegistration = Visage.Shared.Models.EventRegistration;
using Visage.FrontEnd.Shared.Models;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services
{
    public class EventService(HttpClient httpClient, IMemoryCache cache, ILogger<EventService> logger) : IEventService
    {
        private const string UpcomingEventsCacheKey = "upcoming-events";
        private const string PastEventsCacheKey = "past-events";
        
        /// <summary>
        /// Asynchronously retrieves a list of upcoming events from the backend with 5min caching.
        /// </summary>
        /// <returns>A list of upcoming <see cref="EventDto"/> objects, or an empty list if none are found.</returns>
        public async Task<List<EventDto>> GetUpcomingEvents()
        {
            // T011: Try cache first (5min TTL)
            if (cache.TryGetValue(UpcomingEventsCacheKey, out List<SharedEvent>? cachedEvents))
            {
                return cachedEvents?.Select(ToDto).ToList() ?? new List<EventDto>();
            }

            // Fetch upcoming events from the backend
            var events = await httpClient.GetFromJsonAsync<List<SharedEvent>>("/events/upcoming")
                   ?? new List<SharedEvent>();
            
            // Cache for 5 minutes
            cache.Set(UpcomingEventsCacheKey, events, TimeSpan.FromMinutes(5));
            
            return events.Select(ToDto).ToList();
        }

        /// <summary>
        /// Asynchronously retrieves a list of past events from the backend with 1hr caching.
        /// </summary>
        /// <returns>A list of past events, or an empty list if none are found.</returns>
        public async Task<List<EventDto>> GetPastEvents()
        {
            // T012: Try cache first (1hr TTL)
            if (cache.TryGetValue(PastEventsCacheKey, out List<SharedEvent>? cachedEvents))
            {
                return cachedEvents?.Select(ToDto).ToList() ?? new List<EventDto>();
            }

            // Fetch past events from the backend
            var events = await httpClient.GetFromJsonAsync<List<SharedEvent>>("/events/past")
                   ?? new List<SharedEvent>();
            
            // Cache for 1 hour
            cache.Set(PastEventsCacheKey, events, TimeSpan.FromHours(1));
            
            return events.Select(ToDto).ToList();
        }
        
        /// <summary>
        /// Asynchronously retrieves a single event by ID with 10min caching (T013).
        /// </summary>
        public async Task<EventDto?> GetEventByIdAsync(Id<SharedEvent> id)
        {
            var cacheKey = $"event-{id}";
            
            // Try cache first (10min TTL)
            if (cache.TryGetValue(cacheKey, out SharedEvent? cachedEvent))
            {
                return cachedEvent is null ? null : ToDto(cachedEvent);
            }

            // Fetch event from backend
            var evt = await httpClient.GetFromJsonAsync<SharedEvent>($"/events/{id}");
            
            if (evt != null)
            {
                // Cache for 10 minutes
                cache.Set(cacheKey, evt, TimeSpan.FromMinutes(10));
            }
            
            return evt is null ? null : ToDto(evt);
        }

        /// <summary>
        /// Sends a request to schedule a new event by posting its details to the backend.
        /// </summary>
        /// <param name="newEvent">The event to be scheduled.</param>
        public async Task ScheduleEvent(EventDto newEvent)
        {
            var shared = FromDto(newEvent);
            var response = await httpClient.PostAsJsonAsync("/events", shared);
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

        // Mapping helpers
        private static EventDto ToDto(SharedEvent e) => new EventDto
        {
            Id = e.Id,
            Title = e.Title,
            Type = e.Type,
            Description = e.Description,
            StartDate = e.StartDate,
            StartTime = e.StartTime,
            EndDate = e.EndDate,
            EndTime = e.EndTime,
            Location = e.Location,
            CoverPicture = e.CoverPicture,
            AttendeesPercentage = e.AttendeesPercentage,
            Hashtag = e.Hashtag,
            Theme = e.Theme
        };

        private static SharedEvent FromDto(EventDto d) => new SharedEvent
        {
            Id = d.Id,
            Title = d.Title,
            Type = d.Type,
            Description = d.Description,
            StartDate = d.StartDate,
            StartTime = d.StartTime,
            EndDate = d.EndDate,
            EndTime = d.EndTime,
            Location = d.Location,
            CoverPicture = d.CoverPicture,
            AttendeesPercentage = d.AttendeesPercentage,
            Hashtag = d.Hashtag,
            Theme = d.Theme
        };

        // Eventing-specific operations (registrations and check-in)
        public async Task<SharedEventRegistration?> RegisterForEventAsync(Id<SharedEvent> eventId, string? additionalDetails = null)
        {
            try
            {
                var req = new RegisterEventRequest(eventId, additionalDetails);
                logger?.LogInformation("Calling eventing /registrations to register for {EventId}", eventId);

                using var response = await httpClient.PostAsJsonAsync("/registrations", req);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<SharedEventRegistration>();
                }

                logger?.LogWarning("EventService.RegisterForEventAsync failed: {Status} {Message}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error registering for event {EventId}", eventId);
                return null;
            }
        }

        public async Task<List<SharedEventRegistration>> GetMyRegistrationsAsync()
        {
            try
            {
                using var response = await httpClient.GetAsync("/registrations/my");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<SharedEventRegistration>>() ?? new List<SharedEventRegistration>();
                }

                logger?.LogWarning("GetMyRegistrationsAsync failed: {Status}", response.StatusCode);
                return new List<SharedEventRegistration>();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error fetching my registrations");
                return new List<SharedEventRegistration>();
            }
        }

        public async Task<CheckInResponse?> CheckInToSessionAsync(Id<SharedEvent> eventId, string sessionId)
        {
            try
            {
                var req = new CheckInRequest(eventId, sessionId);
                using var response = await httpClient.PostAsJsonAsync("/checkin", req);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CheckInResponse>();
                }

                logger?.LogWarning("CheckInToSessionAsync failed: {Status} {Message}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error checking in to session {SessionId} for event {EventId}", sessionId, eventId);
                return null;
            }
        }

        public async Task<CheckOutResponse?> CheckOutFromSessionAsync(Id<SessionCheckIn> checkInId)
        {
            try
            {
                var req = new CheckOutRequest(checkInId);
                using var response = await httpClient.PostAsJsonAsync("/checkin/checkout", req);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CheckOutResponse>();
                }

                logger?.LogWarning("CheckOutFromSessionAsync failed: {Status} {Message}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error checking out CheckInId {CheckInId}", checkInId);
                return null;
            }
        }

        public async Task<SharedEventRegistration?> LookupRegistrationByPinAsync(string pin)
        {
            try
            {
                using var response = await httpClient.GetAsync($"/checkin/pin/{pin}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<SharedEventRegistration>();
                }

                logger?.LogWarning("LookupRegistrationByPinAsync failed: {Status} for pin {Pin}", response.StatusCode, pin);
                return null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error looking up pin {Pin}", pin);
                return null;
            }
        }
    }
}
