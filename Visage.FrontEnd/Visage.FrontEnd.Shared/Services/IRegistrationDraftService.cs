using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// T087: Service to persist registration form drafts across page reloads (e.g. during OAuth redirects)
/// </summary>
public interface IRegistrationDraftService
{
    Task SaveDraftAsync(Registrant registrant);
    Task<Registrant?> GetDraftAsync();
    Task ClearDraftAsync();
}

public class NoOpRegistrationDraftService : IRegistrationDraftService
{
    public Task SaveDraftAsync(Registrant registrant) => Task.CompletedTask;
    public Task<Registrant?> GetDraftAsync() => Task.FromResult<Registrant?>(null);
    public Task ClearDraftAsync() => Task.CompletedTask;
}
