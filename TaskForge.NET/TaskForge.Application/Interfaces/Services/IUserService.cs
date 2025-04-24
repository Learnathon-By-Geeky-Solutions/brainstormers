using Microsoft.AspNetCore.Identity;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;

namespace TaskForge.Application.Interfaces.Services;

public interface IUserService
{
    Task<PaginatedList<UserListItemDto>> GetFilteredUsersAsync(UserFilterDto filter, int pageIndex, int pageSize);
    Task<bool> DeleteUserAsync(string userId);
    Task<List<IdentityRole>> GetAllRolesAsync();
    Task<bool> AssignRoleAsync(string userId, string role);
}