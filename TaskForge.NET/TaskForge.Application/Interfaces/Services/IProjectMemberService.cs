using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectMemberService
    {
        Task<ProjectMember?> GetByIdAsync(int memberId);
        Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId);
        Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId);
        Task RemoveAsync(int memberId);
        Task<int> GetUserProjectCountAsync(int? userProfileId);
    }
}
