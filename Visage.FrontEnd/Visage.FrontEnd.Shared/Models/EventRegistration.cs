using StrictId;
using System.ComponentModel.DataAnnotations;
using SharedEvent = Visage.Shared.Models.Event;

namespace Visage.FrontEnd.Shared.Models;

/// <summary>
/// EventRegistration entity representing a user's registration for a specific event.
/// Frontend model matching the backend EventRegistration entity.
/// </summary>
public class EventRegistration
{
    [Required]
    public Id<EventRegistration> Id { get; init; }

    // Foreign keys
    [Required]
    public Id<User> UserId { get; set; }

    [Required]
    public Id<SharedEvent> EventId { get; set; }

    // Registration status tracking
    [Required]
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CheckedInAt { get; set; }

    // Event-specific fields
    /// <summary>
    /// Whether this is the user's first time attending this type of event.
    /// </summary>
    public string FirstTimeAttendee { get; set; } = string.Empty;

    /// <summary>
    /// User's areas of interest for this specific event.
    /// </summary>
    public string AreasOfInterest { get; set; } = string.Empty;

    public string SelfDescribeAreasOfInterest { get; set; } = string.Empty;

    /// <summary>
    /// Volunteer opportunities the user is interested in for this event.
    /// </summary>
    public string VolunteerOpportunities { get; set; } = string.Empty;

    // Ticketing
    public string? TicketType { get; set; }

    public string? TicketNumber { get; set; }
}

/// <summary>
/// Registration status enumeration for tracking the lifecycle of an event registration.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>Initial registration submitted</summary>
    Registered = 0,

    /// <summary>Registration confirmed (e.g., after payment or verification)</summary>
    Confirmed = 1,

    /// <summary>User cancelled their registration</summary>
    Cancelled = 2,

    /// <summary>Registration waitlisted due to capacity</summary>
    Waitlisted = 3,

    /// <summary>User checked in at the event</summary>
    CheckedIn = 4,

    /// <summary>User attended and completed the event</summary>
    Attended = 5,

    /// <summary>User did not attend (no-show)</summary>
    NoShow = 6
}
