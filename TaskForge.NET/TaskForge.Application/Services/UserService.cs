using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Services;

public class UserService : IUserService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserProfileRepository _userProfile;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
        IUserProfileRepository userProfile, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userProfile = userProfile;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<UserListItemDto>> GetFilteredUsersAsync(UserFilterDto filter, int pageIndex,
        int pageSize)
    {
        Expression<Func<UserProfile, bool>> predicate = user =>
            string.IsNullOrEmpty(filter.SearchTerm) ||
            user.FullName.Contains(filter.SearchTerm) ||
            user.User.Email!.Contains(filter.SearchTerm);

        var (filteredUserList, totalCount) = await _userProfile.GetPaginatedListAsync(
            predicate,
            query => query.OrderBy(u => u.FullName),
            query => query.Include(p => p.User),
            skip: (pageIndex - 1) * pageSize,
            take: pageSize
        );

        var userList = new List<UserListItemDto>();

        foreach (var user in filteredUserList)
        {
            var roles = await _userManager.GetRolesAsync(user.User);

            if (!string.IsNullOrEmpty(filter.RoleFilter) && !roles.Contains(filter.RoleFilter)) continue;

            userList.Add(new UserListItemDto
            {
                UserId = user.UserId,
                Email = user.User.Email ?? "",
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber,
                JobTitle = user.JobTitle,
                Company = user.Company,
                Role = string.Join(", ", roles)
            });
        }

        return new PaginatedList<UserListItemDto>(userList, totalCount, pageIndex, pageSize);
    }

    public async Task<UserListItemDto?> GetUserByIdAsync(string userId)
    {
        var userProfile = (await _userProfile.FindByExpressionAsync(
            u => u.UserId == userId,
            includes: query => query.Include(p => p.User)
        )).FirstOrDefault();

        if (userProfile == null) return null;

        var roles = await _userManager.GetRolesAsync(userProfile.User);
        return new UserListItemDto
        {
            UserId = userProfile.UserId,
            Email = userProfile.User.Email ?? "",
            FullName = userProfile.FullName,
            AvatarUrl = userProfile.AvatarUrl,
            PhoneNumber = userProfile.PhoneNumber,
            JobTitle = userProfile.JobTitle,
            Company = userProfile.Company,
            Role = string.Join(", ", roles)
        };
    }

    public async Task<IdentityResult> CreateUserAsync(UserCreateDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = true,
        };


		var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, dto.Role);

			var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
            };

            await _userProfile.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<List<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<bool> AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }
}