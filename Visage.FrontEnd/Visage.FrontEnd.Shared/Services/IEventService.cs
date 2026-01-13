using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using StrictId;
using Visage.FrontEnd.Shared.Models;
using Visage.Shared.Models;
using SharedEvent = Visage.Shared.Models.Event;
using SharedEventRegistration = Visage.Shared.Models.EventRegistration;

namespace Visage.FrontEnd.Shared.Services
{
    public interface IEventService
    {
        Task<List<EventDto>> GetUpcomingEvents();
        Task<List<EventDto>> GetPastEvents();
        Task ScheduleEvent(EventDto newEvent);

        Task<SharedEventRegistration?> RegisterForEventAsync(Id<SharedEvent> eventId, string? additionalDetails = null);
        Task<List<SharedEventRegistration>> GetMyRegistrationsAsync();

        Task<CheckInResponse?> CheckInToSessionAsync(Id<SharedEvent> eventId, string sessionId);
        Task<CheckOutResponse?> CheckOutFromSessionAsync(Id<SessionCheckIn> checkInId);

        Task<SharedEventRegistration?> LookupRegistrationByPinAsync(string pin);
    }
}
