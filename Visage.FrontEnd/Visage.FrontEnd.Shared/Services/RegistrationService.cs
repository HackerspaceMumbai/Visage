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

    /// <inheritdoc />
    public async Task<RegistrationResult> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating/updating user profile at {BaseAddress}/api/users", _httpClient.BaseAddress);

            using var response = await _httpClient.PostAsJsonAsync("/api/users", user, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var savedUser = await response.Content.ReadFromJsonAsync<User>(cancellationToken: cancellationToken);
                if (savedUser is null)
                {
                    _logger.LogWarning("User API returned success but payload was empty");
                    return RegistrationResult.Failure(response.StatusCode, "Server returned an unexpected response.");
                }

                return RegistrationResult.Success(savedUser, response.StatusCode);
            }

            var backendMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            var trimmedMessage = string.IsNullOrWhiteSpace(backendMessage) ? null : backendMessage.Trim();

            _logger.LogWarning("User API returned {StatusCode}. Message: {Message}", response.StatusCode, trimmedMessage);

            return RegistrationResult.Failure(response.StatusCode, trimmedMessage);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("User creation request cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating user.");
            return RegistrationResult.Failure(HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.");
        }
    }

    /// <inheritdoc />
    public async Task<RegistrationResult> RegisterForEventAsync(EventRegistration registration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering user {UserId} for event {EventId}", registration.UserId, registration.EventId);

            using var response = await _httpClient.PostAsJsonAsync("/api/registrations", registration, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var savedRegistration = await response.Content.ReadFromJsonAsync<EventRegistration>(cancellationToken: cancellationToken);
                if (savedRegistration is null)
                {
                    _logger.LogWarning("Registration API returned success but payload was empty");
                    return RegistrationResult.Failure(response.StatusCode, "Server returned an unexpected response.");
                }

                return RegistrationResult.Success(savedRegistration, response.StatusCode);
            }

            var backendMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            var trimmedMessage = string.IsNullOrWhiteSpace(backendMessage) ? null : backendMessage.Trim();

            _logger.LogWarning("Registration API returned {StatusCode}. Message: {Message}", response.StatusCode, trimmedMessage);

            return RegistrationResult.Failure(response.StatusCode, trimmedMessage);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Event registration request cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while registering for event.");
            return RegistrationResult.Failure(HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.");
        }
    }

    /// <inheritdoc />
    [Obsolete("Use CreateUserAsync instead")]
    public async Task<RegistrationResult> RegisterAsync(User user, CancellationToken cancellationToken = default)
    {
        return await CreateUserAsync(user, cancellationToken);
    }
}
