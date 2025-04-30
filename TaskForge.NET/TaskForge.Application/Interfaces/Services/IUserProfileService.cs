using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IUserProfileService
    {
        Task<int?> GetUserProfileIdByUserIdAsync(string userId);
        Task<UserProfile?> GetByUserIdAsync(string userId);
		Task CreateUserProfileAsync(string userId, string fullName);
		Task UpdateAsync(UserProfile profile);
	}
}
