using StrictId;

namespace Visage.Shared.Models;

public sealed record CheckInRequest(Id<Event> EventId, string SessionId);

public sealed record CheckOutRequest(Id<SessionCheckIn> CheckInId);

public sealed record CheckInResponse(Id<SessionCheckIn> CheckInId, Id<EventRegistration> EventRegistrationId, string SessionId, DateTime CheckedInAt, string Message);

public sealed record CheckOutResponse(Id<SessionCheckIn> CheckInId, DateTime CheckedOutAt, int StayDurationMinutes, string Message);
