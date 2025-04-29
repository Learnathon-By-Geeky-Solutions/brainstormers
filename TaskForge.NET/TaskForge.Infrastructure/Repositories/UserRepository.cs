using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        // This interface is intentionally left empty as a marker for repository types

        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<(IList<UserListItemDto> Items, int TotalCount)> GetFilteredUsersAsync(string? searchTerm, string? roleFilter, int? take = null, int? skip = null)
        {

            var query = from user in _context.Users
                        join userProfile in _context.UserProfiles on user.Id equals userProfile.UserId
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        select new { user, userProfile, roleName = role.Name };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.user.Email!.Contains(searchTerm) || x.user.UserName!.Contains(searchTerm) || x.userProfile.FullName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(x => x.roleName == roleFilter);
            }

            var totalUsers = await query.CountAsync();

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            var users = await query.OrderBy(i => i.roleName).ToListAsync();

            var userList = users.Select(user => new UserListItemDto
            {
                UserId = user.user.Id,
                Email = user.user.Email!,
                FullName = user.userProfile.FullName,
                AvatarUrl = user.userProfile.AvatarUrl,
                PhoneNumber = user.userProfile.PhoneNumber,
                JobTitle = user.userProfile.JobTitle,
                Company = user.userProfile.Company,
                Role = user.roleName
            }).ToList();

            return (userList, totalUsers);
        }
    }
}
