using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public class RegistrationService : IRegistrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(HttpClient httpClient, ILogger<RegistrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RegistrationResult> RegisterAsync(Registrant registrant, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting registration for user to {BaseAddress}/register", _httpClient.BaseAddress);

            using var response = await _httpClient.PostAsJsonAsync("/register", registrant, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var savedRegistrant = await response.Content.ReadFromJsonAsync<Registrant>(cancellationToken: cancellationToken);
                if (savedRegistrant is null)
                {
                    _logger.LogWarning("Registration API returned success but payload was empty");
                    return RegistrationResult.Failure(response.StatusCode, "Server returned an unexpected response.");
                }

                return RegistrationResult.Success(savedRegistrant, response.StatusCode);
            }

            var backendMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            var trimmedMessage = string.IsNullOrWhiteSpace(backendMessage) ? null : backendMessage.Trim();

            _logger.LogWarning("Registration API returned {StatusCode}. Message: {Message}", response.StatusCode, trimmedMessage);

            return RegistrationResult.Failure(response.StatusCode, trimmedMessage);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Registration request cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while posting registration.");
            return RegistrationResult.Failure(HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.");
        }
    }
}
