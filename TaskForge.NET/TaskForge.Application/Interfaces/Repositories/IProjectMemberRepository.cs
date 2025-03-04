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
        Task<List<int>> GetProjectIdsByUserProfileIdAsync(int userProfileId);
        Task AddAsync(int projectId,int userProfileId);
        Task<bool> IsUserAssignedToProjectAsync(string userId, int projectId);
        Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId);
    }
}
