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
    public class ProjectInvitationRepository : IProjectInvitationRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectInvitationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProjectInvitation invitation)
        {
            await _context.ProjectInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectInvitation?> GetByIdAsync(int invitationId)
        {
            return await _context.ProjectInvitations.FindAsync(invitationId);
        }

        public async Task<List<ProjectInvitation>> GetByUserProfileIdAsync(int userProfileId)
        {
            return await _context.ProjectInvitations
                .Where(i => i.InvitedUserProfileId == userProfileId)
                .ToListAsync();
        }

        public async Task UpdateAsync(ProjectInvitation invitation)
        {
            _context.ProjectInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }
    }

}


