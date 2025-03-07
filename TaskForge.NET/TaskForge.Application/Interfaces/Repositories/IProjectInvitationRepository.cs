using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IProjectInvitationRepository
    {
        Task AddAsync(ProjectInvitation invitation);
        Task<ProjectInvitation?> GetByIdAsync(int invitationId);
        Task<List<ProjectInvitation>> GetByProjectIdAsync(int projectId);
        Task<List<ProjectInvitation>> GetByUserProfileIdAsync(int userProfileId);
        Task UpdateInvitationStatusAsync(int invitationId, InvitationStatus status);
        Task UpdateAsync(ProjectInvitation invitation);
    }
}
