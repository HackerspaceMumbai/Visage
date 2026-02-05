using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// T087: Service to persist registration form drafts across page reloads (e.g. during OAuth redirects)
/// </summary>
public interface IRegistrationDraftService
{
    Task SaveDraftAsync(User user);
    Task<User?> GetDraftAsync();
    Task ClearDraftAsync();
}

public class NoOpRegistrationDraftService : IRegistrationDraftService
{
    public Task SaveDraftAsync(User user) => Task.CompletedTask;
    public Task<User?> GetDraftAsync() => Task.FromResult<User?>(null);
    public Task ClearDraftAsync() => Task.CompletedTask;
}
