using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// Service abstraction for profile operations including completion status checks with client-side caching.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets the profile completion status for the current authenticated user.
    /// Results are cached for 5 minutes to reduce API calls.
    /// </summary>
    /// <returns>Profile completion status DTO with mandatory and AIDE completion flags</returns>
    Task<ProfileCompletionStatusDto?> GetCompletionStatusAsync();

    /// <summary>
    /// Invalidates the cached profile completion status, forcing next call to fetch from API.
    /// Should be called after profile updates (mandatory or AIDE completion).
    /// </summary>
    Task InvalidateCacheAsync();

    /// <summary>
    /// Gets the full profile data for the current authenticated user.
    /// </summary>
    /// <returns>Full Registrant profile or null if not found</returns>
    Task<Registrant?> GetProfileAsync();

    /// <summary>
    /// Updates the profile data for the current authenticated user.
    /// </summary>
    /// <param name="registrant">Updated registrant data</param>
    /// <returns>True if update succeeded, false otherwise</returns>
    Task<bool> UpdateProfileAsync(Registrant registrant);

    /// <summary>
    /// Saves a draft of the profile section (e.g., AIDE fields) for the current authenticated user.
    /// Drafts expire after 30 days and are auto-saved every 30 seconds during editing.
    /// </summary>
    /// <param name="section">Section name (e.g., "AIDE")</param>
    /// <param name="draftData">JSON serialized draft data</param>
    /// <returns>True if draft saved successfully, false otherwise</returns>
    Task<bool> SaveDraftAsync(string section, string draftData);

    /// <summary>
    /// Retrieves a saved draft for the specified section.
    /// </summary>
    /// <param name="section">Section name (e.g., "AIDE")</param>
    /// <returns>Draft data as JSON string, or null if no draft exists or draft is expired</returns>
    Task<string?> GetDraftAsync(string section);

    /// <summary>
    /// Deletes the draft for the specified section (e.g., after successful submission).
    /// </summary>
    /// <param name="section">Section name (e.g., "AIDE")</param>
    /// <returns>True if deletion succeeded or draft didn't exist, false on error</returns>
    Task<bool> DeleteDraftAsync(string section);
}
