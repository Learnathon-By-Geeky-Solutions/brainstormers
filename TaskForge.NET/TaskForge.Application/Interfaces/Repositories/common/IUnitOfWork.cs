using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace TaskForge.Application.Interfaces.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository Projects { get; }
        ITaskRepository Tasks { get; }
        IProjectMemberRepository ProjectMembers { get; }
        IProjectInvitationRepository ProjectInvitations { get; }
        IUserProfileRepository UserProfiles { get; }
        ITaskAttachmentRepository TaskAttachments { get; }
        ITaskAssignmentRepository TaskAssignments { get; }
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<int> SaveChangesAsync();
    }
}
