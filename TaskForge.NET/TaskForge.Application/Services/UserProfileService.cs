using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;


    public UserProfileService(IUnitOfWork unitOfWork, IUserProfileRepository userProfileRepository)
    {
        _unitOfWork = unitOfWork;
        _userProfileRepository = userProfileRepository;
    }

    public async Task CreateUserProfileAsync(string userId, string fullName)
    {
        var userProfile = new UserProfile
        {
            UserId = userId,
            FullName = fullName
        };
        await _userProfileRepository.AddAsync(userProfile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int?> GetByUserIdAsync(string userId)
    {
        var userProfiles = await _userProfileRepository
            .FindByExpressionAsync(up => up.UserId == userId);

        var userProfile = userProfiles.FirstOrDefault();

        return userProfile?.Id;
    }
}