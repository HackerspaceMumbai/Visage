using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetCurrentUserProfileAsync();
    Task UpdateCurrentUserProfileAsync(UserProfileDto profile);
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
}