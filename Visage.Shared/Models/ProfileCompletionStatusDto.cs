using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// T005: Profile completion status response DTO.
/// Returns current completion state for both mandatory and AIDE profiles.
/// Matches OpenAPI schema in contracts/profile-completion-api.yaml
/// </summary>
public class ProfileCompletionStatusDto
{
    /// <summary>
    /// User identifier (StrictId format)
    /// </summary>
    [Required]
    public required string UserId { get; init; }

    /// <summary>
    /// Whether mandatory registration fields are complete
    /// </summary>
    [Required]
    public required bool IsProfileComplete { get; init; }

    /// <summary>
    /// Whether optional AIDE profile fields are complete
    /// </summary>
    [Required]
    public required bool IsAideProfileComplete { get; init; }

    /// <summary>
    /// Timestamp when mandatory profile was completed (null if incomplete)
    /// </summary>
    public DateTime? ProfileCompletedAt { get; init; }

    /// <summary>
    /// Timestamp when AIDE profile was completed (null if incomplete)
    /// </summary>
    public DateTime? AideProfileCompletedAt { get; init; }

    /// <summary>
    /// List of incomplete mandatory field names (empty if profile complete)
    /// Used to show specific validation errors or progress indicators
    /// </summary>
    public List<string>? IncompleteMandatoryFields { get; init; }

    /// <summary>
    /// Timestamp when this status was checked (for cache invalidation)
    /// </summary>
    [Required]
    public required DateTime CheckedAt { get; init; }
}
