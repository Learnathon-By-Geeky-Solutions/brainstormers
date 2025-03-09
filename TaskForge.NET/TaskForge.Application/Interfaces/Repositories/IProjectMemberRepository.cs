using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IProjectMemberRepository
    {
        Task<ProjectMember?> GetByIdAsync(int memberId);
        Task<List<int>> GetProjectIdsByUserProfileIdAsync(int userProfileId);
        Task AddAsync(ProjectMember projectMember);
        Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId);
        Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId);
        Task RemoveAsync(ProjectMember projectMember);
    }
}
