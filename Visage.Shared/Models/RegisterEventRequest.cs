using StrictId;

namespace Visage.Shared.Models;

public sealed record RegisterEventRequest(Id<Event> EventId, string? AdditionalDetails);
