namespace Visage.Shared.Models;

/// <summary>
/// DTO for disconnecting a previously verified social profile.
/// </summary>
public sealed class SocialDisconnectDto
{
    /// <summary>
    /// Social provider name ("linkedin" or "github").
    /// </summary>
    public string Provider { get; set; } = string.Empty;
}
