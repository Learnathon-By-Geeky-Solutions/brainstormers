using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories.common;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IProjectMemberRepository : IRepository<ProjectMember>
    {
        Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId);
        Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId);
    }
}
