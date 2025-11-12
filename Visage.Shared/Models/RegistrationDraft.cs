using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T040: Database entity for storing registration draft data.
/// Enables auto-save and restoration of partial registration progress.
/// Implements 30-day TTL per FR-008.
/// </summary>
public class RegistrationDraft
{
    /// <summary>
    /// Primary key (auto-increment)
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User identifier (StrictId format)
    /// Indexed for fast lookup
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string UserId { get; set; }

    /// <summary>
    /// Section identifier: "mandatory" or "aide"
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string Section { get; set; }

    /// <summary>
    /// Draft data as JSON blob
    /// Stores partial Registrant fields serialized from client form state
    /// </summary>
    [Required]
    public required string DraftData { get; set; }

    /// <summary>
    /// Timestamp when draft was created (UTC)
    /// </summary>
    [Required]
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when draft was last updated (UTC)
    /// </summary>
    [Required]
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when draft expires (UTC)
    /// Defaults to CreatedAt + 30 days
    /// </summary>
    [Required]
    public required DateTime ExpiresAt { get; set; }

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
