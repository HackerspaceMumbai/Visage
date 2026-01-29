using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T003/T040: Draft registration data for partial save functionality.
/// Stores JSON serialized draft data with 30-day TTL per FR-008.
/// Updated to support section-specific drafts (mandatory vs AIDE).
/// </summary>
public class DraftRegistration
{
    [Required]
    public Id<DraftRegistration> Id { get; init; }

    [Required]
    public Id<User> UserId { get; set; }

    /// <summary>
    /// Section identifier: "mandatory" or "aide"
    /// Allows separate drafts for each registration section
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Section { get; set; } = "mandatory";

    /// <summary>
    /// JSON serialized draft data containing partial registration form fields.
    /// Encrypted at rest for sensitive information protection.
    /// </summary>
    [Required]
    public string DraftData { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when draft was last updated (UTC)
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Expiration timestamp for automatic cleanup (30 days after creation per FR-008).
    /// Used by database trigger or background job for draft cleanup.
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Optional hash of draft data for integrity verification
    /// </summary>
    [MaxLength(64)]
    public string? DataHash { get; set; }

    /// <summary>
    /// Whether the draft has been applied/converted to final registration
    /// Used to prevent accidental restoration of applied drafts
    /// </summary>
    public bool IsApplied { get; set; } = false;
}
