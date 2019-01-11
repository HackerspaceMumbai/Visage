using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Checkins.API
{
    public interface IEventbriteClient
    {
        Task<List<Attendee>> GetAttendees(string eventID);
    }

    public class EventbriteClient :  IEventbriteClient
    {   
        private readonly HttpClient _client;

        public IConfiguration Configuration { get; }

        public EventbriteClient(HttpClient httpClient, IConfiguration configuration)
        {
            Configuration = configuration;
            httpClient.BaseAddress = new System.Uri(Configuration["BaseURI"]);
            //httpClient.BaseAddress = new System.Uri("http://eventbriteapi.com/");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
        }


        public async Task<List<Attendee>> GetAttendees(string eventID)
        {
            var response = await _client.GetAsync("/v3/events/" + eventID + "/attendees/?token=" + Configuration["token"]);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsAsync<EventBriteEventResponse>();

            return content.attendees;
        }

        
    }   
}

