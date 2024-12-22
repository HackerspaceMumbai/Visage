using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;

public interface IRegistrationService
{
    Task<bool> RegisterAsync(Registrant registrant);
}
