using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

public class EventRegistration
{
    /// <summary>
    /// Unique identifier for this registration
    /// </summary>
    [Required]
    public Id<EventRegistration> Id { get; init; }

    /// <summary>
    /// ID of the event this registration is for
    /// </summary>
    [Required]
    public Id<Event> EventId { get; set; }

    /// <summary>
    /// Local FK to the Users table (UserProfile DB).
    /// </summary>
    public Id<User> UserId { get; set; }

    /// <summary>
    /// Navigation for EF in UserProfile DB.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Auth0 subject claim of the registered user (cross-service reference used by Eventing).
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Auth0Subject { get; set; } = string.Empty;

    /// <summary>
    /// When the user registered for this event
    /// </summary>
    [Required]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current status of the registration
    /// </summary>
    [Required]
    [StringLength(50)]
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    /// <summary>
    /// Reason if registration was rejected
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// 4-digit PIN for quick check-in at door/kiosk (generated on approval)
    /// </summary>
    [StringLength(4, MinimumLength = 4)]
    public string? CheckInPin { get; set; }

    /// <summary>
    /// Event-specific registration questions/answers stored as JSON
    /// Example: {"tshirtSize": "L", "mealPreference": "vegetarian"}
    /// </summary>
    public string? AdditionalDetails { get; set; }

    /// <summary>
    /// Timestamp when registration was approved (for approved registrations)
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Who approved the registration (Auth0 subject of admin/organizer)
    /// </summary>
    public string? ApprovedBy { get; set; }
}

/// <summary>
/// Registration status lifecycle
/// </summary>
public enum RegistrationStatus
{
    /// <summary>
    /// Registration submitted, awaiting review
    /// </summary>
    Pending,

    /// <summary>
    /// Registration approved, user can attend
    /// </summary>
    Approved,

    /// <summary>
    /// Registration rejected (e.g., event full, doesn't meet criteria)
    /// </summary>
    Rejected,

    /// <summary>
    /// Event full, user is on waitlist
    /// </summary>
    Waitlisted,

    /// <summary>
    /// User cancelled their registration
    /// </summary>
    Cancelled
}
