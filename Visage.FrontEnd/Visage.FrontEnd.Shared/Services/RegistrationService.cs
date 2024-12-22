using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;

public class RegistrationService : IRegistrationService
{
    private readonly HttpClient _httpClient;

    public RegistrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RegisterAsync(Registrant registrant)
    {
        var response = await _httpClient.PostAsJsonAsync("/register", registrant);
        return response.IsSuccessStatusCode;
    }
}
