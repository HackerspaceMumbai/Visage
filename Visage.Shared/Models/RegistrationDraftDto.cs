using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T039: Registration draft save/restore DTO.
/// Captures partial registration progress to prevent data loss.
/// Supports both mandatory and AIDE profile fields.
/// </summary>
public class RegistrationDraftDto
{
    /// <summary>
    /// User identifier (StrictId format)
    /// </summary>
    [Required]
    public required string UserId { get; init; }

    /// <summary>
    /// Draft data as JSON blob containing partial Registrant fields
    /// Serialized from client-side form state
    /// </summary>
    [Required]
    public required string DraftData { get; init; }

    /// <summary>
    /// Which section the draft applies to: "mandatory" or "aide"
    /// </summary>
    [Required]
    [RegularExpression("^(mandatory|aide)$")]
    public required string Section { get; init; }

    /// <summary>
    /// Timestamp when draft was last saved (UTC)
    /// </summary>
    [Required]
    public required DateTime SavedAt { get; init; }

    /// <summary>
    /// Timestamp when draft expires (UTC)
    /// Defaults to 30 days from SavedAt per FR-008
    /// </summary>
    [Required]
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Client-provided hash of draft data for integrity verification
    /// Optional field for detecting corruption
    /// </summary>
    public string? DataHash { get; init; }
}
