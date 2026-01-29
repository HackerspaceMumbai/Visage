using StrictId;

namespace Visage.Shared.Models;

/// <summary>
/// Durable audit trail event for social profile verification actions.
/// </summary>
public sealed class SocialVerificationEvent
{
    public Id<SocialVerificationEvent> Id { get; init; }

    public Id<User> UserId { get; set; }

    /// <summary>
    /// Social provider name ("linkedin" or "github").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Action name (e.g., "link_attempt", "link_succeeded", "link_failed", "disconnect").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The verified profile URL, if available.
    /// </summary>
    public string? ProfileUrl { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Outcome (e.g., "succeeded" or "failed").
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Optional sanitized failure reason (avoid sensitive details).
    /// </summary>
    public string? FailureReason { get; set; }
}
