using Microsoft.EntityFrameworkCore;
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

	public async Task<UserProfile?> GetByUserIdAsync(string userId)
	{
		var userProfiles = await _userProfileRepository.FindByExpressionAsync(
			predicate: up => up.UserId == userId, 
			includes: query => query.Include(up => up.User)
			);

		return userProfiles.FirstOrDefault();
	}

	public async Task<int?> GetUserProfileIdByUserIdAsync(string userId)
	{
		var userProfiles = await _userProfileRepository
			.FindByExpressionAsync(up => up.UserId == userId);

		var userProfile = userProfiles.FirstOrDefault();

		return userProfile?.Id;
	}

	public async Task CreateUserProfileAsync(string userId, string fullName)
	{
        // Check if the user profile already exists
        var existingUserProfile = await _userProfileRepository
            .FindByExpressionAsync(up => up.UserId == userId);
        if (existingUserProfile.Any())
        {
            // User profile already exists, no need to create a new one
            // Optionally, you can log this or throw an exception
            Console.WriteLine("User profile already exists.");
            return;
        }

        var userProfile = new UserProfile
		{
			UserId = userId,
			FullName = fullName
		};
		await _userProfileRepository.AddAsync(userProfile);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task UpdateAsync(UserProfile profile)
	{
		await _userProfileRepository.UpdateAsync(profile);
		await _unitOfWork.SaveChangesAsync();
	}
}
