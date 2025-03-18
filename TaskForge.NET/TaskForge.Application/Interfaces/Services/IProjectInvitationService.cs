using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectInvitationService
    {
        Task<ProjectInvitation?> GetByIdAsync(int invitationId);
        Task<List<ProjectInvitation>> GetInvitationListAsync(int projectId);
        Task<ServiceResult> AddAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole);
        Task UpdateInvitationStatusAsync(int id, InvitationStatus status);
        Task<List<ProjectInvitation>> GetInvitationsForUserAsync(int? userProfileId);
    }
}