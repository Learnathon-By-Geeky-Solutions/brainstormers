using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;

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
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<int> SaveChangesAsync();
    }
}
