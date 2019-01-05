using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Checkins.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckinsController : ControllerBase
    {

        private readonly IEventbriteClient eventbriteClient;

        public CheckinsController(IEventbriteClient _eventbriteClient)
        {
            eventbriteClient = _eventbriteClient;
        }

        // GET: api/Checkins
        [HttpGet("{id}")]
        public async Task<List<Attendee>> GetAsync(string id, [FromQuery] string attStatus="")
        {

            List<Attendee> result = await eventbriteClient.GetAttendees(id);

            //if (String.IsNullOrEmpty(attStatus))
            //{

            //    return result;
            //}

            switch (attStatus)
            {
                case "checkedin":
                    return result.Where(a => a.checked_in.Equals(true)).ToList<Attendee>();
                  

                case "noshow":
                    return result.Where(a => a.checked_in.Equals(false)).ToList<Attendee>();

                default:
                    return result;
            }

        }

        // GET: api/Checkins/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Checkins
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Checkins/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
