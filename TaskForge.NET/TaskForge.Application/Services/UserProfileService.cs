using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task CreateUserProfileAsync(string userId, string FullName)
        {
            var userProfile = new UserProfile
            {
                UserId = userId,
                FullName = FullName
            };
            await _unitOfWork.UserProfiles.AddAsync(userProfile);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int?> GetByUserIdAsync(string userId)
        {
            var userProfile = await _unitOfWork.UserProfiles
                .FindByExpressionAsync(up => up.UserId == userId);

            return userProfile.Select(up => up.Id).FirstOrDefault();
        }
    }
}
