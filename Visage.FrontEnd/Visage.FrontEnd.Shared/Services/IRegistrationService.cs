using System.Threading;
using System.Threading.Tasks;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(Registrant registrant, CancellationToken cancellationToken = default);
}
