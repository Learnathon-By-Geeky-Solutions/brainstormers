using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
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
        public async Task<List<ProjectInvitation>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectInvitations
                .Where(i => i.ProjectId == projectId)
                .Include(i => i.InvitedUserProfile).ThenInclude(i => i.User)
                .ToListAsync();
        }
        public async Task<List<ProjectInvitation>> GetByUserProfileIdAsync(int userProfileId)
        {
            return await _context.ProjectInvitations
                .Where(i => i.InvitedUserProfileId == userProfileId)
                .ToListAsync();
        }

        public async Task UpdateInvitationStatusAsync(int invitationId, InvitationStatus status)
        {
            var invitation = await GetByIdAsync(invitationId);
            if (invitation != null)
            {
                invitation.Status = status;
                await UpdateAsync(invitation);
            }
        }

        public async Task UpdateAsync(ProjectInvitation invitation)
        {
            _context.ProjectInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }
    }

}


