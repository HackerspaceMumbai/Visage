namespace Visage.Shared.Models;

/// <summary>
/// T087: DTO for retrieving pending social profiles from session
/// </summary>
public class PendingSocialProfilesDto
{
    public string? LinkedInProfile { get; set; }
    public string? LinkedInEmail { get; set; }

    public string? LinkedInSubject { get; set; }
    public string? LinkedInRawProfileJson { get; set; }
    public string? LinkedInRawEmailJson { get; set; }

    public string? GitHubProfile { get; set; }
    public string? GitHubEmail { get; set; }
}
