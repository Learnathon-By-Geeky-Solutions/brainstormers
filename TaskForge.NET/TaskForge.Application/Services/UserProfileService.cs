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
        private readonly IUserProfileRepository _userProfileRepository;
        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }
        public async Task CreateUserProfileAsync(string userId, string FullName)
        {
            await _userProfileRepository.CreateAsync(userId, FullName);
        }

        public async Task<int> GetByUserIdAsync(string userId)
        {
            return await _userProfileRepository.GetByUserIdAsync(userId);
        }
    }
}
