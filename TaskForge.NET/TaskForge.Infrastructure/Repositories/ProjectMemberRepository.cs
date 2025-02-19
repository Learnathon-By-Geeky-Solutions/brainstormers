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
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetProjectIdsByUserProfileIdAsync(int userProfileId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserProfileId == userProfileId)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
        }

    }
}
