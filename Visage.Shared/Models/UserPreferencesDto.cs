using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T007: User preferences data transfer object.
/// Stores UI behavior preferences like banner dismissal.
/// Matches OpenAPI schema in contracts/profile-completion-api.yaml
/// </summary>
public class UserPreferencesDto
{
    /// <summary>
    /// User identifier (StrictId format)
    /// </summary>
    [Required]
    public required string UserId { get; init; }

    /// <summary>
    /// Timestamp when AIDE completion banner was dismissed.
    /// If not null and within 30 days, banner will not be shown.
    /// Null means banner has never been dismissed or suppression expired.
    /// </summary>
    public DateTime? AideBannerDismissedAt { get; init; }

    /// <summary>
    /// Timestamp when preferences were last updated
    /// </summary>
    [Required]
    public required DateTime UpdatedAt { get; init; }
}
