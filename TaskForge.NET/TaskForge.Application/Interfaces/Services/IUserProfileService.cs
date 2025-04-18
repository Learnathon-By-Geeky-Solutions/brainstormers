namespace TaskForge.Application.Interfaces.Services
{
    public interface IUserProfileService
    {
        Task<int?> GetByUserIdAsync(string userId);
        Task CreateUserProfileAsync(string userId, string fullName);
    }
}
