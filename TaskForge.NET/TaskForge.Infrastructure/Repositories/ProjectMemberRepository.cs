using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.common.Repositories;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class ProjectMemberRepository : Repository<ProjectMember>, IProjectMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectMemberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProjectMember?> GetByIdAsync(int memberId)
        {
            return await _context.ProjectMembers.FindAsync(memberId);
        }

        public async Task AddAsync(ProjectMember projectMember)
        {
            await _context.AddAsync(projectMember);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetProjectIdsByUserProfileIdAsync(int userProfileId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserProfileId == userProfileId)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
        }

        public async Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserProfile.UserId == userId && pm.ProjectId == projectId)
                .Select(pm => new ProjectMemberDto
                {
                    Id = pm.Id,
                    Name = pm.UserProfile.FullName,
                    Email = pm.UserProfile.User.UserName,
                    Role = pm.Role // Assuming Role is an enum or string
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => new ProjectMemberDto
                {
                    Id = pm.Id,
                    Name = pm.UserProfile.FullName,
                    Email = pm.UserProfile.User.UserName,
                    Role = pm.Role
                })
                .ToListAsync();
        }

        public async Task RemoveAsync(ProjectMember projectMember)
        {
            _context.ProjectMembers.Remove(projectMember);
            await _context.SaveChangesAsync();
        }

    }
}
