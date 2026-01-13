using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T004: User preferences for UI behavior (e.g., banner dismissal).
/// Stores user-specific preferences like AIDE banner dismissal timestamp.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// User ID serves as primary key (one preferences record per user).
    /// </summary>
    [Required]
    public Id<User> UserId { get; set; }

    /// <summary>
    /// Timestamp when the AIDE completion banner was dismissed.
    /// If not null and within 30 days, the banner will not be shown.
    /// </summary>
    public DateTime? AideBannerDismissedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
