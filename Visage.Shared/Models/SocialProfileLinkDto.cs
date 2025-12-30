namespace Visage.Shared.Models;

/// <summary>
/// T087: DTO for OAuth callback to link social profiles
/// </summary>
public class SocialProfileLinkDto
{
    /// <summary>
    /// Social provider name (e.g., "linkedin", "github")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// OAuth-verified profile URL (may be null/empty for providers that don't return a public URL)
    /// </summary>
    public string? ProfileUrl { get; set; }

    /// <summary>
    /// OAuth-verified provider subject/id (e.g., LinkedIn sub/id). Used when ProfileUrl is absent.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Optional: OAuth access token for future API calls (not stored in DB)
    /// </summary>
    public string? AccessToken { get; set; }
}

/// <summary>
/// T088: DTO for retrieving social connection status
/// </summary>
public class SocialConnectionStatusDto
{
    public SocialProviderStatusDto LinkedIn { get; set; } = new();
    public SocialProviderStatusDto GitHub { get; set; } = new();
}

public class SocialProviderStatusDto
{
    public bool IsConnected { get; set; }
    public string? ProfileUrl { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
