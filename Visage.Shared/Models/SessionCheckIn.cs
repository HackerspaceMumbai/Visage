using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

public sealed class SessionCheckIn
{
    [Required]
    public Id<SessionCheckIn> Id { get; init; }

    [Required]
    public Id<EventRegistration> EventRegistrationId { get; set; }

    [Required]
    [MaxLength(128)]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public DateTime CheckedInAt { get; set; }

    public DateTime? CheckedOutAt { get; set; }

    [MaxLength(32)]
    public string? CheckInMethod { get; set; }

    public int? StayDurationMinutes { get; set; }
}
