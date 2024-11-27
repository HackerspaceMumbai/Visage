using System.Collections.Generic;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Text.Json;

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

        public async Task ScheduleEvent(Event newEvent)
        {
            // Send a POST request to the backend to create a new event
            await httpClient.PostAsJsonAsync("/events", newEvent);
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
