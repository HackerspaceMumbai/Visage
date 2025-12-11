using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Visage.Shared.Models;

/// <summary>
/// T006: Draft registration data transfer object.
/// Stores partial registration form data with 24-hour expiration.
/// Matches OpenAPI schema in contracts/profile-completion-api.yaml
/// </summary>
public class DraftRegistrationDto
{
    /// <summary>
    /// Draft identifier (StrictId format, Guid-based)
    /// </summary>
    [Required]
    public required string Id { get; init; }

    /// <summary>
    /// User identifier who owns this draft
    /// </summary>
    [Required]
    public required string UserId { get; init; }

    /// <summary>
    /// JSON serialized draft data containing partial form fields.
    /// Structure matches Registrant entity properties.
    /// Example: {"FirstName": "John", "Email": "john@example.com", "City": "Mumbai"}
    /// </summary>
    [Required]
    public required JsonElement DraftData { get; init; }

    /// <summary>
    /// Timestamp when draft was created
    /// </summary>
    [Required]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Expiration timestamp (24 hours after CreatedAt)
    /// After this time, draft will be auto-deleted
    /// </summary>
    [Required]
    public required DateTime ExpiresAt { get; init; }
}
