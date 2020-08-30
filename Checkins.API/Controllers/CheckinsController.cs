using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Checkins.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CheckinsController : ControllerBase
    {

        private readonly IEventbriteClient eventbriteClient;

        public CheckinsController(IEventbriteClient _eventbriteClient)
        {
            eventbriteClient = _eventbriteClient;
        }


        // GET: api/Checkins/5
        [HttpGet]
        public string Get()
        {
            return "value";
        }


        // GET: api/Checkins/5

        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<List<Attendee>>> GetAsyncByEventId(string id, [FromQuery] string attStatus = "")
        {
            string validStatus = "all checkedin noshow";
            if( !String.IsNullOrEmpty(attStatus) && !validStatus.Contains(attStatus))
            {
                return BadRequest();
            }

            List<Attendee> result = await eventbriteClient.GetAttendees(id);

            switch (attStatus)
            {
                case "checkedin":
                    return Ok(result.Where(a => a.checked_in.Equals(true)).ToList<Attendee>());
                  

                case "noshow":
                    return Ok(result.Where(a => a.checked_in.Equals(false)).ToList<Attendee>());

                case "all":
                    return Ok(result);

                default:
                    return BadRequest();
            }

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
