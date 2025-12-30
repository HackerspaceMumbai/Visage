namespace Visage.Shared.Models;

/// <summary>
/// DTO for disconnecting a previously verified social profile.
/// </summary>
public sealed class SocialDisconnectDto
{
    /// <summary>
    /// Social provider name ("linkedin" or "github").
    /// </summary>
    [Required(ErrorMessage = "Provider is required")]
    [RegularExpression("^(linkedin|github)$", ErrorMessage = "Provider must be 'linkedin' or 'github'")]
    public string Provider { get; set; } = string.Empty;
}
