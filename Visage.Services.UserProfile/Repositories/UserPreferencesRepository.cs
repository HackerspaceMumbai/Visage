using Visage.Shared.Models;
using Microsoft.EntityFrameworkCore;
using StrictId;

namespace Visage.Services.UserProfile.Repositories;

/// <summary>
/// T028: Repository for managing user preferences including AIDE banner dismissal.
/// Implements 30-day suppression logic for dismissed banners.
/// </summary>
public class UserPreferencesRepository
{
    private readonly UserDB _context;

    public UserPreferencesRepository(UserDB context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets user preferences for a specific user.
    /// Returns null if no preferences exist.
    /// </summary>
    public async Task<UserPreferences?> GetByUserIdAsync(Id<User> userId)
    {
        return await _context.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    /// <summary>
    /// Dismisses the AIDE completion banner for a user.
    /// Creates preferences record if it doesn't exist, or updates existing one.
    /// Banner will be suppressed for 30 days from the dismissal time.
    /// </summary>
    public async Task DismissAideBannerAsync(Id<User> userId)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preferences == null)
        {
            // Create new preferences record
            preferences = new UserPreferences
            {
                UserId = userId,
                AideBannerDismissedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.UserPreferences.Add(preferences);
        }
        else
        {
            // Update existing preferences
            preferences.AideBannerDismissedAt = DateTime.UtcNow;
            preferences.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if the AIDE banner should be shown for a user.
    /// Returns true if banner should be shown (not dismissed or dismissal expired).
    /// Returns false if banner is currently suppressed (dismissed within 30 days).
    /// </summary>
    public async Task<bool> ShouldShowAideBannerAsync(Id<User> userId)
    {
        var preferences = await GetByUserIdAsync(userId);

        if (preferences?.AideBannerDismissedAt == null)
        {
            // Never dismissed - show banner
            return true;
        }

        var daysSinceDismissal = (DateTime.UtcNow - preferences.AideBannerDismissedAt.Value).TotalDays;

        // Show banner if dismissal expired (>= 30 days)
        return daysSinceDismissal >= 30;
    }
}
