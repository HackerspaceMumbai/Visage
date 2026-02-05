using System.Threading;
using System.Threading.Tasks;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public interface IRegistrationService
{
    /// <summary>
    /// Creates or updates a user profile.
    /// </summary>
    Task<RegistrationResult> CreateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a user for a specific event.
    /// </summary>
    Task<RegistrationResult> RegisterForEventAsync(EventRegistration registration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy method for backward compatibility - creates user profile.
    /// </summary>
    [Obsolete("Use CreateUserAsync instead")]
    Task<RegistrationResult> RegisterAsync(User user, CancellationToken cancellationToken = default);
}
