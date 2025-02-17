namespace Visage.FrontEnd.Shared.Services
{
    public interface IUserService
    {
        Task<bool> IsFirstTimeVisitorAsync();
    }
}
