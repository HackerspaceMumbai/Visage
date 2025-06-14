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

    public async Task<Registrant?> RegisterAsync(Registrant registrant)
    {
        var response = await _httpClient.PostAsJsonAsync("/register", registrant);
        if (response.IsSuccessStatusCode)
        {
            // Deserialize the returned Registrant (with Id)
            return await response.Content.ReadFromJsonAsync<Registrant>();
        }
        return null;
    }
}
