namespace Visage.FrontEnd.Shared.Models;

public interface IUserRegistrationState
{
    bool IsNewUser { get; }
    bool HasCompletedRegistration { get; }
    IDictionary<string, bool> CompletedRegistrationSteps { get; }
}