using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(string userId, string FullName)
        {
            var userProfile = new UserProfile();
            userProfile.UserId = userId;
            userProfile.FullName = FullName;
            userProfile.CreatedBy = userId;
            _context.Add(userProfile);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserProfileIdByUserIdAsync(string userId)
        {
            return await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => up.Id)
                .FirstOrDefaultAsync();
        }

    }
}
