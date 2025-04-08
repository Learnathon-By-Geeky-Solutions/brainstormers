using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectInvitationService
    {
        Task<ProjectInvitation?> GetByIdAsync(int invitationId);
        Task<PaginatedList<ProjectInvitation>> GetInvitationListAsync(int projectId, int pageIndex, int pageSize);
        Task<ServiceResult> AddAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole);
        Task UpdateInvitationStatusAsync(int id, InvitationStatus status);
        Task<PaginatedList<ProjectInvitation>> GetInvitationsForUserAsync(int? userProfileId, int pageIndex, int pageSize);
    }
}