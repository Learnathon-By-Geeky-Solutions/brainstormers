using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;

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
            await _unitOfWork.UserProfiles.CreateAsync(userId, FullName);
        }

        public async Task<int?> GetByUserIdAsync(string userId)
        {
            var userProfile = await _unitOfWork.UserProfiles
                .FindAsync(up => up.UserId == userId);

            return userProfile.Select(up => up.Id).FirstOrDefault();
        }
    }
}
