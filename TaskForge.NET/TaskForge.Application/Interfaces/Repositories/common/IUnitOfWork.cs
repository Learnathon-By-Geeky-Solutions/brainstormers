using System;
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

        Task<int> SaveChangesAsync(); // Commit transactions
    }
}
