using System.Collections.Generic;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Visage.FrontEnd.Shared.Services
{
    public interface IEventService
    {
        Task<List<Event>> GetUpcomingEvents();
        Task<List<Event>> GetPastEvents();

        Task ScheduleEvent(Event newEvent);

        Task<string> UploadCoverPicture(IBrowserFile file);
    }
}
