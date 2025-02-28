using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectInvitationService
    {
        Task<bool> SendInvitationAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole);
    }
}