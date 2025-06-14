using System.Net.Http.Json;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public class UserProfileService : IUserProfileService
{
    private readonly HttpClient _httpClient;

    public UserProfileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserProfileDto?> GetCurrentUserProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/profile");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserProfileDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateCurrentUserProfileAsync(UserProfileDto profile)
    {
        var response = await _httpClient.PutAsJsonAsync("api/profile", profile);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/profile/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserProfileDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}