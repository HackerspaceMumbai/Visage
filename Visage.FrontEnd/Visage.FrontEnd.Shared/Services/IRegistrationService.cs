using System.Threading.Tasks;
using Visage.FrontEnd.Shared.Models;

public interface IRegistrationService
{
    Task<Registrant?> RegisterAsync(Registrant registrant);
}
