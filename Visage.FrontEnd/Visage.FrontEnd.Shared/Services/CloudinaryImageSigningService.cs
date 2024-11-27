using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Services;

namespace Visage.FrontEnd.Shared.Services
{
    public class CloudinaryImageSigningService : ICloudinaryImageSigningService
    {
        private readonly HttpClient _httpClient;

        public CloudinaryImageSigningService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CloudinaryUploadParams> SignUploadAsync()
        {
            var response = await _httpClient.GetAsync("sign-upload");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CloudinaryUploadParams>(jsonString);
            if (result == null)
            {
                throw new JsonException("Deserialization returned null");
            }
            return result;
        }
    }
}
