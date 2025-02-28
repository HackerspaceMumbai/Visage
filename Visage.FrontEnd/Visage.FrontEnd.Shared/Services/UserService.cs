using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsFirstTimeVisitorAsync()
    {
        var response = await _httpClient.GetAsync("/auth0/first-time-visitor");
        if (response.IsSuccessStatusCode)
        {
            var isFirstTimeVisitor = await response.Content.ReadFromJsonAsync<bool>();
            return isFirstTimeVisitor;
        }
        return false;
    }
}
