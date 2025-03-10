using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading.Tasks;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository Projects { get; }
        ITaskRepository Tasks { get; }
        IProjectMemberRepository ProjectMembers { get; }
        IProjectInvitationRepository ProjectInvitations { get; }
        IUserProfileRepository UserProfiles { get; }
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<int> SaveChangesAsync(); // Commit transactions
    }
}
