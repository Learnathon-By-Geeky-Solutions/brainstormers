using TaskForge.Application.DTOs;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<(IList<UserListItemDto> Items, int TotalCount)> GetFilteredUsersAsync(string? searchTerm, string? roleFilter, int? take = null, int? skip = null);

    }
}
