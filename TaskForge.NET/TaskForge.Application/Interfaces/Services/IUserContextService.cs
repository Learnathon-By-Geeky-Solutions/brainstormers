namespace TaskForge.Application.Interfaces.Services
{
    public interface IUserContextService
    {
        Task<string> GetCurrentUserIdAsync();
    }
}
